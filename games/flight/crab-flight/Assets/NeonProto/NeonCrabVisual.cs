using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Adapted from LivingCrabVisual. Sources drive this separate render tree only.
public sealed class NeonCrabVisual {
 struct Link {public Transform source,view;}
 readonly List<Link> links=new List<Link>();
 readonly List<Transform> legs=new List<Transform>(),claws=new List<Transform>(),extraLegs=new List<Transform>();
 readonly List<Renderer> hidden=new List<Renderer>();
 readonly Material core,red,tip;
 readonly Transform source,body;
 readonly Dictionary<Transform,Transform> map=new Dictionary<Transform,Transform>();
 readonly MeshRenderer glow;
 readonly Camera camera;
 readonly PortHeroVisual portHero;
 readonly float glowSize;
 readonly float[] legAngles,legSpeeds;
 float clock,flapPhase,launchAge=100,landAge=100;
 public readonly Transform Root;
 public int LegCount=>legs.Count;
 public int GlowCount=>glow?1:0;
 public int HiddenSourceCount=>hidden.Count;
 public float SpinCoreBlend{get;private set;}
 public Color CoreColor=>core.color;
 public void SampleSpinColor(float dt,string phase,float spin,bool reduced,bool effects,bool aimingOrCharging=false){
  float target=NeonProto.SpinGate(phase,spin,reduced,effects,aimingOrCharging)?1:0;
  SpinCoreBlend=Mathf.Lerp(SpinCoreBlend,target,1-Mathf.Exp(-10*Mathf.Clamp(dt,0,.08f)));
  if(Mathf.Abs(SpinCoreBlend-target)<.0001f)SpinCoreBlend=target;
  ApplyCoreColor();
  SamplePort(phase,aimingOrCharging);
 }
 public void SyncTankVisibility(string phase)=>portHero.SyncTankVisibility(phase);
 void SamplePort(string phase,bool aiming){
  glow.enabled=!portHero.Sample(phase,aiming);
#if UNITY_EDITOR
  if(reviewNoGlow)glow.enabled=false;
#endif
 }
 void ApplyCoreColor(){
  core.color=Color.Lerp(NeonProto.CrabCore,NeonProto.CrabRed,SpinCoreBlend);
#if UNITY_EDITOR
  if(reviewCoreRed)core.color=NeonProto.CrabRed;
#endif
 }
#if UNITY_EDITOR
 bool reviewCoreRed,reviewNoGlow;
 public void ReviewIsolation(bool coreRed,bool noGlow){reviewCoreRed=coreRed;reviewNoGlow=noGlow;ApplyCoreColor();glow.enabled=!reviewNoGlow;}
#endif
 public NeonCrabVisual(CrabModel crab,Transform owner,Camera viewCamera){
  source=crab.Root;camera=viewCamera;
  var shader=Resources.Load<Shader>("NeonProto/NeonUnlit");if(!shader)throw new InvalidOperationException("Neon unlit shader missing");
  core=new Material(shader){name="Neon dark core",color=NeonProto.CrabCore};red=new Material(shader){name="Neon red shell and limbs",color=NeonProto.CrabRed};tip=new Material(shader){name="Neon hot tips",color=NeonProto.CrabTip};
  Root=new GameObject("Neon crab render only").transform;Root.SetParent(owner,false);map[source]=Root;
  foreach(Transform child in source)Copy(child,Root);
  body=map[crab.Body];
  foreach(var part in crab.CollisionParts)if(map.TryGetValue(part.Part,out var view)){if(part.Kind==3&&!legs.Contains(view))legs.Add(view);else if(part.Kind==2&&!claws.Contains(view))claws.Add(view);}
  if(legs.Count==6)for(int side=0;side<2;side++){
   var template=legs[side==0?1:4];var extra=UnityEngine.Object.Instantiate(template,template.parent,false);extra.name="Neon extra visual leg "+side;
   Transform original=null;foreach(var pair in map)if(pair.Value==template){original=pair.Key;break;}
   links.Add(new Link{source=original,view=extra});extraLegs.Add(extra);legs.Add(extra);
  }
  if(legs.Count!=8)throw new InvalidOperationException("Expected eight neon render-only legs; found "+legs.Count);
  legAngles=new float[legs.Count];legSpeeds=new float[legs.Count];
  // HeroR2 disables the original mesh. Copy its original anatomy above, then hide
  // every currently visible source renderer, including the excluded HeroR branches.
  foreach(var r in source.GetComponentsInChildren<Renderer>(true))if(r.enabled){hidden.Add(r);r.enabled=false;}
  var bounds=new Bounds(crab.Body.position,Vector3.one);bool first=true;
  foreach(var r in body.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}
  glowSize=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z))*2;
  glow=NeonGlowTexture.Quad("Neon crab back glow",owner,NeonProto.CrabRed);
  portHero=new PortHeroVisual(Root,body,camera,owner);
  Sample(0,"ready",0,Vector3.zero,false,true);
 }
 void Copy(Transform src,Transform parent){
  if(src.name.StartsWith("HeroR",StringComparison.Ordinal)||src.GetComponent<Collider>())return;
  var dst=new GameObject(src.name).transform;dst.SetParent(parent,false);dst.gameObject.SetActive(src.gameObject.activeSelf);
  dst.localPosition=src.localPosition;dst.localRotation=src.localRotation;dst.localScale=src.localScale;map[src]=dst;links.Add(new Link{source=src,view=dst});
  var mr=src.GetComponent<MeshRenderer>();var mf=src.GetComponent<MeshFilter>();bool heroReplacement=false;
  foreach(Transform child in src)if(child.name.StartsWith("HeroR",StringComparison.Ordinal))heroReplacement=true;
  if(mr&&mf&&mf.sharedMesh&&(mr.enabled||heroReplacement)){
   dst.gameObject.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var r=dst.gameObject.AddComponent<MeshRenderer>();
   string n=src.name;var color=n.StartsWith("BodyCore")||n.EndsWith("surface 4")?core:n.EndsWith("surface 3")?tip:red;
   var mats=new Material[mf.sharedMesh.subMeshCount];for(int i=0;i<mats.Length;i++)mats[i]=color;r.sharedMaterials=mats;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
  }
  foreach(Transform child in src)Copy(child,dst);
 }
 public void Launch(){launchAge=0;landAge=100;}
 public void Land(){landAge=0;}
 public void Reset(){clock=flapPhase=0;launchAge=landAge=100;SpinCoreBlend=0;ApplyCoreColor();Array.Clear(legAngles,0,legAngles.Length);Array.Clear(legSpeeds,0,legSpeeds.Length);}
 public void Sample(float dt,string phase,float charge,Vector3 velocity,bool reduced,bool effects){
  if(!source)return;dt=Mathf.Clamp(dt,0,.08f);clock+=dt;launchAge+=dt;landAge+=dt;
  Root.SetPositionAndRotation(source.position,source.rotation);Root.localScale=source.lossyScale;
  foreach(var link in links)if(link.source){link.view.localPosition=link.source.localPosition;link.view.localRotation=link.source.localRotation;link.view.localScale=link.source.localScale;}
  for(int i=0;i<extraLegs.Count;i++){extraLegs[i].localPosition+=new Vector3(0,0,-.19f);extraLegs[i].localRotation*=Quaternion.Euler(0,i==0?-24:24,0);}
  float amount=effects?(reduced?.2f:1):0;
  float breath=Mathf.Sin(clock*2.1f+.38f*Mathf.Sin(clock*.73f))*.017f,crouch=phase=="ready"?Mathf.Clamp01(charge):0;
  float stretch=launchAge<.24f?Mathf.Sin(launchAge/.24f*Mathf.PI)*.30f:0,squash=landAge<.32f?Mathf.Sin(landAge/.32f*Mathf.PI)*.28f:0;
  float vertical=Mathf.Max(.55f,1+amount*(breath-crouch*.24f+stretch-squash));
  Root.localScale=Vector3.Scale(Root.localScale,new Vector3(1/Mathf.Sqrt(vertical),vertical,1/Mathf.Sqrt(vertical)));
  body.localPosition+=Vector3.down*(crouch*.16f+squash*.14f)*amount;
  float descent=Mathf.Clamp01(-velocity.y/28);flapPhase+=dt*Mathf.Lerp(8,22,descent);
  for(int i=0;i<legs.Count;i++){
   float offset=i*2.399963f,target=phase=="flight"?Mathf.Sin(flapPhase+offset)*(18+descent*15):Mathf.Sin(clock*1.3f+offset)*2;
   // Stable damped following gives each limb a delayed secondary motion.
   if(dt>0)legAngles[i]=Mathf.SmoothDamp(legAngles[i],target,ref legSpeeds[i],.065f+i*.009f,360,dt);
   legs[i].localRotation*=Quaternion.Euler((legAngles[i]+crouch*15)*amount,Mathf.Sin(flapPhase*.7f+offset)*amount*(phase=="flight"?8:1),0);
  }
  for(int i=0;i<claws.Count;i++)claws[i].localRotation*=Quaternion.Euler((-crouch*22+Mathf.Sin(clock*2.4f+i*2)*4)*amount,0,Mathf.Sin(clock*1.7f+i)*3*amount);
  var away=camera?camera.transform.forward:Vector3.forward;
  glow.transform.position=body.position+away*.65f;glow.transform.rotation=camera?camera.transform.rotation:Quaternion.identity;glow.transform.localScale=Vector3.one*glowSize;
  glow.sharedMaterial.color=new Color(1,1,1,.61f);
  SamplePort(phase,false);
 }
 public void Dispose(){portHero.Dispose();foreach(var r in hidden)if(r)r.enabled=true;if(Root)NeonProto.Release(Root.gameObject);if(glow){NeonProto.Release(glow.sharedMaterial);NeonProto.Release(glow.gameObject);}NeonProto.Release(core);NeonProto.Release(red);NeonProto.Release(tip);}
}

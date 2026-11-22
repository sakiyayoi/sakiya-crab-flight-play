using System;
using UnityEngine;
[Serializable]public sealed class NeonSparkAudit {public int emitted;public float radiusMin=100,radiusMax,radialSpeedMin=100,radialSpeedMax,tangentSpeedMin=100,tangentSpeedMax,lifeMin=100,lifeMax,scaleMax;public int pool=24;public float emissionMin=34,emissionMax=54;}

// Back glow and a fixed spark pool only. No arc, ring or line geometry is created.
public sealed class NeonSpinVortex {
 public const int SparkLimit=24;
 readonly Transform source;
 readonly Camera camera;
 readonly MeshRenderer glow;
 readonly MeshRenderer[] sparks=new MeshRenderer[SparkLimit];
 readonly Vector3[] positions=new Vector3[SparkLimit],velocities=new Vector3[SparkLimit];
 readonly float[] ages=new float[SparkLimit],lifetimes=new float[SparkLimit];
 public NeonSparkAudit Audit{get;private set;}=new NeonSparkAudit();
 float emission;
 int serial;
 public readonly Transform Root;
 public bool Active{get;private set;}
 public int Bands=>0;
 public bool GlowVisible=>glow.enabled&&glow.gameObject.activeInHierarchy;
 public int Sparks{get;private set;}
 public float Strength{get;private set;}
#if UNITY_EDITOR
 bool reviewNoGlow;
 public void ReviewIsolation(bool noGlow){reviewNoGlow=noGlow;glow.enabled=!reviewNoGlow;}
#endif
 public NeonSpinVortex(Transform owner,Transform crab,Camera view){
  source=crab;camera=view;
  Root=new GameObject("Neon spin vortex render only").transform;Root.SetParent(owner,false);
  glow=NeonGlowTexture.Quad("Neon vortex back glow",Root,Color.white);
  glow.transform.localPosition=new Vector3(0,0,.16f);
  for(int i=0;i<SparkLimit;i++){
   int palette=i%4==2?3:i%4==3?5:i%4;
   sparks[i]=NeonGlowTexture.Quad("Neon vortex spark "+i,Root,NeonProto.Palette[palette]);
  }
  Reset();
 }
 public void Reset(){Audit=new NeonSparkAudit();Active=false;Strength=0;Sparks=0;emission=0;serial=0;Root.gameObject.SetActive(false);for(int i=0;i<SparkLimit;i++){lifetimes[i]=0;sparks[i].enabled=false;}}
 public void Sample(float dt,string phase,float spinRps,float angle,bool reduced,bool effects,bool aimingOrCharging=false){
  bool portVisual=PortLayout.Enabled&&(phase=="ready"||phase=="charge"||aimingOrCharging||PortLayout.InPort(source.position.x));
  bool show=!portVisual&&NeonProto.SpinGate(phase,spinRps,reduced,effects,aimingOrCharging);
  if(!show){if(Active)Reset();return;}
  Active=true;Root.gameObject.SetActive(true);Strength=Mathf.Clamp01(Mathf.Abs(spinRps)/28);
  dt=Mathf.Clamp(dt,0,.08f);float sense=Mathf.Sign(spinRps);
  Root.SetPositionAndRotation(source.position+camera.transform.forward*.25f,camera.transform.rotation);
  Color back=Color.Lerp(NeonProto.Palette[0],NeonProto.Palette[1],Strength);back.a=.19f+.17f*Strength;
  glow.sharedMaterial.color=back;glow.transform.localScale=Vector3.one*Mathf.Lerp(4.5f,6.3f,Strength);
  // Discrete points only; no random-state dependence or per-frame allocation.
  for(int i=0;i<SparkLimit;i++)if(lifetimes[i]>0){
   ages[i]+=dt;if(ages[i]>=lifetimes[i]){lifetimes[i]=0;sparks[i].enabled=false;continue;}
   positions[i]+=velocities[i]*dt;
  }
  emission+=dt*Mathf.Lerp(34,54,Strength);
  for(int budget=0;emission>=1&&budget<SparkLimit;budget++,emission--){
   int slot=-1;for(int i=0;i<SparkLimit;i++)if(lifetimes[i]<=0){slot=i;break;}
   if(slot<0){emission=0;break;}
   float a=angle*Mathf.Deg2Rad+sense*serial*2.39996323f;
   var radial=camera.transform.right*Mathf.Cos(a)+camera.transform.up*Mathf.Sin(a);
   var tangent=(camera.transform.right*-Mathf.Sin(a)+camera.transform.up*Mathf.Cos(a))*sense;
   positions[slot]=Root.position+radial*(.55f+(serial%5)*.1375f);
   velocities[slot]=tangent*Mathf.Lerp(3.5f,6.5f,Strength)+radial*(.35f+(serial%5)*.1125f);
   ages[slot]=0;lifetimes[slot]=.22f+(serial%7)/6f*.14f;
   float radius=Vector3.Distance(positions[slot],Root.position),radialSpeed=Vector3.Dot(velocities[slot],radial),tangentSpeed=Vector3.Dot(velocities[slot],tangent);
   Audit.emitted++;Audit.radiusMin=Mathf.Min(Audit.radiusMin,radius);Audit.radiusMax=Mathf.Max(Audit.radiusMax,radius);Audit.radialSpeedMin=Mathf.Min(Audit.radialSpeedMin,radialSpeed);Audit.radialSpeedMax=Mathf.Max(Audit.radialSpeedMax,radialSpeed);Audit.tangentSpeedMin=Mathf.Min(Audit.tangentSpeedMin,tangentSpeed);Audit.tangentSpeedMax=Mathf.Max(Audit.tangentSpeedMax,tangentSpeed);Audit.lifeMin=Mathf.Min(Audit.lifeMin,lifetimes[slot]);Audit.lifeMax=Mathf.Max(Audit.lifeMax,lifetimes[slot]);serial++;
  }
  Sparks=0;
  for(int i=0;i<SparkLimit;i++)if(lifetimes[i]>0){
   Sparks++;float fade=1-ages[i]/lifetimes[i];sparks[i].enabled=true;
   sparks[i].transform.SetPositionAndRotation(positions[i],camera.transform.rotation);
   sparks[i].transform.localScale=Vector3.one*(.28f+.14f*Strength)*(.4f+.6f*fade);
   Audit.scaleMax=Mathf.Max(Audit.scaleMax,sparks[i].transform.localScale.x);
   sparks[i].sharedMaterial.color=new Color(1,1,1,fade*fade*.95f);
  }
 }
 public void Dispose(){NeonProto.Release(glow.sharedMaterial);foreach(var s in sparks)NeonProto.Release(s.sharedMaterial);NeonProto.Release(Root.gameObject);}
}

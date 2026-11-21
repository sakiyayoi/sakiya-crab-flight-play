using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace CrabLifeActors {
public enum ActorKind {HarborGull,CityPigeon,Chef,Shopper,OfficeWalker,Courier}
public enum BirdMode {Grounded,Flying}
public enum ReactionCue {NearbyStartle,Contact,Shockwave}
[Serializable]public struct ReactionReceipt{public int generation,eventId;public ActorKind kind;public ReactionCue cue;public float at,strength;}
// Host owns anchors, collisions, proximity, counts and clock. Only this child rig is animated.
public sealed class LifeActorRig:MonoBehaviour {
 public bool PhysicalFlight {get;set;}
 public const string Revision="life-actors-r1-r19-accef9b";
 [SerializeField] ActorKind kind;[SerializeField] Transform motion;[SerializeField] Transform[] bones;[SerializeField] Vector3[] rest;[SerializeField] Quaternion[] rotation;[SerializeField] int seed;
 public ActorKind Kind=>kind;public bool IsBird=>kind==ActorKind.HarborGull||kind==ActorKind.CityPigeon;[SerializeField] SkinnedMeshRenderer actorRenderer;public SkinnedMeshRenderer Renderer=>actorRenderer;
 public event Action<ReactionReceipt> ReactionStarted;
 int generation,lastEvent=-1;float reactionAt=-100,strength;ReactionCue cue;Vector3 push;BirdMode mode;bool departed;Mesh ownedMesh;LifeGeometryCacheR17 meshCacheR17;static Material shared;
 public static LifeActorRig Create(ActorKind kind,Transform parent,Vector3 localPosition,float yawDegrees=0,float scale=1,int seed=0,LifeGeometryCacheR17 geometryCache=null){
  var go=new GameObject("LifeActor "+kind);go.transform.SetParent(parent,false);go.transform.localPosition=localPosition;go.transform.localRotation=Quaternion.Euler(0,yawDegrees,0);go.transform.localScale=Vector3.one*Mathf.Clamp(scale,.1f,4);var actor=go.AddComponent<LifeActorRig>();actor.kind=kind;actor.seed=seed;actor.meshCacheR17=geometryCache;actor.Build();actor.ResetVisuals(0);return actor;
 }
 void Build(){
  motion=new GameObject("Motion offset - render only").transform;motion.SetParent(transform,false);var bb=new List<Transform>();
  Func<string,int,Vector3,int> bone=(name,parent,p)=>{var b=new GameObject(name).transform;b.SetParent(parent<0?motion:bb[parent],false);b.localPosition=p;bb.Add(b);return bb.Count-1;};
  if(IsBird){bone("Body",-1,new Vector3(0,.34f,0));bone("Head",0,new Vector3(0,.18f,-.21f));bone("WingL",0,new Vector3(-.10f,.04f,.03f));bone("WingR",0,new Vector3(.10f,.04f,.03f));bone("LegL",0,new Vector3(-.08f,-.15f,0));bone("LegR",0,new Vector3(.08f,-.15f,0));bone("Tail",0,new Vector3(0,-.015f,.25f));}
  else{bone("Pelvis",-1,new Vector3(0,.84f,0));bone("Torso",0,new Vector3(0,.19f,0));bone("Head",1,new Vector3(0,.48f,0));bone("ArmL",1,new Vector3(-.28f,.28f,0));bone("HandL",3,new Vector3(0,-.31f,0));bone("ArmR",1,new Vector3(.28f,.28f,0));bone("HandR",5,new Vector3(0,-.31f,0));bone("LegL",0,new Vector3(-.13f,-.08f,0));bone("FootL",7,new Vector3(0,-.38f,0));bone("LegR",0,new Vector3(.13f,-.08f,0));bone("FootR",9,new Vector3(0,-.38f,0));bone("Surprise mouth",2,new Vector3(0,.045f,-.15f));}
  bones=bb.ToArray();rest=new Vector3[bones.Length];rotation=new Quaternion[bones.Length];for(int i=0;i<bones.Length;i++){rest[i]=bones[i].localPosition;rotation[i]=bones[i].localRotation;}
  bool owns=true;var displayMesh=meshCacheR17!=null?meshCacheR17.Acquire(kind,transform,BuildMeshR17,out owns):BuildMeshR17();if(owns)ownedMesh=displayMesh;actorRenderer=gameObject.AddComponent<SkinnedMeshRenderer>();Renderer.sharedMesh=displayMesh;Renderer.bones=bones;Renderer.rootBone=motion;Renderer.localBounds=new Bounds(new Vector3(0,1.5f,0),new Vector3(7,8,7));Renderer.shadowCastingMode=ShadowCastingMode.Off;Renderer.receiveShadows=false;
  if(!shared){var shader=Shader.Find("CustomCrabBattle/ChitinR3");if(!shader)throw new InvalidOperationException("LifeActor shader missing");shared=new Material(shader){name="Life actor shared existing chitin shader"};shared.SetColor("_Color",Color.white);shared.SetFloat("_Roughness",.78f);}Renderer.sharedMaterial=shared;
 }
 Mesh BuildMeshR17(){var m=new LifeActorMesh(transform,bones);if(IsBird)Bird(m);else Human(m);return m.Finish();}
 static Color C(float r,float g,float b){return new Color(r,g,b);}
 void Bird(LifeActorMesh m){
  bool gull=kind==ActorKind.HarborGull;Color body=gull?C(.72f,.72f,.65f):C(.18f,.22f,.25f),wing=gull?C(.34f,.40f,.43f):C(.12f,.16f,.20f),dark=C(.025f,.035f,.04f),beak=gull?C(.72f,.43f,.07f):C(.08f,.07f,.065f),feet=C(.48f,.19f,.08f);
  m.Ell(0,Vector3.zero,gull?new Vector3(.17f,.19f,.34f):new Vector3(.19f,.22f,.27f),body);m.Ell(1,new Vector3(0,.02f,-.025f),gull?new Vector3(.13f,.13f,.15f):new Vector3(.12f,.14f,.13f),gull?body:C(.12f,.25f,.21f));
  m.Poly(1,new[]{new Vector3(-.07f,-.015f,-.12f),new Vector3(.07f,-.015f,-.12f),new Vector3(0,-.045f,gull?-.36f:-.23f),new Vector3(0,.045f,-.13f)},beak);
  for(int sign=-1;sign<=1;sign+=2){m.Ell(1,new Vector3(sign*.115f,.055f,-.085f),new Vector3(.022f,.026f,.024f),dark,8,5);int b=sign<0?2:3;float span=gull?.70f:.48f;m.Ell(b,new Vector3(sign*span*.43f,0,.10f),new Vector3(span*.5f,.035f,.14f),wing,10,5);m.Poly(b,new[]{Vector3.zero,new Vector3(sign*span,.01f,.17f),new Vector3(sign*span*.87f,-.012f,.25f),new Vector3(sign*span*.69f,-.012f,.21f),new Vector3(sign*span*.47f,-.006f,.24f),new Vector3(sign*.13f,0,.18f)},wing);m.Poly(b,new[]{new Vector3(sign*span*.69f,0,.13f),new Vector3(sign*span,.01f,.17f),new Vector3(sign*span*.87f,-.01f,.25f)},dark);
   int leg=sign<0?4:5;m.Ell(leg,new Vector3(0,-.09f,0),new Vector3(.025f,.11f,.025f),feet,8,5);m.Poly(leg,new[]{new Vector3(-.06f,-.18f,.015f),new Vector3(0,-.18f,-.15f),new Vector3(.06f,-.18f,.015f)},feet);
  }
  m.Poly(6,new[]{new Vector3(-.13f,0,0),new Vector3(.13f,0,0),new Vector3(gull?.12f:.18f,-.04f,.23f),new Vector3(gull?-.12f:-.18f,-.04f,.23f)},wing);
 }
 void Human(LifeActorMesh m){
  bool chef=kind==ActorKind.Chef,shop=kind==ActorKind.Shopper,office=kind==ActorKind.OfficeWalker;Color skin=C(.57f,.31f,.17f),dark=C(.035f,.055f,.065f),cloth=chef?C(.69f,.68f,.57f):shop?C(.24f,.075f,.16f):office?C(.055f,.12f,.19f):C(.045f,.30f,.28f),trousers=chef?dark:shop?C(.16f,.12f,.10f):C(.045f,.065f,.10f);
  m.Ell(0,Vector3.zero,new Vector3(shop?.28f:.22f,.18f,.16f),trousers);m.Ell(1,new Vector3(0,.12f,0),new Vector3(chef?.30f:shop?.33f:.25f,shop?.34f:.31f,chef?.21f:.17f),cloth);m.Ell(2,new Vector3(0,.13f,0),new Vector3(.16f,.185f,.145f),skin);
  m.Ell(11,Vector3.zero,new Vector3(.048f,.018f,.019f),dark,8,5);m.Ell(2,new Vector3(0,.11f,-.15f),new Vector3(.045f,.042f,.055f),skin,8,5);for(int s=-1;s<=1;s+=2)m.Ell(2,new Vector3(s*.065f,.18f,-.13f),new Vector3(.018f,.019f,.015f),dark,8,5);
  for(int side=0;side<2;side++){int arm=side==0?3:5,hand=arm+1,leg=side==0?7:9,foot=leg+1;m.Ell(arm,new Vector3(0,-.14f,0),new Vector3(.085f,.19f,.085f),cloth);m.Ell(hand,new Vector3(0,-.12f,-.01f),new Vector3(.068f,.16f,.07f),chef?cloth:skin);m.Ell(hand,new Vector3(0,-.27f,-.025f),new Vector3(.075f,.07f,.07f),skin,10,6);m.Ell(leg,new Vector3(0,-.17f,0),new Vector3(.10f,.22f,.105f),trousers);m.Ell(foot,new Vector3(0,-.15f,0),new Vector3(.08f,.19f,.085f),trousers);m.Ell(foot,new Vector3(0,-.34f,-.075f),new Vector3(.105f,.055f,.15f),dark,10,6);}
  if(chef){m.Box(1,new Vector3(0,.07f,-.19f),new Vector3(.37f,.61f,.035f),C(.79f,.77f,.64f));m.Ell(2,new Vector3(0,.38f,0),new Vector3(.19f,.18f,.16f),cloth);for(int k=-1;k<=1;k++)m.Ell(2,new Vector3(k*.105f,.50f,0),new Vector3(.105f,.13f,.14f),cloth,10,6);m.Ell(6,new Vector3(0,-.29f,-.12f),new Vector3(.24f,.035f,.21f),C(.06f,.08f,.085f),12,5);m.Box(6,new Vector3(0,-.27f,.03f),new Vector3(.06f,.06f,.26f),dark);}
  else if(shop){m.Ell(2,new Vector3(0,.25f,.035f),new Vector3(.185f,.15f,.16f),C(.12f,.055f,.028f));m.Box(1,new Vector3(0,.20f,-.19f),new Vector3(.12f,.31f,.045f),C(.65f,.38f,.09f));m.Box(4,new Vector3(0,-.43f,-.03f),new Vector3(.31f,.34f,.16f),C(.61f,.38f,.12f));m.Ell(4,new Vector3(0,-.28f,-.03f),new Vector3(.105f,.11f,.04f),C(.27f,.15f,.055f),10,6);}
  else if(office){m.Ell(2,new Vector3(0,.285f,.025f),new Vector3(.17f,.07f,.15f),dark);m.Box(1,new Vector3(0,.13f,-.18f),new Vector3(.045f,.34f,.025f),C(.55f,.12f,.055f));m.Box(6,new Vector3(0,-.40f,0),new Vector3(.40f,.25f,.10f),C(.075f,.045f,.03f));m.Box(1,new Vector3(0,-.12f,.035f),new Vector3(.43f,.20f,.28f),cloth);}
  else{m.Ell(2,new Vector3(0,.29f,0),new Vector3(.19f,.105f,.17f),C(.69f,.38f,.055f));m.Box(2,new Vector3(0,.25f,-.18f),new Vector3(.30f,.035f,.17f),C(.50f,.26f,.04f));m.Box(1,new Vector3(0,.10f,.24f),new Vector3(.49f,.54f,.25f),C(.62f,.21f,.055f));for(int s=-1;s<=1;s+=2)m.Box(1,new Vector3(s*.16f,.18f,-.17f),new Vector3(.055f,.41f,.035f),dark);}
 }
 public void SetBirdMode(BirdMode value){mode=value;departed=false;}
 public bool React(ReactionCue value,float at,Vector3 localDirection,float amount,int eventId,int eventGeneration=0){if(eventGeneration!=generation||eventId<=lastEvent)return false;lastEvent=eventId;cue=value;reactionAt=at;strength=Mathf.Clamp01(amount);push=Vector3.ProjectOnPlane(localDirection,Vector3.up);if(push.sqrMagnitude<.001f)push=Vector3.right;push.Normalize();if(IsBird)departed=true;ReactionStarted?.Invoke(new ReactionReceipt{generation=generation,eventId=eventId,kind=kind,cue=cue,at=at,strength=strength});return true;}
 public void ResetVisuals(int newGeneration){generation=newGeneration;lastEvent=-1;reactionAt=-100;departed=false;mode=BirdMode.Grounded;Sample(0,false);}
 void A(int b,float x,float y,float z){bones[b].localRotation=rotation[b]*Quaternion.Euler(x,y,z);}
 public void Sample(float age,bool reducedMotion,bool animate=true){
  if(bones==null||bones.Length==0)return;for(int i=0;i<bones.Length;i++){bones[i].localPosition=rest[i];bones[i].localRotation=rotation[i];}motion.localPosition=Vector3.zero;motion.localRotation=Quaternion.identity;if(!IsBird)bones[11].localScale=Vector3.one;if(!animate)return;float phase=age+(seed%17)*.37f,t=age-reactionAt,hit=t>=0&&t<1.8f?Mathf.Sin(Mathf.Clamp01(t/1.8f)*Mathf.PI):0;float calm=reducedMotion?.35f:1,amount=(.5f+strength*.5f)*calm;bool contact=cue==ReactionCue.Contact||cue==ReactionCue.Shockwave;
  if(IsBird){bool gull=kind==ActorKind.HarborGull;bool flying=mode==BirdMode.Flying||departed&&t>.18f;float flap=Mathf.Sin(phase*(gull?9:16))*(reducedMotion?16:50);A(1,0,Mathf.Sin(phase*1.6f)*12,0);if(flying){A(2,0,0,-flap);A(3,0,0,flap);A(4,-55,0,0);A(5,-55,0,0);motion.localPosition=Vector3.up*(.12f+Mathf.Sin(phase*3)*.04f*calm);}else{A(2,0,65,0);A(3,0,-65,0);A(4,Mathf.Sin(phase*7)*10,0,0);A(5,-Mathf.Sin(phase*7)*10,0,0);bones[0].localPosition+=Vector3.up*Mathf.Sin(phase*5)*.018f;}
   if(departed&&t>=0){float u=Mathf.Clamp01((t-.12f)/(gull?1.2f:.85f));motion.localPosition+=(push*Mathf.SmoothStep(0,1,u)*(gull?1.6f:1.05f)+Vector3.up*Mathf.SmoothStep(0,1,u)*(gull?2.3f:1.6f))*calm;A(0,-15*hit*calm,0,0);}if(PhysicalFlight)motion.localPosition=Vector3.zero;return;
  }
  float walk=Mathf.Sin(phase*3.2f)*(kind==ActorKind.OfficeWalker?7:3);A(7,walk,0,0);A(9,-walk,0,0);A(3,0,0,-7);A(5,0,0,7);A(2,0,Mathf.Sin(phase*1.2f)*4,0);
  if(hit<=0)return;bones[11].localScale=new Vector3(1+hit*.6f,1+hit*5,1);float r=hit*amount;float direction=push.x>=0?1:-1;
  switch(kind){
   case ActorKind.Chef:motion.localPosition=push*(contact?.48f:.18f)*r;A(0,contact?-12*r:18*r,0,-14*r*direction);A(3,-38*r,0,-22*r);A(4,-65*r,0,0);A(5,-70*r,0,32*r);A(6,-45*r,0,0);A(2,-10*r,0,12*r*direction);bones[2].localPosition+=Vector3.up*Mathf.Sin(Mathf.Clamp01(t/.7f)*Mathf.PI)*.08f*calm;A(7,-18*r,0,0);break;
   case ActorKind.Shopper:motion.localPosition=push*.62f*r;A(0,-10*r,0,22*r*direction);A(3,-25*r,0,-40*r);A(4,-35*r,0,25*r*Mathf.Sin(t*9));A(5,-50*r,0,48*r);A(7,-30*r,0,0);A(10,25*r,0,0);break;
   case ActorKind.OfficeWalker:motion.localPosition=push*.75f*r;A(0,0,contact?65*r:10*r,-16*r*direction);A(3,Mathf.Sin(t*13)*55*r,0,-65*r);A(5,-Mathf.Sin(t*13)*55*r,0,65*r);A(7,-25*r,0,-10*r);A(9,20*r,0,10*r);break;
   case ActorKind.Courier:motion.localPosition=push*(contact?1.0f:.40f)*r+Vector3.up*(contact&&!reducedMotion?.65f:0)*hit;A(0,contact?-42*r:20*r,0,contact&&!reducedMotion?32*r*direction:0);A(7,-58*r,0,0);A(9,-30*r,0,0);A(8,65*r,0,0);A(3,-55*r,0,-45*r);A(5,-55*r,0,45*r);break;
  }
  if(PhysicalFlight)motion.localPosition=Vector3.zero;
 }
 public void Dispose(){if(ownedMesh){if(Application.isPlaying)Destroy(ownedMesh);else DestroyImmediate(ownedMesh);ownedMesh=null;}if(Application.isPlaying)Destroy(gameObject);else DestroyImmediate(gameObject);}
 void OnDestroy(){if(ownedMesh){if(Application.isPlaying)Destroy(ownedMesh);else DestroyImmediate(ownedMesh);}}
}
}

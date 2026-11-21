using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Collections;

[Serializable] public sealed class RigidImpact {
 public float time,x,impulse,normalY,preVx,postVx,preVy,postVy,spinBefore,spinAfter;
 public string part,surface;public int kind;public Vector3 position,normal,velocity;
}
// One real Rigidbody, 13 primitive compound shapes / 11 authored anatomical IDs.
// ContactEvent is the same engine contact stream in native fixtures and WebGL.
public sealed partial class RigidCrabFlight : IDisposable {
 public const float StepSeconds=1f/240;
 public readonly Scene Scene;public readonly PhysicsScene Physics;
 readonly SimulationMode previousMode;readonly bool editorManual;
 public Rigidbody Body {get;private set;}
 public readonly List<RigidImpact> Impacts=new List<RigidImpact>();
 public readonly Dictionary<int,CrabPartProxy> Parts=new Dictionary<int,CrabPartProxy>();
 readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();
 readonly List<Collider> colliders=new List<Collider>();
 readonly Dictionary<int,string> surfaces=new Dictionary<int,string>();
 FlightSettings settings;Transform root;float restHeight,lockedComZ,stillClock,lastGround=-100,lastImpact=-100,lastAir=-100,braceTime=-100;
 bool running,pressed,braced,hasContact;int direction;Vector3 incoming;float incomingSpin;float bestImpulse,contactNormalY;Vector3 contactPoint,contactNormal;CrabPartProxy contactPart;string contactSurface;
 public float X,Y,Vx,Vy,Time,Peak,Distance,FirstContact=-1,FirstContactTime=-1,MinBottom=1000,MaxSpeed,MaxSpin;
 public int LastKind;
 public int Remaining=3,Used,Perfects,Goods,EarlyMisses,ImpactID,EventID,ContactSamples,SpeedCaps;
 public string LastGrade="",LastSurface="sand",LastEvent="launch",LastPart="",AbortReason="";
 public KickFlightPhysics.RunStage Stage {get;private set;}=KickFlightPhysics.RunStage.LAUNCH;
 public bool Finished=>Stage==KickFlightPhysics.RunStage.RESULT;public bool Braced=>braced;
 public bool UseLandingPushR27;public bool CanKick=>(!UseRotorBursts||UseLandingPushR27)&&running&&!Finished&&Remaining>0&&!braced&&Time-lastGround>.06f&&Time>.1f;
 public float Speed=>Body?Body.linearVelocity.magnitude:0;
 public float SpinRps=>Body?Body.angularVelocity.magnitude/(Mathf.PI*2):0;
 public float TimeToGround=>Vy<0?Mathf.Max(0,Y/Mathf.Max(-Vy,.01f)):10;
 public int ColliderCount=>colliders.Count;
 public RigidCrabFlight(){
  #if UNITY_EDITOR
  if(!Application.isPlaying){var previousActive=SceneManager.GetActiveScene();Scene=UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Additive);SceneManager.SetActiveScene(previousActive);previousMode=UnityEngine.Physics.simulationMode;UnityEngine.Physics.simulationMode=SimulationMode.Script;editorManual=true;}
  else
#endif
  {Scene=SceneManager.CreateScene("Crab isolated rigid scene "+Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics3D));}
  Physics=Scene.GetPhysicsScene();InitDebrisSceneCalmR4();
  var sand=Material("Sand",.56f,.42f,.06f);Floor("sand",new Vector3(0,-.25f,0),new Vector3(800,.5f,20),sand);
  var bounce=Material("Original float",.28f,.23f,.5f);for(int side=-1;side<=1;side+=2)Floor("float",new Vector3(side*90,.035f,0),new Vector3(20,.07f,13),bounce);
  UnityEngine.Physics.ContactEvent+=ReadContacts;
 }
 PhysicsMaterial Material(string name,float dynamicFriction,float staticFriction,float bounce){var m=new PhysicsMaterial(name){dynamicFriction=dynamicFriction,staticFriction=staticFriction,bounciness=bounce,frictionCombine=PhysicsMaterialCombine.Average,bounceCombine=PhysicsMaterialCombine.Maximum};owned.Add(m);return m;}
 void Floor(string type,Vector3 pos,Vector3 size,PhysicsMaterial mat){var o=new GameObject("Physical "+type);SceneManager.MoveGameObjectToScene(o,Scene);o.transform.position=pos;var c=o.AddComponent<BoxCollider>();c.size=size;c.sharedMaterial=mat;c.providesContacts=false;surfaces[c.GetInstanceID()]=type;MirrorDebrisStaticCalmR4(c);}
 public void Attach(CrabModel crab,FlightSettings config,float rest){
  AttachPhaseSplitR18.Begin(UnityEngine.Time.renderedFrameCount,Scene.handle,crab.Root.gameObject.GetInstanceID(),UseHeroColliderFitR21);bool attachSplitCompletedR18=false;try{
  running=false;Parts.Clear();colliders.Clear();root=crab.Root;settings=config.Copy();restHeight=rest;SceneManager.MoveGameObjectToScene(root.gameObject,Scene);PrimitiveColliderTraceR18.Mutated();
  Body=root.gameObject.AddComponent<Rigidbody>();Body.isKinematic=true;Body.useGravity=true;Body.mass=(float)settings.Mass*3.5f;Body.maxAngularVelocity=UseOverdrive?AngularSafetyR16:160;Body.maxDepenetrationVelocity=8;Body.solverIterations=32;Body.solverVelocityIterations=16;Body.interpolation=RigidbodyInterpolation.None;Body.constraints=RigidbodyConstraints.FreezePositionZ;Body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;
  var shell=Material("Shell rolls",.20f,.28f,.30f);var claw=Material("Claw knocks",.43f,.52f,.43f);var leg=Material("Leg grips",.58f+config.legs*.001f,.7f,.08f);
  if(UseHeroColliderFitR21){foreach(var part in crab.CollisionParts){int count=part.Kind==2?2:1;for(int half=0;half<count;half++){AttachPhaseSplitR18.Enter(2);var o=new GameObject("Contact "+part.Id+" hull "+half);o.transform.SetParent(part.Part,false);AttachPhaseSplitR18.Leave(2);AttachPhaseSplitR18.Enter(1);var mesh=HeroColliderFitR21.Mesh(part,crab,part.Kind==2?half:-1);AttachPhaseSplitR18.Leave(1);owned.Add(mesh);AttachPhaseSplitR18.Enter(2);var c=o.AddComponent<MeshCollider>();c.convex=true;c.sharedMesh=mesh;Register(c,part,part.Kind==1?shell:part.Kind==2?claw:leg);AttachPhaseSplitR18.Leave(2);}}}
  else {
  foreach(var part in crab.CollisionParts){
   var o=new GameObject("Contact "+part.Id);o.transform.SetParent(part.Part,false);var b=CrabPhysicsEnvelope.At(part,config);var size=b.size;Collider c;
   if(part.Kind==3){var cap=o.AddComponent<CapsuleCollider>();cap.center=b.center;int axis=size.x>size.z?0:2;cap.direction=axis;float transverse=axis==0?size.z:size.x;cap.radius=Mathf.Clamp(Mathf.Min(size.y,transverse)*.5f,.045f,.18f);cap.height=Mathf.Max(axis==0?size.x:size.z,cap.radius*2);c=cap;}
   else {var box=o.AddComponent<BoxCollider>();box.center=b.center;box.size=Vector3.Scale(size,part.Kind==1?new Vector3(.88f,.70f,.88f):new Vector3(.91f,.85f,.91f));c=box;}
   Register(c,part,part.Kind==1?shell:part.Kind==2?claw:leg);
   if(part.Kind==1){for(int sign=-1;sign<=1;sign+=2){var slice=new GameObject("Shell bevel "+sign);slice.transform.SetParent(part.Part,false);var box=slice.AddComponent<BoxCollider>();box.center=b.center+Vector3.up*(b.extents.y*.55f*sign);box.size=Vector3.Scale(size,new Vector3(.68f,.38f,.72f));Register(box,part,shell);}}
  }
  }
  if(Parts.Count!=13)throw new InvalidOperationException("Unexpected collider count");AttachPhaseSplitR18.Enter(3);Body.ResetCenterOfMass();Body.ResetInertiaTensor();Reset();AttachPhaseSplitR18.Leave(3);attachSplitCompletedR18=true;
 }finally{AttachPhaseSplitR18.End(Parts.Count,attachSplitCompletedR18);}
 }
 void Register(Collider c,CrabPartProxy part,PhysicsMaterial mat){c.sharedMaterial=mat;c.contactOffset=.008f;c.providesContacts=true;colliders.Add(c);Parts[c.GetInstanceID()]=part;}
 public float RotationRadius(){float radius=.5f;var center=root.position;foreach(var c in colliders){var b=c.bounds;for(int k=0;k<8;k++){var point=b.center+Vector3.Scale(b.extents,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1));radius=Mathf.Max(radius,Vector3.Distance(center,point));}}return radius;}
 public void Reset(){if(!Body)return;running=false;Body.isKinematic=true;if(restaurant){foreach(var c in colliders)c.enabled=false;}ResetRestaurant();if(restaurant){PrimitiveColliderTraceR18.BeforeMain(Scene,Time,"reset",true);Physics.Simulate(StepSeconds);foreach(var c in colliders)c.enabled=true;}Body.position=new Vector3(0,restHeight+(restaurant?.4f:0),0);Body.rotation=Quaternion.Euler(0,90,0);root.SetPositionAndRotation(Body.position,Body.rotation);braced=pressed=false;}
 public void Launch(FlightSettings config,int sign,float spin,Quaternion pose){
  settings=config.Copy();direction=sign<0?-1:1;Reset();ResetCeilingRun();FreezeLaunchCompound();Body.rotation=pose;root.rotation=pose;
  // Lift only enough to keep the actual rotated compound above the launch floor.
  UnityEngine.Physics.SyncTransforms();float bottom=Lowest();Body.position+=Vector3.up*((restaurant?.455f:.055f)-bottom);if(Mathf.Abs(spin)>8)Body.position=new Vector3(0,Mathf.Max(Body.position.y,(restaurant?.455f:.055f)+RotationRadius()),0);root.position=Body.position;UnityEngine.Physics.SyncTransforms();
  Body.ResetCenterOfMass();Body.ResetInertiaTensor();
  // Bounded arcade inertia: stored spin energy is at most 40% of forward launch energy.
  // High visual/body angular speed is real; it cannot create an unbounded ground catapult.
  float bodySpinLimit=UseOverdrive?ActiveBodySpinLimitR16:22;float bodySpin=UseOverdrive&&UseEnhancedSpinR16?Mathf.Sign(spin)*SpinCharge.SoftBodyRps(Mathf.Abs(spin),bodySpinLimit):Mathf.Clamp(spin,-bodySpinLimit,bodySpinLimit);float omega=(UseOverdrive&&UseEnhancedSpinR16?Mathf.Min(OverdriveBodySpinLimit,Mathf.Abs(spin)):Mathf.Abs(bodySpin))*Mathf.PI*2,launchSpeed=(float)settings.LaunchSpeed;float inertiaCap=.4f*Body.mass*launchSpeed*launchSpeed/Mathf.Max(omega*omega,1);Body.inertiaTensor=Vector3.Min(Body.inertiaTensor,Vector3.one*Mathf.Max(.01f,inertiaCap));
  Body.isKinematic=false;Body.linearDamping=.035f;Body.angularDamping=.12f;Body.linearVelocity=new Vector3(direction*(float)settings.LaunchSpeed*Mathf.Cos(settings.angle*Mathf.Deg2Rad),(float)settings.LaunchSpeed*Mathf.Sin(settings.angle*Mathf.Deg2Rad),0);
  lockedComZ=Body.worldCenterOfMass.z;Body.angularVelocity=new Vector3(0,.28f,-.96f).normalized*(bodySpin*Mathf.PI*2);Body.WakeUp();
  ResetFloorContactSteps();ResetFloorDiagnosticsR10();Time=Peak=X=Y=Vx=Vy=Distance=0;FirstContact=FirstContactTime=-1;MinBottom=1000;MaxSpeed=MaxSpin=stillClock=0;lastGround=lastImpact=lastAir=-100;Remaining=3;Used=Perfects=Goods=EarlyMisses=ImpactID=EventID=ContactSamples=SpeedCaps=0;Impacts.Clear();LastGrade=LastPart=AbortReason="";LastSurface="sand";LastEvent="launch";Stage=KickFlightPhysics.RunStage.AIRBORNE;running=true;ChargeRotor(spin);if(UseOverdrive&&UseEnhancedSpinR16)FundLaunchSpinR16(spin);
 }
 public bool SetKickPressed(bool value){if(UseRotorBursts&&!UseLandingPushR27){pressed=braced=false;return false;}if(!value){pressed=false;return false;}if(pressed)return false;pressed=true;if(!CanKick)return false;Remaining--;Used++;braced=true;braceTime=Time;Event("brace");return true;}
 void Event(string name){LastEvent=name;EventID++;}
 void ReadContacts(PhysicsScene scene,NativeArray<ContactPairHeader>.ReadOnly headers){if(!CalmSupportPerfR3.Active){ReadContactsRawCalmR3(scene,headers);return;}long stamp=CalmSupportPerfR3.Stamp();CalmSupportPerfR3.ContactHeaders+=headers.Length;for(int i=0;i<headers.Length;i++)CalmSupportPerfR3.ContactPairs+=headers[i].pairCount;ReadContactsRawCalmR3(scene,headers);CalmSupportPerfR3.ContactTicks+=CalmSupportPerfR3.Stamp()-stamp;}
 void ReadContactsRawCalmR3(PhysicsScene scene,NativeArray<ContactPairHeader>.ReadOnly headers){
  if(scene==Physics&&PrimitiveColliderTraceR18.EligibleMain>0){for(int h=0;h<headers.Length;h++)for(int j=0;j<headers[h].pairCount;j++){var pair=headers[h].GetContactPair(j);if(!pair.isCollisionExit)PrimitiveColliderTraceR18.ObserveMainPair(pair.colliderInstanceID,pair.otherColliderInstanceID,Time);}}
  if(!running||!Body)return;if(SeparateDebrisCalmR4&&scene==debrisPhysicsCalmR4){for(int i=0;i<headers.Length;i++)ReadCityDebrisContacts(headers[i]);return;}if(scene!=Physics)return;
  for(int i=0;i<headers.Length;i++){var h=headers[i];if(h.bodyInstanceID!=Body.GetInstanceID()&&h.otherBodyInstanceID!=Body.GetInstanceID()){ReadCityDebrisContacts(h);continue;}
   for(int j=0;j<h.pairCount;j++){var pair=h.GetContactPair(j);if(pair.isCollisionExit)continue;CrabPartProxy part;bool ownFirst=Parts.TryGetValue(pair.colliderInstanceID,out part);if(!ownFirst&&!Parts.TryGetValue(pair.otherColliderInstanceID,out part))continue;
    int other=ownFirst?pair.otherColliderInstanceID:pair.colliderInstanceID;string surf;if(!surfaces.TryGetValue(other,out surf))continue;
    for(int k=0;k<pair.contactCount;k++){var p=pair.GetContactPoint(k);if(p.separation>.035f&&p.impulse.sqrMagnitude<.00000001f)continue;hasContact=true;ContactSamples++;float impulse=p.impulse.magnitude;Vector3 outward=p.normal;if(Vector3.Dot(outward,Body.position-p.position)<0)outward=-outward;
#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
ObserveContactR24(part.Id,other,surf,p.position,outward,p.impulse,p.separation);
#endif
QueueBreak(other,impulse,p.position,outward);if(contactPart==null||impulse>bestImpulse){bestImpulse=impulse;contactPoint=p.position;contactNormal=outward;contactPart=part;contactSurface=surf;contactNormalY=p.normal.y;}}
   }
  }
 }
 // Existing settling clock: physics seconds while grounded and nearly still; not an abnormal-stop verdict.
 public float GroundStillSecondsR23=>stillClock;
 public void Step(){
  if(!running||Finished)return;incoming=Body.linearVelocity;incomingSpin=SpinRps;hasContact=false;contactPart=null;bestImpulse=0;
  Body.angularDamping=Time-lastGround<.05f?(UseOverdrive?1.2f:1.65f):.12f;Body.linearDamping=Time-lastGround<.05f?(UseOverdrive?.12f:.16f):.035f;
  PrepareCityDebris();SimulateFloorContact();StepDebrisCalmR4();Time+=StepSeconds;TickCityDecor(StepSeconds);long calmBreakStamp=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;ApplyBreaks();if(CalmSupportPerfR3.Active)CalmSupportPerfR3.BreakTicks+=CalmSupportPerfR3.Stamp()-calmBreakStamp;ObserveCeilingExit();CaptureFirstContactProbe();
  if(!Finite(Body.position)||!Finite(Body.linearVelocity)||!Finite(Body.angularVelocity)){Fault("non-finite");return;}
  float speed=Body.linearVelocity.magnitude,spin=Body.angularVelocity.magnitude;MaxSpeed=Mathf.Max(MaxSpeed,speed);MaxSpin=Mathf.Max(MaxSpin,spin/(Mathf.PI*2));
  float linearCap=UseOverdrive?100:90,angularCap=UseOverdrive?AngularSafetyR16:160;if(speed>linearCap){Body.linearVelocity=Vector3.ClampMagnitude(Body.linearVelocity,linearCap);SpeedCaps++;}if(spin>angularCap){Body.angularVelocity=Vector3.ClampMagnitude(Body.angularVelocity,angularCap);SpeedCaps++;}
  X=direction*Body.position.x;Y=Mathf.Max(0,Body.position.y-restHeight);Vx=direction*Body.linearVelocity.x;Vy=Body.linearVelocity.y;Peak=Mathf.Max(Peak,Y);Distance=Mathf.Max(0,X);float low=Lowest();MinBottom=Mathf.Min(MinBottom,low);
  RecordFloorDiagnosticsR10(low);if(UseRouteBlockoutR11&&Mathf.Abs(Body.position.x)>=3220){Stage=KickFlightPhysics.RunStage.RESULT;running=false;Body.Sleep();Event("settled");return;}if(low<-.45f){Fault("floor-tunnel");return;}if(Mathf.Abs(Body.position.x)>(UseRouteBlockoutR11?WorldRouteR11.WorldLimit:UseOverdrive?1450:375)||Mathf.Abs(Body.worldCenterOfMass.z-lockedComZ)>.1f||Body.position.y>(UseOverdrive?200:140)){Fault("world-bound");return;}
  if(hasContact){
   bool impact=(Time-lastGround>.045f&&Time-lastImpact>.08f&&(incoming.y<-.5f||incomingSpin>1.2f))||(contactPart.Kind!=LastKind&&bestImpulse/Body.mass>.8f&&Time-lastImpact>.12f);
   lastGround=Time;
   if(impact&&bestImpulse/Body.mass>.025f){lastImpact=Time;OverdriveGroundBounce();ImpactID++;LastPart=contactPart.Id;LastKind=contactPart.Kind;LastSurface=contactSurface;if(FirstContact<0){FirstContact=Distance;FirstContactTime=Time;}LastGrade="LAND";
    var entry=new RigidImpact{position=contactPoint,normal=contactNormal,velocity=incoming,time=Time,x=Body.position.x,part=LastPart,kind=contactPart.Kind,surface=LastSurface,impulse=bestImpulse,normalY=contactNormalY,preVx=incoming.x,postVx=Body.linearVelocity.x,preVy=incoming.y,postVy=Body.linearVelocity.y,spinBefore=incomingSpin,spinAfter=SpinRps};Impacts.Add(entry);
    if(braced&&Time-braceTime<=.25f){float age=Time-braceTime;bool perfect=age<=.1f;if(perfect)Perfects++;else Goods++;LastGrade=perfect?"PERFECT":"GOOD";float delta=perfect?2.2f:1.25f;Body.AddForceAtPosition((Vector3.up+Vector3.right*direction*.2f)*Body.mass*delta,contactPoint,ForceMode.Impulse);braced=false;Event(perfect?"perfect":"good");}else Event("land");
   }
  }else lastAir=Time;
  if(braced&&Time-braceTime>.25f){braced=false;EarlyMisses++;LastGrade="MISS";Event("miss");}
  Stage=braced?KickFlightPhysics.RunStage.BRACED:Time-lastGround<.05f?KickFlightPhysics.RunStage.TUMBLE:KickFlightPhysics.RunStage.AIRBORNE;
  if(Time-lastGround<.06f&&Body.linearVelocity.sqrMagnitude<.09f&&Body.angularVelocity.sqrMagnitude<.36f)stillClock+=StepSeconds;else stillClock=0;
  if(stillClock>.65f||Body.IsSleeping()){Stage=KickFlightPhysics.RunStage.RESULT;running=false;Body.Sleep();Event("settled");}
  if(Time>(UseRouteBlockoutR11?300:30))Fault("settle-timeout");RecordMotionR22();
#if FLOOR_DIAGNOSTICS && !UNITY_EDITOR
  RecordStepR24();
#endif
 }
#if UNITY_EDITOR
 public void ReviewDropPose(Quaternion rotation,Vector3 velocity,Vector3 angularVelocity){
  Body.rotation=rotation;root.rotation=rotation;Body.position=Vector3.zero;root.position=Vector3.zero;UnityEngine.Physics.SyncTransforms();var offset=Body.worldCenterOfMass;Body.position=new Vector3(0,8,0)-offset;root.position=Body.position;UnityEngine.Physics.SyncTransforms();lockedComZ=Body.worldCenterOfMass.z;Body.linearVelocity=velocity;Body.angularVelocity=angularVelocity;Body.WakeUp();
 }
#endif
 public bool FullyOutsideRestaurant(){if(!restaurant)return false;float minX=float.PositiveInfinity,maxX=float.NegativeInfinity,minY=float.PositiveInfinity;foreach(var c in colliders){minX=Mathf.Min(minX,c.bounds.min.x);maxX=Mathf.Max(maxX,c.bounds.max.x);minY=Mathf.Min(minY,c.bounds.min.y);}return minX>25.3f||maxX< -25.3f||minY>13.8f;}
 public float Lowest(){float y=float.PositiveInfinity;foreach(var c in colliders)y=Mathf.Min(y,c.bounds.min.y);return y;}
 void Fault(string reason){AbortReason=reason;RecordMotionR22();running=false;Stage=KickFlightPhysics.RunStage.RESULT;Body.Sleep();Event("fault");}
 static bool Finite(Vector3 v)=>!(float.IsNaN(v.x)||float.IsNaN(v.y)||float.IsNaN(v.z)||float.IsInfinity(v.x)||float.IsInfinity(v.y)||float.IsInfinity(v.z));
 public void Dispose(){DisposeDebrisSceneCalmR4();ClearFragmentVisibility();UnityEngine.Physics.ContactEvent-=ReadContacts;if(editorManual)UnityEngine.Physics.simulationMode=previousMode;foreach(var item in owned)Release(item);owned.Clear();if(Scene.IsValid()&&Scene.isLoaded){
#if UNITY_EDITOR
  if(!Application.isPlaying)UnityEditor.SceneManagement.EditorSceneManager.CloseScene(Scene,true);else SceneManager.UnloadSceneAsync(Scene);
#else
  SceneManager.UnloadSceneAsync(Scene);
#endif
 }}
 static void Release(UnityEngine.Object item){if(!item)return;
#if UNITY_EDITOR
  if(!Application.isPlaying)UnityEngine.Object.DestroyImmediate(item);else UnityEngine.Object.Destroy(item);
#else
  UnityEngine.Object.Destroy(item);
#endif
 }
}

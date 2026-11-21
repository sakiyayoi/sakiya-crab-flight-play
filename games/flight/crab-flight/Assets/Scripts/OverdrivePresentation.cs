using UnityEngine;
using UnityEngine.Rendering;

// Presentation observes the existing simulation receipts. It never detects a
// second collision, emits a gameplay event, changes a Rigidbody or adds energy.
public sealed partial class CrabFlightGame {
 OverdrivePresentation overdrivePresentation;
 RigidCrabFlight overdriveObserved;
 Transform overdriveCrab;
 int overdriveBurstsRead,overdriveBreaksRead,overdriveImpactsRead,overdriveImpactId,overdriveGeneration=-1,overdriveAttempt=-1;
 float overdriveSpinClock,overdriveSpeedClock,overdriveLastAccent=-100,overdriveRadius=1.3f,overdriveBoostPulse;
 int overdriveLastPriority;
 bool overdriveReduced,overdriveAlternate;

 public bool OverdrivePresentationAllocated=>overdrivePresentation;
 public int OverdrivePresentationActive=>overdrivePresentation?overdrivePresentation.ActiveCount:0;
 public int OverdrivePresentationVertices=>overdrivePresentation?overdrivePresentation.VertexCount:0;

 // Integration: after Initialize has made the crab/camera/effects. A false flag
 // returns before allocating any GameObject, Mesh, Material or presentation pool.
 void SetupOverdrivePresentation(){
  if(!useOverdrive||overdrivePresentation)return;
  // Reuse the shader reference already serialized on the existing VFX prefab;
  // no Shader.Find-only dependency that could be stripped from a WebGL build.
  Shader ink=effects?effects.inkShader:effectsPrefab?effectsPrefab.inkShader:null;
  if(!ink)return;
  var child=new GameObject("Overdrive local motion accents");child.transform.SetParent(transform,false);
  overdrivePresentation=child.AddComponent<OverdrivePresentation>();
  overdrivePresentation.Initialize(ink);overdriveReduced=ReducedMotion;
 }

 // Integration: Prepare/StreamReset and OnDestroy. The owned child releases its
 // Mesh/Material in OnDestroy; Prepare keeps the same fixed pool for the next run.
 // Also safe during a run (e.g. changing reduced motion): consume old receipts,
 // so clearing presentation never replays an earlier boost or impact.
 void ResetOverdrivePresentation(){
  overdriveObserved=simulation;overdriveCrab=null;
  overdriveBurstsRead=simulation!=null?simulation.BurstEvents.Count:0;
  overdriveBreaksRead=simulation!=null?simulation.BreakEvents.Count:0;
  overdriveImpactsRead=simulation!=null?simulation.Impacts.Count:0;
  overdriveImpactId=simulation!=null?simulation.ImpactID:0;
  overdriveGeneration=generation;overdriveAttempt=attempts;
  overdriveSpinClock=overdriveSpeedClock=overdriveBoostPulse=0;overdriveLastAccent=-100;overdriveLastPriority=0;overdriveAlternate=false;
  if(overdrivePresentation)overdrivePresentation.Clear();
 }

 // Integration: once after the rigid steps, crab pose and camera update in Tick.
 // dt is visual time: accents can finish through hit-stop, but pause freezes them.
 void TickOverdrivePresentation(float dt){
  if(FeelOnR27)return;if(!useOverdrive){if(overdrivePresentation){overdrivePresentation.Clear();overdrivePresentation.gameObject.SetActive(false);}return;}
  if(!overdrivePresentation)SetupOverdrivePresentation();
  if(!overdrivePresentation)return;
  if(!overdrivePresentation.gameObject.activeSelf)overdrivePresentation.gameObject.SetActive(true);
  if(overdriveReduced!=ReducedMotion){overdrivePresentation.Clear();overdriveReduced=ReducedMotion;overdriveSpinClock=overdriveSpeedClock=0;}
  if(paused)return;
  if(float.IsNaN(dt)||float.IsInfinity(dt)||dt<=0)return;
  dt=Mathf.Min(dt,.08f);overdrivePresentation.Step(dt);overdriveBoostPulse=Mathf.Max(0,overdriveBoostPulse-dt*4);
  if(crab==null||!crab.Root||!cam){overdrivePresentation.Draw();return;}
  // Readability follows the actual render target, including a pulled-back
  // camera. These are visual units only; body bounds and physics stay intact.
  overdrivePresentation.SetView(2*cam.orthographicSize/Mathf.Max(90,cam.pixelHeight));
  if(overdriveCrab!=crab.Root){
   overdriveCrab=crab.Root;
   overdriveRadius=world!=null?Mathf.Clamp(world.RotationRadius()*.58f,.8f,2):1.3f;
  }
  Vector3 focus=cam.WorldToViewportPoint(crab.Root.position);
  overdrivePresentation.SetFocus(new Vector2(focus.x,focus.y),overdriveRadius*.64f/(2*cam.orthographicSize),cam.aspect);
  // Contact geometry inside a broken shopfront used to disappear behind its
  // surviving facade. Move only the visual plane toward the camera; the shader
  // keeps the shell's centre clear, and the open marks leave the route readable.
  Vector3 behind=-cam.transform.forward*4;
  if(simulation!=null){
   // A new attempt/world resets local cursors, including direct Launch callers.
   if(overdriveObserved!=simulation||overdriveGeneration!=generation||overdriveAttempt!=attempts||
      overdriveBurstsRead>simulation.BurstEvents.Count||overdriveBreaksRead>simulation.BreakEvents.Count||overdriveImpactsRead>simulation.Impacts.Count){
    overdriveObserved=simulation;overdriveGeneration=generation;overdriveAttempt=attempts;
    overdriveBurstsRead=overdriveBreaksRead=overdriveImpactsRead=overdriveImpactId=0;
    overdriveLastAccent=-100;overdriveLastPriority=0;overdriveBoostPulse=0;overdrivePresentation.Clear();
   }
   ObserveOverdriveReceipts(behind);
  }
  if(!ReducedMotion&&(phase=="flight"||phase=="ready"&&spinCharge.Started)){
   float spin=Mathf.Abs(BodySpinForStateR16());
   overdriveSpinClock-=dt;
   // Do not keep stamping the same pose during hit-stop. Existing accents
   // finish in visual time while the recorded contact remains readable.
   if(spin>2.5f&&overdriveSpinClock<=0&&!(phase=="flight"&&useHitStop&&hitStop.Remaining>0)){
    overdriveSpinClock=Mathf.Lerp(.12f,.065f,Mathf.Clamp01(spin/40));
    Vector3 axis=simulation!=null&&phase=="flight"?simulation.Body.angularVelocity:new Vector3(0,.28f,-.96f);
    if(axis.sqrMagnitude<.001f)axis=new Vector3(0,.28f,-.96f);axis.Normalize();
    Vector3 u=Vector3.ProjectOnPlane(crab.Root.forward,axis);
    if(u.sqrMagnitude<.01f)u=Vector3.ProjectOnPlane(crab.Root.up,axis);
    u.Normalize();Vector3 v=Vector3.Cross(axis,u).normalized;
    overdrivePresentation.Spin(crab.Root.position+behind,u,v,overdriveRadius*1.38f,Mathf.Clamp01(spin/40),overdriveBoostPulse);
   }
   overdriveSpeedClock-=dt;
   if(phase=="flight"&&simulation!=null&&simulation.Speed>14&&overdriveSpeedClock<=0&&!(useHitStop&&hitStop.Remaining>0)){
    overdriveSpeedClock=.07f;overdriveAlternate=!overdriveAlternate;
    Vector3 velocity=simulation.Body.linearVelocity,forward=OverdriveForward(velocity);
    Vector3 side=Vector3.Cross(cam.transform.forward,forward).normalized;
    Vector3 origin=crab.Root.position+behind-forward*(overdriveRadius+.35f)+side*overdriveRadius*(overdriveAlternate?.64f:-.64f);
    overdrivePresentation.Streak(origin,forward,side,velocity*.12f,Mathf.Clamp(simulation.Speed*.035f,.45f,2.1f)*(1+.45f*overdriveBoostPulse));
   }
  }
  overdrivePresentation.Draw();
 }

 Vector3 OverdriveForward(Vector3 velocity){
  // Read the actual travel direction, including a ricochet. Side-view projection
  // avoids drawing a screen-length mark when the camera is pitched downwards.
  Vector3 forward=Vector3.ProjectOnPlane(velocity,cam.transform.forward);
  if(forward.sqrMagnitude<.01f)forward=Vector3.right*direction;
  return forward.normalized;
 }

 // Pure receipt reduction is separate from the camera/mesh adapter so one-shot
 // event consumption can be checked without starting the Unity engine.
 struct OverdriveCue {public float time,strength;public Vector3 point,velocity,normal;public string surface;public bool boost,broken;}
 void ObserveOverdriveReceipts(Vector3 behind){
  overdriveBurstsRead=Mathf.Max(overdriveBurstsRead,simulation.BurstEvents.Count-12);
  overdriveBreaksRead=Mathf.Max(overdriveBreaksRead,simulation.BreakEvents.Count-12);
  overdriveImpactsRead=Mathf.Max(overdriveImpactsRead,simulation.Impacts.Count-12);
  OverdriveCue cue;int budget=24;
  while(budget-->0&&TryReadOverdriveCue(simulation.Body.mass,out cue))
   EmitOverdriveAccent(cue.time,cue.point+behind,cue.velocity,cue.normal,cue.surface,cue.strength,cue.boost,cue.broken);
 }
 bool TryReadOverdriveCue(float bodyMass,out OverdriveCue cue){
  cue=new OverdriveCue();
  while(overdriveBurstsRead<simulation.BurstEvents.Count){
   var burst=simulation.BurstEvents[overdriveBurstsRead++];
   if(burst.objectId=="launch"||burst.energySpent<=0||burst.afterForward<=burst.beforeForward+.01f)continue;
   WorldBreakEvent broken=null;RigidImpact contact=null;
   for(int i=simulation.BreakEvents.Count-1;i>=0;i--){var b=simulation.BreakEvents[i];if(!b.secondary&&b.id==burst.objectId){broken=b;break;}}
   for(int i=simulation.Impacts.Count-1;i>=0;i--){var hit=simulation.Impacts[i];if(Mathf.Abs(hit.time-burst.time)<=.055f&&(hit.position-burst.position).sqrMagnitude<16){contact=hit;break;}}
   float sign=broken!=null&&Mathf.Abs(broken.velocity.x)>.1f?Mathf.Sign(broken.velocity.x):direction;
   Vector3 incoming=broken!=null?broken.velocity:Vector3.right*sign;
   cue=new OverdriveCue{time=burst.time,point=broken!=null?broken.position:burst.position,
    velocity=new Vector3(sign*burst.afterForward,contact!=null?contact.postVy:incoming.y,0),
    normal=broken!=null?broken.normal:contact!=null?contact.normal:-Vector3.right*sign,
    surface=broken!=null?broken.material:"rotor",strength=.55f+.45f*Mathf.Clamp01((burst.afterForward-burst.beforeForward)/12),boost=true,broken=broken!=null};
   return true;
  }
  // A committed break still deserves a contact cue when no acceleration was
  // bought. Consume its receipt once; never infer another break from geometry.
  while(overdriveBreaksRead<simulation.BreakEvents.Count){
   var broken=simulation.BreakEvents[overdriveBreaksRead++];bool boosted=false;
   for(int i=simulation.BurstEvents.Count-1;i>=0;i--){var b=simulation.BurstEvents[i];if(b.objectId==broken.id&&b.energySpent>0&&b.afterForward>b.beforeForward+.01f&&Mathf.Abs(b.time-broken.time)<=.055f){boosted=true;break;}}
   if(boosted||broken.secondary)continue;
   cue=new OverdriveCue{time=broken.time,point=broken.position,velocity=simulation.Body.linearVelocity,normal=broken.normal,surface=broken.material,strength=Mathf.Clamp01(broken.strength*.6f+broken.weight*.4f),broken=true};return true;
  }
  while(overdriveImpactsRead<simulation.Impacts.Count){
   var hit=simulation.Impacts[overdriveImpactsRead++];float perMass=hit.impulse/Mathf.Max(.01f,bodyMass);
   if(perMass<2.6f)continue;
   bool boosted=false;
   for(int i=simulation.BurstEvents.Count-1;i>=0;i--){var b=simulation.BurstEvents[i];if(b.objectId!="launch"&&b.energySpent>0&&b.afterForward>b.beforeForward+.01f&&Mathf.Abs(b.time-hit.time)<=.055f&&(b.position-hit.position).sqrMagnitude<16){boosted=true;break;}}
   if(boosted)continue;
   bool broke=false;
   for(int i=simulation.BreakEvents.Count-1;i>=0;i--){var b=simulation.BreakEvents[i];if(!b.secondary&&b.material==hit.surface&&Mathf.Abs(b.time-hit.time)<=.055f&&(b.position-hit.position).sqrMagnitude<16){broke=true;break;}}
   if(broke)continue;
   Vector3 outgoing=new Vector3(hit.postVx,hit.postVy,0);
   cue=new OverdriveCue{time=hit.time,point=hit.position,velocity=outgoing,
    normal=hit.normal,surface=hit.surface,strength=Mathf.Clamp01(perMass/12),boost=false};
   return true;
  }
  overdriveImpactId=simulation.ImpactID;return false;
 }

 void EmitOverdriveAccent(float time,Vector3 point,Vector3 velocity,Vector3 normal,string surface,float strength,bool boost,bool broken){
  int priority=boost?5:broken?(surface=="wall"||surface=="metal"?4:3):2;
  if(time-overdriveLastAccent<(ReducedMotion?.24f:.10f)&&priority<=overdriveLastPriority)return;
  Vector3 viewport=cam.WorldToViewportPoint(point);
  if(viewport.z<=0||viewport.x<-.15f||viewport.x>1.15f||viewport.y<-.15f||viewport.y>1.15f)return;
  overdriveLastAccent=time;overdriveLastPriority=priority;
  var forward=OverdriveForward(velocity);
  overdrivePresentation.Accent(point,forward,normal,cam.transform.right,cam.transform.up,overdriveRadius,strength,velocity.magnitude,surface,boost,ReducedMotion,broken);
  if(boost&&!ReducedMotion){overdriveBoostPulse=1;overdrivePresentation.Boost(crab.Root.position-cam.transform.forward*4,forward,Vector3.Cross(cam.transform.forward,forward).normalized,overdriveRadius,strength);}
 }
}

// One child, one unlit alpha material, one dynamic mesh. Nothing has a Collider,
// Rigidbody, Light, particle system, audio source, coroutine or autonomous Tick.
[DisallowMultipleComponent,AddComponentMenu("")]
public sealed class OverdrivePresentation : MonoBehaviour {
 public static bool NeonSpinStyle;
 public const int PoolLimit=40,VertexLimit=4096,IndexLimit=6144;
 enum Kind {Spin,Streak,Impact,Chip,Boost}
 struct Mark {
  public bool live,reduced,boost;public Kind kind;
  public Vector3 point,u,v,velocity,normal;public Color ink;
  public float age,life,size,width,growth,strength,gravity;public string surface;
 }
 Mark[] marks;Vector3[] vertices;Color[] colors;int[] indices;
 Mesh mesh;Material material;int vertexCount,indexCount;Matrix4x4 geometryWorldToLocal;float pixel=.012f;
 public int ActiveCount {get;private set;}
 public int VertexCount=>vertexCount;
 public int ContactCues{get;private set;}public int BreakCues{get;private set;}public int BoostCues{get;private set;}
 public void SetView(float unitsPerPixel){if(!float.IsNaN(unitsPerPixel)&&!float.IsInfinity(unitsPerPixel))pixel=Mathf.Clamp(unitsPerPixel,.002f,.25f);}
 public void SetFocus(Vector2 point,float radius,float aspect){if(material)material.SetVector("_AccentFocus",new Vector4(point.x,point.y,radius,aspect));}

 public void Initialize(Shader ink){
  if(mesh||!ink)return;
  marks=new Mark[PoolLimit];vertices=new Vector3[VertexLimit];colors=new Color[VertexLimit];indices=new int[IndexLimit];
  mesh=new Mesh{name="Overdrive bounded local accents"};mesh.MarkDynamic();
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
  material=new Material(ink){name="Overdrive instance ink"};
  var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
  renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
 }

 public void Clear(){
  if(marks!=null)for(int i=0;i<marks.Length;i++)marks[i].live=false;
  ActiveCount=vertexCount=indexCount=ContactCues=BreakCues=BoostCues=0;if(mesh)mesh.Clear(false);
 }

 public void Step(float dt){
  if(marks==null)return;
  for(int i=0;i<marks.Length;i++){
   if(!marks[i].live)continue;var m=marks[i];m.age+=dt;
   if(m.age>=m.life){marks[i].live=false;continue;}
   m.point+=m.velocity*dt;
   if(m.kind==Kind.Chip&&!m.reduced)m.velocity+=Vector3.down*(dt*m.gravity);
   marks[i]=m;
  }
 }

 int Slot(Kind kind){
  // Separate quotas protect the crab outline from event-dense chip showers.
  int limit=kind==Kind.Spin?3:kind==Kind.Streak?5:kind==Kind.Impact?4:kind==Kind.Boost?4:24;
  int free=-1,oldest=-1,count=0;float age=-1;
  for(int i=0;i<marks.Length;i++){
   if(!marks[i].live){if(free<0)free=i;continue;}
   if(marks[i].kind!=kind)continue;count++;
   if(marks[i].age>age){oldest=i;age=marks[i].age;}
  }
  if(count>=limit&&oldest>=0)return oldest;
  return free>=0?free:oldest;
 }

 void Put(Mark mark){int slot=Slot(mark.kind);if(slot<0)return;mark.live=true;marks[slot]=mark;}
 public void Spin(Vector3 point,Vector3 u,Vector3 v,float radius,float strength,float pulse=0){
  if(NeonSpinStyle)return;
  strength=Mathf.Clamp01(strength);
  Put(new Mark{kind=Kind.Spin,point=point,u=u,v=v,life=.15f,size=Mathf.Max(radius,pixel*13),width=Mathf.Max(.035f+.025f*strength,pixel*.85f),strength=strength,ink=new Color(1,.70f,.20f,.30f+.14f*strength+.24f*pulse)});
 }
 public void Streak(Vector3 point,Vector3 u,Vector3 v,Vector3 velocity,float length){
  Put(new Mark{kind=Kind.Streak,point=point,u=u,v=v,velocity=velocity,life=.18f,size=Mathf.Max(length,pixel*15),width=Mathf.Max(.055f,pixel*.85f),ink=new Color(.80f,1,.92f,.46f)});
 }
 public void Boost(Vector3 point,Vector3 forward,Vector3 side,float radius,float strength){
  BoostCues++;radius=Mathf.Max(radius,pixel*17);
  Put(new Mark{kind=Kind.Boost,point=point,u=forward,v=side,velocity=forward*(radius*1.4f),life=.36f,size=radius,width=Mathf.Max(.11f,pixel*2.6f),strength=strength,ink=new Color(1,.66f,.12f,.98f)});
 }
 public void Accent(Vector3 point,Vector3 forward,Vector3 normal,Vector3 u,Vector3 v,float radius,float strength,float outgoingSpeed,string surface,bool boost,bool reduced,bool broken=false){
  Vector3 planeNormal=Vector3.Cross(u,v).normalized;
  normal=Vector3.ProjectOnPlane(normal,planeNormal);
  normal=normal.sqrMagnitude>.01f?normal.normalized:-forward;
  ContactCues++;if(broken)BreakCues++;
  strength=Mathf.Clamp01(strength);radius=Mathf.Max(Mathf.Clamp(radius,.8f,1.55f),pixel*(reduced?12:26));
  // A warm, open contact fan + an outgoing lance distinguish impact from the
  // continuous mint/amber charge orbit. No filled disc, fog or screen overlay.
  Color ink=boost?new Color(1,.75f,.22f,.96f):Color.Lerp(SurfaceInk(surface),new Color(1,.90f,.64f,.92f),.40f);
  Put(new Mark{kind=Kind.Impact,point=point,u=forward,v=Vector3.Cross(planeNormal,forward).normalized,normal=normal,
   life=reduced?.18f:.34f,size=radius,width=Mathf.Max(reduced?.045f:.085f,pixel*(reduced?.65f:1.8f)),strength=strength,growth=reduced?0:Mathf.Clamp01(outgoingSpeed/20),ink=ink,reduced=reduced,boost=boost});
  int count=reduced?2:surface=="food"?3:surface=="glass"?8:6;
  Color chips=SurfaceInk(surface);chips.a=.9f;
  for(int i=0;i<count;i++){
   bool exiting=!reduced&&outgoingSpeed>1.6f&&i>=5;
   Vector3 axis=exiting?forward:normal,side=Vector3.Cross(planeNormal,axis).normalized;
   float spread=(exiting?(i-6)*14:(i-(Mathf.Min(count,5)-1)*.5f)*(reduced?32:29))*Mathf.Deg2Rad;
   Vector3 travel=axis*Mathf.Cos(spread)+side*Mathf.Sin(spread);
   float offset=radius*(.30f+(i%3)*.12f),speed=Mathf.Max(3.2f+strength*4.5f+(i%3)*.9f,pixel*65);
   Put(new Mark{kind=Kind.Chip,point=point+travel*offset,u=travel,v=Vector3.Cross(planeNormal,travel).normalized,
    velocity=reduced?Vector3.zero:travel*speed,life=reduced?.15f:.30f+(i%3)*.055f,size=Mathf.Max(reduced?.11f:.12f+(i%3)*.045f,pixel*(reduced?1.1f:1.8f)),ink=chips,reduced=reduced,surface=surface,gravity=surface=="metal"?9:surface=="wood"?4:3});
  }
 }
 static Color SurfaceInk(string surface){
  switch(surface){
   case "glass":return new Color(.73f,1,.94f,.9f);
   case "tableware":return new Color(1,.96f,.79f,.92f);
   case "metal":return new Color(.78f,.89f,1,.88f);
   case "wood":return new Color(1,.65f,.28f,.88f);
   case "wall":return new Color(1,.73f,.47f,.9f);
   case "food":return new Color(.73f,.89f,.35f,.85f);
   default:return new Color(1,.80f,.52f,.88f);
  }
 }

 public void Draw(){
  if(!mesh||marks==null)return;
  BuildGeometry(transform.worldToLocalMatrix);
  mesh.Clear(false);if(vertexCount==0)return;
  mesh.SetVertices(vertices,0,vertexCount);mesh.SetColors(colors,0,vertexCount);
  mesh.SetIndices(indices,0,indexCount,MeshTopology.Triangles,0,false);mesh.RecalculateBounds();
 }
 // The same real vertex/index construction is managed-checkable without Mesh,
 // Camera or Transform native calls. Draw supplies the actual object's matrix.
 void BuildGeometry(Matrix4x4 worldToLocal){
  geometryWorldToLocal=worldToLocal;vertexCount=indexCount=ActiveCount=0;
  for(int i=0;i<marks.Length;i++){
   var m=marks[i];if(!m.live)continue;ActiveCount++;
   float progress=m.age/m.life,fade=(1-progress)*(1-progress);
   Color ink=m.ink;ink.a*=fade;
   if(m.kind==Kind.Spin){
    // Time-sampled broken orbits retain the real angular plane and pose. The
    // empty centre and two broad gaps leave the crab and route visible.
    Color under=new Color(.10f,.17f,.18f,ink.a*.7f);
    Arc(m,m.size,m.width+.020f,10,78,7,under);Arc(m,m.size,m.width+.020f,190,78,7,under);
    Arc(m,m.size,m.width,10,78,7,ink);Arc(m,m.size,m.width,190,78,7,ink);
    Color cool=new Color(.73f,1,.91f,ink.a*.72f);
    Arc(m,m.size*.93f,m.width*.55f,102,36,4,cool);Arc(m,m.size*.93f,m.width*.55f,282,36,4,cool);
   }else if(m.kind==Kind.Impact){
    DrawImpact(m,progress,ink);
   }else if(m.kind==Kind.Boost){
    DrawBoost(m,progress,ink);
   }else if(m.kind==Kind.Streak){
    Vector3 tail=m.point-m.u*m.size;
    Quad(tail-m.v*m.width*.18f,tail+m.v*m.width*.18f,m.point+m.v*m.width,m.point-m.v*m.width,ink,0,0,1,1);
   }else {
    DrawChip(m,progress,ink);
   }
  }
 }
 void DrawBoost(Mark m,float progress,Color ink){
  // A travelling double chevron is reserved for a successful recorded boost.
  // It starts behind the actor and leaves the centre and outgoing route open.
  float stretch=1+progress*.65f;
  for(int i=0;i<2;i++){
   Vector3 tip=m.point-m.u*m.size*(1.15f+i*.78f)*stretch;
   for(int sign=-1;sign<=1;sign+=2){
    Vector3 tail=tip-m.u*m.size*.65f+m.v*(sign*m.size*(.78f+.16f*progress));
    Spear(tail,tip,m.v,m.width*(i==0?1:.7f),ink);
   }
  }
 }
 void DrawChip(Mark m,float progress,Color ink){
  float size=m.size*(m.reduced?1:1-progress*.35f);
  if(m.surface=="wood"){
   Quad(m.point-m.u*size*2.6f-m.v*size*.30f,m.point+m.u*size*2.2f-m.v*size*.18f,m.point+m.u*size*2.6f+m.v*size*.10f,m.point-m.u*size*1.9f+m.v*size*.35f,ink);
  }else if(m.surface=="metal"){
   Quad(m.point-m.u*size-m.v*size*.6f,m.point+m.u*size*1.2f-m.v*size*.4f,m.point+m.u*size*.7f+m.v*size*.6f,m.point-m.u*size*.8f+m.v*size*.4f,ink);
  }else if(m.surface=="food"){
   Quad(m.point-m.u*size,m.point-m.v*size*.7f,m.point+m.u*size,m.point+m.v*size*.7f,ink);
  }else {
   Triangle(m.point+m.u*size*(m.surface=="glass"?2.4f:1.4f),m.point-m.u*size-m.v*size*.65f,m.point-m.u*size+m.v*size*.65f,ink);
  }
 }
 void DrawImpact(Mark m,float progress,Color ink){
  Vector3 planeNormal=Vector3.Cross(m.u,m.v).normalized,side=Vector3.Cross(planeNormal,m.normal).normalized;
  if(m.reduced){
   float angle=Mathf.Atan2(Vector3.Dot(m.normal,m.v),Vector3.Dot(m.normal,m.u))*Mathf.Rad2Deg;
   Color under=new Color(.10f,.17f,.18f,ink.a*.8f);
   Arc(m,m.size*.62f,m.width*2,angle-65,130,6,under);
   Arc(m,m.size*.62f,m.width,angle-65,130,6,ink);return;
  }
  float expand=1-(1-progress)*(1-progress);
  float reach=m.size*(.58f+.78f*m.strength)*(.55f+.45f*expand);
  float contactAngle=Mathf.Atan2(Vector3.Dot(m.normal,m.v),Vector3.Dot(m.normal,m.u))*Mathf.Rad2Deg;
  Color rim=ink;rim.a*=.72f;
  Arc(m,m.size*(.47f+.20f*expand),m.width*.55f,contactAngle-72,144,8,rim);
  // Seven thin wedges spray out along the recorded collision normal. Their open
  // centre leaves the crab silhouette clear while the taper reads as a pop.
  for(int i=0;i<7;i++){
   float angle=(-81+i*27)*Mathf.Deg2Rad;
   Vector3 ray=m.normal*Mathf.Cos(angle)+side*Mathf.Sin(angle);
   float inner=m.size*(.16f+.10f*expand),outer=reach*(i%2==0?1:.76f);
   Vector3 a=m.point+ray*inner,b=m.point+ray*outer;
   float width=m.width*(.65f+.5f*m.strength)*(i%2==0?1:.72f);
   Spear(a,b,Vector3.Cross(planeNormal,ray).normalized,width,ink);
  }
  // The recorded outgoing velocity gives the escape direction (including a
  // bounce). Boost has a longer tip; its shape is not used by charge or travel.
  for(int i=-1;m.growth>.08f&&i<=1;i++){
   float sideOffset=i*m.size*.27f;
   float length=m.size*(1.10f+.65f*m.strength)*(m.boost?1.22f:.88f)*(i==0?1:.64f)*(.3f+.7f*m.growth);
   Vector3 a=m.point+m.u*m.size*.22f+m.v*sideOffset;
   Vector3 b=a+m.u*length*(.65f+.35f*expand);
   Spear(a,b,m.v,m.width*(i==0?1.25f:.62f),ink);
  }
 }
 void Spear(Vector3 a,Vector3 b,Vector3 side,float width,Color ink){
  Vector3 shoulder=Vector3.Lerp(a,b,.30f);
  Color under=new Color(.07f,.10f,.12f,ink.a*.9f);float edge=Mathf.Max(.028f,pixel*.75f);
  Quad(a,shoulder+side*(width+edge),b,shoulder-side*(width+edge),under,.2f,1,.25f,1);
  Quad(a,shoulder+side*width,b,shoulder-side*width,ink,0,1,.55f,1);
 }

 void Arc(Mark m,float radius,float width,float start,float sweep,int segments,Color ink){
  for(int i=0;i<segments;i++){
   float a=(start+sweep*i/segments)*Mathf.Deg2Rad,b=(start+sweep*(i+1)/segments)*Mathf.Deg2Rad;
   Vector3 first=m.u*Mathf.Cos(a)+m.v*Mathf.Sin(a),next=m.u*Mathf.Cos(b)+m.v*Mathf.Sin(b);
   float fade0=Mathf.Sin(Mathf.PI*(i+.35f)/(segments+.7f)),fade1=Mathf.Sin(Mathf.PI*(i+1.35f)/(segments+.7f));
   Quad(m.point+first*(radius-width),m.point+first*(radius+width),m.point+next*(radius+width),m.point+next*(radius-width),ink,fade0,fade0,fade1,fade1);
  }
 }
 void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d,Color ink,float aa=1,float ab=1,float ac=1,float ad=1){
  if(vertexCount+4>VertexLimit||indexCount+6>IndexLimit)return;
  int k=vertexCount;Vertex(a,ink,aa);Vertex(b,ink,ab);Vertex(c,ink,ac);Vertex(d,ink,ad);
  indices[indexCount++]=k;indices[indexCount++]=k+1;indices[indexCount++]=k+2;
  indices[indexCount++]=k;indices[indexCount++]=k+2;indices[indexCount++]=k+3;
 }
 void Triangle(Vector3 a,Vector3 b,Vector3 c,Color ink){
  if(vertexCount+3>VertexLimit||indexCount+3>IndexLimit)return;
  int k=vertexCount;Vertex(a,ink,1);Vertex(b,ink,.7f);Vertex(c,ink,.7f);
  indices[indexCount++]=k;indices[indexCount++]=k+1;indices[indexCount++]=k+2;
 }
 void Vertex(Vector3 point,Color ink,float alpha){vertices[vertexCount]=geometryWorldToLocal.MultiplyPoint3x4(point);ink.a*=Mathf.Clamp01(alpha);colors[vertexCount++]=ink;}
 void OnDisable(){Clear();}
 void OnDestroy(){if(mesh)Release(mesh);if(material)Release(material);}
 static void Release(Object item){
#if UNITY_EDITOR
  if(!Application.isPlaying){Object.DestroyImmediate(item);return;}
#endif
  Object.Destroy(item);
 }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed partial class RigidCrabFlight {
 sealed class ActionObject {
  public ObjectRules.Placement data;public int side;public GameObject visual;
  public ObjectVisual.Motion storyMotion;public BoxCollider collider;public Material material;public bool consumed;
 }
 public sealed class ObjectHit {public ObjectRules.Kind kind;public Vector3 position;public float time;}
 readonly List<ActionObject> actionObjects=new List<ActionObject>(32);
 readonly List<ActionObject> objectHits=new List<ActionObject>(32);
 readonly HashSet<BreakPanel> objectBreakPending=new HashSet<BreakPanel>();
 readonly HashSet<BreakPanel> objectBlastPending=new HashSet<BreakPanel>();
 public readonly List<ObjectHit> ObjectHits=new List<ObjectHit>();
 public readonly ObjectRules.Run ObjectRun=new ObjectRules.Run();
 public bool UseObjectRedesign {get;private set;}
 public bool ObjectReducedMotion;
 bool objectBandsBuilt;Bounds objectCrabBounds;
 readonly List<BreakPanel> objectBands=new List<BreakPanel>(60);
 public void ConfigureObjects(bool enabled){
  if(running)return;UseObjectRedesign=enabled;
  if(enabled&&!objectBandsBuilt){
   objectBandsBuilt=true;
   foreach(int side in new[]{-1,1}){
    foreach(float x in ObjectRules.BandX)foreach(float y in ObjectRules.BandY){
     AddPanel("object-band-"+side+"-"+x+"-"+y,"glass",new Vector3(side*x,y,0),new Vector3(.5f,2,2.4f),new Color(.55f,.8f,.85f),ObjectRules.BandThreshold);
     var p=panels[panels.Count-1];p.objectBand=true;objectBands.Add(p);
     // Frangible trigger: no rigid-wall solver slowdown. Momentum loss is
     // applied once per contact batch via an impulse, before the simulation.
     foreach(var shape in p.shapes){shape.isTrigger=true;if(debrisStaticsCalmR4.TryGetValue(shape,out var proxy))proxy.isTrigger=true;}
    }
    foreach(var data in ObjectRules.Placements){
     var visual=ObjectVisual.Build(data.kind,out var material,out var storyMotion);owned.Add(material);
     visual.name="Action "+side+" "+data.id;visual.transform.localScale=new Vector3(side,1,1);SceneManager.MoveGameObjectToScene(visual,Scene);
     var collider=visual.AddComponent<BoxCollider>();collider.size=Vector3.one*(ObjectRules.HalfSize*2);collider.isTrigger=true;
     actionObjects.Add(new ActionObject{data=data,side=side,visual=visual,storyMotion=storyMotion,collider=collider,material=material});
    }
   }
  }
  ResetObjects();
 }
 void ResetObjects(){
  ObjectRun.Reset();ObjectHits.Clear();objectBreakPending.Clear();objectBlastPending.Clear();
  foreach(var p in panels){
   bool active=p.objectBand?UseObjectRedesign:ObjectRules.KeepLegacyPanel(UseObjectRedesign?"redesign":"legacy",p.position.x);
   p.objectSuppressed=!active;p.visual.SetActive(active&&!p.broken);SetPanelContactsCalmR4(p,active&&!p.broken);
  }
  foreach(var a in actionObjects){a.consumed=false;a.visual.SetActive(UseObjectRedesign);a.collider.enabled=UseObjectRedesign;}
  MoveObjects(0);
 }
 void MoveObjects(float time){
  if(!UseObjectRedesign)return;
  foreach(var a in actionObjects){if(a.consumed)continue;var d=ObjectRules.Get(a.data.kind);
   float wave=Mathf.Sin(time*Mathf.PI*2*d.hertz),dx=0,dy=0;
   if(d.motion=="slide"||d.motion=="fast-slide")dx=wave*d.amplitude*a.side;
   else if(d.motion!="rotate")dy=wave*d.amplitude;
   a.visual.transform.SetPositionAndRotation(new Vector3(a.side*a.data.x+dx,a.data.y+dy,a.data.z),
    Quaternion.Euler(0,0,d.motion=="rotate"?a.side*time*d.hertz*360:0));
   a.storyMotion.Pose(time,ObjectReducedMotion);
   if(a.data.kind==ObjectRules.Kind.Blast){
    // 2Hz maximum; reduced motion keeps steady emission, without changing hits.
    ColorUtility.TryParseHtmlString(d.color,out var color);
    a.material.SetColor("_EmissionColor",color*(ObjectReducedMotion?ObjectRules.Emission:ObjectRules.Emission*(.7f+.3f*(wave+1)*.5f)));
   }
  }
 }
 bool TouchesObject(BoxCollider target,RaycastHit[] sweep){
  if(!target.enabled||!target.gameObject.activeInHierarchy||!target.bounds.Intersects(objectCrabBounds))return false;
  foreach(var hit in sweep)if(hit.collider==target)return true;
  foreach(var c in colliders)if(c.enabled&&UnityEngine.Physics.ComputePenetration(c,c.transform.position,c.transform.rotation,target,target.transform.position,target.transform.rotation,out var axis,out var depth))return true;
  return false;
 }
 // Run before ANY new action force, and again after the solver before breaks.
 // Trigger colliders never impose a bounce/stop through the contact solver.
 bool DetectObjects(bool predict){
  if(!UseObjectRedesign)return false;
  UnityEngine.Physics.SyncTransforms();
  Vector3 v=Body.linearVelocity;
  objectCrabBounds=colliders[0].bounds;foreach(var c in colliders)objectCrabBounds.Encapsulate(c.bounds);
  if(predict){var end=objectCrabBounds;end.center+=v*StepSeconds;objectCrabBounds.Encapsulate(end);}
  var sweep=predict&&v.sqrMagnitude>.0001f?Body.SweepTestAll(v.normalized,v.magnitude*StepSeconds,QueryTriggerInteraction.Collide):Array.Empty<RaycastHit>();
  objectHits.Clear();
  foreach(var a in actionObjects)if(!a.consumed&&TouchesObject(a.collider,sweep))objectHits.Add(a);
  // Two passes ensure list/physics callback order cannot let boost beat death.
  foreach(var a in objectHits)if(a.data.kind==ObjectRules.Kind.Death){
   ConsumeObject(a);ObjectRun.Kill();pendingBreaks.Clear();breakNormals.Clear();breakImpulses.Clear();
   primaryPending.Clear();secondarySources.Clear();objectBreakPending.Clear();objectBlastPending.Clear();
   X=direction*Body.position.x;Y=Mathf.Max(0,Body.position.y-restHeight);Distance=Mathf.Max(0,X);Peak=Mathf.Max(Peak,Y);
   running=false;Stage=KickFlightPhysics.RunStage.RESULT;Body.Sleep();Event("death");return true;
  }
  foreach(var a in objectHits){ConsumeObject(a);var d=ObjectRules.Get(a.data.kind);
   switch(a.data.kind){
    case ObjectRules.Kind.Boost:
     var forward=v.sqrMagnitude>.01f?v.normalized:Vector3.right*direction;
     Body.AddForce(forward*(Body.mass*d.amount),ForceMode.Impulse);break;
    case ObjectRules.Kind.Bounce:
     Body.AddForce(Vector3.up*(Body.mass*ObjectRules.BounceDelta(v.y,ObjectRun.Spinning(Time))),ForceMode.Impulse);break;
    case ObjectRules.Kind.Spin:ObjectRun.Spin(Time);break;
    case ObjectRules.Kind.Brake:Body.AddForce(-v*(Body.mass*(1-d.amount)),ForceMode.Impulse);break;
    case ObjectRules.Kind.Blast:
     var outward=Body.worldCenterOfMass-a.visual.transform.position;outward.z=0;
     if(outward.sqrMagnitude<.01f)outward=new Vector3(direction,.6f,0);
     Body.AddForce(outward.normalized*(Body.mass*d.amount),ForceMode.Impulse);
     foreach(var p in panels)if(!p.broken&&!p.objectSuppressed&&Vector3.Distance(p.position,a.visual.transform.position)<=ObjectRules.BlastRadius)QueueObjectBreak(p,true);
     break;
   }
  }
  bool bandHit=false;
  foreach(var p in objectBands)if(!p.broken&&!p.objectSuppressed&&!objectBreakPending.Contains(p)){
   foreach(var c in p.shapes)if(TouchesObject(c,sweep)){QueueObjectBreak(p,false);bandHit=true;break;}
  }
  if(bandHit)Body.AddForce(-v*(Body.mass*(1-ObjectRules.BandRetention)),ForceMode.Impulse);
  return false;
 }
 void ConsumeObject(ActionObject a){
  a.consumed=true;a.collider.enabled=false;a.visual.SetActive(false);
  ObjectHits.Add(new ObjectHit{kind=a.data.kind,position=a.visual.transform.position,time=Time});
 }
 void QueueObjectBreak(BreakPanel p,bool blast){
  objectBreakPending.Add(p);if(blast)objectBlastPending.Add(p);
  pendingBreaks[p]=p.position;breakNormals[p]=Vector3.up;
  breakImpulses[p]=Body.mass*Mathf.Max(p.threshold*4,ObjectRules.BandThreshold);
 }
 bool BeforeObjectStep(){
  if(!UseObjectRedesign)return false;
  MoveObjects(Time);if(DetectObjects(true))return true;
  if(ObjectRun.JetPending){ObjectRun.JetPending=false;var v=Body.linearVelocity;float sign=Mathf.Abs(v.x)>.01f?Mathf.Sign(v.x):direction;
   Body.AddForce(new Vector3(sign,ObjectRules.JetUp,0).normalized*(Body.mass*ObjectRules.JetDeltaSpeed),ForceMode.Impulse);
  }
  if(ObjectRun.Dive)Body.AddForce(Vector3.down*ObjectRules.DiveAcceleration,ForceMode.Acceleration);
  if(ObjectRun.Spinning(Time)){
   var target=new Vector3(0,.28f,-.96f).normalized*(direction*ObjectRules.Get(ObjectRules.Kind.Spin).amount);
   Body.AddTorque(target-Body.angularVelocity,ForceMode.VelocityChange);
  }
  return false;
 }
 void ObjectGroundBounce(){
  if(UseObjectRedesign&&ObjectRun.Spinning(Time)&&contactNormal.y>.35f&&incoming.y< -1&&
     (contactSurface=="sand"||contactSurface=="float"||contactSurface=="tank"))
   Body.AddForce(Vector3.up*(Body.mass*ObjectRules.BounceDelta(Body.linearVelocity.y,true)),ForceMode.Impulse);
 }
 public bool ObjectJet()=>UseObjectRedesign&&ObjectRun.RequestJet(running&&!Finished);
 public void ObjectDive(bool held){ObjectRun.Dive=held&&UseObjectRedesign&&running&&!Finished;}
}

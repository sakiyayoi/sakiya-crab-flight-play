using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class RigidCrabFlight {
 public bool NewLoopActive {get;private set;}
 public bool NewLoopReleased {get;private set;}
 public bool NewLoopParked {get;private set;}
 public bool NewLoopBeamActive {get;private set;}
 public bool NewLoopBeamUsed {get;private set;}
 public bool NewLoopBeamArmed {get;private set;}
 public int NewLoopSegment {get;private set;}
 public float NewLoopEnergy {get;private set;}
 public Vector3 NewLoopContact {get;private set;}
 public string NewLoopEndReason {get;private set;}="";
 public string NewLoopContactId {get;private set;}="";
 public int NewLoopTotal=>panels.Count(NewLoopTarget);
 public int NewLoopBroken=>panels.Count(p=>NewLoopTarget(p)&&p.broken);
 public readonly List<string> NewLoopImpulsed=new List<string>();
 public double NewLoopRestoreMs {get;private set;}
 readonly HashSet<BreakPanel> newLoopBeamPending=new HashSet<BreakPanel>();
 readonly HashSet<BreakPanel> newLoopPhysicalPending=new HashSet<BreakPanel>();
 sealed class RestPanel {public BreakPanel panel;public Vector3 position,scale;public Quaternion rotation;public Rigidbody body;}
 readonly List<RestPanel> newLoopRest=new List<RestPanel>();
 readonly Dictionary<BreakPanel,RestPanel> newLoopRestByPanel=new Dictionary<BreakPanel,RestPanel>();
 readonly List<BreakPanel> newLoopBeamTargets=new List<BreakPanel>();
 public float NewLoopLaunchHeight {get;private set;}
 Vector3 newLoopBeamPosition,newLoopBeamVelocity,newLoopBeamOmega;
 Quaternion newLoopBeamRotation;
 public void ConfigureNewLoop(int segment){
  NewLoopActive=true;NewLoopSegment=Mathf.Clamp(segment,0,NewLoopRules.SegmentCount-1);
  if(newLoopRest.Count==0)foreach(var p in panels.Where(p=>PortLayout.Find(p.id)!=null)){
   var t=p.visual.transform;var rest=new RestPanel{panel=p,position=t.position,rotation=t.rotation,scale=t.localScale};
   newLoopRest.Add(rest);newLoopRestByPanel.Add(p,rest);
  }
 }
 public void BeginNewLoop(float energy,bool beamArmed){
  NewLoopEnergy=NewLoopRules.Energy(energy);NewLoopBeamArmed=beamArmed;
  NewLoopReleased=NewLoopParked=NewLoopBeamUsed=NewLoopBeamActive=false;
  NewLoopEndReason=NewLoopContactId="";NewLoopImpulsed.Clear();
  // Charge is release energy only: legacy rotor/funding stock is not carried into this mode.
  ResetRotor();Remaining=0;braced=pressed=false;
  Body.position=new Vector3(NewLoopRules.SegmentStart(NewLoopSegment),NewLoopLaunchHeight,0);root.position=Body.position;
  float angle=settings.angle*Mathf.Deg2Rad;
  Body.linearVelocity=new Vector3(NewLoopRules.LaunchSpeed*Mathf.Cos(angle),NewLoopRules.LaunchSpeed*Mathf.Sin(angle),0);
  UnityEngine.Physics.SyncTransforms();
 }
 void RestoreNewLoopRest(){
  var timer=System.Diagnostics.Stopwatch.StartNew();running=false;
  if(NewLoopBeamActive)EndNewLoopBeam();
  foreach(var r in newLoopRest){
   if(r.body){if(!r.body.isKinematic){r.body.linearVelocity=Vector3.zero;r.body.angularVelocity=Vector3.zero;}r.body.isKinematic=true;}
   r.panel.visual.transform.SetPositionAndRotation(r.position,r.rotation);r.panel.visual.transform.localScale=r.scale;
  }
  ResetRestaurant();ResetRotor();
  newLoopBeamPending.Clear();newLoopPhysicalPending.Clear();NewLoopImpulsed.Clear();
  NewLoopReleased=NewLoopParked=NewLoopBeamUsed=NewLoopBeamActive=NewLoopBeamArmed=false;NewLoopEnergy=0;NewLoopEndReason=NewLoopContactId="";
  Body.isKinematic=true;Body.position=new Vector3(0,restHeight+.4f,0);Body.rotation=Quaternion.Euler(0,90,0);
  root.SetPositionAndRotation(Body.position,Body.rotation);braced=pressed=false;
  SyncNewLoopProxies();UnityEngine.Physics.SyncTransforms();
  NewLoopLaunchHeight=Mathf.Max(Body.position.y+.455f-Lowest(),.455f+RotationRadius());
  // No Physics.Simulate / settling step here: the snapshot is the authored rest state.
  timer.Stop();NewLoopRestoreMs=timer.Elapsed.TotalMilliseconds;
 }
 public void ReleaseNewLoopActor(){
  running=false;if(root)root.gameObject.SetActive(false);
  foreach(var m in colliders.Where(c=>c).Select(c=>c.sharedMaterial).Distinct().ToArray())if(m&&owned.Remove(m))Release(m);
  foreach(var mesh in colliders.OfType<MeshCollider>().Where(c=>c).Select(c=>c.sharedMesh).Distinct().ToArray())if(mesh)Release(mesh);
  launchCompoundFrozen=false;LaunchEnvelopeError=LaunchBoundsDifferenceR30=0;hullVerticesR21.Clear();supportGeometryR14.Clear();
 }
 bool NewLoopTarget(BreakPanel p){var e=PortLayout.Find(p.id);return e!=null&&e.breakable&&e.beat==NewLoopRules.Beat(NewLoopSegment);}
 void ObserveNewLoopContact(BreakPanel p,Vector3 point){
  if(!NewLoopActive||NewLoopReleased||NewLoopParked||!NewLoopTarget(p))return;
  NewLoopReleased=true;NewLoopContact=point;NewLoopContactId=p.id;
  PortRoleReceipts.Add(new PortRoleReceipt{id=p.id,role="release",cause="first-breakable-contact",time=Time,before=Body.linearVelocity,after=Body.linearVelocity});
  ScheduleNewLoopWave(p,true);
 }
 bool ScheduleNewLoopWave(BreakPanel source,bool release=false){
  if(!NewLoopActive)return false;
  if(!NewLoopReleased||NewLoopParked)return true;
  var entry=PortLayout.Find(source.id);if(entry==null)return true;
  bool ignition=NewLoopRules.Ignition(entry.role)||portChainScheduled.Contains(source.id);
  var eligible=panels.Where(p=>p!=source&&!p.broken&&p.city2&&p.cityProposal.massKgSuggestion<=100&&NewLoopTarget(p))
   .Where(p=>entry.chain.Length>0&&PortLayout.Find(p.id).chain==entry.chain).ToArray();
  int baseline=eligible.Count(p=>Vector3.Distance(source.position,p.position)<=8);
  int capacity=ignition?NewLoopRules.Capacity(baseline,NewLoopEnergy):release?NewLoopRules.OrdinaryCapacity(NewLoopEnergy):0,count=0;
  foreach(var p in eligible.Where(p=>Vector3.Distance(source.position,p.position)<=NewLoopRules.Reach(NewLoopEnergy)).OrderBy(p=>Vector3.Distance(source.position,p.position)).ThenBy(p=>p.id,StringComparer.Ordinal)){
   if(portScheduled.Contains(p.id))continue;if(count++>=capacity)break;
   if(ignition)portChainScheduled.Add(p.id);portScheduled.Add(p.id);
   portWaves.Add(new PortWave{target=p,from=source.position,due=Time+PortWaveDelay});
  }
  return true;
 }
 // ON waves ONLY move real bodies. The contact callback below supplies solver impulses to ApplyBreaks.
 void ImpulseNewLoopPanel(PortWave wave){
  var p=wave.target;if(!NewLoopReleased||NewLoopParked||p.broken||!NewLoopTarget(p)||p.cityProposal.massKgSuggestion>100)return;
  var rest=newLoopRestByPanel[p];
  if(!rest.body){rest.body=p.visual.AddComponent<Rigidbody>();rest.body.mass=p.cityProposal.massKgSuggestion;rest.body.isKinematic=true;rest.body.collisionDetectionMode=CollisionDetectionMode.ContinuousSpeculative;}
  var body=rest.body;body.isKinematic=false;
  var away=p.visual.transform.position-wave.from;away.y=0;if(away.sqrMagnitude<.01f)away=Vector3.right;
  body.AddForce((away.normalized+Vector3.up)*body.mass*PortWaveImpulsePerMass,ForceMode.Impulse);body.WakeUp();
  NewLoopImpulsed.Add(p.id);
 }
 void SyncNewLoopProxies(){
  foreach(var r in newLoopRest)foreach(var shape in r.panel.shapes){
   if(debrisStaticsCalmR4.TryGetValue(shape,out var proxy))proxy.transform.SetPositionAndRotation(shape.transform.position,shape.transform.rotation);
  }
 }
 void ReadNewLoopObjectContacts(ContactPairHeader header){
  if(!NewLoopActive||!NewLoopReleased||NewLoopParked)return;
  for(int j=0;j<header.pairCount;j++){
   var pair=header.GetContactPair(j);if(pair.isCollisionExit)continue;
   breakByCollider.TryGetValue(pair.colliderInstanceID,out var a);breakByCollider.TryGetValue(pair.otherColliderInstanceID,out var b);
   bool movingA=a!=null&&newLoopRestByPanel.TryGetValue(a,out var ra)&&ra.body&&!ra.body.isKinematic;
   bool movingB=b!=null&&newLoopRestByPanel.TryGetValue(b,out var rb)&&rb.body&&!rb.body.isKinematic;
   if(!movingA&&!movingB)continue;
   for(int k=0;k<pair.contactCount;k++){
    var c=pair.GetContactPoint(k);if(c.impulse.sqrMagnitude<.000001f)continue;
    QueueNewLoopSolverBreak(a,c.impulse.magnitude,c.position,c.normal);
    QueueNewLoopSolverBreak(b,c.impulse.magnitude,c.position,-c.normal);
   }
  }
 }
 void QueueNewLoopSolverBreak(BreakPanel p,float impulse,Vector3 point,Vector3 normal){
  if(p==null||p.broken||!NewLoopTarget(p)||primaryPending.Contains(p)||newLoopBeamPending.Contains(p))return;
  pendingBreaks[p]=point;breakNormals[p]=normal;breakImpulses.TryGetValue(p,out float total);breakImpulses[p]=total+impulse;
  secondarySources.Remove(p);portWavePending.Add(p);newLoopPhysicalPending.Add(p);
 }
 void StopNewLoopPanel(BreakPanel p){if(newLoopRestByPanel.TryGetValue(p,out var rest)&&rest.body&&!rest.body.isKinematic){rest.body.linearVelocity=Vector3.zero;rest.body.angularVelocity=Vector3.zero;rest.body.isKinematic=true;}}
 public Vector3 NewLoopBeamOrigin=>newLoopBeamPosition+Vector3.up*.3f+Vector3.right*1.1f;
 public Vector3 NewLoopBeamEnd(float seconds){
  var targets=newLoopBeamTargets;
  if(targets.Count==0)return new Vector3(NewLoopRules.SegmentEnd(NewLoopSegment),2,0);
  float index=Mathf.Clamp01(seconds/NewLoopRules.BeamSeconds)*(targets.Count-1);int lo=Mathf.FloorToInt(index),hi=Mathf.Min(targets.Count-1,lo+1);
  return Vector3.Lerp(targets[lo].visual.transform.position+Vector3.up,targets[hi].visual.transform.position+Vector3.up,index-lo);
 }
 public bool BeginNewLoopBeam(){
  if(!NewLoopReleased||!NewLoopBeamArmed||NewLoopBeamUsed||NewLoopParked)return false;
  NewLoopBeamUsed=NewLoopBeamActive=true;newLoopBeamPosition=Body.position;
  newLoopBeamTargets.Clear();newLoopBeamTargets.AddRange(panels.Where(p=>NewLoopTarget(p)&&!p.broken&&p.position.x>newLoopBeamPosition.x).OrderBy(p=>p.position.x).ThenBy(p=>p.id,StringComparer.Ordinal));newLoopBeamRotation=Body.rotation;
  newLoopBeamVelocity=Body.linearVelocity;newLoopBeamOmega=Body.angularVelocity;
  Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.isKinematic=true;return true;
 }
 public void EndNewLoopBeam(){
  if(!NewLoopBeamActive)return;NewLoopBeamActive=false;
  Body.position=newLoopBeamPosition;Body.rotation=newLoopBeamRotation;root.SetPositionAndRotation(newLoopBeamPosition,newLoopBeamRotation);
  Body.isKinematic=false;Body.linearVelocity=newLoopBeamVelocity;Body.angularVelocity=newLoopBeamOmega;Body.WakeUp();
 }
 public void SweepNewLoopBeam(float fromAge,float toAge){
  if(!NewLoopBeamActive)return;
  var origin=NewLoopBeamOrigin;int slices=Mathf.Max(1,Mathf.CeilToInt((toAge-fromAge)*240));
  for(int i=0;i<=slices;i++){
   var end=NewLoopBeamEnd(Mathf.Lerp(fromAge,toAge,(float)i/slices));
   foreach(var p in panels){if(p.broken||!NewLoopTarget(p)||newLoopBeamPending.Contains(p))continue;
    bool hit=false;foreach(var shape in p.shapes){if(!shape||!shape.enabled)continue;
     // Intersect each actual local box, including yaw/scale; an expanded AABB is not a beam hit.
     var t=shape.transform;Vector3 a=t.InverseTransformPoint(origin),b=t.InverseTransformPoint(end),d=b-a;
     var radius=Vector3.one*NewLoopRules.BeamRadius;var scale=t.lossyScale;
     radius=new Vector3(radius.x/Mathf.Abs(scale.x),radius.y/Mathf.Abs(scale.y),radius.z/Mathf.Abs(scale.z));
     var bounds=new Bounds(shape.center,shape.size+radius*2);
     if(bounds.Contains(a)||bounds.IntersectRay(new Ray(a,d.normalized),out float distance)&&distance<=d.magnitude){hit=true;break;}}
    if(!hit)continue;
    pendingBreaks[p]=p.visual.transform.position;breakNormals[p]=Vector3.up;breakImpulses[p]=Body.mass*NewLoopRules.BeamImpulsePerMass;
    primaryPending.Remove(p);secondarySources.Remove(p);portWavePending.Add(p);newLoopBeamPending.Add(p);
   }
  }
 }
 string NewLoopBreakCause(BreakPanel p,bool wave,bool secondary)=>newLoopBeamPending.Contains(p)?"beam":newLoopPhysicalPending.Contains(p)?"wave-contact":secondary?"debris-contact":"direct-contact";
 public void StepNewLoop(){
  if(NewLoopParked)return;
  if(NewLoopBeamActive){
   PrepareCityDebris();Physics.Simulate(StepSeconds);SyncNewLoopProxies();StepDebrisCalmR4();Time+=StepSeconds;TickCityDecor(StepSeconds);ApplyBreaks();
  }else{
   Step();SyncNewLoopProxies();
   bool rest=hasContact&&Body.linearVelocity.sqrMagnitude<.09f&&Body.angularVelocity.sqrMagnitude<.36f;
   if(Finished||rest||!NewLoopRules.Contains(Body.position.x,NewLoopSegment))ParkNewLoop(AbortReason.Length>0?AbortReason:Finished||rest?"stopped":"segment-exit");
  }
 }
 public void ParkNewLoop(string reason){
  if(NewLoopParked)return;if(NewLoopBeamActive)EndNewLoopBeam();NewLoopParked=true;NewLoopEndReason=reason;running=false;
  Body.linearVelocity=Vector3.zero;Body.angularVelocity=Vector3.zero;Body.isKinematic=true;
  foreach(var r in newLoopRest)StopNewLoopPanel(r.panel);
  portWaves.Clear();pendingBreaks.Clear();breakImpulses.Clear();breakNormals.Clear();newLoopBeamPending.Clear();newLoopPhysicalPending.Clear();
  Distance=Mathf.Max(0,Body.position.x-NewLoopRules.SegmentStart(NewLoopSegment));
 }
 public void FinishNewLoop(string reason){if(!NewLoopParked)ParkNewLoop(reason);Stage=KickFlightPhysics.RunStage.RESULT;Event("settled");}
}

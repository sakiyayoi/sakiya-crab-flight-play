using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class NewLoopState {
 public string revision=NewLoopRules.Revision,beat,segmentName,endReason,contactId;
 public int segment,total,broken,tier,previousBroken,delta;
 public float energy,destructionRate,elapsed,releaseAge,beamAge,aimAngle;
 public bool released,cleared,nextUnlocked,beamArmed,beamActive,beamUsed,overSpun,dizzy,hasPrevious,held;
 public double resetMs,restoreMs,resultToReadyMs;
}
public sealed partial class CrabFlightGame {
 // Set OFF before Initialize (Inspector or native fixture) to use the original full-flight path.
 public bool useNewLoop=true;
 public int newLoopSegment;
 float newLoopElapsed,newLoopReleaseAge,newLoopBeamAge;
 bool newLoopObserved,newLoopBeamHeld,newLoopLaunchArmed,newLoopCleared;
 int newLoopFinalBroken,newLoopPreviousBroken,newLoopDelta;
 readonly int[] newLoopLastCounts={-1,-1};readonly bool[] newLoopUnlocked={true,false};
 double newLoopResetMs,newLoopResultAt=-1,newLoopResultToReadyMs;
 System.Diagnostics.Stopwatch newLoopResetWatch;
 LineRenderer newLoopBeamLine,newLoopChargeRing;
 readonly Dictionary<Renderer,bool> newLoopHiddenCrab=new Dictionary<Renderer,bool>();
 float NewLoopStart=>NewLoopRules.SegmentStart(newLoopSegment);
 float NewLoopEnd=>NewLoopRules.SegmentEnd(newLoopSegment);
 bool NewLoopMotion=>!ReducedMotion&&!feelSettingsR27.reducedMotion&&feelSettingsR27.preset!="off";
 float NewLoopAimAngle=>Mathf.Clamp(settings.angle+NewLoopRules.Wobble(spinCharge.OverSpun,spinCharge.Held,spinCharge.OverSpinAge),10,45);
 public void SetNewLoopBeamHeld(string value){
  if(!useNewLoop||phase!="flight"||paused)return;
  newLoopBeamHeld=value=="1";
  if(!newLoopBeamHeld)simulation.EndNewLoopBeam();
  else if(simulation.BeginNewLoopBeam())newLoopBeamAge=0;
  UpdateCamera(0,true);Report();
 }
 public void SelectNewLoopSegment(string value){
  if(!useNewLoop||phase=="flight"||paused||!int.TryParse(value,out int segment)||segment<0||segment>=NewLoopRules.SegmentCount||!newLoopUnlocked[segment])return;
  newLoopSegment=segment;Prepare("");
 }
 void ApplyNewLoopAim(){
  newLoopLaunchArmed=spinCharge.BeamArmed;settings.angle=NewLoopAimAngle;direction=1;
  // Normalize linear speed before Launch's inertia/funding calculations, independently of charge or upgrades.
  settings.power=(float)(NewLoopRules.LaunchSpeed/((1+settings.upgradeLevel*.04)*.49*(.86+settings.claw*.0048)/Math.Pow(settings.Mass,.32)));
 }
 void StartNewLoop(){
  spinCharge.Held=false;newLoopElapsed=newLoopReleaseAge=newLoopBeamAge=0;newLoopObserved=newLoopBeamHeld=false;
  newLoopFinalBroken=0;newLoopCleared=false;
  simulation.BeginNewLoop(launchChargeEnergy,newLoopLaunchArmed);
  message="最初に当てる場所で、連鎖を変えよう";
 }
 void ResetNewLoopGame(){
  newLoopElapsed=newLoopReleaseAge=newLoopBeamAge=0;newLoopObserved=newLoopBeamHeld=newLoopLaunchArmed=false;
  newLoopFinalBroken=0;newLoopCleared=false;newLoopPreviousBroken=newLoopLastCounts[newLoopSegment];newLoopDelta=0;
  crab.Root.position=new Vector3(NewLoopStart,world.NewLoopLaunchHeight,0);world.Body.position=crab.Root.position;
  shadow.position=new Vector3(NewLoopStart,.045f,0);landingFlag.gameObject.SetActive(false);bestFlag.gameObject.SetActive(false);
  if(newLoopBeamLine)newLoopBeamLine.enabled=false;
  message="押して回す・上下に動かして狙う・離して発射";
 }
 void CompleteNewLoopReset(){
  if(newLoopResetWatch==null)return;newLoopResetWatch.Stop();newLoopResetMs=newLoopResetWatch.Elapsed.TotalMilliseconds;
  newLoopResultToReadyMs=newLoopResultAt<0?0:(Time.realtimeSinceStartupAsDouble-newLoopResultAt)*1000;newLoopResetWatch=null;
 }
 float AdvanceNewLoopClock(float dt){
  if(phase!="flight")return dt;
  // Update passes real, unscaled time in this mode; deadlines are independent of slow-motion.
  newLoopElapsed+=Mathf.Max(0,dt);if(newLoopObserved)newLoopReleaseAge+=Mathf.Max(0,dt);
  if(EndNewLoopIfDue())return 0;
  if(newLoopBeamHeld&&!simulation.NewLoopBeamUsed&&simulation.BeginNewLoopBeam())newLoopBeamAge=0;
  if(simulation.NewLoopBeamActive){float before=newLoopBeamAge;newLoopBeamAge=Mathf.Min(NewLoopRules.BeamSeconds,newLoopBeamAge+dt);simulation.SweepNewLoopBeam(before,newLoopBeamAge);}
  return newLoopObserved&&newLoopReleaseAge<NewLoopRules.SlowSeconds&&NewLoopMotion?Mathf.Min(dt,.08f)*NewLoopRules.SlowScale:Mathf.Min(dt,.08f);
 }
 void ObserveNewLoopRelease(){
  if(newLoopObserved||!simulation.NewLoopReleased)return;
  newLoopObserved=true;newLoopReleaseAge=0;
  Emit("release",simulation.NewLoopContactId,"",simulation.NewLoopContact,Vector3.up,simulation.Body.linearVelocity,simulation.NewLoopEnergy,false,1,"first-breakable-contact");
  if(newLoopBeamHeld&&simulation.BeginNewLoopBeam())newLoopBeamAge=0;
 }
 bool EndNewLoopIfDue(){
  if(phase!="flight")return phase=="result";
  bool outside=!NewLoopRules.Contains(simulation.Body.position.x,newLoopSegment);
  if(!NewLoopRules.ResultDue(newLoopElapsed,newLoopReleaseAge,newLoopObserved,simulation.NewLoopParked||simulation.Finished,outside))return false;
  simulation.FinishNewLoop(outside?"segment-exit":newLoopObserved?"destruction-cap":"no-contact-timeout");
  newLoopFinalBroken=simulation.NewLoopBroken;newLoopCleared=simulation.AbortReason.Length==0&&NewLoopRules.Cleared(newLoopFinalBroken,simulation.NewLoopTotal);
  newLoopPreviousBroken=newLoopLastCounts[newLoopSegment];newLoopDelta=newLoopPreviousBroken<0?0:newLoopFinalBroken-newLoopPreviousBroken;
  newLoopLastCounts[newLoopSegment]=newLoopFinalBroken;if(newLoopCleared&&newLoopSegment+1<newLoopUnlocked.Length)newLoopUnlocked[newLoopSegment+1]=true;
  phase="result";newLoopResultAt=Time.realtimeSinceStartupAsDouble;newLoopBeamHeld=false;
  if(newLoopBeamLine)newLoopBeamLine.enabled=false;if(effects)effects.EndRun(false);
  message=newLoopCleared?"区間クリア！":"狙いを変えて、もう一投！";AuditFinish();UpdateCamera(0,true);Report();return true;
 }
 void PreviewNewLoopAim(){
  if(!aim||crab==null)return;
  float a=NewLoopAimAngle*Mathf.Deg2Rad;var p=crab.Root.position;var v=new Vector3(NewLoopRules.LaunchSpeed*Mathf.Cos(a),NewLoopRules.LaunchSpeed*Mathf.Sin(a),0);
  var points=new List<Vector3>{p};for(int i=0;i<60;i++){v+=Vector3.down*9.81f*.04f;p+=v*.04f;points.Add(p);if(p.y<.5f||p.x>NewLoopEnd)break;}
  aim.positionCount=points.Count;aim.SetPositions(points.ToArray());aim.enabled=phase=="ready";
  if(aimMarker)aimMarker.gameObject.SetActive(false);
 }
 void UpdateNewLoopCamera(float dt,bool snap){
  if(simulation!=null&&phase=="flight"&&simulation.NewLoopBeamActive){
   cam.orthographic=false;cam.fieldOfView=65;cam.transform.position=simulation.NewLoopBeamOrigin;
   cam.transform.rotation=Quaternion.LookRotation(simulation.NewLoopBeamEnd(newLoopBeamAge)-cam.transform.position,Vector3.up);return;
  }
  cam.orthographic=true;cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16).normalized,Vector3.up);
  float size=cam.aspect<1?13:10,lead=Mathf.Min(6,size*cam.aspect*.4f);
  Vector3 target=new Vector3(crab.Root.position.x+lead,Mathf.Max(5,crab.Root.position.y+2),-16);
  float punch=phase=="flight"&&newLoopObserved?Mathf.Clamp01(1-newLoopReleaseAge/.28f):0;
  cam.orthographicSize=size*(NewLoopMotion?1-.22f*punch:1);
  if(snap)cam.transform.position=target;else cam.transform.position=Vector3.Lerp(cam.transform.position,target,1-Mathf.Exp(-Mathf.Max(0,dt)*12));
  if(NewLoopMotion&&feelSettingsR27.shake&&punch>0)cam.transform.position+=new Vector3(Mathf.Sin(newLoopReleaseAge*89),Mathf.Sin(newLoopReleaseAge*67)*.6f,0)*(.35f*punch*punch);
 }
 void PresentNewLoop(float dt){
  if(phase=="ready")PreviewNewLoopAim();
  if(newLoopBeamLine==null){newLoopBeamLine=Line("New loop beam",new Color(.4f,1,1),.25f);newLoopBeamLine.positionCount=2;}
  bool beam=phase=="flight"&&simulation!=null&&simulation.NewLoopBeamActive;
  if(beam){
   foreach(var renderer in crab.Root.GetComponentsInChildren<Renderer>(true))HideNewLoopCrabRenderer(renderer);
   if(neonCrab!=null)foreach(var renderer in neonCrab.Root.GetComponentsInChildren<Renderer>(true))HideNewLoopCrabRenderer(renderer);
  }else RestoreNewLoopCrabRenderers();
  newLoopBeamLine.enabled=beam;if(beam){newLoopBeamLine.SetPosition(0,simulation.NewLoopBeamOrigin);newLoopBeamLine.SetPosition(1,simulation.NewLoopBeamEnd(newLoopBeamAge));}
  // End only after the last swept contact has entered the ordinary accounting step.
  if(beam&&newLoopBeamAge>=NewLoopRules.BeamSeconds){simulation.EndNewLoopBeam();RestoreNewLoopCrabRenderers();UpdateCamera(0,true);newLoopBeamLine.enabled=false;}
  if(newLoopChargeRing==null){newLoopChargeRing=Line("Release energy",Color.cyan,.06f);newLoopChargeRing.loop=true;newLoopChargeRing.positionCount=48;}
  bool charge=phase=="ready"||phase=="flight"&&!newLoopObserved;newLoopChargeRing.enabled=charge;
  float energy=phase=="ready"?spinCharge.Energy:launchChargeEnergy;var color=NewLoopRules.Tier(energy)==3?new Color(1,.85f,.15f):NewLoopRules.Tier(energy)==2?new Color(1,.3f,.6f):new Color(.2f,.9f,1);
  newLoopChargeRing.startColor=newLoopChargeRing.endColor=color;
  float radius=1.25f+.5f*energy;for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;newLoopChargeRing.SetPosition(i,crab.Root.position+cam.transform.right*Mathf.Cos(a)*radius+cam.transform.up*Mathf.Sin(a)*radius);}
 }
 void HideNewLoopCrabRenderer(Renderer renderer){if(!newLoopHiddenCrab.ContainsKey(renderer))newLoopHiddenCrab.Add(renderer,renderer.enabled);renderer.enabled=false;}
 void RestoreNewLoopCrabRenderers(){foreach(var pair in newLoopHiddenCrab)if(pair.Key)pair.Key.enabled=pair.Value;newLoopHiddenCrab.Clear();}
 NewLoopState NewLoopSnapshot(){
  int total=world!=null?world.NewLoopTotal:0,broken=phase=="result"?newLoopFinalBroken:simulation!=null?simulation.NewLoopBroken:0;
  return new NewLoopState{segment=newLoopSegment,segmentName=NewLoopRules.Beat(newLoopSegment),beat=phase=="result"?"result":phase=="ready"?"charge":newLoopObserved?"destruction":"flight",total=total,broken=broken,tier=spinCharge.Tier,energy=phase=="ready"?spinCharge.Energy:launchChargeEnergy,elapsed=newLoopElapsed,releaseAge=newLoopReleaseAge,beamAge=newLoopBeamAge,released=newLoopObserved,cleared=newLoopCleared,destructionRate=NewLoopRules.Rate(broken,total),nextUnlocked=newLoopUnlocked[1],beamArmed=phase=="ready"?spinCharge.BeamArmed:newLoopLaunchArmed,beamActive=simulation!=null&&simulation.NewLoopBeamActive,beamUsed=simulation!=null&&simulation.NewLoopBeamUsed,overSpun=spinCharge.OverSpun,dizzy=spinCharge.Dizzy>0,held=spinCharge.Held,aimAngle=NewLoopAimAngle,hasPrevious=newLoopPreviousBroken>=0,previousBroken=newLoopPreviousBroken,delta=newLoopDelta,resetMs=newLoopResetMs,restoreMs=world!=null?world.NewLoopRestoreMs:0,resultToReadyMs=newLoopResultToReadyMs,endReason=simulation!=null?simulation.NewLoopEndReason:"",contactId=simulation!=null?simulation.NewLoopContactId:""};
 }
}

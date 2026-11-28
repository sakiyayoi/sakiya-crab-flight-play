using UnityEngine;

[System.Serializable] public sealed class ObjectCaptionState {
 public int sequence;public string kind;public bool visible;
}

public sealed partial class EffekseerBurstBridge {
 // Existing Play retains its scale=1 and all 24-handle lifecycle policies.
 public bool PlayObject(Vector3 position,Color color,float scale){
  if(!Play(position,true))return false;
  var handle=playing[playing.Count-1].handle;
  handle.SetAllColor(color);handle.SetScale(Vector3.one*scale);return true;
 }
}
public sealed partial class CrabFlightGame {
 int readObjectHits;
 ObjectCaptionState objectCaption;
 ObjectHitStopFrame objectHitStop;
 public bool ObjectHitStopEnabled=>world!=null&&world.UseObjectRedesign&&FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion&&!paused&&(phase=="flight"||phase=="result");
 bool ObjectDeathPresentationHeld=>world!=null&&world.UseObjectRedesign&&simulation!=null&&simulation.ObjectRun.Dead&&objectCapture.Active;
 void PresentObjectHitStop(ObjectRules.Kind kind){
  if(!ObjectHitStopEnabled)return;
  if(!objectHitStop){objectHitStop=cam.gameObject.AddComponent<ObjectHitStopFrame>();objectHitStop.Bind(this);}
  objectHitStop.Request(kind);
 }
 void ApplyObjectPresentationReport(FlightState report){
  AddObjectReadabilityReport(report);
  if(!ObjectDeathPresentationHeld)return;
  report.phase="flight";report.resultOverview=null;report.newRecord=false;
 }
 public void Jet(string _){if(!paused&&phase=="flight"&&simulation!=null){simulation.ObjectJet();Report();}}
 public void DiveDown(string _){if(!paused&&phase=="flight"&&simulation!=null)simulation.ObjectDive(true);}
 public void DiveUp(string _){if(simulation!=null)simulation.ObjectDive(false);}
 void ConfigureObjectsPresentation(){
  // Never change a live throw/charge. A new config is used on the next Prepare.
  if(phase!="flight"&&!aiming&&!spinCharge.Started)world.ConfigureObjects(feelR27.UsesObjectRedesign);
 }
 void ClearObjectBursts(){ClearObjectCapture();objectCaption=null;objectDeathBurst=false;if(objectHitStop)objectHitStop.Clear();if(juiceBurst)juiceBurst.Clear();}
 void DrainObjectEvents(){
  while(readObjectHits<simulation.ObjectHits.Count){
   var hit=simulation.ObjectHits[readObjectHits++];var def=ObjectRules.Get(hit.kind);
   // One latest-value snapshot survives the 32-event ring when a blast causes
   // many destructions before Report. This is presentation, never another hit.
   objectCaption=new ObjectCaptionState{sequence=readObjectHits,kind=hit.kind.ToString().ToLowerInvariant(),
    visible=FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion&&!paused};
   Emit("action",hit.kind.ToString().ToLowerInvariant(),"action",hit.position,Vector3.up,simulation.Body.linearVelocity,1);
   if(hit.kind==ObjectRules.Kind.Death){BeginObjectCapture(hit.position);if(ReducedMotion||feelSettingsR27.reducedMotion)continue;}
   if(!FeelEffectsR27||paused)continue;
   PresentObjectHitStop(hit.kind);
   ColorUtility.TryParseHtmlString(def.color,out var color);
   feelDrawingR27.ObjectParticles(hit.position,color,cam,Time.realtimeSinceStartup,def.burstScale);
   if(feelR27.UsesEffekseer&&!ReducedMotion&&!feelSettingsR27.reducedMotion&&juiceBurst)
    {objectDeathBurst=hit.kind==ObjectRules.Kind.Death;juiceBurst.PlayObject(hit.position,color,def.burstScale);}
  }
 }
}
public sealed partial class MashimashiPresentationR27 {
 public void ObjectParticles(Vector3 position,Color color,Camera camera,float now,float scale){
  int count=JuiceParticles(position,color,"metal",camera,now);
  for(int i=marks.Count-count;i<marks.Count;i++){
   var m=marks[i];m.size*=scale;m.v*=Mathf.Sqrt(scale);marks[i]=m;
  }
 }
}

using UnityEngine;

[System.Serializable] public sealed class ObjectBadgeState {
 public string kind;public float x,y;public bool visible;public int side;
}
[System.Serializable] public sealed class ObjectCaptureState {
 public string stage;public bool caption;
}
public sealed partial class RigidCrabFlight {
 ObjectBadgeState[] readabilityBadges;
 // Read the true visual center; screen-space badges never inherit root rotation.
 public ObjectBadgeState[] ReadObjectBadges(Camera camera){
  if(readabilityBadges==null||readabilityBadges.Length!=actionObjects.Count){
   readabilityBadges=new ObjectBadgeState[actionObjects.Count];
   for(int i=0;i<readabilityBadges.Length;i++)readabilityBadges[i]=new ObjectBadgeState{kind=actionObjects[i].data.kind.ToString().ToLowerInvariant(),side=actionObjects[i].side};
  }
  for(int i=0;i<actionObjects.Count;i++){
   var a=actionObjects[i];var b=readabilityBadges[i];
   var center=camera.WorldToViewportPoint(a.visual.transform.position);
   var top=camera.WorldToViewportPoint(a.visual.transform.position+camera.transform.up*(ObjectRules.ObjectScale*1.35f));
   b.x=top.x;b.y=top.y;b.visible=UseObjectRedesign&&!a.consumed&&center.z>0&&center.x>=0&&center.x<=1&&center.y>=0&&center.y<=1;
  }
  return readabilityBadges;
 }
 public void PoseObjectReadability(float time,bool reduced){
  if(!UseObjectRedesign)return;
  foreach(var a in actionObjects)if(!a.consumed)a.storyMotion.Pose(time,reduced);
 }
}
public sealed partial class CrabFlightGame {
 readonly ObjectCaptureTimeline objectCapture=new ObjectCaptureTimeline();
 readonly ObjectCaptureState objectCaptureState=new ObjectCaptureState();
 ObjectCaptureView objectCaptureView;
 bool ObjectCaptureAnimated=>FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion;
 void BeginObjectCapture(Vector3 fisher){
  objectCapture.Begin(Time.realtimeSinceStartupAsDouble,ObjectCaptureAnimated);
  if(!ObjectCaptureAnimated)return;
  if(!objectCaptureView)objectCaptureView=gameObject.AddComponent<ObjectCaptureView>();
  objectCaptureView.Begin(neonCrab!=null?neonCrab.Root:crab.Root,transform,crab.Root.position,fisher,cam);
 }
 void ClearObjectCapture(){objectCapture.Clear();if(objectCaptureView)objectCaptureView.Clear();}
 void LateObjectReadability(){
  if(!initialized||world==null)return;
  if(!paused)world.PoseObjectReadability(Time.realtimeSinceStartup,!ObjectCaptureAnimated);
  if(!objectCapture.Active)return;
  objectCapture.Advance(Time.realtimeSinceStartupAsDouble,objectHitStop&&objectHitStop.Holding,ObjectCaptureAnimated,paused);
  if(objectCaptureView){
   if(!ObjectCaptureAnimated||!objectCapture.Active)objectCaptureView.Clear();
   else objectCaptureView.Pose(objectCapture.Current,objectCapture.PullProgress,paused);
  }
 }
 void AddObjectReadabilityReport(FlightState report){
  report.objectBadges=world!=null?world.ReadObjectBadges(cam):null;
  objectCaptureState.stage=objectCapture.Current.ToString().ToLowerInvariant();
  objectCaptureState.caption=objectCapture.CaptionVisible&&!paused;
  report.objectCapture=objectCaptureState;
 }
}

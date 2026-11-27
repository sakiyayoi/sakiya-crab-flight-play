using System;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class PortPreview {
 public bool cameraEnabled,reversalReady;public string landmark;public float remaining;public string[] nextRoles;
}
public sealed partial class CrabFlightGame {
 // Inspector/runtime switch. False executes the original R19 framing and following verbatim.
 public bool usePortCamera=true;
 public void SetPortCamera(string value){Initialize();usePortCamera=value!="0";portFollowing=false;portMoment=portShake=0;UpdateCamera(0,true);Report();}
 float portMoment,portShake,portShakeDuration,portShakeAge,portVertical,portZoom;
 bool portHigh,portFollowing;
 float PortCameraDt=>Mathf.Min(Time.unscaledDeltaTime,.08f);
 bool PortMotion=>usePortCamera&&!ReducedMotion&&!feelSettingsR27.reducedMotion&&feelSettingsR27.preset!="off";
 void ObservePortCamera(FlightGameEvent e){
  if(e.type=="reset"){portFollowing=false;portHigh=false;portMoment=portShake=0;portCrushCount=0;portLastCrushTime=-10;return;}
  if(!usePortCamera||e.type!="destruction")return;
  var entry=PortLayout.Find(e.objectId);
  if(entry!=null&&(entry.role=="landmark"||entry.role=="jackpot"||entry.role.StartsWith("boost",StringComparison.Ordinal)))portMoment=.25f;
  // One presentation frame may contain several actual breaks. Aggregate only that physics instant.
  if(Mathf.Abs(e.physicsTime-portLastCrushTime)>.02f)portCrushCount=0;
  portLastCrushTime=e.physicsTime;portCrushCount++;
  portShake=Mathf.Clamp(portCrushCount*.02f,0,.18f);portShakeDuration=Mathf.Lerp(.10f,.16f,Mathf.Clamp01(portCrushCount/9f));portShakeAge=0;
 }
 float portLastCrushTime=-10;int portCrushCount;
 bool FramePortCamera(ref float size,ref float lead,ref float centerY){
  if(!usePortCamera||phase!="flight"||simulation==null)return false;
  float aspect=Mathf.Max(.3f,cam.aspect),speed=Mathf.Abs(simulation.Body.linearVelocity.x),altitude=Mathf.Max(0,crab.Root.position.y);
  bool redesign=simulation.UseObjectRedesign;
  float desired=redesign?PortCameraSettings.Zoom(speed,altitude,aspect):PortRules.Zoom(speed,altitude,aspect);
  var landmark=PortLayout.NextLandmark(crab.Root.position.x,direction);
  // Reveal at entry into the forward half of the view, then release after passing or destroying it.
  if(landmark!=null&&!simulation.BrokenObjects.Contains(landmark.id)&&landmark.p[0]-crab.Root.position.x<desired*aspect*1.35f)desired*=1.15f;
  if(redesign)desired=Mathf.Min(desired,PortCameraSettings.Maximum);
  Vector3 nextObject=Vector3.zero;
  bool hasNext=redesign&&simulation.NextObjectCameraTarget(crab.Root.position.x,simulation.Body.linearVelocity.x<0?-1:1,out nextObject);
  float upY=16/Mathf.Sqrt(16*16+5.3f*5.3f);
  if(hasNext)desired=PortCameraSettings.FitNext(desired,Mathf.Abs(nextObject.x-crab.Root.position.x),(nextObject.y-altitude)*upY,aspect);
  float realDt=paused?0:PortCameraDt;portMoment=Mathf.Max(0,portMoment-realDt);
  float enter=Mathf.SmoothStep(0,1,simulation.Time/.32f);
  size=Mathf.Lerp(size,desired,enter);
  lead=PortRules.Lead(speed,aspect,size);
  if(hasNext)lead=Mathf.Max(lead,Mathf.Min(Mathf.Abs(nextObject.x-crab.Root.position.x)*.5f,size*aspect*.45f));
  // Hysteresis prevents zoom/bounce chatter. The ground appears at 20% from the bottom.
  if(!portHigh&&simulation.Body.linearVelocity.y>=0&&altitude>size*.9f)portHigh=true;
  if(portHigh&&simulation.Body.linearVelocity.y<0&&altitude<size*.85f)portHigh=false;
  float targetY=portHigh?altitude-size*.10f/upY:size*.60f/upY;
  if(hasNext){
   float low=Mathf.Max(altitude-(size-PortCameraSettings.SubjectMargin)/upY,nextObject.y-(size-PortCameraSettings.NextObjectMargin)/upY);
   float high=Mathf.Min(altitude+(size-PortCameraSettings.SubjectMargin)/upY,nextObject.y+(size-PortCameraSettings.NextObjectMargin)/upY);
   if(low<=high)targetY=Mathf.Clamp(targetY,low,high);
  }
  if(!portFollowing)portVertical=targetY;
  else portVertical=Mathf.Lerp(portVertical,targetY,1-Mathf.Exp(-realDt*PortCameraSettings.VerticalFollowRate));
  centerY=Mathf.Lerp(centerY,portVertical,enter);return true;
 }
 bool FollowPortCamera(Vector3 target,float size,float dt,bool snap){
  if(!usePortCamera||phase!="flight")return false;
  float elapsed=snap||paused?0:PortCameraDt;
  portZoom=snap||!portFollowing?size:Mathf.Lerp(portZoom,size,1-Mathf.Exp(-elapsed*PortCameraSettings.ZoomFollowRate));
  cam.orthographicSize=portZoom*(PortMotion?Mathf.Lerp(1,.88f,portMoment/.25f):1);
  var anchor=simulation.Body.worldCenterOfMass;var offset=target-anchor;
  if(snap||!portFollowing)cameraOffsetR29=offset;
  else cameraOffsetR29=Vector3.Lerp(cameraOffsetR29,offset,1-Mathf.Exp(-elapsed*PortCameraSettings.PositionFollowRate));
  cameraOffsetR29.x=Mathf.Clamp(cameraOffsetR29.x,-cam.orthographicSize*cam.aspect*.45f,cam.orthographicSize*cam.aspect*.45f);
  cam.transform.position=new Vector3(anchor.x+cameraOffsetR29.x,target.y,target.z);
  portFollowing=true;cameraFollowingR29=true;return true;
 }
 Vector3 PortShakeOffset(float dt){
  portShakeAge+=Mathf.Max(0,dt);
  if(!PortMotion||!feelSettingsR27.shake||portShakeDuration<=0||portShakeAge>=portShakeDuration)return Vector3.zero;
  float t=portShakeAge/portShakeDuration,a=portShake*(1-t)*(1-t);
  return Vector3.ClampMagnitude(cam.transform.right*(Mathf.Sin(t*43)*a)+cam.transform.up*(Mathf.Sin(t*57+1)*a*.65f),a);
 }
 PortPreview PortPreviewState(){
  var next=PortLayout.NextLandmark(crab.Root.position.x,direction);
  return new PortPreview{cameraEnabled=usePortCamera,reversalReady=simulation!=null&&simulation.PortReversalReady,landmark=next==null?"":next.beat=="S1"?"港のクレーンゲート":"ヤードのクレーン",remaining=next==null?0:Mathf.Max(0,next.p[0]-crab.Root.position.x),
   nextRoles=!PortLayout.Enabled||direction<0?Array.Empty<string>():PortLayout.Data.instances.Where(e=>e.role.Length>0&&e.p[0]>crab.Root.position.x&&(simulation==null||!simulation.BrokenObjects.Contains(e.id))).OrderBy(e=>e.p[0]).Take(3).Select(e=>e.role).ToArray()};
 }
}

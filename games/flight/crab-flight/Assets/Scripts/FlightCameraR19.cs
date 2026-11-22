using UnityEngine;
// Flight framing only. Result overview, input, simulation and hit-stop clocks stay authoritative.
public sealed partial class CrabFlightGame {
 Vector3 cameraOffsetR29;bool cameraFollowingR29;float steadyFlightSpeedR3;
#if UNITY_EDITOR
 public bool ReviewFlightCameraR19Enabled=true;
#endif
 void FrameFlightR19(ref float size,ref float lead,ref float centerY){
  if(phase!="flight"||simulation==null)return;
#if UNITY_EDITOR
  if(!ReviewFlightCameraR19Enabled)return;
#endif
  if(!cameraFollowingR29)steadyFlightSpeedR3=Mathf.Abs(simulation.Body.linearVelocity.x);
  if(FramePortCamera(ref size,ref lead,ref centerY))return;
  float aspect=Mathf.Max(.3f,cam.aspect),narrow=Mathf.InverseLerp(1.4f,.55f,aspect);
  // Launch calls UpdateCamera before the first simulation step updates Vx.
  // Read the already-launched body once, preserving the calm constant zoom.
  if(!cameraFollowingR29)steadyFlightSpeedR3=Mathf.Abs(simulation.Body.linearVelocity.x);
  float speed=steadyFlightSpeedR3,speedZoom=Mathf.SmoothStep(0,1,Mathf.InverseLerp(16,85,speed));
  float desired=Mathf.Lerp(Mathf.Lerp(11,22.5f,narrow),Mathf.Lerp(19,27,narrow),speedZoom);
  float maximum=Mathf.Lerp(28,25,Mathf.InverseLerp(.45f,1.7f,aspect));
  float framedSize=Mathf.Min(Mathf.Max(size,desired),maximum);
  float framedLead=Mathf.Min(Mathf.Min(15,4+speed*.14f),framedSize*aspect*.42f);
  float enter=Mathf.SmoothStep(0,1,(float)simulation.Time/.32f);
  size=Mathf.Lerp(size,framedSize,enter);lead=Mathf.Lerp(lead,framedLead,enter);
  float framedY=Mathf.Max(centerY,size*.6f-1);framedY=Mathf.Max(framedY,crab.Root.position.y-size*.38f);
  centerY=Mathf.Lerp(centerY,framedY,enter);
 }
 void FollowFlightCameraR29(Vector3 target,float size,float dt,bool snap){
  if(FollowPortCamera(target,size,dt,snap))return;
  float f=snap?1:1-Mathf.Exp(-dt*3),zoom=snap?1:1-Mathf.Exp(-dt*2.5f);
  cam.orthographicSize=Mathf.Lerp(cam.orthographicSize,size,zoom);
  if(phase=="flight"){
   // Follow COM horizontally without speed lag; damp vertical composition independently.
   // Root rotation/bounces must not be copied directly into the screen's vertical motion.
   Vector3 anchor=simulation.Body.worldCenterOfMass,offset=target-anchor;
   if(snap||!cameraFollowingR29)cameraOffsetR29=offset;else cameraOffsetR29=Vector3.Lerp(cameraOffsetR29,offset,f);
   float maxLead=cam.orthographicSize*Mathf.Max(.3f,cam.aspect)*.42f;cameraOffsetR29.x=Mathf.Clamp(cameraOffsetR29.x,-maxLead,maxLead);
   cam.transform.position=new Vector3(anchor.x+cameraOffsetR29.x,Mathf.Lerp(cam.transform.position.y,target.y,f),Mathf.Lerp(cam.transform.position.z,target.z,f));cameraFollowingR29=true;
  }else{cameraFollowingR29=false;cam.transform.position=Vector3.Lerp(cam.transform.position,target,f);}
 }
}

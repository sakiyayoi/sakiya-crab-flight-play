using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public static class RigidFlightReview {
 [Serializable] public class Trial {public string label;public FlightState state;public RigidImpact[] contacts;public int contactSamples,colliders,speedCaps;public float minBottom,maxSpeed,maxSpin;public Vector3 rootPosition,centerOfMass;}
 [Serializable] public class Result {public string revision="crab-flight-rigidbody1",scope="Real Unity Rigidbody/compound/ContactEvent native fixtures. Editor-only additive scene uses restored manual defaultPhysics; runtime uses local Physics3D. Browser/iPhone/listening separate. Drop fixtures explicitly override initial pose/velocity, not user gesture evidence.";public List<Trial> trials=new List<Trial>();public int configurations;public bool repeated,physicsSpinMatters,pause,cancel,reduced,cancelReplay,frameRateInvariant,poseChanges,fastContact,kickFinite;public float analyticBaselineDistance;}
 static Result result;static string dir;
 public static void RunAndWeb(){Run();Web();}
 public static void Web(){BuildFlight.Web("../delivery/UnityWeb/rigid-r1-ready");File.Copy("../web-template/launch-input.js","../delivery/UnityWeb/rigid-r1-ready/launch-input.js",true);}
 public static void Run(){
  dir=Path.GetFullPath("../evidence/rigid-native-review04");Directory.CreateDirectory(dir);result=new Result();CrabFlightGame g=null;
  try{
   g=BuildFlight.PrepareScene();g.Initialize();
   var weak=Play(g,"weak",40,39,1,4.68f);var strong=Play(g,"strong",40,78,1,9.36f);var repeat=Play(g,"strong-repeat",40,78,1,9.36f);result.repeated=Math.Abs(strong.state.distance-repeat.state.distance)<.05f;
   var high=Play(g,"spin-high-same-power",40,70,1,8.4f);var zero=Play(g,"spin-zero-same-power",40,70,1,0);result.physicsSpinMatters=Math.Abs(high.state.distance-zero.state.distance)>.25f;result.analyticBaselineDistance=(float)KickFlightPhysics.Complete(new FlightSettings()).Distance;
   var fps30=Play(g,"same-input-30fps",40,70,1,8.4f,true,1f/30);var fps60=Play(g,"same-input-60fps",40,70,1,8.4f,true,1f/60);result.frameRateInvariant=Math.Abs(high.state.distance-fps30.state.distance)<.1f&&Math.Abs(high.state.distance-fps60.state.distance)<.1f;
   Play(g,"left",40,70,-1,-8.4f);Play(g,"low-angle-max",10,100,1,12);Play(g,"high-angle-max",80,100,1,12);
   g.SetReducedMotion("1");var reduced=Play(g,"reduced",40,70,1,8.4f);result.reduced=Math.Abs(high.state.distance-reduced.state.distance)<.05f;g.SetReducedMotion("0");
   Ready(g,40,70);g.BeginAim("");g.PreviewLaunch(Input(40,70,1,8.4f));g.Tick(.08f);g.CancelAim("");result.cancel=g.State().phase=="ready"&&!g.State().aiming&&g.State().visualSpinAngle==0;g.BeginAim("");g.ReleaseLaunch(Input(40,70,1,8.4f));var cancelled=Finish(g,"cancelled-then-same-input",1f/240);result.cancelReplay=Math.Abs(high.state.distance-cancelled.state.distance)<.05f;
   Ready(g,40,70);g.BeginAim("");g.ReleaseLaunch(Input(40,70,1,8.4f));g.Tick(.2f);var before=g.State();var rot=g.ReviewSimulation.Body.rotation;g.SetPaused("1");for(int i=0;i<60;i++)g.Tick(1f/60);var after=g.State();result.pause=before.time==after.time&&before.worldX==after.worldX&&Quaternion.Angle(rot,g.ReviewSimulation.Body.rotation)<.0001f;g.SetPaused("0");
   var feet=Drop(g,"drop-feet-first",0,new Vector3(12,-8,0),Vector3.zero);var shell=Drop(g,"drop-shell-first",180,new Vector3(12,-8,0),Vector3.zero);var claw=Drop(g,"drop-claw-first",-90,new Vector3(12,-8,0),Vector3.zero);
   var firstKinds=new HashSet<int>{feet.contacts[0].kind,shell.contacts[0].kind,claw.contacts[0].kind};result.poseChanges=firstKinds.Count>=2&&Mathf.Abs(feet.state.distance-shell.state.distance)>.2f;
   var fast=Drop(g,"fast-downward-70x35-spin12",55,new Vector3(70,-35,0),new Vector3(0,0,12*Mathf.PI*2));result.fastContact=fast.contactSamples>0&&fast.minBottom>-.45f;
   var none=Play(g,"push-none",40,70,1,8.4f);Ready(g,40,70);g.BeginAim("");g.ReleaseLaunch(Input(40,70,1,8.4f));bool sent=false;var pushed=Finish(g,"push-timed-contact",1f/240,()=>{if(!sent&&g.State().time>=none.contacts[0].time-.065f){g.KickDown("");g.KickUp("");sent=true;}});
   Ready(g,40,70);g.BeginAim("");g.ReleaseLaunch(Input(40,70,1,8.4f));var spam=Finish(g,"push-spam",1f/120,()=>{g.KickUp("");g.KickDown("");});result.kickFinite=pushed.state.kicksUsed==1&&pushed.state.perfects+pushed.state.goods==1&&spam.state.kicksUsed<=3;
   foreach(float c in new[]{0f,50f,100f})foreach(float s in new[]{0f,50f,100f})foreach(float l in new[]{0f,50f,100f}){g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=c,shell=s,legs=l,angle=40,power=70}));Play(g,"config-"+c+"-"+s+"-"+l,40,70,1,8.4f,false);result.configurations++;}
   Save();if(!result.repeated||!result.physicsSpinMatters||!result.pause||!result.cancel||!result.reduced||!result.cancelReplay||!result.frameRateInvariant||!result.poseChanges||!result.fastContact||!result.kickFinite)throw new Exception("Rigid comparison condition failed; read saved result");
  }catch(Exception error){Save();File.WriteAllText(Path.Combine(dir,"failure.txt"),error.GetType().Name+": "+error.Message);throw;}finally{if(g)g.ShutdownReviewPhysics();}
 }
 static void Save(){File.WriteAllText(Path.Combine(dir,"result.json"),JsonUtility.ToJson(result,true));}
 static string Input(float angle,float power,int direction,float spin)=>JsonUtility.ToJson(new FlightAimInput{angle=angle,power=power,direction=direction,spinRps=spin,eligible=true});
 static void Ready(CrabFlightGame g,float angle,float power){g.SetPaused("0");g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=45,shell=45,legs=45,angle=angle,power=power}));}
 static Trial Play(CrabFlightGame g,string label,float angle,float power,int direction,float spin,bool defaultParts=true,float dt=1f/240){g.SetPaused("0");g.Prepare("");if(defaultParts)g.Configure(JsonUtility.ToJson(new FlightSettings{claw=45,shell=45,legs=45,angle=angle,power=power}));g.BeginAim("");g.ReleaseLaunch(Input(angle,power,direction,spin));return Finish(g,label,dt);}
 static Trial Drop(CrabFlightGame g,string label,float roll,Vector3 velocity,Vector3 angular){Ready(g,40,70);g.BeginAim("");g.ReleaseLaunch(Input(40,70,1,0));g.ReviewSimulation.ReviewDropPose(Quaternion.AngleAxis(roll,Vector3.forward)*Quaternion.Euler(0,90,0),velocity,angular);return Finish(g,label,1f/240);}
 static Trial Finish(CrabFlightGame g,string label,float dt,Action input=null){
  bool captured=false;int impact=0;
  for(int i=0;i<14400&&!g.Finished;i++){input?.Invoke();g.Tick(dt);var sim=g.ReviewSimulation;if(!captured&&sim.Time>.65f&&(label=="strong"||label=="spin-zero-same-power")){Capture(g,Path.Combine(dir,label+"-air.png"),844,390);captured=true;}if(sim.ImpactID>impact){impact=sim.ImpactID;if((label=="strong"||label.StartsWith("drop-"))&&impact<=2)Capture(g,Path.Combine(dir,label+"-impact-"+impact+".png"),844,390);}}
  var run=g.ReviewSimulation;var trial=new Trial{label=label,state=g.State(),contacts=run.Impacts.ToArray(),contactSamples=run.ContactSamples,colliders=run.ColliderCount,speedCaps=run.SpeedCaps,minBottom=run.MinBottom,maxSpeed=run.MaxSpeed,maxSpin=run.MaxSpin,rootPosition=run.Body.position,centerOfMass=run.Body.worldCenterOfMass};result.trials.Add(trial);Save();
  if(!g.Finished||trial.state.physicsFault.Length>0||run.ContactSamples==0||run.ImpactID==0)throw new Exception(label+" did not validly settle/contact: "+trial.state.physicsFault+" samples="+run.ContactSamples);return trial;
 }
 public static void Capture(CrabFlightGame g,string file,int width,int height){var cam=g.ReviewCamera;cam.aspect=(float)width/height;g.RefreshReviewCamera();var rt=new RenderTexture(width,height,24);var prev=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prev;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);}
}

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
public static class RestaurantReview {
 [Serializable] public class Run {public string label;public FlightState state;public RigidImpact[] contacts;public string[] broken;public int maxFragments;}
 [Serializable] public class Result {public string revision="crab-flight-restaurant1",scope="Original representative aquarium/restaurant segment, real Unity compound-body contacts and impulse-triggered breakage. Not mall/city/country/space completion; browser input and listening separate.";public List<Run> runs=new List<Run>();public bool noAutomaticBreak,reset,pause,repeat,weakStrong,comfort,spinMatters,retryButton;public int configurations;public float repeatDistanceDelta,reducedRunDistanceDelta;public string comfortCriterion="same live body position/rotation/linear and angular velocity/mass unchanged by the toggle; separate chaotic runs also reported";}
 static Result result;static string dir;
 public static void RunAndWeb(){Review();Web();}
 public static void Web(){BuildFlight.Web("../delivery/UnityWeb/restaurant-r1-ready");File.Copy("../web-template/launch-input.js","../delivery/UnityWeb/restaurant-r1-ready/launch-input.js",true);}
 public static void Review(){
  dir=Path.GetFullPath("../evidence/restaurant-native07");Directory.CreateDirectory(dir);result=new Result();CrabFlightGame g=null;
  try{
   g=BuildFlight.PrepareScene();g.Initialize();g.Prepare("");Capture(g,"aquarium-ready");for(int i=0;i<600;i++)g.Tick(1f/60);result.noAutomaticBreak=g.State().glassBroken==0&&g.State().wallsBroken==0;
   var weak=Play(g,"weak-low",10,10,1,1.2f);var strong=Play(g,"strong-low",10,85,1,10.2f);g.RetryLaunch("");result.retryButton=g.State().phase=="flight"&&g.State().glassBroken==0&&g.State().wallsBroken==0&&g.State().fragments==0;var button=Finish(g,"same-input-retry-button");result.retryButton=result.retryButton&&Mathf.Abs(strong.state.distance-button.state.distance)<.1f;var again=Play(g,"strong-repeat",10,85,1,10.2f);result.repeatDistanceDelta=Mathf.Abs(strong.state.distance-again.state.distance);result.repeat=result.repeatDistanceDelta<.1f&&strong.broken.Length==again.broken.Length;
   result.weakStrong=strong.state.glassBroken>0&&strong.state.wallsBroken>0&&strong.state.distance>weak.state.distance+3;
   var zero=Play(g,"same-power-no-spin",10,85,1,0);result.spinMatters=Mathf.Abs(zero.state.distance-strong.state.distance)>.1f||zero.broken.Length!=strong.broken.Length;
   Play(g,"left-low",10,85,-1,-10.2f);Play(g,"high-ceiling",55,90,1,10.8f);Play(g,"maximum-low",10,100,1,12);
   g.SetReducedMotion("1");var comfort=Play(g,"reduced-same-input",10,85,1,10.2f);result.reducedRunDistanceDelta=Mathf.Abs(comfort.state.distance-strong.state.distance);g.SetReducedMotion("0");
   Ready(g,10,85);g.BeginAim("");g.ReleaseLaunch(Input(10,85,1,10.2f));g.Tick(.2f);var before=g.State();var rotation=g.ReviewSimulation.Body.rotation;var body=g.ReviewSimulation.Body;var position=body.position;var linear=body.linearVelocity;var angular=body.angularVelocity;var mass=body.mass;g.SetReducedMotion("1");result.comfort=body.position==position&&Quaternion.Angle(body.rotation,rotation)<.00001f&&body.linearVelocity==linear&&body.angularVelocity==angular&&body.mass==mass;g.SetReducedMotion("0");g.SetPaused("1");for(int i=0;i<100;i++)g.Tick(1f/60);var after=g.State();result.pause=before.time==after.time&&before.destructionEvent==after.destructionEvent&&Quaternion.Angle(rotation,g.ReviewSimulation.Body.rotation)<.001f;g.SetPaused("0");
   g.Prepare("");result.reset=g.State().phase=="ready";g.BeginAim("");g.ReleaseLaunch(Input(10,85,1,10.2f));result.reset=result.reset&&g.State().glassBroken==0&&g.State().wallsBroken==0&&g.State().fragments==0;
   Finish(g,"reset-then-launch");
   foreach(float c in new[]{0f,50f,100f})foreach(float s in new[]{0f,50f,100f})foreach(float l in new[]{0f,50f,100f}){g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=c,shell=s,legs=l,angle=10,power=85}));Play(g,"parts-"+c+"-"+s+"-"+l,10,85,1,10.2f,false);result.configurations++;}
   Save();if(!result.noAutomaticBreak||!result.reset||!result.pause||!result.repeat||!result.weakStrong||!result.comfort||!result.spinMatters||!result.retryButton)throw new Exception("Restaurant comparison failed; inspect saved result");
  }catch(Exception e){Save();File.WriteAllText(Path.Combine(dir,"failure.txt"),e.GetType().Name+": "+e.Message);throw;}finally{if(g)g.ShutdownReviewPhysics();}
 }
 static void Save(){File.WriteAllText(Path.Combine(dir,"result.json"),JsonUtility.ToJson(result,true));}
 static void Ready(CrabFlightGame g,float angle,float power){g.SetPaused("0");g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=45,shell=45,legs=45,angle=angle,power=power}));}
 static string Input(float a,float p,int d,float s)=>JsonUtility.ToJson(new FlightAimInput{angle=a,power=p,direction=d,spinRps=s,spinPhase=0,eligible=true});
 static Run Play(CrabFlightGame g,string name,float a,float p,int d,float s,bool defaults=true){if(defaults)Ready(g,a,p);else g.Prepare("");g.BeginAim("");g.ReleaseLaunch(Input(a,p,d,s));return Finish(g,name);}
 static Run Finish(CrabFlightGame g,string name){int glass=0,walls=0,max=0;for(int i=0;i<9000&&!g.Finished;i++){g.Tick(1f/240);var state=g.State();max=Mathf.Max(max,state.fragments);if(name=="strong-low"&&state.glassBroken>glass){Capture(g,"glass-break");glass=state.glassBroken;}if(name=="strong-low"&&state.wallsBroken>walls){Capture(g,"first-wall-break");walls=state.wallsBroken;}}
  var run=new Run{label=name,state=g.State(),contacts=g.ReviewSimulation.Impacts.ToArray(),broken=g.ReviewSimulation.BrokenObjects.ToArray(),maxFragments=max};result.runs.Add(run);Save();if(name=="strong-low")Capture(g,"after-wall-result");
  if(!g.Finished||run.state.physicsFault.Length>0||max>32)throw new Exception(name+" invalid: "+run.state.physicsFault+" fragments="+max);return run;
 }
 static void Capture(CrabFlightGame g,string name){RigidFlightReview.Capture(g,Path.Combine(dir,name+".png"),844,390);}
}

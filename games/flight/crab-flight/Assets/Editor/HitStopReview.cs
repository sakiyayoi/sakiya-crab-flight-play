using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public static class HitStopReview {
 [Serializable] public class Row {public string mode;public int trial,requests,breaks,cityFragments,fragments;public float wallSeconds,simulationSeconds,distance,frozenSeconds,maxHold;public bool pauseStable,resetClean,fullImpactMetadata,finished;}
 [Serializable] public class Result {public string scope="Same source/input, off/on/reduced. Physics solver variance remains; distance variation is not attributed to hitstop.";public int unitChecks;public List<Row> runs=new List<Row>();}
 public static void RunAndWeb(){
  var result=new Result();var c=new ImpactStop();c.Request(1,1,"destruction","wall",1,false);for(int i=0;i<100;i++)c.Request(1,1,"destruction","wall",1,false);if(c.Requests!=1||c.Remaining>.06f)throw new Exception("duplicate/hold cap");result.unitChecks++;
  for(int i=2;i<2002;i++){c.Request(1,i,"destruction","wall",1,false);c.Filter(.005f);}if(c.TotalFrozen>1.32f||c.Requests>22)throw new Exception("rolling freeze budget");result.unitChecks++;c.Reset();if(c.Remaining!=0||c.TotalFrozen!=0||c.Requests!=0)throw new Exception("reset");result.unitChecks++;
  Directory.CreateDirectory("../evidence/hitstop");
  foreach(string mode in new[]{"off","on","reduced"})for(int n=0;n<3;n++){
   var g=BuildFlight.PrepareScene();g.useHitStop=mode!="off";g.Initialize();g.SetReducedMotion(mode=="reduced"?"1":"0");var row=new Row{mode=mode,trial=n,pauseStable=true};
   try{
    g.BeginSpinCharge("");for(int i=0;i<601;i++){g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=2f/60,seconds=1f/60,direction=1}));g.Tick(1f/60);}if(!g.State().chargeReady||g.State().chargeElapsed!=10)throw new Exception("charge regression");
    g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,eligible=true}));bool paused=false,captured=false;float wall=0;
    for(int i=0;i<4800&&!g.Finished;i++){var before=g.State();if(before.hitStopRemaining>=1f/120){var t=before.time;var pos=g.ReviewSimulation.Body.position;g.Tick(1f/120);wall+=1f/120;var after=g.State();if(after.time!=t||after.worldX!=pos.x)throw new Exception("physics moved during frozen slice");}else{g.Tick(1f/60);wall+=1f/60;}
     var s=g.State();row.maxHold=Mathf.Max(row.maxHold,s.hitStopRemaining);row.fragments=Mathf.Max(row.fragments,s.fragments);row.cityFragments=Mathf.Max(row.cityFragments,s.cityFragments);
     if(!paused&&s.hitStopRemaining>0){float t=s.time,remain=s.hitStopRemaining;g.SetPaused("1");g.Tick(.5f);row.pauseStable=g.State().time==t&&g.State().hitStopRemaining==remain;g.SetPaused("0");paused=true;}
     if(!captured&&s.wallsBroken>0&&n==0){RigidFlightReview.Capture(g,"../evidence/hitstop/"+mode+"-wall.png",844,390);captured=true;}
    }
    var end=g.State();row.wallSeconds=wall;row.simulationSeconds=end.time;row.distance=end.distance;row.frozenSeconds=end.hitStopSeconds;row.requests=end.hitStopRequests;row.breaks=end.destructionEvent;row.finished=g.Finished&&end.physicsFault.Length==0;row.fullImpactMetadata=g.ReviewSimulation.Impacts.All(x=>x.normal.sqrMagnitude>.5f&&x.position.sqrMagnitude>.01f&&x.velocity.sqrMagnitude>.001f);
    g.Prepare("");row.resetClean=g.State().hitStopRemaining==0&&g.State().hitStopRequests==0&&g.State().vfxActive==0&&g.State().fragments==0;
    if(!row.finished||!row.resetClean||!row.pauseStable||!row.fullImpactMetadata||row.maxHold>.060001f||row.fragments>48||row.cityFragments>12)throw new Exception("playback gate");
    result.runs.Add(row);File.WriteAllText("../evidence/hitstop/result.json",JsonUtility.ToJson(result,true));
   }finally{g.ShutdownReviewPhysics();}
  }
  string path="../delivery/UnityWeb/impact-impact01";BuildFlight.Web(path);foreach(var name in new[]{"launch-input.js","sound-pack.js"})File.Copy("../web-template/"+name,path+"/"+name,true);Copy("../web-template/SoundPack",path+"/SoundPack");
 }
 public static void CaptureOnly(){
  Directory.CreateDirectory("../evidence/render-readback");var rows=new List<string>();
  foreach(string mode in new[]{"off","on","reduced"}){
   // The temporary Editor Scene destroys generated materials. Reset that review cache before loading another Scene.
   CrabModel.ReviewUseBaseline(false);var g=BuildFlight.PrepareScene();g.useHitStop=mode!="off";g.Initialize();g.SetReducedMotion(mode=="reduced"?"1":"0");
   try{
    g.BeginSpinCharge("");for(int i=0;i<601;i++){g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=2f/60,seconds=1f/60,direction=1}));g.Tick(1f/60);}g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,eligible=true}));
    for(int i=0;i<1200&&g.State().wallsBroken==0&&!g.Finished;i++)g.Tick(1f/60);
    var mats=g.ReviewCrab.Root.GetComponentsInChildren<MeshRenderer>().Select(x=>x.sharedMaterial).ToArray();bool valid=mats.All(m=>m&&m.shader==g.crabShader&&m.shader.isSupported);if(!valid)throw new Exception("Actor material lost after temporary Scene load");
    RigidFlightReview.Capture(g,"../evidence/render-readback/"+mode+"-wall.png",844,390);rows.Add(mode+":35 renderer materials valid, shader matches, walls="+g.State().wallsBroken+", activeVfx="+g.State().vfxActive);
   }finally{g.ShutdownReviewPhysics();}
  }
  File.WriteAllLines("../evidence/render-readback/result.txt",rows);CrabModel.ReviewUseBaseline(false);
 }
 static void Copy(string from,string to){Directory.CreateDirectory(to);foreach(var p in Directory.GetFiles(from))File.Copy(p,Path.Combine(to,Path.GetFileName(p)),true);foreach(var p in Directory.GetDirectories(from))Copy(p,Path.Combine(to,Path.GetFileName(p)));}
}

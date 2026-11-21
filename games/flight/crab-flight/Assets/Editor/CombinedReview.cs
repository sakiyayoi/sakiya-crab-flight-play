using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class CombinedReview {
 [Serializable] public class EventCheck {public int uniqueBreaks,sameStepBreaks,cityFragments,maxFragments,vfxPeak;public bool resetClean,distinctTokens,shaderSupported;public string[] ids;}
 public static void RunAndWeb(){
  SurfaceIntegrationReview.ModelOnly();CheckEvents();ChargeReview.Review();
  Web();
 }
 public static void FinalWeb(){CheckEvents();Web();}
 static void Web(){
  string attempt=Environment.GetEnvironmentVariable("CRAB_CHARGE_ATTEMPT")??"combined01",path="../delivery/UnityWeb/integrated-"+attempt;BuildFlight.Web(path);
  foreach(var name in new[]{"launch-input.js","sound-pack.js"})File.Copy("../web-template/"+name,path+"/"+name,true);Copy("../web-template/SoundPack",path+"/SoundPack");
 }
 static void CheckEvents(){
  Directory.CreateDirectory("../evidence/integration");var g=BuildFlight.PrepareScene();var result=new EventCheck();
  try{
   g.Initialize();g.BeginSpinCharge("");for(int i=0;i<601;i++){g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=2f/60,seconds=1f/60,direction=1}));g.Tick(1f/60);}
   g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,eligible=true}));g.ReviewSimulation.ReviewAddSimultaneousPanels();
   int maxFragments=0,cityFragments=0,vfxPeak=0;var breaks=new List<WorldBreakEvent>();bool shot=false;
   for(int i=0;i<2400&&!g.Finished;i++){g.Tick(1f/60);var s=g.State();maxFragments=Mathf.Max(maxFragments,s.fragments);cityFragments=Mathf.Max(cityFragments,s.cityFragments);vfxPeak=Mathf.Max(vfxPeak,s.vfxActive);if(!shot&&s.glassBroken>0){RigidFlightReview.Capture(g,"../evidence/integration/simultaneous.png",844,390);shot=true;}}
   breaks.AddRange(g.ReviewSimulation.BreakEvents);result.uniqueBreaks=breaks.Select(x=>x.id).Distinct().Count();result.sameStepBreaks=breaks.GroupBy(x=>x.time).Max(x=>x.Count());result.ids=breaks.Select(x=>x.id).ToArray();result.maxFragments=maxFragments;result.cityFragments=cityFragments;result.vfxPeak=vfxPeak;
   var events=g.ReviewEvents;result.distinctTokens=events.Select(x=>x.id).Distinct().Count()==events.Length;
   result.shaderSupported=g.GetComponentsInChildren<CrabDestructionVfx.CrabVfxRig>().All(x=>x.inkShader&&x.inkShader.isSupported);
   g.Prepare("");result.resetClean=g.State().fragments==0&&g.State().cityFragments==0&&g.State().vfxActive==0&&g.ReviewEvents.Length==1&&g.ReviewEvents[0].type=="reset";
   if(result.sameStepBreaks<2||result.maxFragments>48||result.cityFragments>12||result.vfxPeak>320||!result.resetClean||!result.distinctTokens||!result.shaderSupported)throw new Exception("Integrated event/pool/shader gate failed");
  }finally{File.WriteAllText("../evidence/integration/event-check.json",JsonUtility.ToJson(result,true));g.ShutdownReviewPhysics();}
 }
 static void Copy(string from,string to){Directory.CreateDirectory(to);foreach(var p in Directory.GetFiles(from))File.Copy(p,Path.Combine(to,Path.GetFileName(p)),true);foreach(var p in Directory.GetDirectories(from))Copy(p,Path.Combine(to,Path.GetFileName(p)));}
}

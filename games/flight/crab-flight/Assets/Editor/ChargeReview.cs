using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public static class ChargeReview {
 [Serializable] public class Run {public string label;public FlightState charge,result;public RigidImpact[] contacts;public string[] broken;public float bodyMass,maxSpeed,maxSpin;public int maxFragments,colliders;}
 [Serializable] public class Results {public string revision="crab-flight-charge1",scope="New integrated native game validation; observer60Hz / physics240Hz; not reused anatomy41. Browser gestures, actual listening and iPhone separate.";public bool noIdleEnergy,noAutomaticLaunch,earlyReleaseBlocked,frequencyStable,pauseStable,retryStarted,resetClean;public float frequencyDelta;public int configurations;public List<Run> runs=new List<Run>();}
 static Results results;static string dir;static string attempt;
 public static void RunAndWeb(){Review();string dest="../delivery/UnityWeb/charge1-ready-"+attempt;BuildFlight.Web(dest);File.Copy("../web-template/launch-input.js",dest+"/launch-input.js",true);}
 public static void Review(){
  attempt=Environment.GetEnvironmentVariable("CRAB_CHARGE_ATTEMPT")??"01";dir=Path.GetFullPath("../evidence/native"+attempt);Directory.CreateDirectory(dir);results=new Results();CrabFlightGame g=null;
  try{
   float[] energies=new float[3];int k=0;foreach(int hz in new[]{30,60,120}){var c=new SpinCharge();c.Begin();for(int i=0;i<hz*10;i++){c.Add(2f/hz,1f/hz);c.Tick(1f/hz);}energies[k++]=c.Energy;}results.frequencyDelta=Mathf.Max(energies)-Mathf.Min(energies);results.frequencyStable=results.frequencyDelta<.0001f;
   g=BuildFlight.PrepareScene();g.Initialize();g.BeginSpinCharge("");for(int i=0;i<601;i++)g.Tick(1f/60);var idle=g.State();results.noIdleEnergy=idle.chargeReady&&idle.chargeEnergy==0;results.noAutomaticLaunch=idle.attempts==0&&idle.phase=="ready";
   g.Prepare("");g.BeginSpinCharge("");g.BeginAim("");g.PreviewLaunch(Input(10,85,1));g.ReleaseLaunch(Input(10,85,1));results.earlyReleaseBlocked=g.State().attempts==0&&g.State().phase=="ready";
   var empty=Play(g,"idle-charge-then-fling",10,85,1,0);var slow=Play(g,"slow-work",10,85,1,.2f);var fast=Play(g,"fast-work",10,85,1,2);
   g.RetryLaunch("");results.retryStarted=g.State().phase=="flight"&&g.State().fragments==0;Finish(g,"one-tap-replay",fast.charge);
   Play(g,"fast-repeat",10,85,1,2);Play(g,"fast-left",10,85,-1,2);Play(g,"high-angle",55,85,1,2);Play(g,"low-power",10,35,1,2);Play(g,"maximum",10,100,1,2);
   g.Prepare("");g.BeginSpinCharge("");g.Tick(.1f);var paused=g.State();g.SetPaused("1");for(int i=0;i<120;i++)g.Tick(1f/60);results.pauseStable=g.State().chargeElapsed==paused.chargeElapsed;g.SetPaused("0");
   foreach(float c in new[]{0f,50f,100f})foreach(float s in new[]{0f,50f,100f})foreach(float l in new[]{0f,50f,100f}){g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{claw=c,shell=s,legs=l,angle=10,power=85}));Play(g,"parts-"+c+"-"+s+"-"+l,10,85,1,2,false);results.configurations++;}
   g.Prepare("");var reset=g.State();results.resetClean=reset.chargeElapsed==0&&reset.chargeEnergy==0&&!reset.chargeStarted&&reset.fragments==0&&reset.phase=="ready";
   Save();if(!results.noIdleEnergy||!results.noAutomaticLaunch||!results.earlyReleaseBlocked||!results.frequencyStable||!results.pauseStable||!results.resetClean||!results.retryStarted)throw new Exception("Charge state gate failed");
   if(fast.charge.chargeEnergy<=slow.charge.chargeEnergy+.3f)throw new Exception("Active input energy not distinguished");
  }finally{Save();if(g)g.ShutdownReviewPhysics();}
 }
 static string Input(float a,float p,int d)=>JsonUtility.ToJson(new FlightAimInput{angle=a,power=p,direction=d,spinRps=0,spinPhase=0,eligible=true});
 static Run Play(CrabFlightGame g,string label,float angle,float power,int direction,float work,bool defaults=true){
  g.SetPaused("0");g.Prepare("");if(defaults)g.Configure(JsonUtility.ToJson(new FlightSettings{angle=angle,power=power,claw=45,shell=45,legs=45}));g.BeginSpinCharge("");
  for(int i=0;i<601;i++){if(work>0)g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=work/60,seconds=1f/60,direction=direction}));g.Tick(1f/60);if(label=="fast-work"&&(i==0||i==300||i==600))Capture(g,"charge-"+i);}
  var charged=g.State();g.BeginAim("");g.ReleaseLaunch(Input(angle,power,direction));return Finish(g,label,charged);
 }
 static Run Finish(CrabFlightGame g,string label,FlightState charged){int fragments=0,glass=0,walls=0;bool outside=false;
  for(int i=0;i<9000&&!g.Finished;i++){g.Tick(1f/60);var s=g.State();fragments=Mathf.Max(fragments,s.fragments);if(label=="fast-work"){if(s.glassBroken>glass){Capture(g,"glass");glass=s.glassBroken;}if(s.wallsBroken>walls){Capture(g,"wall-"+s.wallsBroken);walls=s.wallsBroken;}if(!outside&&Mathf.Abs(s.worldX)>35){Capture(g,"outside");outside=true;}}}
  var sim=g.ReviewSimulation;if(sim==null)throw new Exception(label+" launch missing");var run=new Run{label=label,charge=charged,result=g.State(),contacts=sim.Impacts.ToArray(),broken=sim.BrokenObjects.ToArray(),bodyMass=sim.Body.mass,maxSpeed=sim.MaxSpeed,maxSpin=sim.MaxSpin,maxFragments=fragments,colliders=sim.ColliderCount};results.runs.Add(run);Save();if(label=="fast-work")Capture(g,"result");
  if(!g.Finished||run.result.physicsFault.Length>0||fragments>48||sim.ColliderCount!=13)throw new Exception(label+" fault "+run.result.physicsFault);return run;
 }
 static void Save(){if(results!=null&&dir!=null)File.WriteAllText(Path.Combine(dir,"result.json"),JsonUtility.ToJson(results,true));}
 static void Capture(CrabFlightGame g,string name){RigidFlightReview.Capture(g,Path.Combine(dir,name+".png"),844,390);}
}

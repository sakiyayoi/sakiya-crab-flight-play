using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class RepeatReview {
 [Serializable] public class Shape {public string name;public Vector3 position,scale,center,extent;public Quaternion rotation;}
 [Serializable] public class Initial {public FlightState charge;public Vector3 position,velocity,omega,com,inertia;public Quaternion rotation,inertiaRotation;public float mass;public Shape[] shapes;}
 [Serializable] public class Row {public string label;public Initial initial;public FlightState result;public RigidImpact[] contacts;public string[] broken;public Vector3[] firstSteps;}
 [Serializable] public class Report {public string purpose="Same input/initial-state comparison; render side effect, reset and solver divergence distinguished.";public List<Row> runs=new List<Row>();}
 static Report report;static string dir;
 public static void Run(){
  dir=Path.GetFullPath("../evidence/repeat-"+(Environment.GetEnvironmentVariable("CRAB_CHARGE_ATTEMPT")??"diagnose01"));Directory.CreateDirectory(dir);report=new Report();CrabFlightGame g=null;
  try {g=BuildFlight.PrepareScene();g.Initialize();Play(g,"warmup-idle",0,false);Play(g,"warmup-slow",.2f,false);Play(g,"fast-render",2,true);Replay(g);Play(g,"fast-no-render-a",2,false);Play(g,"fast-no-render-b",2,false);Play(g,"fast-render-b",2,true);}
  finally {Save();if(g)g.ShutdownReviewPhysics();}
 }
 static Row Play(CrabFlightGame g,string label,float work,bool render){
  g.SetPaused("0");g.Prepare("");g.Configure(JsonUtility.ToJson(new FlightSettings{angle=10,power=85,claw=45,shell=45,legs=45}));g.BeginSpinCharge("");
  for(int i=0;i<601;i++){if(work>0)g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=work/60,seconds=1f/60,direction=1}));g.Tick(1f/60);if(render&&(i==0||i==300||i==600))RigidFlightReview.Capture(g,Path.Combine(dir,label+"-charge-"+i+".png"),844,390);}
  var charged=g.State();g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,spinRps=0,spinPhase=0,eligible=true}));return Finish(g,label,charged,render);
 }
 static void Replay(CrabFlightGame g){var charge=report.runs[2].initial.charge;g.RetryLaunch("");Finish(g,"same-throw-replay",charge,false);}
 static Row Finish(CrabFlightGame g,string label,FlightState charged,bool render){
  var sim=g.ReviewSimulation;var b=sim.Body;var row=new Row{label=label,initial=new Initial{charge=charged,position=b.position,rotation=b.rotation,velocity=b.linearVelocity,omega=b.angularVelocity,com=b.centerOfMass,inertia=b.inertiaTensor,inertiaRotation=b.inertiaTensorRotation,mass=b.mass,shapes=b.GetComponentsInChildren<Collider>().Select(c=>new Shape{name=c.name,position=c.transform.localPosition,rotation=c.transform.localRotation,scale=c.transform.lossyScale,center=c.bounds.center,extent=c.bounds.extents}).ToArray()}};
  var steps=new List<Vector3>();int glass=0,walls=0;bool outside=false;
  for(int i=0;i<9000&&!g.Finished;i++){g.Tick(1f/240);if(i<32)steps.Add(b.position);var s=g.State();if(render){if(s.glassBroken>glass){RigidFlightReview.Capture(g,Path.Combine(dir,label+"-glass.png"),844,390);glass=s.glassBroken;}if(s.wallsBroken>walls){RigidFlightReview.Capture(g,Path.Combine(dir,label+"-wall"+s.wallsBroken+".png"),844,390);walls=s.wallsBroken;}if(!outside&&Mathf.Abs(s.worldX)>35){RigidFlightReview.Capture(g,Path.Combine(dir,label+"-outside.png"),844,390);outside=true;}}}
  row.result=g.State();row.firstSteps=steps.ToArray();row.contacts=sim.Impacts.ToArray();row.broken=sim.BrokenObjects.ToArray();report.runs.Add(row);Save();if(!g.Finished||row.result.physicsFault.Length>0)throw new Exception(label+" failed");return row;
 }
 static void Save(){if(report!=null&&dir!=null)File.WriteAllText(Path.Combine(dir,"result.json"),JsonUtility.ToJson(report,true));}
}

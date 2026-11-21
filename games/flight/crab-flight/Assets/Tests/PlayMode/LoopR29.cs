#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.IO;using System.Linq;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
public sealed partial class FlightMeasurementPlayMode {
 [Serializable]public class R29Probe{public int gear,activeColliders,authoredTargets;public float mass,minX=1,maxX,minY=1,maxY;public string launch,final;public List<string> actualBreakIDs=new List<string>();}
 [Serializable]public class R29Report{public string state="RUNNING",scope="actual production setup and fixed Tick; fixture is not a browser swipe or natural full-route reach";public List<R29Probe> rows=new List<R29Probe>();}
 [UnityTest,Timeout(300000)]public IEnumerator R29GearCameraAndContacts(){
  var report=new R29Report();Directory.CreateDirectory(Evidence);var output=Path.Combine(Evidence,"r29-integration.json");Assert.That(File.Exists(output),Is.False);
  float[,] shape={{45,45,45},{68,42,34},{36,60,38},{38,40,66}};
  for(int gear=0;gear<4;gear++){
   yield return NewRetrySubject(false,true,true,true,true,true,true);
   Call(current,"Configure","{\"angle\":40,\"power\":70,\"claw\":"+shape[gear,0]+",\"shell\":"+shape[gear,1]+",\"legs\":"+shape[gear,2]+",\"upgradeLevel\":3,\"gear\":"+gear+"}");
   var model=Property(current,"ReviewCrab");var root=(Transform)Field(model,"Root");
   var row=new R29Probe{gear=gear};report.rows.Add(row);
   var world=Field(current,"world");foreach(var p in (IEnumerable)Field(world,"panels"))if(((string)Field(p,"id")).StartsWith("r29-"))row.authoredTargets++;
   Assert.That(row.authoredTargets,Is.EqualTo(38));
   Call(current,"BeginSpinCharge","");for(int i=0;i<601;i++){Call(current,"AddSpinWork","{\"distance\":0.033333334,\"seconds\":0.016666668,\"direction\":1}");Call(current,"Tick",1f/60);}
   Call(current,"BeginAim","");Call(current,"ReleaseLaunch","{\"angle\":40,\"power\":70,\"direction\":1,\"eligible\":true}");
   var sim=Property(current,"ReviewSimulation");var body=(Rigidbody)Property(sim,"Body");row.mass=body.mass;row.launch=JsonUtility.ToJson(Field(State(current),"launchReceipt"));row.activeColliders=root.GetComponentsInChildren<Collider>().Count(c=>c.enabled);Assert.That(row.activeColliders,Is.EqualTo(13));
   var camera=(Camera)Property(current,"ReviewCamera");camera.aspect=1.6f;
   for(int i=0;i<900&&!(bool)Property(current,"Finished");i++){
    Call(current,"Tick",1f/60);var p=camera.WorldToViewportPoint(root.position);row.minX=Mathf.Min(row.minX,p.x);row.maxX=Mathf.Max(row.maxX,p.x);row.minY=Mathf.Min(row.minY,p.y);row.maxY=Mathf.Max(row.maxY,p.y);
   }
   row.final=JsonUtility.ToJson(State(current));foreach(var e in (IEnumerable)Field(sim,"BreakEvents"))row.actualBreakIDs.Add((string)Field(e,"id"));
   File.WriteAllText(output,JsonUtility.ToJson(report,true));Assert.That((string)Field(State(current),"physicsFault"),Is.Empty);Assert.That(row.minX,Is.GreaterThan(.02));Assert.That(row.maxX,Is.LessThan(.98));Assert.That(row.minY,Is.GreaterThan(.02));Assert.That(row.maxY,Is.LessThan(.98));
   CaptureRoute("r29-gear-"+gear,"production-camera");
   // Actual direction reversal/translation fixture: no claim of natural reach.
   if(!(bool)Property(current,"Finished")){body.position+=Vector3.left*90;body.linearVelocity=Vector3.left*60;Call(current,"Tick",1f/60);var p=camera.WorldToViewportPoint(root.position);Assert.That(p.x,Is.InRange(.02f,.98f),"whole-camera translation lag returned");}
   Call(current,"SetPaused","1");var pos=camera.transform.position;Call(current,"Tick",1f/60);Assert.That(camera.transform.position,Is.EqualTo(pos));Call(current,"SetPaused","0");Call(current,"Prepare","");Assert.That(F(State(current),"distance"),Is.Zero);Assert.That((bool)Field(State(current),"chargeStarted"),Is.False);
   Call(current,"ShutdownReviewPhysics");current=null;yield return null;
  }
  Assert.That(report.rows[2].mass,Is.GreaterThan(report.rows[0].mass));report.state="NATIVE_GEAR_CAMERA_CONTACT_CONTRACT_PASSED_NOT_ALL_M_GATES";File.WriteAllText(output,JsonUtility.ToJson(report,true));
 }
}
#endif

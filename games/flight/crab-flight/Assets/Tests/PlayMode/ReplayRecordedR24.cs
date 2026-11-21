#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.IO;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
public sealed partial class FlightMeasurementPlayMode {
 [Serializable]class ReplayR24Case{public string id,launchBody,joints,referenceFirstContact;public float angle,power,spin,charge,phase,referenceDistance,referenceMin;public int direction;public float[] dts;}
 [Serializable]class ReplayR24Inputs{public ReplayR24Case[] cases;}
 [Serializable]class ReplayR24Result{public string id,referenceBody,actualBody,firstContact,final,trace,contactTrace,floor;public bool finished,initialBodyExact,initialJointsExact;public int frames;public float referenceDistance,referenceMin;}
 [UnityTest,Timeout(600000)]public IEnumerator R24RecordedCausality(){
  var input=JsonUtility.FromJson<ReplayR24Inputs>(Resources.Load<TextAsset>("ReplayInputR24").text);Directory.CreateDirectory(Evidence);
  foreach(var c in input.cases){
   yield return NewRetrySubject(false,true,true,true,true,true,true);
   Call(current,"Configure",JsonUtility.ToJson(new InputSpec{angle=c.angle,power=c.power,direction=c.direction}));var crab=Property(current,"ReviewCrab");Call(crab,"ChargePose",c.charge);
   foreach(var p in new[]{new object[]{"direction",c.direction},new object[]{"launchSpinRps",c.spin},new object[]{"launchChargeEnergy",c.charge},new object[]{"spinAngle",c.phase},new object[]{"displaySpinAngle",c.phase}})gameType.GetField((string)p[0],System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).SetValue(current,p[1]);
   Call(current,"Launch","");var launch=Field(State(current),"launchReceipt");var sim=Property(current,"ReviewSimulation");sim.GetType().GetField("ReviewObserveR24").SetValue(sim,true);
   var r=new ReplayR24Result{id=c.id,referenceBody=c.launchBody,actualBody=(string)Field(launch,"bodyJson"),referenceDistance=c.referenceDistance,referenceMin=c.referenceMin,initialJointsExact=(string)Field(launch,"jointSHA256")==c.joints};r.initialBodyExact=r.actualBody==c.launchBody;
   foreach(float dt in c.dts){if((bool)Property(current,"Finished"))break;Call(current,"Tick",dt);r.frames++;}
   r.finished=(bool)Property(current,"Finished");r.final=JsonUtility.ToJson(State(current));r.firstContact=(string)Field(sim,"FirstContactProbe");r.trace=(string)Call(sim,"ReviewTraceR24");r.contactTrace=(string)Call(sim,"ReviewContactTraceJson");r.floor=(string)Call(sim,"FloorDiagnosticsJSON");File.WriteAllText(Path.Combine(Evidence,"replay-"+c.id+".json"),JsonUtility.ToJson(r,true));
   Call(current,"ShutdownReviewPhysics");current=null;yield return null;yield return null;
  }
  Assert.That(input.cases.Length,Is.EqualTo(3));
 }
}
#endif

#if UNITY_EDITOR
using System;using System.IO;using System.Collections;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
public sealed partial class FlightMeasurementPlayMode {
 [Serializable]class R30FloorCase{public string id,launch;public float referenceMin,referenceDistance;public float[] dts;public int chargeTicksSkipped;}
 [Serializable]class R30SavedLaunch{public string bodyJson,jointSHA256;}
 [Serializable]class R30FloorInputs{public R30FloorCase[] cases;}
 [Serializable]class R30FloorResult{public string id,method="saved accepted launch + recorded flightTick window; Native rendering clock/platform may differ from WebGL; not a browser input replay guarantee",initial,final,floor,trace,motion;public float referenceMin,referenceDistance;public bool initialBodyExact,initialJointsExact;}
 [UnityTest,Timeout(240000)]public IEnumerator R30FloorCause(){
  var input=JsonUtility.FromJson<R30FloorInputs>(File.ReadAllText(Path.Combine(Application.dataPath,"Editor/BaselineData/ReplayR30.json")));Directory.CreateDirectory(Evidence);
  foreach(var c in input.cases){
   yield return NewRetrySubject(false,true,true,true,true,true,true);
   Call(current,"LoadFeelR27",File.ReadAllText(Path.Combine(Application.dataPath,"StreamingAssets/feel.json")));Call(current,"SetFeelR27","{\"preset\":\"mashimashi\",\"shake\":true,\"reducedMotion\":false}");
   Call(current,"ReviewReplayAccepted",c.launch);var sim=Property(current,"ReviewSimulation");sim.GetType().GetField("ReviewObserveR24").SetValue(sim,true);var saved=JsonUtility.FromJson<R30SavedLaunch>(c.launch);var initial=Field(State(current),"launchReceipt");
   var row=new R30FloorResult{id=c.id,referenceMin=c.referenceMin,referenceDistance=c.referenceDistance,initial=JsonUtility.ToJson(initial),initialBodyExact=(string)Field(initial,"bodyJson")==saved.bodyJson,initialJointsExact=(string)Field(initial,"jointSHA256")==saved.jointSHA256};
   foreach(var dt in c.dts){if((bool)Property(current,"Finished"))break;Call(current,"Tick",dt);}
   row.final=JsonUtility.ToJson(State(current));row.floor=(string)Call(sim,"FloorDiagnosticsJSON");row.trace=(string)Call(sim,"ReviewTraceR24");row.motion=(string)Call(sim,"ReviewMotionR22JSON");File.WriteAllText(Path.Combine(Evidence,"floor-cause-"+c.id+".json"),JsonUtility.ToJson(row,true));
   Assert.That((string)Field(State(current),"physicsFault"),Is.Empty);Call(current,"ShutdownReviewPhysics");current=null;yield return null;
  }
 }
}
#endif

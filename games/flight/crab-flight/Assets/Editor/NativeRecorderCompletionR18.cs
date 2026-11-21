using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
public static class NativeRecorderCompletionR18 {
 [Serializable] class Sample{public string name,unit;public long before,afterA,idle,afterB;}
 [Serializable] class Result{public string error;public bool idleStable,monotonic,restored;public int frameBegin,frameEnd;public Sample[] values;}
 static void Idle(){var w=System.Diagnostics.Stopwatch.StartNew();while(w.Elapsed.TotalMilliseconds<2){} }
 public static void Run(){
  CurrentValueValidationR18.Run();var dir=Path.GetFullPath("../../../../evidence/currentvalue-validation02");var r=new Result{frameBegin=Time.renderedFrameCount};
  bool original=Profiler.enabled,binary=Profiler.enableBinaryLog;string logfile=Profiler.logFile;var mode=Physics.simulationMode;
  var recs=new List<ProfilerRecorder>();var rows=new List<Sample>();GameObject body=null,floor=null;
  try{
   Profiler.logFile=Path.Combine(dir,"native-completion.raw");Profiler.enableBinaryLog=true;Profiler.enabled=true;
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Directory.CreateDirectory("Assets/DiagnosticsR18");EditorSceneManager.SaveScene(scene,"Assets/DiagnosticsR18/CurrentValueOnly.unity");
   Physics.simulationMode=SimulationMode.Script;floor=new GameObject("Synthetic floor");floor.transform.position=Vector3.down*.5f;floor.AddComponent<BoxCollider>().size=new Vector3(10,1,10);body=new GameObject("Synthetic completion control");body.transform.position=Vector3.up*.5f;body.AddComponent<BoxCollider>();body.AddComponent<Rigidbody>().linearVelocity=Vector3.down;
   Physics.Simulate(1f/15360f);var hs=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(hs);
   foreach(string name in new[]{"PxScene.simulate","ScScene.broadPhase","ScScene.rigidBodyNarrowPhase","ScScene.rigidBodySolver","ScScene.updateDynamics","Physics.ReadTransforms","Physics.SyncTransforms","Physics.FetchPhysXResults","Physics.PublishResultsStage"}){
    bool found=false;foreach(var h in hs){var d=ProfilerRecorderHandle.GetDescription(h);if(d.Name!=name||d.Category.Name!="Physics")continue;var rec=new ProfilerRecorder(h,1,ProfilerRecorderOptions.StartImmediately|ProfilerRecorderOptions.SumAllSamplesInFrame|ProfilerRecorderOptions.WrapAroundWhenCapacityReached);if(!rec.Valid)throw new Exception("Invalid native recorder");recs.Add(rec);rows.Add(new Sample{name=name,unit=rec.UnitType.ToString(),before=rec.CurrentValue});found=true;break;}if(!found)throw new Exception("Required marker unavailable: "+name);
   }
   Physics.Simulate(1f/15360f);for(int i=0;i<recs.Count;i++)rows[i].afterA=recs[i].CurrentValue;
   Idle();for(int i=0;i<recs.Count;i++)rows[i].idle=recs[i].CurrentValue;
   Physics.Simulate(1f/15360f);for(int i=0;i<recs.Count;i++)rows[i].afterB=recs[i].CurrentValue;
   r.idleStable=true;r.monotonic=true;foreach(var x in rows){r.idleStable&=x.afterA==x.idle;r.monotonic&=x.afterA>=x.before&&x.afterB>=x.idle&&x.unit=="TimeNanoseconds";}
   r.frameEnd=Time.renderedFrameCount;if(!r.idleStable||!r.monotonic||r.frameBegin!=r.frameEnd)throw new Exception("Native completion/quiescence not established");
  }catch(Exception e){r.error=e.ToString();throw;}
  finally{r.values=rows.ToArray();foreach(var rec in recs)rec.Dispose();if(body)UnityEngine.Object.DestroyImmediate(body);if(floor)UnityEngine.Object.DestroyImmediate(floor);Physics.simulationMode=mode;Profiler.enabled=original;Profiler.enableBinaryLog=binary;Profiler.logFile=logfile;r.restored=Physics.simulationMode==mode&&Profiler.enabled==original&&Profiler.enableBinaryLog==binary&&Profiler.logFile==logfile;File.WriteAllText(Path.Combine(dir,"native-receipt.json"),JsonUtility.ToJson(r,true));}
 }
}

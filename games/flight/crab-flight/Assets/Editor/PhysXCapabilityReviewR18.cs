using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.SceneManagement;

// Editor-only availability/positive-control probe. Never changes game settings/assets.
public static class PhysXCapabilityReviewR18 {
 [Serializable] public class Metric { public string name,category,unit,flags; public bool valid,running,wrapped; public int samples; public long before,after,sampleTotal,sampleCalls; }
 [Serializable] public class Receipt { public string scope="API capability and synthetic control only; not game performance or WebGL evidence",unity,platform,error;public bool development,profilerBefore,profilerAfter,simulationModeRestored; public int availableBefore,availableAfter,steps;public double simulateWallMs;public List<Metric> inventory=new List<Metric>(),recorded=new List<Metric>();public Metric positive; }
 static bool Relevant(string n){return new[]{"physics","pxs","scscene","broadphase","narrowphase","solver","gc.alloc","garbagecollect","gc used","gc reserved","sync transforms"}.Any(x=>n.IndexOf(x,StringComparison.OrdinalIgnoreCase)>=0);}
 static string output="editor-capability02";
 public static void RunBinary(){
  var dir=Path.GetFullPath("../../../../evidence/editor-capability03-binary");Directory.CreateDirectory(dir);
  bool enabled=Profiler.enabled,binary=Profiler.enableBinaryLog;string file=Profiler.logFile;
  var oldMax=Profiler.maxUsedMemory;output="editor-capability03-binary";
  try{Profiler.logFile=Path.Combine(dir,"synthetic64.raw");Profiler.enableBinaryLog=true;Profiler.maxUsedMemory=16*1024*1024;Profiler.enabled=true;Run();}
  finally{Profiler.enabled=enabled;Profiler.enableBinaryLog=binary;Profiler.logFile=file;Profiler.maxUsedMemory=oldMax;}
 }
 public static void Run(){
  var dir=Path.GetFullPath("../../../../evidence/"+output);Directory.CreateDirectory(dir);
  var r=new Receipt{unity=Application.unityVersion,platform=Application.platform.ToString(),development=Debug.isDebugBuild,profilerBefore=Profiler.enabled};
  var mode=Physics.simulationMode;var recs=new List<ProfilerRecorder>();var rows=new List<Metric>();GameObject floor=null,body=null;
  try{
   var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);r.availableBefore=handles.Count;
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
   Directory.CreateDirectory("Assets/DiagnosticsR18");EditorSceneManager.SaveScene(scene,"Assets/DiagnosticsR18/CapabilityOnly.unity");
   Physics.simulationMode=SimulationMode.Script;
   floor=new GameObject("Synthetic floor - not product");floor.transform.position=new Vector3(0,-.5f,0);floor.AddComponent<BoxCollider>().size=new Vector3(10,1,10);
   body=new GameObject("Synthetic positive control - not saved game state");body.transform.position=new Vector3(0,.5f,0);body.AddComponent<BoxCollider>();var rb=body.AddComponent<Rigidbody>();rb.linearVelocity=Vector3.down;
   Physics.Simulate(1f/15360f); // register lazy native marker names; not part of measured control.
   handles.Clear();ProfilerRecorderHandle.GetAvailable(handles);r.availableAfter=handles.Count;
   foreach(var h in handles){var d=ProfilerRecorderHandle.GetDescription(h);if(!Relevant(d.Name))continue;var row=new Metric{name=d.Name,category=d.Category.Name,unit=d.UnitType.ToString(),flags=d.Flags.ToString()};r.inventory.Add(row);
    if(d.UnitType!=ProfilerMarkerDataUnit.TimeNanoseconds)continue;
    var rec=new ProfilerRecorder(h,4096,ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.SumAllSamplesInFrame);row.valid=rec.Valid;row.running=rec.IsRunning;row.before=rec.Valid?rec.CurrentValue:0;recs.Add(rec);rows.Add(row);
   }
   var marker=new ProfilerMarker(ProfilerCategory.Scripts,"R18.Capability.PositiveControl");
   using(var positive=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"R18.Capability.PositiveControl",8,ProfilerRecorderOptions.StartImmediately | ProfilerRecorderOptions.SumAllSamplesInFrame)){
    using(marker.Auto()){var w=System.Diagnostics.Stopwatch.StartNew();while(w.Elapsed.TotalMilliseconds<1){} }
    r.positive=new Metric{name="R18.Capability.PositiveControl",valid=positive.Valid,running=positive.IsRunning,after=positive.CurrentValue,samples=positive.Count};
   }
   var sw=System.Diagnostics.Stopwatch.StartNew();for(int i=0;i<64;i++){Physics.Simulate(1f/15360f);r.steps++;}sw.Stop();r.simulateWallMs=sw.Elapsed.TotalMilliseconds;
   for(int i=0;i<recs.Count;i++){var rec=recs[i];var row=rows[i];row.after=rec.CurrentValue;row.samples=rec.Count;row.wrapped=rec.WrappedAround;for(int j=0;j<rec.Count;j++){var s=rec.GetSample(j);row.sampleTotal+=s.Value;row.sampleCalls+=s.Count;}r.recorded.Add(row);}
  }catch(Exception ex){r.error=ex.ToString();throw;}
  finally{foreach(var rec in recs)rec.Dispose();if(body)UnityEngine.Object.DestroyImmediate(body);if(floor)UnityEngine.Object.DestroyImmediate(floor);Physics.simulationMode=mode;r.simulationModeRestored=Physics.simulationMode==mode;r.profilerAfter=Profiler.enabled;File.WriteAllText(Path.Combine(dir,"receipt.json"),JsonUtility.ToJson(r,true));}
 }
}

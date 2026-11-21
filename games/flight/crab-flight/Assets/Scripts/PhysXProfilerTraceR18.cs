using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;

// Opt-in diagnostics: CurrentValue at completed OUTER scope boundaries only.
public static class PhysXProfilerTraceR18 {
 const int Capacity=8192,MaxMetrics=16;
 static readonly string[] Selected={"PxScene.simulate","ScScene.broadPhase","ScScene.rigidBodyNarrowPhase","ScScene.rigidBodySolver","ScScene.updateDynamics","Physics.ReadTransforms","Physics.SyncTransforms","Physics.FetchPhysXResults","Physics.PublishResultsStage"};
 [Serializable] public class Metric{public string name,category,unit,flags;}
 [Serializable] public class Row{public int frame,mainScopes,debrisScopes,mainScene,debrisScene;public float sim;public long probeTicks;public long[] values,main,debris,outside,before,after;}
 [Serializable] public class Report{public string scope="Inclusive native markers assigned to synchronous outer call intervals; outside residual retained, not exclusive task costs",error;public bool enabled,development,profilerBefore,profilerRestored;public int installedFrame,available,metrics,dropped,capacity=Capacity,scopeErrors;public long frequency=System.Diagnostics.Stopwatch.Frequency;public Metric[] names;public Row[] rows;}
 static bool enabled,original,installed,restored,inFrame;static string error="";static int available,installedFrame,dropped,count,currentFrame,openScope,scopeErrors,mainCount,debrisCount,mainScene,debrisScene;
 static readonly List<ProfilerRecorder> recs=new List<ProfilerRecorder>(MaxMetrics);static readonly List<Metric> names=new List<Metric>(MaxMetrics);
 static long[] before,scopeBefore,mainSum,debrisSum,values,mains,debris,beforeStore,afterStore,probeTimes;static int[] frameIds,mainCounts,debrisCounts,mainScenes,debrisScenes;static float[] simTimes;static long probe;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Init(){
  var u=Application.absoluteURL;enabled=Application.isPlaying&&(u.StartsWith("http://127.0.0.1:",StringComparison.Ordinal)||u.StartsWith("http://localhost:",StringComparison.Ordinal))&&u.IndexOf("physxTrace=1",StringComparison.Ordinal)>=0;
  if(!enabled)return;original=Profiler.enabled;Profiler.enabled=true;
  before=new long[MaxMetrics];scopeBefore=new long[MaxMetrics];mainSum=new long[MaxMetrics];debrisSum=new long[MaxMetrics];
  values=new long[Capacity*MaxMetrics];mains=new long[Capacity*MaxMetrics];debris=new long[Capacity*MaxMetrics];beforeStore=new long[Capacity*MaxMetrics];afterStore=new long[Capacity*MaxMetrics];
  frameIds=new int[Capacity];mainCounts=new int[Capacity];debrisCounts=new int[Capacity];mainScenes=new int[Capacity];debrisScenes=new int[Capacity];simTimes=new float[Capacity];probeTimes=new long[Capacity];
 }
 static void Install(int frame){
  installed=true;installedFrame=frame;
  try{var handles=new List<ProfilerRecorderHandle>();ProfilerRecorderHandle.GetAvailable(handles);available=handles.Count;
   foreach(string name in Selected)foreach(var h in handles){var d=ProfilerRecorderHandle.GetDescription(h);if(d.Name!=name||d.Category.Name!="Physics"||d.UnitType!=ProfilerMarkerDataUnit.TimeNanoseconds)continue;
    var r=new ProfilerRecorder(h,1,ProfilerRecorderOptions.StartImmediately|ProfilerRecorderOptions.SumAllSamplesInFrame|ProfilerRecorderOptions.WrapAroundWhenCapacityReached);
    if(!r.Valid){r.Dispose();continue;}recs.Add(r);names.Add(new Metric{name=d.Name,category=d.Category.Name,unit=d.UnitType.ToString(),flags=d.Flags.ToString()});break;
   }
   if(recs.Count!=Selected.Length)error="Required native markers unavailable";
  }catch(Exception ex){error=ex.ToString();}
 }
 public static void Begin(int frame){
  if(!enabled||!installed)return;
  long t=System.Diagnostics.Stopwatch.GetTimestamp();inFrame=true;currentFrame=frame;openScope=mainCount=debrisCount=mainScene=debrisScene=0;
  for(int i=0;i<recs.Count;i++){before[i]=recs[i].CurrentValue;mainSum[i]=debrisSum[i]=0;}probe=System.Diagnostics.Stopwatch.GetTimestamp()-t;
 }
 public static void BeginScene(int kind,int scene){
  if(!enabled||!installed||!inFrame)return;
  long t=System.Diagnostics.Stopwatch.GetTimestamp();if(openScope!=0||Time.renderedFrameCount!=currentFrame){scopeErrors++;return;}openScope=kind;
  if(kind==1){mainCount++;if(mainScene!=0&&mainScene!=scene)scopeErrors++;mainScene=scene;}else{debrisCount++;if(debrisScene!=0&&debrisScene!=scene)scopeErrors++;debrisScene=scene;}
  for(int i=0;i<recs.Count;i++)scopeBefore[i]=recs[i].CurrentValue;probe+=System.Diagnostics.Stopwatch.GetTimestamp()-t;
 }
 public static void EndScene(int kind){
  if(!enabled||!installed||!inFrame)return;
  long t=System.Diagnostics.Stopwatch.GetTimestamp();if(openScope!=kind||Time.renderedFrameCount!=currentFrame){scopeErrors++;return;}
  var sum=kind==1?mainSum:debrisSum;for(int i=0;i<recs.Count;i++)sum[i]+=recs[i].CurrentValue-scopeBefore[i];openScope=0;probe+=System.Diagnostics.Stopwatch.GetTimestamp()-t;
 }
 public static void End(int frame,float sim){
  if(!enabled)return;if(!installed){Install(frame);return;}
  if(openScope!=0||frame!=currentFrame)scopeErrors++;inFrame=false;if(count==Capacity){dropped++;return;}
  long t=System.Diagnostics.Stopwatch.GetTimestamp();int k=count*MaxMetrics;
  for(int i=0;i<recs.Count;i++){long after=recs[i].CurrentValue;values[k+i]=after-before[i];beforeStore[k+i]=before[i];afterStore[k+i]=after;mains[k+i]=mainSum[i];debris[k+i]=debrisSum[i];}
  frameIds[count]=frame;simTimes[count]=sim;mainCounts[count]=mainCount;debrisCounts[count]=debrisCount;mainScenes[count]=mainScene;debrisScenes[count]=debrisScene;probeTimes[count]=probe+System.Diagnostics.Stopwatch.GetTimestamp()-t;count++;
 }
 static long[] Copy(long[] source,int row){var a=new long[names.Count];Array.Copy(source,row*MaxMetrics,a,0,a.Length);return a;}
 public static string JSON(){
  var output=new Row[count];for(int j=0;j<count;j++){var whole=Copy(values,j);var m=Copy(mains,j);var d=Copy(debris,j);var residual=new long[names.Count];for(int i=0;i<residual.Length;i++)residual[i]=whole[i]-m[i]-d[i];output[j]=new Row{frame=frameIds[j],sim=simTimes[j],mainScopes=mainCounts[j],debrisScopes=debrisCounts[j],mainScene=mainScenes[j],debrisScene=debrisScenes[j],values=whole,main=m,debris=d,outside=residual,before=Copy(beforeStore,j),after=Copy(afterStore,j),probeTicks=probeTimes[j]};}
  foreach(var r in recs)r.Dispose();recs.Clear();if(enabled){Profiler.enabled=original;restored=Profiler.enabled==original;}
  return JsonUtility.ToJson(new Report{enabled=enabled,development=Debug.isDebugBuild,profilerBefore=original,profilerRestored=restored,installedFrame=installedFrame,available=available,metrics=names.Count,names=names.ToArray(),rows=output,dropped=dropped,error=error,scopeErrors=scopeErrors});
 }
}

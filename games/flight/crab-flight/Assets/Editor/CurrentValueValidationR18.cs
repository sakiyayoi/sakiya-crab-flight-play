using System;
using System.IO;
using UnityEngine;
using Unity.Profiling;
public static class CurrentValueValidationR18 {
 [Serializable] class Result {public string error,unit,options;public int frameStart,frameEnd,renderStart,renderEnd;public long before,insideA,afterA,gap,insideB,afterB,afterStop,lastBefore;public double wallAms,wallBms;public bool sameFrame,completedOnly,additive,stoppedZero;}
 static void Work(double ms){var w=System.Diagnostics.Stopwatch.StartNew();while(w.Elapsed.TotalMilliseconds<ms){} }
 public static void Run(){
  var dir=Path.GetFullPath("../../../../evidence/currentvalue-validation02");Directory.CreateDirectory(dir);var r=new Result{frameStart=Time.frameCount,renderStart=Time.renderedFrameCount};
  var marker=new ProfilerMarker(ProfilerCategory.Scripts,"R18.CurrentValue.SequentialControl");var options=ProfilerRecorderOptions.StartImmediately|ProfilerRecorderOptions.SumAllSamplesInFrame|ProfilerRecorderOptions.WrapAroundWhenCapacityReached;r.options=options.ToString();
  using(var rec=ProfilerRecorder.StartNew(ProfilerCategory.Scripts,"R18.CurrentValue.SequentialControl",1,options))try{
   if(!rec.Valid)throw new Exception("Control recorder invalid");r.unit=rec.UnitType.ToString();r.before=rec.CurrentValue;r.lastBefore=rec.LastValue;
   var w=System.Diagnostics.Stopwatch.StartNew();using(marker.Auto()){Work(1);r.insideA=rec.CurrentValue;}w.Stop();r.wallAms=w.Elapsed.TotalMilliseconds;r.afterA=rec.CurrentValue;r.gap=rec.CurrentValue;
   w.Restart();using(marker.Auto()){Work(2);r.insideB=rec.CurrentValue;}w.Stop();r.wallBms=w.Elapsed.TotalMilliseconds;r.afterB=rec.CurrentValue;
   r.frameEnd=Time.frameCount;r.renderEnd=Time.renderedFrameCount;r.sameFrame=r.frameStart==r.frameEnd&&r.renderStart==r.renderEnd;r.completedOnly=r.insideA==r.before&&r.insideB==r.afterA;r.additive=r.afterA>r.before&&r.afterB>r.afterA&&r.gap==r.afterA;
   rec.Stop();r.afterStop=rec.CurrentValue;r.stoppedZero=r.afterStop==0;
   if(r.unit!="TimeNanoseconds"||!r.sameFrame||!r.additive||!r.stoppedZero)throw new Exception("CurrentValue boundary semantics not demonstrated");
  }catch(Exception e){r.error=e.ToString();throw;}finally{File.WriteAllText(Path.Combine(dir,"receipt.json"),JsonUtility.ToJson(r,true));}
 }
}

using System;using System.Collections.Generic;using UnityEngine;
// Retains every work unit inside Retry→ready. Never simulates during construction.
public static class PrepareBoundaryR18 {public static readonly object Value=new object();}
public sealed partial class CrabFlightGame {
 IEnumerator<object> prepareWorkR18;FlightState prepareViewR18;FlightInputR23 prepareInputR18;FlightInputAckR23 prepareAckR18;Action prepareLaunchR18;
 readonly Queue<FlightInputR23> prepareQueuedR18=new Queue<FlightInputR23>();bool? preparePauseR18;long prepareStartedR18;int prepareFramesR18;double prepareCpuR18,prepareWallR18,prepareMaxChunkR18;bool prepareDiagnosticTimingR18;long prepareLastWorkEndR18;double prepareFirstWorkDelayR18,prepareMaxWaitR18;
 public const double PrepareBudgetMsR18=3;
 bool BeginPrepareFrameR18(FlightInputR23 c,FlightInputAckR23 a,bool repeat){
 int traceBeginR18=PrepareEnvelopeR18.EnterReceipt(PrepareEnvelopeR18.BeginPrepare,c!=null?c.id:0,Time.renderedFrameCount);try{
  if(useNewLoop||simulation==null)return false;PrepareEnvelopeR18.Preparing(c!=null?c.id:0);
  prepareViewR18=State();prepareViewR18.phase="preparing";prepareViewR18.stage="PREPARING";prepareViewR18.message="次の一投を準備しています…";prepareViewR18.events=Array.Empty<FlightGameEvent>();prepareViewR18.resultOverview=null;prepareViewR18.chargeStarted=prepareViewR18.chargeReady=false;prepareViewR18.spinRps=0;
  prepareInputR18=c;prepareAckR18=a;preparePauseR18=null;prepareLaunchR18=null;prepareStartedR18=System.Diagnostics.Stopwatch.GetTimestamp();prepareCpuR18=prepareWallR18=prepareMaxChunkR18=0;prepareFramesR18=0;prepareDiagnosticTimingR18=PrepareCostProbeR15.Enabled;prepareFirstWorkDelayR18=-1;prepareMaxWaitR18=0;prepareLastWorkEndR18=0;
  if(repeat){float phase0=spinAngle,spin0=launchSpinRps,charge0=launchChargeEnergy,target0=launchTargetPower;prepareLaunchR18=()=>{launchTargetPower=target0;launchSpinRps=spin0;launchChargeEnergy=charge0;crab.ChargePose(charge0);spinAngle=displaySpinAngle=phase0;requestedLaunchSource="retry";Launch("");};}
  prepareWorkR18=PrepareWorkR18(true).GetEnumerator();Report();return true;
 }finally{PrepareEnvelopeR18.Exit(traceBeginR18,Time.renderedFrameCount);}
}
 FlightState PreparationStateR18(){prepareViewR18.renderedFrameCount=Time.renderedFrameCount;prepareViewR18.frameRealtime=Time.realtimeSinceStartupAsDouble;prepareViewR18.inputAcks=inputAcksR23.ToArray();prepareViewR18.throwAudit=latestThrowAudit;prepareViewR18.throwAuditChanged=true;prepareViewR18.generation=generation;return prepareViewR18;}
 static double PrepareMsR18(long ticks)=>ticks*1000d/System.Diagnostics.Stopwatch.Frequency;
 void AdvancePrepareFrameR18(){
  long start=System.Diagnostics.Stopwatch.GetTimestamp();PrepareUnitTimelineR18.BeginAdvance(start,Time.renderedFrameCount);try{if(prepareDiagnosticTimingR18){if(prepareFramesR18==0)prepareFirstWorkDelayR18=PrepareMsR18(start-prepareStartedR18);else prepareMaxWaitR18=Math.Max(prepareMaxWaitR18,PrepareMsR18(start-prepareLastWorkEndR18));}prepareFramesR18++;bool complete=false;
  PrepareCostProbeR15.ResumeWork();
  try{do{if(!(PrepareEnvelopeR18.NeedsFirstMove?FirstPrepareMoveR18():prepareWorkR18.MoveNext())){complete=true;break;}if(ReferenceEquals(prepareWorkR18.Current,PrepareBoundaryR18.Value))break;}while(PrepareMsR18(System.Diagnostics.Stopwatch.GetTimestamp()-start)<PrepareBudgetMsR18);}
  catch(Exception ex){FailPrepareFrameR18(ex);return;}
  finally{PrepareCostProbeR15.SuspendWork();if(prepareDiagnosticTimingR18)prepareLastWorkEndR18=System.Diagnostics.Stopwatch.GetTimestamp();}
  double cost=PrepareMsR18(System.Diagnostics.Stopwatch.GetTimestamp()-start);prepareCpuR18+=cost;prepareMaxChunkR18=Math.Max(prepareMaxChunkR18,cost);prepareWallR18=PrepareMsR18(System.Diagnostics.Stopwatch.GetTimestamp()-prepareStartedR18);
  if(!complete)return;
  prepareWorkR18.Dispose();prepareWorkR18=null;prepareViewR18=null;var c=prepareInputR18;var a=prepareAckR18;var launch=prepareLaunchR18;prepareInputR18=null;prepareAckR18=null;prepareLaunchR18=null;
  try{if(preparePauseR18.HasValue)SetPaused(preparePauseR18.Value?"1":"0");if(launch!=null&&paused){launchRejectReasonR23="paused-during-preparation";message="一時停止したので、再投は待機に戻しました";if(c!=null)CompleteInputR23(c,a,launchRejectReasonR23);else Report();}else{launch?.Invoke();launchRejectReasonR23="";if(c!=null)CompleteInputR23(c,a,"");else Report();}int traceDrainR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.QueueDrain,Time.renderedFrameCount);try{while(prepareWorkR18==null&&prepareQueuedR18.Count>0){string queuedJsonR18;int traceEncodeR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.QueueEncode,Time.renderedFrameCount);try{queuedJsonR18=JsonUtility.ToJson(prepareQueuedR18.Dequeue());}finally{PrepareEnvelopeR18.Exit(traceEncodeR18,Time.renderedFrameCount);}ReceiveInputR23(queuedJsonR18);}}finally{PrepareEnvelopeR18.Exit(traceDrainR18,Time.renderedFrameCount);}}
  catch(Exception ex){FailPrepareFrameR18(ex);}
 }finally{PrepareUnitTimelineR18.EndAdvance();}
 }
 bool FirstPrepareMoveR18(){int traceFirstMoveR18=PrepareEnvelopeR18.BeginFirstMove(Time.renderedFrameCount);try{return prepareWorkR18.MoveNext();}finally{PrepareEnvelopeR18.Exit(traceFirstMoveR18,Time.renderedFrameCount);}}
 void FailPrepareFrameR18(Exception ex){
  PrepareCostProbeR15.Cancel();
  if(prepareWorkR18!=null)prepareWorkR18.Dispose();prepareWorkR18=null;contactVisualTemplateR15.ReleaseStagedR18();phase="error";if(prepareViewR18!=null){prepareViewR18.phase="error";prepareViewR18.runtimeReady=false;prepareViewR18.physicsFault="prepare-failed";prepareViewR18.message="準備に失敗しました。再読み込みしてください。";}
  if(prepareInputR18!=null)CompleteInputR23(prepareInputR18,prepareAckR18,"prepare-failed:"+ex.GetType().Name);while(prepareQueuedR18.Count>0){var c=prepareQueuedR18.Dequeue();CompleteInputR23(c,new FlightInputAckR23{id=c.id,method=c.method,payload=c.payload,beforePhase="preparing"},"prepare-failed");}prepareInputR18=null;prepareAckR18=null;Report();Debug.LogException(ex);
 }
 public void ReadPreparationFramesR18(string _){
#if UNITY_WEBGL && !UNITY_EDITOR
  if(!new Uri(Application.absoluteURL).IsLoopback)return;
#endif
  resourceAuditR32=JsonUtility.ToJson(new PreparationReceiptR18{frames=prepareFramesR18,cpuMs=prepareCpuR18,wallMs=prepareWallR18,maxChunkMs=prepareMaxChunkR18,diagnosticTiming=prepareDiagnosticTimingR18,firstWorkDelayMs=prepareFirstWorkDelayR18,maxBetweenWorkMs=prepareMaxWaitR18,pending=prepareWorkR18!=null,queued=prepareQueuedR18.Count});Report();resourceAuditR32="";
 }
 void CancelPreparationR18(){PrepareCostProbeR15.Cancel();if(prepareWorkR18!=null){prepareWorkR18.Dispose();prepareWorkR18=null;if(crab!=null&&crab.Root)crab.Dispose();}prepareViewR18=null;prepareQueuedR18.Clear();prepareInputR18=null;prepareAckR18=null;}
 [Serializable]class PreparationReceiptR18{public string scope="Work-iterator active wall spans (legacy cpuMs), not CPU cycles. Opt-in firstWorkDelayMs/maxBetweenWorkMs separate scheduler waits; receipt/bridge/drained configuration excluded. Full rAF and send-to-ack remain acceptance metrics.";public int frames,queued;public double cpuMs,wallMs,maxChunkMs,firstWorkDelayMs,maxBetweenWorkMs;public bool pending,diagnosticTiming;}
#if UNITY_EDITOR
 public void ReviewPrepareFramesR18(){if(!BeginPrepareFrameR18(null,null,false)){Prepare("");return;}for(int k=0;k<10000&&prepareWorkR18!=null;k++)Tick(1f/60);if(prepareWorkR18!=null||prepareViewR18!=null||phase!="ready")throw new Exception("Frame preparation did not finish ready");}
 public string ReviewPreparationR18()=>JsonUtility.ToJson(new PreparationReceiptR18{frames=prepareFramesR18,cpuMs=prepareCpuR18,wallMs=prepareWallR18,maxChunkMs=prepareMaxChunkR18,diagnosticTiming=prepareDiagnosticTimingR18,firstWorkDelayMs=prepareFirstWorkDelayR18,maxBetweenWorkMs=prepareMaxWaitR18,pending=prepareWorkR18!=null,queued=prepareQueuedR18.Count});

#endif
}

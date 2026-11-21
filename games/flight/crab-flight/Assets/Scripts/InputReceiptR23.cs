using System;using System.Collections.Generic;using UnityEngine;
[Serializable] public sealed class FlightInputR23 {public int id;public string method,payload;}
[Serializable] public sealed class FlightInputAckR23 {
 public int id,generation,attempts,upgradeLevel;public string method,payload,beforePhase,phase,reason;public bool accepted,aiming,chargeStarted,chargeReady;
 public float requestedPower,appliedPower,chargeElapsed,chargeEnergy,spinPhase,spinRps;
}
public sealed partial class CrabFlightGame {
 readonly List<FlightInputAckR23> inputAcksR23=new List<FlightInputAckR23>();string launchRejectReasonR23="";
 string sourceIdentityR23;string SourceIdentityR23(){if(sourceIdentityR23==null){var a=Resources.Load<TextAsset>("SourceIdentityR21");sourceIdentityR23=a?a.text:"UNSTAMPED";}return sourceIdentityR23;}
 // Browser commands are acknowledged on receipt, not inferred from a DOM change.
 // Native callers keep their existing API/physical semantics.
 public void ReceiveInputR23(string json){
 int traceReceiveR18=PrepareEnvelopeR18.BeginReceive(Time.renderedFrameCount);try{
  FlightInputR23 c;int traceParseR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.Parse,Time.renderedFrameCount);try{c=JsonUtility.FromJson<FlightInputR23>(json);}finally{PrepareEnvelopeR18.Exit(traceParseR18,Time.renderedFrameCount);}if(c==null)return;PrepareEnvelopeR18.BindReceive(traceReceiveR18,c.id,c.method);int traceInitR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.Initialize,Time.renderedFrameCount);try{Initialize();}finally{PrepareEnvelopeR18.Exit(traceInitR18,Time.renderedFrameCount);}
  FlightInputAckR23 a;int traceAckCreateR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.AckCreate,Time.renderedFrameCount);try{a=new FlightInputAckR23{id=c.id,method=c.method,payload=c.payload,beforePhase=phase,requestedPower=settings.power};}finally{PrepareEnvelopeR18.Exit(traceAckCreateR18,Time.renderedFrameCount);}string reason="";
  if(prepareWorkR18!=null){if((c.method=="Prepare"||c.method=="Configure"||c.method=="SetUpgradeLevel")&&prepareQueuedR18.Count<64)prepareQueuedR18.Enqueue(c);else CompleteInputR23(c,a,"preparing");return;}
  switch(c.method){
   case "Prepare":if(BeginPrepareFrameR18(c,a,false))return;Prepare(c.payload);launchRejectReasonR23="";break;
   case "RetryLaunch":if(paused||phase!="result")reason=paused?"paused":"not-result";else{if(BeginPrepareFrameR18(c,a,true))return;RetryLaunch(c.payload);launchRejectReasonR23="";}break;
   case "SetUpgradeLevel":if(phase=="flight"||aiming||(phase=="ready"&&spinCharge.Started))reason="not-upgradable";else{SetUpgradeLevel(c.payload);if(!int.TryParse(c.payload,out _))reason="invalid-level";}break;
   case "Configure":if(phase=="flight"||phase=="charge"||aiming)reason="not-configurable";else{Configure(c.payload);a.requestedPower=settings.power;}break;
   case "BeginSpinCharge":if(paused||phase!="ready")reason=paused?"paused":"not-ready";else BeginSpinCharge(c.payload);break;
   case "AddSpinWork":if(paused||phase!="ready")reason=paused?"paused":"not-ready";else AddSpinWork(c.payload);break;
   case "BeginAim":if(paused||phase!="ready"||aiming)reason=paused?"paused":aiming?"already-aiming":"not-ready";else BeginAim(c.payload);break;
   case "PreviewLaunch":if(paused||phase!="ready"||!aiming)reason=paused?"paused":!aiming?"not-aiming":"not-ready";else PreviewLaunch(c.payload);break;
   case "CancelAim":if(!aiming)reason="not-aiming";else CancelAim(c.payload);break;
   case "ReleaseLaunch":
    if(paused||phase!="ready"||!aiming)reason=paused?"paused":!aiming?"not-aiming":"not-ready";
    else {var input=JsonUtility.FromJson<FlightAimInput>(c.payload);a.requestedPower=input.power;ReleaseLaunch(c.payload);if(phase!="flight")reason=!spinCharge.Ready?"charge-not-ready":"ineligible-swipe";}
    launchRejectReasonR23=reason;break;
   default:reason="unknown-method";break;
  }
  CompleteInputR23(c,a,reason);
 }finally{PrepareEnvelopeR18.Exit(traceReceiveR18,Time.renderedFrameCount);}
}
 void CompleteInputR23(FlightInputR23 c,FlightInputAckR23 a,string reason){
 int traceReceiptR18=PrepareEnvelopeR18.EnterReceipt(PrepareEnvelopeR18.CompleteReceipt,c.id,Time.renderedFrameCount);try{
  a.upgradeLevel=settings.upgradeLevel;a.reason=reason;a.accepted=reason.Length==0;a.phase=phase;a.generation=generation;a.attempts=attempts;a.appliedPower=settings.power;a.aiming=aiming;a.chargeStarted=spinCharge.Started;a.chargeReady=spinCharge.Ready;a.chargeElapsed=spinCharge.Elapsed;a.chargeEnergy=spinCharge.Energy;a.spinPhase=displaySpinAngle;a.spinRps=currentSpinRps;
  inputAcksR23.Add(a);if(inputAcksR23.Count>32)inputAcksR23.RemoveAt(0);
  // Rejected input is retained too, including a release that never creates a throw.
  int traceAuditR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.ReceiptAudit,Time.renderedFrameCount);try{auditInputs.Add(new ThrowInputR21{method="receipt:"+c.method,payload=JsonUtility.ToJson(a),tick=auditTick});}finally{PrepareEnvelopeR18.Exit(traceAuditR18,Time.renderedFrameCount);}Report();
 }finally{PrepareEnvelopeR18.Exit(traceReceiptR18,Time.renderedFrameCount);}
}
}

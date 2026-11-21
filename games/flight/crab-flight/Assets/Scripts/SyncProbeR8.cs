using System;using System.Runtime.InteropServices;using UnityEngine;
// Loopback opt-in diagnosis only. No state serialization or physics/settings change.
public sealed partial class CrabFlightGame {
 bool syncProbeR8;
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightSyncStampR8(int kind,int frame,double simulation,float realDelta,int phaseCode);
#endif
 public void SetSyncProbeR8(string value){
#if UNITY_WEBGL && !UNITY_EDITOR
  if(!new Uri(Application.absoluteURL).IsLoopback)return;
#endif
  syncProbeR8=value=="1";
 }
 void SyncStampR8(int kind){
#if UNITY_WEBGL && !UNITY_EDITOR
  if(syncProbeR8)FlightSyncStampR8(kind,Time.renderedFrameCount,simulation!=null?simulation.Time:0,Time.unscaledDeltaTime,paused?3:phase=="flight"?1:phase=="result"?2:0);
#endif
 }
}

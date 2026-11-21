using System;using System.Collections.Generic;using UnityEngine;
// Opt-in loopback-only preparation diagnosis. No frame/physics clock or gameplay work changes.
public static class PrepareCostProbeR15 {
 [Serializable]public class Segment {public string name;public double ms,activeWallMs,waitMs;}
 [Serializable]public class Sample {public string beforePhase;public int attempts,frame;public double totalMs,activeWallMs,waitMs;public Segment[] segments;}
 [Serializable]class Export {public string schema="prepare-r18.2",scope="Opt-in Stopwatch wall subspans. ms/totalMs retain elapsed wall meaning; activeWallMs excludes time between work slices but includes OS preemption during a slice. Not CPU cycles or a performance-gate measurement";public Sample[] samples;}
 public static bool Enabled,Active;static long start,last,activeStart,activeTicks,markedActiveTicks;static bool running;static Sample current;static readonly List<Segment> segments=new List<Segment>();static readonly List<Sample> history=new List<Sample>();
 static long Now()=>System.Diagnostics.Stopwatch.GetTimestamp();static double Ms(long t)=>t*1000d/System.Diagnostics.Stopwatch.Frequency;
 // Synchronous Prepare begins active; async Advance explicitly suspends/resumes at slice boundaries.
 static long ActiveAt(long now)=>activeTicks+(running?now-activeStart:0);
 // Reuse the existing active clock; it already excludes suspended/yield time.
 public static long ReadActiveTicksR18()=>Active?ActiveAt(Now()):0;
 public static void ResumeWork(){if(!Active||running)return;activeStart=Now();running=true;}
 public static void SuspendWork(){if(!Active||!running)return;long now=Now();activeTicks+=now-activeStart;running=false;}
 public static void Cancel(){Active=false;running=false;}
 public static void Begin(string phase,int attempts){if(!Enabled)return;Active=true;segments.Clear();current=new Sample{beforePhase=phase,attempts=attempts,frame=Time.renderedFrameCount};start=last=activeStart=Now();activeTicks=markedActiveTicks=0;running=true;}
 static void MarkAt(string name,long now){long active=ActiveAt(now);segments.Add(new Segment{name=name,ms=Ms(now-last),activeWallMs=Ms(active-markedActiveTicks),waitMs=Ms((now-last)-(active-markedActiveTicks))});last=now;markedActiveTicks=active;}
 public static void Mark(string name){if(Active)MarkAt(name,Now());}
 public static void End(){if(!Active)return;long now=Now();MarkAt("prepare.report",now);long active=ActiveAt(now);current.totalMs=Ms(now-start);current.activeWallMs=Ms(active);current.waitMs=Ms((now-start)-active);current.segments=segments.ToArray();history.Add(current);Cancel();}
 public static string JSON()=>JsonUtility.ToJson(new Export{samples=history.ToArray()});
}
public sealed partial class CrabFlightGame {
 bool LocalPrepareProbeR15(){
#if UNITY_WEBGL && !UNITY_EDITOR
 return new Uri(Application.absoluteURL).IsLoopback;
#else
 return true;
#endif
 }
 public void SetPrepareProbeR15(string value){if(!LocalPrepareProbeR15())return;PrepareCostProbeR15.Enabled=value=="1";if(!PrepareCostProbeR15.Enabled)PrepareCostProbeR15.Cancel();}
 public void ReadPrepareProbeR15(string _){if(!LocalPrepareProbeR15())return;resourceAuditR32=PrepareCostProbeR15.JSON();Report();resourceAuditR32="";}
}

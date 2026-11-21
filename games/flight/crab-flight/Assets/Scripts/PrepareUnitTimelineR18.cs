using System;
using System.Runtime.InteropServices;
using UnityEngine;

// Private opt-in construction diagnosis. Fixed buffers; serialize only after capture.
public static class PrepareUnitTimelineR18 {
 public const int AdvanceCapacity=256,UnitCapacity=512;
 [Serializable]public struct AdvanceRow {public int seq,frame,firstUnit,units,main,proxy;public long start,end,enterBefore,enterAfter,exitBefore,exitAfter;}
 [Serializable]public struct UnitRow {public string id;public int seq,advance,frame,main,proxy;public long start,end,activeStart,activeEnd,creationStart,creationEnd;}
 [Serializable]public class Capture {public string schema="prepare-units-r18.1",scope="Construction diagnosis only. Stopwatch wall ticks, not CPU cycles. kind60/61 use existing synchronous FlightSyncStampR8 bridge: JS performance.now plus C# frame/sequence. Unit spans exclude entry/exit metadata. Observer counter is partial, not total instrumentation cost.";public long frequency=System.Diagnostics.Stopwatch.Frequency,observerTicks;public int advanceCount,unitCount,dropped,errors,totalMain,totalProxy;public AdvanceRow[] advances;public UnitRow[] units;}
 static AdvanceRow[] advances;static UnitRow[] units;static int ac,uc,openAdvance=-1,openUnit=-1,dropped,errors,main,proxy,mainAtUnit,proxyAtUnit;static long observer;static bool enabled;
 static long Now()=>System.Diagnostics.Stopwatch.GetTimestamp();
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightSyncStampR8(int kind,int frame,double stampSeconds,float sequence,int phaseCode);
#endif
 static void Bridge(int kind,int frame,long stamp,int seq){
#if UNITY_WEBGL && !UNITY_EDITOR
  FlightSyncStampR8(kind,frame,stamp/(double)System.Diagnostics.Stopwatch.Frequency,seq,60);
#endif
 }
 public static void SetEnabled(bool value){enabled=false;if(value){if(advances==null)advances=new AdvanceRow[AdvanceCapacity];else Array.Clear(advances,0,advances.Length);if(units==null)units=new UnitRow[UnitCapacity];else Array.Clear(units,0,units.Length);ac=uc=dropped=errors=main=proxy=0;observer=0;openAdvance=openUnit=-1;enabled=true;}}
 public static void BeginAdvance(long originalStart,int frame){if(!enabled)return;long t=Now();if(openAdvance>=0||openUnit>=0){errors++;return;}if(ac>=AdvanceCapacity){dropped++;return;}openAdvance=ac++;var row=new AdvanceRow{seq=ac,frame=frame,start=originalStart,firstUnit=uc,main=main,proxy=proxy};row.enterBefore=Now();Bridge(60,frame,row.enterBefore,row.seq);row.enterAfter=Now();advances[openAdvance]=row;observer+=Now()-t;}
 public static void EndAdvance(){if(!enabled||openAdvance<0)return;long t=Now();if(openUnit>=0)errors++;var row=advances[openAdvance];row.units=uc-row.firstUnit;row.main=main-row.main;row.proxy=proxy-row.proxy;row.exitBefore=Now();Bridge(61,row.frame,row.exitBefore,row.seq);row.exitAfter=Now();row.end=row.exitAfter;advances[openAdvance]=row;openAdvance=-1;observer+=Now()-t;}
 public static void BeginUnit(string id,int frame){if(!enabled)return;long t=Now();if(openAdvance<0||openUnit>=0){errors++;return;}if(uc>=UnitCapacity){dropped++;return;}openUnit=uc++;mainAtUnit=main;proxyAtUnit=proxy;units[openUnit]=new UnitRow{id=id,seq=uc,advance=advances[openAdvance].seq,frame=frame,activeStart=PrepareCostProbeR15.ReadActiveTicksR18(),creationStart=CityShapeSplitProbeR18.ReadCreationTicks()};units[openUnit].start=Now();observer+=units[openUnit].start-t;}
 public static void Created(bool hasProxy){if(!enabled||openAdvance<0)return;main++;if(hasProxy)proxy++;}
 public static void EndUnit(){if(!enabled||openUnit<0)return;long t=Now();var row=units[openUnit];row.end=t;row.activeEnd=PrepareCostProbeR15.ReadActiveTicksR18();row.creationEnd=CityShapeSplitProbeR18.ReadCreationTicks();row.main=main-mainAtUnit;row.proxy=proxy-proxyAtUnit;if(openAdvance<0||row.advance!=advances[openAdvance].seq||row.frame!=advances[openAdvance].frame)errors++;units[openUnit]=row;openUnit=-1;observer+=Now()-t;}
 public static Capture Snapshot(){var a=new AdvanceRow[ac];var u=new UnitRow[uc];if(ac>0)Array.Copy(advances,a,ac);if(uc>0)Array.Copy(units,u,uc);return new Capture{advanceCount=ac,unitCount=uc,dropped=dropped,errors=errors+(openAdvance>=0?1:0)+(openUnit>=0?1:0),totalMain=main,totalProxy=proxy,observerTicks=observer,advances=a,units=u};}
 public static void Control(){for(int i=0;i<8;i++){BeginAdvance(Now(),Time.renderedFrameCount);BeginUnit("clock-control",Time.renderedFrameCount);EndUnit();EndAdvance();}}
}
public sealed partial class CrabFlightGame {
 public void SetPrepareTimelineR18(string value){if(!LocalPrepareProbeR15())return;PrepareUnitTimelineR18.SetEnabled(value=="1");}
 public void ReadPrepareTimelineR18(string _){if(!LocalPrepareProbeR15())return;resourceAuditR32=JsonUtility.ToJson(PrepareUnitTimelineR18.Snapshot());Report();resourceAuditR32="";}
 public void TestPrepareTimelineClockR18(string _){if(!LocalPrepareProbeR15())return;PrepareUnitTimelineR18.Control();ReadPrepareTimelineR18("");}
}

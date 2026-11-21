using System;
using UnityEngine;
// Private fixed-capacity, same-clock wall partition. No physics calls or per-part allocation.
public static class AttachPhaseSplitR18 {
 public const int Capacity=4;
 [Serializable]public struct Row{public int seq,frame,scene,root,parts,hullCalls,colliderIntervals,resetCalls,errors;public bool hero,complete;public long start,end,totalTicks,hullTicks,colliderTicks,resetTicks,residualTicks,observerTicks;}
 [Serializable]public class Capture{public string schema="attach-phase-split-r18.1",scope="Direct Attach wall scope; disjoint hull, collider native setup/registration, final mass/inertia+Reset, residual, partial observer. Not exclusive CPU/native cook profiling or performance acceptance.";public long frequency=System.Diagnostics.Stopwatch.Frequency;public int count,dropped;public Row[] rows;}
 static Row[] rows;static int count,dropped,index=-1,mode;static long last;static bool enabled;
 static long Now()=>System.Diagnostics.Stopwatch.GetTimestamp();
 public static void Set(string value){if(value=="reset"){enabled=false;if(rows==null)rows=new Row[Capacity];else Array.Clear(rows,0,rows.Length);count=dropped=0;index=-1;mode=0;}else enabled=value=="1"&&rows!=null;}
 static bool Active=>enabled&&index>=0;
 static void Close(long t){var r=rows[index];long dt=t-last;if(dt<0){r.errors++;dt=0;}if(mode==1)r.hullTicks+=dt;else if(mode==2)r.colliderTicks+=dt;else if(mode==3)r.resetTicks+=dt;else if(mode==4)r.observerTicks+=dt;else r.residualTicks+=dt;rows[index]=r;last=t;}
 public static void Begin(int frame,int scene,int root,bool hero){if(!enabled)return;if(index>=0){var bad=rows[index];bad.errors++;rows[index]=bad;return;}if(count>=Capacity){dropped++;return;}long t=Now();index=count++;rows[index]=new Row{seq=count,frame=frame,scene=scene,root=root,hero=hero,start=t};last=t;mode=4;Close(Now());mode=0;}
 public static void Enter(int kind){if(!Active)return;Close(Now());if(mode!=0){var bad=rows[index];bad.errors++;rows[index]=bad;}mode=4;var r=rows[index];if(kind==1)r.hullCalls++;else if(kind==2)r.colliderIntervals++;else if(kind==3)r.resetCalls++;else r.errors++;rows[index]=r;Close(Now());mode=kind;}
 public static void Leave(int kind){if(!Active)return;Close(Now());if(mode!=kind){var bad=rows[index];bad.errors++;rows[index]=bad;}mode=4;Close(Now());mode=0;}
 public static void End(int parts,bool complete){if(!Active)return;Close(Now());if(mode!=0){var bad=rows[index];bad.errors++;rows[index]=bad;}mode=4;var r=rows[index];r.parts=parts;r.complete=complete;rows[index]=r;long end=Now();Close(end);r=rows[index];r.end=end;r.totalTicks=end-r.start;rows[index]=r;index=-1;mode=0;}
 public static Capture Snapshot(){var copy=new Row[count];if(count>0)Array.Copy(rows,copy,count);return new Capture{count=count,dropped=dropped,rows=copy};}
}
public sealed partial class CrabFlightGame {
 public void SetAttachPhaseSplitR18(string value){if(!LocalPrepareProbeR15())return;AttachPhaseSplitR18.Set(value);}
 public void ReadAttachPhaseSplitR18(string _){if(!LocalPrepareProbeR15())return;resourceAuditR32=JsonUtility.ToJson(AttachPhaseSplitR18.Snapshot());Report();resourceAuditR32="";}
}

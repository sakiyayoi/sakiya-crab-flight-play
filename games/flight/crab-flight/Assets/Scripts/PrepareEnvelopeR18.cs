using System;using System.Runtime.InteropServices;using UnityEngine;
// Diagnostic-only. All spans are synchronous elapsed wall ticks, NOT CPU cycles.
// Existing Advance/unit/Attach recorders remain authoritative for construction.
public static class PrepareEnvelopeR18 {
 public const int Capacity=2048,CommandCapacity=128,StackCapacity=32;
 public const int Receive=1,Parse=2,Initialize=3,AckCreate=4,BeginPrepare=5,State=6,Report=7,SerializeReport=8,ReportBridge=9,FirstMoveNext=10,CompleteReceipt=11,ReceiptAudit=12,QueueDrain=13,QueueEncode=14,Configure=15;
 [Serializable]public struct Span {public int seq,parent,kind,command,frame,endFrame;public long start,end;public bool closed;}
 [Serializable]public struct Command {public int seq,id;public string method;public bool parsed;}
 [Serializable]public class Capture {public string schema="prepare-envelope-r18.1",scope="Private non-yield inclusive wall spans. Nested spans overlap; do not sum parents and children. Existing unit/Advance/Attach reused. kind70/71 bracket existing JS sync bridge; real WebGL clock/overhead validation pending.";public long frequency=System.Diagnostics.Stopwatch.Frequency,windowStart,windowEnd,entryBefore,entryAfter,exitBefore,exitAfter;public int epoch,entryFrame,exitFrame,count,commandCount,open,dropped,errors,prepareStarts,firstMoves;public bool frozen,capturing,firstMovePending;public Span[] spans;public Command[] commands;}
 static Span[] rows;static Command[] commands;static int[] stack;static int count,cc,depth,dropped,errors,epoch,entryFrame,exitFrame,pendingCommand,prepareStarts,firstMoves;static long windowStart,windowEnd,entryBefore,entryAfter,exitBefore,exitAfter;static bool enabled,capturing,frozen,firstMovePending;
#if PREPARE_ENVELOPE_TEST
 public static Func<long> TestClock;
#endif
 static long Now(){
#if PREPARE_ENVELOPE_TEST
  if(TestClock!=null)return TestClock();
#endif
  return System.Diagnostics.Stopwatch.GetTimestamp();
 }
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightSyncStampR8(int kind,int frame,double stampSeconds,float sequence,int phaseCode);
#endif
 static void Anchor(int kind,int frame,long stamp){
#if UNITY_WEBGL && !UNITY_EDITOR
  FlightSyncStampR8(kind,frame,stamp/(double)System.Diagnostics.Stopwatch.Frequency,epoch,70);
#endif
 }
 public static void SetEnabled(bool value){enabled=false;if(!value){Freeze();return;}if(rows==null){rows=new Span[Capacity];commands=new Command[CommandCapacity];stack=new int[StackCapacity];}else{Array.Clear(rows,0,rows.Length);Array.Clear(commands,0,commands.Length);Array.Clear(stack,0,stack.Length);}count=cc=depth=dropped=errors=pendingCommand=prepareStarts=firstMoves=0;windowStart=windowEnd=entryBefore=entryAfter=exitBefore=exitAfter=0;entryFrame=exitFrame=0;capturing=frozen=firstMovePending=false;epoch++;enabled=true;}
 static int CurrentCommand=>depth>0?rows[stack[depth-1]].command:pendingCommand;
 static int Add(int kind,int frame,int command){if(!enabled||!capturing)return -1;if(count>=Capacity||depth>=StackCapacity){dropped++;return -1;}int token=count++;rows[token]=new Span{seq=token+1,parent=depth>0?stack[depth-1]+1:0,kind=kind,command=command,frame=frame,start=Now()};stack[depth++]=token;return token;}
 public static int Enter(int kind,int frame){return Add(kind,frame,CurrentCommand);}
 public static int BeginReceive(int frame){if(!enabled)return -1;if(!capturing){capturing=true;windowStart=Now();entryFrame=frame;entryBefore=Now();Anchor(70,frame,entryBefore);entryAfter=Now();}if(cc>=CommandCapacity){dropped++;return -1;}int serial=++cc;commands[serial-1]=new Command{seq=serial};return Add(Receive,frame,serial);}
 public static void BindReceive(int token,int id,string method){if(!enabled||token<0||token>=count)return;int c=rows[token].command;if(c<1||c>cc){errors++;return;}commands[c-1]=new Command{seq=c,id=id,method=method,parsed=true};}
 static int Find(int id){for(int k=cc-1;k>=0;k--)if(commands[k].parsed&&commands[k].id==id)return k+1;return CurrentCommand;}
 public static int EnterReceipt(int kind,int id,int frame){if(!enabled||!capturing)return -1;return Add(kind,frame,Find(id));}
 public static void Preparing(int id){if(!enabled||!capturing)return;pendingCommand=Find(id);prepareStarts++;firstMovePending=true;}
 public static bool NeedsFirstMove=>enabled&&capturing&&firstMovePending;
 public static int BeginFirstMove(int frame){if(!NeedsFirstMove)return -1;firstMovePending=false;firstMoves++;return Add(FirstMoveNext,frame,pendingCommand);}
 public static void Exit(int token,int frame){if(!enabled||token<0)return;if(token>=count||depth==0||stack[depth-1]!=token){errors++;return;}long t=Now();var row=rows[token];row.end=t;row.endFrame=frame;row.closed=true;if(row.frame!=frame||t<row.start)errors++;rows[token]=row;depth--;}
 public static void Freeze(){if(frozen)return;if(capturing){exitFrame=Time.renderedFrameCount;exitBefore=Now();Anchor(71,exitFrame,exitBefore);exitAfter=Now();windowEnd=exitAfter;}enabled=false;frozen=true;}
 public static Capture Snapshot(){var s=new Span[count];var c=new Command[cc];if(count>0)Array.Copy(rows,s,count);if(cc>0)Array.Copy(commands,c,cc);return new Capture{epoch=epoch,entryFrame=entryFrame,exitFrame=exitFrame,windowStart=windowStart,windowEnd=windowEnd,entryBefore=entryBefore,entryAfter=entryAfter,exitBefore=exitBefore,exitAfter=exitAfter,count=count,commandCount=cc,open=depth,dropped=dropped,errors=errors,prepareStarts=prepareStarts,firstMoves=firstMoves,frozen=frozen,capturing=enabled&&capturing,firstMovePending=firstMovePending,spans=s,commands=c};}
}
public sealed partial class CrabFlightGame {
 public void SetPrepareEnvelopeR18(string value){if(!LocalPrepareProbeR15())return;PrepareEnvelopeR18.SetEnabled(value=="1");}
 public void ReadPrepareEnvelopeR18(string _){if(!LocalPrepareProbeR15())return;PrepareEnvelopeR18.Freeze();resourceAuditR32=JsonUtility.ToJson(PrepareEnvelopeR18.Snapshot());Report();resourceAuditR32="";}
}

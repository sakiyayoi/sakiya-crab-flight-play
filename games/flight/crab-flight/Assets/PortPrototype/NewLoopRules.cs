using System;
using System.Collections.Generic;
using System.Linq;

// Shared runtime policy; exercised without a Unity process by the contract probe.
public static class NewLoopRules {
 public const string Revision="new-loop-fix-1";
 public const float Start=697,End=757,DestructionSeconds=4,MaxSeconds=5.2f;
 public const float BeamSeconds=2.5f,SlowSeconds=.30f,SlowScale=.25f;
 public const float WaveHopSeconds=.04f,CameraPunchSeconds=.30f,CameraPunchScale=.72f;
 public const float ShardSpeedMin=20,ShardSpeedMax=28;
 public const float BeamRadius=.7f,BeamImpulsePerMass=8,LaunchSpeed=22,LaunchSpin=2;
 public const float ChargeSeconds=1.8f,DizzySeconds=1,DecayPerSecond=.5f;
 public const int SegmentCount=2;
 public static float SegmentStart(int segment)=>segment==1?781:Start;
 public static float SegmentEnd(int segment)=>segment==1?848:End;
 public static string Beat(int segment)=>segment==1?"S7":"S5";
 public static bool Contains(float x,int segment=0)=>x>=SegmentStart(segment)&&x<=SegmentEnd(segment);
 public static float Energy(float e)=>float.IsFinite(e)?Math.Clamp(e,0,1):0;
 public static int Tier(float e)=>Energy(e)>=1?3:Energy(e)>=.5f?2:1;
 public static float Reach(float e)=>8*(1+Energy(e));
 public static int Capacity(int baseline,float e)=>(int)Math.Ceiling(Math.Max(1,baseline)*(1+2*Energy(e)));
 public static bool Ignition(string role)=>role!=null&&(role.StartsWith("boost",StringComparison.Ordinal)||role=="jackpot");
 public static int OrdinaryCapacity(float e)=>(int)Math.Ceiling(3*Energy(e));
 public static float Intensity(float e)=>(float)((Math.Exp(3*Energy(e))-1)/(Math.Exp(3)-1));
 public static float VisualSpin(float e)=>2+30*Intensity(e);
 public static float HopDue(float start,int hop)=>start+Math.Max(0,hop)*WaveHopSeconds;
 // A breadth-first authored graph, independent of solver contacts and break order.
 // Include already-broken nodes as bridges; they are skipped only at accounting time.
 public static List<(T target,int hop)> WavePlan<T>(T source,IEnumerable<T> members,float energy,bool ignition,Func<T,T,float> distance,Func<T,string> id){
  var all=members.OrderBy(id,StringComparer.Ordinal).ToArray();
  var plan=new List<(T target,int hop)>{(source,0)};var seen=new HashSet<T>{source};
  for(int cursor=0;cursor<plan.Count;cursor++){
   var node=plan[cursor];if(!ignition&&cursor>0)break;
   int baseline=all.Count(p=>!EqualityComparer<T>.Default.Equals(p,node.target)&&distance(node.target,p)<=8);
   int cap=ignition?Capacity(baseline,energy):OrdinaryCapacity(energy);
   foreach(var p in all.Where(p=>!seen.Contains(p)&&distance(node.target,p)<=Reach(energy)).OrderBy(p=>distance(node.target,p)).ThenBy(id,StringComparer.Ordinal).Take(cap)){
    seen.Add(p);plan.Add((p,node.hop+1));
   }
  }
  return plan;
 }
 public static float Wobble(bool dizzy,bool held,float age)=>dizzy&&held&&age<DizzySeconds?6*(float)Math.Sin(age*19):0;
 public static bool Cleared(int broken,int total)=>total>0&&broken*10>=total*7;
 public static float Rate(int broken,int total)=>total>0?(float)broken/total:0;
 public static bool ResultDue(float elapsed,float releaseAge,bool released,bool stopped,bool outside)=>stopped||outside||(released?releaseAge>=DestructionSeconds:elapsed>=MaxSeconds);
}

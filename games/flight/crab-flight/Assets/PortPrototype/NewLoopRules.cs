using System;

// Shared runtime policy; exercised without a Unity process by the contract probe.
public static class NewLoopRules {
 public const string Revision="new-loop-2";
 public const float Start=697,End=757,DestructionSeconds=3,MaxSeconds=5.2f;
 public const float BeamSeconds=2.5f,SlowSeconds=.22f,SlowScale=.25f;
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
 public static float Wobble(bool dizzy,bool held,float age)=>dizzy&&held&&age<DizzySeconds?6*(float)Math.Sin(age*19):0;
 public static bool Cleared(int broken,int total)=>total>0&&broken*10>=total*7;
 public static float Rate(int broken,int total)=>total>0?(float)broken/total:0;
 public static bool ResultDue(float elapsed,float releaseAge,bool released,bool stopped,bool outside)=>stopped||outside||(released?releaseAge>=DestructionSeconds:elapsed>=MaxSeconds);
}

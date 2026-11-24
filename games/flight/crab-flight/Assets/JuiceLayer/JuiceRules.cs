using System;

// Presentation policy only. No Unity clock, RNG, contacts, forces or score writes.
public static class JuiceRules {
 public const int MarkLimit=512, MarksPerBreak=40, ChainMilestone=10, TargetFrameRate=60;
 public const double ChainWindow=1.5, PeakSeconds=.28, PeakScale=.25, PeakCooldown=1.0;
 public const float SmokeLife=.65f, ShardLife=.55f, SparkLife=.32f;
 public const float HeroShake=.18f, LandmarkShake=.12f, PeakShake=.10f;
 public const float SmallSize=1.25f, NormalShakeMin=.025f, NormalShakeMax=.07f;
 public const float NormalShakeSeconds=.12f, PeakShakeSeconds=.20f, NormalShakeGap=.18f;
 public static int Layer(int index)=>index%5<3?0:index%5==3?5:6;
 public static int Evict(int active,int incoming)=>Math.Max(0,active+incoming-MarkLimit);
 public static float Shake(float size,bool hero,bool landmark,bool milestone){
  if(hero)return HeroShake;if(landmark)return LandmarkShake;if(milestone)return PeakShake;
  if(float.IsNaN(size)||size<SmallSize)return 0;
  return NormalShakeMin+(NormalShakeMax-NormalShakeMin)*Math.Min(1,Math.Max(0,(size-SmallSize)/8));
 }
 public sealed class ShakePulse {
  public double Age {get;private set;}
  public double Duration {get;private set;}
  public double Amplitude {get;private set;}
  double gap;
  public void Reset(){Age=Duration=Amplitude=gap=0;}
  public bool Request(double amplitude,bool peak){
   if(double.IsNaN(amplitude)||double.IsInfinity(amplitude)||amplitude<=0||gap>0&&amplitude<=Amplitude)return false;
   Amplitude=amplitude;Age=0;Duration=peak?PeakShakeSeconds:NormalShakeSeconds;
   gap=peak?PeakShakeSeconds+.06:NormalShakeGap;return true;
  }
  public void Advance(double delta){
   if(double.IsNaN(delta)||double.IsInfinity(delta)||delta<=0)return;
   Age+=delta;gap=Math.Max(0,gap-delta);
  }
 }
 public sealed class Sequence {
  double last=double.NegativeInfinity,nextPeak=double.NegativeInfinity;
  int lastId=-1;
  public int Count {get;private set;}
  public void Reset(){Count=0;lastId=-1;last=nextPeak=double.NegativeInfinity;}
  public bool Record(int id,double now){
   if(id<=lastId||double.IsNaN(now)||double.IsInfinity(now))return false;
   lastId=id;if(now<last||now-last>ChainWindow)Count=0;
   Count++;last=now;return true;
  }
  public bool Milestone=>Count>0&&Count%ChainMilestone==0;
  public bool TakePeak(double now,bool hero,bool landmark){
   if(!(hero||landmark||Milestone)||now<nextPeak)return false;
   nextPeak=now+PeakCooldown;return true;
  }
 }
}

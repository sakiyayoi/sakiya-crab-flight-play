using System;
using System.Collections.Generic;

namespace CrabFeelV1 {
 // One master gate must own EVERY white, colour and target flash in the game.
 // Call at the actual render of each phase, using a continuous unscaled clock.
 public sealed class FlashGate {
  public const int MaximumPerSecond=3;
  readonly Queue<double> displayed=new Queue<double>();
  readonly HashSet<string> seen=new HashSet<string>();
  readonly Queue<string> seenOrder=new Queue<string>();
  double lastTime=double.NegativeInfinity;
  public int Accepted {get;private set;}
  public int Suppressed {get;private set;}
  public bool TryDisplay(double actualNow,string phaseKey,bool effectsOn,bool reducedMotion){
   if(double.IsNaN(actualNow)||double.IsInfinity(actualNow)||actualNow<lastTime||string.IsNullOrEmpty(phaseKey)){Suppressed++;return false;}
   lastTime=actualNow;
   if(!seen.Add(phaseKey))return false;
   seenOrder.Enqueue(phaseKey);while(seenOrder.Count>512)seen.Remove(seenOrder.Dequeue());
   // Keep history through pause, generation changes, off/on and reduced motion.
   // A closed one-second interval at an exact boundary is still limited to3.
   while(displayed.Count>0&&actualNow-displayed.Peek()>1.000001)displayed.Dequeue();
   if(!effectsOn||reducedMotion||displayed.Count>=MaximumPerSecond){Suppressed++;return false;}
   displayed.Enqueue(actualNow);Accepted++;return true;
  }
 }

 // Local simulation-time adapter. UI, voice and music continue on real time.
 // It never sets global Time.timeScale, poses, forces, score or distance.
 public sealed class PresentationClock {
  double hold,slow,scale=1;
  public double HoldRemaining {get{return hold;}}
  public double SlowRemaining {get{return slow;}}
  public void Reset(){hold=slow=0;scale=1;}
  public void Request(double holdSeconds,double slowSeconds,double slowScale){
   if(!Finite(holdSeconds)||!Finite(slowSeconds)||!Finite(slowScale))return;
   // Coincident cues promote to the longest hold; their durations never add.
   hold=Math.Max(hold,Math.Max(0,holdSeconds));
   slow=Math.Max(slow,Math.Max(0,slowSeconds));scale=Math.Min(1,Math.Max(.01,slowScale));
  }
  public double Filter(double realDelta,bool paused,bool effectsOn){
   if(!Finite(realDelta)||realDelta<=0||paused)return 0;
   if(!effectsOn){Reset();return realDelta;}
   double frozen=Math.Min(hold,realDelta);hold-=frozen;double remaining=realDelta-frozen;
   double slowed=Math.Min(slow,remaining);slow-=slowed;
   return frozen*.05+slowed*scale+(remaining-slowed);
  }
  static bool Finite(double x){return !double.IsNaN(x)&&!double.IsInfinity(x);}
 }

 public struct CameraDelta {public double x,y,kick;}
 public static class CameraSafety {
  // This additive delta belongs only to the camera. No UI ancestor is moved.
  // No Unity/System Random use: presentation must not consume simulation RNG.
  public static CameraDelta Sample(double age,double duration,double amplitude,double kick,bool effectsOn,bool shakeOn,bool reducedMotion,double seed){
   var r=new CameraDelta();
   if(!effectsOn||!shakeOn||reducedMotion||!Finite(age)||!Finite(duration)||!Finite(amplitude)||!Finite(kick)||!Finite(seed)||age<0||duration<=0||age>=duration)return r;
   double envelope=1-age/duration;envelope*=envelope;
   r.x=Math.Sin(age*137.1+seed)*amplitude*envelope;
   r.y=Math.Sin(age*173.7+seed*.73)*amplitude*envelope;
   r.kick=kick*envelope;return r;
  }
  static bool Finite(double x){return !double.IsNaN(x)&&!double.IsInfinity(x);}
 }
}

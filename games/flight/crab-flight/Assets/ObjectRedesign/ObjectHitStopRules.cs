using System;

// Presentation seconds only. Never passed to a simulation/accumulator clock.
public static class ObjectHitStopRules {
 public const double Boost=.070, Bounce=.070, Spin=.070, Brake=.090, Blast=.140, Death=.200;
 public static double Seconds(ObjectRules.Kind kind){
  switch(kind){
   case ObjectRules.Kind.Boost:return Boost;
   case ObjectRules.Kind.Bounce:return Bounce;
   case ObjectRules.Kind.Spin:return Spin;
   case ObjectRules.Kind.Brake:return Brake;
   case ObjectRules.Kind.Blast:return Blast;
   case ObjectRules.Kind.Death:return Death;
   default:return 0;
  }
 }
 public sealed class Window {
  double until,pending;
  public bool NeedsCapture=>pending>0;
  public bool Active(double now)=>NeedsCapture||now<until;
  public double Remaining(double now)=>Math.Max(pending,Math.Max(0,until-now));
  public void Request(double seconds){pending=Math.Max(pending,seconds);}
  // Concurrent contacts/peaks use the longer remaining interval, never a sum.
  // Start on the actual rendered impact frame, even after a long CPU frame.
  public void Captured(double now){until=Math.Max(until,now+pending);pending=0;}
  public void Clear(){until=pending=0;}
 }
}

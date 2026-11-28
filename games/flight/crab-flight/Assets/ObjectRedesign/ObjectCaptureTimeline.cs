using System;

// Presentation-only, unscaled clock. The authoritative run is already finished.
public sealed class ObjectCaptureTimeline {
 public const double PullSeconds=.6, CaptionSeconds=1.2;
 public enum Stage { Idle, Hold, Capture, Caption, Done }
 public Stage Current {get;private set;}
 public double Age {get;private set;}
 double last;
 public bool Active=>Current==Stage.Hold||Current==Stage.Capture||Current==Stage.Caption;
 public bool CaptionVisible=>Current==Stage.Caption;
 public float PullProgress=>(float)Math.Min(1,Age/PullSeconds);
 public void Begin(double now,bool animate){last=now;Age=0;Current=animate?Stage.Hold:Stage.Caption;}
 public void Advance(double now,bool imageHeld,bool animate,bool paused){
  double delta=Math.Max(0,now-last);last=now;
  if(!Active||paused)return;
  if(!animate&&Current!=Stage.Caption){Current=Stage.Caption;Age=0;return;}
  if(Current==Stage.Hold){
   // Wait for the actual rendered hold, including a delayed first GPU frame.
   if(!imageHeld){Current=Stage.Capture;Age=0;}return;
  }
  Age+=delta;
  // Each new stage gets a displayed frame before its own clock starts.
  if(Current==Stage.Capture&&Age>=PullSeconds){Current=Stage.Caption;Age=0;}
  else if(Current==Stage.Caption&&Age>=CaptionSeconds)Current=Stage.Done;
 }
 public void Clear(){Current=Stage.Idle;Age=0;last=0;}
}

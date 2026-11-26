using System.Collections.Generic;
using UnityEngine;

public sealed partial class CrabFlightGame {
 readonly Dictionary<string,JuiceVisual> juiceVisuals=new Dictionary<string,JuiceVisual>(1536);
 readonly JuiceRules.Sequence juiceSequence=new JuiceRules.Sequence();
 readonly JuiceRules.ShakePulse juiceShake=new JuiceRules.ShakePulse();
 void ResetJuice(){
  juiceSequence.Reset();ClearJuiceShake();if(juiceBurst)juiceBurst.Clear();
  if(world!=null)world.ReadJuiceVisuals(juiceVisuals);
 }
 void ClearJuiceShake(){juiceShake.Reset();}
 void PresentJuiceBreak(FlightGameEvent e,FeelCueR27 cue){
  double now=Time.realtimeSinceStartupAsDouble;
  if(!juiceSequence.Record(e.id,now)||!FeelEffectsR27)return;
  if(!juiceVisuals.TryGetValue(e.objectId,out var visual))
   visual=new JuiceVisual{color=new Color(1,.953f,.839f),size=0};
  PresentJuiceBurst(e,cue,visual,(float)now);
  bool motion=!ReducedMotion&&!feelSettingsR27.reducedMotion;
  if(motion&&juiceSequence.TakePeak(now,visual.hero,visual.landmark)){
   // Existing local clock only: same fixed-step simulation, no global Time.timeScale write.
   feelClockStampR27=now;feelWasSlowR27=true;
   feelClockR27.Request(0,JuiceRules.PeakSeconds,JuiceRules.PeakScale);
   FeelLogR27(cue,"juice_peak",x=>{x.seconds=(float)JuiceRules.PeakSeconds;x.timeScale=(float)JuiceRules.PeakScale;x.chain=juiceSequence.Count;});
  }
  float amplitude=JuiceRules.Shake(visual.size,visual.hero,visual.landmark,juiceSequence.Milestone);
  if(!motion||!feelSettingsR27.shake||amplitude<=0)return;
  // Do not extend a running shake on every contact. A larger peak may promote it once.
  juiceShake.Request(amplitude,visual.hero||visual.landmark||juiceSequence.Milestone);
 }
 Vector3 JuiceShakeOffset(float dt){
  if(!FeelEffectsR27||!feelSettingsR27.shake||ReducedMotion||feelSettingsR27.reducedMotion||phase!="flight"){
   ClearJuiceShake();return Vector3.zero;
  }
  var delta=CrabFeelV1.CameraSafety.Sample(juiceShake.Age,juiceShake.Duration,juiceShake.Amplitude,0,true,true,false,generation);
  juiceShake.Advance(dt);
  return Vector3.ClampMagnitude(cam.transform.right*(float)delta.x+cam.transform.up*(float)delta.y,(float)juiceShake.Amplitude);
 }
}

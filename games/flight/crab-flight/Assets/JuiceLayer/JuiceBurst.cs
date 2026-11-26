using UnityEngine;

public sealed partial class CrabFlightGame {
 EffekseerBurstBridge juiceBurst;
 void ConfigureJuiceBurst(){
  if(!feelR27.UsesEffekseer){if(juiceBurst)juiceBurst.Clear();return;}
  if(!juiceBurst){
   var host=new GameObject("Juice Effekseer bursts");host.transform.SetParent(transform,false);
   juiceBurst=host.AddComponent<EffekseerBurstBridge>();
  }
  juiceBurst.Initialize();
 }
 void SyncJuiceBurst(){
  // Same pause/reset clearing as marks; reduced motion never shows ToonHit's flash.
  if(juiceBurst&&(!FeelEffectsR27||!feelR27.UsesEffekseer||paused||ReducedMotion||feelSettingsR27.reducedMotion||phase!="flight"))juiceBurst.Clear();
 }
 void PresentJuiceBurst(FlightGameEvent e,FeelCueR27 cue,JuiceVisual visual,float now){
  if(feelR27.UsesEffekseer){
   bool reduced=ReducedMotion||feelSettingsR27.reducedMotion;
   bool played=!reduced&&!paused&&juiceBurst&&juiceBurst.Play(e.position,visual.hero||visual.landmark||juiceSequence.Milestone);
   FeelLogR27(cue,"effekseer_burst",x=>{x.count=played?1:0;x.kind="ToonHit";x.on=played;x.reason=reduced?"reduced":played?"played":"capacity-or-unavailable";});
  }else{
   int count=feelDrawingR27.JuiceParticles(e.position,visual.color,e.material,cam,now);
   FeelLogR27(cue,"particles",x=>{x.count=count;x.kind="object-shards/white-smoke/lantern-sparks";});
  }
 }
}

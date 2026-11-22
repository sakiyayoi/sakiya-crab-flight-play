using System;
using System.Collections.Generic;
using UnityEngine;
using CrabDestructionVfx;

[Serializable] public sealed class FlightGameEvent {
 public int generation,id;public bool secondary;public string type,objectId,material;public Vector3 position,normal,velocity;public float strength,weight,physicsTime;
}
[Serializable] public sealed class WorldBreakEvent {
 public string id,material;public bool secondary;public Vector3 position,normal,velocity;public float strength,time,weight;
}

public sealed partial class CrabFlightGame {
 public CrabVfxRig effectsPrefab;
 public static bool CityEnabled=true;
 CrabVfxRig effects;GameObject cityVisuals;
 readonly List<FlightGameEvent> eventStream=new List<FlightGameEvent>();
 int generation,eventSequence,readBreaks,readImpacts,readBursts;bool outsideSent;string exitSource="";float lastWorkCue=-100;
 public FlightGameEvent[] ReviewEvents=>eventStream.ToArray();
 void StreamReset(){if(farLifeR10)farLifeR10.ResetVisuals();hitStop.Reset();generation++;eventSequence=readBreaks=readImpacts=readBursts=0;outsideSent=false;exitSource="";lastWorkCue=-100;eventStream.Clear();if(effects){effects.ResetRun();effects.SetPaused(paused);}Emit("reset","","",crab.Root.position,Vector3.up,Vector3.zero,0);}
 void Emit(string type,string objectId,string material,Vector3 point,Vector3 normal,Vector3 velocity,float strength,bool secondary=false,float weight=.5f){
  var e=new FlightGameEvent{generation=generation,id=++eventSequence,type=type,objectId=objectId,material=material,position=point,normal=normal,velocity=velocity,strength=Mathf.Clamp01(strength),secondary=secondary,weight=weight,physicsTime=simulation!=null?simulation.Time:0};eventStream.Add(e);if(eventStream.Count>32)eventStream.RemoveAt(0);
  ObservePortCamera(e);ObserveFeelEventR27(e);if(!FeelOnR27&&useHitStop&&!secondary)hitStop.Request(generation,e.id,type,material,e.strength,ReducedMotion,e.weight);
  if(FeelOnR27||!effects)return;
  if(type=="launch")effects.EmitLaunch(point,velocity,launchChargeEnergy);
  else if(type=="destruction")effects.EmitBreak(e.id,CrabVfxRig.SurfaceFromId(material),point,velocity,normal,BreakVisualStrength(e));
  else if(type=="boost")effects.EmitBoost(e.id,point,velocity,Mathf.Clamp01(strength*.65f));
  else if(type=="exit")effects.EmitExit(point,velocity);
 }
 float BreakVisualStrength(FlightGameEvent e){
#if UNITY_EDITOR
  if(Street3Presentation.ReviewBaseline)return e.strength;
#endif
  return Mathf.Clamp01(e.strength*.6f+e.weight*.4f);
 }
 void DrainPhysicsEvents(){
  while(readBreaks<simulation.BreakEvents.Count){var e=simulation.BreakEvents[readBreaks++];Emit("destruction",e.id,e.material,e.position,e.normal,e.velocity,e.strength,e.secondary,e.weight);if(e.material=="wall")exitSource=e.id;}
  while(readImpacts<simulation.Impacts.Count){var e=simulation.Impacts[readImpacts++];Emit("impact",e.part,e.surface,e.position,e.normal,e.velocity,e.impulse/simulation.Body.mass/8);if(e.surface=="sand"||e.surface=="tank"||e.surface=="float")Emit("landing",e.part,e.surface,e.position,e.normal,e.velocity,e.impulse/18);}
  while(readBursts<simulation.BurstEvents.Count){var b=simulation.BurstEvents[readBursts++];if(b.objectId!="launch")Emit("boost",b.objectId,"rotor",b.position,Vector3.up,simulation.Body.linearVelocity,Mathf.Clamp01((b.afterForward-b.beforeForward)/12));}
  if(!outsideSent&&simulation.WallsBroken>0&&simulation.FullyOutsideRestaurant()){outsideSent=true;Emit("exit",exitSource,"wall",crab.Root.position,Vector3.up,simulation.Body.linearVelocity,1);}
 }
 void TickEffects(float dt){if(FeelOnR27||!effects)return;effects.ObserveCharge(crab.Root.position,spinCharge.Elapsed,spinCharge.Energy,displaySpinAngle,phase=="ready"&&spinCharge.Started,spinCharge.Ready);effects.Tick(dt);if(cityVisuals){var old=cityVisuals.GetComponent<CrabCityDensity.CityParallaxBands>();if(old)old.Apply();var newer=cityVisuals.GetComponent<CrabCityDensityR2.CityParallaxBands>();if(newer)newer.Apply();}}
 void AddCityVisuals(){
  if(cityLayout>0){RestaurantVisuals.DensityEnabled=false;cityVisuals=City2Data.Build(cityLayout,false);cityVisuals.transform.SetParent(transform,false);Street3Presentation.Build(cityVisuals.transform,groundShader);if(useCityDetailR6&&cityLayout==2&&!useWorldArtR9)CityDetailR6.Build(cityVisuals.transform,groundShader);if(useCoreDestruction&&!useWorldArtR9)CityDistrictArtR7.Build(cityVisuals.transform,groundShader);if(useWorldArtR9)DistrictWorldArtR9.BuildBackground(cityVisuals.transform,groundShader);BuildFarWorldR10();foreach(float side in new[]{-1f,1f})CrabModel.Shape("City2 broad district ground",cityVisuals.transform,PrimitiveType.Cube,new Vector3(side*(useOverdrive?800:208),-.19f,9),new Vector3(useOverdrive?1560:364,.30f,useResultOverview?230:26),new Color(.38f,.42f,.42f));CrabCityDensityR2.CityParallaxBands.Attach(cityVisuals,cam);return;}
  RestaurantVisuals.DensityEnabled=!CityEnabled;if(!CityEnabled)return;
  cityVisuals=CrabCityDensity.CityDensityPack.Build(Resources.Load<TextAsset>("CityDensity/CityGeometry"),Resources.Load<TextAsset>("CityDensity/CityPlacement"),groundShader,CrabCityDensity.CityDensityPack.Detail.Balanced);
  cityVisuals.transform.SetParent(transform,false);CrabCityDensity.CityParallaxBands.Attach(cityVisuals,cam);
 }
}

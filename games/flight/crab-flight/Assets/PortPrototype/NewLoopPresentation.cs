using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// ON-only render tree. Never scales the crab's collision root or consumes simulation RNG.
public sealed partial class CrabFlightGame {
 float newLoopRotations,newLoopShowRps,newLoopTierAt=-100,newLoopMaxAt=-100,newLoopSuction;
 int newLoopHighestTier=1,newLoopSuctionSerial,newLoopMaxCue;
 const int NewLoopSuctionLimit=64;
 readonly MeshRenderer[] newLoopMotes=new MeshRenderer[NewLoopSuctionLimit];
 readonly float[] newLoopMoteBorn=new float[NewLoopSuctionLimit],newLoopMoteAngle=new float[NewLoopSuctionLimit];
 MeshRenderer newLoopGoldAura;LineRenderer newLoopShockwave;
 CommandBuffer newLoopDimCommands;Material newLoopDimMaterial;Mesh newLoopDimQuad;
 readonly List<Renderer> newLoopDimHero=new List<Renderer>();
 void TickNewLoopChargeShow(float dt){
  newLoopShowRps=spinCharge.Held?(NewLoopMotion?NewLoopRules.VisualSpin(spinCharge.Energy):spinCharge.BodySpinRps):0;
  if(spinCharge.Held)newLoopRotations+=newLoopShowRps*Mathf.Max(0,dt);
  if(spinCharge.Tier<=newLoopHighestTier)return;
  newLoopHighestTier=spinCharge.Tier;
  Emit("charge-tier",spinCharge.Tier.ToString(),"",crab.Root.position,Vector3.up,Vector3.zero,spinCharge.Energy);
  // Tier flashes share the same rolling safety budget as every destruction flash.
  if(NewLoopMotion&&feelGateR27.TryDisplay(Time.realtimeSinceStartupAsDouble,generation+"/charge-tier/"+spinCharge.Tier,true,false))newLoopTierAt=visualClock;
  if(spinCharge.Tier==3){newLoopMaxAt=visualClock;newLoopMaxCue++;}
  Report();
 }
 void ResetNewLoopShow(){
  newLoopRotations=newLoopShowRps=newLoopSuction=0;newLoopHighestTier=1;newLoopMaxCue=newLoopSuctionSerial=0;newLoopTierAt=newLoopMaxAt=-100;
  for(int i=0;i<NewLoopSuctionLimit;i++){newLoopMoteBorn[i]=-100;if(newLoopMotes[i])newLoopMotes[i].enabled=false;}
  if(newLoopGoldAura)newLoopGoldAura.enabled=false;if(newLoopShockwave)newLoopShockwave.enabled=false;
  SetNewLoopDim(0);
 }
 void RefreshNewLoopComfort(){
  if(!useNewLoop)return;
  // Settings often arrive while paused, when Tick deliberately does not run.
  if(!NewLoopMotion||paused){
   newLoopTierAt=newLoopMaxAt=-100;newLoopSuction=0;
   foreach(var mote in newLoopMotes)if(mote)mote.enabled=false;
   if(newLoopGoldAura)newLoopGoldAura.enabled=false;if(newLoopShockwave)newLoopShockwave.enabled=false;
   if(newLoopChargeRing){newLoopChargeRing.widthMultiplier=1;if(feelSettingsR27.preset=="off")newLoopChargeRing.enabled=false;}
   if(neonCrab!=null){neonCrab.Root.localScale=crab.Root.lossyScale;neonCrab.Root.rotation=crab.Root.rotation;}
   SetNewLoopDim(0);
  }
  UpdateNewLoopCamera(0,true);
 }
 void PresentNewLoopShow(float dt){
  bool held=phase=="ready"&&spinCharge.Held&&NewLoopMotion;
  if(held&&neonCrab!=null){
   neonCrab.Root.rotation=Quaternion.AngleAxis((newLoopRotations%1)*360,new Vector3(0,.28f,-.96f).normalized)*Quaternion.Euler(0,90,0);
   float age=visualClock-newLoopTierAt;
   if(age>=0&&age<.18f)neonCrab.Root.localScale*=1+.22f*Mathf.Sin(age/.18f*Mathf.PI);
  }
  if(held){
   newLoopSuction+=dt*(14+82*NewLoopRules.Intensity(spinCharge.Energy));
   for(int n=0;n<NewLoopSuctionLimit&&newLoopSuction>=1;n++,newLoopSuction--){
    int slot=newLoopSuctionSerial++%NewLoopSuctionLimit;
    if(!newLoopMotes[slot])newLoopMotes[slot]=NeonGlowTexture.Quad("Charge inward mote "+slot,transform,Color.white);
    newLoopMoteBorn[slot]=visualClock;newLoopMoteAngle[slot]=newLoopSuctionSerial*2.399963f;
   }
  }else newLoopSuction=0;
  for(int i=0;i<NewLoopSuctionLimit;i++)if(newLoopMotes[i]){
   float t=(visualClock-newLoopMoteBorn[i])/.55f;bool show=held&&t>=0&&t<1;newLoopMotes[i].enabled=show;if(!show)continue;
   float radius=Mathf.Lerp(3.6f+(i%5)*.4f,.12f,t),a=newLoopMoteAngle[i]+t*(7+5*spinCharge.Energy);
   var mote=newLoopMotes[i];mote.transform.SetPositionAndRotation(crab.Root.position+cam.transform.right*Mathf.Cos(a)*radius+cam.transform.up*Mathf.Sin(a)*radius,cam.transform.rotation);
   mote.transform.localScale=Vector3.one*(.18f+.14f*spinCharge.Energy)*(1-.5f*t);mote.sharedMaterial.color=new Color(1,1,1,Mathf.Sin(t*Mathf.PI));
  }
  float maxAge=visualClock-newLoopMaxAt;bool max=NewLoopMotion&&phase=="ready"&&maxAge>=0&&maxAge<.38f;
  if(max&&!newLoopGoldAura)newLoopGoldAura=NeonGlowTexture.Quad("MAX gold aura",transform,new Color(1,.85f,.15f));
  if(newLoopGoldAura){newLoopGoldAura.enabled=max;if(max){
   newLoopGoldAura.transform.SetPositionAndRotation(crab.Root.position+cam.transform.forward*.4f,cam.transform.rotation);
   newLoopGoldAura.transform.localScale=Vector3.one*(4.5f+maxAge*5);newLoopGoldAura.sharedMaterial.color=new Color(1,1,1,1-maxAge/.38f);
  }}
  SetNewLoopDim(max?.15f*Mathf.Sin(maxAge/.38f*Mathf.PI):0);
  bool shock=NewLoopMotion&&phase=="flight"&&newLoopObserved&&newLoopReleaseAge<.45f;
  if(shock&&!newLoopShockwave){newLoopShockwave=Line("Release radial shockwave",PortVisuals.Lantern,.18f);newLoopShockwave.transform.SetParent(transform,false);newLoopShockwave.loop=true;newLoopShockwave.positionCount=64;}
  if(newLoopShockwave){newLoopShockwave.enabled=shock;if(shock){float t=newLoopReleaseAge/.45f,radius=Mathf.Lerp(.3f,NewLoopRules.Reach(launchChargeEnergy),1-(1-t)*(1-t));
   newLoopShockwave.widthMultiplier=1-t;for(int i=0;i<64;i++){float a=i*Mathf.PI*2/64;newLoopShockwave.SetPosition(i,simulation.NewLoopContact+cam.transform.right*Mathf.Cos(a)*radius+cam.transform.up*Mathf.Sin(a)*radius);}
  }}
 }
 void SetNewLoopDim(float opacity){
  if(newLoopDimCommands!=null){cam.RemoveCommandBuffer(CameraEvent.AfterEverything,newLoopDimCommands);newLoopDimCommands.Clear();}
  if(opacity<=0||!cam)return;
  if(newLoopDimCommands==null){
   newLoopDimCommands=new CommandBuffer{name="MAX scene dim; redraw hero unchanged"};
   newLoopDimMaterial=new Material(Resources.Load<Shader>("PortPrototype/NewLoopDim"));
   newLoopDimQuad=new Mesh{name="MAX fullscreen tint"};newLoopDimQuad.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};newLoopDimQuad.triangles=new[]{0,1,2,0,2,3};
  }
  newLoopDimMaterial.SetFloat("_Opacity",opacity);
  newLoopDimCommands.DrawMesh(newLoopDimQuad,Matrix4x4.identity,newLoopDimMaterial);
  // Redraw only visible crab renderers after the tint. No layer/material/collider
  // mutation, no second camera and no hole that leaves the background undimmed.
  newLoopDimHero.Clear();(neonCrab!=null?neonCrab.Root:crab.Root).GetComponentsInChildren(false,newLoopDimHero);
  foreach(var r in newLoopDimHero)if(r.enabled){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i])newLoopDimCommands.DrawRenderer(r,mats[i],i);}
  cam.AddCommandBuffer(CameraEvent.AfterEverything,newLoopDimCommands);
 }
 void DisposeNewLoopShow(){
  if(newLoopDimCommands!=null){if(cam)cam.RemoveCommandBuffer(CameraEvent.AfterEverything,newLoopDimCommands);newLoopDimCommands.Release();newLoopDimCommands=null;}
  if(newLoopDimMaterial)Destroy(newLoopDimMaterial);if(newLoopDimQuad)Destroy(newLoopDimQuad);
  foreach(var mote in newLoopMotes)if(mote)Destroy(mote.sharedMaterial);
  if(newLoopGoldAura)Destroy(newLoopGoldAura.sharedMaterial);
  if(newLoopShockwave)Destroy(newLoopShockwave.sharedMaterial);
 }
}

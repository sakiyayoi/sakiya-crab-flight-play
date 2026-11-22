using System;
using UnityEngine;

[Serializable]public sealed class NeonPrototypeState {
 public string revision=NeonProto.Revision,stage3="spin-vortex-world-v1",stage4=NeonProto.SpinStyle;
 public int legs,glow,sectionGlows,strongGlows,hiddenSourceRenderers;
 public Vector3 visualScale;
 public string stage2,backdropHookOrder;
 public int darkenedMaterials,darkenedRenderers;
 public bool otherRegionsUnchanged,protectedMaterialsUnchanged,allSectorsPresent,spinVortexActive,spinGlow,legacyRotorVisible;
 public int spinBands,spinSparks,ghostMarks,visibleGhosts;
 public bool spinGhostSuppressed,spinBubbleSuppressed;
 public int bubbleMarks,visibleBubbles;
 public NeonSparkAudit sparkAudit;
 public NeonSectorArtInfo[] sectorArt;
 public float spinStrength;
 public float spinCoreBlend;
 public Color coreColor;
 public NeonBackdropLayer[] backdropLayers;
 public NeonBackdropSector[] backdropSectors;
 public NeonSignSector[] signSectors;
}
public sealed partial class CrabFlightGame {
 bool portAtmosphereBuilt;
 NeonSectorArt neonSectorArt;
 NeonCrabVisual neonCrab;NeonSectionArt neonSection;NeonBackdropDarken neonBackdrop;NeonSpinVortex neonVortex;int neonGeneration=-1,neonEvent;
 void AttachNeon(){
  neonVortex?.Dispose();neonVortex=null;neonCrab?.Dispose();neonCrab=null;
  OverdrivePresentation.NeonSpinStyle=NeonProto.Enabled;
  if(!NeonProto.Enabled)return;
  if(neonSection==null)neonSection=new NeonSectionArt(transform,cam);
  if(neonSectorArt==null)neonSectorArt=NeonSectorArt.Build(transform);
  if(neonBackdrop==null&&NeonBackdropDarken.Ready(transform))neonBackdrop=new NeonBackdropDarken(transform);
  if(!portAtmosphereBuilt){PortAtmosphere.Build(transform);portAtmosphereBuilt=true;}
  neonCrab=new NeonCrabVisual(crab,transform,cam);neonVortex=new NeonSpinVortex(transform,crab.Root,cam);neonGeneration=-1;neonEvent=0;
 }
 void TickNeon(float dt){
  if(!NeonProto.Enabled)return;
  if(neonBackdrop==null&&NeonBackdropDarken.Ready(transform))neonBackdrop=new NeonBackdropDarken(transform);
  if(spinWake)spinWake.enabled=false;
  if(neonCrab==null)return;
  if(neonGeneration!=generation){neonGeneration=generation;neonEvent=0;neonCrab.Reset();neonVortex.Reset();}
  foreach(var e in eventStream)if(e.id>neonEvent){
   if(e.type=="launch")neonCrab.Launch();else if(e.type=="landing")neonCrab.Land();else if(e.type=="reset"){neonCrab.Reset();neonVortex.Reset();}neonEvent=e.id;
  }
  bool reduced=ReducedMotion||feelSettingsR27.reducedMotion,effects=!FeelOnR27||FeelEffectsR27;
  Vector3 velocity=simulation!=null&&phase=="flight"?simulation.Body.linearVelocity:Vector3.zero;
  neonCrab.Sample(dt,phase,spinCharge.Started?spinCharge.Energy:0,velocity,reduced,effects);neonSection.Tick(dt,reduced,effects);
  neonCrab.SampleSpinColor(dt,phase,currentSpinRps,reduced,effects,aiming||spinCharge.Started);
  neonVortex.Sample(dt,phase,currentSpinRps,displaySpinAngle,reduced,effects,aiming||spinCharge.Started);
 }
 NeonPrototypeState NeonState()=>neonCrab==null||neonBackdrop==null?null:new NeonPrototypeState{sectorArt=neonSectorArt.Sectors.ToArray(),sparkAudit=neonVortex.Audit,spinBubbleSuppressed=feelDrawingR27&&feelDrawingR27.NeonSpinBubbleSuppressed,bubbleMarks=feelDrawingR27?feelDrawingR27.NeonBubbleMarks:0,visibleBubbles=feelDrawingR27?feelDrawingR27.NeonVisibleBubbles:0,stage2=neonBackdrop.Audit.stage2,backdropHookOrder=neonBackdrop.Audit.hookOrder,darkenedMaterials=neonBackdrop.Audit.darkenedMaterials,darkenedRenderers=neonBackdrop.Audit.darkenedRenderers,otherRegionsUnchanged=neonBackdrop.Audit.otherRegionsUnchanged,protectedMaterialsUnchanged=neonBackdrop.Audit.protectedMaterialsUnchanged,backdropLayers=neonBackdrop.Audit.layers,backdropSectors=neonBackdrop.Audit.sectors,allSectorsPresent=neonBackdrop.Audit.allSectorsPresent,signSectors=neonSection.Sectors,legs=neonCrab.LegCount,glow=neonCrab.GlowCount+neonSection.GlowCount,sectionGlows=neonSection.GlowCount,strongGlows=neonSection.StrongCount,hiddenSourceRenderers=neonCrab.HiddenSourceCount,visualScale=neonCrab.Root.localScale,spinVortexActive=neonVortex.Active,spinBands=neonVortex.Bands,spinGlow=neonVortex.GlowVisible,legacyRotorVisible=feelDrawingR27&&feelDrawingR27.RotorVisible,spinSparks=neonVortex.Sparks,spinStrength=neonVortex.Strength,spinCoreBlend=neonCrab.SpinCoreBlend,coreColor=neonCrab.CoreColor,spinGhostSuppressed=feelDrawingR27&&feelDrawingR27.NeonSpinGhostSuppressed,ghostMarks=feelDrawingR27?feelDrawingR27.NeonGhostMarks:0,visibleGhosts=feelDrawingR27?feelDrawingR27.NeonVisibleGhosts:0};
#if UNITY_EDITOR
 public void ReviewStage45Isolation(bool coreRed=false,bool noVortexGlow=false,bool noCrabGlow=false){neonCrab.ReviewIsolation(coreRed,noCrabGlow);neonVortex.ReviewIsolation(noVortexGlow);}
 public NeonBackdropAudit ReviewBackdropAudit=>neonBackdrop.Audit;
 public bool ReviewOldSpinWakeEnabled=>spinWake.enabled;
 public void ReviewMallBackdropDark(bool enabled)=>neonBackdrop.ReviewMallDark(enabled);
 public void ReviewBackdropDark(bool enabled)=>neonBackdrop.ReviewDark(enabled);
 public void ReviewSampleNeon(float dt,string pose,float charge,Vector3 velocity){neonCrab.Sample(dt,pose,charge,velocity,false,true);neonSection.Tick(dt,false,true);}
 public void ReviewSampleVortex(float dt,string pose,float spin,float angle,bool reduced,bool effects,bool eligible=false){neonVortex.Sample(dt,pose,spin,angle,reduced,effects,eligible);neonCrab.SampleSpinColor(dt,pose,spin,reduced,effects,eligible);}
 public void ReviewSpinVortex(float spinRps){
  cam.aspect=1.6f;cam.orthographicSize=4.4f;cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
  crab.Root.position=new Vector3(79,5,0);cam.transform.position=crab.Root.position-cam.transform.forward*22;
  neonVortex.Reset();
  for(int i=0;i<30;i++){ReviewSampleNeon(1f/60,"flight",0,new Vector3(30,0,0));ReviewSampleVortex(1f/60,"flight",spinRps,i*spinRps*6,false,true);}
 }
 public void ReviewNeonLaunch()=>neonCrab.Launch();
 public void ReviewNeonLand()=>neonCrab.Land();
#endif
}

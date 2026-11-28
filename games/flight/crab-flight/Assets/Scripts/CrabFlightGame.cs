using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
[Serializable] public class FlightState {public FeelTelemetryR27 feel;public NeonPrototypeState neon;
 public int supportGeometryEntries,supportGeometryReads;public int worldBirds,worldPeople,worldStartled,peopleHit,birdsHit;public float worldLifeAge;public bool farWorld;public int floorContactExtraSteps,floorContactClampedSteps;public float floorSweepRadius,floorMinBottom;public string floorDiagnostics,rotorDiagnostics;public FlightLaunchReceipt launchReceipt;public string firstContactProbe;public bool ceilingUnlocked,ceilingBreakthrough,ceilingExited;
 public FlightInputAckR23[] inputAcks;public string sourceIdentity,productVersion,launchRejectReason;public Vector3 cameraPosition,cameraRight,cameraUp;public float cameraAspect;public Vector3 actorViewport;public int gear,renderedFrameCount;public double frameRealtime;public bool floorSampled;public float minHeight,groundStuckSeconds;public string throwAudit,resourceAudit;public bool throwAuditChanged;public string revision=Application.version,ruleVersion=SwipeFlightRules.RuleVersion,phase,message,stage,eventName,lastGrade,surface;
 public PortPreview portPreview;public FlightResultOverview resultOverview;public FragmentVisibilityState fragmentVisibility;public float distance,firstContact=-1,height,peak,best,previousBest,bestDelta,time,angle,power,claw,shell,legs,speed,groundTime,spinRps,visualSpinAngle,worldX;
 public int startupShaderVariants,startupUnsupportedPasses;public int automaticGroundBounces,automaticGroundLimit;public float startupShaderWarmupMs;public bool startupRenderingReady;
 public bool runtimeReady,newRecord,reducedMotion,canKick,braced,paused,aiming,canSwipeLaunch;
 public string contactPart="",physicsFault="";public float impactImpulse;public float bodySpinRps,requestedSpinRps;public float chargeWork;public float chargeElapsed,chargeEnergy;public bool chargeStarted,chargeReady;
 public int rechargeCount;public float reclaimedRotorJ,lastReclaimJ;public int burstCount;public float rotorRemaining,rotorCapacity,lastBurstGain;
 public bool widerCamera,auraVisible;public int tracePoints,auraPoints;public float cameraSize,rotorPulseCapacity;public int upgradeLevel;public string district="restaurant";public float chargeLaunchFloor;public float hitStopRemaining,hitStopSeconds;public int hitStopRequests;public int contactKind;public int furnitureBroken,equipmentBroken,foodBroken;public int glassBroken,wallsBroken,dishesBroken,destructionEvent,fragments,foodFragments;public string lastBroken="";
 public bool compoundFrozen;public float compoundEnvelopeError,compoundBoundsDifference;public int cityLayout,cityTargets,chainBreaks,cityDecorative;public int generation;public FlightGameEvent[] events;public int vfxActive,cityFragments;
 public ObjectCaptionState objectCaption;
 public ObjectBadgeState[] objectBadges;public ObjectCaptureState objectCapture;
 public string objectMode="legacy";public int jetsRemaining=2,maxChain;public bool diving,spinActive,objectDeath;
 public int direction=1;
 public int attempts,kicksRemaining=3,kicksUsed,perfects,goods,earlyMisses,impacts,eventId;
}
public sealed partial class CrabFlightGame : MonoBehaviour {
 public int cityLayout=2;public float secondaryThresholdFraction=.25f;public static int ActiveCityLayout=2;public Shader cityR2Shader;public Texture2D cityR2Albedo,cityR2Metal;
 public bool useCoreDestruction=true,useCoreHitFeedback=true;
 public bool useSpectacleCrab=true,useCityDetailR6=true;public bool useOverdrive=true,useOverdriveRoute=true;public bool useFragmentVisibility=true;public Shader fragmentFadeShader;public bool useRotorBursts=true;public bool useReadableSurfaces=true;public bool useCeilingRicochet=true;public bool useHitStop=true;readonly ImpactStop hitStop=new ImpactStop();
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightReport(string json);
#endif
 public Shader crabShader,groundShader;
 public FlightSettings settings=new FlightSettings();
 CrabModel crab;Camera cam;LineRenderer aim,trail,landingRing,spinWake;Transform landingFlag,bestFlag,shadow,aimMarker;
 readonly List<Vector3> trailPoints=new List<Vector3>();
 readonly SpinCharge spinCharge=new SpinCharge();
 RigidCrabFlight world,simulation;string phase="ready",message="角度と勢いを決めて、飛ばそう";
 float best,previousBest,lastDistance,lastPeak,reportClock,accumulator,landingClock,chargeClock,restHeight=.08f,visualClock;
 int attempts,lastEvent=-1,lastImpact=-1;bool newRecord,initialized,paused,aiming;float launchTargetPower=-1,minimumLaunchPower,beforeAimAngle,beforeAimPower,launchSpinRps,launchChargeEnergy,chargeLiftHeight,currentSpinRps,spinAngle,displaySpinAngle,lastSpinInput;int direction=1,beforeAimDirection=1;bool previewEligible;
 public bool useWiderFlightCamera=true;public bool useFlightAura=true;
 readonly List<float> trailTimes=new List<float>();
 public Camera ReviewCamera=>cam;
#if UNITY_EDITOR
 public CrabModel ReviewCrab=>crab;
#endif
 public RigidCrabFlight ReviewSimulation=>simulation;public bool Finished=>phase=="result";
 public bool ReducedMotion {get;private set;}
#if UNITY_EDITOR
 public void ShutdownReviewPhysics(){CancelPreparationR18();var ownedWorld=world;world=null;if(ownedWorld!=null)ownedWorld.Dispose();contactVisualTemplateR15.Dispose();lifeGeometryCacheR17.Dispose();}
 public void ReviewSetFoodFragments(bool enabled){world.UseCC0FoodFragments=enabled;}
 public void RefreshReviewCamera(){if(initialized)UpdateCamera(1,true);}
#endif
 bool startupRenderingReadyR12;float startupShaderWarmupMsR12;
 void Start(){Initialize();}
 readonly ContactVisualTemplateR15 contactVisualTemplateR15=new ContactVisualTemplateR15();
 readonly CrabLifeActors.LifeGeometryCacheR17 lifeGeometryCacheR17=new CrabLifeActors.LifeGeometryCacheR17();
 public void Initialize(){
  if(initialized)return;initialized=true;CrabSculptR6.Enabled=useSpectacleCrab;CrabSurfaceBinding.DetailR6=useSpectacleCrab;City2Data.UseCoreLayout=useCoreDestruction;City2Data.UseAtriumRoute=useAtriumRoute;City2Data.UseWorldArtR9=useWorldArtR9;City2Data.UseFarWorldR10=useFarWorldR10;City2Data.UseRouteBlockoutR11=useRouteBlockoutR11;City2Data.UseProgressionR12=useProgressionR12;City2Data.GlassShaderR10=fragmentFadeShader;hitStop.CoreFeedback=useCoreHitFeedback;spinCharge.ContinuousWork=useRotorBursts;spinCharge.EnhancedSpinR16=EnhancedSpinR16&&useOverdrive;spinCharge.PowerScale=useOverdrive?28f/22:1;spinCharge.BodyLimit=useOverdrive?(EnhancedSpinR16?RigidCrabFlight.EnhancedBodySpinLimitR16:28):22;ActiveCityLayout=cityLayout;RigidCrabFlight.SecondaryThresholdFraction=Mathf.Clamp(secondaryThresholdFraction,.1f,1);if(cityLayout>0)City2Data.Bind(cityR2Shader,cityR2Albedo,cityR2Metal);settings.Sanitize();CrabModel.SurfaceShader=crabShader;CrabModel.ArenaShader=groundShader;
  Application.targetFrameRate=45;QualitySettings.shadows=ShadowQuality.Disable;
  if(useSpectacleCrab){RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.45f,.52f,.60f);RenderSettings.ambientEquatorColor=new Color(.34f,.38f,.39f);RenderSettings.ambientGroundColor=new Color(.32f,.29f,.25f);}else{RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.68f,.78f,.82f);}
  cam=new GameObject("Flight camera").AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6;cam.nearClipPlane=.1f;cam.farClipPlane=700;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.35f,.65f,.71f);
  var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(1,.93f,.78f);sun.intensity=useSpectacleCrab?1.35f:1.2f;sun.transform.rotation=Quaternion.Euler(48,-35,0);
  if(useSpectacleCrab){var fill=new GameObject("Crab bounce fill").AddComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(.70f,.80f,.90f);fill.intensity=.38f;fill.shadows=LightShadows.None;fill.transform.rotation=Quaternion.Euler(-32,135,0);}
  if(effectsPrefab){effects=Instantiate(effectsPrefab);effects.transform.SetParent(transform,false);effects.manualTick=true;effects.Ensure();}world=new RigidCrabFlight(){CityVisualTemplateR15=contactVisualTemplateR15,LifeGeometryCacheR17=lifeGeometryCacheR17,UseLandingPushR27=true,UseEnhancedSpinR16=EnhancedSpinR16,UseProgressionR12=useProgressionR12,UseRouteBlockoutR11=useRouteBlockoutR11,UseFloorContactSteps=useFloorContactSteps,UseCeilingGrowth=useCeilingGrowth,UseCoreDestruction=useCoreDestruction,UseOverdrive=useOverdrive,UseRotorBursts=useRotorBursts,UseCeilingRicochet=useCeilingRicochet,UseReadableSurfaces=useReadableSurfaces,UseFragmentVisibility=useFragmentVisibility,FragmentFadeShader=fragmentFadeShader};MakeField();world.EnableOverdriveWorld();world.EnableRouteFloorR11();world.EnableRestaurant();if(useOverdrive&&useOverdriveRoute)world.EnableOverdriveRestaurantRoute();if(useCoreDestruction)world.EnableCoreDistrictContacts();Rebuild();Prepare("");SetupOverdrivePresentation();
#if UNITY_WEBGL && !UNITY_EDITOR
  double warmupStart=Time.realtimeSinceStartupAsDouble;WarmSceneShaderVariantsR12();startupShaderWarmupMsR12=(float)((Time.realtimeSinceStartupAsDouble-warmupStart)*1000);
#endif
  startupRenderingReadyR12=true;Report();
 }
 public void Configure(string json){
 int traceConfigureR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.Configure,Time.renderedFrameCount);try{
  Initialize();if(phase=="flight"||phase=="charge"||aiming)return;var s=JsonUtility.FromJson<FlightSettings>(json);if(s==null)return;s.Sanitize();bool changed=Mathf.Abs(s.claw-settings.claw)>.001f||Mathf.Abs(s.shell-settings.shell)>.001f||Mathf.Abs(s.legs-settings.legs)>.001f||s.gear!=settings.gear;
  AuditInput("Configure",json);settings=s;if(changed)Rebuild();if(phase=="result")phase="ready";PreviewAim();Report();
 }finally{PrepareEnvelopeR18.Exit(traceConfigureR18,Time.renderedFrameCount);}
}
 public void RestoreBest(string value){Initialize();if(float.TryParse(value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out float v))best=Mathf.Max(best,FlightSettings.Safe(v,0,10000));PlaceBest();Report();}
 public void SetReducedMotion(string value){Initialize();AuditInput("SetReducedMotion",value);ReducedMotion=value=="1";hitStop.SetReducedMotion(ReducedMotion);if(effects)effects.SetReducedMotion(ReducedMotion);ResetOverdrivePresentation();Report();}
 public void KickDown(string _){if(!paused&&phase=="flight"&&simulation!=null){simulation.SetKickPressed(true);RefreshMessage();Report();}}
 public void KickUp(string _){if(simulation!=null)simulation.SetKickPressed(false);}
 // Preview input never creates or advances a flight. Release is a separate explicit command.
 public void BeginSpinCharge(string _){Initialize();if(!paused&&phase=="ready"){AuditInput("BeginSpinCharge",_);if(!spinCharge.Started)chargeLiftHeight=.5f+world.RotationRadius();spinCharge.Begin();}Report();}
 public void AddSpinWork(string json){if(paused||phase!="ready")return;var work=JsonUtility.FromJson<SpinWorkInput>(json);if(work==null)return;AuditInput("AddSpinWork",json);spinCharge.Add(work.distance,work.seconds);if(spinCharge.Started&&!spinCharge.Ready&&work.distance>0&&work.seconds>0&&visualClock-lastWorkCue>=.075f){lastWorkCue=visualClock;Emit("charge","stroke","",crab.Root.position,Vector3.up,Vector3.zero,spinCharge.Energy);}Report();}
 public void BeginAim(string _){Initialize();if(paused||phase!="ready"||aiming)return;AuditInput("BeginAim",_);beforeAimAngle=settings.angle;beforeAimPower=settings.power;beforeAimDirection=direction;if(!spinCharge.Started){spinAngle=displaySpinAngle=0;currentSpinRps=launchSpinRps=0;}previewEligible=false;aiming=true;lastSpinInput=visualClock;message="上向きにスワイプ！ 速さで回って、その向きへ";PreviewAim();Report();}
 public void PreviewLaunch(string json){if(paused||phase!="ready"||!aiming)return;var input=JsonUtility.FromJson<FlightAimInput>(json);if(input==null)return;AuditInput("PreviewLaunch",json);settings.angle=FlightSettings.Safe(input.angle,10,80);settings.power=FlightSettings.Safe(input.power,0,100);direction=input.direction<0?-1:1;launchSpinRps=direction*(float.IsNaN(input.spinRps)||float.IsInfinity(input.spinRps)?0:Mathf.Abs(Mathf.Clamp(input.spinRps,-SwipeFlightRules.MaxSpinRps,SwipeFlightRules.MaxSpinRps)));if(!spinCharge.Started){currentSpinRps=launchSpinRps;spinAngle=displaySpinAngle=FlightSettings.Safe(input.spinPhase,-360,360);}lastSpinInput=visualClock;previewEligible=input.eligible;PreviewAim();PlaceBest();Report();}
 public void CancelAim(string _){if(!aiming)return;AuditInput("CancelAim",_);settings.angle=beforeAimAngle;settings.power=beforeAimPower;direction=beforeAimDirection;aiming=false;if(!spinCharge.Started){spinAngle=displaySpinAngle=0;crab.Root.rotation=Quaternion.Euler(0,90,0);}currentSpinRps=launchSpinRps=spinCharge.Started?spinCharge.SpinRps*direction:0;previewEligible=false;PreviewAim();PlaceBest();Report();}
 public void ReleaseLaunch(string json){if(paused||phase!="ready"||!aiming)return;AuditInput("ReleaseLaunch",json);PreviewLaunch(json);if(!spinCharge.Ready||!previewEligible){CancelAim("");return;}launchTargetPower=settings.power;minimumLaunchPower=0;settings.power=CoreLaunchPower(settings.power,spinCharge.Energy,useCoreDestruction);launchSpinRps=direction*spinCharge.SpinRps;launchChargeEnergy=spinCharge.Energy;spinAngle=displaySpinAngle;aiming=false;requestedLaunchSource="release";Launch("");}
 public static float CoreLaunchPower(float flick,float energy,bool active){// G-POWER: an accepted release names the launch target, not a pre-charge base.
  // Charge still sets rotor work/spin; the ready gate remains in ReleaseLaunch.
  return active?flick:flick*(.18f+.82f*energy);}
 public void SetUpgradeLevel(string text){Initialize();if(phase=="flight"||aiming||(phase=="ready"&&spinCharge.Started))return;if(int.TryParse(text,out int level)){AuditInput("SetUpgradeLevel",text);settings.upgradeLevel=Mathf.Clamp(level,0,3);}Report();}
 public void RetryLaunch(string _){if(paused||phase!="result")return;float retainedPhase=spinAngle,retainedSpin=launchSpinRps,retainedCharge=launchChargeEnergy,retainedTarget=launchTargetPower;Prepare("");launchTargetPower=retainedTarget;launchSpinRps=retainedSpin;launchChargeEnergy=retainedCharge;crab.ChargePose(retainedCharge);spinAngle=displaySpinAngle=retainedPhase;requestedLaunchSource="retry";Launch("");}
 public void SetPaused(string value){if(prepareWorkR18!=null){preparePauseR18=value=="1";if(prepareViewR18!=null)prepareViewR18.paused=preparePauseR18.Value;Report();return;}Initialize();AuditInput("SetPaused",value);bool next=value=="1";if(next)CancelAim("");paused=next;if(next&&FeelOnR27){BeginFeelFrameR27();feelDrawingR27.Clear();}if(effects)effects.SetPaused(next);if(simulation!=null){simulation.SetKickPressed(false);simulation.ObjectRun.Release();}Report();}
 public void BeginCharge(string _){BeginSpinCharge("");}
 public void Launch(string _){
  if(paused||phase=="flight")return;Initialize();BeginFeelFrameR27();aiming=false;currentSpinRps=launchSpinRps;settings.Sanitize();simulation=world;phase="flight";AuditInput("Launch",requestedLaunchSource);accumulator=0;attempts++;
  ResetResultOverview();previousBest=best;newRecord=false;lastDistance=lastPeak=0;landingClock=0;lastEvent=lastImpact=-1;message="ガラス・食器・壁へ！ 接触の勢いで壊れる";
  trailPoints.Clear();trailTimes.Clear();trail.positionCount=0;aim.enabled=false;if(aimMarker)aimMarker.gameObject.SetActive(false);landingFlag.gameObject.SetActive(false);var joints=LaunchJointBits();simulation.FirstContactProbe="";simulation.Launch(settings,direction,launchSpinRps,Quaternion.AngleAxis(spinAngle,new Vector3(0,.28f,-.96f).normalized)*Quaternion.Euler(0,90,0));launchReceipt=new FlightLaunchReceipt{source=requestedLaunchSource,serial=attempts,settingsJson=JsonUtility.ToJson(settings),angle=settings.angle,targetPower=launchTargetPower>=0?launchTargetPower:settings.power,power=settings.power,direction=direction,upgrade=settings.upgradeLevel,spin=launchSpinRps,charge=launchChargeEnergy,phase=spinAngle,reduced=ReducedMotion,jointBits=LaunchFingerprint.Full(joints),jointSHA256=LaunchFingerprint.Hash(joints),bodyJson=simulation.LaunchBodyReceipt()};AuditLaunch();Emit("launch","","",crab.Root.position,Vector3.up,simulation.Body.linearVelocity,launchChargeEnergy);UpdateCamera(1,true);Report();
 }
 public void Prepare(string _){foreach(var unit in PrepareWorkR18(false)){} }
 IEnumerable<object> PrepareWorkR18(bool split){
  PrepareCostProbeR15.Begin(phase,attempts);Initialize();BeginFeelFrameR27();AuditPrepare();PrepareCostProbeR15.Mark("prepare.audit");if(simulation!=null){crab.Dispose();PrepareCostProbeR15.Mark("prepare.crabDispose");world.Dispose();PrepareCostProbeR15.Mark("prepare.worldDispose");world=new RigidCrabFlight(){CityVisualTemplateR15=contactVisualTemplateR15,LifeGeometryCacheR17=lifeGeometryCacheR17,UseLandingPushR27=true,UseEnhancedSpinR16=EnhancedSpinR16,UseProgressionR12=useProgressionR12,UseRouteBlockoutR11=useRouteBlockoutR11,UseFloorContactSteps=useFloorContactSteps,UseCeilingGrowth=useCeilingGrowth,UseCoreDestruction=useCoreDestruction,UseOverdrive=useOverdrive,UseRotorBursts=useRotorBursts,UseCeilingRicochet=useCeilingRicochet,UseReadableSurfaces=useReadableSurfaces,UseFragmentVisibility=useFragmentVisibility,FragmentFadeShader=fragmentFadeShader};PrepareCostProbeR15.Mark("prepare.worldConstructor");world.EnableOverdriveWorld();PrepareCostProbeR15.Mark("prepare.overdriveWorld");world.EnableRouteFloorR11();PrepareCostProbeR15.Mark("prepare.routeFloor");yield return null;if(split){foreach(var unit in contactVisualTemplateR15.StageCopyR18())yield return unit;foreach(var unit in world.RestaurantWorkR18())yield return unit;}else world.EnableRestaurant();PrepareCostProbeR15.Mark("prepare.restaurant");if(useOverdrive&&useOverdriveRoute)world.EnableOverdriveRestaurantRoute();PrepareCostProbeR15.Mark("prepare.restaurantRoute");if(useCoreDestruction)world.EnableCoreDistrictContacts();PrepareCostProbeR15.Mark("prepare.coreTargets");crab=null;yield return PrepareBoundaryR18.Value;MakeCrabR18();yield return PrepareBoundaryR18.Value;AttachCrabR18();yield return PrepareBoundaryR18.Value;PrepareCostProbeR15.Mark("prepare.afterRebuild");simulation=null;}if(aiming){settings.angle=beforeAimAngle;settings.power=beforeAimPower;direction=beforeAimDirection;}aiming=false;currentSpinRps=0;previewEligible=false;paused=false;if(simulation!=null)simulation.SetKickPressed(false);phase="ready";launchTargetPower=-1;minimumLaunchPower=0;spinCharge.Reset();launchSpinRps=launchChargeEnergy=0;launchReceipt=null;lastDistance=lastPeak=0;crab.ChargePose(0);world.ConfigureObjects(feelR27!=null&&feelR27.UsesObjectRedesign);world.Reset();PrepareCostProbeR15.Mark("prepare.readyReset");simulation=null;spinAngle=displaySpinAngle=0;accumulator=landingClock=0;newRecord=false;message="生簀から、店の壁をぶち抜こう！";
  ResetResultOverview();trailPoints.Clear();trailTimes.Clear();trail.positionCount=0;crab.Root.position=new Vector3(0,restHeight+.4f,0);crab.Root.rotation=Quaternion.Euler(0,90,0);shadow.position=new Vector3(0,.045f,0);landingRing.enabled=false;StreamReset();ResetOverdrivePresentation();PreviewAim();UpdateCamera(1,true);PrepareCostProbeR15.Mark("prepare.presentationReset");Report();PrepareCostProbeR15.End();
 
 }
 void Rebuild(){MakeCrabR18();AttachCrabR18();}
 void MakeCrabR18(){PrepareCostProbeR15.Mark("rebuild.enter");if(crab!=null)crab.Dispose();PrepareCostProbeR15.Mark("rebuild.dispose");crab=CrabModel.Make("FlightCrab",settings,Color.white);PrepareCostProbeR15.Mark("rebuild.modelMake");
#if !CRAB_KING_BOLD_R2
ReadableMaterials.Actor(crab.Root,useReadableSurfaces);
#endif
crab.Root.position=Vector3.zero;crab.Root.rotation=Quaternion.Euler(0,90,0);float minY=float.PositiveInfinity;foreach(var renderer in crab.Root.GetComponentsInChildren<MeshRenderer>())minY=Mathf.Min(minY,renderer.bounds.min.y);restHeight=.04f-minY;crab.Root.position=new Vector3(0,restHeight,0);
#if !CRAB_KING_BOLD_R2
CrabHeroVisualR2.Attach(crab);CrabGearR29.Apply(crab,settings.gear);
#endif
PrepareCostProbeR15.Mark("rebuild.poseAndBounds");}
 void AttachCrabR18(){world.Attach(crab,settings,restHeight);AttachNeon();PrepareCostProbeR15.Mark("rebuild.afterAttach");}
 void Update(){SyncStampR8(0);feelRealDeltaR27=Time.unscaledDeltaTime;Tick(Mathf.Min(Time.unscaledDeltaTime,.08f));feelRealDeltaR27=-1;SyncStampR8(1);}
 void LateUpdate(){SyncStampR8(5);neonCrab?.SyncTankVisibility(phase);LateObjectReadability();}
 public void Tick(float dt){
  if(prepareWorkR18!=null){AdvancePrepareFrameR18();return;}
  if(!initialized||paused)return;PrimitiveColliderTraceR18.BeforeMain(world.Scene,world.Time,"frame");BeginCalmFrameR3(dt);BeginFeelFrameR27();AuditTick(dt);float gameplayDt=FeelOnR27?FilterFeelR27(dt):phase=="flight"&&useHitStop?hitStop.Filter(dt):dt;visualClock+=dt;
  if(phase=="ready"&&spinCharge.Started){spinCharge.Tick(dt);currentSpinRps=direction*spinCharge.SpinRps;displaySpinAngle=(displaySpinAngle+direction*spinCharge.BodySpinRps*360*dt)%360;crab.ChargePose(spinCharge.Energy);crab.Root.position=Vector3.Lerp(crab.Root.position,new Vector3(0,chargeLiftHeight,0),1-Mathf.Exp(-dt*5));}
  if(phase=="flight"){
   accumulator+=gameplayDt;while(accumulator>=RigidCrabFlight.StepSeconds&&phase=="flight"){
    accumulator-=RigidCrabFlight.StepSeconds;long calmStepStart=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;simulation.ObjectReducedMotion=ReducedMotion||feelSettingsR27.reducedMotion||!FeelEffectsR27;simulation.Step();if(CalmSupportPerfR3.Active){calmFrameR3.steps++;calmFrameR3.stepMs+=CalmSupportPerfR3.Ms(CalmSupportPerfR3.Stamp()-calmStepStart);}ObserveResultTravel();long calmEventStart=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;DrainPhysicsEvents();if(CalmSupportPerfR3.Active)calmFrameR3.eventMs+=CalmSupportPerfR3.Ms(CalmSupportPerfR3.Stamp()-calmEventStart);lastDistance=simulation.Distance;lastPeak=simulation.Peak;
    if(!simulation.ObjectRun.Dead&&(FeelOnR27?feelClockR27.HoldRemaining>0:useHitStop&&hitStop.Remaining>0)){accumulator=0;break;}
    if(simulation.ImpactID!=lastImpact){if(simulation.ImpactID>0)landingClock=.24f;lastImpact=simulation.ImpactID;}RefreshMessage();
    if(simulation.Finished){if(effects)effects.EndRun(false);phase="result";newRecord=simulation.AbortReason.Length==0&&lastDistance>best+.05f;if(simulation.AbortReason.Length==0)best=Mathf.Max(best,lastDistance);message=simulation.AbortReason.Length>0?"物理試作を安全停止しました":newRecord?"自己ベスト！":"接触姿勢を変えて、もう一投！";landingFlag.position=new Vector3(direction*lastDistance,.03f,useReadableSurfaces?2.7f:-1.9f);landingFlag.gameObject.SetActive(true);PlaceBest();BeginResultOverview();AuditFinish();Report();}
   }
   currentSpinRps=direction*simulation.SpinRps;displaySpinAngle=crab.Root.eulerAngles.z;
   if(!useFlightAura){var point=crab.Root.position+Vector3.up*.45f;if(trailPoints.Count==0||Vector3.Distance(trailPoints[trailPoints.Count-1],point)>.25f){trailPoints.Add(point);if(trailPoints.Count>90)trailPoints.RemoveAt(0);trail.positionCount=trailPoints.Count;trail.SetPositions(trailPoints.ToArray());}}
  }
  if(phase=="ready"&&aiming&&!spinCharge.Started&&visualClock-lastSpinInput>.10f){currentSpinRps*=Mathf.Exp(-dt*18);previewEligible=false;aim.enabled=false;aimMarker.gameObject.SetActive(false);}

  if(phase=="ready"&&(aiming||spinCharge.Started)){crab.Root.rotation=Quaternion.AngleAxis(displaySpinAngle,new Vector3(0,.28f,-.96f).normalized)*Quaternion.Euler(0,90,0);}
  // Rigid-body contacts use the same frozen limb pose as the rendered model.
  // Reduced motion disables secondary wind/trail, never changes collider orientation.
  if(useFlightAura)UpdateFlightTrace();
  landingClock=Mathf.Max(0,landingClock-dt);trail.enabled=!ReducedMotion;
  if(phase=="ready"&&!aiming&&!spinCharge.Started)crab.Root.rotation=Quaternion.Slerp(crab.Root.rotation,Quaternion.Euler(0,90,0),Mathf.Clamp01(dt*8));
  shadow.position=new Vector3(crab.Root.position.x,.045f,0);float elevation=phase=="flight"?(float)simulation.Y:0,size=1.8f+Mathf.Min(elevation*.04f,1.6f);shadow.localScale=new Vector3(size,.008f,size*.7f);
  long calmPresentStart=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;TickEffects(dt);TickFarWorldR10(gameplayDt);world.TickLifeContactsR6(gameplayDt,crab.Root.position.x,ReducedMotion,feelSettingsR27.preset!="off");UpdateSpinWake();UpdateLandingRing();UpdateCamera(useCoreHitFeedback&&phase=="flight"&&hitStop.Remaining>0?0:dt,false);TickOverdrivePresentation(dt);world.TickFragmentVisibility(cam,crab,dt);FinishFeelFrameR27(dt);TickNeon(dt);if(CalmSupportPerfR3.Active)calmFrameR3.presentMs=CalmSupportPerfR3.Ms(CalmSupportPerfR3.Stamp()-calmPresentStart);reportClock+=dt;if(reportClock>.04f){reportClock=0;Report();}EndCalmFrameR3();
 }
 string CoreDistrictName(float x){if(useRouteBlockoutR11)return WorldRouteR11.RegionAt(x);float d=Mathf.Abs(x);return d<28?"restaurant":d<135?"mall":d<600?"office":d<900?"port":"viaduct";}
 void RefreshMessage(){
  if(simulation==null||simulation.EventID==lastEvent)return;lastEvent=simulation.EventID;
  switch(simulation.LastEvent){case "brace":message="着地プッシュを構えた！";break;case "perfect":message="PERFECT！ 接地ぴったり！";break;case "good":message="GOOD！ もうひと跳ね！";break;case "miss":message="早すぎた！ 次は地面の近くで";break;case "land":message=(simulation.LastKind==1?"甲羅":simulation.LastKind==2?"ハサミ":"脚")+"が接触！ まだ転がる！";break;case "settled":message="止まるまでが飛距離！";break;}
 }
 void UpdateCamera(float dt,bool snap){
  // Keep the collision view until death's rendered hold has ended. Actual
  // simulation/result bookkeeping has already finished, unchanged.
  if(ObjectDeathPresentationHeld)return;
  if(UpdateResultCamera(dt,snap))return;
  float y=phase=="flight"?(float)simulation.Y:spinCharge.Started?Mathf.Max(0,crab.Root.position.y-restHeight):0,v=phase=="flight"?(float)simulation.Vx:0;
  // Side-on X axis keeps the launch direction horizontal; elevation reveals the sand.
  float reveal=RestaurantVisuals.ExteriorEnabled?Mathf.SmoothStep(0,1,Mathf.InverseLerp(20,35,Mathf.Abs(crab.Root.position.x))):0;
  float size=(phase=="ready"?3.7f:Mathf.Max(5.8f,(y+5)*.59f))*(1+reveal*.14f)*Mathf.Max(1,.75f/Mathf.Max(cam.aspect,.3f));
  // Grow the field after leaving the restaurant; setup framing remains untouched.
  if(useWiderFlightCamera&&phase=="flight")size*=1+reveal*.18f;
  #if CRAB_KING_BOLD_R2
  if(phase=="ready")size*=cam.aspect<.7f?1.5f:1.12f;
#endif
  float lead=Mathf.Min(phase=="flight"?Mathf.Clamp(1.2f+v*.075f,1.2f,4):0,size*Mathf.Max(cam.aspect,.3f)*.35f);
  float centerY=.7f+y*.47f;
  // Keep the wider ground-flight framing, but retain a readable crab above the roof.
  if(phase=="flight"&&useCeilingGrowth){float maxSize=Mathf.Lerp(36,22,Mathf.InverseLerp(.45f,1.7f,cam.aspect));size=Mathf.Min(size,maxSize);centerY=Mathf.Max(centerY,crab.Root.position.y-size*.42f);}
  bool streetFraming=true;
#if UNITY_EDITOR
  streetFraming=!Street3Presentation.ReviewBaseline;
#endif
  if(streetFraming)centerY=Mathf.Lerp(centerY,Mathf.Max(centerY,size*.70f-1),reveal);
  FrameFlightR19(ref size,ref lead,ref centerY);
  float viewDirection=phase=="flight"?v/Mathf.Max(4,Mathf.Abs(v)):direction;
  Vector3 center=new Vector3((phase=="flight"?simulation.Body.worldCenterOfMass.x:crab.Root.position.x)+viewDirection*lead,centerY,0);
  Vector3 target=center+new Vector3(0,5.3f,-16);
  // Orthographic backoff preserves screen positions, but keeps the bottom rays above the ground plane.
  Vector3 viewR16=new Vector3(0,-5.3f,16).normalized;float bottomR16=target.y-size*16/Mathf.Sqrt(16*16+5.3f*5.3f);
  if(useResultOverview)target-=viewR16*Mathf.Max(0,(.5f-bottomR16)/-viewR16.y);FollowFlightCameraR29(target,size,dt,snap);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16),Vector3.up);
 }
 float RecentBoost(){if(simulation==null||simulation.BurstEvents.Count==0)return 0;var e=simulation.BurstEvents[simulation.BurstEvents.Count-1];return Mathf.Clamp01(1-((float)simulation.Time-e.time)/.22f);}
 void UpdateFlightTrace(){
  if(phase=="flight"){var point=crab.Root.position+cam.transform.forward*.22f;if(trailPoints.Count==0||(trailPoints[trailPoints.Count-1]-point).sqrMagnitude>.09f){trailPoints.Add(point);trailTimes.Add(visualClock);}}
  while(trailPoints.Count>0&&(visualClock-trailTimes[0]>.36f||trailPoints.Count>48||(trailPoints[trailPoints.Count-1]-trailPoints[0]).sqrMagnitude>196)){trailPoints.RemoveAt(0);trailTimes.RemoveAt(0);}
  float energy=simulation!=null?Mathf.Clamp01(simulation.Speed/52):0;trail.startWidth=0;trail.endWidth=.09f+.055f*energy+.03f*RecentBoost();trail.startColor=new Color(1,.76f,.3f,0);trail.endColor=new Color(1,.86f,.53f,.38f+.20f*energy);trail.positionCount=trailPoints.Count;if(trailPoints.Count>0)trail.SetPositions(trailPoints.ToArray());
 }
 // Secondary wake follows the real rigid body; never writes its pose or adds energy.
 void UpdateSpinWake(){
  spinWake.enabled=!(effects&&phase=="ready"&&!useRotorBursts)&&!ReducedMotion&&(phase=="flight"||aiming||spinCharge.Started)&&Mathf.Abs(currentSpinRps)>1;spinWake.startWidth=.04f+.08f*Mathf.Clamp01(Mathf.Abs(currentSpinRps)/22);spinWake.startColor=new Color(1,.80f,.40f);spinWake.endColor=new Color(.80f,.93f,.97f,0);
  if(EnhancedSpinR16&&useFlightAura&&spinWake.enabled){
   Vector3 omega=phase=="flight"&&simulation!=null?simulation.Body.angularVelocity:new Vector3(0,.28f,-.96f).normalized*(direction*spinCharge.BodySpinRps*Mathf.PI*2);
   float speed=omega.magnitude/(Mathf.PI*2),sense=Mathf.Sign(Vector3.Dot(omega,cam.transform.forward));
   Vector3 radial=Vector3.ProjectOnPlane(crab.Root.forward,cam.transform.forward);if(radial.sqrMagnitude<.01f)radial=Vector3.ProjectOnPlane(crab.Root.up,cam.transform.forward);radial.Normalize();
   float headAngle=Mathf.Atan2(Vector3.Dot(radial,cam.transform.up),Vector3.Dot(radial,cam.transform.right));Vector3 tangent=Vector3.Cross(cam.transform.forward,radial)*sense;
   Vector3 center=crab.Root.position+cam.transform.forward*.25f,head=center+radial*2.45f;float spinArcLengthR16=Mathf.Clamp(speed*Mathf.PI*2*.018f,Mathf.PI*.35f,Mathf.PI*1.6f);
   spinWake.startWidth=.075f+.04f*(1-Mathf.Exp(-speed/18));spinWake.endWidth=.008f;spinWake.startColor=new Color(1,.89f,.61f,.55f);spinWake.endColor=new Color(.72f,.94f,1,.015f);
   spinWake.SetPosition(0,head-tangent*.32f+radial*.16f);spinWake.SetPosition(1,head);spinWake.SetPosition(2,head-tangent*.32f-radial*.16f);spinWake.SetPosition(3,head);
   for(int i=4;i<24;i++){float a=headAngle-sense*spinArcLengthR16*(i-3)/20;spinWake.SetPosition(i,center+(cam.transform.right*Mathf.Cos(a)+cam.transform.up*Mathf.Sin(a))*2.45f);}return;
  }
  if(useFlightAura&&phase=="flight"&&spinWake.enabled){
   float spin=Mathf.Clamp01(Mathf.Abs(currentSpinRps)/28),boost=RecentBoost();
   spinWake.startWidth=.085f+.045f*spin+.035f*boost;spinWake.endWidth=.008f;
   spinWake.startColor=new Color(1,.77f,.30f,.32f+.16f*spin+.10f*boost);spinWake.endColor=new Color(1,.83f,.45f,0);
   var center=crab.Root.position+cam.transform.forward*.25f;float auraArcLength=(1.1f+.45f*spin)*Mathf.PI;
   for(int i=0;i<24;i++){float a=displaySpinAngle*Mathf.Deg2Rad-i*auraArcLength/23;spinWake.SetPosition(i,center+(cam.transform.right*Mathf.Cos(a)+cam.transform.up*Mathf.Sin(a))*2.45f);}return;
  }
  if(!spinWake.enabled)return;float length=Mathf.Min(1.9f,Mathf.Log(1+Mathf.Abs(currentSpinRps)/12))*Mathf.PI*1.45f;
  for(int i=0;i<24;i++){float a=displaySpinAngle*Mathf.Deg2Rad-i*length/23;spinWake.SetPosition(i,crab.Root.position+new Vector3(Mathf.Cos(a)*2.3f,.65f,Mathf.Sin(a)*2.3f));}
 }
 void UpdateLandingRing(){
  landingRing.enabled=!useRotorBursts&&phase=="flight"&&simulation.Y>.04;if(!landingRing.enabled)return;float ahead=(float)Math.Min(20,simulation.Vx*simulation.TimeToGround*.8),x=direction*((float)simulation.X+ahead),r=.85f;
  var color=simulation.Braced?new Color(1,.85f,.35f):new Color(.90f,.96f,.91f);landingRing.startColor=landingRing.endColor=color;
  for(int i=0;i<40;i++){float a=i*Mathf.PI*2/40;landingRing.SetPosition(i,new Vector3(x+Mathf.Cos(a)*r,.065f,Mathf.Sin(a)*r));}
 }
 void PreviewAim(){
  if(aim==null)return;var points=new List<Vector3>{new Vector3(0,restHeight,0)};float a=settings.angle*Mathf.Deg2Rad;Vector3 pos=points[0],velocity=new Vector3(direction*(float)settings.LaunchSpeed*Mathf.Cos(a),(float)settings.LaunchSpeed*Mathf.Sin(a),0);
  // Only the first1.25seconds: rigid impacts and rotating support extent remain uncertain.
  for(int i=0;i<300;i++){velocity=(velocity+Vector3.down*9.81f*RigidCrabFlight.StepSeconds)/(1+.035f*RigidCrabFlight.StepSeconds);pos+=velocity*RigidCrabFlight.StepSeconds;if(i%6==0)points.Add(pos);if(pos.y<.5f)break;}
  aim.positionCount=points.Count;aim.SetPositions(points.ToArray());aim.enabled=phase=="ready"&&(!aiming||previewEligible);
  if(aimMarker){float markerAngle=settings.angle*Mathf.Deg2Rad,r=1.2f+settings.power*.025f;aimMarker.position=new Vector3(direction*Mathf.Cos(markerAngle)*r,.4f+Mathf.Sin(markerAngle)*r,0);aimMarker.localScale=Vector3.one*(.16f+settings.power*.0015f);aimMarker.gameObject.SetActive(phase=="ready"&&(!aiming||previewEligible));}
 }

 void PlaceBest(){if(bestFlag){bestFlag.position=new Vector3(direction*best,.03f,2.5f);bestFlag.gameObject.SetActive(best>.05f);}}
 public FlightState State(){
 int traceStateR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.State,Time.renderedFrameCount);try{
  if(prepareViewR18!=null)return PreparationStateR18();
  var s=new FlightState{objectCaption=objectCaption,objectMode=world!=null&&world.UseObjectRedesign?"redesign":"legacy",jetsRemaining=world!=null?world.ObjectRun.Jets:2,maxChain=world!=null?world.ObjectRun.MaxChain:0,diving=world!=null&&world.ObjectRun.Dive,spinActive=world!=null&&world.ObjectRun.Spinning(world.Time),objectDeath=world!=null&&world.ObjectRun.Dead,neon=NeonState(),supportGeometryEntries=world!=null?world.SupportGeometryEntriesR14:0,supportGeometryReads=world!=null?world.SupportGeometryReadsR14:0,startupShaderVariants=startupShaderVariantsR12,startupUnsupportedPasses=startupUnsupportedPassesR12,automaticGroundBounces=simulation!=null?simulation.OverdriveGroundBounces:0,automaticGroundLimit=world!=null?world.OverdriveGroundLimit:0,startupShaderWarmupMs=startupShaderWarmupMsR12,startupRenderingReady=startupRenderingReadyR12,renderedFrameCount=Time.renderedFrameCount,frameRealtime=Time.realtimeSinceStartupAsDouble,gear=settings.gear,actorViewport=cam.WorldToViewportPoint(crab.Root.position),cameraPosition=cam.transform.position,cameraRight=cam.transform.right,cameraUp=cam.transform.up,cameraAspect=cam.aspect,feel=FeelStateR27(),inputAcks=inputAcksR23.ToArray(),sourceIdentity=SourceIdentityR23(),productVersion=Application.version,launchRejectReason=launchRejectReasonR23,floorSampled=simulation!=null&&float.IsFinite(simulation.MinGeometricSupportR21),minHeight=simulation!=null&&float.IsFinite(simulation.MinGeometricSupportR21)?simulation.MinGeometricSupportR21:0,groundStuckSeconds=simulation!=null?simulation.GroundStillSecondsR23:0,throwAudit=latestThrowAudit,throwAuditChanged=true,resourceAudit=resourceAuditR32,portPreview=PortPreviewState(),resultOverview=phase=="result"?resultOverview:null,bodySpinRps=BodySpinForStateR16(),requestedSpinRps=RequestedSpinForStateR16(),rechargeCount=simulation!=null?simulation.ReclaimedNodes:0,reclaimedRotorJ=simulation!=null?simulation.ReclaimedRotorJ:0,lastReclaimJ=simulation!=null?simulation.LastReclaimJ:0,worldBirds=(farLifeR10?farLifeR10.BirdCount:0)+(world!=null?world.LifeBirdTargetsR6:0),worldPeople=(farLifeR10?farLifeR10.PeopleCount:0)+(world!=null?world.LifePeopleTargetsR6:0),peopleHit=world!=null?world.PeopleHitR6:0,birdsHit=world!=null?world.BirdsHitR6:0,worldStartled=farLifeR10?farLifeR10.StartledCount:0,worldLifeAge=farLifeR10?farLifeR10.Age:0,farWorld=useFarWorldR10,floorContactClampedSteps=simulation!=null?simulation.FloorContactClampedSteps:0,floorSweepRadius=simulation!=null?simulation.FloorSweepRadius:0,floorMinBottom=simulation!=null?simulation.MinBottom:0,floorContactExtraSteps=simulation!=null?simulation.FloorContactExtraSteps:0,ceilingUnlocked=useCeilingGrowth&&settings.upgradeLevel>=2,ceilingBreakthrough=simulation!=null&&simulation.CeilingBreakthrough,ceilingExited=simulation!=null&&simulation.CeilingExited,launchReceipt=launchReceipt,firstContactProbe=simulation!=null?simulation.FirstContactProbe:"",rotorPulseCapacity=simulation!=null?simulation.RotorPulseCapacity:0,widerCamera=useWiderFlightCamera,auraVisible=useFlightAura&&spinWake.enabled,tracePoints=trail.enabled?trail.positionCount:0,auraPoints=spinWake.enabled?spinWake.positionCount:0,cameraSize=cam.orthographicSize,upgradeLevel=settings.upgradeLevel,district=CoreDistrictName(crab.Root.position.x),chargeLaunchFloor=minimumLaunchPower,fragmentVisibility=world!=null?world.Visibility:null,burstCount=simulation!=null?simulation.DestructionBoostCount:0,rotorRemaining=simulation!=null?simulation.RotorRemaining:0,rotorCapacity=simulation!=null?simulation.RotorCapacity:0,lastBurstGain=simulation!=null?simulation.LastBurstGain:0,compoundFrozen=world!=null&&world.FrozenCompoundApplied,compoundEnvelopeError=world!=null?world.LaunchEnvelopeError:0,compoundBoundsDifference=world!=null?world.LaunchBoundsDifferenceR30:0,cityLayout=cityLayout,cityTargets=world!=null?world.CityContactTargets:0,chainBreaks=world!=null?world.CitySecondaryBreaks:0,cityDecorative=world!=null?world.CityDecorativeFragments:0,hitStopRemaining=hitStop.Remaining,hitStopSeconds=hitStop.TotalFrozen,hitStopRequests=hitStop.Requests,generation=generation,events=eventStream.ToArray(),vfxActive=(effects?effects.Core.Active:0)+OverdrivePresentationActive,runtimeReady=startupRenderingReadyR12&&UnityEngine.Rendering.SplashScreen.isFinished,phase=phase,stage=phase=="ready"?"AIM":phase=="charge"?"LAUNCH":simulation!=null?simulation.Stage.ToString():"SETUP",message=message,distance=lastDistance,height=phase=="flight"?(float)simulation.Y:0,peak=lastPeak,best=best,previousBest=previousBest,bestDelta=lastDistance-previousBest,time=simulation!=null?(float)simulation.Time:0,angle=settings.angle,power=settings.power,claw=settings.claw,shell=settings.shell,legs=settings.legs,speed=simulation!=null?(float)simulation.Speed:(float)settings.LaunchSpeed,newRecord=newRecord,reducedMotion=ReducedMotion,paused=paused,aiming=aiming,canSwipeLaunch=previewEligible,direction=direction,spinRps=currentSpinRps,visualSpinAngle=displaySpinAngle,worldX=crab.Root.position.x,attempts=attempts,chargeWork=spinCharge.Work,chargeElapsed=spinCharge.Elapsed,chargeEnergy=spinCharge.Energy,chargeStarted=spinCharge.Started,chargeReady=spinCharge.Ready};
  if(simulation!=null){s.cityFragments=simulation.CityFragments;s.furnitureBroken=simulation.FurnitureBroken;s.equipmentBroken=simulation.EquipmentBroken;s.foodBroken=simulation.FoodBroken;s.glassBroken=simulation.GlassBroken;s.wallsBroken=simulation.WallsBroken;s.dishesBroken=simulation.DishesBroken;s.destructionEvent=simulation.DestructionEvent;s.fragments=simulation.ActiveFragments;s.foodFragments=simulation.FoodFragmentsSpawned;s.lastBroken=simulation.LastBroken;s.contactPart=simulation.LastPart;s.contactKind=simulation.LastKind;s.physicsFault=simulation.AbortReason;s.impactImpulse=simulation.Impacts.Count>0?simulation.Impacts[simulation.Impacts.Count-1].impulse:0;s.firstContact=(float)simulation.FirstContact;s.canKick=simulation.CanKick;s.braced=simulation.Braced;s.kicksRemaining=simulation.Remaining;s.kicksUsed=simulation.Used;s.perfects=simulation.Perfects;s.goods=simulation.Goods;s.earlyMisses=simulation.EarlyMisses;s.impacts=simulation.ImpactID;s.eventId=simulation.EventID;s.eventName=simulation.LastEvent;s.lastGrade=simulation.LastGrade;s.surface=simulation.LastSurface;s.groundTime=(float)simulation.TimeToGround;}
#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
  if(simulation!=null&&Finished){s.floorDiagnostics=simulation.FloorDiagnosticsJSON();s.rotorDiagnostics=simulation.RotorDiagnosticsJSON();}
#endif
  return s;
 }finally{PrepareEnvelopeR18.Exit(traceStateR18,Time.renderedFrameCount);}
}
 void OnDestroy(){CancelPreparationR18();ResetOverdrivePresentation();if(world!=null){world.Dispose();world=null;}contactVisualTemplateR15.Dispose();lifeGeometryCacheR17.Dispose();}
 void Report(){
#if UNITY_WEBGL && !UNITY_EDITOR
 int traceReportR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.Report,Time.renderedFrameCount);try{
  // Receipts are archived by the browser once per change, not retransmitted every 40ms.
  var report=State();ApplyObjectPresentationReport(report);report.throwAuditChanged=auditReportDirtyR32;if(!auditReportDirtyR32)report.throwAudit="";
  string reportJsonR18;int traceJsonR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.SerializeReport,Time.renderedFrameCount);try{reportJsonR18=JsonUtility.ToJson(report);}finally{PrepareEnvelopeR18.Exit(traceJsonR18,Time.renderedFrameCount);}int traceBridgeR18=PrepareEnvelopeR18.Enter(PrepareEnvelopeR18.ReportBridge,Time.renderedFrameCount);try{FlightReport(reportJsonR18);}finally{PrepareEnvelopeR18.Exit(traceBridgeR18,Time.renderedFrameCount);}auditReportDirtyR32=false;
}finally{PrepareEnvelopeR18.Exit(traceReportR18,Time.renderedFrameCount);}
#endif
 }
 LineRenderer Line(string name,Color color,float width){var l=new GameObject(name).AddComponent<LineRenderer>();l.useWorldSpace=true;l.startWidth=l.endWidth=width;l.numCornerVertices=2;var m=new Material(groundShader?groundShader:Shader.Find("Standard"));m.color=color;m.SetFloat("_Glossiness",0);l.sharedMaterial=m;return l;}
 Transform Flag(string name,Color color){var root=new GameObject(name).transform;CrabModel.Shape("pole",root,PrimitiveType.Cylinder,new Vector3(0,.8f,0),new Vector3(.045f,.8f,.045f),color);CrabModel.Shape("flag",root,PrimitiveType.Cube,new Vector3(.3f,1.35f,0),new Vector3(.6f,.4f,.045f),color);return root;}
 void MakeField(){
  SectionWorldArtR16.Enabled=useSectionArtR16&&useWorldArtR9&&cityLayout==2;RestaurantVisuals.ExteriorEnabled=!SectionWorldArtR16.Enabled;AddCityVisuals();RestaurantVisuals.Build(groundShader);if(useRotorBursts){RestaurantVisuals.DepthTone();RestaurantVisuals.DepthTone(cityVisuals);}if(SectionWorldArtR16.Enabled){SectionWorldArtR16.BuildBackground(transform,cityVisuals,groundShader,useFarWorldR10,useRouteBlockoutR11);MallModelPackR18.AddBay(transform);GiftStoreR7.AddSurroundings(transform);}
  shadow=CrabModel.Shape("Crab shadow",null,PrimitiveType.Cylinder,new Vector3(0,.045f,0),new Vector3(1.8f,.008f,1.2f),new Color(.41f,.44f,.35f)).transform;
  aimMarker=CrabModel.Shape("Launch force marker",null,PrimitiveType.Sphere,new Vector3(2,2,0),Vector3.one*.25f,new Color(1,.75f,.28f)).transform;
  spinWake=Line("Swipe wind arc",new Color(.88f,.95f,.73f),.045f);spinWake.positionCount=24;spinWake.endWidth=.008f;spinWake.enabled=false;
  aim=Line("Original first arc aim",new Color(1,.72f,.36f),.055f);trail=Line("Flight trace",new Color(.91f,.97f,.88f),.035f);landingRing=Line("Landing estimate ring",new Color(.90f,.96f,.91f),.055f);landingRing.loop=true;landingRing.positionCount=40;landingRing.enabled=false;
  landingFlag=Flag("Final stop",new Color(1,.43f,.21f));landingFlag.gameObject.SetActive(false);bestFlag=Flag("Kick rule best",new Color(.96f,.77f,.26f));bestFlag.gameObject.SetActive(false);
 }
}

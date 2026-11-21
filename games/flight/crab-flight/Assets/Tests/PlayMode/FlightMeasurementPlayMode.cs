// Fixed-input adapter for intentional r5 physical changes; old/new and repeated-input results remain separate.
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

public sealed partial class FlightMeasurementPlayMode
{
    [Serializable] public sealed class InputSpec { public float angle,power,claw=45,shell=45,legs=45; public int direction; public int upgradeLevel; }
    [Serializable] public sealed class Work { public float distance=2f/60,seconds=1f/60; public int direction; }
    [Serializable] public sealed class Aim { public float angle,power,spinRps,spinPhase; public int direction; public bool eligible=true; }
    [Serializable] public sealed class InitialBody { public Vector3 position,velocity,angularVelocity,centerOfMass; public Quaternion rotation; public float mass; public int[] floatBits; public string collisionDetection; }
    [Serializable] public sealed class BreakRecord { public bool secondary;public string material;public string id;public float time;public Vector3 position; }
    [Serializable] public sealed class Trial {
        public string id,variant,stateJson,error="";public int repetition,colliders,chargeTicks,flightTicks,cityBreaks,secondaryBreaks,distanceBits,gapBits;
        public bool finished,chargeReady,resetClean,pauseStable;
        public float peakSpeed,peakSpin,maxForwardDistance,overdriveSpent,remainingBudget,minRotorRemaining=1e30f;public int groundBounces,presentationPeak,presentationVertexPeak;public string overdriveJson;public float distance,firstCityTime=-1,lastWallTime=-1,wallToCityGap=-1,legacyTickBudgetFloat,physicsStateTime,hitStopFrozenSeconds,accumulatorAtResult;
        public double tickBudgetSeconds,physicsProgressSeconds,chargeLoopRealSeconds,flightLoopRealSeconds,setupRealSeconds,physicsGapSeconds,estimatedAccumulatorDiscardSeconds;
        public int accumulatorDiscardCount;public InitialBody initialBody;public BreakRecord[] breaks;public string visibilityJson;public bool displayPhysicsUnchanged;public string contactTraceJson;public string burstsJson;public float rotorCapacity,rotorRemaining;public int burstCount;public bool landingInputRejected;public List<float> speedTrace=new List<float>();
    }
    [Serializable] public sealed class Group {
        public string schema="crab.measurement2a.v03.1",inputID,revision="crab-flight-momentum-r8",state="RUNNING";
        public InputSpec input;public List<Trial> trials=new List<Trial>();
        public string oldLayoutSHA256,gapLayoutSHA256;public bool gameCodeChanged=true,physicsSettingsChanged=true,presentationOff=false,hitStopEnabled=true,reducedMotion=false;
        public float suppliedTickSeconds=1f/60,manualPhysicsStepSeconds=1f/240;public long stopwatchFrequency=Stopwatch.Frequency;
        public string clocks="physicsProgressSeconds is the existing simulation clock; real times use Stopwatch around synchronous measurement loops, not real-time player duration or isolated Physics CPU time. tickBudgetSeconds/legacyTickBudgetFloat are supplied Tick dt totals, not wall clock. accumulatorDiscard is observed/reconstructed, never changed.";
        public string comparisonPolicy="Same-version repeat equality and old/gap deltas are separate. A mismatch alone does not prove a physics change. C-7 final quality/adoption is not decided by this harness.";
    }
    [Serializable] public sealed class BurstRead{public string objectId;public float time,energySpent,remaining,beforeForward,afterForward;}
    [Serializable] public sealed class BurstWrapper{public BurstRead[] events;}
    Type gameType,cityType,presentationType,modelType;Behaviour template,current;Scene templateScene;readonly List<Scene> ownedScenes=new List<Scene>();
    bool momentumOnly;bool coreCompare;bool coreFeedback;int testUpgrade;bool auraCompare;bool cameraCompare;bool diagnosticOnly;bool visualOnly;string oldLayout,gapLayout;static string Evidence=>Path.GetFullPath(Path.Combine(Application.dataPath,"../../evidence",Environment.GetEnvironmentVariable("CRAB_EVIDENCE_SCOPE")??"unscoped"));
    static Type RuntimeType(string n)=>AppDomain.CurrentDomain.GetAssemblies().Select(a=>a.GetType(n,false)).FirstOrDefault(t=>t!=null)??throw new Exception("Missing runtime type: "+n);
    static object Field(object o,string n)=>o.GetType().GetField(n,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).GetValue(o);
    static object Property(object o,string n){var p=o.GetType().GetProperty(n,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);return p!=null?p.GetValue(o):Field(o,n);}
    static object Call(object o,string n,params object[] args)=>o.GetType().GetMethod(n,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Invoke(o,args);
    static void StaticCall(Type t,string n,params object[] args)=>t.GetMethod(n,BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).Invoke(null,args);
    static void StaticField(Type t,string n,object v)=>t.GetField(n,BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic).SetValue(null,v);
    static float F(object o,string n)=>Convert.ToSingle(Field(o,n));
    static int Bits(float f)=>BitConverter.ToInt32(BitConverter.GetBytes(f),0);
    static string SHA(string s){using(var h=System.Security.Cryptography.SHA256.Create())return string.Concat(h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s)).Select(b=>b.ToString("x2")));}
    static void Save(Group g){Directory.CreateDirectory(Evidence);File.WriteAllText(Path.Combine(Evidence,g.inputID+".json"),JsonUtility.ToJson(g,true));}
    static object State(Behaviour g)=>Call(g,"State");
    [UnitySetUp] public IEnumerator SetUp()
    {
        Assert.That(Application.isPlaying,Is.True,"Actual PlayMode required");
        gameType=RuntimeType("CrabFlightGame");cityType=RuntimeType("City2Data");presentationType=RuntimeType("Street3Presentation");modelType=RuntimeType("CrabModel");
        oldLayout=File.ReadAllText(Path.Combine(Application.dataPath,"Editor/BaselineData/r4-city-layout.json"));
        gapLayout=Resources.Load<TextAsset>("CityDensityR2/CityPlacementDense").text;
        // LoadSceneInPlayMode completes on a later frame. Disable the template in sceneLoaded (before Start), then wait for it.
        UnityEngine.Events.UnityAction<Scene,LoadSceneMode> loaded=(s,mode)=>{if(s.path!="Assets/FlightField.unity")return;templateScene=s;foreach(var root in s.GetRootGameObjects())foreach(var component in root.GetComponentsInChildren<MonoBehaviour>(true))if(component.GetType()==gameType){template=component;template.enabled=false;}};
        SceneManager.sceneLoaded+=loaded;
        try { templateScene=EditorSceneManager.LoadSceneInPlayMode("Assets/FlightField.unity",new LoadSceneParameters(LoadSceneMode.Additive));ownedScenes.Add(templateScene);for(int frame=0;frame<120&&!template;frame++)yield return null; }
        finally { SceneManager.sceneLoaded-=loaded; }
        Assert.That(template,Is.Not.Null,"Template scene did not finish loading");template.enabled=false;gameType.GetField("fragmentFadeShader").SetValue(template,UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>("Assets/CrabCityDensityR2/Shaders/BrokenMetalVisibility.shader"));
        yield return null;yield return null;
        Assert.That(Time.timeScale,Is.EqualTo(1f));
    }
    [UnityTearDown] public IEnumerator TearDown()
    {
        if(current) { Call(current,"ShutdownReviewPhysics");current=null; }
        if(template) { Call(template,"ShutdownReviewPhysics");template=null; }
        StaticField(presentationType,"ReviewBaseline",false);StaticCall(cityType,"ReviewSetLayout",gapLayout);StaticCall(modelType,"ReviewUseBaseline",false);
        foreach(var s in ownedScenes.AsEnumerable().Reverse().ToArray())if(s.IsValid()&&s.isLoaded){var a=SceneManager.UnloadSceneAsync(s);if(a!=null)while(!a.isDone)yield return null;}
        StaticField(RuntimeType("RigidCrabFlight"),"ReviewSuppressCoreObjectBurst",false);StaticField(RuntimeType("RigidCrabFlight"),"ReviewCoreRotorCoefficient",-1f);StaticField(RuntimeType("RigidCrabFlight"),"ReviewDisableCorePanels",false);StaticField(cityType,"ReviewUseDenseLayout",false);ownedScenes.Clear();yield return null;yield return null;
    }
    [UnityTest,Order(0)] public IEnumerator Standard_OldGap_ThreePairs()=>RunGroup("standard",12,100,1);
    [UnityTest,Order(1)] public IEnumerator LowPower_OldGap_ThreePairs()=>RunGroup("low",12,70,1);
    [UnityTest,Order(2)] public IEnumerator HighAngle_OldGap_ThreePairs()=>RunGroup("high",28,100,1);
    [UnityTest,Order(3)] public IEnumerator Left_OldGap_ThreePairs()=>RunGroup("left",12,100,-1);
    [UnityTest,Order(4)] public IEnumerator RuntimeInspector_ApplyAndClose_UsesExistingGameParameters()
    {
        var type=RuntimeType("FlightTuningPanel");var panel=template.GetComponent(type);Assert.That(panel,Is.Not.Null);
        Call(panel,"SetDeveloperMode","1");yield return null;yield return null;
        Call(panel,"ApplyFlightTuning","{\"Angle\":28,\"Power\":70,\"Claw\":45,\"Shell\":45,\"Legs\":45,\"SecondaryFraction\":0.15}");
        var state=State(template);Assert.That(F(state,"angle"),Is.EqualTo(28));Assert.That(F(state,"power"),Is.EqualTo(70));Assert.That((bool)Field(state,"paused"),Is.True);Assert.That(F(state,"time"),Is.EqualTo(0));
        Assert.That(Convert.ToInt32(Property(panel,"ApplyCount")),Is.EqualTo(1));Assert.That(Convert.ToSingle(gameType.GetField("secondaryThresholdFraction").GetValue(template)),Is.EqualTo(.15f));
        Call(panel,"ApplyFlightTuning","{\"Angle\":12,\"Power\":100,\"Claw\":45,\"Shell\":45,\"Legs\":45,\"SecondaryFraction\":0.25}");Call(panel,"SetDeveloperMode","0");yield return null;
        Assert.That((bool)Property(panel,"Visible"),Is.False);Assert.That((bool)Field(State(template),"paused"),Is.False);Assert.That(Time.timeScale,Is.EqualTo(1));
    }
    IEnumerator RunGroup(string inputID,float angle,float power,int direction,float workRate=2,bool reduced=false)
    {
        var input=new InputSpec{angle=angle,power=power,direction=direction,upgradeLevel=testUpgrade};
        Assert.That(File.Exists(Path.Combine(Evidence,inputID+".json")),Is.False,"Do not overwrite a previous experiment");var group=new Group{inputID=inputID,input=input,oldLayoutSHA256=SHA(oldLayout),gapLayoutSHA256=SHA(gapLayout),reducedMotion=reduced};Save(group);
        for(int repetition=1;repetition<=3;repetition++)foreach(int variant in new[]{2,3})
        {
            if(momentumOnly&&variant!=3)continue;
            if(cameraCompare&&!coreCompare&&repetition!=1)continue;
            if(visualOnly&&(variant!=3||repetition!=1))continue;
            bool baseline=variant==1;string variantName=coreCompare?(variant==2?"r7-reference":"r8-momentum"):auraCompare?(variant==2?"r7-aura-off":"r7-aura-on"):(variant==2?"r6-camera-reference":"r7-wider-camera");
            var row=new Trial{id=inputID+"-"+variantName+"-"+repetition,variant=variantName,repetition=repetition};group.trials.Add(row);Save(group);
            var setup=Stopwatch.StartNew();StaticField(presentationType,"ReviewBaseline",false);StaticCall(cityType,"ReviewSetLayout",gapLayout);StaticCall(modelType,"ReviewUseBaseline",false);
            var scene=SceneManager.CreateScene("Measurement "+row.id);ownedScenes.Add(scene);SceneManager.SetActiveScene(scene);
            current=(Behaviour)new GameObject("CrabFlightGame").AddComponent(gameType);current.enabled=false;
            foreach(string n in new[]{"crabShader","groundShader","fragmentFadeShader","secondaryThresholdFraction","cityR2Shader","cityR2Albedo","cityR2Metal","effectsPrefab"})gameType.GetField(n).SetValue(current,gameType.GetField(n).GetValue(template));
            gameType.GetField("useCoreDestruction").SetValue(current,coreCompare&&variant==3);gameType.GetField("useCoreHitFeedback").SetValue(current,coreCompare&&variant==3&&coreFeedback);gameType.GetField("useSpectacleCrab").SetValue(current,true);gameType.GetField("useWiderFlightCamera").SetValue(current,coreCompare||auraCompare||variant==3);gameType.GetField("useFlightAura").SetValue(current,coreCompare||auraCompare&&variant==3);gameType.GetField("useCityDetailR6").SetValue(current,true);gameType.GetField("useOverdrive").SetValue(current,true);gameType.GetField("useOverdriveRoute").SetValue(current,true);gameType.GetField("useFragmentVisibility").SetValue(current,true);gameType.GetField("cityLayout").SetValue(current,2);gameType.GetField("useRotorBursts").SetValue(current,true);gameType.GetField("useCeilingRicochet").SetValue(current,true);Call(current,"Initialize");Call(current,"SetReducedMotion",reduced?"1":"0");Call(current,"Configure",JsonUtility.ToJson(input));
            yield return null;yield return null;setup.Stop();row.setupRealSeconds=setup.Elapsed.TotalSeconds;
            var aim=JsonUtility.ToJson(new Aim{angle=angle,power=power,direction=direction});var work=JsonUtility.ToJson(new Work{direction=direction,distance=workRate/60});
            Call(current,"BeginAim","");Call(current,"PreviewLaunch",aim);Call(current,"BeginSpinCharge","");var charge=Stopwatch.StartNew();
            for(int i=0;i<601;i++){Call(current,"AddSpinWork",work);Call(current,"Tick",1f/60);row.chargeTicks++;}charge.Stop();row.chargeLoopRealSeconds=charge.Elapsed.TotalSeconds;
            var ready=State(current);row.chargeReady=(bool)Field(ready,"chargeReady")&&F(ready,"chargeEnergy")>=.999f;Assert.That(row.chargeReady,Is.True,"601-tick charge precondition");
            Call(current,"ReleaseLaunch",aim);var sim=Property(current,"ReviewSimulation");row.colliders=Convert.ToInt32(Property(sim,"ColliderCount"));row.landingInputRejected=baseline||!(bool)Call(sim,"SetKickPressed",true);Call(sim,"SetKickPressed",false);var body=(Rigidbody)Property(sim,"Body");
            var initial=new[]{body.position.x,body.position.y,body.position.z,body.rotation.x,body.rotation.y,body.rotation.z,body.rotation.w,body.linearVelocity.x,body.linearVelocity.y,body.linearVelocity.z,body.angularVelocity.x,body.angularVelocity.y,body.angularVelocity.z,body.mass};
            if(repetition==1){CaptureRoute(inputID+"-"+variantName,"launch");if(!cameraCompare)CaptureCrabDetail(inputID+"-"+variantName);}
            row.initialBody=new InitialBody{position=body.position,rotation=body.rotation,velocity=body.linearVelocity,angularVelocity=body.angularVelocity,mass=body.mass,centerOfMass=body.worldCenterOfMass,floatBits=initial.Select(Bits).ToArray(),collisionDetection=body.collisionDetectionMode.ToString()};
            row.pauseStable=true;bool paused=false,firstBreakShot=false,exitShot=false,streetShot=false;var flight=Stopwatch.StartNew();
            for(int i=0;i<6000&&!(bool)Property(current,"Finished");i++)
            {
                var before=State(current);float accBefore=(float)Field(current,"accumulator"),held=Math.Min(1f/60,F(before,"hitStopRemaining"));double timeBefore=Convert.ToDouble(Property(sim,"Time"));
                Call(current,"Tick",1f/60);if(diagnosticOnly&&i%2==0)Call(sim,"ReviewSampleDebris");row.speedTrace.Add(Convert.ToSingle(Property(sim,"Speed")));row.flightTicks++;row.tickBudgetSeconds+=1f/60;row.legacyTickBudgetFloat+=1f/60;
                var state=State(current);row.maxForwardDistance=Mathf.Max(row.maxForwardDistance,F(state,"distance"));row.minRotorRemaining=Mathf.Min(row.minRotorRemaining,F(state,"rotorRemaining"));row.presentationPeak=Mathf.Max(row.presentationPeak,Convert.ToInt32(Property(current,"OverdrivePresentationActive")));row.presentationVertexPeak=Mathf.Max(row.presentationVertexPeak,Convert.ToInt32(Property(current,"OverdrivePresentationVertices")));
                if(repetition==1){if(!firstBreakShot&&Convert.ToInt32(Field(state,"destructionEvent"))>0){CaptureRoute(inputID+"-"+variantName,"first-break");firstBreakShot=true;}if(!exitShot&&Convert.ToInt32(Field(state,"wallsBroken"))>0&&(bool)Call(sim,"FullyOutsideRestaurant")){CaptureRoute(inputID+"-"+variantName,"exit");exitShot=true;}if(!streetShot&&F(state,"distance")>90){CaptureRoute(inputID+"-"+variantName,"street");streetShot=true;}}double timeAfter=Convert.ToDouble(Property(sim,"Time"));
                if(timeAfter>timeBefore&&F(state,"hitStopRemaining")>0&&(float)Field(current,"accumulator")==0)
                {int steps=(int)Math.Round((timeAfter-timeBefore)/(1f/240));float discarded=accBefore+(1f/60-held);for(int k=0;k<steps;k++)discarded-=1f/240;row.estimatedAccumulatorDiscardSeconds+=Math.Max(0,discarded);row.accumulatorDiscardCount++;}
                if(!paused&&F(state,"hitStopRemaining")>0){float pt=F(state,"time"),hold=F(state,"hitStopRemaining");var bp=body.position;Call(current,"SetPaused","1");Call(current,"Tick",.25f);var ps=State(current);row.pauseStable=F(ps,"time")==pt&&F(ps,"hitStopRemaining")==hold&&body.position==bp;Call(current,"SetPaused","0");paused=true;}
            }
            flight.Stop();row.flightLoopRealSeconds=flight.Elapsed.TotalSeconds;
            row.peakSpeed=Convert.ToSingle(Field(sim,"MaxSpeed"));row.peakSpin=Convert.ToSingle(Field(sim,"MaxSpin"));row.overdriveSpent=Convert.ToSingle(Property(sim,"OverdriveSpent"));row.remainingBudget=Convert.ToSingle(Property(sim,"RotorRemaining"));row.groundBounces=Convert.ToInt32(Property(sim,"OverdriveGroundBounces"));row.overdriveJson=JsonUtility.ToJson(new OverdriveList{events=((IEnumerable)Property(sim,"OverdriveImpulses")).Cast<object>().Select(x=>new OverdriveRead{source=(string)Field(x,"source"),spent=F(x,"spent"),forwardJ=F(x,"forwardJ"),verticalJ=F(x,"verticalJ"),spinJ=F(x,"spinJ"),remaining=F(x,"remaining"),time=F(x,"time")}).ToArray()});row.visibilityJson=JsonUtility.ToJson(Property(sim,"Visibility"));row.displayPhysicsUnchanged=(bool)Property(sim,"ReviewVisibilityPhysicsUnchanged");Assert.That((int)Field(Property(sim,"Visibility"),"missingMeshes"),Is.Zero,"Display mesh CPU data unavailable");row.contactTraceJson=(string)Call(sim,"ReviewContactTraceJson");var final=State(current);row.rotorCapacity=F(final,"rotorCapacity");row.rotorRemaining=F(final,"rotorRemaining");row.burstCount=Convert.ToInt32(Field(final,"burstCount"));row.burstsJson=JsonUtility.ToJson(new BurstWrapper{events=((IEnumerable)Property(sim,"BurstEvents")).Cast<object>().Select(x=>new BurstRead{objectId=(string)Field(x,"objectId"),time=F(x,"time"),energySpent=F(x,"energySpent"),remaining=F(x,"remaining"),beforeForward=F(x,"beforeForward"),afterForward=F(x,"afterForward")}).ToArray()});row.stateJson=JsonUtility.ToJson(final);row.distance=F(final,"distance");row.distanceBits=Bits(row.distance);row.physicsProgressSeconds=Convert.ToDouble(Property(sim,"Time"));row.physicsStateTime=F(final,"time");row.hitStopFrozenSeconds=F(final,"hitStopSeconds");row.accumulatorAtResult=(float)Field(current,"accumulator");row.error=(string)Field(final,"physicsFault");row.finished=(bool)Property(current,"Finished")&&row.error.Length==0;
            var breaks=new List<BreakRecord>();foreach(var e in (IEnumerable)Property(sim,"BreakEvents"))breaks.Add(new BreakRecord{secondary=(bool)Field(e,"secondary"),material=(string)Field(e,"material"),id=(string)Field(e,"id"),time=F(e,"time"),position=(Vector3)Field(e,"position")});row.breaks=breaks.ToArray();
            row.firstCityTime=breaks.Where(e=>e.id.StartsWith("street3-")).Select(e=>e.time).DefaultIfEmpty(-1).First();row.lastWallTime=breaks.Where(e=>e.id.StartsWith("outer-wall-")&&e.time<=row.firstCityTime).Select(e=>e.time).DefaultIfEmpty(-1).Max();row.wallToCityGap=row.firstCityTime>=0&&row.lastWallTime>=0?row.firstCityTime-row.lastWallTime:-1;row.physicsGapSeconds=row.wallToCityGap;row.gapBits=Bits(row.wallToCityGap);row.cityBreaks=breaks.Count(e=>e.id.StartsWith("street3-"));row.secondaryBreaks=Convert.ToInt32(Property(sim,"CitySecondaryBreaks"));Save(group);
            if(repetition==1){CaptureRoute(inputID+"-"+variantName,"result");if((Environment.GetEnvironmentVariable("CRAB_EVIDENCE_SCOPE")??"").StartsWith("visibility"))CaptureVisibility(inputID,(Rigidbody)Property(sim,"Body"));}
            if(inputID=="upgrade-one"){
                Call(current,"SetUpgradeLevel","2");var bought=State(current);Assert.That(Convert.ToInt32(Field(bought,"upgradeLevel")),Is.EqualTo(2),"result purchase applies before retry");Call(current,"RetryLaunch","");var retried=State(current);Assert.That(Convert.ToInt32(Field(retried,"upgradeLevel")),Is.EqualTo(2));var retrySim=Property(current,"ReviewSimulation");var retryBody=(Rigidbody)Property(retrySim,"Body");float expectedSpeed=row.initialBody.velocity.magnitude*(1.08f/1.04f);Assert.That(retryBody.linearVelocity.magnitude,Is.EqualTo(expectedSpeed).Within(.0001f));File.WriteAllText(Path.Combine(Evidence,"purchase-retry.json"),JsonUtility.ToJson(new PurchaseRetry{level=2,initialSpeed=retryBody.linearVelocity.magnitude,expectedSpeed=expectedSpeed,attempts=Convert.ToInt32(Field(retried,"attempts"))},true));
            }
            Call(current,"Prepare","");yield return null;yield return null;var reset=State(current);row.resetClean=Convert.ToInt32(Field(reset,"fragments"))==0&&Convert.ToInt32(Field(reset,"cityFragments"))==0&&Convert.ToInt32(Field(reset,"vfxActive"))==0&&Convert.ToInt32(Field(reset,"hitStopRequests"))==0&&F(reset,"hitStopRemaining")==0;Save(group);
            Call(current,"ShutdownReviewPhysics");current=null;yield return null;yield return null;SceneManager.SetActiveScene(templateScene);var unload=SceneManager.UnloadSceneAsync(scene);while(!unload.isDone)yield return null;ownedScenes.Remove(scene);Save(group);
        }
        group.state="MEASURED_CURRENT_COMPARISON";Save(group);
        // Assertions concern safe measurement execution. Repeat equality and C-7 are reported independently in the aggregation.
        foreach(var row in group.trials){if(inputID=="weak-flick-curve"&&row.variant=="r7-reference")Assert.That(row.error,Is.EqualTo("floor-tunnel"),"preserved old weak input fault is recorded, not scored");else Assert.That(row.finished,Is.True,row.id+" did not finish");Assert.That(row.colliders,Is.EqualTo(13),row.id+" collider contract");if(row.variant=="r8-momentum"){Assert.That(row.minRotorRemaining,Is.GreaterThanOrEqualTo(-.001f),row.id+" finite nonnegative reserve");Assert.That(row.overdriveSpent,Is.LessThanOrEqualTo(row.rotorCapacity+.05f),row.id+" no overspend");}Assert.That(row.resetClean&&row.pauseStable,Is.True,row.id+" reset/pause");}
    }
    [Serializable]public class PurchaseRetry{public int level,attempts;public float initialSpeed,expectedSpeed;}
    [Serializable]public class OverdriveRead{public string source;public float time,spent,forwardJ,verticalJ,spinJ,remaining;}
    [Serializable]public class OverdriveList{public OverdriveRead[] events;}
    [UnityTest,Order(11)] public IEnumerator OverdriveFiveInputComparison(){
        yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);yield return RunGroup("high-work-proxy",12,100,1,6);
        visualOnly=true;yield return RunGroup("reduced-standard",12,100,1,2,true);visualOnly=false;
    }
    [UnityTest,Order(12)] public IEnumerator OverdriveWoodVisibilityReferences(){ visualOnly=true;yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);yield return RunGroup("high-work-proxy",12,100,1,6);visualOnly=false;}
    [UnityTest,Order(13)] public IEnumerator SpectacleR6Regression(){yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);yield return RunGroup("high-work-proxy",12,100,1,6);visualOnly=true;yield return RunGroup("reduced-standard",12,100,1,2,true);visualOnly=false;}
    [UnityTest,Order(14)] public IEnumerator SpectacleR6LightingRepair(){visualOnly=true;yield return RunGroup("standard",12,100,1);yield return RunGroup("reduced-standard",12,100,1,2,true);visualOnly=false;}
    [UnityTest,Order(15)] public IEnumerator WiderFlightCameraR7(){cameraCompare=true;yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);yield return RunGroup("reduced-standard",12,100,1,2,true);cameraCompare=false;}
    [UnityTest,Order(16)] public IEnumerator FlightAuraR7(){cameraCompare=true;auraCompare=true;yield return RunGroup("standard",12,100,1);yield return RunGroup("high",28,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("reduced-standard",12,100,1,2,true);auraCompare=false;cameraCompare=false;}
    [UnityTest,Order(24)] public IEnumerator MomentumGrowthLegacy(){coreCompare=true;coreFeedback=true;momentumOnly=true;testUpgrade=1;yield return RunGroup("growth-stock-standard",12,100,1);yield return RunGroup("growth-stock-low",12,70,1);yield return RunGroup("growth-stock-high",28,100,1);visualOnly=true;yield return RunGroup("upgrade-one",12,70,1);visualOnly=false;testUpgrade=0;momentumOnly=false;coreCompare=false;}
    [UnityTest,Order(23)] public IEnumerator MomentumReserveScreen(){coreCompare=true;coreFeedback=true;visualOnly=true;yield return RunGroup("reserve-standard",12,100,1);yield return RunGroup("reserve-low",12,70,1);yield return RunGroup("reserve-high",28,100,1);yield return RunGroup("reserve-left",12,100,-1);yield return RunGroup("reserve-weak",12,21,1);visualOnly=false;coreCompare=false;}
    [UnityTest,Order(22)] public IEnumerator MomentumR8Regression(){coreCompare=true;coreFeedback=true;
        // Candidate only; archived r7 per-input n3 is retained as the matched reference.
        foreach(var input in new[]{new InputSpec{angle=12,power=100,direction=1},new InputSpec{angle=12,power=70,direction=1},new InputSpec{angle=28,power=100,direction=1},new InputSpec{angle=12,power=100,direction=-1},new InputSpec{angle=12,power=21,direction=1}}){
            string id=input.power==21?"r8-weak":input.direction<0?"r8-left":input.angle==28?"r8-high":input.power==70?"r8-low":"r8-standard";
            momentumOnly=true;yield return RunGroup(id,input.angle,input.power,input.direction);momentumOnly=false;
        }
        visualOnly=true;yield return RunGroup("r8-reduced-standard",12,100,1,2,true);testUpgrade=1;yield return RunGroup("upgrade-one",12,70,1);testUpgrade=0;visualOnly=false;coreCompare=false;
    }
    [UnityTest,Order(21)] public IEnumerator CoreNewObjectPaymentControl(){coreCompare=true;coreFeedback=true;visualOnly=true;var rigid=RuntimeType("RigidCrabFlight");StaticField(rigid,"ReviewSuppressCoreObjectBurst",true);yield return RunGroup("ablate-core-payment-off",12,100,1);StaticField(rigid,"ReviewSuppressCoreObjectBurst",false);visualOnly=false;coreCompare=false;}
    [UnityTest,Order(20)] public IEnumerator CoreAblationMatrix(){coreCompare=true;coreFeedback=true;visualOnly=true;
        var rigid=RuntimeType("RigidCrabFlight");
        yield return RunGroup("ablate-current",12,100,1);
        StaticField(rigid,"ReviewCoreRotorCoefficient",.50f);yield return RunGroup("ablate-capacity50",12,100,1);StaticField(rigid,"ReviewCoreRotorCoefficient",-1f);
        StaticField(cityType,"ReviewUseDenseLayout",true);yield return RunGroup("ablate-old-layout",12,100,1);StaticField(cityType,"ReviewUseDenseLayout",false);
        StaticField(rigid,"ReviewDisableCorePanels",true);yield return RunGroup("ablate-no-panels",12,100,1);StaticField(rigid,"ReviewDisableCorePanels",false);
        coreFeedback=false;yield return RunGroup("ablate-old-feedback",12,100,1);coreFeedback=true;
        visualOnly=false;coreCompare=false;
    }
    [UnityTest,Order(19)] public IEnumerator CoreChargeCurve(){coreCompare=true;coreFeedback=true;
        var powers=new List<float>();var method=gameType.GetMethod("CoreLaunchPower",BindingFlags.Public|BindingFlags.Static);float previous=-1;
        foreach(float power in new[]{10f,21f,40f,60f,70f,100f}){float result=(float)method.Invoke(null,new object[]{power,1f,true});Assert.That(result,Is.GreaterThan(previous),"final flick remains meaningful at full charge");previous=result;powers.Add(result);}
        Assert.That((float)method.Invoke(null,new object[]{21f,0f,true}),Is.EqualTo(21f).Within(.0001f),"accepted launch target is not charge-scaled; ReleaseLaunch ready gate tested separately");Directory.CreateDirectory(Evidence);File.WriteAllText(Path.Combine(Evidence,"charge-curve.json"),JsonUtility.ToJson(new CurveResult{powers=powers.ToArray()},true));yield return RunGroup("weak-flick-curve",12,21,1);coreCompare=false;}
    [Serializable]public class CurveResult{public float[] powers;}
    [UnityTest,Order(18)] public IEnumerator CoreRemainingAndPurchase(){coreCompare=true;coreFeedback=true;visualOnly=true;yield return RunGroup("reduced-standard",12,100,1,2,true);testUpgrade=1;yield return RunGroup("upgrade-one",12,70,1);testUpgrade=0;visualOnly=false;coreCompare=false;}
    [UnityTest,Order(17)] public IEnumerator CoreDestructionComparison(){coreCompare=true;coreFeedback=true;yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);yield return RunGroup("weak-flick",12,21,1);visualOnly=true;yield return RunGroup("reduced-standard",12,100,1,2,true);testUpgrade=1;yield return RunGroup("upgrade-one",12,70,1);testUpgrade=0;visualOnly=false;coreCompare=false;}
    void CaptureCrabDetail(string input){
     var c=(Camera)Property(current,"ReviewCamera");var model=Property(current,"ReviewCrab");var root=(Transform)Field(model,"Root");var pos=c.transform.position;var rot=c.transform.rotation;float sz=c.orthographicSize,asp=c.aspect;var output=Path.Combine(Evidence,"crab-detail");Directory.CreateDirectory(output);var center=root.position+Vector3.up*.2f;
     foreach(var v in new[]{new Vector3(3,3,-4),new Vector3(-4,2,-2)}){c.transform.position=center+v;c.transform.LookAt(center);c.orthographicSize=2.6f;c.aspect=1;var rt=new RenderTexture(640,640,24);rt.Create();var previous=RenderTexture.active;c.targetTexture=rt;RenderTexture.active=rt;c.Render();var im=new Texture2D(640,640,TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,640,640),0,0);im.Apply();File.WriteAllBytes(Path.Combine(output,input+"-"+(v.x>0?"shell":"claw")+".png"),im.EncodeToPNG());c.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(im);rt.Release();Object.DestroyImmediate(rt);}
     c.transform.SetPositionAndRotation(pos,rot);c.orthographicSize=sz;c.aspect=asp;
    }
    void CaptureRoute(string id,string stage){
        using(var isolation=new RenderIsolation(current.gameObject.scene,(Scene)Property(Property(current,"ReviewSimulation")??Field(current,"world"),"Scene"))){
        var c=(Camera)Property(current,"ReviewCamera");var p=c.transform.position;var q=c.transform.rotation;float size=c.orthographicSize,aspect=c.aspect;var output=Path.Combine(Evidence,"route-comparison");Directory.CreateDirectory(output);
        foreach(var wh in new[]{new[]{390,844},new[]{844,390}}){c.aspect=(float)wh[0]/wh[1];Call(current,"RefreshReviewCamera");var rt=new RenderTexture(wh[0],wh[1],24);rt.Create();var previous=RenderTexture.active;c.targetTexture=rt;RenderTexture.active=rt;c.Render();var im=new Texture2D(wh[0],wh[1],TextureFormat.RGB24,false);im.ReadPixels(new Rect(0,0,wh[0],wh[1]),0,0);im.Apply();File.WriteAllBytes(Path.Combine(output,id+"-"+stage+"-"+wh[0]+".png"),im.EncodeToPNG());c.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(im);rt.Release();Object.DestroyImmediate(rt);}
        c.aspect=aspect;c.transform.SetPositionAndRotation(p,q);c.orthographicSize=size;
        }
    }
    [UnityTest,Order(6)] public IEnumerator ReadabilityFourStoredInputPoses()
    {
        visualOnly=true;
        yield return RunGroup("visual-standard",12,100,1);
        yield return RunGroup("visual-low",12,70,1);
        yield return RunGroup("visual-high",28,100,1);
        yield return RunGroup("visual-left",12,100,-1);
        visualOnly=false;
    }
    [UnityTest,Order(8)] public IEnumerator LowHighDebrisOpportunity()
    { diagnosticOnly=true;visualOnly=true;yield return RunGroup("probe-low",12,70,1);yield return RunGroup("probe-high",28,100,1);visualOnly=false;diagnosticOnly=false; }
    [UnityTest,Order(7)] public IEnumerator ChainLineageEvidenceStandard()
    {
        visualOnly=true;yield return RunGroup("lineage-standard",12,100,1);visualOnly=false;
    }
    [UnityTest,Order(10)] public IEnumerator FragmentVisibilityFourReferences()
    {
        var type=RuntimeType("RigidCrabFlight");var method=type.GetMethod("ReviewRayTriangle");
        Func<Vector3,Vector3,float,bool> hit=(o,d,l)=>(bool)method.Invoke(null,new object[]{o,d,new Vector3(-1,-1,3),new Vector3(-1,1,3),new Vector3(1,-1,3),l});
        Assert.That(hit(new Vector3(-.2f,-.2f,0),Vector3.forward,4),Is.True);Assert.That(hit(new Vector3(4,4,0),Vector3.forward,4),Is.False);Assert.That(hit(new Vector3(-.2f,-.2f,4),Vector3.forward,4),Is.False);Assert.That(hit(new Vector3(-.2f,-.2f,0),Vector3.forward,2),Is.False);
        yield return RunGroup("standard",12,100,1);yield return RunGroup("low",12,70,1);yield return RunGroup("high",28,100,1);yield return RunGroup("left",12,100,-1);
    }
    void CaptureVisibility(string inputID,Rigidbody body)
    {
        var cam=(Camera)Property(current,"ReviewCamera");var pos=body.position;var rot=body.rotation;var velocity=body.linearVelocity;var omega=body.angularVelocity;var sim=Property(current,"ReviewSimulation");
        var output=Path.Combine(Evidence,"visibility-comparison");Directory.CreateDirectory(output);
        foreach(bool reduced in new[]{false,true})foreach(var size in new[]{new[]{390,844},new[]{844,390}})
        {
            Call(current,"SetReducedMotion",reduced?"1":"0");Call(current,"Tick",0f);cam.aspect=(float)size[0]/size[1];Call(current,"RefreshReviewCamera");var cp=cam.transform.position;var cq=cam.transform.rotation;float cs=cam.orthographicSize;
            foreach(bool enabled in new[]{false,true})
            {
                Call(current,"ReviewRefreshFragmentVisibility",enabled);var rt=new RenderTexture(size[0],size[1],24);rt.Create();cam.targetTexture=rt;var previous=RenderTexture.active;RenderTexture.active=rt;cam.Render();
                var image=new Texture2D(size[0],size[1],TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size[0],size[1]),0,0);image.Apply();string file=inputID+"-"+size[0]+"-"+(reduced?"reduced":"normal")+"-"+(enabled?"on":"off");File.WriteAllBytes(Path.Combine(output,file+".png"),image.EncodeToPNG());
                File.WriteAllText(Path.Combine(output,file+".json"),JsonUtility.ToJson(Property(sim,"Visibility"),true));cam.targetTexture=null;RenderTexture.active=previous;Object.DestroyImmediate(image);rt.Release();Object.DestroyImmediate(rt);
                Assert.That(body.position,Is.EqualTo(pos));Assert.That(body.rotation,Is.EqualTo(rot));Assert.That(body.linearVelocity,Is.EqualTo(velocity));Assert.That(body.angularVelocity,Is.EqualTo(omega));Assert.That(cam.transform.position,Is.EqualTo(cp));Assert.That(cam.transform.rotation,Is.EqualTo(cq));Assert.That(cam.orthographicSize,Is.EqualTo(cs));Assert.That((bool)Property(sim,"ReviewVisibilityPhysicsUnchanged"),Is.True);
            }
        }
        Call(current,"SetReducedMotion","0");Call(current,"ReviewRefreshFragmentVisibility",true);cam.ResetAspect();Call(current,"RefreshReviewCamera");
        File.WriteAllText(Path.Combine(output,inputID+"-scope.json"),"{\"renderer\":\""+SystemInfo.graphicsDeviceType+"\",\"sameBodyAndCameraPerPair\":true,\"simulationStepsBetweenPair\":0}");
    }
    [Serializable] public sealed class SafetyRow {public float rate,angle,work,credit,spent,distance,peakSpeed,maxSpin;public int direction,boosts,secondary,visualPeak;public string fault;public bool finished,landingRejected,resetClean;}
    [Serializable] public sealed class SafetyResults {public List<SafetyRow> rows=new List<SafetyRow>();public float work30,work60,work120,beforeSaturation,afterSaturation;}
    [UnityTest,Order(5)] public IEnumerator Rotor_InputCreditAndBoundedStress()
    {
        Directory.CreateDirectory(Evidence);var output=new SafetyResults();var type=RuntimeType("SpinCharge");
        foreach(int hz in new[]{30,60,120}){var c=Activator.CreateInstance(type);type.GetField("ContinuousWork").SetValue(c,true);Call(c,"Begin");for(int i=0;i<hz*10;i++){Call(c,"Add",6f/hz,1f/hz);Call(c,"Tick",1f/hz);}float w=Convert.ToSingle(Property(c,"Work"));if(hz==30)output.work30=w;else if(hz==60)output.work60=w;else output.work120=w;}
        Assert.That(Mathf.Abs(output.work30-output.work120),Is.LessThan(.0001f));Assert.That(Mathf.Abs(output.work60-output.work120),Is.LessThan(.0001f));
        var c2=Activator.CreateInstance(type);type.GetField("ContinuousWork").SetValue(c2,true);Call(c2,"Begin");for(int i=0;i<300;i++){Call(c2,"Add",.1f,1f/60);Call(c2,"Tick",1f/60);}output.beforeSaturation=Convert.ToSingle(Property(c2,"SpinRps"));for(int i=0;i<240;i++){Call(c2,"Add",.1f,1f/60);Call(c2,"Tick",1f/60);}output.afterSaturation=Convert.ToSingle(Property(c2,"SpinRps"));Assert.That(output.afterSaturation,Is.GreaterThan(output.beforeSaturation));
        foreach(var spec in new[]{new[]{.5f,12f,1f},new[]{2f,12f,1f},new[]{6f,12f,1f},new[]{6f,28f,1f},new[]{6f,12f,-1f},new[]{0f,12f,1f}}){
            var row=new SafetyRow{rate=spec[0],angle=spec[1],direction=(int)spec[2]};output.rows.Add(row);StaticCall(cityType,"ReviewSetLayout",gapLayout);
            var scene=SceneManager.CreateScene("Rotor safety");ownedScenes.Add(scene);SceneManager.SetActiveScene(scene);current=(Behaviour)new GameObject("CrabFlightGame").AddComponent(gameType);current.enabled=false;
            foreach(string n in new[]{"crabShader","groundShader","fragmentFadeShader","secondaryThresholdFraction","cityR2Shader","cityR2Albedo","cityR2Metal","effectsPrefab"})gameType.GetField(n).SetValue(current,gameType.GetField(n).GetValue(template));
            gameType.GetField("cityLayout").SetValue(current,2);gameType.GetField("useRotorBursts").SetValue(current,true);Call(current,"Initialize");Call(current,"Configure",JsonUtility.ToJson(new InputSpec{angle=row.angle,power=100,direction=row.direction}));yield return null;yield return null;
            var aim=JsonUtility.ToJson(new Aim{angle=row.angle,power=100,direction=row.direction});Call(current,"BeginAim","");Call(current,"PreviewLaunch",aim);Call(current,"BeginSpinCharge","");
            for(int i=0;i<601;i++){Call(current,"AddSpinWork",JsonUtility.ToJson(new Work{distance=row.rate/60,seconds=1f/60,direction=row.direction}));Call(current,"Tick",1f/60);}row.work=F(State(current),"chargeWork");Call(current,"ReleaseLaunch",aim);var sim=Property(current,"ReviewSimulation");
            row.landingRejected=!(bool)Call(sim,"SetKickPressed",true);Call(sim,"SetKickPressed",false);
            for(int i=0;i<6000&&!(bool)Property(current,"Finished");i++)Call(current,"Tick",1f/60);
            var state=State(current);row.credit=F(state,"rotorCapacity");row.distance=F(state,"distance");row.boosts=Convert.ToInt32(Field(state,"burstCount"));row.secondary=Convert.ToInt32(Field(state,"chainBreaks"));row.fault=(string)Field(state,"physicsFault");row.finished=(bool)Property(current,"Finished");row.peakSpeed=Convert.ToSingle(Property(sim,"MaxSpeed"));row.maxSpin=Convert.ToSingle(Property(sim,"MaxSpin"));
            var ids=new HashSet<string>();foreach(var e in (IEnumerable)Property(sim,"BurstEvents")){Assert.That(ids.Add((string)Field(e,"objectId")),Is.True,"duplicate object boost");row.spent+=F(e,"energySpent");Assert.That(F(e,"afterForward"),Is.LessThanOrEqualTo(72.001f));Assert.That(F(e,"afterForward"),Is.GreaterThan(F(e,"beforeForward")));}
            var fx=Field(current,"effects");row.visualPeak=Convert.ToInt32(Property(Property(fx,"Core"),"Peak"));Assert.That(row.visualPeak,Is.LessThanOrEqualTo(320));Assert.That(row.spent,Is.LessThanOrEqualTo(row.credit+.02f));Assert.That(row.landingRejected,Is.True);
            Call(current,"Prepare","");yield return null;var reset=State(current);row.resetClean=Convert.ToInt32(Field(reset,"fragments"))==0&&Convert.ToInt32(Field(reset,"vfxActive"))==0&&Convert.ToInt32(Field(reset,"burstCount"))==0&&F(reset,"rotorRemaining")==0;Assert.That(row.resetClean,Is.True);
            File.WriteAllText(Path.Combine(Evidence,"rotor-safety.json"),JsonUtility.ToJson(output,true));Call(current,"ShutdownReviewPhysics");current=null;yield return null;SceneManager.SetActiveScene(templateScene);var u=SceneManager.UnloadSceneAsync(scene);while(!u.isDone)yield return null;ownedScenes.Remove(scene);
            Assert.That(row.finished&&row.fault.Length==0,Is.True,"bounded stress failed at "+row.rate+"/"+row.angle+"/"+row.direction+": "+row.fault);
        }
    }

}
#endif

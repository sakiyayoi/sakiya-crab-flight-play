using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using C2=CrabCityDensityR2;

public static class NeonStage45Review {
 static string Root=>Path.GetFullPath("../../../..");
 [Serializable]class Counts {public int mallTargets,ground,mid,high,decorColliders,decorBodies,neonColliders,neonBodies,neonJoints,heroCopies,sourceVisible,legs,glows;}
 [Serializable]class Receipt {
  public string revision=NeonProto.Revision,scope="Native Unity MainCamera renders; staged overview/pose views; accepted-input fixed replay (seed 42043). Not an ordinary-input or performance test.",error;
  public NeonBackdropAudit backdrop;public NeonDensityReview.Audit density;public int addedBreaks;public string densityEvidence="Original target signatures and panel counts before/after; same-camera additions on/off screenshots.";
  public NeonPrototypeState stagedSpin;public bool vortexBoundariesPass,oldSpinSuppressed;public int maxSpinSparks;
  public Counts counts;public int ticks,breaks,maxStrongGlows;
  public float minimumDistance=2000;
  public bool physicalTransformsUnchanged,randomStateUnchanged,poseChanges,countsPass,replayPass;
  public FlightState state;
 }
 public static void Candidate(){
  string dir=Root+"/evidence/stage45-native-shots03";
  if(Directory.Exists(dir))throw new Exception("Use a fresh stage45 review directory");
  Directory.CreateDirectory(dir);var receipt=new Receipt();CrabFlightGame game=null;
  try{
   // Baseline was captured successfully in neon-review06 with additions disabled.
   // A second scene in that process invalidates the legacy R9 static mesh cache;
   // compare the saved world snapshot in a fresh process instead.
   var baseline=JsonUtility.FromJson<NeonDensityReview.World>(File.ReadAllText(Root+"/evidence/neon-shots06/stage4-world-before.json"));
   NeonProto.Enabled=MallDensityProto.Enabled=NeonDensity.Enabled=true;
   UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   receipt.density=NeonDensityReview.Compare(baseline,game);
   File.WriteAllText(dir+"/stage45-density-audit.json",JsonUtility.ToJson(receipt.density,true));
   if(!receipt.density.pass)throw new Exception("Stage4 density registration/count/original target contract failed");
   receipt.backdrop=game.ReviewBackdropAudit;
   if(!receipt.backdrop.allSectorsPresent||!receipt.backdrop.allLayersPresent||!receipt.backdrop.otherRegionsUnchanged||!receipt.backdrop.protectedMaterialsUnchanged)throw new Exception("Stage3 backdrop isolation failed");
   File.WriteAllText(dir+"/backdrop-audit.json",JsonUtility.ToJson(receipt.backdrop,true));
   var world=(RigidCrabFlight)typeof(CrabFlightGame).GetField("world",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
   var targets=world.Scene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<C2.CityVisualBreakable>(true)).Where(t=>t.transform.position.x>=NeonProto.Start&&t.transform.position.x<NeonProto.End).ToArray();
   var decor=GameObject.Find("DensityProto static mall decor");var neon=game.transform.GetComponentsInChildren<Transform>(true).Where(t=>t.name.StartsWith("Neon ")).SelectMany(t=>t.GetComponentsInChildren<Transform>(true)).Distinct().ToArray();
   var visual=game.transform.Find("Neon crab render only");var section=game.transform.Find("Neon world signs render only");
   if(!decor||!visual||!section)throw new Exception("Missing expected render roots");
   receipt.counts=new Counts{mallTargets=targets.Length,ground=targets.Count(t=>t.transform.position.y<2),mid=targets.Count(t=>t.transform.position.y>=2&&t.transform.position.y<20),high=targets.Count(t=>t.transform.position.y>=20),decorColliders=decor.GetComponentsInChildren<Collider>(true).Length,decorBodies=decor.GetComponentsInChildren<Rigidbody>(true).Length,neonColliders=neon.Sum(t=>t.GetComponents<Collider>().Length),neonBodies=neon.Sum(t=>t.GetComponents<Rigidbody>().Length),neonJoints=neon.Sum(t=>t.GetComponents<Joint>().Length),heroCopies=visual.GetComponentsInChildren<Transform>(true).Count(t=>t.name.StartsWith("HeroR")),sourceVisible=game.ReviewCrab.Root.GetComponentsInChildren<Renderer>(true).Count(r=>r.enabled),legs=game.State().neon.legs,glows=game.State().neon.glow};
   var c=receipt.counts;receipt.countsPass=c.mallTargets==74&&c.ground==54&&c.mid==14&&c.high==6&&c.decorColliders==0&&c.decorBodies==0&&c.neonColliders==0&&c.neonBodies==0&&c.neonJoints==0&&c.heroCopies==0&&c.sourceVisible==0&&c.legs==8&&c.glows==65;
   File.WriteAllLines(dir+"/mall-targets.tsv",new[]{"id\tx\ty\tz"}.Concat(targets.OrderBy(t=>t.transform.position.x).Select(t=>t.placementId+"\t"+t.transform.position.x+"\t"+t.transform.position.y+"\t"+t.transform.position.z)));
   var transforms=game.ReviewCrab.Root.GetComponentsInChildren<Transform>(true);string before=TransformBits(transforms),random=JsonUtility.ToJson(UnityEngine.Random.state);
   var beforePose=visual.GetComponentsInChildren<Transform>(true).Select(t=>t.localRotation).ToArray();
   for(int k=0;k<600;k++){game.ReviewSampleNeon(1f/60,"flight",0,new Vector3(30,-24,0));game.ReviewSampleVortex(1f/60,"flight",k<300?20:-20,k*120,false,true);receipt.maxStrongGlows=Math.Max(receipt.maxStrongGlows,game.State().neon.strongGlows);receipt.maxSpinSparks=Math.Max(receipt.maxSpinSparks,game.State().neon.spinSparks);}
   receipt.physicalTransformsUnchanged=before==TransformBits(transforms);receipt.randomStateUnchanged=random==JsonUtility.ToJson(UnityEngine.Random.state);
   receipt.poseChanges=visual.GetComponentsInChildren<Transform>(true).Select(t=>t.localRotation).Where((q,i)=>q!=beforePose[i]).Any();
   if(!receipt.countsPass||!receipt.physicalTransformsUnchanged||!receipt.randomStateUnchanged||!receipt.poseChanges||receipt.maxStrongGlows>NeonSectionArt.MaxStrongGlows)throw new Exception("Neon render-only/count contract failed");
   var camera=game.ReviewCamera;camera.aspect=1.6f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
   receipt.vortexBoundariesPass=true;
   foreach(var boundary in new[]{"reduced","off","idle","slow","disabled"}){
    NeonProto.Enabled=boundary!="disabled";
    game.ReviewSampleVortex(1f/60,boundary=="idle"?"ready":"flight",boundary=="slow"?1:20,90,boundary=="reduced",boundary!="off");
    var n=game.State().neon;receipt.vortexBoundariesPass&=!n.spinVortexActive&&n.spinBands==0&&n.spinSparks==0&&!n.spinGlow;
    NeonProto.Enabled=true;game.ReviewSampleVortex(1f/60,"flight",20,90,false,true);
   }
   game.ReviewSampleVortex(1f/60,"ready",-20,90,false,true,true);
   receipt.vortexBoundariesPass&=game.State().neon.spinVortexActive&&game.State().neon.spinBands==0&&game.State().neon.spinGlow;
   if(!receipt.vortexBoundariesPass||receipt.maxSpinSparks>24||receipt.maxSpinSparks==0)throw new Exception("Vortex display boundary or pool failed");
   game.ReviewSampleVortex(0,"ready",0,0,false,true);
   foreach(float x in new[]{43f,79,118}){camera.orthographicSize=x==118?14:11;camera.transform.position=new Vector3(x,x==118?14:11,-30);game.ReviewCrab.Root.position=new Vector3(x-10,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);Capture(camera,dir+"/overview-"+x+".png");if(x==79){game.ReviewBackdropDark(false);Capture(camera,dir+"/overview-79-background-before.png");game.ReviewBackdropDark(true);}}
   camera.orthographicSize=3.8f;camera.transform.position=new Vector3(43,5,-18);game.ReviewCrab.Root.position=new Vector3(43,2.5f,0);
   game.ReviewSampleNeon(0,"ready",0,Vector3.zero);Capture(camera,dir+"/crab-idle.png");
   game.ReviewSampleNeon(.08f,"ready",1,Vector3.zero);Capture(camera,dir+"/crab-charge.png");
   game.ReviewNeonLaunch();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,20,0));Capture(camera,dir+"/crab-launch.png");
   game.ReviewNeonLand();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,-20,0));game.ReviewSampleNeon(.08f,"flight",0,Vector3.zero);Capture(camera,dir+"/crab-land.png");
   game.ReviewSpinVortex(20);receipt.stagedSpin=game.State().neon;
   if(!receipt.stagedSpin.spinVortexActive||receipt.stagedSpin.spinBands!=0||receipt.stagedSpin.spinSparks<1||!receipt.stagedSpin.spinGlow)throw new Exception("Staged vortex missing");
   NeonStage45Identify.Frame finalFrame=NeonStage45Identify.Capture(game,dir,"stage45-spin-native");File.WriteAllText(dir+"/stage45-spin-frame.json",JsonUtility.ToJson(finalFrame,true));game.ReviewSampleVortex(0,"ready",0,0,false,true);
   Capture(camera,dir+"/stage45-spin-off.png");
   NeonDensityReview.CaptureSectors(game,dir);
   foreach(var file in Directory.GetFiles(dir,"stage4-*"))File.Move(file,Path.Combine(dir,Path.GetFileName(file).Replace("stage4-","stage45-")));
   receipt.oldSpinSuppressed=OverdrivePresentation.NeonSpinStyle;
   game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));UnityEngine.Random.InitState(42043);game.ReviewReplayAcceptedCalmR3(File.ReadAllText(Root+"/accepted-input.json"));
   var marks=new[]{32f,57,82,105,123};int mark=0;
   for(int k=0;k<18000&&!game.Finished;k++){
    game.Tick(1f/60);receipt.ticks++;var s=game.State();receipt.oldSpinSuppressed&=!game.ReviewOldSpinWakeEnabled&&!s.neon.legacyRotorVisible;
    if(mark<marks.Length&&s.worldX>=marks[mark]){Capture(camera,dir+"/flight-"+marks[mark]+".png");File.WriteAllText(dir+"/flight-"+marks[mark]+".json",JsonUtility.ToJson(s,true));mark++;}
   }
   receipt.state=game.State();receipt.breaks=game.ReviewSimulation.BreakEvents.Count;receipt.addedBreaks=game.ReviewSimulation.BreakEvents.Count(e=>NeonDensity.IsAddition(e.id));
   File.WriteAllLines(dir+"/break-events.jsonl",game.ReviewSimulation.BreakEvents.Select(e=>JsonUtility.ToJson(e)));Capture(camera,dir+"/result.png");
   receipt.replayPass=receipt.oldSpinSuppressed&&game.Finished&&string.IsNullOrEmpty(receipt.state.physicsFault)&&receipt.ticks==4128&&receipt.state.distance==3220.297119140625&&receipt.breaks==124&&receipt.state.phase=="result"&&receipt.addedBreaks==76&&mark==marks.Length;
   if(!receipt.replayPass)throw new Exception("Stage4 fixed-input completion: distance="+receipt.state.distance+", breaks="+receipt.breaks+", fault="+receipt.state.physicsFault);
   NeonStage45ColorReview.Check(game,dir);
  }catch(Exception e){receipt.error=e.ToString();throw;}
  finally{File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }
 static string TransformBits(Transform[] ts)=>string.Join("|",ts.SelectMany(t=>new[]{t.localPosition.x,t.localPosition.y,t.localPosition.z,t.localRotation.x,t.localRotation.y,t.localRotation.z,t.localRotation.w,t.localScale.x,t.localScale.y,t.localScale.z}).Select(BitConverter.SingleToInt32Bits));
 public static void Capture(Camera camera,string path)=>MallDensityReview3.Capture(camera,path);
}

using System;
using System.IO;
using System.Linq;
using UnityEngine;

// Each series freezes the simulation, cue clock and all transforms before isolation.
public static class NeonStage46Identify {
 [Serializable]public class Frame {public string file;public Vector3 crabCenterPixels,bodyCenterPixels;public string coreColor,cues;public bool vortexGlow,crabGlow;public int visibleCore;}
 [Serializable]class Series {public string name;public bool sameTransforms;public Frame[] frames;public FlightState state;public float frozenCueTime;}
 [Serializable]class Receipt {public string revision=NeonProto.Revision,error;public float stagedSpin=20;public Series[] series;}
 public static void Run(){
  var dir=Path.GetFullPath("../../../../evidence/stage46-identify-shots01");
  if(Directory.Exists(dir))throw new Exception("Use a new identification directory");Directory.CreateDirectory(dir);
  var receipt=new Receipt();CrabFlightGame game=null;
  try{
   NeonProto.Enabled=MallDensityProto.Enabled=NeonDensity.Enabled=true;UnityEngine.Random.InitState(42043);
   game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   for(int k=0;k<600;k++){game.ReviewSampleNeon(1f/60,"flight",0,new Vector3(30,-24,0));game.ReviewSampleVortex(1f/60,"flight",k<300?20:-20,k*120,false,true);}
   foreach(float x in new[]{43f,79,118}){game.ReviewCrab.Root.position=new Vector3(x-10,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);}
   game.ReviewCrab.Root.position=new Vector3(43,2.5f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   game.ReviewSampleNeon(.08f,"ready",1,Vector3.zero);game.ReviewNeonLaunch();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,20,0));
   game.ReviewNeonLand();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,-20,0));game.ReviewSampleNeon(.08f,"flight",0,Vector3.zero);
   game.ReviewSpinVortex(20);
   receipt.series=new Series[2];receipt.series[0]=Isolate(game,dir,"stage46-id", "staged spin=20; original stage45 staging");
   // Also isolate the actual early-flight cues. The staged pose above has no loaded Feel cues.
   // Use real fixed-input events, never hand-created particles or artificial ghost geometry.
   game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));UnityEngine.Random.InitState(42043);
   game.ReviewReplayAcceptedCalmR3(File.ReadAllText("../../../../accepted-input.json"));
   for(int i=0;i<30;i++){game.Tick(1f/60);var f=game.GetComponentInChildren<MashimashiPresentationR27>();if(f&&f.ReadResources(Time.realtimeSinceStartup).ghosts>0)break;}
   receipt.series[1]=Isolate(game,dir,"stage46-live-id","actual accepted-input early flight; no ticks within isolation series");
  }catch(Exception e){receipt.error=e.ToString();throw;}
  finally{File.WriteAllText(dir+"/stage46-identification.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }
 static Series Isolate(CrabFlightGame game,string dir,string prefix,string description){
  var camera=game.ReviewCamera;camera.aspect=1.6f;
  var ts=game.transform.GetComponentsInChildren<Transform>(true);string before=Bits(ts);
  var cue=game.GetComponentInChildren<MashimashiPresentationR27>();float frozen=Time.realtimeSinceStartup;
  var core=Core(game);var enabled=core.Select(r=>r.enabled).ToArray();
  string[] names={"baseline","nofan","novortexglow","nocrabglow","nocore","noghost","nobubbles"};
  var result=new Series{name=description,frozenCueTime=frozen,frames=new Frame[names.Length],state=game.State()};
  for(int i=0;i<names.Length;i++){
   game.ReviewStage45Isolation(false,i==2,i==3);
   for(int k=0;k<core.Length;k++)core[k].enabled=i==4?false:enabled[k];
   if(cue)cue.ReviewFrozenDraw(camera,frozen,i==1,i==5,i==6);
   result.frames[i]=Capture(game,dir,prefix+"-"+names[i]);
   if(Bits(ts)!=before)throw new Exception("Isolation changed transforms: "+names[i]);
  }
  game.ReviewStage45Isolation();for(int k=0;k<core.Length;k++)core[k].enabled=enabled[k];
  if(cue)cue.ReviewFrozenDraw(camera,frozen,false,false,false);
  result.sameTransforms=true;return result;
 }
 static Renderer[] Core(CrabFlightGame g)=>g.transform.Find("Neon crab render only").GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterial&&r.sharedMaterial.name=="Neon dark core").ToArray();
 static string Bits(Transform[] ts)=>string.Join("|",ts.SelectMany(t=>new[]{t.position.x,t.position.y,t.position.z,t.rotation.x,t.rotation.y,t.rotation.z,t.rotation.w,t.localScale.x,t.localScale.y,t.localScale.z}).Select(BitConverter.SingleToInt32Bits));
 public static Frame Capture(CrabFlightGame game,string dir,string name){
  var camera=game.ReviewCamera;var core=Core(game);var glow=game.transform.Find("Neon crab back glow").GetComponent<Renderer>();var cue=game.GetComponentInChildren<MashimashiPresentationR27>();
  Func<Vector3,Vector3> project=p=>{var v=camera.WorldToViewportPoint(p);return new Vector3(v.x*1280,(1-v.y)*800,v.z);};
  var frame=new Frame{file=name+".png",crabCenterPixels=project(game.ReviewCrab.Root.position),bodyCenterPixels=project(game.ReviewCrab.Body.position),coreColor=ColorUtility.ToHtmlStringRGBA(core[0].sharedMaterial.color),vortexGlow=game.State().neon.spinGlow,crabGlow=glow.enabled,visibleCore=core.Count(r=>r.enabled),cues=cue?cue.ReviewCueInventory():"No Feel renderer loaded"};
  NeonProtoReview.Capture(camera,dir+"/"+frame.file);return frame;
 }
}

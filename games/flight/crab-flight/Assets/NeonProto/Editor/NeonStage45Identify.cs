using System;
using System.IO;
using System.Linq;
using UnityEngine;

// No simulation/time advancement between the four renders. Overrides are Editor-only.
public static class NeonStage45Identify {
 [Serializable]public class Frame {public string file;public Vector3 crabCenterPixels,bodyCenterPixels;public string coreColor;public bool vortexGlow,crabGlow;public string anatomy;}
 [Serializable]class Receipt {public string revision=NeonProto.Revision,error;public float stagedSpin=20;public bool sameTransforms;public Frame[] frames;}
 public static void Run(){
  var dir=Path.GetFullPath("../../../../evidence/stage45-identify-shots01");
  if(Directory.Exists(dir))throw new Exception("Use a new identification directory");Directory.CreateDirectory(dir);
  var receipt=new Receipt();CrabFlightGame game=null;
  try{
   NeonProto.Enabled=MallDensityProto.Enabled=NeonDensity.Enabled=true;UnityEngine.Random.InitState(42043);
   game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   // Match stage4 Candidate's visual sample history before ReviewSpinVortex(20).
   for(int k=0;k<600;k++){game.ReviewSampleNeon(1f/60,"flight",0,new Vector3(30,-24,0));game.ReviewSampleVortex(1f/60,"flight",k<300?20:-20,k*120,false,true);}
   foreach(float x in new[]{43f,79,118}){game.ReviewCrab.Root.position=new Vector3(x-10,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);}
   game.ReviewCrab.Root.position=new Vector3(43,2.5f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   game.ReviewSampleNeon(.08f,"ready",1,Vector3.zero);game.ReviewNeonLaunch();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,20,0));
   game.ReviewNeonLand();game.ReviewSampleNeon(.08f,"flight",0,new Vector3(30,-20,0));game.ReviewSampleNeon(.08f,"flight",0,Vector3.zero);
   game.ReviewSpinVortex(20);
   var transforms=game.transform.Find("Neon crab render only").GetComponentsInChildren<Transform>(true);
   string before=Bits(transforms);receipt.frames=new Frame[4];
   string[] names={"baseline","corered","novortexglow","nocrabglow"};
   for(int i=0;i<4;i++){
    game.ReviewStage45Isolation(i==1,i==2,i==3);
    receipt.frames[i]=Capture(game,dir,"stage45-id-"+names[i]);
    if(Bits(transforms)!=before)throw new Exception("Isolation capture changed visual transforms");
   }
   receipt.sameTransforms=true;game.ReviewStage45Isolation();
  }catch(Exception e){receipt.error=e.ToString();throw;}
  finally{File.WriteAllText(dir+"/stage45-identification.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }
 static string Bits(Transform[] ts)=>string.Join("|",ts.SelectMany(t=>new[]{t.position.x,t.position.y,t.position.z,t.rotation.x,t.rotation.y,t.rotation.z,t.rotation.w,t.localScale.x,t.localScale.y,t.localScale.z}).Select(BitConverter.SingleToInt32Bits));
 public static Frame Capture(CrabFlightGame game,string dir,string name){
  var camera=game.ReviewCamera;var visual=game.transform.Find("Neon crab render only");
  var core=visual.GetComponentsInChildren<Renderer>(true).Where(r=>r.sharedMaterial&&r.sharedMaterial.name=="Neon dark core").ToArray();
  var glow=game.transform.Find("Neon crab back glow").GetComponent<Renderer>();
  // Camera viewport is independent of Editor window size. PNG is 1280x800, top-left origin.
  Func<Vector3,Vector3> project=p=>{var v=camera.WorldToViewportPoint(p);return new Vector3(v.x*1280,(1-v.y)*800,v.z);};
  var frame=new Frame{file=name+".png",crabCenterPixels=project(game.ReviewCrab.Root.position),bodyCenterPixels=project(game.ReviewCrab.Body.position),coreColor=ColorUtility.ToHtmlStringRGBA(core[0].sharedMaterial.color),vortexGlow=game.State().neon.spinGlow,crabGlow=glow.enabled,anatomy=string.Join("; ",core.Select(r=>r.name+" vertices="+r.GetComponent<MeshFilter>().sharedMesh.vertexCount))};
  NeonProtoReview.Capture(camera,dir+"/"+frame.file);return frame;
 }
}

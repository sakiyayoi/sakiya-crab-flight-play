using System;using System.IO;using System.Linq;using System.Collections.Generic;using UnityEngine;
public static class LifeReviewR6 {
 [Serializable]class Receipt{public string scope="Native actual legal saved launch/contact/energy/reset; not WebGL/FPS/hand operation",error,audit,final,retry;public bool finished,resetClean;public int ticks,uniqueBreaks,maxFragments,maxCity;}
 public static void Run(){
  var dir=Path.GetFullPath("../../../../life-r6-evidence/native01");Directory.CreateDirectory(dir);var result=new Receipt();CrabFlightGame game=null;
  try{game=BuildFlight.PrepareScene();game.Initialize();game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));game.ReviewReplayAcceptedCalmR3(File.ReadAllText("../../../../accepted-input-r2b.json"));var camera=game.ReviewCamera;camera.aspect=1.6f;
   bool shot=false;float shotAt=0;
   for(;result.ticks<18000&&!game.Finished;result.ticks++){
    game.Tick(1f/60);var s=game.State();result.maxFragments=Mathf.Max(result.maxFragments,s.fragments);result.maxCity=Mathf.Max(result.maxCity,s.cityFragments);
    if(!shot&&s.peopleHit+s.birdsHit>0){shot=true;shotAt=s.time;Capture(camera,dir,"contact");}
    if(shot&&shotAt>0&&s.time>=shotAt+.6f){Capture(camera,dir,"contact-plus-0.6");shotAt=0;}
   }
   var state=game.State();result.finished=game.Finished;result.final=JsonUtility.ToJson(state);result.audit=game.ReviewSimulation.ReadLifeScatterR6();result.uniqueBreaks=game.ReviewSimulation.BrokenObjects.Distinct().Count();
   Capture(camera,dir,"result");if(!game.Finished||state.physicsFault.Length>0)throw new Exception("Native loop failed "+state.physicsFault);
   if(game.ReviewSimulation.ScatterMaxEnergyExcessR6>0)throw new Exception("Rotational+linear energy exceeded budget");
   if(result.uniqueBreaks!=state.destructionEvent)throw new Exception("Duplicate actual score event");
   if(result.maxFragments>48||result.maxCity>20)throw new Exception("Existing fragment caps exceeded");
   game.Prepare("");var r=game.State();result.retry=JsonUtility.ToJson(r);result.resetClean=r.phase=="ready"&&r.fragments==0&&r.cityFragments==0&&r.peopleHit==0&&r.birdsHit==0;Capture(camera,dir,"retry");if(!result.resetClean)throw new Exception("Life retry residual");
  }catch(Exception e){result.error=e.ToString();throw;}finally{File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(result,true));if(game)game.ShutdownReviewPhysics();}
 }
 static void Capture(Camera camera,string dir,string name){var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(dir+"/"+name+".png",im.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
}

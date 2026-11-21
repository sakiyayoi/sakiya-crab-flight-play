using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class KingBoldReviewR2 {
 [Serializable] class Receipt { public string scope="Native king-bold R2 actual geometry/ready/throw/reset, not WebGL, FPS or M14",error,ready,launch,final,retry;public int ticks,parts,meshes;public bool modelActive,finished,retryReady; }
 public static void Run(){
  var output=Path.GetFullPath("../../../../king-r2-evidence/native01");Directory.CreateDirectory(output);
  var result=new Receipt();CrabFlightGame game=null;
  try{
   UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));
   var root=GameObject.Find("FlightCrab");if(!root)throw new Exception("No actual model root");
   result.modelActive=root.GetComponentsInChildren<Transform>().Any(t=>t.name=="CarapacePart");
   if(!result.modelActive)throw new Exception("King-bold flag/model not active; legacy model is not a pass");
   result.meshes=root.GetComponentsInChildren<MeshFilter>().Length;
   game.Configure("{\"claw\":45,\"shell\":45,\"legs\":45,\"angle\":40,\"power\":70,\"direction\":1,\"gear\":0,\"upgradeLevel\":0}");
   var camera=UnityEngine.Object.FindFirstObjectByType<Camera>();camera.aspect=1.6f;game.RefreshReviewCamera();
   result.ready=JsonUtility.ToJson(game.State());Capture(camera,output,"ready");
   game.BeginSpinCharge("");
   for(int i=0;i<605;i++){if(i%9==0)game.AddSpinWork("{\"distance\":0.25,\"seconds\":0.12,\"direction\":1}");game.Tick(1f/60);}
   if(!game.State().chargeReady)throw new Exception("Charge did not become ready");
   game.BeginAim("");game.ReleaseLaunch("{\"angle\":40,\"power\":70,\"direction\":1,\"eligible\":true,\"spinRps\":8.4}");
   result.launch=JsonUtility.ToJson(game.State());
   if(game.State().phase!="flight")throw new Exception("Native actual launch was not accepted");
   for(;result.ticks<18000&&!game.Finished;result.ticks++){game.Tick(1f/60);if(result.ticks==30)Capture(camera,output,"flight");}
   result.finished=game.Finished;result.final=JsonUtility.ToJson(game.State());Capture(camera,output,"result");
   if(!game.Finished||game.State().physicsFault.Length>0)throw new Exception("Native flight not safely completed: "+game.State().physicsFault);
   game.Prepare("");result.retry=JsonUtility.ToJson(game.State());result.retryReady=game.State().phase=="ready"&&!game.State().chargeStarted;
   if(!result.retryReady)throw new Exception("Retry did not reset");
  }catch(Exception e){result.error=e.ToString();throw;}
  finally{File.WriteAllText(Path.Combine(output,"receipt.json"),JsonUtility.ToJson(result,true));if(game){game.ShutdownReviewPhysics();UnityEngine.Object.DestroyImmediate(game.gameObject);}}
 }
 static void Capture(Camera camera,string output,string name){
  var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,800,24);var image=new Texture2D(1280,800,TextureFormat.RGB24,false);
  camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,1280,800),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());
  camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(rt);
 }
}

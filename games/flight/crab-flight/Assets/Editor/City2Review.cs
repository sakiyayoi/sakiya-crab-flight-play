using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public static class City2Review {
 [Serializable] public class Row {public int mode,trial,targets,cityBreaks,secondaryBreaks,breaks,maxFragments,maxCity,maxDecor,maxVfx,hitstopRequests;public float secondaryFraction;public float distance,simSeconds,wallSeconds,stopSeconds,outsideSpeed,retryMs;public bool finished,resetClean,uniqueBreaks,shaderOK;public string[] broken;}
 [Serializable] public class Result {public string scope="r1/readable/dense same angle10,power85,parts45,10s active charge. Solver variation is recorded, not hidden.";public List<Row> runs=new List<Row>();public int fbxImported;public bool chainHit,chainMiss;}
 static string dir="../evidence/city2";
 static CrabFlightGame Create(int mode,float fraction=.25f){CrabModel.ReviewUseBaseline(false);var g=BuildFlight.PrepareScene();g.cityLayout=mode;g.secondaryThresholdFraction=fraction;g.Initialize();return g;}
 static void Charge(CrabFlightGame g){g.BeginSpinCharge("");for(int i=0;i<601;i++){g.AddSpinWork(JsonUtility.ToJson(new SpinWorkInput{distance=2f/60,seconds=1f/60,direction=1}));g.Tick(1f/60);}g.BeginAim("");g.ReleaseLaunch(JsonUtility.ToJson(new FlightAimInput{angle=10,power=85,direction=1,eligible=true}));}
 public static void Run(){
  Directory.CreateDirectory(dir);var result=new Result();
  foreach(string file in Directory.GetFiles("Assets/CrabCityDensityR2/Models","*.fbx",SearchOption.AllDirectories)){
   var meshes=AssetDatabase.LoadAllAssetsAtPath(file).OfType<Mesh>().ToArray();if(meshes.Length==0||meshes.Any(m=>m.vertexCount==0||m.uv.Length!=m.vertexCount))throw new Exception("City2 FBX import/UV failed: "+Path.GetFileName(file));result.fbxImported++;
  }if(result.fbxImported!=20)throw new Exception("Expected20 city FBX");
  for(int mode=0;mode<3;mode++)for(int n=0;n<3;n++){
   var g=Create(mode);var row=new Row{mode=mode,trial=n};bool shot35=false,shot65=false;
   try{
    if(n==0){var cam=g.ReviewCamera;cam.transform.position=new Vector3(38,7.8f,-16);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));cam.orthographicSize=6.6f;RigidFlightReview.Capture(g,dir+"/mode"+mode+"-samecamera.png",844,390);}
    Charge(g);row.targets=g.State().cityTargets;
    for(int i=0;i<4800&&!g.Finished;i++){g.Tick(1f/60);row.wallSeconds+=1f/60;var s=g.State();row.maxFragments=Mathf.Max(row.maxFragments,s.fragments);row.maxCity=Mathf.Max(row.maxCity,s.cityFragments);row.maxDecor=Mathf.Max(row.maxDecor,s.cityDecorative);row.maxVfx=Mathf.Max(row.maxVfx,s.vfxActive);if(n==0&&!shot35&&s.worldX>35){RigidFlightReview.Capture(g,dir+"/mode"+mode+"-exit.png",844,390);row.outsideSpeed=s.speed;shot35=true;}if(n==0&&!shot65&&s.worldX>65){RigidFlightReview.Capture(g,dir+"/mode"+mode+"-street.png",844,390);shot65=true;}}
    var end=g.State();row.finished=g.Finished&&end.physicsFault.Length==0;row.distance=end.distance;row.simSeconds=end.time;row.stopSeconds=end.hitStopSeconds;row.hitstopRequests=end.hitStopRequests;row.breaks=end.destructionEvent;row.secondaryBreaks=end.chainBreaks;row.broken=g.ReviewSimulation.BrokenObjects.ToArray();row.cityBreaks=row.broken.Count(x=>x.StartsWith("contact-")||x.StartsWith("optional-contact"));row.uniqueBreaks=row.broken.Distinct().Count()==row.broken.Length;row.shaderOK=mode==0||City2Data.Shader.isSupported;
    var watch=System.Diagnostics.Stopwatch.StartNew();g.Prepare("");watch.Stop();row.retryMs=(float)watch.Elapsed.TotalMilliseconds;var reset=g.State();row.resetClean=reset.fragments==0&&reset.cityFragments==0&&reset.cityDecorative==0&&reset.chainBreaks==0&&reset.vfxActive==0&&reset.hitStopRemaining==0;
    result.runs.Add(row);Save(result);if(!row.finished||!row.uniqueBreaks||!row.resetClean||!row.shaderOK||row.maxFragments>48||row.maxCity>(mode==0?12:20)||row.maxDecor>12||row.maxVfx>320)throw new Exception("City2 run gate mode"+mode+" trial"+n);
   }finally{g.ShutdownReviewPhysics();}
  }
  foreach(bool hit in new[]{false,true}){var g=Create(2);try{Charge(g);g.ReviewSimulation.ReviewChainProjectile(hit);for(int i=0;i<90;i++)g.Tick(1f/120);bool observed=g.State().chainBreaks>0;if(hit)result.chainHit=observed;else result.chainMiss=!observed;Save(result);}finally{g.ShutdownReviewPhysics();}}
  if(!result.chainHit||!result.chainMiss)throw new Exception("Actual debris contact chain fixture failed");
 }
 public static void Tuned(){
  dir="../evidence/city2-tuning";Directory.CreateDirectory(dir);var result=new Result();
  foreach(float fraction in new[]{1f,.25f})for(int n=0;n<3;n++){
   var g=Create(2,fraction);var row=new Row{mode=2,trial=n,secondaryFraction=fraction};bool shot=false;
   try{
    if(n==0){var cam=g.ReviewCamera;cam.aspect=844f/390;cam.transform.position=new Vector3(38,7.8f,-16);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));cam.orthographicSize=6.6f;CaptureFixed(g,dir+"/fraction"+fraction+"-world-fixed.png");}
    Charge(g);row.targets=g.State().cityTargets;
    for(int i=0;i<4800&&!g.Finished;i++){g.Tick(1f/60);row.wallSeconds+=1f/60;var s=g.State();row.maxFragments=Mathf.Max(row.maxFragments,s.fragments);row.maxCity=Mathf.Max(row.maxCity,s.cityFragments);row.maxDecor=Mathf.Max(row.maxDecor,s.cityDecorative);row.maxVfx=Mathf.Max(row.maxVfx,s.vfxActive);if(!shot&&n==0&&s.worldX>38){RigidFlightReview.Capture(g,dir+"/fraction"+fraction+"-play.png",844,390);shot=true;}}
    var s2=g.State();row.distance=s2.distance;row.simSeconds=s2.time;row.secondaryBreaks=s2.chainBreaks;row.breaks=s2.destructionEvent;row.stopSeconds=s2.hitStopSeconds;row.hitstopRequests=s2.hitStopRequests;row.finished=g.Finished&&s2.physicsFault.Length==0;row.broken=g.ReviewSimulation.BrokenObjects.ToArray();row.cityBreaks=row.broken.Count(x=>x.StartsWith("contact-"));row.uniqueBreaks=row.broken.Distinct().Count()==row.broken.Length;g.Prepare("");row.resetClean=g.State().fragments==0&&g.State().chainBreaks==0&&g.State().cityDecorative==0&&g.State().hitStopRemaining==0;result.runs.Add(row);Save(result);if(!row.finished||!row.resetClean||!row.uniqueBreaks||row.maxFragments>48||row.maxCity>20||row.maxDecor>12)throw new Exception("City2 tuned gate");
   }finally{g.ShutdownReviewPhysics();}
  }
  BuildFlight.DefaultCityLayout=2;BuildFlight.DefaultSecondaryFraction=.25f;CrabModel.ReviewUseBaseline(false);ChargeReview.Review();
 }
 static void CaptureFixed(CrabFlightGame g,string path){var cam=g.ReviewCamera;var target=new RenderTexture(844,390,24);var prev=RenderTexture.active;cam.targetTexture=target;cam.Render();RenderTexture.active=target;var image=new Texture2D(844,390,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,844,390),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=prev;UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);}
 public static void FinalDense(){
  dir="../evidence/city2-final";Directory.CreateDirectory(dir);
  foreach(string name in new[]{"CityAlbedoAtlas","CityMetalSmoothAtlas"}){var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/CrabCityDensityR2/Textures/"+name+".png");if(importer.sRGBTexture!=(name=="CityAlbedoAtlas")||importer.mipmapEnabled||importer.maxTextureSize!=1024||importer.textureCompression!=TextureImporterCompression.Uncompressed)throw new Exception("City atlas import mismatch");}
  foreach(int mode in new[]{0,1,2}){var g=Create(mode);try{var cam=g.ReviewCamera;cam.aspect=844f/390;cam.transform.position=new Vector3(38,7.8f,-16);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));cam.orthographicSize=6.6f;CaptureFixed(g,dir+"/mode"+mode+"-world-fixed.png");Charge(g);bool outside=false;for(int i=0;i<2400&&!g.Finished;i++){g.Tick(1f/60);if(!outside&&g.State().worldX>38){RigidFlightReview.Capture(g,dir+"/mode"+mode+"-play.png",844,390);outside=true;}}if(!g.Finished||g.State().physicsFault.Length>0)throw new Exception("Final dense playback fault");File.WriteAllText(dir+"/mode"+mode+"-state.json",JsonUtility.ToJson(g.State(),true));}finally{g.ShutdownReviewPhysics();}}
  BuildFlight.DefaultSecondaryFraction=.25f;Build(2);
 }
 public static void BuildDense(){Build(2);}public static void BuildReadable(){Build(1);}
 static void Build(int mode){BuildFlight.DefaultCityLayout=mode;CrabModel.ReviewUseBaseline(false);string path="../delivery/UnityWeb/city2-"+(mode==2?"dense":"readable");BuildFlight.Web(path);foreach(var n in new[]{"launch-input.js","sound-pack.js"})File.Copy("../web-template/"+n,path+"/"+n,true);Copy("../web-template/SoundPack",path+"/SoundPack");}
 static void Save(Result r){File.WriteAllText(dir+"/result.json",JsonUtility.ToJson(r,true));}
 static void Copy(string from,string to){Directory.CreateDirectory(to);foreach(var p in Directory.GetFiles(from))File.Copy(p,Path.Combine(to,Path.GetFileName(p)),true);foreach(var p in Directory.GetDirectories(from))Copy(p,Path.Combine(to,Path.GetFileName(p)));}
}

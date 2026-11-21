using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using C2=CrabCityDensityR2;
public static class MallDensityReview3 {
 static string Root=>Path.GetFullPath("../../../..");
 [Serializable]class Counts{public int v2Targets,mallMidTargets,mallHighTargets,decorNonStatic,targets,mallTargets,mallGroundTargets,r5Targets,addedTargets,totalGO,totalRenderers,mallGO,mallRenderers,decorGroups,decorGO,decorRenderers,decorColliders,decorBodies;}
 [Serializable]class Receipt{public string version=MallDensityProto.Revision,scope,error;public Counts counts;public int ticks,addedBreaks,v2Breaks;public string[] v2BreakEvents;public FlightState state;public string[] addedBreakEvents;}
 public static void Baseline()=>Run(false,false);
 public static void Candidate()=>Run(true,true);
 static void Run(bool enabled,bool flight){
  string dir=Root+"/evidence/"+(enabled?"native-candidate04":"native-baseline04-unused");if(Directory.Exists(dir))throw new Exception("既存の証拠は上書きしません: "+dir);Directory.CreateDirectory(dir);
  var receipt=new Receipt{scope="Native Unity initialized hierarchy and real MainCamera renders; overview poses staged. Fixed-input flight uses existing accepted replay and existing physics."};CrabFlightGame game=null;
  try{
   MallDensityProto.Enabled=enabled;UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   var w=(RigidCrabFlight)typeof(CrabFlightGame).GetField("world",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(game);
   var roots=w.Scene.GetRootGameObjects();var targets=roots.SelectMany(r=>r.GetComponentsInChildren<C2.CityVisualBreakable>(true)).ToArray();
   var mall=targets.Where(t=>t.transform.position.x>=28&&t.transform.position.x<135).ToArray();var decor=GameObject.Find("DensityProto static mall decor");
   receipt.counts=new Counts{v2Targets=mall.Count(t=>t.placementId.StartsWith("density-proto2-")),mallMidTargets=mall.Count(t=>t.transform.position.y>=2&&t.transform.position.y<20),mallHighTargets=mall.Count(t=>t.transform.position.y>=20),decorNonStatic=decor?decor.GetComponentsInChildren<Transform>(true).Count(t=>!t.gameObject.isStatic):0,targets=targets.Length,mallTargets=mall.Length,mallGroundTargets=mall.Count(t=>t.transform.position.y<2),r5Targets=mall.Count(t=>t.assetId.StartsWith("mall_r5_")&&!t.placementId.StartsWith("density-proto")),addedTargets=mall.Count(t=>t.placementId.StartsWith("density-proto")),totalGO=roots.Sum(r=>r.GetComponentsInChildren<Transform>(true).Length),totalRenderers=roots.Sum(r=>r.GetComponentsInChildren<Renderer>(true).Length),mallGO=mall.Sum(r=>r.GetComponentsInChildren<Transform>(true).Length),mallRenderers=mall.Sum(r=>r.GetComponentsInChildren<Renderer>(true).Length),decorGroups=decor?decor.transform.childCount:0,decorGO=decor?decor.GetComponentsInChildren<Transform>(true).Length:0,decorRenderers=decor?decor.GetComponentsInChildren<Renderer>(true).Length:0,decorColliders=decor?decor.GetComponentsInChildren<Collider>(true).Length:0,decorBodies=decor?decor.GetComponentsInChildren<Rigidbody>(true).Length:0};
   File.WriteAllLines(dir+"/mall-targets.tsv",new[]{"id\tasset\tx\ty\tz"}.Concat(mall.OrderBy(t=>t.transform.position.x).Select(t=>t.placementId+"\t"+t.assetId+"\t"+t.transform.position.x+"\t"+t.transform.position.y+"\t"+t.transform.position.z)));
   File.WriteAllLines(dir+"/all-targets.tsv",new[]{"id\tasset\tx\ty\tz"}.Concat(targets.OrderBy(t=>t.placementId).Select(t=>t.placementId+"\t"+t.assetId+"\t"+t.transform.position.x+"\t"+t.transform.position.y+"\t"+t.transform.position.z)));
   File.WriteAllLines(dir+"/v2-target-bounds.tsv",new[]{"id\tminX\tminY\tminZ\tmaxX\tmaxY\tmaxZ"}.Concat(mall.Where(t=>t.placementId.StartsWith("density-proto2-")).Select(t=>{
    var rs=t.intact.GetComponentsInChildren<Renderer>(true).Where(r=>r.enabled).ToArray();var b=rs[0].bounds;foreach(var r in rs.Skip(1))b.Encapsulate(r.bounds);
    if(b.min.x<28||b.max.x>=135)throw new Exception("対象形状が区間外: "+t.placementId);
    return t.placementId+"\t"+b.min.x+"\t"+b.min.y+"\t"+b.min.z+"\t"+b.max.x+"\t"+b.max.y+"\t"+b.max.z;
   })));
   File.WriteAllLines(dir+"/decor-bounds.tsv",new[]{"id\tminX\tminY\tminZ\tmaxX\tmaxY\tmaxZ"}.Concat(decor.GetComponentsInChildren<Renderer>(true).Select(r=>{
    var b=r.bounds;if((r.name.StartsWith("density2-")||r.name.StartsWith("density3-"))&&(b.min.x<28||b.max.x>=135))throw new Exception("飾りが区間外: "+r.name);
    return r.name+"\t"+b.min.x+"\t"+b.min.y+"\t"+b.min.z+"\t"+b.max.x+"\t"+b.max.y+"\t"+b.max.z;
   })));
   var camera=game.ReviewCamera;camera.aspect=1.6f;camera.orthographicSize=11;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
   foreach(float x in new[]{43f,79,118}){camera.orthographicSize=x==118?14:11;camera.transform.position=new Vector3(x,x==118?14:11,-30);game.ReviewCrab.Root.position=new Vector3(x-10,3.3f,0);Capture(camera,dir+"/overview-"+x+".png");}
   foreach(float x in new[]{60f,90,118}){camera.orthographicSize=13;camera.transform.position=new Vector3(x,20,-30);game.ReviewCrab.Root.position=new Vector3(x-10,12,0);Capture(camera,dir+"/midlevel-"+x+".png");}
   if(enabled&&(receipt.counts.addedTargets!=33||receipt.counts.v2Targets!=14||receipt.counts.mallTargets!=74||receipt.counts.mallMidTargets!=14||receipt.counts.decorGroups!=45||receipt.counts.decorNonStatic!=0||receipt.counts.decorColliders!=0||receipt.counts.decorBodies!=0))throw new Exception("Prototype count / static decoration contract failed");
   if(flight){
    game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));UnityEngine.Random.InitState(42043);game.ReviewReplayAcceptedCalmR3(File.ReadAllText(Root+"/accepted-input.json"));
    var marks=new[]{32f,57f,82f,105f,123f};int mark=0;
    for(int k=0;k<18000&&!game.Finished;k++){
     game.Tick(1f/60);receipt.ticks++;var s=game.State();
     if(mark<marks.Length&&s.worldX>=marks[mark]){Capture(game.ReviewCamera,dir+"/flight-"+marks[mark]+".png");File.WriteAllText(dir+"/flight-"+marks[mark]+".json",JsonUtility.ToJson(s,true));mark++;}
    }
    receipt.state=game.State();var events=game.ReviewSimulation.BreakEvents.Select(e=>JsonUtility.ToJson(e)).ToArray();File.WriteAllLines(dir+"/break-events.jsonl",events);receipt.addedBreakEvents=events.Where(e=>e.Contains("density-proto")).ToArray();receipt.addedBreaks=receipt.addedBreakEvents.Length;receipt.v2BreakEvents=events.Where(e=>e.Contains("density-proto2-")).ToArray();receipt.v2Breaks=receipt.v2BreakEvents.Length;
    Capture(game.ReviewCamera,dir+"/result.png");
    if(!game.Finished||receipt.state.physicsFault.Length>0)throw new Exception("Fixed-input flight failed: "+receipt.state.physicsFault);
    // v2接触0も事実として記録。飛行軌道を変えて接触を捏造しない。
   }
  }catch(Exception ex){receipt.error=ex.ToString();throw;}
  finally{File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(receipt,true));if(game)game.ShutdownReviewPhysics();}
 }
 public static void Capture(Camera c,string path){
  var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);
  try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(path,im.EncodeToPNG());}
  finally{c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 }
}

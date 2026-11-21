using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using C2=CrabCityDensityR2;

// Editor-only comparison of the live compact hierarchy with the exact former
// intact Instantiate + Part_* disable behavior. No physics/gameplay mutation.
public static class MallCompactReviewR18 {
 [Serializable] class Counts {public int targets,go,renderers,activeRenderers,fractureParts,colliders,bodies;}
 [Serializable] class Receipt {
  public string scope="Live initialized world; baseline intact assembly restored in the same world/camera. Fracture display is staged, not a flight.",error;
  public Counts baseline,candidate;
  public bool intactExact,fractureExact,colliderExact,cloneExact;
  public int templateBuilds,templateCopies,templateChecks;
 }
 static string Dir=>Path.GetFullPath("../../../../evidence/native01");
 static C2.CityVisualBreakable[] targets;
 static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
 public static void Run(){
  Directory.CreateDirectory(Dir);var r=new Receipt();CrabFlightGame game=null;
  try{
   UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   targets=UnityEngine.Object.FindObjectsByType<C2.CityVisualBreakable>(FindObjectsSortMode.None).Where(v=>v.assetId.StartsWith("mall_r5_")).OrderBy(v=>v.placementId).ToArray();
   if(targets.Length!=50)throw new Exception("Expected 50 live mall targets, found "+targets.Length);
   var crabPosition=game.ReviewCrab.Root.position;var physics=PhysicsSnapshot();r.candidate=Count();
   var intact=VisualSnapshot();CaptureAll(game,"candidate-intact");
   SetFractured(true);var fracture=VisualSnapshot();CaptureAll(game,"candidate-fractured");SetFractured(false);
   RestoreLegacyIntact();r.baseline=Count();
   r.intactExact=intact.SequenceEqual(VisualSnapshot());CaptureAll(game,"baseline-intact");
   SetFractured(true);r.fractureExact=fracture.SequenceEqual(VisualSnapshot());CaptureAll(game,"baseline-fractured");SetFractured(false);
   game.ReviewCrab.Root.position=crabPosition;r.colliderExact=physics.SequenceEqual(PhysicsSnapshot());
   var template=(ContactVisualTemplateR15)typeof(CrabFlightGame).GetField("contactVisualTemplateR15",Private).GetValue(game);
   r.templateBuilds=template.Builds;r.templateCopies=template.Copies;r.templateChecks=template.ReviewExactChecks;r.cloneExact=r.templateBuilds==1&&r.templateCopies>=1&&r.templateChecks>0;
   File.WriteAllLines(Dir+"/intact-renderers.txt",intact);File.WriteAllLines(Dir+"/fracture-renderers.txt",fracture);File.WriteAllLines(Dir+"/physics.txt",physics);
   if(r.candidate.go!=284||r.candidate.renderers!=234||r.baseline.go!=456||r.baseline.renderers!=406||!r.intactExact||!r.fractureExact||!r.colliderExact||!r.cloneExact)throw new Exception("Mall retention/count assertion failed; see receipt");
  }catch(Exception e){r.error=e.ToString();throw;}
  finally{File.WriteAllText(Dir+"/receipt.json",JsonUtility.ToJson(r,true));if(game)game.ShutdownReviewPhysics();}
 }
 static GameObject Prefab(C2.CityVisualBreakable v)=>GiftStoreR7.Prefab(v)??Resources.Load<GameObject>("MallAreasR5/"+v.assetId);
 static Transform IntactModel(C2.CityVisualBreakable v)=>v.intact.transform.Find(Prefab(v).name+"(Clone)");
 static IEnumerable<Transform> Models(C2.CityVisualBreakable v){
  yield return IntactModel(v);
  foreach(var pivot in v.shardPivots)foreach(Transform t in pivot)if(t.name.StartsWith("Part_"))yield return t;
  foreach(Transform node in v.fractured.transform)if(!node.name.StartsWith("Shard_"))foreach(Transform t in node)if(t.name.StartsWith("Part_"))yield return t;
 }
 static Counts Count(){var roots=targets.SelectMany(Models).ToArray();return new Counts{
  targets=targets.Length,go=roots.Sum(t=>t.GetComponentsInChildren<Transform>(true).Length),renderers=roots.Sum(t=>t.GetComponentsInChildren<Renderer>(true).Length),
  activeRenderers=roots.Sum(t=>t.GetComponentsInChildren<Renderer>(true).Count(x=>x.enabled&&x.gameObject.activeInHierarchy)),
  fractureParts=roots.Count(t=>t.name.StartsWith("Part_")),colliders=roots.Sum(t=>t.GetComponentsInChildren<Collider>(true).Length),bodies=roots.Sum(t=>t.GetComponentsInChildren<Rigidbody>(true).Length)};}
 static void RestoreLegacyIntact(){foreach(var v in targets){var old=IntactModel(v);if(!old)throw new Exception("Missing intact model");UnityEngine.Object.DestroyImmediate(old.gameObject);var copy=UnityEngine.Object.Instantiate(Prefab(v),v.intact.transform,false);foreach(Transform t in copy.transform)if(t.name.StartsWith("Part_"))t.gameObject.SetActive(false);}}
 static void SetFractured(bool value){foreach(var v in targets){v.intact.SetActive(!value);v.fractured.SetActive(value);}}
 static string F(float x)=>x.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
 static string Matrix(Transform t)=>string.Join(",",Enumerable.Range(0,16).Select(i=>F(t.localToWorldMatrix[i])));
 static string PathOf(Transform t){var s=t.name;while(t.parent){t=t.parent;s=t.name+"/"+s;}return s;}
 static string[] VisualSnapshot()=>targets.SelectMany(v=>v.GetComponentsInChildren<MeshRenderer>(true).Where(x=>x.enabled&&x.gameObject.activeInHierarchy).Select(x=>v.placementId+"|"+PathOf(x.transform)+"|"+AssetDatabase.GetAssetPath(x.GetComponent<MeshFilter>().sharedMesh)+"|"+string.Join(",",x.sharedMaterials.Select(m=>AssetDatabase.GetAssetPath(m)))+"|"+Matrix(x.transform)+"|"+x.shadowCastingMode+"|"+x.receiveShadows)).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
 static string[] PhysicsSnapshot()=>UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(c=>c.GetInstanceID()+"|"+PathOf(c.transform)+"|"+Matrix(c.transform)+"|"+EditorJsonUtility.ToJson(c)).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
 static void CaptureAll(CrabFlightGame g,string prefix){var c=g.ReviewCamera;c.aspect=1.6f;c.orthographicSize=9.5f;c.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));foreach(float x in new[]{45f,80f,119f}){c.transform.position=new Vector3(x,7,-26);g.ReviewCrab.Root.position=new Vector3(x-8,3.3f,0);Capture(c,prefix+"-"+MallAreasR5.Family(x));}}
 static void Capture(Camera c,string name){var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(Dir+"/"+name+".png",im.EncodeToPNG());}finally{c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}}
}

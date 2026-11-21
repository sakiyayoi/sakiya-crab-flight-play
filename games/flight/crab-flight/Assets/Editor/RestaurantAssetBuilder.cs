using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

// Only selected CC0 data enters this project. No .blend or external scripts are imported.
public static class RestaurantAssetBuilder {
 const string Src="Assets/RestaurantR2/SourceModels/",Dst="Assets/Resources/RestaurantR2/";
 [Serializable] public class Item {public string name,path;public int vertices,triangles,lowTriangles,colliders;public Vector3 bounds,pivot;public bool lodPhysicsInvariant,reopened;}
 [Serializable] public class Report {public string scope="Imported selected CC0 mesh data, actual prefab save/reimport, two LODs and one conservative primitive collider. Donor crab is comparison only; original eleven-part actor remains.";public Item[] foods;public int donorMeshCount,donorBones;public string[] donorClips,originalPartIds;public float donorWalkVertexDelta;public bool donorMotionValidated;}
 static Material atlas;
 static Material Mat(string name,Color c,Texture t=null){string p=Dst+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,p);}m.color=c;m.mainTexture=t;m.SetFloat("_Glossiness",.22f);EditorUtility.SetDirty(m);return m;}
 static Bounds BoundsOf(GameObject root){var renderers=root.GetComponentsInChildren<Renderer>();if(renderers.Length==0)throw new Exception("No renderers: "+root.name);var b=renderers[0].bounds;foreach(var x in renderers)b.Encapsulate(x.bounds);return b;}
 static Mesh Octa(string name,Vector3 size){var e=size*.5f;var m=new Mesh{name=name};m.vertices=new[]{new Vector3(e.x,0,0),new Vector3(-e.x,0,0),new Vector3(0,e.y,0),new Vector3(0,-e.y,0),new Vector3(0,0,e.z),new Vector3(0,0,-e.z)};m.triangles=new[]{2,4,0,2,1,4,2,5,1,2,0,5,3,0,4,3,4,1,3,1,5,3,5,0};m.RecalculateNormals();m.RecalculateBounds();return m;}
 public static void RepairLowNormals(){
  Directory.CreateDirectory("../evidence/assets/pre-normal-repair");
  foreach(var role in new[]{"FoodFish","FoodSushi","FoodPlateBroken"}){
   string path=Dst+role+"-LOD1.asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(!mesh)throw new Exception("Missing generated LOD mesh");
   var vertices=mesh.vertices;var indices=mesh.triangles;float sum=0;
   for(int i=0;i<indices.Length;i+=3){var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];sum+=Vector3.Dot(Vector3.Cross(b-a,c-a),(a+b+c)/3);}
   if(sum<0){string backup="../evidence/assets/pre-normal-repair/"+role+"-LOD1.asset";if(!File.Exists(backup))File.Copy(path,backup);for(int i=0;i<indices.Length;i+=3){int temp=indices[i+1];indices[i+1]=indices[i+2];indices[i+2]=temp;}mesh.triangles=indices;mesh.RecalculateNormals();EditorUtility.SetDirty(mesh);}
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var instance=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Dst+role+".prefab"));instance.GetComponent<LODGroup>().ForceLOD(1);var camera=CameraAt(Vector3.zero,.8f);Capture(camera,"../evidence/assets/"+role+"-LOD1-fixed.png");
   vertices=mesh.vertices;indices=mesh.triangles;for(int i=0;i<indices.Length;i+=3){var a=vertices[indices[i]];var b=vertices[indices[i+1]];var c=vertices[indices[i+2]];if(Vector3.Dot(Vector3.Cross(b-a,c-a),(a+b+c)/3)<=0)throw new Exception("LOD winding still inward");}
  }AssetDatabase.SaveAssets();
 }
 static void Capture(Camera c,string file,int w=640,int h=480){c.aspect=(float)w/h;var rt=RenderTexture.GetTemporary(w,h,24);var before=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(w,h,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,w,h),0,0);t.Apply();File.WriteAllBytes(file,t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=before;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(t);}
 static Camera CameraAt(Vector3 focus,float size){var c=new GameObject("Asset inspection camera").AddComponent<Camera>();c.orthographic=true;c.orthographicSize=size;c.transform.position=focus+new Vector3(3,3,-5);c.transform.LookAt(focus);c.clearFlags=CameraClearFlags.SolidColor;c.backgroundColor=new Color(.13f,.18f,.19f);RenderSettings.ambientLight=new Color(.7f,.72f,.73f);var l=new GameObject("Asset inspection sun").AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.2f;l.transform.rotation=Quaternion.Euler(45,-30,0);return c;}
 static Item Food(string source,string role,Color color){
  var importer=AssetImporter.GetAtPath(Src+source+".fbx") as ModelImporter;if(!importer)throw new Exception("Missing food importer");importer.isReadable=true;importer.importAnimation=false;importer.SaveAndReimport();
  var original=AssetDatabase.LoadAssetAtPath<GameObject>(Src+source+".fbx");if(!original)throw new Exception("Missing FBX "+source);
  var root=new GameObject(role);var high=UnityEngine.Object.Instantiate(original,root.transform);high.name="LOD0 authored "+source;var b=BoundsOf(high);float max=Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z));if(max<=0)throw new Exception("Invalid model scale");high.transform.localScale/=max;b=BoundsOf(high);high.transform.position-=b.center;
  foreach(var mr in high.GetComponentsInChildren<MeshRenderer>())mr.sharedMaterial=atlas;
  var bb=BoundsOf(high);var highRenderers=high.GetComponentsInChildren<Renderer>();int vertices=0,triangles=0;foreach(var mf in high.GetComponentsInChildren<MeshFilter>()){vertices+=mf.sharedMesh.vertexCount;triangles+=mf.sharedMesh.triangles.Length/3;}
  var low=new GameObject("LOD1 silhouette");low.transform.SetParent(root.transform,false);var mesh=Octa(role+" simple far silhouette",bb.size);string meshPath=Dst+role+"-LOD1.asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(meshPath))throw new Exception("Existing generated mesh; reconcile before rebuilding");AssetDatabase.CreateAsset(mesh,meshPath);low.AddComponent<MeshFilter>().sharedMesh=mesh;low.AddComponent<MeshRenderer>().sharedMaterial=Mat(role+"-far",color);
  var collider=root.AddComponent<BoxCollider>();collider.center=bb.center;collider.size=Vector3.Max(bb.size*.85f,Vector3.one*.005f);collider.enabled=false;
  var lod=root.AddComponent<LODGroup>();lod.SetLODs(new[]{new LOD(.04f,highRenderers),new LOD(.004f,low.GetComponentsInChildren<Renderer>())});lod.RecalculateBounds();
  string path=Dst+role+".prefab";PrefabUtility.SaveAsPrefabAsset(root,path);UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
  var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);var instance=UnityEngine.Object.Instantiate(prefab);var cc=instance.GetComponent<BoxCollider>();var pos=cc.center;var size=cc.size;var g=instance.GetComponent<LODGroup>();g.ForceLOD(0);bool same=cc.center==pos&&cc.size==size;g.ForceLOD(1);same&=cc.center==pos&&cc.size==size;
  var camera=CameraAt(Vector3.zero,.8f);g.ForceLOD(0);Capture(camera,"../evidence/assets/"+role+"-LOD0.png");g.ForceLOD(1);Capture(camera,"../evidence/assets/"+role+"-LOD1.png");
  UnityEngine.Object.DestroyImmediate(instance);UnityEngine.Object.DestroyImmediate(camera.gameObject);foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(light.gameObject);
  return new Item{name=role,path=path,vertices=vertices,triangles=triangles,lowTriangles=8,colliders=1,bounds=bb.size,pivot=Vector3.zero,lodPhysicsInvariant=same,reopened=prefab!=null};
 }
 public static void Prepare(){
  if(!Application.dataPath.Replace('\\','/').Contains("/flight-restaurant-r2/crab-flight/Assets"))throw new Exception("Wrong project");
  Directory.CreateDirectory(Dst);Directory.CreateDirectory("../evidence/assets");AssetDatabase.Refresh();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Src+"Textures/colormap.png");if(!texture)throw new Exception("Selected CC0 colormap missing");atlas=Mat("Kenney-atlas",Color.white,texture);
  var report=new Report();report.foods=new[]{Food("fish","FoodFish",new Color(.60f,.66f,.67f)),Food("sushi-salmon","FoodSushi",new Color(.83f,.49f,.32f)),Food("plate-broken","FoodPlateBroken",new Color(.86f,.84f,.75f))};
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  string donorPath=Src+"OGA_CrabMotion_CC0.fbx";var importer=AssetImporter.GetAtPath(donorPath) as ModelImporter;if(!importer)throw new Exception("Donor FBX importer missing");importer.isReadable=true;importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Generic;importer.optimizeGameObjects=false;importer.SaveAndReimport();
  var donor=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(donorPath));var clips=AssetDatabase.LoadAllAssetsAtPath(donorPath).OfType<AnimationClip>().Where(x=>!x.name.StartsWith("__preview__")).ToArray();report.donorClips=clips.Select(x=>x.name).ToArray();var skins=donor.GetComponentsInChildren<SkinnedMeshRenderer>();report.donorMeshCount=skins.Length;report.donorBones=skins.Sum(x=>x.bones.Length);
  var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Src+"OGA_CrabTexture.png");var material=Mat("Crab-donor-texture",Color.white,tex);foreach(var skin in skins){skin.sharedMaterial=material;skin.updateWhenOffscreen=true;}
  var walk=clips.FirstOrDefault(x=>x.name.ToLowerInvariant().Contains("walk"));if(walk&&skins.Length>0){var m=new Mesh();walk.SampleAnimation(donor,0);skins[0].BakeMesh(m);var before=m.vertices;walk.SampleAnimation(donor,walk.length*.35f);skins[0].BakeMesh(m);var after=m.vertices;for(int i=0;i<Math.Min(before.Length,after.Length);i++)report.donorWalkVertexDelta=Mathf.Max(report.donorWalkVertexDelta,Vector3.Distance(before[i],after[i]));UnityEngine.Object.DestroyImmediate(m);}report.donorMotionValidated=report.donorWalkVertexDelta>.001f;
  CrabModel.SurfaceShader=Shader.Find("CustomCrabBattle/ChitinR3");CrabModel.ArenaShader=Shader.Find("Standard");var crab=CrabModel.Make("Original r3 preserved",new FlightSettings(),Color.white);report.originalPartIds=crab.CollisionParts.Select(x=>x.Id).ToArray();if(report.originalPartIds.Distinct().Count()!=11)throw new Exception("Original part ID contract broken");
  var db=BoundsOf(donor);float ds=2.6f/Mathf.Max(db.size.x,db.size.z);donor.transform.localScale*=ds;db=BoundsOf(donor);donor.transform.position+=new Vector3(1.8f-db.center.x,-db.min.y,0);
  var cb=BoundsOf(crab.Root.gameObject);crab.Root.localScale*=2.6f/Mathf.Max(cb.size.x,cb.size.z);cb=BoundsOf(crab.Root.gameObject);crab.Root.position+=new Vector3(-1.8f-cb.center.x,-cb.min.y,0);
  var cam=CameraAt(new Vector3(0,.6f,0),2.5f);Capture(cam,"../evidence/assets/crab-original-left_cc0-right.png",960,640);if(walk)walk.SampleAnimation(donor,walk.length*.65f);Capture(cam,"../evidence/assets/crab-motion-sample.png",960,640);
  File.WriteAllText("../evidence/assets/asset-contract.json",JsonUtility.ToJson(report,true));crab.Dispose();AssetDatabase.SaveAssets();
  if(report.foods.Any(x=>!x.reopened||!x.lodPhysicsInvariant||x.lowTriangles>=x.triangles))throw new Exception("Prefab/LOD contract failed");
 }
}

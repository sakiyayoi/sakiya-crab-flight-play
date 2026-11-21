using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class AnatomyModelReview {
 [Serializable] public class Row {public string variant;public int meshes,triangles,vertices,parts;public Vector3 bounds;public string[] ids;public bool gripMoves;}
 [Serializable] public class Report {public Row[] models;public bool mountsMatch;public int fbxMeshes,fbxTriangles;public string scope="Same engine shader/light/camera; charge1 own anatomy1 versus anatomy2. Root-pose stills and actual hinge movement. Not physical iPhone or owner adoption.";}
 public static void Run(){
  Directory.CreateDirectory("../evidence/engine-model");var report=new Report();var rows=new System.Collections.Generic.List<Row>();
  var old=CrabR3Archive.Read(File.ReadAllBytes("Assets/Editor/BaselineData/crab-r3.bytes"));var now=CrabR3Archive.Read(File.ReadAllBytes("Assets/Resources/CrabR3/crab-r3.bytes"));report.mountsMatch=true;
  var settings=new CrabR3Parameters(45,45,45);foreach(var node in old.Nodes.Where(n=>n.PartRoot)){var n=now.Nodes.Single(x=>x.PartRoot&&x.PartId==node.PartId);for(int j=0;j<10;j++)report.mountsMatch&=Mathf.Abs(node.Pose[j]-n.Pose[j])<.000001f;}
  if(!report.mountsMatch)throw new Exception("Anatomical mounts changed");
  foreach(bool baseline in new[]{true,false}){
   EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);CrabModel.ReviewUseBaseline(baseline);CrabModel.SurfaceShader=Shader.Find("CustomCrabBattle/ChitinR3");CrabModel.ArenaShader=Shader.Find("Standard");var crab=CrabModel.Make("Same own actor",new FlightSettings(),Color.white);
   RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.68f,.78f,.82f);var sun=new GameObject("Same sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.2f;sun.transform.rotation=Quaternion.Euler(48,-35,0);sun.color=new Color(1,.93f,.78f);
   var camera=new GameObject("Same camera").AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=2.8f;camera.transform.position=new Vector3(4,3,6);camera.transform.LookAt(new Vector3(0,.7f,0));camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.13f,.18f,.19f);
   string label=baseline?"charge2":"surface-r1";var meshes=crab.Root.GetComponentsInChildren<MeshFilter>();var b=crab.Root.GetComponentInChildren<Renderer>().bounds;foreach(var rr in crab.Root.GetComponentsInChildren<Renderer>())b.Encapsulate(rr.bounds);
   var row=new Row{variant=label,meshes=meshes.Length,vertices=meshes.Sum(m=>m.sharedMesh.vertexCount),triangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3),parts=crab.CollisionParts.Count,ids=crab.CollisionParts.Select(p=>p.Id).ToArray(),bounds=b.size};
   foreach(int angle in new[]{0,90,180,270}){crab.Root.rotation=Quaternion.AngleAxis(angle,new Vector3(0,.28f,-.96f).normalized);Capture(camera,"../evidence/engine-model/"+label+"-roll"+angle+".png");}
   crab.Root.rotation=Quaternion.identity;var finger=crab.Root.GetComponentsInChildren<Transform>().First(x=>x.name=="DactylHinge");var before=finger.localRotation;crab.Animate(0,.75f,0,false);row.gripMoves=Quaternion.Angle(before,finger.localRotation)>.1f;Capture(camera,"../evidence/engine-model/"+label+"-grip.png");rows.Add(row);crab.Dispose();
  }
  CrabModel.ReviewUseBaseline(false);report.models=rows.ToArray();
  var importer=AssetImporter.GetAtPath("Assets/RestaurantSurface1/CustomCrab-surface-r1.fbx") as ModelImporter;if(!importer)throw new Exception("Native exchange FBX absent");importer.isReadable=true;importer.importAnimation=false;importer.SaveAndReimport();var fbx=AssetDatabase.LoadAssetAtPath<GameObject>(importer.assetPath);report.fbxMeshes=fbx.GetComponentsInChildren<MeshFilter>().Length;report.fbxTriangles=fbx.GetComponentsInChildren<MeshFilter>().Sum(x=>x.sharedMesh.triangles.Length/3);if(report.fbxTriangles!=rows[1].triangles)throw new Exception("FBX triangle readback mismatch");
  File.WriteAllText("../evidence/engine-model/result.json",JsonUtility.ToJson(report,true));if(rows.Any(x=>x.parts!=11||!x.gripMoves))throw new Exception("Model contract failed");
 }
 static void Capture(Camera c,string file){c.aspect=4f/3;var rt=RenderTexture.GetTemporary(960,720,24);var prior=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(960,720,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,960,720),0,0);t.Apply();File.WriteAllBytes(file,t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(t);}
}

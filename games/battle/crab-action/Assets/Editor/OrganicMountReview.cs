using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// Controlled native visual comparison, not normal-play or performance evidence.
public static class OrganicMountReview {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
 [Serializable]public class Frame {public string loadout,pose,camera,before,after;public int addedVertices,addedTriangles;public bool noVisualColliders,tipTransformsUnchanged;}
 [Serializable]public class Report {public string scope="Actual Unity native same scene, pose, camera and light; organic mount renderers toggled only. Frozen old weapon geometry/rules preserved.";public string status="PASS_STRUCTURAL_CONTRACT_AND_SAMEPOSE_FRAMES_WRITTEN";public Frame[] frames;public int partIDs,geometryMeshes;}
 static object Field(object o,string name)=>o.GetType().GetField(name,F).GetValue(o);
 static CrabModel Model(CrabActionGame g,string side)=>(CrabModel)Field(Field(g,side),"model");
 static Renderer[] Mounts(CrabActionGame g)=>new[]{Model(g,"player"),Model(g,"enemy")}.SelectMany(m=>m.Root.GetComponentsInChildren<MeshRenderer>(true)).Where(r=>r.name.StartsWith("OrganicMount_")).ToArray();
 static Vector3[] Tips(CrabActionGame g)=>new[]{Model(g,"player"),Model(g,"enemy")}.SelectMany(m=>m.Root.GetComponentsInChildren<Transform>(true)).Where(t=>t.name=="Visible weapon tip").Select(t=>t.position).ToArray();
 static void Save(Camera c,string path){var old=c.targetTexture;var rt=RenderTexture.GetTemporary(720,540,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var active=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(720,540,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,720,540),0,0);t.Apply();File.WriteAllBytes(path,t.EncodeToJPG(94));UnityEngine.Object.DestroyImmediate(t);c.targetTexture=old;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);}
 public static void Run(){
  Directory.CreateDirectory("../evidence/mount-comparison");var data=CrabR3Archive.Read(File.ReadAllBytes("Assets/Resources/CrabR3/crab-r3.bytes"));int partIDs=data.Nodes.Count(n=>n.PartRoot);if(partIDs!=11)throw new Exception("r3 11-part contract lost");var frames=new List<Frame>();
  foreach(var pair in new[]{new[]{"sword","sword"},new[]{"cannon","cannon"},new[]{"creature","creature"},new[]{"sword","cannon"}})foreach(string pose in new[]{"ready","walk","attack"}){
   var g=BuildAction.Prepare();g.aiEnabled=false;g.Initialize();g.Configure("{\"left\":\""+pair[0]+"\",\"right\":\""+pair[1]+"\"}");
   if(pose=="ready")g.Customize("");else{g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,7));if(pose=="walk")g.Move("0.7,0.4");else{g.AttackLeft("");g.AttackRight("");}for(int i=0;i<24;i++)g.Tick(1f/90f);}
   var camera=g.ReviewCamera;camera.aspect=720f/540;var mounts=Mounts(g);if(mounts.Length!=8)throw new Exception("Expected 2 surfaces on each of 4 swapped arms");int vertices=mounts.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.vertexCount);int triangles=mounts.Sum(r=>r.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3);if(vertices!=656)throw new Exception("Unexpected mount vertex budget");if(mounts.Any(r=>r.GetComponent<Collider>()))throw new Exception("Visual mount must have no collider");var tipPositions=Tips(g);if(tipPositions.Length!=4)throw new Exception("Expected four real weapon tips");foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))if(!renderer.sharedMaterial||!renderer.sharedMaterial.shader)throw new Exception("Missing render material: "+renderer.name);
   foreach(string view in new[]{"game","close"}){
    var oldPosition=camera.transform.position;var oldRotation=camera.transform.rotation;float oldSize=camera.orthographicSize;
    if(view=="close"){var focus=Model(g,"player").Root.position+new Vector3(0,.4f,.35f);camera.transform.position=focus+new Vector3(4,5,7);camera.transform.LookAt(focus);camera.orthographicSize=2.35f;}
    string key=pair[0]+"-"+pair[1]+"-"+pose+"-"+view;string before="mount-comparison/"+key+"-before.jpg",after="mount-comparison/"+key+"-after.jpg";
    foreach(var r in mounts)r.enabled=false;Save(camera,"../evidence/"+before);foreach(var r in mounts)r.enabled=true;Save(camera,"../evidence/"+after);
    var now=Tips(g);if(now.Length!=tipPositions.Length||now.Where((p,i)=>(p-tipPositions[i]).sqrMagnitude>1e-10f).Any())throw new Exception("Visual mount changed a weapon tip");
    frames.Add(new Frame{loadout=pair[0]+"/"+pair[1],pose=pose,camera=view,before=before,after=after,addedVertices=vertices,addedTriangles=triangles,noVisualColliders=true,tipTransformsUnchanged=true});camera.transform.position=oldPosition;camera.transform.rotation=oldRotation;camera.orthographicSize=oldSize;
   }
  }
  File.WriteAllText("../evidence/organic-mount-review.json",JsonUtility.ToJson(new Report{frames=frames.ToArray(),partIDs=partIDs,geometryMeshes=data.Meshes.Length},true));
 }
 public static void ReviewAndWeb(){
  ActionReview.Run();PolishActionReview.Run();RenderedContactReview.Run();CameraVisibilityReview.Run();Polish3CueReview.Run();Run();
  var game=BuildAction.Prepare();var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");PlayerSettings.bundleVersion="crab-action-r1-organic1";const string dest="../delivery/UnityWeb/organic1";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});File.WriteAllText("../evidence/organic-build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Organic mount build failed");foreach(string ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/organic1."+ext,dest+"/Build/UnityWeb."+ext);foreach(string file in Directory.GetFiles("../web-organic"))File.Copy(file,dest+"/"+Path.GetFileName(file),true);
 }
}

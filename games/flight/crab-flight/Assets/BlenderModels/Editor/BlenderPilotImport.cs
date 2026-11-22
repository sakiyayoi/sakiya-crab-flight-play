using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public sealed class BlenderPilotImport:AssetPostprocessor {
 public static void Inspect(){
  var lines=new System.Collections.Generic.List<string>();
  foreach(var family in new[]{"mall_r5_gift_small","vending_unit","blade_sign"}){
   var p=Resources.Load<GameObject>("BlenderModels/"+family);
   foreach(var t in p.GetComponentsInChildren<Transform>(true))lines.Add(family+" "+t.name+" pos="+t.localPosition+" rot="+t.localEulerAngles+" scale="+t.localScale);
   var f=p.GetComponentsInChildren<MeshFilter>(true).Single(x=>x.name=="Intact");
   var verts=f.sharedMesh.vertices;var colors=f.sharedMesh.colors;
   for(int k=0;k<verts.Length;k+=Math.Max(1,verts.Length/20))lines.Add("vertex "+verts[k]+" color "+colors[k]);
  }
  System.IO.File.WriteAllLines(System.Environment.GetEnvironmentVariable("BLENDER_PILOT_INSPECT"),lines);
 }
 void OnPreprocessModel(){
  if(!assetPath.StartsWith("Assets/BlenderModels/Resources/BlenderModels/")||!assetPath.EndsWith(".fbx",StringComparison.OrdinalIgnoreCase))return;
  var m=(ModelImporter)assetImporter;m.globalScale=1;m.useFileScale=true;m.bakeAxisConversion=true;
  m.importAnimation=false;m.importCameras=false;m.importLights=false;m.addCollider=false;
  m.importNormals=ModelImporterNormals.Import;m.materialImportMode=ModelImporterMaterialImportMode.None;
  m.isReadable=true;m.meshCompression=ModelImporterMeshCompression.Off;m.preserveHierarchy=true;
 }
 public static void Prepare(){
  AssetDatabase.Refresh();
  const string dir="Assets/BlenderModels/Resources/BlenderModels/";
  var shader=AssetDatabase.LoadAssetAtPath<Shader>(dir+"PilotFlat.shader");
  var mat=AssetDatabase.LoadAssetAtPath<Material>(dir+"PilotFlat.mat");
  if(!mat){mat=new Material(shader);AssetDatabase.CreateAsset(mat,dir+"PilotFlat.mat");}
  else mat.shader=shader;
  mat.color=Color.white;AssetDatabase.SaveAssets();
  foreach(var path in AssetDatabase.FindAssets("t:Model",new[]{dir}).Select(AssetDatabase.GUIDToAssetPath)){
   var p=AssetDatabase.LoadAssetAtPath<GameObject>(path);
   foreach(var f in p.GetComponentsInChildren<MeshFilter>(true)){
    if(f.transform.localPosition.sqrMagnitude>1e-8f||Quaternion.Angle(f.transform.localRotation,Quaternion.identity)>.01f||(f.transform.localScale-Vector3.one).sqrMagnitude>1e-8f)
     throw new Exception("FBX part transform must be identity: "+path+" "+f.name);
    if(f.sharedMesh.colors.Length!=f.sharedMesh.vertexCount)throw new Exception("FBX vertex colours missing: "+path);
   }
  }
 }
}

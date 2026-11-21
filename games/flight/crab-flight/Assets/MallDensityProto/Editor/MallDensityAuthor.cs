using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using C2=CrabCityDensityR2;
using Model=BoutiqueModelBuilderR18.Model;
public static class MallDensityAuthor {
 const string Root="Assets/MallDensityProto/Resources/MallDensityProto";
 static Color Hex(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
 static T Save<T>(T value,string path)where T:UnityEngine.Object{
  var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}
  AssetDatabase.CreateAsset(value,path);return value;
 }
 static Material Atlas(string name,Color[] palette){
  var tex=new Texture2D(64,64,TextureFormat.RGBA32,false){name=name+" atlas",filterMode=FilterMode.Point};
  var metal=new Texture2D(64,64,TextureFormat.RGBA32,false){name=name+" roughness",filterMode=FilterMode.Point};
  for(int y=0;y<64;y++)for(int x=0;x<64;x++){int i=x/16+y/16*4;tex.SetPixel(x,y,palette[i]);metal.SetPixel(x,y,new Color(0,0,0,.24f));}
  tex.Apply();metal.Apply();tex=Save(tex,Root+"/"+name+"-atlas.asset");metal=Save(metal,Root+"/"+name+"-metal.asset");
  var mat=new Material(Shader.Find("CrabCityDensity/R2Surface")){name=name};mat.SetTexture("_MainTex",tex);mat.SetTexture("_MetallicGlossMap",metal);mat.SetColor("_Color",Color.white);
  return Save(mat,Root+"/"+name+".mat");
 }
 public static void Generate(){
  Directory.CreateDirectory(Root+"/Meshes");AssetDatabase.Refresh();
  var colors=new[]{"#F58B31","#F3C55E","#D83D58","#70A64C","#6276C9","#DB73AE","#A18ADC","#EED65E","#203642","#FFF1CD","#FFFFFF","#846855","#8CB3AE","#9FAFA4","#C7C4B3","#637B76"}.Select(Hex).ToArray();
  Atlas("targets",colors);Atlas("fragments",colors.Select((c,i)=>i==8?Hex("#172731"):Color.Lerp(c,Hex("#FFF4D8"),.23f)).ToArray());
  var muted=colors.Select((c,i)=>Color.Lerp(Hex("#788D86"),new Color(c.grayscale,c.grayscale,c.grayscale),.30f)).ToArray();
  muted[8]=Hex("#526B67");muted[9]=Hex("#A4AEA1");muted[10]=Hex("#B7BDA9");var decorMat=Atlas("background",muted);
  var g=JsonUtility.FromJson<C2.CityDensityPack.Geometry>(Resources.Load<TextAsset>("CityDensityR2/CityGeometryR2Compact").text);
  var root=new GameObject("DensityProto static mall decor");
  int count=0;
  foreach(float x in new[]{37f,58,79,100,121}){
   AddAsset(root.transform,g.assets.First(a=>a.id=="shopfront_module"),"facade-"+count++,new Vector3(x,0,13),new Vector3(1.2f,1.12f,.65f),decorMat);
  }
  count=0;foreach(float x in new[]{31f,62,94,132})AddAsset(root.transform,g.assets.First(a=>a.id=="street_lamp"),"lamp-"+count++,new Vector3(x,0,8),Vector3.one*1.2f,decorMat);
  count=0;foreach(float x in new[]{42f,65,87,107,128}){
   var m=new Model("tree-"+count++);m.Box(-1,8,new Vector3(0,1.9f,0),new Vector3(.35f,3.8f,.35f),.06f);
   m.Box(-1,9,new Vector3(0,.4f,0),new Vector3(1.6f,.8f,1.6f),.12f);
   m.Lathe(-1,3,new Vector3(0,4.8f,0),new[]{new Vector2(0,-1.9f),new Vector2(1.5f,-1.3f),new Vector2(1.9f,0),new Vector2(1.5f,1.3f),new Vector2(0,1.9f)},10);
   AddModel(root.transform,m,new Vector3(x,0,9.2f),decorMat);
  }
  count=0;foreach(float x in new[]{34f,53,72,91,110,129}){
   var m=new Model("banner-"+count++);m.Box(-1,9,Vector3.zero,new Vector3(2.3f,3.9f,.12f),.05f);
   m.Box(-1,8,new Vector3(0,2.06f,0),new Vector3(2.7f,.15f,.22f),.04f);
   m.Box(-1,13,new Vector3(0,.55f,-.08f),new Vector3(1.5f,.20f,.05f),.02f);
   m.Box(-1,13,new Vector3(0,-.05f,-.08f),new Vector3(1.1f,.18f,.05f),.02f);
   AddModel(root.transform,m,new Vector3(x,10.3f,10.4f),decorMat);
  }
  MallDensityAuthor2.Append(root.transform,decorMat);
  MallDensityAuthor3.Append(root.transform);
  foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.isStatic=true;
  if(root.GetComponentsInChildren<Collider>(true).Length!=0||root.GetComponentsInChildren<Rigidbody>(true).Length!=0)throw new Exception("Decor must have no physics components");
  PrefabUtility.SaveAsPrefabAsset(root,Root+"/decor.prefab");UnityEngine.Object.DestroyImmediate(root);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
  Debug.Log("DENSITY_AUTHOR_OK staticDecor45 targetVariants7");
 }
 static void AddAsset(Transform parent,C2.CityDensityPack.Asset asset,string name,Vector3 position,Vector3 scale,Material material){
  var vertices=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
  foreach(var p in asset.pieces){int start=vertices.Count;int slot=p.material==4||p.material==10?8:p.material==3?9:13;
   for(int i=0;i<p.v.Length;i+=3){vertices.Add(new Vector3(p.v[i],p.v[i+1],p.v[i+2]));uv.Add(new Vector2((slot%4+.5f)/4,(slot/4+.5f)/4));}tris.AddRange(p.t.Select(i=>start+i));}
  var mesh=new Mesh{name=name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(vertices);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
  AddMesh(parent,mesh,name,position,scale,material);
 }
 internal static void AddModel(Transform parent,Model model,Vector3 pos,Material material){
  foreach(var pair in model.parts){var mesh=pair.Value.Mesh(model.name);var uv=mesh.uv;
   for(int sub=0;sub<mesh.subMeshCount;sub++){int mat=pair.Value.materialIndices[sub];foreach(int i in mesh.GetTriangles(sub).Distinct())uv[i]=new Vector2((mat%4+.5f)/4,(mat/4+.5f)/4);}
   var triangles=Enumerable.Range(0,mesh.subMeshCount).SelectMany(s=>mesh.GetTriangles(s)).ToArray();mesh.uv=uv;mesh.subMeshCount=1;mesh.SetTriangles(triangles,0);AddMesh(parent,mesh,model.name,pos,Vector3.one,material);}
 }
 static void AddMesh(Transform parent,Mesh mesh,string name,Vector3 position,Vector3 scale,Material material){
  mesh=Save(mesh,Root+"/Meshes/"+name+".asset");var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
  go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
 }
}

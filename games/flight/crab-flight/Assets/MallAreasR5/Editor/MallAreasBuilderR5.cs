using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using C2=CrabCityDensityR2;
using Model=BoutiqueModelBuilderR18.Model;

// Reuses the existing R18 rounded-box / lathe authoring, shaders and grain maps.
// New original meshes based on the adopted r35-3 concept, not recovered r35 assets.
public static class MallAreasBuilderR5 {
 const string Root="Assets/MallAreasR5/Resources/MallAreasR5";
 static Material[] mats;static Material sharedSurface;
 static List<C2.CityDensityPack.Asset> assets;
 static List<City2Data.Proposal> proposals;
 [Serializable]class Props{public City2Data.Proposal[] assets;}
 public static void Generate(){
  Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();
  Color[] colors={new Color(.98f,.90f,.71f),new Color(.40f,.77f,.64f),new Color(.92f,.44f,.54f),new Color(.57f,.49f,.79f),new Color(.98f,.73f,.16f),new Color(.055f,.10f,.15f),new Color(.60f,.74f,.78f),new Color(.67f,.86f,.91f,.10f)};
  mats=new Material[colors.Length];for(int i=0;i<mats.Length;i++){
   var m=new Material(Shader.Find(i==7?"MallModelR18/Glass":"MallModelR18/Surface")){name="R5 "+new[]{"warm enamel","mint cloth","coral cloth","lilac laminate","lemon resin","dark recess","satin hardware","clear window"}[i]};
   m.SetColor("_Color",colors[i]);if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",i==1||i==2?.16f:i==6?.6f:.36f);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",i==6?.65f:0);
   var grain=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/MallModelPackR18/Resources/MallModelsR18/Textures/fine-grain.png");if(i!=7&&m.HasProperty("_MainTex"))m.SetTexture("_MainTex",grain);mats[i]=Save(m,Root+"/Materials/"+i+".mat");
  }
  var atlas=new Texture2D(256,128,TextureFormat.RGBA32,false);var metal=new Texture2D(256,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<256;x++){int cell=x/64+4*(y/64);float q=Mathf.Sin(x*12.9898f+y*78.233f)*43758.5453f;q-=Mathf.Floor(q);atlas.SetPixel(x,y,colors[cell]*(.95f+.05f*q));metal.SetPixel(x,y,new Color(cell==6?.65f:0,0,0,cell==1||cell==2?.16f:.36f));}atlas.Apply();metal.Apply();atlas=Save(atlas,Root+"/Materials/pastel-atlas.asset");metal=Save(metal,Root+"/Materials/pastel-metal.asset");
  sharedSurface=new Material(Shader.Find("CrabCityDensity/R2Surface")){name="R5 shared existing city shader"};sharedSurface.SetTexture("_MainTex",atlas);sharedSurface.SetTexture("_MetallicGlossMap",metal);sharedSurface.SetColor("_Color",Color.white);sharedSurface=Save(sharedSurface,Root+"/Materials/shared.mat");
  assets=new List<C2.CityDensityPack.Asset>();proposals=new List<City2Data.Proposal>();
  foreach(var family in new[]{"gift","sweets","capsule"})foreach(bool large in new[]{true,false}){
   var g=new Model("mall_r5_"+family+(large?"_large":"_small"));
   if(family=="gift")Gift(g,large);else if(family=="sweets")Sweets(g,large);else Capsule(g,large);Bake(g,family,large);
  }
  File.WriteAllText(Root+"/Geometry.json",JsonUtility.ToJson(new C2.CityDensityPack.Geometry{revision="city-density-r2",assets=assets.ToArray()}));
  File.WriteAllText(Root+"/Proposals.json",JsonUtility.ToJson(new Props{assets=proposals.ToArray()}));AssetDatabase.Refresh();AssetDatabase.SaveAssets();
 }
 static void B(Model g,int part,int mat,float x,float y,float z,float w,float h,float d,float r=.06f){g.Box(part,mat,new Vector3(x,y,z),new Vector3(w,h,d),r);}
 static void Ball(Model g,int part,int mat,Vector3 c,Vector3 size){var p=new Vector2[9];for(int k=0;k<p.Length;k++){float a=k*Mathf.PI/8;p[k]=new Vector2(Mathf.Sin(a)*size.x*.5f,-Mathf.Cos(a)*size.y*.5f);} // circular horizontal section, intentionally chunky silhouette
  g.Lathe(part,mat,c,p,12);
 }
 static void Bear(Model g,int part,float x,float y,int mat,float s=1){
  Ball(g,part,mat,new Vector3(x,y+.52f*s,-.03f),new Vector3(.85f*s,1.05f*s,.85f*s));Ball(g,part,mat,new Vector3(x,y+1.17f*s,-.12f),new Vector3(.87f*s,.78f*s,.87f*s));
  foreach(float side in new[]{-1f,1f}){Ball(g,part,mat,new Vector3(x+side*.33f*s,y+1.52f*s,-.08f),Vector3.one*.34f*s);Ball(g,part,mat,new Vector3(x+side*.48f*s,y+.7f*s,-.06f),new Vector3(.35f*s,.63f*s,.35f*s));Ball(g,part,0,new Vector3(x+side*.24f*s,y+.17f*s,-.40f*s),Vector3.one*.4f*s);Ball(g,part,5,new Vector3(x+side*.18f*s,y+1.25f*s,-.515f*s),Vector3.one*.09f*s);}
  Ball(g,part,0,new Vector3(x,y+1.02f*s,-.46f*s),new Vector3(.39f*s,.25f*s,.39f*s));Ball(g,part,5,new Vector3(x,y+1.08f*s,-.64f*s),Vector3.one*.10f*s);
 }
 static void Gift(Model g,bool large){
  if(!large){B(g,-1,3,0,.16f,0,1.5f,.32f,1.6f);Bear(g,0,0,.34f,2,1.15f);return;}
  B(g,-1,3,0,.27f,0,5.4f,.5f,2.1f);B(g,0,3,-2.55f,2.6f,.05f,.22f,4.2f,1.9f);B(g,1,3,2.55f,2.6f,.05f,.22f,4.2f,1.9f);
  B(g,2,3,0,3.2f,.84f,4.85f,5.35f,.18f);B(g,2,4,0,5.45f,.68f,3.8f,.6f,.25f);B(g,2,0,0,5.45f,.51f,2.8f,.13f,.08f);
  foreach(int part in new[]{3,4}){float y=part==3?.65f:2.85f;B(g,part,0,0,y,-.1f,5.0f,.20f,1.7f);B(g,part,1,0,y-.13f,-.92f,5f,.16f,.14f);Bear(g,part,-1.3f,y+.12f,part==3?1:2);Bear(g,part,.2f,y+.12f,part==3?2:1,.88f);B(g,part,4,1.55f,y+.62f,-.12f,.83f,.95f,.63f,.16f);B(g,part,0,1.55f,y+1.17f,-.1f,.49f,.27f,.22f);}
 }
 static void Pop(Model g,int part,float x,float y,int mat,float scale){
  B(g,part,0,x,y+1f*scale,-.35f,.14f*scale,2f*scale,.14f*scale,.03f);
  // Broad candy disc facing the play camera, with a three-turn raised spiral.
  Ball(g,part,mat,new Vector3(x,y+2.15f*scale,-.35f),Vector3.one*1.32f*scale);
  Vector3 last=Vector3.zero;for(int i=0;i<38;i++){float t=i/37f,a=t*Mathf.PI*6,r=t*.58f*scale;var p=new Vector3(x+Mathf.Cos(a)*r,y+2.15f*scale+Mathf.Sin(a)*r,-.98f*scale);if(i>0)g.Rod(part,0,last,p,.10f*scale);last=p;}
 }
 static void Sweets(Model g,bool large){
  if(!large){B(g,-1,1,0,.15f,0,1.3f,.3f,1.45f);Pop(g,0,0,.28f,2,1f);return;}
  B(g,-1,1,0,.4f,0,5.1f,.8f,2.0f);for(int k=0;k<7;k++)B(g,-1,k%2==0?2:4,-2.1f+k*.7f,.4f,-1.02f,.61f,.65f,.07f,.02f);
  foreach(int part in new[]{0,1}){float x=part==0?-1.3f:1.3f;B(g,part,0,x,1.0f,0,2.45f,.27f,1.95f);B(g,part,7,x,1.85f,-.88f,2.25f,1.5f,.06f,.01f);foreach(float a in new[]{-1.12f,1.12f})B(g,part,1,x+a,1.88f,0,.11f,1.65f,1.82f);B(g,part,1,x,2.7f,0,2.40f,.15f,1.85f);for(int k=0;k<6;k++)Ball(g,part,k%2==0?2:4,new Vector3(x-.8f+(k%3)*.7f,1.35f+(k/3)*.35f,-.1f),Vector3.one*.52f);B(g,part,6,x,1.76f,-1f,.65f,.08f,.24f);}
  Pop(g,2,-1.65f,2.72f,2,.85f);Pop(g,3,1.65f,2.72f,4,.85f);B(g,4,1,0,3.9f,.85f,2.0f,1.3f,.23f,.16f);B(g,4,0,0,3.9f,.70f,1.35f,.19f,.09f);
 }
 static void Capsule(Model g,bool large){
  if(!large){B(g,-1,3,0,.3f,0,1.45f,.6f,1.5f);Ball(g,0,2,new Vector3(0,1.5f,0),Vector3.one*1.6f);B(g,0,0,0,1.5f,0,1.58f,.09f,1.58f,.02f);return;}
  B(g,-1,3,0,.45f,0,5.2f,.9f,2.05f);
  foreach(int part in new[]{0,1}){float x=part==0?-1.30f:1.30f;int mat=part==0?2:1;
   B(g,part,mat,x,3.0f,0,2.48f,3.5f,1.7f,.20f);B(g,part,5,x,3.45f,-.87f,2.03f,2.12f,.06f,.13f);B(g,part,7,x,3.45f,-1.08f,1.91f,2f,.04f,.1f);
   foreach(float dx in new[]{-.54f,.54f})foreach(float dy in new[]{-.48f,.43f}){Ball(g,part,dy<0?4:3,new Vector3(x+dx,3.45f+dy,-.95f),Vector3.one*.65f);B(g,part,0,x+dx,3.45f+dy,-1.28f,.59f,.045f,.09f,.01f);}
   B(g,part,0,x,2.15f,-.9f,1.6f,.52f,.10f,.1f);
  }
  B(g,2,0,0,5.07f,0,5.18f,.48f,1.95f,.15f);B(g,2,4,0,5.10f,-1.0f,3.5f,.21f,.08f);
  foreach(int part in new[]{3,4}){float x=part==3?-1.3f:1.3f;Ball(g,part,6,new Vector3(x,1.75f,-.9f),Vector3.one*.78f);B(g,part,5,x,1.75f,-1.31f,.52f,.13f,.12f);B(g,part,5,x,1.05f,-.91f,1.1f,.42f,.18f,.12f);}
 }
 static float[] V(Vector3 v){return new[]{v.x,v.y,v.z};}
 static T Save<T>(T item,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(item,old);UnityEngine.Object.DestroyImmediate(item);return old;}AssetDatabase.CreateAsset(item,path);return item;}
 static void Collapse(Mesh mesh,int[] materialIndices){
  var colors=mesh.colors;var uv=mesh.uv;var solid=new List<int>();var glass=new List<int>();
  for(int sub=0;sub<mesh.subMeshCount;sub++){int mat=materialIndices[sub];var tris=mesh.GetTriangles(sub);foreach(int v in tris.Distinct()){colors[v]=Color.white;uv[v]=new Vector2((mat%4+.03f+Mathf.Repeat(uv[v].x,1)*.94f)/4,(mat/4+.03f+Mathf.Repeat(uv[v].y,1)*.94f)/2);}if(mat==7)glass.AddRange(tris);else solid.AddRange(tris);}
  mesh.colors=colors;mesh.uv=uv;mesh.subMeshCount=glass.Count>0?2:1;mesh.SetTriangles(solid,0);if(glass.Count>0)mesh.SetTriangles(glass,1);
 }
 static void CombineIntact(GameObject root,string name){
  var filters=root.GetComponentsInChildren<MeshFilter>();var combined=new List<CombineInstance>();
  for(int sub=0;sub<2;sub++){
   var chunks=filters.Where(f=>f.sharedMesh.subMeshCount>sub).Select(f=>new CombineInstance{mesh=f.sharedMesh,subMeshIndex=sub,transform=Matrix4x4.identity}).ToArray();if(chunks.Length==0)continue;
   var mesh=new Mesh{name=name+" intact "+sub,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.CombineMeshes(chunks,true,false);mesh=Save(mesh,Root+"/Meshes/"+name+"-intact-"+sub+".asset");
   var o=new GameObject("IntactCombined_"+sub);o.transform.SetParent(root.transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=sub==0?sharedSurface:mats[7];
  }
 }
 static void Bake(Model g,string family,bool large){
  var root=new GameObject(g.name);var pieces=new List<C2.CityDensityPack.Piece>();var shards=new List<City2Data.Shard>();var bounds=new Bounds();bool first=true;
  foreach(var pair in g.parts){var mesh=pair.Value.Mesh(g.name+" "+pair.Key);Collapse(mesh,pair.Value.materialIndices);var part=new GameObject("Part_"+pair.Key);part.transform.SetParent(root.transform,false);mesh=Save(mesh,Root+"/Meshes/"+g.name+"-"+pair.Key+".asset");part.AddComponent<MeshFilter>().sharedMesh=mesh;part.AddComponent<MeshRenderer>().sharedMaterials=mesh.subMeshCount==1?new[]{sharedSurface}:new[]{sharedSurface,mats[7]};
   var b=mesh.bounds;if(first){bounds=b;first=false;}else bounds.Encapsulate(b);
   var verts=mesh.vertices.SelectMany(v=>V(v)).ToArray();var tris=Enumerable.Range(0,mesh.subMeshCount).SelectMany(i=>mesh.GetTriangles(i)).ToArray();var uv=mesh.vertices.SelectMany(v=>new[]{.04f,.04f}).ToArray();
   pieces.Add(new C2.CityDensityPack.Piece{name=g.name+" part "+pair.Key,material=0,shard=pair.Key,v=verts,t=tris,uv=uv});
   if(pair.Key>=0)shards.Add(new City2Data.Shard{shard=pair.Key,center=V(b.center),size=V(b.size),massKgSuggestion=large?2.5f:1.2f});
  }
  CombineIntact(root,g.name);PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+g.name+".prefab");UnityEngine.Object.DestroyImmediate(root);
  assets.Add(new C2.CityDensityPack.Asset{id=g.name,breakable=true,massKgSuggestion=large?56:12,collisionEnvelopeSuggestion=new C2.CityDensityPack.Envelope{center=V(bounds.center),size=V(bounds.size)},pieces=pieces.ToArray()});
  proposals.Add(new City2Data.Proposal{asset=g.name,material=family=="gift"?"wood":family=="sweets"?"food":"metal",massKgSuggestion=large?56:12,normalizedImpulseThresholdSuggestion=large?.9f:.4f,maxPostBreakSpeedFractionSuggestion=large?.985f:.994f,shards=shards.ToArray()});
 }
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Model=BoutiqueModelBuilderR18.Model;

// Reuses the already shipped R18 bevel/lathe brush and R2 surface shader. No new shader or physics assets.
public static class GiftStoreBuilderR7 {
 const string Root="Assets/GiftStoreR7/Resources/GiftStoreR7";
 static Material surface;
 static readonly Dictionary<int,List<Mesh>> handles=new Dictionary<int,List<Mesh>>();
 static readonly Color[] Colors={new Color(.64f,.46f,.28f),new Color(.90f,.85f,.73f),new Color(.35f,.60f,.51f),new Color(.78f,.38f,.43f),new Color(.07f,.12f,.14f),new Color(.47f,.54f,.54f),new Color(.91f,.89f,.79f),new Color(.62f,.32f,.23f),new Color(.91f,.66f,.22f),new Color(.22f,.45f,.49f),new Color(.69f,.55f,.40f),new Color(.27f,.17f,.10f),new Color(.66f,.67f,.61f),new Color(.40f,.43f,.39f),new Color(.82f,.80f,.72f),new Color(.98f,.94f,.81f)};
 public static void Generate(){
  Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");AssetDatabase.Refresh();MakeSurface();
  for(int variant=0;variant<3;variant++)foreach(bool large in new[]{true,false}){var g=new Model("gift_"+(large?"large_":"small_")+variant);if(large)Large(g,variant);else Small(g,variant);Bake(g,true);}
  var env=new Model("gift_surroundings");Environment(env);Bake(env,false);AssetDatabase.SaveAssets();AssetDatabase.Refresh();
 }
 static T Save<T>(T value,string path)where T:UnityEngine.Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}AssetDatabase.CreateAsset(value,path);return value;}
 static void MakeSurface(){
  const int n=512,cell=128;var atlas=new Texture2D(n,n,TextureFormat.RGB24,true);var metal=new Texture2D(n,n,TextureFormat.RGBA32,true);
  for(int y=0;y<n;y++)for(int x=0;x<n;x++){
   int m=x/cell+4*(y/cell);float u=x%cell,v=y%cell,q=Mathf.Sin(x*12.9898f+y*78.233f)*43758.5453f;q-=Mathf.Floor(q);float tone=1;
   if(m==0||m==11)tone=.90f+.07f*Mathf.Sin(u*.33f+Mathf.Sin(v*.04f)*1.3f)+q*.04f;
   else if(m==2||m==3||m==10)tone=.94f+.035f*Mathf.Sin(u*2.2f)*Mathf.Sin(v*2.2f)+q*.035f;
   else if(m==12)tone=.95f+.025f*Mathf.Sin(u*.12f+v*.07f)+q*.04f;
   else tone=.975f+q*.025f;
   atlas.SetPixel(x,y,Colors[m]*tone);float smooth=m==5?.65f:m==6?.72f:m==9?.45f:m==12?.30f:m==2||m==3||m==10?.06f:.22f;
   metal.SetPixel(x,y,new Color(m==5?.72f:0,0,0,smooth));
  }
  atlas.Apply();metal.Apply();WriteMap(atlas,"retail-albedo",true);WriteMap(metal,"retail-metal",false);
  surface=new Material(Shader.Find("CrabCityDensity/R2Surface")){name="R7 shared oak canvas ceramic stone"};surface.SetColor("_Color",Color.white);surface.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Materials/retail-albedo.png"));surface.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Materials/retail-metal.png"));surface=Save(surface,Root+"/Materials/shared.mat");
 }
 static void WriteMap(Texture2D t,string name,bool srgb){string path=Root+"/Materials/"+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var i=(TextureImporter)AssetImporter.GetAtPath(path);i.wrapMode=TextureWrapMode.Clamp;i.filterMode=FilterMode.Bilinear;i.mipmapEnabled=true;i.sRGBTexture=srgb;i.textureCompression=TextureImporterCompression.Uncompressed;i.SaveAndReimport();}
 static void B(Model g,int p,int m,float x,float y,float z,float w,float h,float d,float bevel=.03f)=>g.Box(p,m,new Vector3(x,y,z),new Vector3(w,h,d),bevel);
 static void Oval(Model g,int p,int m,Vector3 c,float radius,float height){var a=new Vector2[11];for(int i=0;i<a.Length;i++){float t=i*Mathf.PI/10;a[i]=new Vector2(Mathf.Sin(t)*radius,-Mathf.Cos(t)*height*.5f);}g.Lathe(p,m,c,a,16);}
 static void Handle(Model g,int p,int m,float x,float y,float z,float w,float h,float thickness){var points=new Vector3[13];for(int i=0;i<points.Length;i++){float a=i*Mathf.PI/12;points[i]=new Vector3(x+Mathf.Cos(a)*w*.5f,y+Mathf.Sin(a)*h,z);}Tube(p,m,points,thickness*.5f);}
 // One smooth six-sided tube rather than twelve separately bevelled boxes per handle.
 static void Tube(int part,int material,Vector3[] points,float radius){
  const int sides=6;var vertices=new Vector3[points.Length*sides];var normals=new Vector3[vertices.Length];var uv=new Vector2[vertices.Length];var triangles=new List<int>();
  for(int i=0;i<points.Length;i++){var tangent=(points[Mathf.Min(i+1,points.Length-1)]-points[Mathf.Max(0,i-1)]).normalized;var normal=Vector3.Cross(Vector3.forward,tangent).normalized;for(int j=0;j<sides;j++){float a=j*Mathf.PI*2/sides;int n=i*sides+j;normals[n]=normal*Mathf.Cos(a)+Vector3.forward*Mathf.Sin(a);vertices[n]=points[i]+normals[n]*radius;uv[n]=new Vector2((material%4+.15f+(float)j/sides*.6f)/4,(material/4+.15f+(float)i/points.Length*.6f)/4);}}
  Action<int,int,int> tri=(a,b,c)=>{var direction=Vector3.Cross(vertices[b]-vertices[a],vertices[c]-vertices[a]);if(Vector3.Dot(direction,normals[a]+normals[b]+normals[c])>0)triangles.AddRange(new[]{a,b,c});else triangles.AddRange(new[]{a,c,b});};
  for(int i=0;i<points.Length-1;i++)for(int j=0;j<sides;j++){int a=i*sides+j,b=(i+1)*sides+j,c=i*sides+(j+1)%sides,d=(i+1)*sides+(j+1)%sides;tri(a,b,c);tri(c,b,d);}
  var mesh=new Mesh{name="R7 continuous product handle"};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();if(!handles.TryGetValue(part,out var list)){list=new List<Mesh>();handles.Add(part,list);}list.Add(mesh);
 }
 static void Bag(Model g,int p,float x,float y,float z,int m,float s=1){B(g,p,m,x,y+.38f*s,z,.72f*s,.76f*s,.29f*s,.10f*s);B(g,p,1,x,y+.69f*s,z-.158f*s,.64f*s,.025f*s,.018f,.006f);Handle(g,p,10,x,y+.75f*s,z-.10f*s,.38f*s,.27f*s,.038f*s);B(g,p,15,x+.13f*s,y+.35f*s,z-.16f*s,.16f*s,.19f*s,.014f,.006f);}
 static void Plush(Model g,int p,float x,float y,float z,int m,float s,bool bunny){
  Oval(g,p,m,new Vector3(x,y+.40f*s,z),.35f*s,.74f*s);Oval(g,p,m,new Vector3(x,y+.92f*s,z-.025f),.35f*s,.61f*s);
  foreach(float side in new[]{-1f,1f}){Oval(g,p,m,new Vector3(x+side*.25f*s,y+(bunny?1.35f:1.19f)*s,z),.12f*s,(bunny?.61f:.25f)*s);Oval(g,p,m,new Vector3(x+side*.33f*s,y+.45f*s,z-.02f),.13f*s,.38f*s);Oval(g,p,10,new Vector3(x+side*.18f*s,y+.12f*s,z-.20f*s),.15f*s,.21f*s);Oval(g,p,4,new Vector3(x+side*.13f*s,y+.96f*s,z-.32f*s),.036f*s,.064f*s);}
  Oval(g,p,1,new Vector3(x,y+.81f*s,z-.26f*s),.16f*s,.15f*s);Oval(g,p,4,new Vector3(x,y+.86f*s,z-.40f*s),.048f*s,.06f*s);
 }
 static void Vase(Model g,int p,float x,float y,float z,int m,float s,int shape){
  var profile=shape==0?new[]{new Vector2(0,0),new Vector2(.22f,.025f),new Vector2(.28f,.3f),new Vector2(.19f,.62f),new Vector2(.12f,.76f),new Vector2(.13f,.86f),new Vector2(.09f,.86f),new Vector2(.08f,.76f),new Vector2(.12f,.65f),new Vector2(0,.08f)}:new[]{new Vector2(0,0),new Vector2(.19f,.01f),new Vector2(.29f,.20f),new Vector2(.29f,.42f),new Vector2(.21f,.56f),new Vector2(.18f,.58f),new Vector2(.16f,.54f),new Vector2(.24f,.39f),new Vector2(.24f,.20f),new Vector2(0,.06f)};
  g.Lathe(p,m,new Vector3(x,y,z),profile.Select(v=>v*s).ToArray(),20);
 }
 static void Mug(Model g,int p,float x,float y,float z,int m,float s){
  g.Lathe(p,m,new Vector3(x,y,z),new[]{new Vector2(0,0),new Vector2(.17f*s,0),new Vector2(.20f*s,.05f*s),new Vector2(.21f*s,.36f*s),new Vector2(.17f*s,.38f*s),new Vector2(.16f*s,.09f*s),new Vector2(0,.09f*s)},16);
  var points=new Vector3[13];for(int i=0;i<points.Length;i++){float a=-Mathf.PI*.5f+i*Mathf.PI/12;points[i]=new Vector3(x+.18f*s+Mathf.Cos(a)*.17f*s,y+.20f*s+Mathf.Sin(a)*.14f*s,z);}Tube(p,m,points,.0275f*s);
 }
 static void Books(Model g,int p,float x,float y,float z,int m,int count){for(int i=0;i<count;i++){B(g,p,i%2==0?m:1,x+.025f*(i%2),y+.09f+i*.17f,z,.72f,.14f,.48f,.018f);B(g,p,15,x+.01f,y+.09f+i*.17f,z-.246f,.60f,.09f,.01f,.004f);}}
 static void Label(Model g,int p,float x,float y,float z){B(g,p,15,x,y,z,.40f,.13f,.025f,.009f);B(g,p,4,x-.07f,y+.015f,z-.015f,.15f,.016f,.009f,.003f);}
 static void Frame(Model g,int variant){
  B(g,-1,11,0,.24f,0,5.30f,.46f,2.00f,.06f);B(g,-1,0,0,.43f,-.02f,5.30f,.12f,2.0f,.025f);
  B(g,0,variant==0?4:0,-2.52f,2.55f,.05f,.20f,4.20f,1.85f,.04f);B(g,1,variant==0?4:0,2.52f,2.55f,.05f,.20f,4.20f,1.85f,.04f);
  B(g,2,14,0,3.13f,.84f,4.84f,5.20f,.15f,.015f);for(int i=0;i<9;i++)B(g,2,variant==1?1:0,-2.2f+i*.55f,3.12f,.725f,.10f,5.05f,.075f,.015f);
  B(g,2,0,0,5.53f,.1f,5.25f,.42f,1.8f,.045f);B(g,2,4,0,5.50f,-.82f,3.35f,.32f,.035f,.02f);
  foreach(int p in new[]{3,4}){float y=p==3?.66f:2.75f;B(g,p,0,0,y,-.08f,4.95f,.16f,1.78f,.025f);B(g,p,4,0,y-.085f,-.88f,4.95f,.055f,.06f,.012f);for(int i=-2;i<=2;i++)Label(g,p,i*.88f,y-.03f,-.935f);}
 }
 static void Large(Model g,int variant){
  Frame(g,variant);
  if(variant==0){
   Plush(g,3,-1.55f,.76f,-.05f,10,1.15f,false);Plush(g,3,-.3f,.76f,-.08f,2,.90f,true);Bag(g,3,1.04f,.76f,-.18f,3,1.1f);Bag(g,3,1.95f,.76f,.1f,1,.8f);
   Plush(g,4,-1.55f,2.86f,-.10f,3,.93f,true);Plush(g,4,-.4f,2.86f,-.06f,10,.95f,false);Bag(g,4,.85f,2.86f,-.12f,2,.9f);Bag(g,4,1.75f,2.86f,-.02f,9,.8f);
   B(g,2,1,.9f,4.65f,.44f,2.10f,.13f,.64f,.025f);Bag(g,2,.55f,4.72f,.36f,10,.65f);Bag(g,2,1.35f,4.72f,.36f,2,.60f);
  }else if(variant==1){
   for(int i=0;i<5;i++){Mug(g,3,-1.9f+i*.91f,.76f,-.38f,i%2==0?6:9,1.1f);Mug(g,3,-1.8f+i*.91f,.76f,.34f,7,.9f);}
   Vase(g,4,-1.65f,2.85f,-.08f,7,1.45f,0);Vase(g,4,-.40f,2.85f,-.06f,6,1.9f,1);Vase(g,4,1.1f,2.85f,-.05f,9,1.3f,0);Mug(g,4,2.0f,2.85f,-.15f,6,1.1f);
   B(g,2,0,1.0f,4.65f,.4f,1.9f,.13f,.65f);Vase(g,2,.6f,4.72f,.35f,6,.85f,1);Vase(g,2,1.45f,4.72f,.35f,7,.75f,0);
  }else{
   Books(g,3,-1.55f,.76f,-.15f,9,4);Books(g,3,-.4f,.76f,-.10f,8,2);Bag(g,3,.8f,.76f,-.06f,2,1.15f);Bag(g,3,1.8f,.76f,.12f,1,.80f);
   B(g,4,1,-1.8f,3.33f,.18f,.90f,.90f,.06f);B(g,4,8,-1.8f,3.33f,.13f,.76f,.75f,.025f);Books(g,4,-.55f,2.86f,-.10f,3,3);Books(g,4,.55f,2.86f,-.08f,9,5);Vase(g,4,1.70f,2.86f,0,7,1.0f,1);
   for(int i=0;i<6;i++)g.Rod(4,i%2==0?8:4,new Vector3(1.55f+i*.055f,3.15f,0),new Vector3(1.5f+i*.08f,3.88f+i*.03f,.02f),.04f);
   B(g,2,0,-1,4.65f,.4f,2.20f,.13f,.65f);Books(g,2,-1.0f,4.72f,.35f,2,3);
  }
 }
 static void Small(Model g,int variant){
  B(g,-1,11,0,.15f,0,1.4f,.29f,1.45f,.045f);B(g,0,0,0,.39f,0,1.32f,.18f,1.36f,.035f);
  if(variant==0){B(g,0,10,0,.73f,0,1.05f,.60f,.80f,.11f);Plush(g,0,-.2f,.82f,-.12f,3,.78f,false);Bag(g,0,.38f,.50f,-.19f,2,.6f);}
  else if(variant==1){Vase(g,0,-.3f,.49f,.05f,6,1.15f,0);Mug(g,0,.35f,.49f,-.25f,7,1.1f);Mug(g,0,.40f,.91f,-.25f,9,.85f);}
  else{Books(g,0,-.23f,.49f,-.05f,8,4);Bag(g,0,.35f,.49f,.10f,1,.95f);}
 }
 static void Environment(Model g){
  // Renderer-only stone tiles over the existing drawn floor, with no new physical floor or blockers.
  B(g,-1,0,0,.0465f,4.0f,33.8f,.006f,.15f,.001f);
  B(g,-1,14,0,3.40f,7.1f,33.7f,6.8f,.16f,.03f);
  B(g,-1,11,0,.42f,6.97f,33.7f,.84f,.10f,.015f);
  foreach(float x in new[]{-15.5f,-5.5f,6.5f,15.5f}){B(g,-1,0,x,3.6f,6.92f,.30f,7.2f,.30f,.03f);B(g,-1,5,x,3.6f,6.73f,.05f,7.1f,.035f,.01f);}
  B(g,-1,1,0,7.0f,6.60f,33.8f,.50f,1.3f,.06f);B(g,-1,11,0,6.73f,6.1f,33.6f,.035f,.13f,.01f);
  // Sparse recessed display bays, at a different depth from the actual breakable foreground.
  foreach(float x in new[]{-10.0f,.5f,11.0f}){B(g,-1,11,x,2.7f,6.94f,6.7f,4.65f,.08f,.04f);B(g,-1,1,x,2.8f,6.84f,6.1f,4.25f,.05f,.02f);foreach(float y in new[]{1.15f,2.55f,4.05f}){B(g,-1,0,x,y,6.4f,6.0f,.14f,.80f,.02f);for(int i=-1;i<=1;i++)Vase(g,-1,x+i*1.6f,y+.08f,6.35f,i==0?7:6,.78f,i==0?0:1);}}
 }
 static void Bake(Model g,bool breakable){
  var root=new GameObject(g.name);var meshes=new List<Mesh>();
  foreach(var pair in g.parts){var mesh=pair.Value.Mesh(g.name+" part "+pair.Key);var uv=mesh.uv;var triangles=new List<int>();for(int sub=0;sub<mesh.subMeshCount;sub++){int m=pair.Value.materialIndices[sub];var tris=mesh.GetTriangles(sub);foreach(int i in tris.Distinct())uv[i]=new Vector2((m%4+.04f+Mathf.Repeat(.31f+uv[i].x*.11f,.92f))/4,(m/4+.04f+Mathf.Repeat(.31f+uv[i].y*.11f,.92f))/4);triangles.AddRange(tris);}mesh.uv=uv;mesh.subMeshCount=1;mesh.SetTriangles(triangles,0);if(handles.TryGetValue(pair.Key,out var extras)){var joined=new Mesh{name=mesh.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};joined.CombineMeshes(new[]{mesh}.Concat(extras).Select(m=>new CombineInstance{mesh=m,transform=Matrix4x4.identity}).ToArray(),true,false);UnityEngine.Object.DestroyImmediate(mesh);mesh=joined;}mesh=Save(mesh,Root+"/Meshes/"+g.name+"-"+pair.Key+".asset");meshes.Add(mesh);var part=new GameObject("Part_"+pair.Key);part.transform.SetParent(root.transform,false);part.AddComponent<MeshFilter>().sharedMesh=mesh;part.AddComponent<MeshRenderer>().sharedMaterial=surface;}
  if(breakable){var combined=new Mesh{name=g.name+" intact",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};combined.CombineMeshes(meshes.Select(m=>new CombineInstance{mesh=m,subMeshIndex=0,transform=Matrix4x4.identity}).ToArray(),true,false);combined=Save(combined,Root+"/Meshes/"+g.name+"-intact.asset");var intact=new GameObject("IntactCombined_0");intact.transform.SetParent(root.transform,false);intact.AddComponent<MeshFilter>().sharedMesh=combined;intact.AddComponent<MeshRenderer>().sharedMaterial=surface;}
  if(!breakable)AddFloor(root);
  PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+g.name+".prefab");UnityEngine.Object.DestroyImmediate(root);foreach(var mesh in handles.Values.SelectMany(x=>x))UnityEngine.Object.DestroyImmediate(mesh);handles.Clear();
 }
 static void AddFloor(GameObject root){
  var map=new Texture2D(256,256,TextureFormat.RGB24,true);for(int y=0;y<256;y++)for(int x=0;x<256;x++){float q=Mathf.Sin(x*12.9898f+y*78.233f)*43758.5453f;q-=Mathf.Floor(q);float value=.96f+q*.035f+.012f*Mathf.Sin(x*.07f+y*.04f);map.SetPixel(x,y,(x<2||y<2?new Color(.48f,.50f,.45f):new Color(.72f,.73f,.67f))*value);}map.Apply();WriteMap(map,"stone-floor",true);var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Materials/stone-floor.png");var importer=(TextureImporter)AssetImporter.GetAtPath(Root+"/Materials/stone-floor.png");importer.wrapMode=TextureWrapMode.Repeat;importer.SaveAndReimport();
  var properties=new Texture2D(1,1,TextureFormat.RGBA32,false);properties.SetPixel(0,0,new Color(0,0,0,.30f));properties.Apply();properties=Save(properties,Root+"/Materials/stone-properties.asset");
  var material=new Material(Shader.Find("CrabCityDensity/R2Surface")){name="R7 honed stone floor"};material.SetTexture("_MainTex",texture);material.SetTexture("_MetallicGlossMap",properties);material=Save(material,Root+"/Materials/stone-floor.mat");
  var mesh=new Mesh{name="R7 continuous gift floor"};mesh.vertices=new[]{new Vector3(-16.9f,.046f,-230),new Vector3(16.9f,.046f,-230),new Vector3(16.9f,.046f,24),new Vector3(-16.9f,.046f,24)};mesh.uv=new[]{new Vector2(-8.45f,-115),new Vector2(8.45f,-115),new Vector2(8.45f,12),new Vector2(-8.45f,12)};mesh.triangles=new[]{0,2,1,0,3,2};mesh.RecalculateNormals();mesh.RecalculateBounds();mesh=Save(mesh,Root+"/Meshes/stone-floor.asset");var node=new GameObject("Continuous rendered stone floor, no collider");node.transform.SetParent(root.transform,false);node.AddComponent<MeshFilter>().sharedMesh=mesh;node.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
}

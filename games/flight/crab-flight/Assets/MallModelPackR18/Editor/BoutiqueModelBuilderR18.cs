using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Object=UnityEngine.Object;

// Self-authored geometry and material maps; no external model, package or editor dependency.
public static class BoutiqueModelBuilderR18 {
 const string Root="Assets/MallModelPackR18/Resources/MallModelsR18";
 static Material[] mats;static string export;
 public static void BuildAll(){
  Directory.CreateDirectory(Root+"/Meshes");Directory.CreateDirectory(Root+"/Materials");Directory.CreateDirectory(Root+"/Textures");AssetDatabase.Refresh();
  export=Path.GetFullPath(Path.Combine(Application.dataPath,"../ModelExports/MallObjectR18"));Directory.CreateDirectory(export);Materials();
  var g=new Model("FragranceCase");
  foreach(int part in new[]{0,1}){float x=part==0?-1.55f:1.55f;g.Box(part,1,new Vector3(x,.24f,0),new Vector3(.34f,.46f,1.42f),.045f);g.Box(part,2,new Vector3(x,.43f,0),new Vector3(.36f,.10f,1.43f),.025f);}
  g.Box(2,4,new Vector3(-.91f,1.02f,0),new Vector3(1.76f,1.19f,1.69f),.055f);
  g.Box(2,1,new Vector3(0,.51f,0),new Vector3(3.54f,.18f,1.70f),.035f);
  g.Box(2,11,new Vector3(0,1.75f,0),new Vector3(3.60f,.17f,1.75f),.045f);
  for(int i=0;i<13;i++)g.Box(2,4,new Vector3(-1.70f+i*.128f,1.05f,-.865f),new Vector3(.072f,1.10f,.035f),.014f);
  g.Box(3,4,new Vector3(.83f,1.01f,-.02f),new Vector3(1.55f,1.17f,1.34f),.045f);
  for(int i=0;i<2;i++){g.Box(3,0,new Vector3(.83f,.76f+i*.51f,-.746f),new Vector3(1.43f,.46f,.045f),.018f);g.Box(3,2,new Vector3(.83f,.89f+i*.51f,-.762f),new Vector3(.40f,.026f,.024f),.010f);}
  foreach(float x in new[]{-1.72f,1.72f})foreach(float z in new[]{-1.02f,1.00f})g.Box(4,2,new Vector3(x,2.91f,z),new Vector3(.075f,2.20f,.075f),.022f);
  g.Box(4,0,new Vector3(0,3.97f,-.01f),new Vector3(3.55f,.27f,2.12f),.055f);
  g.Box(4,1,new Vector3(0,3.80f,-.01f),new Vector3(3.42f,.07f,1.98f),.02f);
  g.Box(4,6,new Vector3(0,3.755f,-.63f),new Vector3(3.22f,.034f,.16f),.012f);
  g.Box(4,3,new Vector3(0,2.82f,-1.055f),new Vector3(3.32f,1.80f,.012f),.004f);
  g.Box(4,3,new Vector3(0,2.82f,1.035f),new Vector3(3.32f,1.80f,.012f),.004f);
  foreach(float x in new[]{-1.68f,1.68f})g.Box(4,3,new Vector3(x,2.82f,0),new Vector3(.012f,1.80f,1.98f),.004f);
  g.Box(4,3,new Vector3(0,3.13f,0),new Vector3(3.28f,.025f,1.90f),.009f);
  g.Box(4,2,new Vector3(0,3.10f,.96f),new Vector3(3.28f,.018f,.027f),.007f);
  Bottle(g,4,new Vector3(-.88f,3.145f,.05f),.51f,1);Bottle(g,4,new Vector3(.88f,3.145f,.05f),.51f,2);
  g.Box(5,1,new Vector3(0,1.91f,-.12f),new Vector3(3.30f,.13f,1.43f),.028f);
  Bottle(g,5,new Vector3(-1.02f,1.985f,-.02f),.68f,0);Bottle(g,5,new Vector3(.03f,1.985f,.02f),.77f,1);Bottle(g,5,new Vector3(1.07f,1.985f,-.02f),.58f,2);
  foreach(float x in new[]{-1.02f,.03f,1.07f}){g.Box(5,9,new Vector3(x,1.91f,-.861f),new Vector3(.48f,.12f,.018f),.009f);g.Box(5,5,new Vector3(x,1.925f,-.873f),new Vector3(.37f,.074f,.008f),.003f);}
  Save(g,true,new Vector3(0,.049f,0),new Vector2(3.9f,2.3f),true);

  g=new Model("FragranceDisplay");
  g.Box(0,1,new Vector3(0,.35f,0),new Vector3(2.24f,.24f,1.74f),.045f);
  foreach(float x in new[]{-.85f,.85f})g.Box(-1,2,new Vector3(x,.89f,-.90f),new Vector3(.035f,1.44f,.022f),.007f);
  foreach(int part in new[]{4,5}){float x=part==4?-1.05f:1.05f;g.Box(part,2,new Vector3(x,.83f,0),new Vector3(.14f,1.33f,1.72f),.025f);g.Box(part,3,new Vector3(x,.90f,0),new Vector3(.018f,1.04f,1.58f),.005f);}
  g.Box(1,2,new Vector3(0,1.24f,-.79f),new Vector3(2.20f,.08f,.16f),.020f);g.Box(1,2,new Vector3(0,1.24f,.79f),new Vector3(2.20f,.08f,.16f),.020f);
  g.Box(2,3,new Vector3(0,.85f,-.93f),new Vector3(2.08f,1.29f,.015f),.005f);
  g.Box(3,0,new Vector3(0,.53f,0),new Vector3(1.60f,.18f,1.40f),.04f);Bottle(g,3,new Vector3(0,.63f,0),.82f,0);
  g.Box(3,3,new Vector3(0,.90f,.85f),new Vector3(2.05f,1.17f,.016f),.005f);
  Save(g,true,new Vector3(0,.050f,0),new Vector2(2.5f,2.1f),false);

  g=new Model("BoutiqueEntrance");
  foreach(float x in new[]{-2.78f,2.78f}){
   g.Box(-1,0,new Vector3(x,2.47f,.13f),new Vector3(.26f,4.90f,1.23f),.055f);
   g.Box(-1,1,new Vector3(x,2.45f,-.505f),new Vector3(.16f,4.80f,.035f),.015f);
   for(int k=-1;k<=1;k++)g.Box(-1,2,new Vector3(x+k*.047f,2.45f,-.528f),new Vector3(.014f,4.74f,.013f),.004f);
   // Open glass leaves nest beside the jamb; the physical centre remains open.
   g.Box(-1,3,new Vector3(x*.94f,2.35f,.10f),new Vector3(.020f,4.23f,1.18f),.006f);
   g.Box(-1,2,new Vector3(x*.94f,2.35f,-.48f),new Vector3(.025f,4.23f,.025f),.007f);
  }
  g.Box(-1,2,new Vector3(0,4.31f,-.93f),new Vector3(5.68f,.10f,.12f),.018f);
  for(int part=0;part<6;part++){
   float x=-2.25f+part*.9f;g.Box(part,1,new Vector3(x,4.69f,-.28f),new Vector3(.879f,.58f,1.85f),.012f);
   g.Box(part,9,new Vector3(x,4.935f,-1.19f),new Vector3(.876f,.026f,.04f),.008f);
   g.Box(part,9,new Vector3(x,4.445f,-1.19f),new Vector3(.876f,.026f,.04f),.008f);
   g.Box(part,6,new Vector3(x,4.415f,-.28f),new Vector3(.77f,.029f,1.60f),.009f);
  }
  Glyph(g,1,'E',new Vector3(-1.35f,4.69f,-1.20f),.31f);Glyph(g,3,'A',new Vector3(.45f,4.69f,-1.20f),.31f);Glyph(g,4,'U',new Vector3(1.35f,4.69f,-1.20f),.31f);
  Save(g,true,new Vector3(0,.051f,-.03f),new Vector2(6.3f,2.5f),false);

  g=new Model("BoutiqueBay");
  g.Box(-1,11,new Vector3(0,.022f,-10),new Vector3(24,.048f,44),.004f);
  for(int i=-11;i<=11;i++)g.Box(-1,1,new Vector3(i,.048f,-10),new Vector3(.012f,.002f,44),.001f);
  for(int i=-31;i<=11;i++)g.Box(-1,1,new Vector3(0,.048f,i),new Vector3(24,.002f,.012f),.001f);
  // One asymmetrical fragrance store; no duplicate row of generic product boxes.
  g.Box(-1,0,new Vector3(-5.9f,3.3f,8.3f),new Vector3(10.2f,6.6f,.28f),.055f);
  g.Box(-1,1,new Vector3(3.4f,3.3f,11.3f),new Vector3(8.2f,6.6f,.28f),.045f);
  g.Box(-1,0,new Vector3(8.8f,3.3f,8.3f),new Vector3(2.2f,6.6f,.28f),.055f);
  for(int i=0;i<16;i++)g.Box(-1,4,new Vector3(-9.9f+i*.57f,2.9f,8.10f),new Vector3(.085f,5.65f,.06f),.014f);
  g.Box(-1,1,new Vector3(-5.6f,2.4f,7.84f),new Vector3(6.6f,3.15f,.12f),.04f);
  foreach(float y in new[]{1.18f,2.28f,3.38f}){
   g.Box(-1,4,new Vector3(-5.6f,y,7.43f),new Vector3(6.6f,.09f,.70f),.020f);g.Box(-1,6,new Vector3(-5.6f,y-.065f,7.45f),new Vector3(6.38f,.024f,.42f),.009f);
   for(int i=0;i<3;i++)Bottle(g,-1,new Vector3(-7.7f+i*2.1f,y+.05f,7.4f),.49f,i);
  }
  g.Box(-1,0,new Vector3(0,6.6f,7.5f),new Vector3(22,.38f,8),.055f);
  g.Box(-1,1,new Vector3(0,6.37f,7.5f),new Vector3(21.5f,.10f,7.5f),.025f);
  foreach(float x in new[]{-7f,-2,3,8})g.Box(-1,6,new Vector3(x,6.29f,7.4f),new Vector3(.12f,.024f,6.5f),.009f);
  g.Box(-1,2,new Vector3(-.5f,3.15f,8.15f),new Vector3(.13f,6.1f,.30f),.025f);g.Box(-1,2,new Vector3(7.5f,3.15f,8.15f),new Vector3(.13f,6.1f,.30f),.025f);
  g.Box(-1,3,new Vector3(3.5f,3.2f,8.2f),new Vector3(7.7f,6.0f,.015f),.006f);
  Glyph(g,-1,'E',new Vector3(2.4f,5.40f,8.05f),.65f);Glyph(g,-1,'A',new Vector3(3.5f,5.40f,8.05f),.65f);Glyph(g,-1,'U',new Vector3(4.6f,5.40f,8.05f),.65f);
  Save(g,false,Vector3.zero,Vector2.zero,false);
  g=new Model("Product_RoundFlacon");Bottle(g,-1,Vector3.zero,1,0);Save(g,false,Vector3.zero,Vector2.zero,false);
  g=new Model("Product_SquareFlacon");Bottle(g,-1,Vector3.zero,1,1);Save(g,false,Vector3.zero,Vector2.zero,false);
  AssetDatabase.SaveAssets();AssetDatabase.Refresh();
 }
 static void Bottle(Model g,int part,Vector3 p,float h,int variant){
  float body=h*.72f,r=h*(variant==0?.25f:.24f);int sides=variant==2?12:20;
  if(variant==1){g.Box(part,8,p+Vector3.up*(body*.5f),new Vector3(r*1.65f,body,r*1.25f),r*.17f);g.Box(part,7,p+Vector3.up*(body*.37f),new Vector3(r*1.48f,body*.65f,r*1.10f),r*.13f);}
  else{g.Lathe(part,8,p,new[]{new Vector2(r*.65f,0),new Vector2(r,.035f*h),new Vector2(r,body*.72f),new Vector2(r*.89f,body*.89f),new Vector2(r*.38f,body),new Vector2(r*.30f,body+.05f*h)},sides);g.Lathe(part,7,p,new[]{new Vector2(r*.70f,.055f*h),new Vector2(r*.88f,.07f*h),new Vector2(r*.88f,body*.64f),new Vector2(0,body*.64f)},sides);}
  float neckY=body+.045f*h;g.Lathe(part,9,p,new[]{new Vector2(0,neckY),new Vector2(r*.40f,neckY),new Vector2(r*.40f,h*.85f),new Vector2(0,h*.85f)},sides);
  g.Lathe(part,variant==2?1:9,p,new[]{new Vector2(0,h*.84f),new Vector2(r*.53f,h*.84f),new Vector2(r*.56f,h*.88f),new Vector2(r*.56f,h*.98f),new Vector2(r*.47f,h),new Vector2(0,h)},sides);
  float z=variant==1?-r*.635f:-r*.99f;g.Box(part,5,p+new Vector3(0,body*.44f,z),new Vector3(r*1.20f,body*.34f,.006f*h),.004f*h);
  g.Box(part,1,p+new Vector3(0,body*.48f,z-.005f*h),new Vector3(r*.68f,.012f*h,.004f*h),.002f*h);
  g.Box(part,1,p+new Vector3(0,body*.39f,z-.005f*h),new Vector3(r*.43f,.007f*h,.004f*h),.0015f*h);
 }
 static void Glyph(Model g,int part,char letter,Vector3 c,float h){float w=h*.65f,t=h*.085f;Action<Vector2,Vector2> line=(a,b)=>g.Rod(part,9,c+new Vector3(a.x,a.y,0)*h,c+new Vector3(b.x,b.y,0)*h,t);
  if(letter=='E'){line(new Vector2(-.3f,-.5f),new Vector2(-.3f,.5f));foreach(float y in new[]{-.5f,0,.5f})line(new Vector2(-.3f,y),new Vector2(.3f,y));}
  if(letter=='A'){line(new Vector2(-.3f,-.5f),new Vector2(0,.5f));line(new Vector2(0,.5f),new Vector2(.3f,-.5f));line(new Vector2(-.17f,-.05f),new Vector2(.17f,-.05f));}
  if(letter=='U'){line(new Vector2(-.3f,.5f),new Vector2(-.3f,-.35f));line(new Vector2(.3f,.5f),new Vector2(.3f,-.35f));line(new Vector2(-.3f,-.35f),new Vector2(0,-.5f));line(new Vector2(0,-.5f),new Vector2(.3f,-.35f));}
 }
 static void Materials(){
  Maps();var surface=Shader.Find("MallModelR18/Surface");var glass=Shader.Find("MallModelR18/Glass");var shade=Shader.Find("MallModelR18/Contact shade");if(!surface||!glass||!shade)throw new Exception("R18 shader import failed");
  Color[] colors={new Color(.52f,.57f,.59f),new Color(.065f,.10f,.12f),new Color(.40f,.48f,.51f),new Color(.64f,.74f,.76f,.055f),new Color(.30f,.19f,.11f),new Color(.83f,.82f,.70f),new Color(.86f,.67f,.37f),new Color(.61f,.25f,.055f,.48f),new Color(.61f,.79f,.83f,.11f),new Color(.60f,.43f,.20f),new Color(.025f,.04f,.05f,.48f),new Color(.29f,.37f,.40f)};
  string[] names={"satin ceramic","charcoal gasket","brushed aluminium","clear edge glass","fluted oak","paper labels","warm diffuser","amber liquid","flacon glass","champagne brass","soft contact shade","cool honed stone"};mats=new Material[colors.Length];
  for(int i=0;i<mats.Length;i++){var m=new Material(i==10?shade:i==3||i==7||i==8?glass:surface){name="R18 "+names[i]};m.SetColor("_Color",colors[i]);if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",i==2?.78f:i==9?.68f:0);if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",i==2?.57f:i==9?.62f:i==3||i==8?.92f:i==11?.28f:.33f);if(m.HasProperty("_MainTex"))m.SetTexture("_MainTex",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+(i==4?"oak-grain":"fine-grain")+".png"));if(m.HasProperty("_BumpMap"))m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/fine-normal.png"));if(m.HasProperty("_NormalScale"))m.SetFloat("_NormalScale",i==4?.40f:i==11?.25f:.16f);if(i==6)m.SetColor("_EmissionColor",new Color(.12f,.07f,.025f));mats[i]=SaveAsset(m,Root+"/Materials/"+i+".mat");}
 }
 static void Maps(){const int n=256;foreach(string name in new[]{"fine-grain","oak-grain","fine-normal"}){var t=new Texture2D(n,n,TextureFormat.RGB24,true);var colors=new Color[n*n];for(int y=0;y<n;y++)for(int x=0;x<n;x++){float q=Mathf.Sin(x*12.9898f+y*78.233f)*43758.5453f;q-=Mathf.Floor(q);float a=.94f+q*.06f;if(name=="oak-grain")a=.75f+.12f*Mathf.Sin(x*.34f+Mathf.Sin(y*.07f)*.5f)+q*.08f;colors[y*n+x]=name=="fine-normal"?new Color(.5f+(q-.5f)*.12f,.5f+Mathf.Sin(x*.7f+y*.4f)*.045f,1):new Color(a,a,a);}t.SetPixels(colors);t.Apply();string path=Root+"/Textures/"+name+".png";File.WriteAllBytes(path,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(path);var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.wrapMode=TextureWrapMode.Repeat;importer.mipmapEnabled=true;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.sRGBTexture=name!="fine-normal";if(name=="fine-normal")importer.textureType=TextureImporterType.NormalMap;importer.SaveAndReimport();}}
 static T SaveAsset<T>(T value,string path)where T:Object{var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);Object.DestroyImmediate(value);return old;}AssetDatabase.CreateAsset(value,path);return value;}
 static void Save(Model model,bool shade,Vector3 shadeCenter,Vector2 shadeSize,bool lights){
  var root=new GameObject(model.name);foreach(var pair in model.parts){var mesh=pair.Value.Mesh(model.name+" part "+pair.Key);mesh=SaveAsset(mesh,Root+"/Meshes/"+model.name+"-"+pair.Key+".asset");var o=new GameObject("Part_"+pair.Key);o.transform.SetParent(root.transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterials=pair.Value.materialIndices.Select(i=>mats[i]).ToArray();r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
  if(shade){var o=new GameObject("IntactContactShade");o.transform.SetParent(root.transform,false);var mesh=Shadow(model.name,shadeCenter,shadeSize);mesh=SaveAsset(mesh,Root+"/Meshes/"+model.name+"-contact.asset");o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=mats[10];}
  if(lights)foreach(float x in new[]{-1.05f,1.05f}){var l=new GameObject("Local warm shelf light").AddComponent<Light>();l.transform.SetParent(root.transform,false);l.transform.localPosition=new Vector3(x,3.60f,-.15f);l.type=LightType.Point;l.range=2.3f;l.intensity=.38f;l.color=new Color(1,.77f,.45f);l.shadows=LightShadows.None;l.renderMode=LightRenderMode.ForceVertex;}
  Export(root,model.name);PrefabUtility.SaveAsPrefabAsset(root,Root+"/"+model.name+".prefab");Object.DestroyImmediate(root);
 }
 static Mesh Shadow(string name,Vector3 center,Vector2 size){var v=new List<Vector3>{center};var c=new List<Color>{new Color(1,1,1,.34f)};var t=new List<int>();const int n=24;for(int ring=0;ring<2;ring++)for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,r=ring==0?.37f:.57f;v.Add(center+new Vector3(Mathf.Cos(a)*size.x*r,0,Mathf.Sin(a)*size.y*r));c.Add(new Color(1,1,1,ring==0?.22f:0));}for(int i=0;i<n;i++){int j=(i+1)%n;t.AddRange(new[]{0,1+j,1+i,1+i,1+j,1+n+j,1+i,1+n+j,1+n+i});}var mesh=new Mesh{name=name+" soft contact"};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.RecalculateNormals();return mesh;}
 static void Export(GameObject root,string name){var obj=new StringBuilder("# Self-authored MallModelR18\nmtllib "+name+".mtl\n");var mtl=new StringBuilder();var seen=new HashSet<Material>();int offset=1;foreach(var f in root.GetComponentsInChildren<MeshFilter>()){var mesh=f.sharedMesh;var r=f.GetComponent<MeshRenderer>();if(!mesh.isReadable)continue;obj.Append("o "+f.name+"\n");var vs=mesh.vertices;var ns=mesh.normals;var uvs=mesh.uv;for(int i=0;i<vs.Length;i++){var v=f.transform.TransformPoint(vs[i]);obj.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"v {0} {1} {2}\n",v.x,v.y,v.z);}for(int i=0;i<vs.Length;i++){var uv=i<uvs.Length?uvs[i]:Vector2.zero;obj.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"vt {0} {1}\n",uv.x,uv.y);}for(int i=0;i<vs.Length;i++){var n=i<ns.Length?f.transform.TransformDirection(ns[i]):Vector3.up;obj.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"vn {0} {1} {2}\n",n.x,n.y,n.z);}for(int sub=0;sub<mesh.subMeshCount;sub++){var mat=r.sharedMaterials[sub];string id=mat.name.Replace(' ','_');obj.Append("usemtl "+id+"\n");if(seen.Add(mat)){var color=mat.color;mtl.AppendFormat(System.Globalization.CultureInfo.InvariantCulture,"newmtl {0}\nKd {1} {2} {3}\nd {4}\nNs 60\n\n",id,color.r,color.g,color.b,color.a);}var tri=mesh.GetTriangles(sub);for(int i=0;i<tri.Length;i+=3){int a=offset+tri[i],b=offset+tri[i+1],c=offset+tri[i+2];obj.AppendFormat("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",a,b,c);}}offset+=vs.Length;}File.WriteAllText(Path.Combine(export,name+".obj"),obj.ToString());File.WriteAllText(Path.Combine(export,name+".mtl"),mtl.ToString());}
 public sealed class Model {
  public readonly string name;public readonly Dictionary<int,Part> parts=new Dictionary<int,Part>();public Model(string name){this.name=name;}Part P(int id){if(!parts.TryGetValue(id,out var p)){p=new Part();parts.Add(id,p);}return p;}
  public void Box(int part,int material,Vector3 c,Vector3 size,float radius){P(part).Box(material,c,size,radius,Quaternion.identity);}
  public void Rod(int part,int material,Vector3 a,Vector3 b,float width){P(part).Box(material,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),width*.3f,Quaternion.FromToRotation(Vector3.up,b-a));}
  public void Lathe(int part,int material,Vector3 c,Vector2[] profile,int sides){P(part).Lathe(material,c,profile,sides);}
 }
 public sealed class Part {
  readonly List<Vector3> v=new List<Vector3>(),n=new List<Vector3>();readonly List<Vector2> uv=new List<Vector2>();readonly List<Color> colors=new List<Color>();readonly Dictionary<int,List<int>> indices=new Dictionary<int,List<int>>();public int[] materialIndices;
  List<int> T(int m){if(!indices.TryGetValue(m,out var t)){t=new List<int>();indices.Add(m,t);}return t;}
  int V(Vector3 p,Vector3 normal,Vector2 tex){int i=v.Count;v.Add(p);n.Add(normal);uv.Add(tex);float ao=Mathf.Lerp(.60f,1,Mathf.Clamp01(normal.y*.5f+.65f));colors.Add(new Color(ao,ao,ao,1));return i;}
  public void Box(int m,Vector3 c,Vector3 size,float radius,Quaternion rotation){var h=size*.5f;float r=Mathf.Min(radius,Mathf.Min(h.x,Mathf.Min(h.y,h.z))*.95f);var core=h-Vector3.one*r;Vector3[] normals={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};foreach(var normal in normals){var u=Mathf.Abs(normal.y)>.5f?Vector3.right:Vector3.up;var a=Vector3.Cross(u,normal);var b=Vector3.Cross(normal,a);float ha=Vector3.Dot(Abs(a),h),hb=Vector3.Dot(Abs(b),h);float[] xs={-ha,-ha+r,ha-r,ha},ys={-hb,-hb+r,hb-r,hb};int start=v.Count;for(int y=0;y<4;y++)for(int x=0;x<4;x++){var p=Vector3.Scale(normal,h)+a*xs[x]+b*ys[y];var q=new Vector3(Mathf.Clamp(p.x,-core.x,core.x),Mathf.Clamp(p.y,-core.y,core.y),Mathf.Clamp(p.z,-core.z,core.z));var smooth=(p-q).normalized;V(c+rotation*(q+smooth*r),rotation*smooth,new Vector2(xs[x],ys[y]));}var t=T(m);for(int y=0;y<3;y++)for(int x=0;x<3;x++){int i=start+y*4+x;t.AddRange(new[]{i,i+1,i+5,i,i+5,i+4});}}}
  static Vector3 Abs(Vector3 v){return new Vector3(Mathf.Abs(v.x),Mathf.Abs(v.y),Mathf.Abs(v.z));}
  public void Lathe(int m,Vector3 c,Vector2[] profile,int sides){int start=v.Count;for(int row=0;row<profile.Length;row++){var previous=profile[Mathf.Max(0,row-1)];var next=profile[Mathf.Min(profile.Length-1,row+1)];var d=next-previous;for(int i=0;i<=sides;i++){float a=i*Mathf.PI*2/sides;var normal=new Vector3(Mathf.Cos(a)*d.y,-d.x,Mathf.Sin(a)*d.y).normalized;V(c+new Vector3(Mathf.Cos(a)*profile[row].x,profile[row].y,Mathf.Sin(a)*profile[row].x),normal,new Vector2((float)i/sides,profile[row].y));}}var t=T(m);for(int row=0;row<profile.Length-1;row++)for(int i=0;i<sides;i++){int k=start+row*(sides+1)+i;t.AddRange(new[]{k,k+sides+1,k+1,k+1,k+sides+1,k+sides+2});}}
  public Mesh Mesh(string name){var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetNormals(n);mesh.SetUVs(0,uv);mesh.SetColors(colors);materialIndices=indices.Keys.OrderBy(i=>i).ToArray();mesh.subMeshCount=materialIndices.Length;for(int i=0;i<materialIndices.Length;i++)mesh.SetTriangles(indices[materialIndices[i]],i);mesh.RecalculateBounds();return mesh;}
 }
}

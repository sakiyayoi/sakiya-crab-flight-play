using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using C2=CrabCityDensityR2;

// Static relief on existing non-breakable mid-band buildings only.
// The geometry shares their exact placement transform and parallax parent.
public static class CityDetailR6 {
 const string RootName="CityDetailR6 facade relief";
 const int TriangleLimit=10000;

 // Dense city layout 2: call once after City2Data.Build, passing its city root.
 public static GameObject Build(Transform parent,Shader shader){
  if(!parent)throw new ArgumentNullException(nameof(parent));
  if(!shader||!shader.isSupported)throw new ArgumentException("A supported existing opaque shader is required",nameof(shader));
  var band=parent.Find("DecorativeMid");
  if(!band)throw new InvalidOperationException("CityDetailR6 needs the existing dense city DecorativeMid band");
  var existing=band.Find(RootName);if(existing)return existing.gameObject;
  var input=Resources.Load<TextAsset>("CityDensityR2/CityPlacementDense");
  if(!input)throw new InvalidOperationException("Dense city layout is missing");
  var layout=JsonUtility.FromJson<C2.CityDensityPack.Layout>(input.text);
  if(layout==null||layout.instances==null)throw new InvalidOperationException("Dense city layout could not be read");
  int projectedTriangles=0;
  foreach(var p in layout.instances)if(Eligible(p))projectedTriangles+=p.asset=="shopfront_module"?108:120;
  if(projectedTriangles>TriangleLimit)throw new InvalidOperationException("CityDetailR6 facade triangle budget exceeded");
  var root=new GameObject(RootName);root.transform.SetParent(band,false);
  var owner=root.AddComponent<C2.CityDensityOwnedAssets>();
  var brush=new Relief(root.transform,owner,shader);
  foreach(var p in layout.instances){
   if(!Eligible(p))continue;
   var matrix=Matrix4x4.TRS(V(p.p),Quaternion.Euler(0,p.yaw,0),V(p.s));
   if(p.asset=="shopfront_module")Shop(brush,matrix);
   else Facade(brush,matrix,City2Data.Asset("facade_module"));
  }
  brush.Finish();return root;
 }
 static bool Eligible(C2.CityDensityPack.Placement p){return !p.breakable&&p.band=="mid"&&(p.asset=="shopfront_module"||p.asset=="facade_module");}
 static Vector3 V(float[] a){return new Vector3(a[0],a[1],a[2]);}

 static void Shop(Relief b,Matrix4x4 m){
  // A stepped overhang connects to the original balcony ledge and wall.
  b.Box(m,new Vector3(0,7.84f,-.05f),new Vector3(9.95f,.22f,4.2f),false);
  b.Box(m,new Vector3(0,7.53f,-1.88f),new Vector3(9.8f,.24f,.54f),false);
  b.Box(m,new Vector3(0,7.36f,-.65f),new Vector3(9.35f,.10f,2.9f),true);
  // Thick jambs sit on the existing piers, outside the display glazing.
  b.Box(m,new Vector3(.16f,2.4f,-.5f),new Vector3(.18f,3.9f,.8f),false);
  b.Box(m,new Vector3(4.10f,2.4f,-.5f),new Vector3(.18f,3.9f,.8f),false);
  b.Box(m,new Vector3(2.13f,.50f,-.56f),new Vector3(4.24f,.18f,1f),false);
  b.Box(m,new Vector3(2.13f,4.30f,-.72f),new Vector3(4.24f,.18f,1.3f),true);
  foreach(float x in new[]{-3.95f,3.95f})b.Box(m,new Vector3(x,7.10f,-.50f),new Vector3(.24f,.55f,1.8f),false);
 }
 static void Facade(Relief b,Matrix4x4 m,C2.CityDensityPack.Asset asset){
  int window=0;
  foreach(var piece in asset.pieces){
   if(!piece.name.StartsWith("facade_module__window_recess",StringComparison.Ordinal))continue;
   int row=window++/3;if(row!=1&&row!=3)continue;
   var bounds=City2Data.Bounds(piece);
   b.Box(m,new Vector3(bounds.center.x,bounds.max.y+.10f,-.42f),new Vector3(bounds.size.x+.30f,.18f,1.1f),true);
  }
  foreach(float x in new[]{-4.8f,4.8f})b.Box(m,new Vector3(x,7.5f,-.31f),new Vector3(.34f,15f,.64f),false);
  b.Box(m,new Vector3(0,15.25f,.70f),new Vector3(10.85f,.20f,3.5f),false);
  b.Box(m,new Vector3(0,15.075f,-.52f),new Vector3(10.55f,.15f,.9f),true);
 }

 sealed class Relief {
  readonly Transform parent;readonly C2.CityDensityOwnedAssets owner;
  readonly Surface[] surfaces;
  static readonly int[] Faces={0,3,2,1,4,5,6,7,0,4,7,3,1,2,6,5,3,7,6,2,0,1,5,4};
  static readonly int[] StoneFaces={0,3,3,3,1,2},MetalFaces={3,3,2,2,0,2};
  public Relief(Transform parent,C2.CityDensityOwnedAssets owner,Shader shader){
   this.parent=parent;this.owner=owner;
   // Tones of the existing city ivory, roof and steel palette; no emission.
   surfaces=new[]{
    new Surface("soft ivory fronts",Color.Lerp(new Color(.79f,.80f,.70f),new Color(.27f,.32f,.34f),.22f),shader,owner),
    new Surface("ivory top edges",new Color(.79f,.80f,.70f),shader,owner),
    new Surface("steel undersides",new Color(.16f,.23f,.24f),shader,owner),
    new Surface("roof side returns",new Color(.27f,.32f,.34f),shader,owner)
   };
  }
  public void Box(Matrix4x4 matrix,Vector3 center,Vector3 size,bool metal){
   var e=size*.5f;
   var corners=new[]{new Vector3(-e.x,-e.y,-e.z),new Vector3(e.x,-e.y,-e.z),new Vector3(e.x,e.y,-e.z),new Vector3(-e.x,e.y,-e.z),new Vector3(-e.x,-e.y,e.z),new Vector3(e.x,-e.y,e.z),new Vector3(e.x,e.y,e.z),new Vector3(-e.x,e.y,e.z)};
   var palette=metal?MetalFaces:StoneFaces;
   for(int f=0;f<6;f++){
    var surface=surfaces[palette[f]];
    surface.Quad(matrix.MultiplyPoint3x4(center+corners[Faces[f*4]]),matrix.MultiplyPoint3x4(center+corners[Faces[f*4+1]]),matrix.MultiplyPoint3x4(center+corners[Faces[f*4+2]]),matrix.MultiplyPoint3x4(center+corners[Faces[f*4+3]]));
   }
  }
  public void Finish(){foreach(var surface in surfaces)surface.Finish(parent,owner);}
 }
 sealed class Surface {
  readonly string name;readonly Material material;
  readonly List<Vector3> vertices=new List<Vector3>();readonly List<int> triangles=new List<int>();readonly List<Vector2> uv=new List<Vector2>();
  public Surface(string name,Color color,Shader shader,C2.CityDensityOwnedAssets owner){
   this.name=name;material=new Material(shader){name="CityDetailR6 "+name,color=color};
   if(material.HasProperty("_MainTex"))material.SetTexture("_MainTex",Texture2D.whiteTexture);
   if(material.HasProperty("_MetallicGlossMap"))material.SetTexture("_MetallicGlossMap",Texture2D.blackTexture);
   if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",.12f);
   if(material.HasProperty("_Metallic"))material.SetFloat("_Metallic",0);
   owner.materials.Add(material);
  }
  public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){
   int start=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);vertices.Add(d);
   triangles.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});
   uv.AddRange(new[]{Vector2.zero,Vector2.up,Vector2.one,Vector2.right});
  }
  public void Finish(Transform parent,C2.CityDensityOwnedAssets owner){
   if(vertices.Count==0)return;
   var mesh=new Mesh{name="CityDetailR6 "+name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.SetUVs(0,uv);mesh.RecalculateNormals();mesh.RecalculateBounds();owner.meshes.Add(mesh);mesh.UploadMeshData(true);
   var obj=new GameObject(name);obj.transform.SetParent(parent,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;
   var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
  }
 }
}

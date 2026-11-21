using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Authored scenery on the existing country / launch / orbit route. No collision or tick code.
// One shared palette material and one mesh per side/region keep distant detail batched.
public static class LateWorldArtR9 {
 public static bool Enabled=true;
 public const string Revision="late-world-r9";
 static readonly Color[] Colors={
  new Color(.72f,.77f,.64f),new Color(.57f,.72f,.61f),new Color(.71f,.83f,.75f),new Color(.89f,.86f,.75f),
  new Color(.38f,.51f,.54f),new Color(.79f,.50f,.26f),new Color(.85f,.91f,.88f),new Color(.115f,.18f,.265f),
  new Color(.22f,.39f,.51f),new Color(.38f,.67f,.62f),new Color(.94f,.77f,.36f),new Color(.29f,.39f,.43f),
  new Color(.63f,.70f,.69f),new Color(.45f,.59f,.64f),new Color(.67f,.80f,.83f),new Color(.90f,.91f,.83f)};
 public static GameObject Build(Transform parent,Shader shader){
  var root=new GameObject(Revision);root.transform.SetParent(parent,false);var owned=root.AddComponent<LateWorldAssetsR9>();
  var atlas=new Texture2D(4,4,TextureFormat.RGBA32,false,true){name="R9 distant palette",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};atlas.SetPixels(Colors);atlas.Apply(false,true);owned.atlas=atlas;
  var material=new Material(shader){name="R9 shared distant palette"};material.SetTexture("_MainTex",atlas);material.color=Color.white;if(material.HasProperty("_Glossiness"))material.SetFloat("_Glossiness",.18f);owned.material=material;
  foreach(float side in new[]{-1f,1f}){Country(root.transform,side,material,owned);Range(root.transform,side,material,owned);Orbit(root.transform,side,material,owned);}return root;
 }
 static void Country(Transform root,float side,Material material,LateWorldAssetsR9 owned){
  var b=new Brush(side);b.Box(0,new Vector3(1675,.015f,-106),new Vector3(750,.025f,228));b.Box(0,new Vector3(1675,-90,8),new Vector3(750,180,.10f));
  b.Hill(2,1425,75,280,12);b.Hill(2,1800,100,410,19);b.Hill(1,1545,54,235,9);b.Hill(1,1910,62,240,12);
  foreach(var plot in new[]{new Vector3(1387,76,20),new Vector3(1517,100,24),new Vector3(1760,125,28),new Vector3(1950,70,21)}){
   b.Box(1,b.Ground(plot.x,.08f,plot.z),new Vector3(plot.y,.12f,14));for(int row=0;row<4;row++)b.Box(0,b.Ground(plot.x,.16f,plot.z-5+row*3),new Vector3(plot.y-2,.07f,.65f));
  }
  // A pump house belongs to the actual large irrigation vessel, rather than another city crate.
  b.Box(3,b.Ground(1640,4,34),new Vector3(22,8,12));b.Roof(5,b.Ground(1640,9.4f,34),new Vector3(26,4,15));
  b.Box(4,b.Ground(1640,2.9f,27.8f),new Vector3(4.2f,5.8f,.12f));
  foreach(float x in new[]{1633f,1647}){b.Box(4,b.Ground(x,5,27.7f),new Vector3(3.5f,2.5f,.12f));b.Box(6,b.Ground(x,5,27.6f),new Vector3(.10f,2.5f,.10f));}
  b.Rod(12,b.Ground(1610,1.4f,22),b.Ground(1637,1.4f,22),.5f);b.Rod(12,b.Ground(1637,1.4f,22),b.Ground(1637,3.2f,30),.5f);
  // Different-sized silos and one turbine are landmarks, not a uniform row of poles.
  foreach(var silo in new[]{new Vector3(1454,10,3),new Vector3(1462,7,2.5f),new Vector3(1848,15,4.5f)}){b.Cylinder(12,b.Ground(silo.x,silo.y*.5f,38),silo.z,silo.y);b.Cone(3,b.Ground(silo.x,silo.y+1.5f,38),silo.z,3);}
  var hub=b.Ground(1727,35,44);b.Rod(6,b.Ground(1727,0,44),hub,.8f);b.Circle(4,hub,.9f,16);
  for(int j=0;j<3;j++){float a=(18+j*120)*Mathf.Deg2Rad;var end=hub+new Vector3(Mathf.Cos(a)*13,Mathf.Sin(a)*13,0);b.Rod(6,hub,end,.7f);}
  b.Finish(root,"country terraces, crops and water works",material,owned);
 }
 static void Range(Transform root,float side,Material material,LateWorldAssetsR9 owned){
  var b=new Brush(side);b.Box(12,new Vector3(2365,.018f,-106),new Vector3(630,.03f,228));b.Box(12,new Vector3(2365,-90,8),new Vector3(630,180,.10f));
  // Service sheds, a launch assembly and a tracking dish have distinct silhouettes.
  b.Box(11,b.Ground(2144,2,42),new Vector3(76,4,35));b.Box(3,b.Ground(2157,12,47),new Vector3(48,24,28));b.Roof(12,b.Ground(2157,25,47),new Vector3(52,5,31));
  b.Box(4,b.Ground(2157,9,32.8f),new Vector3(31,18,.18f));for(int i=0;i<4;i++)b.Box(12,b.Ground(2145+i*8,9,32.6f),new Vector3(.20f,18,.13f));
  float rocket=2123,z=28;b.Cylinder(3,b.Ground(rocket,18,z),3.2f,28);b.Cone(6,b.Ground(rocket,35,z),3.2f,6);b.Cylinder(5,b.Ground(rocket,17,z),3.22f,2.2f);b.Cylinder(4,b.Ground(rocket,5,z),4.1f,3);
  b.Rod(4,b.Ground(rocket-10,0,z+3),b.Ground(rocket-10,37,z+3),1.25f);foreach(float y in new[]{10f,21,33}){b.Rod(4,b.Ground(rocket-10,y,z+3),b.Ground(rocket-1,y,z+3),.65f);b.Box(10,b.Ground(rocket-6,y+.7f,z+2.8f),new Vector3(8,.10f,.10f));}
  b.Box(11,b.Ground(2354,2,37),new Vector3(74,4,32));b.Box(3,b.Ground(2356,8,40),new Vector3(58,16,24));b.Roof(12,b.Ground(2356,17.6f,40),new Vector3(62,4,27));
  b.Rod(4,b.Ground(2308,0,31),b.Ground(2308,21,31),1.2f);b.Dish(6,b.Ground(2308,24,31),11,7);b.Rod(4,b.Ground(2308,24,31),b.Ground(2311,31,30),.3f);
  b.Cylinder(12,b.Ground(2606,15,43),8,30);b.Cone(3,b.Ground(2606,33,43),8,6);b.Rod(4,b.Ground(2570,0,29),b.Ground(2570,45,29),1.6f);b.Rod(4,b.Ground(2570,44,29),b.Ground(2602,44,29),1.1f);
  foreach(float x in new[]{2078f,2090,2172,2251,2263,2423,2530,2542,2620})b.Box(10,new Vector3(x,.04f,-1.2f),new Vector3(3,.015f,.6f));
  b.Finish(root,"launch assembly, hangars and tracking dish",material,owned);
 }
 static void Orbit(Transform root,float side,Material material,LateWorldAssetsR9 owned){
  var b=new Brush(side);b.Box(7,new Vector3(2950,63,145),new Vector3(540,280,4));b.Box(11,new Vector3(2950,.02f,-106),new Vector3(540,.03f,228));b.Box(11,new Vector3(2950,-90,8),new Vector3(540,180,.10f));
  // Static limb, ocean and cloud arcs: no flashing sky, duplicate rings or moving camera.
  var earth=b.Ground(2786,38,130);b.Circle(14,earth,72,64);b.Circle(8,earth-Vector3.forward*.08f,68,64);
  b.Arc(6,earth-Vector3.forward*.15f,65,60,118,48,1.4f);b.Arc(2,earth+new Vector3(7,12,-.2f),48,12,92,32,3.8f);b.Arc(14,earth+new Vector3(-19,-10,-.25f),43,36,131,32,2.1f);
  for(int i=0;i<22;i++){float x=2695+(i*67%490),y=4+(i*13%70);b.Circle(i%5==0?6:13,b.Ground(x,y,125),i%5==0?.32f:.12f,6);}
  foreach(var hub in new[]{new Vector3(2753,12,24),new Vector3(2963,18,34),new Vector3(3153,24,40)}){
   var c=b.Ground(hub.x,hub.y,hub.z);b.Cylinder(3,c,4.2f,13);b.Cylinder(4,c+Vector3.up*6.4f,4.3f,.5f);b.Cylinder(4,c-Vector3.up*6.4f,4.3f,.5f);
   b.Box(11,c+new Vector3(0,0,-4),new Vector3(4.6f,3.4f,.4f));b.Circle(14,c+new Vector3(0,0,-4.3f),1.15f,16);
   foreach(float sign in new[]{-1f,1f}){b.Box(4,c+new Vector3(sign*17,0,0),new Vector3(30,.6f,.9f));b.Box(8,c+new Vector3(sign*24,0,0),new Vector3(22,12,.35f));for(int k=0;k<6;k++)b.Box(13,c+new Vector3(sign*(14+k*4),0,-.22f),new Vector3(.12f,11.6f,.06f));for(int k=-1;k<=1;k++)b.Box(13,c+new Vector3(sign*24,k*4,-.22f),new Vector3(22,.10f,.06f));}
  }
  b.Finish(root,"orbital modules, array wings and earth limb",material,owned);
 }
 sealed class Brush {
  readonly float side;readonly List<Vector3> v=new List<Vector3>();readonly List<Vector2> uv=new List<Vector2>();readonly List<int> t=new List<int>();
  public Brush(float side){this.side=side;}public Vector3 Ground(float x,float height,float z)=>new Vector3(x,height-z*5.3f/16,z);
  void Tri(int role,Vector3 a,Vector3 b,Vector3 c){int k=v.Count;v.Add(new Vector3(a.x*side,a.y,a.z));v.Add(new Vector3(b.x*side,b.y,b.z));v.Add(new Vector3(c.x*side,c.y,c.z));var tex=new Vector2((role%4+.5f)/4,(role/4+.5f)/4);uv.Add(tex);uv.Add(tex);uv.Add(tex);if(side>0)t.AddRange(new[]{k,k+1,k+2});else t.AddRange(new[]{k,k+2,k+1});}
  void Quad(int r,Vector3 a,Vector3 b,Vector3 c,Vector3 d){Tri(r,a,b,c);Tri(r,a,c,d);}
  public void Box(int r,Vector3 c,Vector3 s){BoxRot(r,c,s,Quaternion.identity);}
  void BoxRot(int r,Vector3 c,Vector3 s,Quaternion q){var p=new Vector3[8];for(int i=0;i<8;i++)p[i]=c+q*Vector3.Scale(new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1),s*.5f);Quad(r,p[0],p[2],p[3],p[1]);Quad(r,p[4],p[5],p[7],p[6]);Quad(r,p[0],p[4],p[6],p[2]);Quad(r,p[1],p[3],p[7],p[5]);Quad(r,p[2],p[6],p[7],p[3]);Quad(r,p[0],p[1],p[5],p[4]);}
  public void Rod(int r,Vector3 a,Vector3 b,float width){BoxRot(r,(a+b)*.5f,new Vector3(width,(b-a).magnitude,width),Quaternion.FromToRotation(Vector3.up,b-a));}
  public void Cylinder(int r,Vector3 c,float radius,float height){for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;var x=new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius);var y=new Vector3(Mathf.Cos(b)*radius,0,Mathf.Sin(b)*radius);var h=Vector3.up*height*.5f;Quad(r,c+x-h,c+x+h,c+y+h,c+y-h);Tri(r,c+h,c+y+h,c+x+h);Tri(r,c-h,c+x-h,c+y-h);}}
  public void Cone(int r,Vector3 c,float radius,float h){for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;var x=c+new Vector3(Mathf.Cos(a)*radius,-h*.5f,Mathf.Sin(a)*radius);var y=c+new Vector3(Mathf.Cos(b)*radius,-h*.5f,Mathf.Sin(b)*radius);Tri(r,x,c+Vector3.up*h*.5f,y);Tri(r,c-Vector3.up*h*.5f,x,y);}}
  public void Circle(int r,Vector3 c,float radius,int n){for(int i=0;i<n;i++){float a=i*Mathf.PI*2/n,b=(i+1)*Mathf.PI*2/n;Tri(r,c,c+new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0),c+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0));}}
  public void Arc(int r,Vector3 c,float radius,float start,float end,int n,float w){for(int i=0;i<n;i++){float a=Mathf.Lerp(start,end,(float)i/n)*Mathf.Deg2Rad,b=Mathf.Lerp(start,end,(i+1f)/n)*Mathf.Deg2Rad;Rod(r,c+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius,0),c+new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius,0),w);}}
  public void Hill(int r,float x,float z,float width,float height){var c=Ground(x,0,z);for(int i=0;i<22;i++){float a=Mathf.PI*i/22,b=Mathf.PI*(i+1)/22;Tri(r,c,c+new Vector3(Mathf.Cos(b)*width*.5f,Mathf.Sin(b)*height,0),c+new Vector3(Mathf.Cos(a)*width*.5f,Mathf.Sin(a)*height,0));}}
  public void Roof(int r,Vector3 c,Vector3 s){var a=c+new Vector3(-s.x*.5f,-s.y*.5f,-s.z*.5f);var b=c+new Vector3(s.x*.5f,-s.y*.5f,-s.z*.5f);var m=c+new Vector3(0,s.y*.5f,-s.z*.5f);var z=Vector3.forward*s.z;Tri(r,a,m,b);Tri(r,a+z,b+z,m+z);Quad(r,a,a+z,m+z,m);Quad(r,m,m+z,b+z,b);Quad(r,a,b,b+z,a+z);}
  public void Dish(int r,Vector3 c,float radius,float depth){for(int i=0;i<20;i++){float a=i*Mathf.PI/10,b=(i+1)*Mathf.PI/10;var x=c+new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius*.38f,0);var y=c+new Vector3(Mathf.Cos(b)*radius,Mathf.Sin(b)*radius*.38f,0);Tri(r,c-Vector3.forward*depth,y,x);}}
  public void Finish(Transform p,string name,Material material,LateWorldAssetsR9 owned){var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();owned.meshes.Add(mesh);var go=new GameObject(name+" "+side);go.transform.SetParent(p,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;mesh.UploadMeshData(true);}
 }
}
public sealed class LateWorldAssetsR9:MonoBehaviour {
 public readonly List<Mesh> meshes=new List<Mesh>();public Material material;public Texture2D atlas;
 void OnDestroy(){foreach(var m in meshes)Release(m);Release(material);Release(atlas);}
 static void Release(UnityEngine.Object o){if(!o)return;if(Application.isPlaying)UnityEngine.Object.Destroy(o);else UnityEngine.Object.DestroyImmediate(o);}
}

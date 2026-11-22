using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable] public sealed class NeonSectorArtInfo {public string id,motif;public int clusters,triangles;public float start,end,nearestZ=8.05f;}
// Batched fixed geometry at z>=8.05, behind gameplay and the existing signs.
// No Random, time, physics components, scene searches or gameplay writes.
public sealed class NeonSectorArt:MonoBehaviour {
 public readonly List<NeonSectorArtInfo> Sectors=new List<NeonSectorArtInfo>();
 readonly List<Mesh> meshes=new List<Mesh>();readonly List<Material> materials=new List<Material>();
 static readonly Color Sky=new Color(.005f,.007f,.022f),Ink=new Color(.012f,.022f,.047f),Face=new Color(.024f,.038f,.065f);
 sealed class Brush {
  public readonly List<Vector3> v=new List<Vector3>();public readonly List<Color> c=new List<Color>();public readonly List<Vector2> uv=new List<Vector2>();public readonly List<int> t=new List<int>();
  readonly float side;public Brush(float s){side=s;}
  public void Tri(Vector2 a,Vector2 b,Vector2 d,Color ink,float z=8.3f){int n=v.Count;foreach(var p in new[]{a,b,d}){v.Add(new Vector3(p.x*side,p.y,z));c.Add(ink);uv.Add(Vector2.one*.5f);}t.AddRange(new[]{n,n+1,n+2});}
  public void Quad(Vector2 a,Vector2 b,Vector2 d,Vector2 e,Color ink,float z=8.3f){Tri(a,b,d,ink,z);Tri(a,d,e,ink,z);}
  public void Box(float x,float y,float w,float h,Color ink,float z=8.3f){Quad(new Vector2(x-w/2,y-h/2),new Vector2(x+w/2,y-h/2),new Vector2(x+w/2,y+h/2),new Vector2(x-w/2,y+h/2),ink,z);}
  public void Line(float ax,float ay,float bx,float by,float width,Color ink,float z=8.15f){var a=new Vector2(ax,ay);var b=new Vector2(bx,by);var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;Quad(a-n,b-n,b+n,a+n,ink,z);}
  public void Disc(float x,float y,float rx,float ry,Color ink,float z=8.3f){for(int k=0;k<24;k++){float a=k*Mathf.PI/12,b=(k+1)*Mathf.PI/12;Tri(new Vector2(x,y),new Vector2(x+Mathf.Cos(a)*rx,y+Mathf.Sin(a)*ry),new Vector2(x+Mathf.Cos(b)*rx,y+Mathf.Sin(b)*ry),ink,z);}}
  public void Glow(float x,float y,float size,Color ink){int n=v.Count;Box(x,y,size,size,ink,8.05f);var u=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.zero,Vector2.one,Vector2.up};for(int i=0;i<6;i++)uv[n+i]=u[i];}
 }
 public static NeonSectorArt Build(Transform owner){var go=new GameObject("Neon authored sectors render only");go.transform.SetParent(owner,false);var art=go.AddComponent<NeonSectorArt>();art.BuildAll();return art;}
 void BuildAll(){
  var solid=new Material(Resources.Load<Shader>("NeonProto/NeonScenery")){name="Neon sector vertex ink"};
  var glow=NeonGlowTexture.Material(Color.white);glow.name="Neon sector baked point light";materials.Add(solid);materials.Add(glow);
  string[] ids={"port","viaduct","country","launchrange","orbit"},motifs={"dock cranes / stacked ribbed containers","continuous deck / tapered piers / diagonal bracing","gabled homes / field furrows / wired poles","launch gantry / service platforms / rounded fuel tanks","station hubs / solar arrays / fixed starfield"};
  float[] starts={600,900,1300,2050,2680},ends={900,1300,2050,2680,3220};
  Color[] tones={new Color(.06f,.50f,.34f),new Color(.45f,.08f,.29f),new Color(.48f,.34f,.07f),new Color(.48f,.10f,.06f),new Color(.07f,.32f,.55f)};
  for(int i=0;i<5;i++){
   var info=new NeonSectorArtInfo{id=ids[i],motif=motifs[i],start=starts[i],end=ends[i]};Sectors.Add(info);
   foreach(float side in new[]{1f,-1f}){
    if(PortLayout.Enabled&&i==0&&side>0)continue;
    var b=new Brush(side);var g=new Brush(side);Color neon=tones[i];
    b.Box((starts[i]+ends[i])*.5f,70,ends[i]-starts[i],190,Sky,8.9f);
    // Recessed horizon, no opaque front geometry: all contacts remain in front.
    b.Box((starts[i]+ends[i])*.5f,-8,ends[i]-starts[i],13,Ink,8.7f);
    float step=i==0?52:i==1?46:i==2?43:i==3?62:68;
    float[] anchors=i==0?new[]{622f,681f,735f,794f,860f}:i==3?new[]{2086f,2148f,2213f,2287f,2355f,2430f,2505f,2570f,2644f}:i==4?new[]{2718f,2792f,2860f,2938f,3010f,3086f,3160f}:null;
    int count=anchors==null?Mathf.FloorToInt((ends[i]-starts[i]-step*.5f)/step)+1:anchors.Length;
    for(int k=0;k<count;k++){
     float x=anchors==null?starts[i]+step*.5f+k*step:anchors[k];
     if(i==0)Port(b,g,x,k,neon);else if(i==1)Viaduct(b,g,x,step,k,neon);else if(i==2)Country(b,g,x,k,neon);else if(i==3)Range(b,g,x,k,neon);else Orbit(b,g,x,k,neon);
     if(side>0)info.clusters++;
    }
    if(i==1){b.Box(1100,15,400,2.1f,Face);b.Line(900,16.1f,1300,16.1f,.13f,neon);b.Line(900,13.9f,1300,13.9f,.06f,neon*.5f);}
    if(i==4)for(int k=0;k<245;k++){float x=2682+(k*89%535),y=3+(k*31%94);float s=k%9==0?.14f:.055f;b.Disc(x,y,s,s,k%5==0?neon*.7f:new Color(.17f,.21f,.32f),8.55f);if(k%19==0)g.Glow(x,y,1.0f,neon*.5f);}
    info.triangles+=b.t.Count/3+g.t.Count/3;Finish(b,ids[i]+" silhouette and neon edges "+side,solid);Finish(g,ids[i]+" baked lights "+side,glow);
   }
  }
 }
 static void Port(Brush b,Brush g,float x,int k,Color n){
  float h=19+(k%3)*3;
  b.Line(x-9,0,x-7,h,1.0f,Face,8.4f);b.Line(x+1,0,x-3,h,.9f,Face,8.4f);b.Line(x-12,h,x+15,h+2,1.0f,Face);b.Line(x-7,h-7,x+15,h+2,.32f,Face);
  b.Line(x-7,1,x-7,h,.10f,n);b.Line(x-12,h+.4f,x+15,h+2.4f,.13f,n);b.Line(x+12,h+2,x+12,9,.07f,n*.7f);b.Line(x+10,9,x+14,9,.28f,Face);
  g.Glow(x-7,h,2.0f,n*.7f);g.Glow(x+15,h+2,1.6f,n*.6f);
  for(int row=0;row<2;row++)for(int c=0;c<(row==0?3:2);c++){float xx=x-12+c*9+row*3,yy=1.7f+row*3.4f;Color tint=Color.Lerp(Face,n,.05f+(c%2)*.025f);b.Box(xx,yy,8.5f,3.2f,tint,8.25f);b.Line(xx-4.2f,yy+1.6f,xx+4.2f,yy+1.6f,.075f,n*.6f);for(int j=0;j<7;j++)b.Line(xx-3.6f+j*1.2f,yy-1.4f,xx-3.6f+j*1.2f,yy+1.4f,.055f,n*.16f);}
 }
 static void Viaduct(Brush b,Brush g,float x,float step,int k,Color n){
  b.Quad(new Vector2(x-2,-1),new Vector2(x+2,-1),new Vector2(x+1.1f,14),new Vector2(x-1.1f,14),Face);
  b.Box(x,13,8,1.2f,Face);b.Line(x-1.3f,0,x-1.1f,13,.09f,n*.65f);
  b.Line(x-18,13.6f,x,7,.48f,Ink);b.Line(x,7,x+18,13.6f,.48f,Ink);b.Line(x,7,x+18,13.6f,.07f,n*.4f);
  for(int j=-2;j<=2;j++){float xx=x+j*8;b.Line(xx,16.2f,xx,18.4f,.08f,n*.55f);}b.Line(x-22,18.4f,x+22,18.4f,.08f,n*.55f);
  b.Line(x+8,16,x+8,22,.22f,Face);b.Line(x+8,22,x+11,22,.15f,n);g.Glow(x+11,21.8f,2.2f,n*.6f);
 }
 static void Country(Brush b,Brush g,float x,int k,Color n){
  float h=4.5f+(k%3),w=8+k%2*2;
  b.Box(x-4,h*.5f,w,h,Face);b.Tri(new Vector2(x-5-w/2,h),new Vector2(x-4,h+3),new Vector2(x-3+w/2,h),Ink);
  b.Line(x-5-w/2,h,x-4,h+3,.10f,n*.7f);b.Line(x-4,h+3,x-3+w/2,h,.10f,n*.7f);
  b.Box(x-6,2.2f,1.4f,1.7f,n*.58f,8.15f);b.Box(x-2,2.2f,1.1f,1.7f,n*.38f,8.15f);g.Glow(x-6,2.2f,3,n*.25f);
  b.Box(x+9,1.0f,15,1.5f,new Color(.02f,.055f,.037f),8.4f);
  for(int j=0;j<7;j++)b.Line(x+2+j*2,0,x+4+j*2,2.0f,.10f,new Color(.06f,.17f,.10f));
  b.Line(x+16,0,x+16,12.2f,.30f,Face);b.Line(x+13.5f,11,x+18.5f,11,.16f,n*.6f);
  for(int j=0;j<2;j++){float y=11.5f-j*.6f;for(int q=0;q<6;q++){float u=q/6f,v=(q+1)/6f;b.Line(x+16-43*u,y-1.1f*Mathf.Sin(u*Mathf.PI),x+16-43*v,y-1.1f*Mathf.Sin(v*Mathf.PI),.045f,n*.24f);}}
 }
 static void Range(Brush b,Brush g,float x,int k,Color n){
  b.Box(x-6,1,25,2,Face);b.Box(x-11,13,3,26,Face);b.Line(x-12.5f,1,x-12.5f,26,.12f,n);
  for(int j=0;j<5;j++){float y=3+j*4.7f;b.Line(x-12,y,x-10,y+4.1f,.10f,n*.5f);b.Line(x-11,y,x+1,y,.45f,Face);b.Line(x-11,y+.3f,x+1,y+.3f,.09f,n*.7f);}
  // Rocket/service core with shoulder and pointed cap, visibly distinct from fuel tanks.
  b.Box(x-2,11.5f,3.7f,19,Face);b.Tri(new Vector2(x-3.85f,21),new Vector2(x-2,25),new Vector2(x-.15f,21),Face);b.Line(x-3.85f,3,x-3.85f,21,.09f,n*.7f);b.Box(x-2,6,3.8f,.4f,n*.6f,8.14f);
  for(int j=0;j<2;j++){float xx=x+7+j*7,hh=7+j*2;b.Box(xx,hh*.5f,5.4f,hh,Face);b.Disc(xx,hh,2.7f,1.8f,Face);b.Line(xx-2.5f,1,xx-2.5f,hh,.08f,n*.5f);b.Line(xx-2.6f,hh*.4f,xx+2.6f,hh*.4f,.09f,n*.45f);}
  b.Line(x+7,1,x+7,2,.3f,n*.4f);b.Line(x+7,1,x-6,1,.25f,n*.4f);g.Glow(x-11,26,2.4f,n*.8f);g.Glow(x+14,9,1.5f,n*.5f);
 }
 static void Orbit(Brush b,Brush g,float x,int k,Color n){
  float y=12+(k%3)*3;b.Box(x,y,5,9,Face);b.Disc(x,y+4.5f,2.5f,1,Face);b.Disc(x,y-4.5f,2.5f,1,Face);b.Line(x-2.5f,y-4,x-2.5f,y+4,.12f,n);b.Box(x,y,2.5f,1.6f,n*.5f,8.14f);g.Glow(x,y,3,n*.35f);
  foreach(int side in new[]{-1,1}){float xx=x+side*14;b.Line(x,y,xx,y,.5f,Face);b.Box(xx,y,15,7,Ink);b.Line(xx-7.5f,y-3.5f,xx+7.5f,y-3.5f,.10f,n*.65f);b.Line(xx-7.5f,y+3.5f,xx+7.5f,y+3.5f,.10f,n*.65f);for(int j=0;j<6;j++)b.Line(xx-7+j*2.8f,y-3.3f,xx-7+j*2.8f,y+3.3f,.06f,n*.35f);b.Line(xx-7.5f,y,xx+7.5f,y,.07f,n*.4f);}
  b.Line(x,y+4,x+2,y+8,.15f,Face);b.Disc(x+2,y+8,1.5f,.5f,n*.6f,8.15f);
 }
 void Finish(Brush b,string name,Material material){var mesh=new Mesh{name="Neon "+name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(b.v);mesh.SetColors(b.c);mesh.SetUVs(0,b.uv);mesh.SetTriangles(b.t,0);mesh.RecalculateBounds();meshes.Add(mesh);var go=new GameObject("Neon "+name);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
 void OnDestroy(){foreach(var mesh in meshes)NeonProto.Release(mesh);foreach(var material in materials)NeonProto.Release(material);}
}

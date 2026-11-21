using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Editor-only runtime evidence: intersect rendered meshes without adding physics components,
// then remove the actual hit renderer for one render to verify its pixel contribution.
public static class NeonBackdropPixelReview {
 public const int Width=1280,Height=800;
 [Serializable] public class Hit {
  public string path,root,mesh,material,shader,texture,colorSource;
  public int rendererId,submesh,triangle,vertexColors;
  public bool active,enabled,collider,body,breakable,propertyBlockColor;
  public Vector3 point;public Vector2 uv;public float distance;
  public Color materialColor,blockColor,vertexColor,withoutRenderer;public float removalDelta;
  [NonSerialized] public Renderer renderer;
 }
 [Serializable] public class Probe {public string sector;public int x,y;public Color pixel;public float luminance;public Hit[] hits;}
 [Serializable] class ProbeFile {public string coordinates="1280x800 PNG pixels, origin top-left; encoded RGB Rec.709 weighted sum";public Probe[] probes;public RootState[] roots;}
 [Serializable] class RootState {public string path;public bool activeSelf,activeInHierarchy;public int renderers,enabledActiveRenderers;}
 public static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 public static float Luminance(Color c)=>.2126f*c.r+.7152f*c.g+.0722f*c.b;
 public static Texture2D Render(Camera camera){
  var previous=camera.targetTexture;var active=RenderTexture.active;
  var rt=new RenderTexture(Width,Height,24);var im=new Texture2D(Width,Height,TextureFormat.RGB24,false);
  try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,Width,Height),0,0);im.Apply();return im;}
  catch{UnityEngine.Object.DestroyImmediate(im);throw;}
  finally{camera.targetTexture=previous;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
 }
 public static Color Pixel(Texture2D im,int x,int y)=>im.GetPixel(x,Height-1-y);
 [Serializable] public class PixelCheck {public string sector,view,role;public int x,y;public Color rgb;public float luminance;public bool pass;}
 [Serializable] public class SignCheck {public string sector,path;public Vector3 viewport;public float pixelContribution;public bool visible;}
 [Serializable] public class PixelReceipt {public string coordinates="1280x800 PNG, top-left; luminance=.2126R+.7152G+.0722B on encoded RGB";public float limit=.30f;public bool pass;public PixelCheck[] pixels;public SignCheck[] signs;public bool[] mallOutsideExact;}
 static Vector2Int[] Points(string id,bool reframed){
  switch(id){
   case "tank":return reframed?new[]{new Vector2Int(40,150),new Vector2Int(1150,150)}:new[]{new Vector2Int(1150,150),new Vector2Int(100,330)};
   // (400,60) intersects the existing aim line; (400,40) is the same background wall clear of it.
   case "restaurant":return new[]{new Vector2Int(400,40),new Vector2Int(980,90)};
   case "mall":return new[]{new Vector2Int(100,100),new Vector2Int(900,50)};
   case "office":return new[]{new Vector2Int(640,240),new Vector2Int(100,560)};
   case "viaduct":return new[]{new Vector2Int(450,300),new Vector2Int(1150,350)};
   case "launchrange":return new[]{new Vector2Int(100,100),new Vector2Int(1100,550)};
   default:return new[]{new Vector2Int(100,100),new Vector2Int(1150,150)};
  }
 }
 static void Sample(string id,string view,Texture2D im,List<PixelCheck> rows,bool reframed){
  foreach(var p in Points(id,reframed)){var color=Pixel(im,p.x,p.y);float l=Luminance(color);rows.Add(new PixelCheck{sector=id,view=view,role="fixed background surface or sky; actors/glows excluded",x=p.x,y=p.y,rgb=color,luminance=l,pass=l<.30f});}
 }
 public static PixelReceipt Verify(CrabFlightGame game,string dir){
  var camera=game.ReviewCamera;var pixels=new List<PixelCheck>();var signs=new List<SignCheck>();var outside=new List<bool>();
  camera.aspect=1.6f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
  foreach(var sector in WorldRouteR11.Data.sectors){
   camera.orthographicSize=sector.viewSize;camera.transform.position=new Vector3(sector.viewX,sector.viewY,-30);
   game.ReviewCrab.Root.position=new Vector3(sector.viewX-2,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   var original=Render(camera);try{
    File.WriteAllBytes(dir+"/sector-"+sector.id+"-original-view.png",original.EncodeToPNG());Sample(sector.id,"original",original,pixels,false);
    if(sector.id=="country"||sector.id=="office"||sector.id=="restaurant"){
     var points=sector.id=="restaurant"?new[]{new Vector2Int(400,60),new Vector2Int(400,40)}:Points(sector.id,false);
     File.WriteAllText(dir+"/probe-"+sector.id+".json",JsonUtility.ToJson(new ProbeFile{probes=points.Select(p=>Inspect(camera,sector.id,p.x,p.y,original)).ToArray()},true));
    }
   }finally{UnityEngine.Object.DestroyImmediate(original);}
   var candidates=game.transform.Find("Neon world signs render only").GetComponentsInChildren<MeshRenderer>().Where(r=>r.name.StartsWith("Neon sign "+sector.id+" ")).ToArray();
   // Keep accepted sector framing unless none of its signs fits. Country previously had all signs offscreen.
   bool fits=candidates.Any(r=>{var p=camera.WorldToViewportPoint(r.transform.position);return p.z>0&&p.x>.08f&&p.x<.92f&&p.y>.08f&&p.y<.92f;});
   if(!fits){var nearest=candidates.OrderBy(r=>Mathf.Abs(r.transform.position.x-sector.viewX)).First();var pos=camera.transform.position;pos.x=nearest.transform.position.x;var vp=camera.WorldToViewportPoint(nearest.transform.position);if(vp.y<.08f||vp.y>.92f)pos.y=nearest.transform.position.y+.18f*(nearest.transform.position.z-pos.z);camera.transform.position=pos;}
   game.ReviewCrab.Root.position=new Vector3(camera.transform.position.x-2,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   var on=Render(camera);Texture2D off=null;
   try{
    File.WriteAllBytes(dir+"/sector-"+sector.id+".png",on.EncodeToPNG());Sample(sector.id,"sign-view",on,pixels,!fits);
    foreach(var r in candidates)r.enabled=false;off=Render(camera);
    foreach(var r in candidates){var vp=camera.WorldToViewportPoint(r.transform.position);float delta=0;
     if(vp.z>0&&vp.x>.02f&&vp.x<.98f&&vp.y>.02f&&vp.y<.98f){int x=Mathf.RoundToInt(vp.x*Width),y=Mathf.RoundToInt((1-vp.y)*Height);for(int yy=Mathf.Max(0,y-8);yy<=Mathf.Min(Height-1,y+8);yy++)for(int xx=Mathf.Max(0,x-8);xx<=Mathf.Min(Width-1,x+8);xx++){var a=Pixel(on,xx,yy);var b=Pixel(off,xx,yy);delta=Mathf.Max(delta,Mathf.Max(a.r-b.r,Mathf.Max(a.g-b.g,a.b-b.b)));}}
     signs.Add(new SignCheck{sector=sector.id,path=PathOf(r.transform),viewport=vp,pixelContribution=delta,visible=delta>.01f});
    }
   }finally{foreach(var r in candidates)r.enabled=true;UnityEngine.Object.DestroyImmediate(on);if(off)UnityEngine.Object.DestroyImmediate(off);}
  }
  // Only toggle the mall bindings; other sectors intentionally stay dark in both images.
  foreach(float x in new[]{149f,207,650}){
   camera.orthographicSize=5;camera.transform.position=new Vector3(x,5,-30);game.ReviewCrab.Root.position=new Vector3(x-10,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   var on=Render(camera);Texture2D off=null;
   try{game.ReviewMallBackdropDark(false);off=Render(camera);var a=on.EncodeToPNG();var b=off.EncodeToPNG();File.WriteAllBytes(dir+"/mall-outside-"+x+"-on.png",a);File.WriteAllBytes(dir+"/mall-outside-"+x+"-off.png",b);outside.Add(a.SequenceEqual(b));}
   finally{game.ReviewMallBackdropDark(true);UnityEngine.Object.DestroyImmediate(on);if(off)UnityEngine.Object.DestroyImmediate(off);}
  }
  var receipt=new PixelReceipt{pixels=pixels.ToArray(),signs=signs.ToArray(),mallOutsideExact=outside.ToArray()};receipt.pass=pixels.All(p=>p.pass)&&outside.All(p=>p)&&WorldRouteR11.Data.sectors.All(s=>signs.Any(p=>p.sector==s.id&&p.visible));
  File.WriteAllText(dir+"/pixel-assertions.json",JsonUtility.ToJson(receipt,true));
  return receipt;
 }
 static bool Triangle(Ray ray,Vector3 a,Vector3 b,Vector3 c,out float distance,out Vector3 weights){
  distance=0;weights=Vector3.zero;var e1=b-a;var e2=c-a;var p=Vector3.Cross(ray.direction,e2);float d=Vector3.Dot(e1,p);
  if(Mathf.Abs(d)<1e-8f)return false;float inv=1/d;var t=ray.origin-a;float u=Vector3.Dot(t,p)*inv;if(u<0||u>1)return false;
  var q=Vector3.Cross(t,e1);float v=Vector3.Dot(ray.direction,q)*inv;if(v<0||u+v>1)return false;
  distance=Vector3.Dot(e2,q)*inv;weights=new Vector3(1-u-v,u,v);return distance>=0;
 }
 public static Probe Inspect(Camera camera,string sector,int x,int y,Texture2D im){
  var ray=camera.ViewportPointToRay(new Vector3((x+.5f)/Width,1-(y+.5f)/Height,0));var hits=new List<Hit>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)){
   if(!r.enabled||!r.gameObject.activeInHierarchy||(camera.cullingMask&(1<<r.gameObject.layer))==0||!r.bounds.IntersectRay(ray))continue;
   var mf=r.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh||!mf.sharedMesh.isReadable)continue;
   var mesh=mf.sharedMesh;var vertices=mesh.vertices;var uv=mesh.uv;var colors=mesh.colors;var materials=r.sharedMaterials;var matrix=r.localToWorldMatrix;
   var localRay=new Ray(matrix.inverse.MultiplyPoint3x4(ray.origin),matrix.inverse.MultiplyVector(ray.direction));
   Hit closest=null;
   for(int sub=0;sub<mesh.subMeshCount;sub++){
    var ids=mesh.GetTriangles(sub);
    for(int i=0;i<ids.Length;i+=3){
     int a=ids[i],b=ids[i+1],c=ids[i+2];
     if(!Triangle(localRay,vertices[a],vertices[b],vertices[c],out float localDistance,out Vector3 weights))continue;
     var point=matrix.MultiplyPoint3x4(localRay.GetPoint(localDistance));float distance=Vector3.Dot(point-ray.origin,ray.direction);if(closest!=null&&distance>=closest.distance)continue;
     var mat=materials[Mathf.Min(sub,materials.Length-1)];var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
     closest=new Hit{renderer=r,rendererId=r.GetInstanceID(),path=PathOf(r.transform),root=r.transform.root.name,mesh=mesh.name,submesh=sub,triangle=i/3,distance=distance,point=point,uv=uv.Length==vertices.Length?uv[a]*weights.x+uv[b]*weights.y+uv[c]*weights.z:Vector2.zero,vertexColor=colors.Length==vertices.Length?colors[a]*weights.x+colors[b]*weights.y+colors[c]*weights.z:Color.white,vertexColors=colors.Length,material=mat.name,shader=mat.shader.name,texture=mat.HasProperty("_MainTex")&&mat.mainTexture?mat.mainTexture.name:"none",materialColor=mat.HasProperty("_Color")?mat.color:Color.white,blockColor=block.GetColor("_Color"),propertyBlockColor=block.HasColor("_Color"),active=r.gameObject.activeInHierarchy,enabled=r.enabled,collider=r.GetComponentInParent<Collider>()||r.GetComponentsInChildren<Collider>(true).Length>0,body=r.GetComponentInParent<Rigidbody>()||r.GetComponentsInChildren<Rigidbody>(true).Length>0,breakable=r.GetComponentInParent<CrabCityDensityR2.CityVisualBreakable>()};
     closest.colorSource=(closest.propertyBlockColor?"MaterialPropertyBlock _Color overrides material":"material _Color")+"; texture="+closest.texture+"; mesh color count="+colors.Length+"; shader="+closest.shader;
    }
   }
   if(closest!=null)hits.Add(closest);
  }
  var probe=new Probe{sector=sector,x=x,y=y,pixel=Pixel(im,x,y),hits=hits.OrderBy(h=>h.distance).Take(5).ToArray()};probe.luminance=Luminance(probe.pixel);
  foreach(var h in probe.hits.Take(2)){
   h.renderer.enabled=false;Texture2D removed=null;
   try{removed=Render(camera);h.withoutRenderer=Pixel(removed,x,y);h.removalDelta=Mathf.Max(Mathf.Abs(h.withoutRenderer.r-probe.pixel.r),Mathf.Abs(h.withoutRenderer.g-probe.pixel.g),Mathf.Abs(h.withoutRenderer.b-probe.pixel.b));}
   finally{h.renderer.enabled=true;if(removed)UnityEngine.Object.DestroyImmediate(removed);}
  }
  // Lines have no MeshFilter; use the actual rendered difference instead of guessing from a mesh ray.
  var lineHits=new List<Hit>();
  foreach(var r in UnityEngine.Object.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy)){
   r.enabled=false;Texture2D removed=null;
   try{removed=Render(camera);var c=Pixel(removed,x,y);float delta=Mathf.Max(Mathf.Abs(c.r-probe.pixel.r),Mathf.Abs(c.g-probe.pixel.g),Mathf.Abs(c.b-probe.pixel.b));
    if(delta>.001f)lineHits.Add(new Hit{path=PathOf(r.transform),root=r.transform.root.name,rendererId=r.GetInstanceID(),shader=r.sharedMaterial.shader.name,material=r.sharedMaterial.name,colorSource="LineRenderer material/startColor/endColor; verified by renderer removal",materialColor=r.sharedMaterial.color,withoutRenderer=c,removalDelta=delta,active=true,enabled=true});
   }finally{r.enabled=true;if(removed)UnityEngine.Object.DestroyImmediate(removed);}
  }
  probe.hits=probe.hits.Concat(lineHits).ToArray();
  return probe;
 }
 public static void Diagnose(){
  string dir=Path.GetFullPath("../../../../evidence/neon-background-probe04");if(Directory.Exists(dir))throw new Exception("Use fresh probe evidence");Directory.CreateDirectory(dir);
  NeonProto.Enabled=MallDensityProto.Enabled=true;UnityEngine.Random.InitState(42043);var game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
  try{
   var camera=game.ReviewCamera;camera.aspect=1.6f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));var probes=new List<Probe>();
   foreach(string id in new[]{"country","office","tank","restaurant","port","viaduct","launchrange","orbit","mall"}){
    var s=WorldRouteR11.Data.sectors.Single(s=>s.id==id);camera.orthographicSize=s.viewSize;camera.transform.position=new Vector3(s.viewX,s.viewY,-30);game.ReviewCrab.Root.position=new Vector3(s.viewX-2,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
    var im=Render(camera);try{
     File.WriteAllBytes(dir+"/sector-"+id+".png",im.EncodeToPNG());
     var points=id=="country"?new[]{new Vector2Int(100,100),new Vector2Int(1150,150),new Vector2Int(100,650)}:id=="office"?new[]{new Vector2Int(640,240),new Vector2Int(100,560),new Vector2Int(640,100)}:new[]{new Vector2Int(100,100),new Vector2Int(1150,150),new Vector2Int(100,650)};
     foreach(var p in points)probes.Add(Inspect(camera,id,p.x,p.y,im));
    }finally{UnityEngine.Object.DestroyImmediate(im);}
   }
   var roots=UnityEngine.Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,FindObjectsSortMode.None).Where(t=>new[]{"R11 all-section graybox",LateWorldArtR9.Revision,"Restaurant authored environment - visuals only","City2 broad district ground"}.Contains(t.name)).Select(t=>new RootState{path=PathOf(t),activeSelf=t.gameObject.activeSelf,activeInHierarchy=t.gameObject.activeInHierarchy,renderers=t.GetComponentsInChildren<Renderer>(true).Length,enabledActiveRenderers=t.GetComponentsInChildren<Renderer>(true).Count(r=>r.enabled&&r.gameObject.activeInHierarchy)}).ToArray();
   File.WriteAllText(dir+"/runtime-pixel-probe.json",JsonUtility.ToJson(new ProbeFile{probes=probes.ToArray(),roots=roots},true));
  }finally{game.ShutdownReviewPhysics();}
 }
 public static void DiagnoseSample(){
  string dir=Path.GetFullPath("../../../../evidence/neon-background-probe05");if(Directory.Exists(dir))throw new Exception("Use fresh probe evidence");Directory.CreateDirectory(dir);
  NeonProto.Enabled=MallDensityProto.Enabled=true;UnityEngine.Random.InitState(42043);var game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
  try{
   var camera=game.ReviewCamera;camera.aspect=1.6f;camera.orthographicSize=10;camera.transform.position=new Vector3(16,5,-30);camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));game.ReviewCrab.Root.position=new Vector3(14,3.3f,0);game.ReviewSampleNeon(0,"ready",0,Vector3.zero);
   var im=Render(camera);try{File.WriteAllBytes(dir+"/restaurant.png",im.EncodeToPNG());var probes=new[]{Inspect(camera,"restaurant",400,60,im),Inspect(camera,"restaurant",400,40,im)};File.WriteAllText(dir+"/runtime-pixel-probe.json",JsonUtility.ToJson(new ProbeFile{probes=probes},true));}finally{UnityEngine.Object.DestroyImmediate(im);}
  }finally{game.ShutdownReviewPhysics();}
 }
}

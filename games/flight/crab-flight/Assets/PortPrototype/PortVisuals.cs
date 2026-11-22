using System;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

public static class PortVisuals {
 public static readonly Color Ink=new Color(.141176f,.117647f,.180392f),Lantern=new Color(1,.70196f,.27843f);
 public static void Apply(GameObject root){
  if(!PortLayout.Enabled)return;var owner=root.GetComponent<C2.CityDensityOwnedAssets>();if(!owner)return;
  var shader=Resources.Load<Shader>("PortPrototype/Surface");if(!shader)throw new InvalidOperationException("Port surface shader missing");
  var lamps=PortLayout.Data.instances.Where(e=>e.asset=="market_stall"||e.role=="landmark"||e.role.StartsWith("boost",StringComparison.Ordinal)||e.role=="jackpot"||e.role=="hazard").ToArray();
  foreach(var v in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   var e=PortLayout.Find(v.placementId);if(e==null)continue;
   foreach(var r in v.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled)){
    var f=r.GetComponent<MeshFilter>();if(!f||!f.sharedMesh)continue;
    var lamp=lamps.OrderBy(l=>Vector3.SqrMagnitude(City2Data.V(l.p)-v.transform.position)).First();
    var m=new Material(shader){name="Port ink / "+e.asset};m.SetColor("_Family",PortLayout.FamilyColor(e.asset));var b=f.sharedMesh.bounds;
    m.SetVector("_Span",new Vector4(b.min.x,b.size.x,0,0));m.SetVector("_Lamp",new Vector4(lamp.p[0],3,lamp.p[2],1));
    r.sharedMaterials=Enumerable.Repeat(m,f.sharedMesh.subMeshCount).ToArray();owner.materials.Add(m);
   }
   var asset=City2Data.Asset(e.asset);var bounds=City2Data.Bounds(asset.pieces[0]);foreach(var p in asset.pieces.Skip(1))bounds.Encapsulate(City2Data.Bounds(p));
   var shadow=Disc("Port contact shadow",v.intact.transform,new Vector3(bounds.center.x,bounds.min.y+.006f,bounds.center.z),new Vector3(bounds.size.x*.4f,1,bounds.size.z*.4f),new Color(.10f,.12f,.20f));owner.meshes.Add(shadow.GetComponent<MeshFilter>().sharedMesh);owner.materials.Add(shadow.GetComponent<Renderer>().sharedMaterial);
   if(e.asset=="market_stall"||e.role.Length>0&&e.role!="resource"&&e.role!="preview"){
    // A real hanging lantern is the source of every halo/reflection in this district.
    var pole=Box("Port lantern bracket",v.intact.transform,new Vector3(bounds.min.x+.18f,bounds.max.y+.22f,bounds.min.z),new Vector3(.08f,.46f,.08f),Ink);
    var lamp=Box("Port lantern source",v.intact.transform,new Vector3(bounds.min.x+.18f,bounds.max.y+.05f,bounds.min.z-.17f),new Vector3(.25f,.32f,.25f),Lantern);
    Own(pole,owner);Own(lamp,owner);
    var glow=NeonGlowTexture.Quad("Port lantern halo",v.intact.transform,Lantern);glow.transform.localPosition=lamp.transform.localPosition;glow.transform.localScale=Vector3.one*.75f;owner.materials.Add(glow.sharedMaterial);
    var reflection=Disc("Port lantern reflection",v.intact.transform,new Vector3(lamp.transform.localPosition.x,bounds.min.y+.009f,lamp.transform.localPosition.z),new Vector3(.30f,1,.65f),Lantern*.28f);Own(reflection,owner);
    if(e.role.Length>0){
     var at=new Vector3(bounds.min.x+.18f,bounds.max.y+.40f,bounds.min.z-.22f);
     var tag=Box("Port role tag "+e.role,v.intact.transform,at,new Vector3(.62f,.40f,.035f),new Color(1,.953f,.839f));Own(tag,owner);
     if(e.role=="hazard"){
      Own(Box("Port caution stroke",v.intact.transform,at+new Vector3(0,.05f,-.023f),new Vector3(.045f,.17f,.012f),Ink),owner);
      Own(Box("Port caution dot",v.intact.transform,at+new Vector3(0,-.09f,-.023f),new Vector3(.045f,.045f,.012f),Ink),owner);
     }else{
      // Angle is a shape cue as well as a color cue. Jackpot uses a double arrow.
      int arrows=e.role=="jackpot"?2:1;
      float a=(e.role=="boost60"?60:e.role=="boost30"||e.role=="redirect30"?30:45)*Mathf.Deg2Rad;
      for(int i=0;i<arrows;i++){
       var arrow=MeshObject("Port role arrow",v.intact.transform,at+new Vector3(i*.12f-.06f*(arrows-1),0,-.025f),Vector3.one,new[]{new Vector3(-.13f,-.03f,0),new Vector3(.03f,-.03f,0),new Vector3(.03f,-.09f,0),new Vector3(.16f,0,0),new Vector3(.03f,.09f,0),new Vector3(.03f,.03f,0),new Vector3(-.13f,.03f,0)},new[]{0,1,5,0,5,6,2,3,4},Ink);
       arrow.transform.localRotation=Quaternion.Euler(0,0,a*Mathf.Rad2Deg);Own(arrow,owner);
      }
     }
    }
   }
   // Three visual variants: delivery label on a different face/height, no physical transform changes.
   var label=Box("Port manifest label "+e.variant,v.intact.transform,new Vector3(bounds.center.x+(e.variant-1)*bounds.size.x*.22f,bounds.min.y+bounds.size.y*(.30f+e.variant*.16f),bounds.min.z-.012f),new Vector3(.22f,.16f,.018f),new Color(1,.953f,.839f));Own(label,owner);
  }
 }
 static void Own(GameObject o,C2.CityDensityOwnedAssets owner){owner.meshes.Add(o.GetComponent<MeshFilter>().sharedMesh);owner.materials.Add(o.GetComponent<Renderer>().sharedMaterial);}
 public static GameObject Box(string name,Transform parent,Vector3 p,Vector3 s,Color color){
  var v=new[]{new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.5f,-.5f),new Vector3(-.5f,.5f,-.5f),new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.5f,.5f,.5f),new Vector3(-.5f,.5f,.5f)};
  return MeshObject(name,parent,p,s,v,new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5},color);
 }
 public static GameObject Disc(string name,Transform parent,Vector3 p,Vector3 s,Color color){
  var vertices=new Vector3[25];var triangles=new int[72];for(int i=0;i<24;i++){float a=i*Mathf.PI/12;vertices[i+1]=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=(i+1)%24+1;}return MeshObject(name,parent,p,s,vertices,triangles,color);
 }
 public static GameObject MeshObject(string name,Transform parent,Vector3 p,Vector3 s,Vector3[] vertices,int[] triangles,Color color){
  var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=p;go.transform.localScale=s;
  var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.triangles=triangles;mesh.colors=Enumerable.Repeat(color,vertices.Length).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=new Material(Resources.Load<Shader>("NeonProto/NeonScenery"));r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;return go;
 }
}

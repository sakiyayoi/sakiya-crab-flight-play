using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Grounded scenery only: no Collider, Rigidbody, contact registration or energy.
// The generator uses these exact box tops/footprints for supported placements.
public sealed class NeonPlacementSupports : MonoBehaviour {
 [Serializable] public sealed class Support {
  public string id,target; public float[] p,size; public float top,bottom;
  public bool breakable,collision;
 }
 [Serializable] public sealed class Sign { public float[] p,size; public string support; }
 [Serializable] public sealed class Layout { public string revision; public Support[] supports; public Sign orbitSign0; }
 public Layout Data { get; private set; }
 Mesh mesh;
 Material material;

 public static NeonPlacementSupports Build(Transform parent) {
  var asset=Resources.Load<TextAsset>("NeonDensity/Supports");
  if(!asset)throw new InvalidOperationException("Neon placement supports missing; regenerate density layout");
  var layout=JsonUtility.FromJson<Layout>(asset.text);
  var go=new GameObject("Neon grounded placement supports render only");
  go.transform.SetParent(parent,false);
  var result=go.AddComponent<NeonPlacementSupports>();result.Data=layout;result.BuildMesh();
  return result;
 }
 public static Vector3 Vector(float[] v)=>new Vector3(v[0],v[1],v[2]);
 void BuildMesh() {
  var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
  var faces=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
  foreach(var s in Data.supports) {
   if(PortLayout.Find(s.target)!=null)continue;
   if(!NeonDensity.Enabled&&s.target!="Neon sign orbit 0")continue;
   if(s.p==null||s.size==null||s.p.Length!=3||s.size.Length!=3||s.breakable||s.collision
      ||s.size[0]<=0||s.size[1]<=0||s.size[2]<=0||Mathf.Abs(s.p[1]-s.size[1]/2)>0.00001f
      ||Mathf.Abs(s.p[1]+s.size[1]/2-s.top)>0.00001f)
    throw new InvalidOperationException("Invalid grounded placement support: "+s.id);
   int first=vertices.Count;var p=Vector(s.p);var half=Vector(s.size)*.5f;
   foreach(var v in new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),
                         new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)}) {
    vertices.Add(p+Vector3.Scale(v,half));
    colors.Add(v.y>0?new Color(.07f,.14f,.20f):new Color(.024f,.038f,.065f));
   }
   foreach(int i in faces)triangles.Add(first+i);
  }
  mesh=new Mesh{name="Neon grounded support boxes"};mesh.SetVertices(vertices);mesh.SetColors(colors);
  mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
  var shader=Resources.Load<Shader>("NeonProto/NeonScenery");
  if(!shader)throw new InvalidOperationException("Neon support scenery shader missing");
  material=new Material(shader){name="Neon grounded support ink"};
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
  var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
  renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
 }
 void OnDestroy() { if(mesh)NeonProto.Release(mesh);if(material)NeonProto.Release(material); }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

// Fixed grounded scenery. No colliders, rigidbodies, timers, lights or pulses.
public sealed class OpeningLandmarks : MonoBehaviour {
 Mesh mesh;Material material;
 public static void Build(Transform parent){
  var root=new GameObject("Opening boundary landmarks (render only)");
  root.transform.SetParent(parent,false);
  root.AddComponent<OpeningLandmarks>().BuildMesh();
 }
 void BuildMesh(){
  var vertices=new List<Vector3>();var colors=new List<Color>();var triangles=new List<int>();
  var faces=new[]{0,2,1,0,3,2,4,5,6,4,6,7,0,1,5,0,5,4,3,7,6,3,6,2,0,4,7,0,7,3,1,2,6,1,6,5};
  foreach(var b in OpeningLayout.Data.landmarks.SelectMany(sign=>sign.boxes).Concat(OpeningLayout.Data.supports)){
   if(b.p==null||b.p.Length!=3||b.size==null||b.size.Length!=3||b.color==null||b.color.Length!=3
      ||b.size[0]<=0||b.size[1]<=0||b.size[2]<=0||Mathf.Abs(b.p[0])+b.size[0]/2>=135)
    throw new InvalidOperationException("Invalid opening scenery: "+b.key);
   int first=vertices.Count;var p=Vector(b.p);var half=Vector(b.size)*.5f;
   foreach(var corner in new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),
                               new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)}){
    vertices.Add(p+Vector3.Scale(corner,half));colors.Add(new Color(b.color[0],b.color[1],b.color[2]));
   }
   foreach(int index in faces)triangles.Add(first+index);
  }
  mesh=new Mesh{name="Opening grounded sign assemblies"};
  mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
  var shader=Resources.Load<Shader>("NeonProto/NeonScenery");
  if(!shader)throw new InvalidOperationException("Opening scenery shader missing");
  material=new Material(shader){name="Opening steady lamp / port ink"};
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
  var renderer=gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
  renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
 }
 static Vector3 Vector(float[] v)=>new Vector3(v[0],v[1],v[2]);
 void OnDestroy(){if(mesh)NeonProto.Release(mesh);if(material)NeonProto.Release(material);}
}

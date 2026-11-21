using System;using System.Collections.Generic;using UnityEngine;
namespace CrabLifeActors {
// Original geometry. One skinned draw per actor; no collider or physics side effects.
public sealed class LifeActorMesh {
 readonly Transform root;readonly Transform[] bones;readonly List<Vector3> v=new List<Vector3>();readonly List<Color> c=new List<Color>();readonly List<int> t=new List<int>();readonly List<BoneWeight> weights=new List<BoneWeight>();
 public LifeActorMesh(Transform root,Transform[] bones){this.root=root;this.bones=bones;}
 void Vertex(int b,Vector3 p,Color color){v.Add(root.InverseTransformPoint(bones[b].TransformPoint(p)));c.Add(color);weights.Add(new BoneWeight{boneIndex0=b,weight0=1});}
 public void Ell(int bone,Vector3 p,Vector3 size,Color color,int sectors=12,int rings=8){int start=v.Count;for(int y=0;y<=rings;y++){float a=Mathf.PI*y/rings;for(int x=0;x<=sectors;x++){float b=Mathf.PI*2*x/sectors;Vertex(bone,p+Vector3.Scale(size,new Vector3(Mathf.Sin(a)*Mathf.Cos(b),Mathf.Cos(a),Mathf.Sin(a)*Mathf.Sin(b))),color);}}for(int y=0;y<rings;y++)for(int x=0;x<sectors;x++){int a=start+y*(sectors+1)+x,b=a+sectors+1;t.AddRange(new[]{a,a+1,b,a+1,b+1,b});}}
 public void Box(int bone,Vector3 p,Vector3 size,Color color){int start=v.Count;foreach(var s in new[]{new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)})Vertex(bone,p+Vector3.Scale(s,size*.5f),color);int[] f={0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,3,7,6,3,6,2,0,1,5,0,5,4};foreach(int i in f)t.Add(start+i);}
 public void Poly(int bone,Vector3[] p,Color color){int start=v.Count;foreach(var q in p)Vertex(bone,q,color);foreach(var q in p)Vertex(bone,q,color);for(int k=1;k<p.Length-1;k++)t.AddRange(new[]{start,start+k,start+k+1,start+p.Length,start+p.Length+k+1,start+p.Length+k});}
 public Mesh Finish(){var mesh=new Mesh{name="LifeActor original skinned geometry"};mesh.SetVertices(v);mesh.SetColors(c);mesh.SetTriangles(t,0);mesh.boneWeights=weights.ToArray();var bind=new Matrix4x4[bones.Length];for(int i=0;i<bind.Length;i++)bind[i]=bones[i].worldToLocalMatrix*root.localToWorldMatrix;mesh.bindposes=bind;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;}
}
}

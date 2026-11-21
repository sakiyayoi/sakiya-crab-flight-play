using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class RigidCrabFlight {
 public float LaunchEnvelopeError,LaunchBoundsDifferenceR30;
 bool launchCompoundFrozen;public bool FrozenCompoundApplied=>launchCompoundFrozen;
 void FreezeLaunchCompound(){
  if(launchCompoundFrozen)return;
  UnityEngine.Physics.SyncTransforms();var before=new Dictionary<Collider,Bounds>();var vertexBefore=new Dictionary<Collider,Vector3[]>();foreach(var c in colliders){before[c]=c.bounds;if(c is MeshCollider mesh){var v=mesh.sharedMesh.vertices;for(int k=0;k<v.Length;k++)v[k]=mesh.transform.TransformPoint(v[k]);vertexBefore[c]=v;}}
  foreach(var c in colliders){
   // Derive root-local data from authored local transforms only. The previous
   // ready-state world rotation is deliberately absent from this calculation.
   var chain=new List<Transform>();for(var t=c.transform;t!=root;t=t.parent){if(!t)throw new InvalidOperationException("Collider lost body root");chain.Add(t);}
   Matrix4x4 matrix=Matrix4x4.identity;Quaternion rotation=Quaternion.identity;
   for(int i=chain.Count-1;i>=0;i--){var t=chain[i];matrix=matrix*Matrix4x4.TRS(t.localPosition,t.localRotation,t.localScale);rotation=rotation*t.localRotation;}
   Vector3 scale=matrix.lossyScale;scale=new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));Vector3 center;
   if(c is BoxCollider box){center=matrix.MultiplyPoint3x4(box.center);box.center=Vector3.zero;box.size=Vector3.Scale(box.size,scale);}
   else if(c is CapsuleCollider cap){center=matrix.MultiplyPoint3x4(cap.center);float along=cap.direction==0?scale.x:cap.direction==1?scale.y:scale.z;float across=cap.direction==0?Mathf.Max(scale.y,scale.z):cap.direction==1?Mathf.Max(scale.x,scale.z):Mathf.Max(scale.x,scale.y);cap.center=Vector3.zero;cap.radius*=across;cap.height=Mathf.Max(cap.height*along,cap.radius*2);}
   else if(c is MeshCollider hull){
    // Full authored affine transform: decomposing nonuniform scale + limb rotations loses shear.
    // Baking into root coordinates also avoids a second transform approximation in PhysX.
    center=Vector3.zero;rotation=Quaternion.identity;var mesh=hull.sharedMesh;var v=mesh.vertices;for(int k=0;k<v.Length;k++)v[k]=matrix.MultiplyPoint3x4(v[k]);mesh.vertices=v;mesh.RecalculateBounds();hull.sharedMesh=mesh;
   }
   else throw new InvalidOperationException("Unexpected compound primitive");
   c.transform.SetParent(root,false);c.transform.localPosition=center;c.transform.localRotation=rotation;c.transform.localScale=Vector3.one;
  }
  UnityEngine.Physics.SyncTransforms();
  foreach(var c in colliders){var a=before[c];var b=c.bounds;float boxDifference=Mathf.Max((a.center-b.center).magnitude,(a.extents-b.extents).magnitude);LaunchBoundsDifferenceR30=Mathf.Max(LaunchBoundsDifferenceR30,boxDifference);
   // Cooked AABBs can quantize by fractions of a millimetre when mesh coordinates change.
   // Verify the unchanged0.1mm authored-geometry bound against every transformed vertex.
   if(c is MeshCollider hull){var points=hull.sharedMesh.vertices;for(int k=0;k<points.Length;k++)LaunchEnvelopeError=Mathf.Max(LaunchEnvelopeError,Vector3.Distance(vertexBefore[c][k],hull.transform.TransformPoint(points[k])));}
   else LaunchEnvelopeError=Mathf.Max(LaunchEnvelopeError,boxDifference);
  }
  if(LaunchEnvelopeError>.0001f)throw new InvalidOperationException("Frozen compound envelope changed beyond0.1mm: "+LaunchEnvelopeError.ToString("R"));
  hullVerticesR21.Clear();launchCompoundFrozen=true;
 }
}

using System;using System.Collections.Generic;using UnityEngine;
// Snapshot immutable collider dimensions after FreezeLaunchCompound. World pose/scale is always read afresh.
public sealed partial class RigidCrabFlight {
 sealed class SupportGeometryR14{public Transform transform;public Vector3 center,halfSize;public int direction;public float radius,height;public bool box;public Vector3[] points;}
 readonly Dictionary<Collider,SupportGeometryR14> supportGeometryR14=new Dictionary<Collider,SupportGeometryR14>();
 public int SupportGeometryEntriesR14=>supportGeometryR14.Count;public int SupportGeometryReadsR14{get;private set;}
#if UNITY_EDITOR
 public bool ReviewBypassSupportCacheR14,ReviewValidateSupportCacheR14;public int ReviewSupportChecksR14;
 [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Explicit)]struct SupportFloatBitsR14{[System.Runtime.InteropServices.FieldOffset(0)]public float value;[System.Runtime.InteropServices.FieldOffset(0)]public int bits;}
 static int SupportBitsR14(float v)=>new SupportFloatBitsR14{value=v}.bits;
#endif
 void ResetSupportGeometryR14(){
  supportGeometryR14.Clear();SupportGeometryReadsR14=0;
#if UNITY_EDITOR
  ReviewSupportChecksR14=0;
#endif
  if(!launchCompoundFrozen)return;
  foreach(var shape in colliders){
   if(shape is BoxCollider b)supportGeometryR14.Add(shape,new SupportGeometryR14{transform=b.transform,box=true,center=b.center,halfSize=b.size*.5f});
   else if(shape is CapsuleCollider c)supportGeometryR14.Add(shape,new SupportGeometryR14{transform=c.transform,center=c.center,direction=c.direction,radius=c.radius,height=c.height});
   else if(shape is MeshCollider m){var mesh=m.sharedMesh;if(!hullVerticesR21.TryGetValue(mesh,out var vertices)){vertices=mesh.vertices;hullVerticesR21.Add(mesh,vertices);}supportGeometryR14.Add(shape,new SupportGeometryR14{transform=m.transform,points=vertices});}
  }
 }
 Vector3 GeometricSupportPointRawCalmR3(Collider shape){
#if UNITY_EDITOR
  if(ReviewBypassSupportCacheR14)return GeometricSupportPointLegacyR14(shape);
#endif
  if(!supportGeometryR14.TryGetValue(shape,out var g))return GeometricSupportPointLegacyR14(shape);
  SupportGeometryReadsR14++;var t=g.transform;Vector3 point;
  if(g.points!=null){var matrix=t.localToWorldMatrix;point=Vector3.up*float.PositiveInfinity;foreach(var v in g.points){var q=matrix.MultiplyPoint3x4(v);if(q.y<point.y)point=q;}}
  else if(g.box){var q=t.TransformPoint(g.center);var e=g.halfSize;var ax=t.TransformVector(new Vector3(e.x,0,0));var ay=t.TransformVector(new Vector3(0,e.y,0));var az=t.TransformVector(new Vector3(0,0,e.z));q+=ax.y>=0?-ax:ax;q+=ay.y>=0?-ay:ay;q+=az.y>=0?-az:az;point=q;}
  else{var scale=t.lossyScale;scale=new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));int i=g.direction;float along=i==0?scale.x:i==1?scale.y:scale.z,across=i==0?Mathf.Max(scale.y,scale.z):i==1?Mathf.Max(scale.x,scale.z):Mathf.Max(scale.x,scale.y);float radius=g.radius*across;var axis=t.rotation*(i==0?Vector3.right:i==1?Vector3.up:Vector3.forward);point=t.TransformPoint(g.center)-axis*(axis.y>=0?1:-1)*Mathf.Max(0,g.height*along*.5f-radius)-Vector3.up*radius;}
#if UNITY_EDITOR
  if(ReviewValidateSupportCacheR14){var old=GeometricSupportPointLegacyR14(shape);ReviewSupportChecksR14++;if(SupportBitsR14(old.x)!=SupportBitsR14(point.x)||SupportBitsR14(old.y)!=SupportBitsR14(point.y)||SupportBitsR14(old.z)!=SupportBitsR14(point.z))throw new InvalidOperationException("Support cache bit mismatch: "+shape.name);}
#endif
  return point;
 }
}

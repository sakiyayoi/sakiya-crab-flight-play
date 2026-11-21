using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class FragmentVisibilityState {
 public int active,peak,materials,rayCandidates,triangleTests,budgetExhaustions,missingMeshes;
 public float minOpacity=1,maxQueryMs;public string[] blockers=new string[0];
}
// Presentation only. Trace authored opaque triangles toward shell/claw focus points.
// Only already-broken metal (and r5 wood) in the fragment lists is eligible; no collider/pose/camera mutation.
public sealed partial class RigidCrabFlight {
 public bool UseFragmentVisibility=true;public Shader FragmentFadeShader;
 public readonly FragmentVisibilityState Visibility=new FragmentVisibilityState();
 sealed class VisibilityRecord {public Renderer renderer;public Mesh mesh;public Material[] original,faded;public int[] opaqueTriangles;public string id;public float alpha=1,lastBlocked=-10;public bool assigned;}
 sealed class VisibilityMesh {public Vector3[] vertices;public int[] triangles;}
 readonly Dictionary<Renderer,VisibilityRecord> visibilityRecords=new Dictionary<Renderer,VisibilityRecord>();
 readonly Dictionary<Material,Material> visibilityMaterials=new Dictionary<Material,Material>();
 readonly Dictionary<Mesh,VisibilityMesh> visibilityMeshes=new Dictionary<Mesh,VisibilityMesh>();
 readonly HashSet<Renderer> visibilityLive=new HashSet<Renderer>();
 readonly List<Vector3> visibilityFocus=new List<Vector3>(15);
 readonly MaterialPropertyBlock visibilityBlock=new MaterialPropertyBlock();
 float visibilityClock,visibilityNextQuery;
#if UNITY_EDITOR
 public bool ReviewVisibilityPhysicsUnchanged=true;
#endif
 bool OpaqueFadeMaterial(Material m)=>m&&m.renderQueue<3000&&m.GetTag("RenderType",false)!="Transparent";
 Material FadeMaterial(Material original){Material m;if(visibilityMaterials.TryGetValue(original,out m))return m;m=new Material(FragmentFadeShader);m.CopyPropertiesFromMaterial(original);m.shader=FragmentFadeShader;m.renderQueue=3000;m.name=original.name+" broken-metal visibility";visibilityMaterials.Add(original,m);return m;}
 void RestoreRecord(VisibilityRecord r){if(!r.renderer)return;if(r.assigned)r.renderer.sharedMaterials=r.original;r.assigned=false;r.renderer.GetPropertyBlock(visibilityBlock);visibilityBlock.SetFloat("_FragmentOpacity",1);r.renderer.SetPropertyBlock(visibilityBlock);}
 public void ClearFragmentVisibility(){foreach(var r in visibilityRecords.Values)RestoreRecord(r);visibilityRecords.Clear();foreach(var m in visibilityMaterials.Values)Release(m);visibilityMaterials.Clear();visibilityMeshes.Clear();visibilityLive.Clear();visibilityClock=visibilityNextQuery=0;Visibility.active=Visibility.peak=Visibility.materials=Visibility.rayCandidates=Visibility.triangleTests=Visibility.budgetExhaustions=Visibility.missingMeshes=0;Visibility.minOpacity=1;Visibility.maxQueryMs=0;Visibility.blockers=new string[0];}
 public static bool ReviewRayTriangle(Vector3 origin,Vector3 direction,Vector3 a,Vector3 b,Vector3 c,float limit){var e1=b-a;var e2=c-a;var p=Vector3.Cross(direction,e2);float det=Vector3.Dot(e1,p);if(det<=.0000001f)return false;float inv=1/det;var t=origin-a;float u=Vector3.Dot(t,p)*inv;if(u<0||u>1)return false;var q=Vector3.Cross(t,e1);float v=Vector3.Dot(direction,q)*inv;if(v<0||u+v>1)return false;float distance=Vector3.Dot(e2,q)*inv;return distance>.0001f&&distance<limit;}
 bool MeshBlocks(VisibilityRecord item,Ray ray,float distance){
  float near;if(!item.renderer.bounds.IntersectRay(ray,out near)||near>=distance-.05f)return false;
  Visibility.rayCandidates++;VisibilityMesh data;
  if(!visibilityMeshes.TryGetValue(item.mesh,out data)){if(!item.mesh.isReadable){Visibility.missingMeshes++;return false;}data=new VisibilityMesh{vertices=item.mesh.vertices,triangles=item.mesh.triangles};visibilityMeshes.Add(item.mesh,data);}
  // Keep the local direction unnormalized so the parameter remains world-ray distance under scale.
  var tr=item.renderer.transform;var origin=tr.InverseTransformPoint(ray.origin);var direction=tr.InverseTransformVector(ray.direction);var vs=data.vertices;var ts=item.opaqueTriangles??data.triangles;
  for(int i=0;i+2<ts.Length;i+=3){if(Visibility.triangleTests>=30000)return false;Visibility.triangleTests++;if(ReviewRayTriangle(origin,direction,vs[ts[i]],vs[ts[i+1]],vs[ts[i+2]],distance-.05f))return true;}
  return false;
 }
 public void TickFragmentVisibility(Camera camera,CrabModel actor,float dt,bool snap=false){
  if(!camera||actor==null||!actor.Root||!FragmentFadeShader)return;
#if UNITY_EDITOR
  var beforePosition=Body?Body.position:Vector3.zero;var beforeRotation=Body?Body.rotation:Quaternion.identity;var beforeVelocity=Body?Body.linearVelocity:Vector3.zero;var beforeOmega=Body?Body.angularVelocity:Vector3.zero;
#endif
  visibilityClock+=Mathf.Max(0,dt);visibilityLive.Clear();
  // Register even in the OFF comparison: identical material allocation order, never new physics actors.
  foreach(var obj in fragments.Concat(cityDecor.Select(x=>x.obj))){if(!obj||!obj.activeInHierarchy)continue;var role=obj.GetComponent<FragmentMaterialRole>();if(!role||(role.materialType!="metal"&&!(UseOverdrive&&role.materialType=="wood")))continue;
   foreach(var renderer in obj.GetComponentsInChildren<MeshRenderer>()){if(!renderer.enabled)continue;var mf=renderer.GetComponent<MeshFilter>();if(!mf||!mf.sharedMesh)continue;visibilityLive.Add(renderer);if(!visibilityRecords.ContainsKey(renderer)){var original=renderer.sharedMaterials;if(!original.Any(OpaqueFadeMaterial))continue;int[] opaque=null;if(mf.sharedMesh.isReadable&&original.Any(m=>!OpaqueFadeMaterial(m))){var tris=new List<int>();for(int sub=0;sub<mf.sharedMesh.subMeshCount;sub++)if(OpaqueFadeMaterial(original[Mathf.Min(sub,original.Length-1)]))tris.AddRange(mf.sharedMesh.GetTriangles(sub));opaque=tris.ToArray();}visibilityRecords.Add(renderer,new VisibilityRecord{renderer=renderer,mesh=mf.sharedMesh,opaqueTriangles=opaque,original=original,faded=original.Select(m=>OpaqueFadeMaterial(m)?FadeMaterial(m):m).ToArray(),id=obj.name});}}
  }
  foreach(var key in visibilityRecords.Keys.Where(x=>!x||!visibilityLive.Contains(x)).ToArray()){RestoreRecord(visibilityRecords[key]);visibilityRecords.Remove(key);}
  bool query=UseFragmentVisibility&&(snap||visibilityClock>=visibilityNextQuery);
  if(query){long start=System.Diagnostics.Stopwatch.GetTimestamp();visibilityNextQuery=visibilityClock+1f/30;Visibility.rayCandidates=Visibility.triangleTests=0;visibilityFocus.Clear();
   foreach(var part in actor.CollisionParts)if(part.Kind==1||part.Kind==2){var b=part.LocalBounds;visibilityFocus.Add(part.Part.TransformPoint(b.center));visibilityFocus.Add(part.Part.TransformPoint(b.center+Vector3.right*b.extents.x*.35f));visibilityFocus.Add(part.Part.TransformPoint(b.center-Vector3.right*b.extents.x*.35f));visibilityFocus.Add(part.Part.TransformPoint(b.center+Vector3.forward*b.extents.z*.35f));visibilityFocus.Add(part.Part.TransformPoint(b.center-Vector3.forward*b.extents.z*.35f));}
   foreach(var item in visibilityRecords.Values){bool blocked=false;foreach(var point in visibilityFocus){var screen=camera.WorldToViewportPoint(point);if(screen.z<=camera.nearClipPlane||screen.x<0||screen.x>1||screen.y<0||screen.y>1)continue;var ray=camera.ViewportPointToRay(screen);float distance=Vector3.Dot(point-ray.origin,ray.direction);if(distance>0&&MeshBlocks(item,ray,distance)){blocked=true;break;}if(Visibility.triangleTests>=30000)break;}if(blocked)item.lastBlocked=visibilityClock;}
   if(Visibility.triangleTests>=30000)Visibility.budgetExhaustions++;float ms=(float)((System.Diagnostics.Stopwatch.GetTimestamp()-start)*1000.0/System.Diagnostics.Stopwatch.Frequency);Visibility.maxQueryMs=Mathf.Max(Visibility.maxQueryMs,ms);
  }
  Visibility.active=0;Visibility.minOpacity=1;
  foreach(var item in visibilityRecords.Values){float goal=UseFragmentVisibility&&visibilityClock-item.lastBlocked<=.18f?.18f:1;item.alpha=snap?goal:Mathf.MoveTowards(item.alpha,goal,dt/(goal<item.alpha?.08f:.15f));if(item.alpha<.999f){if(!item.assigned){item.renderer.sharedMaterials=item.faded;item.assigned=true;}item.renderer.GetPropertyBlock(visibilityBlock);visibilityBlock.SetFloat("_FragmentOpacity",item.alpha);item.renderer.SetPropertyBlock(visibilityBlock);Visibility.active++;Visibility.minOpacity=Mathf.Min(Visibility.minOpacity,item.alpha);}else RestoreRecord(item);}
  Visibility.peak=Mathf.Max(Visibility.peak,Visibility.active);Visibility.materials=visibilityMaterials.Count;Visibility.blockers=visibilityRecords.Values.Where(x=>x.alpha<.999f).Select(x=>x.id).Distinct().Take(8).ToArray();
#if UNITY_EDITOR
  if(Body)ReviewVisibilityPhysicsUnchanged&=Body.position==beforePosition&&Body.rotation==beforeRotation&&Body.linearVelocity==beforeVelocity&&Body.angularVelocity==beforeOmega;
#endif
 }
}
public sealed partial class CrabFlightGame {
 public void SetFragmentVisibility(string value){useFragmentVisibility=value=="1";if(world!=null){world.UseFragmentVisibility=useFragmentVisibility;world.TickFragmentVisibility(cam,crab,.25f,true);}Report();}
#if UNITY_EDITOR
 public void ReviewRefreshFragmentVisibility(bool on){useFragmentVisibility=on;world.UseFragmentVisibility=on;world.TickFragmentVisibility(cam,crab,.25f,true);}
#endif
}

using System;using System.Collections.Generic;using UnityEngine;
// Derive physical proxies from the visible Hero2 parts, never the hidden donor.
// The mesh/material/art transforms are read only. Thirteen compound hulls preserve the part mapping; visual meshes are unchanged.
public static class HeroColliderFitR21 {
 sealed class CachedHullR22 {public string normals;public List<Vector3> source,points;public int[] faces;}
 // One entry per existing logical part/half. Cache managed data, never a Mesh:
 // FreezeLaunchCompound changes its owned Mesh, so every body gets a copy.
 static readonly Dictionary<string,CachedHullR22> cacheR22=new Dictionary<string,CachedHullR22>();
#if UNITY_EDITOR
 public static int CacheHitsR22,CacheMissesR22;
#endif
 public static List<Vector3> Vertices(CrabPartProxy part,CrabModel crab){var result=new List<Vector3>();foreach(var mesh in part.Part.GetComponentsInChildren<MeshFilter>()){var renderer=mesh.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled||!mesh.sharedMesh)continue;Transform owner=mesh.transform;while(owner){bool isPart=false;foreach(var candidate in crab.CollisionParts)if(candidate.Part==owner){isPart=true;break;}if(isPart)break;owner=owner.parent;}if(owner!=part.Part)continue;foreach(var v in mesh.sharedMesh.vertices)result.Add(part.Part.InverseTransformPoint(mesh.transform.TransformPoint(v)));}if(result.Count==0)throw new InvalidOperationException("Visible physical part missing: "+part.Id);return result;}
 public static Mesh Mesh(CrabPartProxy part,CrabModel crab,int clawHalf=-1){var vertices=new List<Vector3>();var triangles=new List<int>();foreach(var filter in crab.Root.GetComponentsInChildren<MeshFilter>()){var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled||!filter.sharedMesh)continue;Transform owner=filter.transform;while(owner){bool isPart=false;foreach(var candidate in crab.CollisionParts)if(candidate.Part==owner){isPart=true;break;}if(isPart)break;owner=owner.parent;}if(owner!=part.Part&&!(part.Kind==1&&owner==null))continue;bool finger=false;for(var t=filter.transform;t&&t!=part.Part;t=t.parent)if(t.name.Contains("DactylHinge"))finger=true;if(clawHalf>=0&&(finger!=(clawHalf==1)))continue;int offset=vertices.Count;foreach(var v in filter.sharedMesh.vertices)vertices.Add(part.Part.InverseTransformPoint(filter.transform.TransformPoint(v)));foreach(var i in filter.sharedMesh.triangles)triangles.Add(offset+i);}if(vertices.Count<4)throw new InvalidOperationException("Physical hull missing: "+part.Id+"/"+clawHalf);// Intersect128 supporting halfspaces instead of taking an inscribed sample.
 // Each plane contains every visible vertex. A bounded3D polyhedron with128
 // faces has at most252 exact vertices. The254 candidate guard also bounds
 // numerical duplicate intersections below the Px255 vertex/polygon budgets.
 string key=part.Id+"/"+clawHalf;
#if CRAB_KING_BOLD_R2
 var normalsAsset=Resources.Load<TextAsset>("CrabKingBoldR2/hull-normals");
#else
 var normalsAsset=Resources.Load<TextAsset>("HullNormalsR22");
#endif

#if UNITY_EDITOR
 if(BakeModeR22)normalsAsset=null;
#endif
 string normalText=normalsAsset?normalsAsset.text:null;
 if(normalsAsset&&cacheR22.TryGetValue(key,out var cached)&&cached.normals==normalText){bool same=cached.source.Count==vertices.Count;for(int i=0;same&&i<vertices.Count;i++)same=cached.source[i].Equals(vertices[i]);if(same){
#if UNITY_EDITOR
 CacheHitsR22++;
#endif
 RecordOuterFitR22(key,vertices,cached.points);var copy=new Mesh{name="R22 physical supporting hull "+key};copy.SetVertices(cached.points);copy.SetTriangles(cached.faces,0);copy.RecalculateBounds();return copy;}}
#if UNITY_EDITOR
 CacheMissesR22++;
#endif
 List<Vector3> directions=null;if(normalsAsset){var sets=JsonUtility.FromJson<NormalSetsR22>(normalsAsset.text);foreach(var set in sets.rows)if(set.part==key){directions=set.directions;break;}if(directions==null)throw new InvalidOperationException("Missing physical normals "+part.Id);}
#if UNITY_EDITOR
 else if(BakeModeR22)directions=UniformNormalsR22(58);
 else throw new InvalidOperationException("Missing physical normal asset outside explicit bake");
#else
 else throw new InvalidOperationException("Missing physical normal asset");
#endif
 var offsets=OffsetsR22(vertices,directions);var support=IntersectionsR22(directions,offsets);
 RecordOuterFitR22(key,vertices,support);
 var faces=new List<int>();for(int plane=0;plane<directions.Count;plane++){var polygon=new List<int>();Vector3 center=Vector3.zero;for(int k=0;k<support.Count;k++)if(Mathf.Abs(Vector3.Dot(directions[plane],support[k])-offsets[plane])<.0001f){polygon.Add(k);center+=support[k];}if(polygon.Count<3)continue;center/=polygon.Count;var u=Vector3.Cross(directions[plane],Mathf.Abs(directions[plane].y)>.9f?Vector3.right:Vector3.up).normalized;var v=Vector3.Cross(directions[plane],u);polygon.Sort((x,y)=>Mathf.Atan2(Vector3.Dot(support[x]-center,v),Vector3.Dot(support[x]-center,u)).CompareTo(Mathf.Atan2(Vector3.Dot(support[y]-center,v),Vector3.Dot(support[y]-center,u))));for(int k=1;k<polygon.Count-1;k++){faces.Add(polygon[0]);faces.Add(polygon[k]);faces.Add(polygon[k+1]);}}
 if(normalsAsset)cacheR22[key]=new CachedHullR22{normals=normalText,source=vertices,points=support,faces=faces.ToArray()};
 var mesh=new Mesh{name="R22 physical supporting hull "+part.Id+"/"+clawHalf};mesh.SetVertices(support);mesh.SetTriangles(faces,0);mesh.RecalculateBounds();return mesh;}
 [Serializable]public class NormalSetR22{public string part;public List<Vector3> directions;}
 [Serializable]public class NormalSetsR22{public List<NormalSetR22> rows=new List<NormalSetR22>();}
 static List<Vector3> UniformNormalsR22(int count){var dirs=new List<Vector3>{Vector3.up,Vector3.down,Vector3.left,Vector3.right,Vector3.forward,Vector3.back};for(int k=0;k<count;k++){float y=1-2*(k+.5f)/count,r=Mathf.Sqrt(1-y*y),a=k*2.39996323f;dirs.Add(new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r));}return dirs;}
 static float[] OffsetsR22(List<Vector3> vertices,List<Vector3> directions){var offsets=new float[directions.Count];for(int i=0;i<directions.Count;i++){float max=float.NegativeInfinity;foreach(var v in vertices)max=Mathf.Max(max,Vector3.Dot(directions[i],v));offsets[i]=max;}return offsets;}
 static List<Vector3> IntersectionsR22(List<Vector3> directions,float[] offsets){var support=new List<Vector3>();for(int a=0;a<directions.Count-2;a++)for(int b=a+1;b<directions.Count-1;b++)for(int c=b+1;c<directions.Count;c++){var bc=Vector3.Cross(directions[b],directions[c]);float determinant=Vector3.Dot(directions[a],bc);if(Mathf.Abs(determinant)<.00001f)continue;var v=(bc*offsets[a]+Vector3.Cross(directions[c],directions[a])*offsets[b]+Vector3.Cross(directions[a],directions[b])*offsets[c])/determinant;bool inside=true;for(int i=0;i<directions.Count;i++)if(Vector3.Dot(directions[i],v)>offsets[i]+.00001f){inside=false;break;}if(!inside)continue;bool duplicate=false;foreach(var old in support)if((old-v).sqrMagnitude<.000000001f){duplicate=true;break;}if(!duplicate)support.Add(v);}if(support.Count<4||support.Count>254)throw new InvalidOperationException("Physical supporting hull vertex budget: "+support.Count);return support;}

#if UNITY_EDITOR
 [Serializable]public class FitR22 {public string part;public float maximumOverreach;public int hullVertices;[NonSerialized]public List<Vector3> visible;}
 public static bool BakeModeR22;public static readonly List<FitR22> FitRowsR22=new List<FitR22>();
 public static string BakeNormalsJSONR22(){var output=new NormalSetsR22();foreach(var row in FitRowsR22){var candidates=UniformNormalsR22(4096);var candidateOffsets=OffsetsR22(row.visible,candidates);var selected=new List<Vector3>();var used=new HashSet<int>();for(int k=0;k<6;k++){selected.Add(candidates[k]);used.Add(k);}while(selected.Count<128){var hull=IntersectionsR22(selected,OffsetsR22(row.visible,selected));int worst=-1;float maximum=0;for(int i=6;i<candidates.Count;i++){if(used.Contains(i))continue;float top=float.NegativeInfinity;foreach(var point in hull)top=Mathf.Max(top,Vector3.Dot(candidates[i],point));float gap=top-candidateOffsets[i];if(gap>maximum){maximum=gap;worst=i;}}if(maximum<.002f||worst<0)break;used.Add(worst);selected.Add(candidates[worst]);}output.rows.Add(new NormalSetR22{part=row.part,directions=selected});}return JsonUtility.ToJson(output,true);}

#endif
 [System.Diagnostics.Conditional("UNITY_EDITOR")]static void RecordOuterFitR22(string part,List<Vector3> visible,List<Vector3> hull){
#if UNITY_EDITOR
 float maximum=0;for(int k=0;k<256;k++){float y=1-2*(k+.5f)/256,r=Mathf.Sqrt(1-y*y),a=k*2.39996323f;var d=new Vector3(Mathf.Cos(a)*r,y,Mathf.Sin(a)*r);float raw=float.NegativeInfinity,proxy=raw;foreach(var v in visible)raw=Mathf.Max(raw,Vector3.Dot(d,v));foreach(var v in hull)proxy=Mathf.Max(proxy,Vector3.Dot(d,v));maximum=Mathf.Max(maximum,proxy-raw);}FitRowsR22.Add(new FitR22{part=part,maximumOverreach=maximum,hullVertices=hull.Count,visible=visible});
#endif
 }

}
public sealed partial class RigidCrabFlight {public bool UseHeroColliderFitR21=true;}

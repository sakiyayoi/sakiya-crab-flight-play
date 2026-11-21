using System;using System.Collections.Generic;using System.Linq;using UnityEngine;using UnityEngine.Rendering;using C2=CrabCityDensityR2;
// Original two-asset delta. No physics, Scene, score, input or source asset mutation.
public static class FarBreakablePackR10 {
 [Serializable]public sealed class Shard {public int shard;public float[] center,size;public float massKgSuggestion;}
 [Serializable]public sealed class Proposal {public string asset,material;public float massKgSuggestion,normalizedImpulseThresholdSuggestion,maxPostBreakSpeedFractionSuggestion;public Shard[] shards;}
 [Serializable]public sealed class ProposalDocument {public Proposal[] assets;}
 public static C2.CityDensityPack.Geometry MergeGeometry(C2.CityDensityPack.Geometry original,TextAsset delta){
  if(original==null||!delta)throw new ArgumentNullException("Base geometry and explicit delta required");var extra=JsonUtility.FromJson<C2.CityDensityPack.Geometry>(delta.text);if(extra==null||extra.revision!=original.revision)throw new ArgumentException("Geometry revision mismatch");if(extra.palette.Length!=original.palette.Length)throw new ArgumentException("Atlas palette count changed");
  for(int i=0;i<original.palette.Length;i++){var a=original.palette[i];var b=extra.palette[i];if(a.name!=b.name||a.roughness!=b.roughness||a.metallic!=b.metallic||!a.color.SequenceEqual(b.color))throw new ArgumentException("Existing atlas palette changed");}
  var ids=new HashSet<string>(original.assets.Select(a=>a.id));foreach(var a in extra.assets){if(!ids.Add(a.id))throw new ArgumentException("Duplicate asset id "+a.id);if(a.pieces.Sum(p=>p.t.Length/3)>500)throw new ArgumentException("New asset triangle budget exceeded");}
  return new C2.CityDensityPack.Geometry{revision=original.revision,palette=original.palette,assets=original.assets.Concat(extra.assets).ToArray()};
 }
 // Map these proposal objects to the host's descriptor type without reserializing any old20 asset values.
 public static ProposalDocument ReadProposals(TextAsset delta){if(!delta)throw new ArgumentNullException(nameof(delta));var d=JsonUtility.FromJson<ProposalDocument>(delta.text);if(d==null||d.assets==null||d.assets.Length!=2)throw new ArgumentException("Expected exactly two new proposals");return d;}
 public static string SuggestedMaterial(string asset){return asset=="port_lattice_loader_r10"?"metal":asset=="rail_glass_planter_display_r10"?"glass":null;}
 // Full glass planes require an actually supported transparent shader. This helper refuses an opaque fallback.
 // Existing BrokenMetalVisibility has the required _FragmentOpacity property; pass that bound shader explicitly.
 // Call after any R9 mesh swap, before fragment cloning/collider-body setup. No new Collider is created here.
 public static int ApplyTransparentGlass(C2.CityVisualBreakable visual,C2.CityDensityPack.Asset asset,Shader transparentShader){
  if(!visual||asset==null)throw new ArgumentNullException("Visual and matching asset are required");if(asset.id!="rail_glass_planter_display_r10")return 0;if(visual.assetId!=asset.id)throw new ArgumentException("Visual asset mismatch");if(visual.GetComponent<FarNewGlassR10Binding>())return 0;
  if(!transparentShader||!transparentShader.isSupported)throw new InvalidOperationException("Showcase transparent shader is unavailable;do not add invisible pane collision");
  var binding=visual.gameObject.AddComponent<FarNewGlassR10Binding>();int changed=0;
  try {foreach(var filter in visual.GetComponentsInChildren<MeshFilter>(true)){
   int group=Group(visual,filter.transform);if(group== -99)continue;var all=asset.pieces.Where(p=>group==-2||p.shard==group).ToArray();var glass=all.Where(p=>p.material==4&&p.name.Contains("glass_front")).ToArray();if(glass.Length==0)continue;
   var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.sharedMaterial)throw new InvalidOperationException("Showcase source material missing");var mat=new Material(renderer.sharedMaterial){shader=transparentShader,name="R10 original display glass opacity26percent"};if(!mat.HasProperty("_FragmentOpacity")){UnityEngine.Object.Destroy(mat);throw new InvalidOperationException("Transparent shader has no opacity contract");}mat.SetFloat("_FragmentOpacity",.26f);if(mat.HasProperty("_MetalDebrisReadability"))mat.SetFloat("_MetalDebrisReadability",0);if(mat.HasProperty("_FragmentReadability"))mat.SetFloat("_FragmentReadability",0);binding.owned.Add(mat);
   var opaque=MeshOf(all.Except(glass),"R10 display frame and shelves "+group);var pane=MeshOf(glass,"R10 actual thin glass "+group);binding.owned.Add(opaque);binding.owned.Add(pane);binding.filters.Add(filter);binding.originals.Add(filter.sharedMesh);filter.sharedMesh=opaque;
   var child=new GameObject("R10 transparent panes "+group);child.transform.SetParent(filter.transform.parent,false);child.transform.localPosition=filter.transform.localPosition;child.transform.localRotation=filter.transform.localRotation;child.transform.localScale=filter.transform.localScale;child.AddComponent<MeshFilter>().sharedMesh=pane;var r=child.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;binding.children.Add(child);changed++;
  }if(changed!=3)throw new InvalidOperationException("Expected intact plus two independent glass-shard renderers");return changed;}
  catch{RestoreGlass(visual);throw;}
 }
 // Only restore after host fragment cleanup;clones intentionally share the live glass resources.
 public static void RestoreGlass(C2.CityVisualBreakable visual){var b=visual?visual.GetComponent<FarNewGlassR10Binding>():null;if(!b)return;for(int i=0;i<b.filters.Count;i++)if(b.filters[i])b.filters[i].sharedMesh=b.originals[i];foreach(var child in b.children)if(child)UnityEngine.Object.Destroy(child);UnityEngine.Object.Destroy(b);}
 static int Group(C2.CityVisualBreakable visual,Transform filter){int group=-99;for(var p=filter.parent;p&&p!=visual.transform;p=p.parent){if(p==visual.intact.transform)return -2;if(p.name.StartsWith("Shard_"))return int.Parse(p.name.Substring(6));if(p==visual.fractured.transform)group=-1;}return group;}
 static Mesh MeshOf(IEnumerable<C2.CityDensityPack.Piece> pieces,string name){var v=new List<Vector3>();var uv=new List<Vector2>();var t=new List<int>();foreach(var p in pieces){int start=v.Count;for(int i=0;i<p.v.Length;i+=3)v.Add(new Vector3(p.v[i],p.v[i+1],p.v[i+2]));for(int i=0;i<p.uv.Length;i+=2)uv.Add(new Vector2(p.uv[i],p.uv[i+1]));foreach(int k in p.t)t.Add(start+k);}var m=new Mesh{name=name,indexFormat=IndexFormat.UInt32};m.SetVertices(v);m.SetUVs(0,uv);m.SetTriangles(t,0);m.RecalculateNormals();m.RecalculateBounds();return m;}
}
public sealed class FarNewGlassR10Binding:MonoBehaviour {public readonly List<MeshFilter> filters=new List<MeshFilter>();public readonly List<Mesh> originals=new List<Mesh>();public readonly List<GameObject> children=new List<GameObject>();public readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();void OnDestroy(){foreach(var a in owned)if(a){if(Application.isPlaying)UnityEngine.Object.Destroy(a);else UnityEngine.Object.DestroyImmediate(a);}}}

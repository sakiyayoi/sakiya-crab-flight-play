using System;
using System.Collections.Generic;
using UnityEngine;
using C2=CrabCityDensityR2;

// One renderer-only blueprint per game lifetime. Live worlds still create and
// own every contact, break record and body. The blueprint never enters Physics.
// It owns generated meshes/materials; clones borrow them until game shutdown.
public sealed class ContactVisualTemplateR15 : IDisposable {
 GameObject stagedR18;
 GameObject template;string signature;object geometry;
 public int Builds{get;private set;}public int Copies{get;private set;}
#if UNITY_EDITOR
 public static bool ReviewBypass;public int ReviewExactChecks;
 public bool ReviewReleased=>!template;
#endif
 public GameObject Spawn(string key,object source,Func<GameObject> build){
#if UNITY_EDITOR
  if(ReviewBypass)return build();
#endif
  // An unfamiliar layout/style falls back to the original builder. Never
  // invalidate resources that an existing world may still be using, or grow
  // an unbounded cache when a caller changes configuration.
  if(template&&(signature!=key||!ReferenceEquals(geometry,source)))return build();
  if(!template){
   template=build();Builds++;
   if(template.GetComponentsInChildren<Collider>(true).Length!=0||template.GetComponentsInChildren<Rigidbody>(true).Length!=0)
    throw new InvalidOperationException("Contact visual template must contain no physics");
   signature=key;geometry=source;template.SetActive(false);
  }
  if(stagedR18){var ready=stagedR18;stagedR18=null;Copies++;return ready;}
  PrepareCostProbeR15.Mark("template.enter");var copy=UnityEngine.Object.Instantiate(template);copy.name=template.name;PrepareCostProbeR15.Mark("template.instantiate");
  try{CopyBindings(copy);PrepareCostProbeR15.Mark("template.bindings");copy.SetActive(true);PrepareCostProbeR15.Mark("template.activate");Copies++;return copy;}
  catch{Release(copy);throw;}
 }
 void CopyBindings(GameObject copy){
  var a=template.GetComponentsInChildren<Transform>(true);var b=copy.GetComponentsInChildren<Transform>(true);
  if(a.Length!=b.Length)throw new InvalidOperationException("Visual clone hierarchy changed");
  var map=new Dictionary<Transform,Transform>(a.Length);
  for(int i=0;i<a.Length;i++){if(a[i].name!=b[i].name)throw new InvalidOperationException("Visual clone order changed");map.Add(a[i],b[i]);}
#if UNITY_EDITOR
  foreach(var src in a){var dst=map[src];if(!src.localPosition.Equals(dst.localPosition)||!src.localRotation.Equals(dst.localRotation)||!src.localScale.Equals(dst.localScale)||src.gameObject.activeSelf!=dst.gameObject.activeSelf)throw new InvalidOperationException("Visual clone transform/state mismatch");ReviewExactChecks++;}
  foreach(var src in template.GetComponentsInChildren<MeshFilter>(true)){if(src.sharedMesh!=map[src.transform].GetComponent<MeshFilter>().sharedMesh)throw new InvalidOperationException("Visual clone mesh changed");ReviewExactChecks++;}
  foreach(var src in template.GetComponentsInChildren<MeshRenderer>(true)){var dst=map[src.transform].GetComponent<MeshRenderer>();var x=src.sharedMaterials;var y=dst.sharedMaterials;if(src.enabled!=dst.enabled||x.Length!=y.Length)throw new InvalidOperationException("Visual clone renderer changed");for(int i=0;i<x.Length;i++)if(x[i]!=y[i])throw new InvalidOperationException("Visual clone material changed");ReviewExactChecks++;}
#endif
  // Generated asset ownership is intentionally not copied. Retain the existing
  // optional restore APIs by remapping their runtime-only lists explicitly.
  foreach(var owner in copy.GetComponentsInChildren<C2.CityDensityOwnedAssets>(true)){owner.meshes.Clear();owner.materials.Clear();}
  foreach(var src in template.GetComponentsInChildren<WorldArtR9Binding>(true)){
   var dst=map[src.transform].GetComponent<WorldArtR9Binding>();dst.filters.Clear();dst.originals.Clear();
   foreach(var f in src.filters)dst.filters.Add(map[f.transform].GetComponent<MeshFilter>());dst.originals.AddRange(src.originals);
  }
  foreach(var src in template.GetComponentsInChildren<FarNewGlassR10Binding>(true)){
   var dst=map[src.transform].GetComponent<FarNewGlassR10Binding>();dst.filters.Clear();dst.originals.Clear();dst.children.Clear();dst.owned.Clear();
   foreach(var f in src.filters)dst.filters.Add(map[f.transform].GetComponent<MeshFilter>());dst.originals.AddRange(src.originals);
   foreach(var child in src.children)dst.children.Add(map[child.transform].gameObject);
  }
  foreach(var src in template.GetComponentsInChildren<MallObjectBindingR18>(true)){
   var dst=map[src.transform].GetComponent<MallObjectBindingR18>();dst.originalRenderers.Clear();dst.originalEnabled.Clear();dst.generated.Clear();
   foreach(var r in src.originalRenderers)dst.originalRenderers.Add(map[r.transform].GetComponent<MeshRenderer>());dst.originalEnabled.AddRange(src.originalEnabled);
   foreach(var obj in src.generated)dst.generated.Add(map[obj.transform].gameObject);
  }
 }
 public IEnumerable<object> StageCopyR18(){
  if(!template||stagedR18)yield break;
  yield return PrepareBoundaryR18.Value;
  stagedR18=UnityEngine.Object.Instantiate(template);stagedR18.name=template.name;
  yield return PrepareBoundaryR18.Value;
  CopyBindings(stagedR18);
  yield return PrepareBoundaryR18.Value;
  stagedR18.SetActive(true);
 }
 public void ReleaseStagedR18(){if(stagedR18)Release(stagedR18);stagedR18=null;}
 public void Dispose(){ReleaseStagedR18();if(template)Release(template);template=null;signature=null;geometry=null;}
 static void Release(UnityEngine.Object o){if(Application.isPlaying)UnityEngine.Object.Destroy(o);else UnityEngine.Object.DestroyImmediate(o);}
}

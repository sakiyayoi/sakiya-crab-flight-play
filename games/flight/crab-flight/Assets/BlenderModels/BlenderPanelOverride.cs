using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// Mesh-only substitutions; the legacy root, colliders and rigidbodies remain authoritative.
public static class BlenderPanelOverride {
 public const string Revision="blender-panels-1";
 [Serializable] public class Entry {public string id,model;public Vector3 offset;}
 [Serializable] public class Catalog {public Entry[] entries;}
 static Dictionary<string,Entry> entries;
 static Material Material=>Resources.Load<Material>("BlenderModels/PilotFlat");
 static Entry Find(string id){
  if(entries==null){var text=Resources.Load<TextAsset>("BlenderModels/panels-catalog");if(!text)throw new InvalidOperationException("Blender panel catalog missing");entries=JsonUtility.FromJson<Catalog>(text.text).entries.ToDictionary(x=>x.id,StringComparer.Ordinal);}
  entries.TryGetValue(id,out var entry);return entry;
 }
 public static void Apply(GameObject root,string id){
  var entry=Find(id);if(entry==null)return; // Inactive legacy layouts and explicit test fixtures stay outside this rollout.
  var mesh=Mesh(entry.model);var material=Material;if(!material)throw new InvalidOperationException("Blender panel material missing");
  foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
  Draw(root,mesh,entry.model,entry.offset,Vector3.one,material);
 }
 static Mesh Mesh(string model){
  var prefab=Resources.Load<GameObject>("BlenderModels/"+model);
  if(!prefab)throw new InvalidOperationException("Blender panel model missing: "+model);
  if(prefab.GetComponentsInChildren<Collider>(true).Length!=0||prefab.GetComponentsInChildren<Rigidbody>(true).Length!=0)throw new InvalidOperationException("Panel model must be renderer only: "+model);
  return prefab.GetComponentsInChildren<MeshFilter>(true).Single(f=>f.name=="Intact").sharedMesh;
 }
 public static void ApplyFragment(GameObject root,string id,string type){
  if(Find(id)==null)return;
  // Bake no collider or body transforms. Fit the display child to the former local
  // mesh envelope (including CC0 prefab scale) while the actual fragment body stays intact.
  var filters=root.GetComponentsInChildren<MeshFilter>(true);var bounds=new Bounds();bool first=true;
  foreach(var f in filters){var r=f.GetComponent<Renderer>();if(!r||!r.enabled)continue;var b=f.sharedMesh.bounds;var matrix=root.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;
   for(int i=0;i<8;i++){var p=matrix.MultiplyPoint3x4(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);}}
  if(first)throw new InvalidOperationException("Panel fragment has no legacy display: "+id);
  var mesh=Mesh("panel_fragment_"+type);var material=Material;
  foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
  Draw(root,mesh,"panel_fragment_"+type,new Vector3(bounds.center.x,bounds.min.y,bounds.center.z),bounds.size,material);
 }
 static void Draw(GameObject root,Mesh mesh,string model,Vector3 offset,Vector3 scale,Material material){
  var child=new GameObject("Blender "+model);child.transform.SetParent(root.transform,false);child.transform.localPosition=offset;child.transform.localScale=scale;
  child.AddComponent<MeshFilter>().sharedMesh=mesh;var r=child.AddComponent<MeshRenderer>();r.sharedMaterials=Enumerable.Repeat(material,mesh.subMeshCount).ToArray();r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
 }
}

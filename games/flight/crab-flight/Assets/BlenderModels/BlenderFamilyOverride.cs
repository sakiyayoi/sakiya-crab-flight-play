using System;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

// Render-only override. Legacy Geometry/Proposals remain the physics authority.
public static class BlenderFamilyOverride {
 public const string Revision="blender-rollout-1";
 public static bool Enabled=true;
 // Every effective City family, in every sector and both travel directions.
 // CityVisualBreakable exists only on active destructible placements, not scenery.
 public static bool InScope(float x)=>true;
 public static void Apply(GameObject root){
  if(!Enabled)return;
  var material=Resources.Load<Material>("BlenderModels/PilotFlat");
  foreach(var v in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   if(!InScope(v.transform.position.x))continue;
   var prefab=Resources.Load<GameObject>("BlenderModels/"+v.assetId);
   if(!prefab)continue; // A missing family retains the entire legacy visual.
   if(!material)throw new InvalidOperationException("Blender pilot material missing");
   if(prefab.GetComponentsInChildren<Collider>(true).Length!=0||prefab.GetComponentsInChildren<Rigidbody>(true).Length!=0)
    throw new InvalidOperationException("Blender models must be render-only");
   var all=prefab.GetComponentsInChildren<MeshFilter>(true);
   var parts=all.Where(f=>f.name.StartsWith("Part_")).ToArray();
   var intact=all.Single(f=>f.name=="Intact");
   var expected=v.shardPivots.Select(t=>"Part_"+t.name.Substring(6)).ToArray();
   if(expected.Any(n=>parts.Count(p=>p.name==n)!=1))throw new InvalidOperationException("Blender shard mapping mismatch: "+v.assetId);
   var legacy=City2Data.Asset(v.assetId);
   var bounds=new Bounds();bool first=true;
   foreach(var p in legacy.pieces){var b=City2Data.Bounds(p);if(first){bounds=b;first=false;}else bounds.Encapsulate(b);}
   // .blend/FBX origin is bottom-centre. Preserve the old local visual footprint,
   // including old non-zero bottom offsets, without editing any placement.
   var offset=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
   foreach(var r in v.GetComponentsInChildren<Renderer>(true))r.enabled=false;
   var fixedNode=v.fractured.transform.Cast<Transform>().FirstOrDefault(t=>!t.name.StartsWith("Shard_"));
   Draw(intact,v.intact.transform,offset,material);
   foreach(var part in parts){
    if(part.name=="Part_-1"){
     if(!fixedNode)throw new InvalidOperationException("Blender fixed shard has no legacy node: "+v.assetId);
     Draw(part,fixedNode,offset-fixedNode.localPosition,material);
    }else{
     var pivot=v.shardPivots.Single(t=>"Part_"+t.name.Substring(6)==part.name);
     Draw(part,pivot,offset-pivot.localPosition,material);
    }
   }
  }
 }
 static void Draw(MeshFilter source,Transform parent,Vector3 offset,Material material){
  var go=new GameObject("Blender "+source.name);go.transform.SetParent(parent,false);
  // Importer bakes axes into vertices; all part transforms are identity.
  go.transform.localPosition=offset;
  go.AddComponent<MeshFilter>().sharedMesh=source.sharedMesh;
  var r=go.AddComponent<MeshRenderer>();r.sharedMaterials=Enumerable.Repeat(material,source.sharedMesh.subMeshCount).ToArray();
  r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;
 }
}

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using C2=CrabCityDensityR2;

// The adopted three families, on the existing mall's actual contacts (both directions).
// No new targets, energy, score, physics scenes, timers or dynamic bodies.
public static class MallAreasR5 {
 public static bool Enabled=true;
 static readonly HashSet<string> preparedMeshes=new HashSet<string>();
 public const string Revision="mall-three-areas-r5";
 public static string Family(float x){float a=Mathf.Abs(x);return a<62?"gift":a<100?"sweets":"capsule";}
 static readonly string[] LargeIds={"-00-blade_sign","-02-crate_stack","-05-market_crate","-09-blade_sign","-12-street_awning","-14-blade_sign","-17-vending_unit","-20-junction_cabinet","-23-market_crate"};
 public static C2.CityDensityPack.Placement[] Layout(C2.CityDensityPack.Placement[] input){
  if(!Enabled)return input;
  return input.Select(p=>{
   float a=Mathf.Abs(p.p[0]);if(!p.breakable||a<28||a>=135||p.p[1]>2||!p.id.StartsWith("street3-"))return p;
   bool large=LargeIds.Any(p.id.EndsWith);float scale=large?1.12f:1f;
   // Three hero silhouettes per district: wide/low, regular and narrow/tall.
   float width=large?(p.id.Contains("-02-")||p.id.Contains("-12-")||p.id.Contains("-20-")?1.16f:p.id.Contains("-05-")||p.id.Contains("-14-")||p.id.Contains("-23-")?.88f:1f):1f;float height=large?(width>1?.91f:width<1?1.14f:1f):1f;
   return new C2.CityDensityPack.Placement{id=p.id,asset="mall_r5_"+Family(a)+(large?"_large":"_small"),band=p.band,p=p.p,s=new[]{scale*width,scale*height,scale},yaw=p.yaw,breakable=true,chunk=p.chunk,chainGroup=p.chainGroup};
  }).ToArray();
 }
 public static void Apply(GameObject root){
  if(!Enabled)return;
  foreach(var v in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   if(!v.assetId.StartsWith("mall_r5_"))continue;
   if(MallDensityProto.IsVariant(v.assetId)){MallDensityProto.ApplyVariant(v);continue;}
   var prefab=GiftStoreR7.Prefab(v)??Resources.Load<GameObject>("MallAreasR5/"+v.assetId);if(!prefab)throw new InvalidOperationException("Missing area model "+v.assetId);
   // Upload once during real world initialization, without a hidden throw or changing the measurement window. Keep CPU readability for fragment visibility.
   if(Application.isPlaying&&preparedMeshes.Add(prefab.name))foreach(var mesh in prefab.GetComponentsInChildren<MeshFilter>(true).Select(f=>f.sharedMesh).Distinct())mesh.UploadMeshData(false);
   foreach(var r in v.GetComponentsInChildren<MeshRenderer>(true))r.enabled=false;
   CreateIntact(prefab,v.intact.transform);
   foreach(var pivot in v.shardPivots){var part=prefab.transform.Find("Part_"+pivot.name.Substring(6));if(!part)throw new InvalidOperationException("Shard mismatch "+v.assetId);var copy=UnityEngine.Object.Instantiate(part.gameObject,pivot,false);copy.transform.localPosition=-pivot.localPosition;}
   var fixedPart=prefab.transform.Find("Part_-1");var fixedNode=v.fractured.transform.Cast<Transform>().FirstOrDefault(t=>!t.name.StartsWith("Shard_"));
   if(fixedPart&&fixedNode)UnityEngine.Object.Instantiate(fixedPart.gameObject,fixedNode,false);
  }
 }
 // Part_* already has a separate copy under the host's fracture pivots.
 // Keep only the authored intact children in the visual template; otherwise
 // every world clone also carries a second, permanently inactive shard set.
 static GameObject CreateIntact(GameObject prefab,Transform parent){
  // Preserve the original behavior for a future prefab with root components.
  if(prefab.GetComponents<Component>().Length!=1){
   var full=UnityEngine.Object.Instantiate(prefab,parent,false);
   foreach(Transform child in full.transform)if(child.name.StartsWith("Part_"))child.gameObject.SetActive(false);
   return full;
  }
  var intact=new GameObject(prefab.name+"(Clone)");intact.SetActive(false);
  intact.layer=prefab.layer;intact.tag=prefab.tag;intact.isStatic=prefab.isStatic;
  var t=intact.transform;t.SetParent(parent,false);
  t.localPosition=prefab.transform.localPosition;t.localRotation=prefab.transform.localRotation;t.localScale=prefab.transform.localScale;
  foreach(Transform child in prefab.transform)if(!child.name.StartsWith("Part_")){
   var copy=UnityEngine.Object.Instantiate(child.gameObject,t,false);copy.name=child.name;
  }
  intact.SetActive(prefab.activeSelf);return intact;
 }
}

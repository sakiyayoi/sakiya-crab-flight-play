using System;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

// Visual retail layer only. Existing R5 target IDs, collision/shards/masses remain authoritative.
public static class GiftStoreR7 {
 public static bool Enabled=true;
 public const string Revision="gift-store-r7";
 public static GameObject Prefab(C2.CityVisualBreakable target){
  if(!Enabled||!target.assetId.StartsWith("mall_r5_gift_"))return null;
  var parts=target.placementId.Split('-');int slot=parts.Length>2&&int.TryParse(parts[2],out var n)?n:0;
  string name="gift_"+(target.assetId.EndsWith("_large")?"large_":"small_")+(slot/2%3);
  var model=Resources.Load<GameObject>("GiftStoreR7/"+name);if(!model)throw new InvalidOperationException("Missing original retail model "+name);return model;
 }
 public static void AddSurroundings(Transform parent){
  if(!Enabled)return;var source=Resources.Load<GameObject>("GiftStoreR7/gift_surroundings");if(!source)throw new InvalidOperationException("Missing gift-store surroundings");
  if(Application.isPlaying)foreach(var mesh in source.GetComponentsInChildren<MeshFilter>().Select(x=>x.sharedMesh).Distinct())mesh.UploadMeshData(false);
  foreach(float side in new[]{-1f,1f}){var copy=UnityEngine.Object.Instantiate(source,parent,false);copy.name="R7 gift storefront "+side;copy.transform.localPosition=new Vector3(side*45,0,0);}
  MallDensityProto.AddSurroundings(parent);
 }
}

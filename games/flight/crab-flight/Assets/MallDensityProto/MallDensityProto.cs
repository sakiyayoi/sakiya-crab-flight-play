using System;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

// Isolated additive mall prototype. The original host owns contact generation,
// thresholds, debris, chaining and score; no flight or shop values are changed.
public static class MallDensityProto {
 public const string Revision="kani-density-proto3-20261115";
 public const float Start=28f,End=135f;
 public static bool Enabled=true;
 public static bool IsVariant(string id)=>id.StartsWith("mall_r5_density_",StringComparison.Ordinal);
 public static C2.CityDensityPack.Asset[] Geometry()=>JsonUtility.FromJson<C2.CityDensityPack.Geometry>(Resources.Load<TextAsset>("MallDensityProto/Geometry").text).assets;
 [Serializable] public class Proposals { public City2Data.Proposal[] assets; }
 public static City2Data.Proposal[] Props()=>JsonUtility.FromJson<Proposals>(Resources.Load<TextAsset>("MallDensityProto/Proposals").text).assets;
 public static C2.CityDensityPack.Placement[] Additions()=>JsonUtility.FromJson<C2.CityDensityPack.Layout>(Resources.Load<TextAsset>("MallDensityProto/Placements").text).instances;
 public static C2.CityDensityPack.Placement[] Layout(C2.CityDensityPack.Placement[] original)=>Enabled?original.Concat(Additions()).ToArray():original;
 public static void ApplyVariant(C2.CityVisualBreakable v){
  var intact=Resources.Load<Material>("MallDensityProto/targets");var broken=Resources.Load<Material>("MallDensityProto/fragments");
  if(!intact||!broken)throw new InvalidOperationException("Generate the prototype materials before building");
  foreach(var r in v.intact.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=intact;
  foreach(var r in v.fractured.GetComponentsInChildren<Renderer>(true))r.sharedMaterial=broken;
 }
 public static void AddSurroundings(Transform parent){
  if(!Enabled)return;
  var prefab=Resources.Load<GameObject>("MallDensityProto/decor");
  if(!prefab)throw new InvalidOperationException("Generate the prototype decor before building");
  var root=UnityEngine.Object.Instantiate(prefab,parent,false);root.name="DensityProto static mall decor";
 }
}

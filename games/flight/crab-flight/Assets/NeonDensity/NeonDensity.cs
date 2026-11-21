using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using C2=CrabCityDensityR2;

// Add authored placements to the existing host loader. City2Contacts owns all physics,
// thresholds, fragments, finite resources, break events and score/chain accounting.
public static class NeonDensity {
 public static bool Enabled=true;
 public const string Prefix="neon-density-";
 static C2.CityDensityPack.Placement[] additions;
 public static C2.CityDensityPack.Placement[] Additions=>additions??(additions=JsonUtility.FromJson<C2.CityDensityPack.Layout>(Resources.Load<TextAsset>("NeonDensity/Placements").text).instances);
 public static C2.CityDensityPack.Placement[] Layout(C2.CityDensityPack.Placement[] original)=>Enabled?original.Concat(Additions).ToArray():original;
 public static bool IsAddition(string id)=>id.StartsWith(Prefix,StringComparison.Ordinal);
 public static void ApplyAppearance(GameObject root){
  if(!Enabled)return;
  var owner=root.GetComponent<C2.CityDensityOwnedAssets>();
  var variants=new Dictionary<string,Material>();
  foreach(var visual in root.GetComponentsInChildren<C2.CityVisualBreakable>()){
   if(!IsAddition(visual.placementId))continue;
   int index=Array.FindIndex(Additions,p=>p.id==visual.placementId),colorIndex=index%NeonProto.Palette.Length;
   foreach(var renderer in visual.GetComponentsInChildren<Renderer>(true)){
    var source=renderer.sharedMaterial;
    string key=source.GetInstanceID()+":"+colorIndex;
    if(!variants.TryGetValue(key,out var material)){
     // Clone the existing textured material. Standard emission preserves the atlas
     // details, adds a steady neon tint, and follows the existing shards when broken.
     material=new Material(source){name="Density neon variant "+colorIndex,shader=Shader.Find("Standard")};
     var tint=NeonProto.Palette[colorIndex];
     material.SetColor("_Color",Color.Lerp(Color.white,tint,.30f));
     material.SetTexture("_EmissionMap",source.mainTexture);
     material.SetColor("_EmissionColor",tint*.65f);material.EnableKeyword("_EMISSION");
     material.SetFloat("_Glossiness",.25f);
     variants.Add(key,material);owner.materials.Add(material);
    }
    renderer.sharedMaterial=material;
   }
  }
 }
}

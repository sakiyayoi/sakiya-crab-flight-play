using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
// Only material variants actually present in this scene; never all loaded/hidden shader permutations.
public sealed partial class CrabFlightGame {
 int startupShaderVariantsR12,startupUnsupportedPassesR12;
 void WarmSceneShaderVariantsR12(){
  var variants=new ShaderVariantCollection();var materials=new HashSet<Material>();
  foreach(var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))foreach(var m in renderer.sharedMaterials)if(m&&m.shader)materials.Add(m);
  foreach(var m in materials){
   foreach(var pass in new[]{PassType.Normal,PassType.ForwardBase,PassType.ForwardAdd}){
    var keywords=new List<string>(m.shaderKeywords);if(pass!=PassType.Normal&&!keywords.Contains("DIRECTIONAL"))keywords.Add("DIRECTIONAL");
    try{variants.Add(new ShaderVariantCollection.ShaderVariant(m.shader,pass,keywords.ToArray()));}catch(ArgumentException){startupUnsupportedPassesR12++;}
    // Actual first-flight GLSL adds only SH ambient uniforms: UnityCG gates them with LIGHTPROBE_SH.
    if(pass==PassType.ForwardBase){keywords.Add("LIGHTPROBE_SH");try{variants.Add(new ShaderVariantCollection.ShaderVariant(m.shader,pass,keywords.ToArray()));}catch(ArgumentException){startupUnsupportedPassesR12++;}}
   }
  }
  startupShaderVariantsR12=variants.variantCount;variants.WarmUp();Destroy(variants);
 }
}

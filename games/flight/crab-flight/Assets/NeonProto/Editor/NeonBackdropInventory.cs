using System;
using System.IO;
using System.Linq;
using UnityEngine;

public static class NeonBackdropInventory {
 [Serializable]public class Row {public string path,shader,material,texture;public int rendererId,materialId,users;public Color color,blockColor;public bool colorOverride,breakable,collider;public Vector3 min,max;}
 [Serializable]class Inventory {public Row[] renderers;}
 public static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
 public static void Inspect(){
  NeonProto.Enabled=MallDensityProto.Enabled=true;
  var game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
  try{
   var all=UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include,FindObjectsSortMode.None);
   var users=all.SelectMany(r=>r.sharedMaterials).Where(m=>m).GroupBy(m=>m.GetInstanceID()).ToDictionary(g=>g.Key,g=>g.Count());
   var rows=all.Where(r=>r.transform.IsChildOf(game.transform)||r.name.Contains("ground")).SelectMany(r=>r.sharedMaterials.Where(m=>m).Select(m=>{
    var block=new MaterialPropertyBlock();r.GetPropertyBlock(block);
    return new Row{path=PathOf(r.transform),rendererId=r.GetInstanceID(),shader=m.shader.name,material=m.name,materialId=m.GetInstanceID(),users=users[m.GetInstanceID()],color=m.HasProperty("_Color")?m.color:Color.clear,colorOverride=block.HasColor("_Color"),blockColor=block.GetColor("_Color"),texture=m.HasProperty("_MainTex")&&m.mainTexture?m.mainTexture.name:"",min=r.bounds.min,max=r.bounds.max,breakable=r.GetComponentInParent<CrabCityDensityR2.CityVisualBreakable>()!=null,collider=r.GetComponentInParent<Collider>()!=null};
   })).ToArray();
   File.WriteAllText(System.IO.Path.GetFullPath("../../../../evidence/stage2-material-inventory.json"),JsonUtility.ToJson(new Inventory{renderers=rows},true));
  }finally{game.ShutdownReviewPhysics();}
 }
}

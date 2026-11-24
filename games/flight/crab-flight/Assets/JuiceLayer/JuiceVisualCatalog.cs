using System;
using System.Collections.Generic;
using UnityEngine;

public struct JuiceVisual {
 public Color color;public float size;public bool hero,landmark;
}

public static class JuicePanelColors {
 [Serializable] sealed class Entry {public string id;public float[] color;}
 [Serializable] sealed class Document {public Entry[] entries;}
 static Dictionary<string,Color> colors;
 public static bool Find(string id,out Color color){
  if(colors==null){
   colors=new Dictionary<string,Color>();
   var asset=Resources.Load<TextAsset>("JuiceLayer/PanelColors");
   if(!asset)throw new InvalidOperationException("Juice panel colour catalog missing");
   foreach(var entry in JsonUtility.FromJson<Document>(asset.text).entries)
    colors.Add(entry.id,new Color(entry.color[0],entry.color[1],entry.color[2]));
  }
  return colors.TryGetValue(id,out color);
 }
}

// Reads the intact render tree once per prepared world. Never mutates a panel.
public sealed partial class RigidCrabFlight {
 public void ReadJuiceVisuals(Dictionary<string,JuiceVisual> output){
  output.Clear();var meshColors=new Dictionary<Mesh,Color>();
  var renderers=new List<MeshRenderer>();var histogram=new Dictionary<Color,float>();
  var heroes=new HashSet<string>();
  foreach(var entry in OpeningLayout.Data.instances)if(!string.IsNullOrEmpty(entry.hero))heroes.Add(entry.id);
  foreach(var panel in panels){
   var entry=PortLayout.Find(panel.id);
   var color=panel.color;float largest=-1,worldSize=0;
   bool baked=JuicePanelColors.Find(panel.id,out var panelColor);
   var root=panel.city2?panel.city2.intact:panel.city?panel.city.intact:panel.visual;
   renderers.Clear();if(root)root.GetComponentsInChildren(true,renderers);
   foreach(var renderer in renderers){
    if(!renderer.enabled)continue;
    var filter=renderer.GetComponent<MeshFilter>();if(!filter||!filter.sharedMesh)continue;
    var mesh=filter.sharedMesh;var size=renderer.bounds.size;
    float area=size.x*size.y+size.y*size.z+size.z*size.x;
    worldSize=Mathf.Max(worldSize,size.x,size.y,size.z);
    if(area<=largest)continue;
    var material=renderer.sharedMaterial;Color candidate;
    if(material&&material.HasProperty("_Family"))candidate=material.GetColor("_Family");
    else if(baked)candidate=panelColor;
    else {
     if(!meshColors.TryGetValue(mesh,out candidate)){
      // Area-weighted vertex colour: tessellation and tiny accent details cannot win by vertex count.
      candidate=panel.color;
      if(mesh.isReadable){
       var colors=mesh.colors;var positions=mesh.vertices;var triangles=mesh.triangles;
       histogram.Clear();float best=0;
       if(colors.Length==positions.Length)for(int i=0;i+2<triangles.Length;i+=3){
        int a=triangles[i],b=triangles[i+1],c=triangles[i+2];
        float weight=Vector3.Cross(positions[b]-positions[a],positions[c]-positions[a]).magnitude/3;
        for(int j=0;j<3;j++){
         Color key=colors[triangles[i+j]];key.a=1;
         histogram.TryGetValue(key,out float total);total+=weight;histogram[key]=total;
         if(total>best){best=total;candidate=key;}
        }
       }
       if(best==0&&material&&material.HasProperty("_Color"))candidate=material.color;
      }else if(material&&material.HasProperty("_Color"))candidate=material.color;
      meshColors.Add(mesh,candidate);
     }
    }
    color=candidate;largest=area;
   }
   // Non-readable legacy meshes fall back to their authored panel colour.
   var extent=panel.size;
   output[panel.id]=new JuiceVisual{color=color,size=worldSize>0?worldSize:Mathf.Max(extent.x,extent.y,extent.z),
    hero=heroes.Contains(panel.id)||entry!=null&&entry.role=="jackpot",
    landmark=entry!=null&&entry.role=="landmark"};
  }
 }
}

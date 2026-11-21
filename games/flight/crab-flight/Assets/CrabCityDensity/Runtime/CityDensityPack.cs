using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CrabCityDensity {
 // Explicit opt-in renderer-only loader. Read CityPlacement.json + CityGeometry.json.
 // No existing scene search/replacement, Resources dependency, contact or gameplay code.
 public static class CityDensityPack {
  [Serializable] public sealed class Palette {public string name;public float[] color;public float roughness,metallic;}
  [Serializable] public sealed class Piece {public string name;public int material,shard;public float[] v;public int[] t;}
  [Serializable] public sealed class Envelope {public float[] center,size;}
  [Serializable] public sealed class Asset {public string id;public bool breakable;public float massKgSuggestion;public Piece[] pieces;public Envelope collisionEnvelopeSuggestion;}
  [Serializable] public sealed class Geometry {public string revision;public Palette[] palette;public Asset[] assets;}
  [Serializable] public sealed class Placement {public string id,asset,band;public float[] p,s;public float yaw;public bool breakable;public int chunk;}
  [Serializable] public sealed class Layout {public string revision;public Placement[] instances;}
  public enum Detail { Full, Balanced, Low }
  sealed class Batch {public Material material;public readonly List<CombineInstance> meshes=new List<CombineInstance>();}
  sealed class CachedAsset {public Asset source;public readonly Dictionary<int,Mesh> intact=new Dictionary<int,Mesh>();public readonly Dictionary<int,Dictionary<int,Mesh>> shards=new Dictionary<int,Dictionary<int,Mesh>>();public readonly Dictionary<int,Mesh> fixedParts=new Dictionary<int,Mesh>();}

  public static GameObject Build(TextAsset geometryJson,TextAsset layoutJson,Shader shader,Detail detail=Detail.Balanced) {
   if(!geometryJson||!layoutJson)throw new ArgumentNullException("Both city JSON assets are required");
   var g=JsonUtility.FromJson<Geometry>(geometryJson.text);var l=JsonUtility.FromJson<Layout>(layoutJson.text);
   if(g==null||l==null||g.revision!="city-density-r1"||l.revision!=g.revision)throw new ArgumentException("City revision mismatch");
   if(!shader)shader=Shader.Find("Standard");if(!shader||!shader.isSupported)throw new InvalidOperationException("Pass the host's supported opaque shader");
   var root=new GameObject("CrabCityDensity-r1 renderer-only candidate");
   var midBand=Container("DecorativeMid",root.transform).transform;
   var farBand=Container("DecorativeFar",root.transform).transform;
   var owner=root.AddComponent<CityDensityOwnedAssets>();
   var materials=new Material[g.palette.Length];
   for(int i=0;i<materials.Length;i++){
    var p=g.palette[i];var m=new Material(shader){name="City-r1 "+p.name};var c=p.color;
    if(m.HasProperty("_Color"))m.SetColor("_Color",new Color(c[0],c[1],c[2],c[3]));
    if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",new Color(c[0],c[1],c[2],c[3]));
    if(m.HasProperty("_Glossiness"))m.SetFloat("_Glossiness",1-p.roughness);
    if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",1-p.roughness);
    if(m.HasProperty("_Metallic"))m.SetFloat("_Metallic",p.metallic);
    if(m.HasProperty("_Roughness"))m.SetFloat("_Roughness",p.roughness);
    materials[i]=m;owner.materials.Add(m);
   }
   var cache=new Dictionary<string,CachedAsset>();
   foreach(var a in g.assets){var ca=new CachedAsset{source=a};cache.Add(a.id,ca);ca.intact.Clear();Collect(a.pieces,-2,ca.intact,owner);
    if(a.breakable){Collect(a.pieces,-1,ca.fixedParts,owner);var shards=new HashSet<int>();foreach(var p in a.pieces)if(p.shard>=0)shards.Add(p.shard);
     foreach(int shard in shards){var dict=new Dictionary<int,Mesh>();Collect(a.pieces,shard,dict,owner);ca.shards.Add(shard,dict);}
    }
   }
   var batches=new Dictionary<string,Batch>();int farIndex=0,midIndex=0;
   foreach(var p in l.instances){
    bool skip=false;
    if(p.band=="far"){int n=farIndex++;skip=(detail==Detail.Balanced&&n%2==1)||(detail==Detail.Low&&n%3!=0);}
    if(p.band=="mid"&&detail==Detail.Low){int n=midIndex++;skip=p.asset=="roof_tank"||p.asset=="utility_rack"||(p.asset=="facade_module"&&n%2==1);}
    if(skip)continue;
    CachedAsset a;if(!cache.TryGetValue(p.asset,out a))throw new ArgumentException("Unknown city asset "+p.asset);
    var matrix=Matrix4x4.TRS(V(p.p),Quaternion.Euler(0,p.yaw,0),V(p.s));
    if(p.breakable&&a.source.breakable){
     var o=new GameObject(p.id);o.transform.SetParent(root.transform,false);o.transform.localPosition=V(p.p);o.transform.localRotation=Quaternion.Euler(0,p.yaw,0);o.transform.localScale=V(p.s);
     var state=o.AddComponent<CityVisualBreakable>();state.placementId=p.id;state.assetId=p.asset;state.suggestedTotalMassKg=a.source.massKgSuggestion;state.suggestedEnvelopeCenter=V(a.source.collisionEnvelopeSuggestion.center);state.suggestedEnvelopeSize=V(a.source.collisionEnvelopeSuggestion.size);
     state.intact=Container("Intact",o.transform);Draw(a.intact,state.intact.transform,materials);
     state.fractured=Container("Fractured",o.transform);Draw(a.fixedParts,state.fractured.transform,materials);
     var pivots=new List<Transform>();var rest=new List<Vector3>();foreach(var pair in a.shards){
      var pivot=Container("Shard_"+pair.Key,state.fractured.transform);bool first=true;var bounds=new Bounds();foreach(var mesh in pair.Value.Values){if(first){bounds=mesh.bounds;first=false;}else bounds.Encapsulate(mesh.bounds);}
      pivot.transform.localPosition=bounds.center;Draw(pair.Value,pivot.transform,materials);foreach(Transform child in pivot.transform)child.localPosition=-bounds.center;pivots.Add(pivot.transform);rest.Add(bounds.center);
     }
     state.shardPivots=pivots.ToArray();state.shardRestPositions=rest.ToArray();state.SetBroken(false);
    }else foreach(var pair in a.intact){
     string key=p.band+"/chunk-"+p.chunk+"/material-"+pair.Key;Batch batch;
     if(!batches.TryGetValue(key,out batch)){batch=new Batch{material=materials[pair.Key]};batches.Add(key,batch);}
     batch.meshes.Add(new CombineInstance{mesh=pair.Value,transform=matrix});
    }
   }
   foreach(var pair in batches){var mesh=new Mesh{name=pair.Key,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pair.Value.meshes.ToArray(),true,true);mesh.RecalculateBounds();owner.meshes.Add(mesh);var parent=pair.Key.StartsWith("mid/")?midBand:pair.Key.StartsWith("far/")?farBand:root.transform;Renderer(pair.Key,parent,mesh,pair.Value.material);}
   return root;
  }
  static Vector3 V(float[] p){if(p==null||p.Length!=3)throw new ArgumentException("Expected vec3");return new Vector3(p[0],p[1],p[2]);}
  // shardFilter -2 = all intact geometry, -1 = fixed scaffold, >=0 = that shard.
  static void Collect(Piece[] pieces,int shardFilter,Dictionary<int,Mesh> target,CityDensityOwnedAssets owner){
   var vertices=new Dictionary<int,List<Vector3>>();var indices=new Dictionary<int,List<int>>();
   foreach(var p in pieces){if(shardFilter!=-2&&p.shard!=shardFilter)continue;List<Vector3> v;List<int> t;
    if(!vertices.TryGetValue(p.material,out v)){v=new List<Vector3>();vertices.Add(p.material,v);t=new List<int>();indices.Add(p.material,t);}else t=indices[p.material];
    int start=v.Count;if(p.v.Length%3!=0||p.t.Length%3!=0)throw new ArgumentException("Bad city mesh");
    for(int i=0;i<p.v.Length;i+=3)v.Add(new Vector3(p.v[i],p.v[i+1],p.v[i+2]));
    foreach(int index in p.t){if(index<0||index>=p.v.Length/3)throw new ArgumentException("Bad city index");t.Add(start+index);}
   }
   foreach(var pair in vertices){var mesh=new Mesh{name="City cached part",indexFormat=IndexFormat.UInt32};mesh.SetVertices(pair.Value);mesh.SetTriangles(indices[pair.Key],0);var white=new Color32[pair.Value.Count];for(int i=0;i<white.Length;i++)white[i]=new Color32(255,255,255,255);mesh.colors32=white;mesh.RecalculateNormals();mesh.RecalculateBounds();target.Add(pair.Key,mesh);owner.meshes.Add(mesh);}
  }
  static GameObject Container(string name,Transform parent){var o=new GameObject(name);o.transform.SetParent(parent,false);return o;}
  static void Draw(Dictionary<int,Mesh> meshes,Transform parent,Material[] materials){foreach(var pair in meshes)Renderer("Palette "+pair.Key,parent,pair.Value,materials[pair.Key]);}
  static void Renderer(string name,Transform parent,Mesh mesh,Material material){var o=Container(name,parent);o.AddComponent<MeshFilter>().sharedMesh=mesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
 }
}

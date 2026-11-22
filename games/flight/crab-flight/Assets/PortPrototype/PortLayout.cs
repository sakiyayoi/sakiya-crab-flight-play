using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

// One explicit +x override, after all existing layout passes, before contacts and drawing.
public static class PortLayout {
 public const string Revision="port-prototype-1";
 public static bool Enabled=true;
 [Serializable] public sealed class Entry {
  public string id,key,asset,beat,support,role,chain,band;public float[] p,s;
  public float yaw;public int chunk,chainGroup,tier,variant;public bool breakable;
  public C2.CityDensityPack.Placement Placement()=>new C2.CityDensityPack.Placement{id=id,asset=asset,band=band,p=p,s=s,yaw=yaw,chunk=chunk,chainGroup=chainGroup,breakable=breakable};
 }
 [Serializable] public sealed class Document {public string revision,baseline;public Entry[] instances;public float[] resourceReserve;}
 static Document data;static Dictionary<string,Entry> entries;
 public static Document Data {get {if(data==null){var text=Resources.Load<TextAsset>("PortPrototype/Layout");if(!text)throw new InvalidOperationException("Port layout missing");data=JsonUtility.FromJson<Document>(text.text);if(data.revision!=Revision||data.instances.Length!=145)throw new InvalidOperationException("Port revision/count mismatch");entries=data.instances.ToDictionary(e=>e.id);}return data;}}
 public static bool InPort(float x)=>Enabled&&x>=600&&x<900;
 public static Entry Find(string id){if(!Enabled||string.IsNullOrEmpty(id))return null;var _=Data;return entries.TryGetValue(id,out var e)?e:null;}
 public static C2.CityDensityPack.Placement[] Apply(C2.CityDensityPack.Placement[] original){
  if(!Enabled)return original;var _=Data;
  var selected=original.Where(p=>p.breakable&&p.p[0]>=600&&p.p[0]<900).ToArray();
  // This prototype is only installed on the complete active route, never on a partial diagnostic fixture.
  if(selected.Length==0)return original;
  if(selected.Length!=entries.Count||selected.Any(p=>!entries.ContainsKey(p.id)))throw new InvalidOperationException("Port inventory drift: regenerate from current route");
  return original.Select(p=>p.breakable&&entries.TryGetValue(p.id,out var e)?e.Placement():p).ToArray();
 }
 public static Entry NextLandmark(float x,int direction=1){if(!Enabled||direction<0)return null;return Data.instances.Where(e=>e.role=="landmark"&&e.p[0]>x).OrderBy(e=>e.p[0]).FirstOrDefault();}
 public static Color FamilyColor(string asset){
  if(asset=="cargo_container_neon6")return new Color(.64f,.36f,.25f);
  if(asset=="steel_drum")return new Color(.26f,.42f,.56f);
  if(asset.Contains("crate")||asset=="market_stall")return new Color(.72f,.55f,.32f);
  if(asset.Contains("glass"))return new Color(.55f,.71f,.74f);
  return new Color(.75f,.72f,.62f);
 }
}

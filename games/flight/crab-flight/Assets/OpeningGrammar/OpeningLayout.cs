using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using C2=CrabCityDensityR2;

// Placement-only overlay, applied after the existing district generators.
// Contact shapes, destruction, masses, input and camera remain host-owned.
public static class OpeningLayout {
 public const string Revision="grammar-opening-1";
 [Serializable] public sealed class Entry {
  public string id,asset,band,beat,hero;public float[] p,s;
  public float yaw;public int chunk,chainGroup;public bool breakable;
  public C2.CityDensityPack.Placement Placement()=>new C2.CityDensityPack.Placement{
   id=id,asset=asset,band=band,p=p,s=s,yaw=yaw,chunk=chunk,chainGroup=chainGroup,breakable=breakable};
 }
 [Serializable] public sealed class Box {public string key;public float[] p,size,color;}
 [Serializable] public sealed class Landmark {public string id,nextSector;public int side;public Box[] boxes;}
 [Serializable] public sealed class Panel {public string id;public float[] p;}
 [Serializable] public sealed class Document {public string revision,baseline;public Entry[] instances;public Landmark[] landmarks;public Panel[] panels;public Box[] supports;}
 static Document data;
 static Dictionary<string,Entry> entries;
 static Dictionary<string,Vector3> panelPositions;
 public static Document Data {get {
  if(data==null){
   var source=Resources.Load<TextAsset>("OpeningGrammar/Layout");
   if(!source)throw new InvalidOperationException("Opening layout missing");
   var loaded=JsonUtility.FromJson<Document>(source.text);
   if(loaded==null||loaded.revision!=Revision||loaded.instances==null||loaded.instances.Length!=136
      ||loaded.landmarks==null||loaded.landmarks.Length!=6||loaded.panels==null||loaded.panels.Length!=8
      ||loaded.supports==null||loaded.supports.Length!=2)
    throw new InvalidOperationException("Opening layout revision/count mismatch");
   var indexed=new Dictionary<string,Entry>();
   foreach(var e in loaded.instances){
    if(e.p==null||e.p.Length!=3||e.s==null||e.s.Length!=3||!e.breakable||Mathf.Abs(e.p[0])>=135
       ||e.s.Any(v=>!float.IsFinite(v)||v<=0)||e.p.Any(v=>!float.IsFinite(v))||!float.IsFinite(e.yaw))
     throw new InvalidOperationException("Invalid opening placement: "+e.id);
    indexed.Add(e.id,e);
   }
   var positions=new Dictionary<string,Vector3>();
   foreach(var p in loaded.panels){
    if(p.p==null||p.p.Length!=3||p.p.Any(v=>!float.IsFinite(v))||Mathf.Abs(p.p[0])<4.5f||Mathf.Abs(p.p[0])>=28)
     throw new InvalidOperationException("Invalid restaurant placement: "+p.id);
    positions.Add(p.id,new Vector3(p.p[0],p.p[1],p.p[2]));
   }
   entries=indexed;panelPositions=positions;data=loaded;
  }
  return data;
 }}
 public static Vector3 PanelPosition(string id,Vector3 original){
  var document=Data;return panelPositions.TryGetValue(id,out var position)?position:original;
 }
 public static C2.CityDensityPack.Placement[] Apply(C2.CityDensityPack.Placement[] original){
  var document=Data;
  var selected=original.Where(p=>p.breakable&&Mathf.Abs(p.p[0])<135).ToArray();
  if(selected.Length==0)return original; // Independent diagnostic fixtures.
  if(selected.Length!=entries.Count||selected.Any(p=>!entries.ContainsKey(p.id)))
   throw new InvalidOperationException("Opening inventory drift: regenerate from the current route");
  return original.Select(p=>p.breakable&&entries.TryGetValue(p.id,out var e)?e.Placement():p).ToArray();
 }
}

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using C2=CrabCityDensityR2;

// Opt-in via existing loopback Prepare probe. Two disjoint active-wall categories.
public static class CityShapeSplitProbeR18 {
 [StructLayout(LayoutKind.Explicit)]struct Bits{[FieldOffset(0)]public float value;[FieldOffset(0)]public int bits;}
 [Serializable]public class AssetRow{public string id;public int instances,repeatedInstances,shapeCount,definitionMismatches,sourceReferenceChanges;public int[] localDefinitionBits;[NonSerialized]public readonly List<int> bits=new List<int>();[NonSerialized]public City2Data.Proposal sourceProposal;}
 [Serializable]public class Sample{public long frequency=System.Diagnostics.Stopwatch.Frequency,definitionTicks,creationTicks,observerTicks,groupActiveTicks,maxDefinitionVisualTicks,maxCreationVisualTicks;public string maxDefinitionVisual,maxCreationVisual,error;public int visuals,mainCreated,proxyCreated;public bool complete;public AssetRow[] assets;[NonSerialized]public readonly Dictionary<C2.CityDensityPack.Asset,AssetRow> map=new Dictionary<C2.CityDensityPack.Asset,AssetRow>();}
 [Serializable]class Export{public string schema="city-shapes-split-r18.1",scope="Existing Prepare active-wall clock only. Definition includes lookup/panel bookkeeping/iteration, not all cacheable; creation includes fresh Collider/proxy registration. Observer metadata is separately measured. No CPU cycle or formal performance verdict";public Sample[] samples;}
 static readonly List<Sample> history=new List<Sample>();static Sample sample;static AssetRow current;static long last,start,visualDef,visualCreate;static bool first;static int cursor;static string visual;static int mode; // 1 definition,2 creation,3 observer
 static long Clock()=>PrepareCostProbeR15.ReadActiveTicksR18();
 static void Close(long t){if(t<last){if(sample!=null)sample.error="Active clock reset inside group";return;}if(mode==1)sample.definitionTicks+=t-last;else if(mode==2)sample.creationTicks+=t-last;else if(mode==3)sample.observerTicks+=t-last;last=t;}
 static bool Active=>sample!=null&&!sample.complete&&PrepareCostProbeR15.Active;
 static void Metadata(){Close(Clock());mode=3;}
 static void Definitions(){Close(Clock());mode=1;}
 public static void BeginGroup(){if(!PrepareCostProbeR15.Active)return;sample=new Sample();history.Add(sample);last=start=Clock();mode=1;current=null;}
 public static void BeginVisual(C2.CityDensityPack.Asset asset,City2Data.Proposal prop,string id){if(!Active)return;Metadata();visual=id;sample.visuals++;first=!sample.map.TryGetValue(asset,out current);if(first){current=new AssetRow{id=asset.id,sourceProposal=prop};sample.map.Add(asset,current);}else{current.repeatedInstances++;if(!ReferenceEquals(current.sourceProposal,prop))current.sourceReferenceChanges++;}current.instances++;cursor=0;Definitions();visualDef=sample.definitionTicks;visualCreate=sample.creationTicks;}
 static void Bit(float v){int b=new Bits{value=v}.bits;if(first)current.bits.Add(b);else if(cursor>=current.bits.Count||current.bits[cursor]!=b)current.definitionMismatches++;cursor++;}
 public static bool BeforeCreate(Vector3 center,Vector3 size){if(!Active||current==null)return false;Metadata();Bit(center.x);Bit(center.y);Bit(center.z);Bit(size.x);Bit(size.y);Bit(size.z);Close(Clock());mode=2;return true;}
 public static void AfterCreate(bool traced,bool proxy){if(!traced||!Active)return;Close(Clock());mode=3;sample.mainCreated++;if(proxy)sample.proxyCreated++;Definitions();}
 public static void EndVisual(){if(!Active||current==null)return;Metadata();if(first)current.shapeCount=cursor/6;else if(cursor!=current.bits.Count)current.definitionMismatches++;long d=sample.definitionTicks-visualDef,c=sample.creationTicks-visualCreate;if(d>sample.maxDefinitionVisualTicks){sample.maxDefinitionVisualTicks=d;sample.maxDefinitionVisual=visual;}if(c>sample.maxCreationVisualTicks){sample.maxCreationVisualTicks=c;sample.maxCreationVisual=visual;}current=null;Definitions();}
 public static void EndGroup(){if(!Active)return;Close(Clock());sample.groupActiveTicks=last-start;sample.complete=true;mode=0;current=null;}
 public static long ReadCreationTicks()=>sample!=null?sample.creationTicks:0;
 public static string JSON(){foreach(var s in history){var rows=new List<AssetRow>(s.map.Values);foreach(var a in rows)a.localDefinitionBits=a.bits.ToArray();s.assets=rows.ToArray();}return JsonUtility.ToJson(new Export{samples=history.ToArray()});}
}
public sealed partial class CrabFlightGame {
 public void ReadCityShapeSplitR18(string _){if(!LocalPrepareProbeR15())return;resourceAuditR32=CityShapeSplitProbeR18.JSON();Report();resourceAuditR32="";}
}

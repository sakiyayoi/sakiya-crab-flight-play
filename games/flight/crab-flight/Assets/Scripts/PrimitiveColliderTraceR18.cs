using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Loopback opt-in. Tracks only colliders created by CrabModel.Shape, never enumerates a Scene.
public static class PrimitiveColliderTraceR18 {
 sealed class Pending {public Collider collider;public int serial,id,bits=-1,scene=-1,body=-1;public bool sampled;}
 [Serializable]public sealed class Row {public string kind,name,scene,scope;public int serial,id,other,frame,sceneHandle,body,bits;public float sim;public double realtime;}
 [Serializable]public sealed class Receipt {public bool enabled;public int created,pending,eligibleMain,contacts,dropped,checks,lastAbsentFrame;public long probeTicks,frequency;public Row[] events;}
 static readonly List<Pending> pending=new List<Pending>(32);
 static readonly List<Row> events=new List<Row>(128);
 static readonly Dictionary<int,int> eligible=new Dictionary<int,int>();
 static int created,contacts,dropped,checks,lastAbsentFrame=-1,epoch,lastEpoch=-1,lastFrame=-1,lastMain=-1;
 static long probeTicks;
 #if UNITY_WEBGL && !UNITY_EDITOR
 static bool resolved,enabled;
 #endif
#if UNITY_EDITOR
 public static bool ReviewEnabled;
#endif
 public static bool Enabled {get {
#if UNITY_EDITOR
  return ReviewEnabled;
#elif UNITY_WEBGL
  if(!resolved){resolved=true;Uri u;if(Uri.TryCreate(Application.absoluteURL,UriKind.Absolute,out u))enabled=u.IsLoopback&&Array.IndexOf(u.Query.TrimStart('?').Split('&'),"primitiveTrace=1")>=0;}return enabled;
#else
  return false;
#endif
 }}
 public static int PendingCount=>pending.Count;
 public static int EligibleMain=>eligible.Count;
 public static int Contacts=>contacts;
 public static int Dropped=>dropped;
 public static int Checks=>checks;
 static void Add(Row row){if(events.Count<512)events.Add(row);else dropped++;}
 static Row State(Pending p,string kind,string scope,float sim){
  var c=p.collider;var scene=c.gameObject.scene;var rb=c.attachedRigidbody;
  return new Row{kind=kind,name=c.gameObject.name,serial=p.serial,id=p.id,frame=Time.renderedFrameCount,scene=scene.name,sceneHandle=scene.handle,body=rb?rb.GetInstanceID():0,bits=(c.enabled?1:0)|(c.gameObject.activeInHierarchy?2:0)|(c.isTrigger?4:0),scope=scope,sim=sim,realtime=Time.realtimeSinceStartupAsDouble};
 }
 public static void CreatedDestroyScheduled(Collider c){
  if(!Enabled||!c)return;long stamp=CalmSupportPerfR3.Stamp();var p=new Pending{collider=c,id=c.GetInstanceID(),serial=++created};pending.Add(p);epoch++;Add(State(p,"created-destroy-scheduled","CrabModel.Shape",-1));probeTicks+=CalmSupportPerfR3.Stamp()-stamp;
 }
 public static void Mutated(){if(Enabled)epoch++;}
 public static void BeforeMain(Scene main,float sim,string scope,bool force=false){
  if(!Enabled||pending.Count==0)return;
  int frame=Time.renderedFrameCount;if(!force&&lastFrame==frame&&lastMain==main.handle&&lastEpoch==epoch)return;
  long stamp=CalmSupportPerfR3.Stamp();checks++;eligible.Clear();
  for(int i=pending.Count-1;i>=0;i--){var p=pending[i];if(!p.collider){Add(new Row{kind="first-observed-absent",serial=p.serial,id=p.id,frame=frame,sceneHandle=p.scene,body=p.body,bits=p.bits,scope=scope,sim=sim,realtime=Time.realtimeSinceStartupAsDouble});pending.RemoveAt(i);lastAbsentFrame=frame;continue;}
   var row=State(p,"before-main-state",scope,sim);if(row.sceneHandle==main.handle&&(row.bits&3)==3){eligible[p.id]=p.serial;row.bits|=8;}
   if(!p.sampled||p.bits!=row.bits||p.scene!=row.sceneHandle||p.body!=row.body){Add(row);p.sampled=true;p.bits=row.bits;p.scene=row.sceneHandle;p.body=row.body;}
  }
  lastFrame=frame;lastMain=main.handle;lastEpoch=epoch;probeTicks+=CalmSupportPerfR3.Stamp()-stamp;
 }
 public static void ObserveMainPair(int a,int b,float sim){
  if(!Enabled||eligible.Count==0)return;int serial;int own,other;if(eligible.TryGetValue(a,out serial)){own=a;other=b;}else if(eligible.TryGetValue(b,out serial)){own=b;other=a;}else return;
  contacts++;Add(new Row{kind="main-contact-pair",serial=serial,id=own,other=other,frame=Time.renderedFrameCount,sim=sim,realtime=Time.realtimeSinceStartupAsDouble,scope="existing ContactEvent before gameplay filter"});
 }
 public static string JSON()=>JsonUtility.ToJson(new Receipt{enabled=Enabled,created=created,pending=pending.Count,eligibleMain=eligible.Count,contacts=contacts,dropped=dropped,checks=checks,lastAbsentFrame=lastAbsentFrame,probeTicks=probeTicks,frequency=System.Diagnostics.Stopwatch.Frequency,events=events.ToArray()});
#if UNITY_EDITOR
 public static void ReviewReset(){pending.Clear();events.Clear();eligible.Clear();created=contacts=dropped=checks=epoch=0;lastEpoch=lastFrame=lastMain=lastAbsentFrame=-1;probeTicks=0;}
#endif
}

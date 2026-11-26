using System;
using System.Collections.Generic;
using System.Linq;
using Effekseer;
using UnityEngine;

// In-memory native/engine boundary doubles. Production bridge, dispatcher,
// sequence and configuration are compiled unchanged into this executable.
namespace UnityEngine {
 public class Object {
  public static implicit operator bool(Object o)=>o!=null;
  public static T FindFirstObjectByType<T>(FindObjectsInactive _) where T:Object=>GameObject.All.SelectMany(x=>x.components).OfType<T>().FirstOrDefault();
  public static void DontDestroyOnLoad(Object o){}
 }
 public enum FindObjectsInactive {Include}
 public class MonoBehaviour:Object {public GameObject gameObject;public Transform transform=>gameObject.transform;public bool enabled=true;}
 public class Transform {public Vector3 right=new Vector3(1,0,0),up=new Vector3(0,1,0);public void SetParent(Transform p,bool b){}}
 public class GameObject:Object {
  public static List<GameObject> All=new List<GameObject>();public List<Object> components=new List<Object>();public Transform transform=new Transform();
  public GameObject(string name){All.Add(this);}public void SetActive(bool b){}
  public T AddComponent<T>() where T:MonoBehaviour,new(){var c=new T{gameObject=this};components.Add(c);if(c is EffekseerRuntime)EffekseerSystem.Instance=new EffekseerSystem();return c;}
 }
 public struct Vector3 {public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;}public static Vector3 one=>new Vector3(1,1,1);public static Vector3 zero=>new Vector3();public static Vector3 operator*(Vector3 a,float b)=>new Vector3(a.x*b,a.y*b,a.z*b);public static Vector3 operator+(Vector3 a,Vector3 b)=>new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);public static Vector3 ClampMagnitude(Vector3 a,float f)=>a;}
 public struct Color {public Color(float r,float g,float b){}}
 public class Camera {public Transform transform=new Transform();}
 public static class Resources {public static EffekseerEffectAsset Asset=new EffekseerEffectAsset();public static string Path;public static T Load<T>(string p) where T:Object{Path=p;return Asset as T;}}
 public static class Debug {public static int warnings;public static void LogWarning(string s,Object o){warnings++;}}
 public static class Time {public static double realtimeSinceStartupAsDouble;}
}
namespace Effekseer {
 public enum EffekseerRendererType {Native,Unity}
 public enum EffekseerTimeScale {Scale,Unscale}
 public class EffekseerRuntime:MonoBehaviour {}
 public class EffekseerEffectAsset:UnityEngine.Object {}
 public class EffekseerSettings {public static EffekseerSettings Instance=new EffekseerSettings();public EffekseerRendererType RendererType;}
 public class EffekseerSystem {
  public static EffekseerSystem Instance=new EffekseerSystem();public bool enabled=true;public object renderer=new object();public EffekseerRendererType RendererType;
  public static bool IsValid=>Instance!=null&&Instance.enabled;
  public class State {public bool exists=true;public Vector3 position,scale;public EffekseerTimeScale time;}
  public static List<State> handles=new List<State>();public static bool FailNext;
  public static EffekseerHandle PlayEffect(EffekseerEffectAsset asset,Vector3 p){var s=new State{position=p,exists=!FailNext};FailNext=false;handles.Add(s);return new EffekseerHandle{state=s};}
 }
 public struct EffekseerHandle {public EffekseerSystem.State state;public bool exists=>state!=null&&state.exists;public void Stop(){state.exists=false;}public void SetScale(Vector3 s){state.scale=s;}public EffekseerTimeScale TimeScale {set{state.time=value;}}}
}
public struct JuiceVisual {public Color color;public float size;public bool hero,landmark;}
public class FlightGameEvent {public int id;public string objectId="normal",material="wood";public Vector3 position;}
public class FeelCueR27 {}
public class FakeLog {public int count,chain;public string kind,reason;public bool on;public float seconds,timeScale;}
public class FakeDrawing {public int calls;public int JuiceParticles(Vector3 p,Color c,string m,Camera cam,float n){calls++;return 40;}}
public class FakeWorld {public void ReadJuiceVisuals(Dictionary<string,JuiceVisual> v){}}
public class FakeSettings {public bool reducedMotion,shake=true;}
public class FakeClock {public int requests;public void Request(double hold,double slow,double scale){requests++;}}
namespace CrabFeelV1 {public static class CameraSafety {public struct Delta{public double x,y;}public static Delta Sample(double a,double d,double m,int k,bool on,bool shake,bool reduced,int gen)=>new Delta();}}
public sealed partial class CrabFlightGame:MonoBehaviour {
 FeelConfig feelR27=new FeelConfig();FakeSettings feelSettingsR27=new FakeSettings();FakeDrawing feelDrawingR27=new FakeDrawing();FakeWorld world=new FakeWorld();Camera cam=new Camera();FakeClock feelClockR27=new FakeClock();
 bool ReducedMotion,paused,FeelEffectsR27=true,feelWasSlowR27;string phase="flight";int generation;double feelClockStampR27;
 List<FakeLog> logs=new List<FakeLog>();void FeelLogR27(FeelCueR27 cue,string fx,Action<FakeLog> set){var l=new FakeLog();set(l);logs.Add(l);}
 static int passed;
 static void Check(bool b,string name){if(!b)throw new Exception("FAIL "+name);passed++;Console.WriteLine("PASS "+name);}
 static EffekseerBurstBridge Fresh(){EffekseerSystem.Instance=new EffekseerSystem();Resources.Asset=new EffekseerEffectAsset();var b=new GameObject("bridge").AddComponent<EffekseerBurstBridge>();Check(b.Initialize(),"initialize available native system");return b;}
 static void Fill(EffekseerBurstBridge b,bool priority){for(int i=0;i<24;i++)if(!b.Play(new Vector3(i,2,3),priority))throw new Exception("fill failed");}
 static void Main(){
  Check(new FeelConfig().burstMode=="marks"&&!new FeelConfig().UsesEffekseer,"default marks");
  foreach(string s in new[]{null,"","invalid","MARKS","Effekseer"})Check(!new FeelConfig{burstMode=s}.UsesEffekseer,"unknown value uses marks: "+s);
  Check(new FeelConfig{burstMode="effekseer"}.UsesEffekseer,"explicit effekseer");
  var b=Fresh();var original=EffekseerSystem.Instance;Check(b.Initialize()&&ReferenceEquals(original,EffekseerSystem.Instance),"reuse singleton");
  Check(Resources.Path==EffekseerBurstBridge.ResourcePath,"Resources path");
  var outsider=EffekseerSystem.PlayEffect(Resources.Asset,Vector3.zero);Fill(b,false);Check(b.ActiveCount==24&&!b.Play(Vector3.zero,false),"25th ordinary skipped");
  var first=EffekseerSystem.handles[EffekseerSystem.handles.Count-24];Check(b.Play(new Vector3(9,8,7),true)&&!first.exists&&b.ActiveCount==24,"peak evicts oldest ordinary");
  var last=EffekseerSystem.handles.Last();Check(last.position.x==9&&last.position.y==8&&last.position.z==7,"break world position");
  Check(last.scale.x==1&&last.scale.y==1&&last.scale.z==1&&last.time==EffekseerTimeScale.Unscale,"scale and real-time group");
  last.exists=false;Check(b.ActiveCount==23&&b.Play(Vector3.zero,false)&&b.ActiveCount==24,"expired handles free capacity");
  b.Clear();Check(b.ActiveCount==0&&outsider.exists,"clear owns only its handles");
  Fill(b,true);var oldest=EffekseerSystem.handles[EffekseerSystem.handles.Count-24];Check(b.Play(Vector3.zero,true)&&!oldest.exists&&b.ActiveCount==24,"full priority pool still admits peak");
  b.Clear();b.Play(Vector3.zero,true);FillToLimit(b);var hero=EffekseerSystem.handles[EffekseerSystem.handles.Count-24];b.Play(Vector3.zero,true);Check(hero.exists&&b.ActiveCount==24,"ordinary victim preferred over older peak");
  for(int i=0;i<10000;i++)b.Play(Vector3.zero,i%10==0);Check(b.ActiveCount==24,"10000 events remain bounded");
  typeof(EffekseerBurstBridge).GetMethod("OnDisable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(b,null);Check(b.ActiveCount==0,"disable clears");
  Resources.Asset=null;Check(!b.Initialize()&&!b.Play(Vector3.zero,true),"missing import never fakes playback");
  b=Fresh();EffekseerSystem.FailNext=true;Check(!b.Play(Vector3.zero,true)&&b.ActiveCount==0,"invalid native handle not counted");
  GameObject.All.Clear();EffekseerSystem.Instance=null;b=new GameObject("bridge").AddComponent<EffekseerBurstBridge>();Check(b.Initialize()&&GameObject.All.SelectMany(g=>g.components).OfType<EffekseerRuntime>().Count()==1,"missing runtime created once");
  Check(b.Initialize()&&GameObject.All.SelectMany(g=>g.components).OfType<EffekseerRuntime>().Count()==1,"repeat initialization no duplicate runtime");
  b.Play(Vector3.zero,false);EffekseerSystem.Instance=null;b.Clear();Check(b.ActiveCount==0,"teardown after system destruction");
  var g=new GameObject("game").AddComponent<CrabFlightGame>();EffekseerSystem.Instance=new EffekseerSystem();g.ConfigureJuiceBurst();Check(g.juiceBurst==null,"marks does not create bridge");
  g.PresentJuiceBreak(new FlightGameEvent{id=1},new FeelCueR27());Check(g.feelDrawingR27.calls==1,"legacy marks path intact");
  g.feelR27.burstMode="effekseer";g.ConfigureJuiceBurst();g.ResetJuice();Time.realtimeSinceStartupAsDouble=1;
  g.PresentJuiceBreak(new FlightGameEvent{id=1,position=new Vector3(4,5,6)},new FeelCueR27());Check(g.juiceBurst.ActiveCount==1&&g.feelDrawingR27.calls==1,"effekseer replaces only burst");
  g.PresentJuiceBreak(new FlightGameEvent{id=1},new FeelCueR27());Check(g.juiceBurst.ActiveCount==1,"duplicate event not replayed");
  for(int i=2;i<=10;i++)g.PresentJuiceBreak(new FlightGameEvent{id=i},new FeelCueR27());Check(g.feelClockR27.requests==1&&g.juiceSequence.Milestone,"10 chain keeps juice peak");
  g.paused=true;g.SyncJuiceBurst();Check(g.juiceBurst.ActiveCount==0,"pause clears");g.paused=false;
  g.ReducedMotion=true;g.PresentJuiceBreak(new FlightGameEvent{id=11},new FeelCueR27());Check(g.juiceBurst.ActiveCount==0,"OS reduced blocks new bursts");g.ReducedMotion=false;
  g.feelSettingsR27.reducedMotion=true;g.PresentJuiceBreak(new FlightGameEvent{id=12},new FeelCueR27());Check(g.juiceBurst.ActiveCount==0,"feel reduced blocks new bursts");g.feelSettingsR27.reducedMotion=false;
  g.PresentJuiceBreak(new FlightGameEvent{id=13},new FeelCueR27());g.FeelEffectsR27=false;g.SyncJuiceBurst();g.PresentJuiceBreak(new FlightGameEvent{id=14},new FeelCueR27());Check(g.juiceBurst.ActiveCount==0,"off clears and blocks");g.FeelEffectsR27=true;
  g.PresentJuiceBreak(new FlightGameEvent{id=15},new FeelCueR27());g.ResetJuice();Check(g.juiceBurst.ActiveCount==0&&g.juiceSequence.Count==0,"retry clears both sequences");
  g.PresentJuiceBreak(new FlightGameEvent{id=1},new FeelCueR27());g.phase="result";g.SyncJuiceBurst();Check(g.juiceBurst.ActiveCount==0,"result clears");
  g.phase="flight";g.ResetJuice();Fill(g.juiceBurst,false);
  g.juiceVisuals["hero"]=new JuiceVisual{hero=true};g.juiceVisuals["landmark"]=new JuiceVisual{landmark=true};
  int before=EffekseerSystem.handles.Count;
  g.PresentJuiceBreak(new FlightGameEvent{id=1,objectId="hero"},new FeelCueR27());
  Check(EffekseerSystem.handles.Count==before+1&&g.juiceBurst.ActiveCount==24,"full pool still plays hero");
  g.PresentJuiceBreak(new FlightGameEvent{id=2,objectId="landmark"},new FeelCueR27());
  Check(EffekseerSystem.handles.Count==before+2&&g.juiceBurst.ActiveCount==24,"full pool still plays landmark");
  for(int i=3;i<=10;i++)g.PresentJuiceBreak(new FlightGameEvent{id=i},new FeelCueR27());
  Check(EffekseerSystem.handles.Count==before+3&&g.juiceBurst.ActiveCount==24,"full pool skips ordinary but plays chain milestone");
  g.feelR27.burstMode="marks";g.ConfigureJuiceBurst();Check(g.juiceBurst.ActiveCount==0,"mode switch clears previous effects");
  Console.WriteLine($"{passed} passed / 0 failed (engine/native stubs; no Unity execution)");
 }
 static void FillToLimit(EffekseerBurstBridge b){while(b.ActiveCount<24)b.Play(Vector3.zero,false);}
}

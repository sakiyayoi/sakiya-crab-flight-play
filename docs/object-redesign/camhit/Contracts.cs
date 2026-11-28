using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace UnityEngine {
 public class Object {public bool destroyed;public static implicit operator bool(Object o)=>o!=null&&!o.destroyed;public static void Destroy(Object o){if(o!=null)o.destroyed=true;}}
 public class GameObject {public T AddComponent<T>() where T:new()=>new T();}
 public class MonoBehaviour:Object {public GameObject gameObject=new GameObject();}
 public class Camera:MonoBehaviour {}
 public class RequireComponent:Attribute {public RequireComponent(Type t){}}
 public enum FilterMode{Bilinear}
 public enum RenderTextureFormat{Default,ARGBHalf}
 public struct RenderTextureDescriptor {public int width,height,depthBufferBits,msaaSamples;public bool useMipMap,autoGenerateMips;public RenderTextureFormat format;}
 public class RenderTexture:Object {public int width,height;public RenderTextureFormat format;public string name;public FilterMode filterMode;public double pixel;public static int live;bool allocated;public RenderTexture(int w,int h,int d,RenderTextureFormat f){width=w;height=h;format=f;}public RenderTexture(RenderTextureDescriptor d):this(d.width,d.height,d.depthBufferBits,d.format){}public RenderTextureDescriptor descriptor=>new RenderTextureDescriptor{width=width,height=height,format=format};public RenderTextureFormat graphicsFormat=>format;public bool IsCreated()=>allocated;public void Create(){allocated=true;live++;}public void Release(){if(allocated){allocated=false;live--;}}}
 public static class Graphics {public static void Blit(RenderTexture a,RenderTexture b){b.pixel=a.pixel;}}
 public static class Time {public static double realtimeSinceStartupAsDouble;public static float realtimeSinceStartup=>(float)realtimeSinceStartupAsDouble;}
 public struct Vector3 {public static Vector3 up=>new Vector3();}
 public struct Color {}
 public static class ColorUtility {public static bool TryParseHtmlString(string s,out Color c){c=new Color();return true;}}
}
public class ObjectCaptionState {public int sequence;public string kind;public bool visible;}
public class FlightState {public string phase;public object resultOverview;public bool newRecord;public double distance;public int broken;}
public class EffekseerBurstBridge:UnityEngine.Object {public int plays;public void Clear(){} public void PlayObject(Vector3 p,Color c,float s){plays++;}}
public class Hit {public ObjectRules.Kind kind;public Vector3 position;}
public class Simulation {public bool UseObjectRedesign=true;public ObjectRules.Run ObjectRun=new ObjectRules.Run();public List<Hit> ObjectHits=new List<Hit>();public Body Body=new Body();}
public class Body {public Vector3 linearVelocity;}
public class Settings {public bool reducedMotion;}
public class Config {public bool UsesEffekseer=true;}
public class Drawing {public int particles;public void ObjectParticles(Vector3 p,Color c,Camera cam,float now,float scale){particles++;}}
public sealed partial class CrabFlightGame:MonoBehaviour {
 public Simulation world=new Simulation(),simulation;public bool FeelEffectsR27=true,ReducedMotion,paused;public string phase="flight";
 public Settings feelSettingsR27=new Settings();public Config feelR27=new Config();public Drawing feelDrawingR27=new Drawing();public Camera cam=new Camera();public EffekseerBurstBridge juiceBurst=new EffekseerBurstBridge();bool objectDeathBurst;
 readonly ObjectCaptureTimeline objectCapture=new ObjectCaptureTimeline();
 void BeginObjectCapture(Vector3 p){objectCapture.Begin(Time.realtimeSinceStartupAsDouble,FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion);}
 void ClearObjectCapture(){objectCapture.Clear();}
 void AddObjectReadabilityReport(FlightState report){}
 public void AdvanceCapture(){objectCapture.Advance(Time.realtimeSinceStartupAsDouble,objectHitStop&&objectHitStop.Holding,FeelEffectsR27&&!ReducedMotion&&!feelSettingsR27.reducedMotion,paused);}
 public CrabFlightGame(){simulation=world;}
 void Emit(string a,string b,string c,Vector3 d,Vector3 e,Vector3 f,int g){}
 public void Hit(ObjectRules.Kind kind){simulation.ObjectHits.Add(new Hit{kind=kind});DrainObjectEvents();}
 public ObjectHitStopFrame Frame=>objectHitStop;
 public string Caption=>objectCaption?.kind;
 public void Reset(){ClearObjectBursts();}
 public void Report(FlightState s){ApplyObjectPresentationReport(s);}
}
class Contracts {
 static int count;
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);count++;Console.WriteLine("PASS "+label);}
 static void Near(double a,double b,string label)=>Check(Math.Abs(a-b)<1e-5,label+" = "+a);
 static void Render(ObjectHitStopFrame frame,RenderTexture source,RenderTexture dest)=>typeof(ObjectHitStopFrame).GetMethod("OnRenderImage",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(frame,new object[]{source,dest});
 static void Main(){
  Near(PortCameraSettings.Maximum,51,"hard maximum 30 x 1.7");
  Near(PortCameraSettings.Minimum(1.6f),13,"landscape base 10 x 1.3");Near(PortCameraSettings.Minimum(.462f),15.6,"portrait base 12 x 1.3");Near(3.7f*PortCameraSettings.BaseMultiplier,4.81,"aim base 3.7 x 1.3");
  Near(PortCameraSettings.ZoomFollowRate,2,"zoom response unchanged");Near(PortCameraSettings.PositionFollowRate,3,"position response unchanged");Near(PortCameraSettings.VerticalFollowRate,1.2,"vertical response unchanged");
  foreach(float aspect in new[]{320f/568,390f/844,844f/390,1280f/800}){
   float previous=0;
   foreach(float speed in new[]{0,5,20,60,100,160,10000}){
    float z=PortCameraSettings.Zoom(speed,0,aspect);Check(z>=PortCameraSettings.Minimum(aspect)&&z<=51&&z>=previous,"zoom bounded/monotonic "+aspect+" "+speed);previous=z;
   }
   Near(previous,51,"saturates new max "+aspect);
   foreach(float dx in new[]{3,6,12,24})foreach(float dy in new[]{0,7,24}){
    float s=PortCameraSettings.FitNext(PortCameraSettings.Minimum(aspect),dx,dy,aspect),lead=Math.Min(dx*.5f,s*aspect*.45f);
    Check(s<=51&&dx-lead+PortCameraSettings.NextObjectMargin<=s*aspect+.0001f&&lead+PortCameraSettings.SubjectMargin<=s*aspect+.0001f&&dy+5.5f<=2*s+.0001f,"crab + next target fit "+aspect+" "+dx+" "+dy);
   }
  }
  var window=new ObjectHitStopRules.Window();window.Request(.07);window.Request(.09);window.Request(.14);Near(window.Remaining(20),.14,"same frame takes max, not sum");window.Captured(20);Near(window.Remaining(20),.14,"capture clock starts at actual frame");window.Request(.07);window.Captured(20.01);Near(window.Remaining(20.01),.13,"shorter overlapping hold cannot extend longer");window.Request(.2);window.Captured(20.02);Near(window.Remaining(20.02),.2,"longer overlap replaces remainder, not adds");Check(!window.Active(20.221),"hold expires");
  window.Request(.2);window.Clear();Check(!window.Active(0),"reset drops pending and captured holds");
  double[] times={.07,.07,.07,.09,.14,.7};int i=0;
  foreach(ObjectRules.Kind kind in Enum.GetValues(typeof(ObjectRules.Kind))){
   Near(ObjectHitStopRules.Seconds(kind),times[i++],"duration "+kind);
   var game=new CrabFlightGame();Time.realtimeSinceStartupAsDouble=100;game.Hit(kind);
   Check(game.Frame&&game.Frame.Holding&&game.Caption==kind.ToString().ToLowerInvariant(),"actual drain wires hold and caption "+kind);
   Check(game.feelDrawingR27.particles==1&&game.juiceBurst.plays==1,"original particle and burst paths still run "+kind);
   var source=new RenderTexture(390,844,0,RenderTextureFormat.Default){pixel=10};var output=new RenderTexture(390,844,0,RenderTextureFormat.Default);
   Render(game.Frame,source,output);Near(output.pixel,10,"captures impact frame "+kind);
   source.pixel=99;Time.realtimeSinceStartupAsDouble=100+ObjectHitStopRules.Seconds(kind)/2;Render(game.Frame,source,output);Near(output.pixel,10,"holds camera pixels while source keeps moving "+kind);
   Time.realtimeSinceStartupAsDouble=100+ObjectHitStopRules.Seconds(kind)+.001;Render(game.Frame,source,output);Near(output.pixel,99,"live image resumes "+kind);
   game.Reset();Check(RenderTexture.live==0,"reset releases owned frame "+kind);
  }
  foreach(string flag in new[]{"off","reduced","nativeReduced","paused","legacy","ready"}){
   var g=new CrabFlightGame();if(flag=="off")g.FeelEffectsR27=false;if(flag=="reduced")g.feelSettingsR27.reducedMotion=true;if(flag=="nativeReduced")g.ReducedMotion=true;if(flag=="paused")g.paused=true;if(flag=="legacy")g.world.UseObjectRedesign=false;if(flag=="ready")g.phase="ready";
   g.Hit(ObjectRules.Kind.Blast);Check(!g.Frame,"no hold when "+flag);
   if(flag=="off"||flag=="reduced"||flag=="nativeReduced"){int particles=g.feelDrawingR27.particles;g.Hit(ObjectRules.Kind.Death);Check(!g.Frame&&g.feelDrawingR27.particles==particles&&g.juiceBurst.plays==0,"death keeps information only when "+flag);}
  }
  var death=new CrabFlightGame();Time.realtimeSinceStartupAsDouble=200;death.simulation.ObjectRun.Kill();death.Hit(ObjectRules.Kind.Death);death.phase="result";
  var report=new FlightState{phase="result",resultOverview=new object(),newRecord=true,distance=1266.590576171875,broken=80};death.Report(report);
  Check(death.phase=="result"&&report.phase=="flight"&&report.resultOverview==null&&!report.newRecord,"only browser presentation waits; real phase is result");
  Near(report.distance,1266.590576171875,"report distance untouched (sentinel, not PhysX replay)");Check(report.broken==80,"report count untouched (sentinel)");
  var src=new RenderTexture(390,844,0,RenderTextureFormat.Default){pixel=8};var dst=new RenderTexture(390,844,0,RenderTextureFormat.Default);Render(death.Frame,src,dst);
  Time.realtimeSinceStartupAsDouble=200.699;Check(death.Frame.Holding,"death still held before 700ms");Time.realtimeSinceStartupAsDouble=200.701;death.AdvanceCapture();Time.realtimeSinceStartupAsDouble=201.302;death.AdvanceCapture();Time.realtimeSinceStartupAsDouble=202.503;death.AdvanceCapture();report.phase="result";death.Report(report);Check(report.phase=="result","result released after hold, capture and caption");
  death.Hit(ObjectRules.Kind.Blast);Render(death.Frame,src,dst);var old=src;src=new RenderTexture(844,390,0,RenderTextureFormat.ARGBHalf){pixel=12};Render(death.Frame,src,dst);Check(RenderTexture.live==1,"resize/format change replaces single buffer");Near(dst.pixel,12,"resize shows a valid image");
  death.paused=true;Render(death.Frame,src,dst);Check(RenderTexture.live==0&&!death.Frame.Holding,"pause clears immediately before blit");death.paused=false;Check(!death.Frame.Holding,"unpause never replays stale hold");
  death.Hit(ObjectRules.Kind.Blast);Render(death.Frame,src,dst);typeof(ObjectHitStopFrame).GetMethod("OnDisable",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(death.Frame,null);Check(RenderTexture.live==0&&!death.Frame.Holding,"disable cleanup");
  Console.WriteLine(count+" passed / 0 failed");
 }
}

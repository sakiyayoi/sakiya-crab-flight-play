// Offline entry-point execution only: production Run/RunOne/writer, recording
// game/Unity adapters. No PhysX, rendering, real input, or native review claim.
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

public static class ReviewContracts {
 static int passed;
 public static void Check(bool ok,string name){if(!ok)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
 public static int Count => int.Parse(Environment.GetEnvironmentVariable("KANI_REVIEW_FIXTURE_HITS")??"48");
 public static RigidCrabFlight.ObjectHit Hit(int i)=>new RigidCrabFlight.ObjectHit {
  kind=(ObjectRules.Kind)(i%6), position=new Vector3(i%2==0?21:-58,2.5f+i*.25f,i/10f), time=i*.125f };
 public static void Main(){
  PortNativeReview.Run();
  var dir=Environment.GetEnvironmentVariable("PORT_REVIEW_OUTPUT");
  var path=Path.Combine(dir,"camon","action-events.jsonl");
  Check(File.Exists(path),"Run writes action log beneath PORT_REVIEW_OUTPUT/camon");
  Check(!File.Exists(Path.Combine(dir,"action-events.jsonl")),"no misplaced root action log");
  var lines=File.ReadAllLines(path);
  Check(lines.Length==Count,"all consumed hits written exactly once including beyond UI ring capacity");
  for(int i=0;i<lines.Length;i++){
   using var doc=JsonDocument.Parse(lines[i]);var e=doc.RootElement;var want=Hit(i);var p=e.GetProperty("position");
   Check(e.EnumerateObject().Count()==3&&e.GetProperty("kind").GetString()==want.kind.ToString().ToLowerInvariant()
    &&e.GetProperty("time").GetSingle()==want.time&&p.GetProperty("x").GetSingle()==want.position.x
    &&p.GetProperty("y").GetSingle()==want.position.y&&p.GetProperty("z").GetSingle()==want.position.z,"entry preserves kind/position/time "+i);
  }
  Check(Count>0||new FileInfo(path).Length==0,"zero-hit review creates an empty file");
  Check(CrabFlightGame.Instance.ReviewSimulation.ObjectHits.Count==0,"retry really clears source hits after file write");
  Check(CrabFlightGame.Instance.Shutdown,"existing physics shutdown called");
  using var receipt=JsonDocument.Parse(File.ReadAllText(Path.Combine(dir,"receipts.json")));
  var run=receipt.RootElement.GetProperty("runs")[0];
  Check(run.GetProperty("cameraOn").GetBoolean()&&run.GetProperty("retryReady").GetBoolean(),"Run completes camon result and retry receipt");
  Check(BlenderFamilyOverride.Enabled&&BlenderPilotImport.Prepared,"existing initialization preserved");
  Check(UnityEditor.SceneManagement.EditorSceneManager.Opened=="Assets/FlightField.unity","existing scene path preserved");
  Check(UnityEngine.Random.Seeds.Count==2&&UnityEngine.Random.Seeds.All(x=>x==42043),"accepted replay seed preserved");
  Check(CrabFlightGame.Instance.Input==File.ReadAllText(Environment.GetEnvironmentVariable("PORT_REVIEW_INPUT")),"accepted input passes unmodified through Run");
  Check(File.Exists(Path.Combine(dir,"camon","route.csv"))&&File.Exists(Path.Combine(dir,"camon","break-events.jsonl")),"existing route/break output still written");
  Console.WriteLine(passed+" passed / 0 failed (offline review entry adapters)");
 }
}

public class RigidCrabFlight {
 public class ObjectHit { public ObjectRules.Kind kind;public Vector3 position;public float time; }
 public List<ObjectHit> ObjectHits=new(); public List<object> BreakEvents=new(); public List<PortRoleReceipt> PortRoleReceipts=new();
}
public class PortRoleReceipt {}
public static class BlenderFamilyOverride {public static bool Enabled;}
public static class BlenderPilotImport {public static bool Prepared;public static void Prepare(){Prepared=true;}}
public class CrabFlightGame:UnityEngine.Object {
 public static CrabFlightGame Instance;
 public bool enabled=true,usePortCamera,Finished,Shutdown; public string Input;
 public Camera ReviewCamera=new(); public RigidCrabFlight ReviewSimulation=new();
 private Vector3 cameraOffsetR29;private bool cameraFollowingR29;private float steadyFlightSpeedR3;
 public class Snapshot {public float time,worldX,height,speed,distance;public int destructionEvent;public string phase;}
 int ticks;bool ready;
 public void Initialize(){Instance=this;}
 public void RefreshReviewCamera(){}
 public void LoadFeelR27(string json){}
 public void ReviewReplayAcceptedCalmR3(string input){Input=input;}
 public void Tick(float dt){
  if(ticks<ReviewContracts.Count)ReviewSimulation.ObjectHits.Add(ReviewContracts.Hit(ticks));
  ticks++;Finished=ticks>=Math.Max(1,ReviewContracts.Count);
 }
 public Snapshot State()=>new(){time=ticks*.125f,worldX=ticks*3,height=12,speed=56,distance=ticks*3,phase=ready?"ready":Finished?"result":"flight"};
 public void Prepare(string ignored){
  var path=Path.Combine(Environment.GetEnvironmentVariable("PORT_REVIEW_OUTPUT"),"camon","action-events.jsonl");
  ReviewContracts.Check(File.Exists(path)&&File.ReadAllLines(path).Length==ReviewSimulation.ObjectHits.Count,"action file exists before Prepare resets consumed history");
  ReviewSimulation.ObjectHits.Clear();ready=true;
 }
 public void ShutdownReviewPhysics(){Shutdown=true;}
}
namespace UnityEditor {}
namespace UnityEditor.SceneManagement {
 public enum OpenSceneMode{Single}
 public static class EditorSceneManager {public static string Opened;public static void OpenScene(string p,OpenSceneMode mode){Opened=p;}}
}
namespace UnityEngine {
 public class Object {
  public static implicit operator bool(Object o)=>o!=null;
  public static T FindFirstObjectByType<T>() where T:new()=>new T();
  public static void DestroyImmediate(Object o){}
 }
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}}
 public struct Quaternion {}
 public class Transform {public Vector3 position;public Quaternion rotation;public void SetPositionAndRotation(Vector3 p,Quaternion r){position=p;rotation=r;}}
 public class Camera:Object {public RenderTexture targetTexture;public float aspect=1.6f,orthographicSize=10;public Transform transform=new();public void Render(){}}
 public enum RenderTextureFormat{ARGB32}
 public class RenderTexture:Object {public static RenderTexture active;public RenderTexture(int w,int h,int depth,RenderTextureFormat f){}}
 public enum TextureFormat{RGB24}
 public struct Rect {public Rect(float x,float y,float w,float h){}}
 public class Texture2D:Object {public Texture2D(int w,int h,TextureFormat f,bool mip){}public void ReadPixels(Rect r,int x,int y){}public void Apply(){}public byte[] EncodeToPNG()=>Array.Empty<byte>();}
 public static class Random {public static List<int> Seeds=new();public static void InitState(int seed){Seeds.Add(seed);}}
 public static class JsonUtility {public static string ToJson(object o,bool pretty=false)=>JsonSerializer.Serialize(o,new JsonSerializerOptions{IncludeFields=true,WriteIndented=pretty});}
}

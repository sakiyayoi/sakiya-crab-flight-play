// Executes the actual OpeningLayout.cs with JSON/resource shims, not Unity.
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Reflection;
using C2=CrabCityDensityR2;
namespace UnityEngine {
 public class TextAsset {public string text;public static implicit operator bool(TextAsset x)=>x!=null;}
 public static class Resources {public static string Source;public static T Load<T>(string path) where T:class =>new TextAsset{text=Source} as T;}
 public static class JsonUtility {public static T FromJson<T>(string s)=>JsonSerializer.Deserialize<T>(s,new JsonSerializerOptions{IncludeFields=true});}
 public static class Mathf {public static float Abs(float x)=>Math.Abs(x);}
 public struct Vector3 {public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;}}
}
namespace CrabCityDensityR2 {public static class CityDensityPack {
 public class Placement {public string id,asset,band;public float[] p,s;public float yaw;public int chunk,chainGroup;public bool breakable;}
}}
public static class LayoutContract {
 static int passed;
 static void Check(bool value,string name){if(!value)throw new Exception(name);passed++;Console.WriteLine("PASS "+name);}
 static void Reject(Action action,string name){try{action();}catch(InvalidOperationException){Check(true,name);return;}throw new Exception("Failed to reject "+name);}
 static void Reset(string text){typeof(OpeningLayout).GetField("data",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,null);UnityEngine.Resources.Source=text;}
 public static int Main(string[] args){
  var json=File.ReadAllText(args[0]);Reset(json);
  var original=UnityEngine.JsonUtility.FromJson<C2.CityDensityPack.Placement[]>(File.ReadAllText(args[1]));
  string encoded=JsonSerializer.Serialize(original,new JsonSerializerOptions{IncludeFields=true});
  var result=OpeningLayout.Apply(original);
  Check(result.Length==original.Length&&original.Length==1407,"City world count remains 1407");
  Check(OpeningLayout.Data.instances.Length==136,"exact opening inventory");
  Check(original.Where((p,i)=>Math.Abs(p.p[0])>=135).All(p=>Object.ReferenceEquals(p,result.Single(x=>x.id==p.id))),"all 1271 outside-opening entries remain original objects");
  Check(encoded==JsonSerializer.Serialize(original,new JsonSerializerOptions{IncludeFields=true}),"input arrays not mutated");
  foreach(var e in OpeningLayout.Data.instances){var r=result.Single(p=>p.id==e.id);if(r.asset!=e.asset||!r.p.SequenceEqual(e.p)||!r.s.SequenceEqual(e.s)||r.yaw!=e.yaw)throw new Exception(e.id);}
  Check(true,"all 136 runtime overlays match authored data");
  var second=OpeningLayout.Apply(result);Check(second.Select(p=>p.id).SequenceEqual(result.Select(p=>p.id)),"stable identities on repeat");
  var fixture=original.Where(p=>Math.Abs(p.p[0])>=135).ToArray();Check(Object.ReferenceEquals(OpeningLayout.Apply(fixture),fixture),"non-opening diagnostic fixture bypassed");
  Reject(()=>OpeningLayout.Apply(original.Skip(1).ToArray()),"partial opening inventory rejected");
  var first=original.First(p=>Math.Abs(p.p[0])<135);string id=first.id;first.id="unknown-opening";
  Reject(()=>OpeningLayout.Apply(original),"unknown opening identity rejected");first.id=id;
  foreach(var p in OpeningLayout.Data.panels){var v=OpeningLayout.PanelPosition(p.id,new UnityEngine.Vector3(999,999,999));Check(v.x==p.p[0]&&v.y==p.p[1]&&v.z==p.p[2],"restaurant panel "+p.id);}
  var unchanged=OpeningLayout.PanelPosition("core-office-glass-1",new UnityEngine.Vector3(205,3.5f,0));Check(unchanged.x==205&&unchanged.y==3.5f,"office panel passthrough");
  Reset(json.Replace("grammar-opening-1","wrong-revision"));Reject(()=>{var x=OpeningLayout.Data;},"wrong revision rejected");
  Reset(json);Check(OpeningLayout.Data.revision==OpeningLayout.Revision,"valid reload after rejected revision");
  Console.WriteLine("TOTAL "+passed+" passed / 0 failed");return 0;
 }
}

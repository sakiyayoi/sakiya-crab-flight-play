using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;
public static class LogContracts {
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
 static void Main(){
  var dir=Environment.GetEnvironmentVariable("KANI_ACTION_TEST_DIR");Directory.CreateDirectory(dir);
  var events=Enumerable.Range(0,48).Select(i=>new RigidCrabFlight.ObjectHit{kind=(ObjectRules.Kind)(i%6),position=new Vector3(i%2==0?19:-64,20,i/10f),time=i*.125f}).ToList();
  PortNativeReview.WriteActionEvents(dir,events);
  var lines=File.ReadAllLines(Path.Combine(dir,"action-events.jsonl"));
  Check(lines.Length==48,"all consumed hits beyond 32 event ring written once");
  for(int i=0;i<lines.Length;i++){
   using var doc=JsonDocument.Parse(lines[i]);var e=doc.RootElement;
   Check(e.EnumerateObject().Count()==3&&e.GetProperty("kind").GetString()==events[i].kind.ToString().ToLowerInvariant()&&e.GetProperty("time").GetSingle()==events[i].time&&e.GetProperty("position").GetProperty("x").GetSingle()==events[i].position.x&&e.GetProperty("position").GetProperty("y").GetSingle()==20&&e.GetProperty("position").GetProperty("z").GetSingle()==events[i].position.z,"kind position simulation time exact event "+i);
  }
  Check(events.Count==48&&events[0].position.x==19&&events[47].time==47*.125f,"writer does not consume or mutate source hits");
  var empty=Path.Combine(dir,"empty");Directory.CreateDirectory(empty);PortNativeReview.WriteActionEvents(empty,Array.Empty<RigidCrabFlight.ObjectHit>());
  Check(File.Exists(Path.Combine(empty,"action-events.jsonl"))&&new FileInfo(Path.Combine(empty,"action-events.jsonl")).Length==0,"empty legacy throw creates zero-line file");
  Console.WriteLine(passed+" passed / 0 failed");
 }
}
public sealed class RigidCrabFlight {public sealed class ObjectHit {public ObjectRules.Kind kind;public Vector3 position;public float time;}}
namespace UnityEngine {
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}}
 // Serialization-only test adapter. Actual Unity JsonUtility is also typechecked.
 public static class JsonUtility {public static string ToJson(object o)=>JsonSerializer.Serialize(o,new JsonSerializerOptions{IncludeFields=true});}
}

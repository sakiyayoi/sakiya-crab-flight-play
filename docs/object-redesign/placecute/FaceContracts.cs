using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;
public static class FaceContracts {
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
 static GameObject Find(string n)=>GameObject.All.Single(o=>o.name==n);
 static void Main(){
  var sheets=new List<object>();
  for(int i=0;i<6;i++){
   GameObject.All.Clear();var kind=(ObjectRules.Kind)i;var root=ObjectVisual.Build(kind,out var mat,out var motion);
   var story=root.transform.children.Single();var face=Find("face-"+kind.ToString().ToLowerInvariant());
   Check(story.localScale.Equals(Vector3.one*1.6f),"visual multiplier exactly 1.6 "+kind);
   Check(Math.Abs(ObjectRules.HalfSize/1.15f-1.6f)<.000001f,"trigger multiplier exactly 1.6 "+kind);
   var eyes=GameObject.All.Where(o=>o.name.StartsWith("face-eye-")).ToArray();
   Check(eyes.Length==2&&eyes.All(o=>o.primitive==PrimitiveType.Sphere&&o.transform.parent==face.transform),"two primitive eyes on face "+kind);
   var mouth=Find("face-mouth");Check(mouth.transform.parent==face.transform&&mouth.transform.children.Count>0,"small mouth on face "+kind);
   var ink=GameObject.All.Where(o=>o.primitive.HasValue&&o.name.StartsWith("face-")).ToArray();
   ColorUtility.TryParseHtmlString("#241E2E",out var color);
   Check(ink.All(o=>o.renderer.block.colors["_Color"]==color&&o.renderer.block.colors["_EmissionColor"]==Color.black),"warm ink no emission "+kind);
   Check(ink.All(o=>!o.collider.enabled&&o.collider.destroyed),"face has no physics colliders "+kind);
   Check(face.transform.localPosition.z<0,"face on negative Z camera surface "+kind);
   Check(GameObject.All.All(o=>o.primitive!=PrimitiveType.Cube),"rounded sphere capsule and disc palette "+kind);
   Check(eyes.All(o=>o.transform.localScale.x>0&&o.transform.localScale.y>0)&&mouth.transform.localPosition.y<eyes[0].transform.localPosition.y,"positive eye geometry and mouth below eyes "+kind);
   Check(GameObject.All.Count<128,"bounded geometry count "+kind);
   if(kind==ObjectRules.Kind.Boost)Check(face.transform.parent==Find("fan-head").transform&&face.transform.localPosition.z<-.18f,"hub face in front of rotating blades and guard");
   if(kind==ObjectRules.Kind.Bounce)Check(face.transform.parent==Find("tilted-frying-pan").transform&&Find("pan-disc").transform.localRotation.x==90,"front-facing pan disc and attached face");
   if(kind==ObjectRules.Kind.Spin)Check(face.transform.parent!=motion.rotor&&Find("sushi-face-rim").transform.parent==face.transform.parent,"plate front rim outside rotating topping");
   if(kind==ObjectRules.Kind.Brake)Check(mouth.transform.children.Count==2&&Find("mochi-white-body").renderer.block.colors["_Color"]==Color.white,"mochi white body with tiny w mouth");
   if(kind==ObjectRules.Kind.Blast)Check(Find("face-mouth-o").transform.localScale.y>Find("face-mouth-o").transform.localScale.x,"firework surprised round mouth");
   if(kind==ObjectRules.Kind.Death)Check(Find("fisher-hat-crown").primitive==PrimitiveType.Sphere&&Find("fisher-free-arm").primitive==PrimitiveType.Capsule&&face.transform.parent==Find("fisher-person").transform,"fisher rounded hat two arms and face");
   motion.Pose(.125f,false);
   sheets.Add(new{kind=kind.ToString(),nodes=GameObject.All.Select(o=>new{name=o.name,parent=o.transform.parent?.gameObject.name,shape=o.primitive?.ToString(),p=o.transform.localPosition,s=o.transform.localScale,r=o.transform.localRotation,color=o.renderer?.block?.colors.GetValueOrDefault("_Color",mat.color)??mat.color,emission=o.renderer?.block?.colors.GetValueOrDefault("_EmissionColor",mat.colors["_EmissionColor"])??mat.colors["_EmissionColor"]}).ToArray()});
  }
  File.WriteAllText(Environment.GetEnvironmentVariable("KANI_SHAPE_RECORD"),JsonSerializer.Serialize(sheets,new JsonSerializerOptions{IncludeFields=true,WriteIndented=true}));
  Console.WriteLine(passed+" passed / 0 failed");
 }
}

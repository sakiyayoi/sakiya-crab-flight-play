// Executes the real ObjectVisual builder using recording-only Unity API doubles.
// This proves hierarchy/parameters, NOT rendering or PhysX behaviour.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
public static class ShapeContracts {
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);passed++;}
 static GameObject Find(string name)=>GameObject.All.Single(x=>x.name==name);
 static void Main(){
  var names=new[]{"industrial-fan","frying-pan","sushi-conveyor","mochi","firework-ball","crab-fisher"};
  for(int k=0;k<6;k++){
   GameObject.All.Clear();var kind=(ObjectRules.Kind)k;
   var root=ObjectVisual.Build(kind,out var material,out var motion);
   Check(root.transform.children.Single().gameObject.name==names[k],"story root "+kind);
   ColorUtility.TryParseHtmlString(ObjectRules.Get(kind).color,out var color);
   Check(material.color==color&&material.colors["_EmissionColor"]==color*4,"Stage 1 colour/emission "+kind);
   var parts=GameObject.All.Where(x=>x.primitive.HasValue).ToArray();
   Check(parts.Length>1&&parts.All(x=>!x.collider.enabled&&x.collider.destroyed),"all visual primitive colliders disabled/destroyed "+kind);
   Check(parts.All(x=>ReferenceEquals(x.renderer.sharedMaterial,material)),"one owned shared material "+kind);
   var oldRoot=root.transform.localRotation;motion.Pose(.125f,false);
   Check(root.transform.localRotation.Equals(oldRoot),"detail motion cannot rotate root "+kind);
   if(kind==ObjectRules.Kind.Boost){
    Check(Find("fan-base").primitive==PrimitiveType.Cylinder&&GameObject.All.Count(x=>x.name.StartsWith("fan-blade-"))==4&&GameObject.All.Count(x=>x.name.StartsWith("fan-guard-grid-"))==10,"fan cylindrical base four blades and open guard");
    Check(motion.rotor==Find("fan-rotor").transform&&motion.rotor.localRotation.z==90,"fan child spins at 720 degrees/second");
   }
   if(kind==ObjectRules.Kind.Bounce)Check(Find("pan-disc").primitive==PrimitiveType.Cylinder&&Find("pan-disc").transform.localScale.y<.1f&&Find("pan-handle").transform.localPosition.x>0&&Find("tilted-frying-pan").transform.localRotation.z==-12,"flat pan with handle and tilt");
   if(kind==ObjectRules.Kind.Spin){
    Check(Find("sushi-belt").primitive==PrimitiveType.Sphere&&Find("sushi-plate").primitive==PrimitiveType.Cylinder&&Find("sushi-topping").renderer.block.colors["_Color"]!=color,"belt plate and contrasting sushi topping");
    Check(motion.plate&&motion.rotor==Find("sushi-plate-rotor").transform&&motion.rotor.localRotation.y==22.5f&&Find("sushi-topping").transform.parent==motion.rotor,"sushi rides the rotating plate on the belt");
   }
   if(kind==ObjectRules.Kind.Brake)Check(Find("mochi-white-body").primitive==PrimitiveType.Sphere&&Find("mochi-white-body").transform.localScale.y<Find("mochi-white-body").transform.localScale.x&&Find("mochi-white-body").renderer.block.colors["_Color"]==Color.white&&GameObject.All.Count(x=>x.name.StartsWith("mochi-glow-edge-"))==16,"white squashed mochi and purple rim");
   if(kind==ObjectRules.Kind.Blast)Check(Find("firework-shell").primitive==PrimitiveType.Sphere&&Find("firework-fuse").transform.localPosition.y>.6f&&Find("firework-lit-tip").transform.localPosition.y>1,"firework ball fuse and lit tip");
   if(kind==ObjectRules.Kind.Death){
    Check(Find("fisher-body").primitive==PrimitiveType.Capsule&&Find("fisher-head").primitive==PrimitiveType.Sphere&&Find("fisher-hat-brim").transform.localScale.x>Find("fisher-head").transform.localScale.x&&GameObject.All.Count(x=>x.name.StartsWith("fisher-boot-toe-"))==2,"human body head wide hat and two boots");
    Check(Find("fisher-net").transform.localPosition.x>0&&Find("fisher-person").transform.localPosition.x<0&&GameObject.All.Count(x=>x.name.StartsWith("net-grid-"))==10&&GameObject.All.Count(x=>x.name.StartsWith("net-hoop-"))==16,"separate wide open net grid and hoop");
    Check(Find("fisher-hat-brim").renderer.block.colors["_Color"]==Color.black&&Find("fisher-body").renderer.block==null,"death retains magenta and black");
   }
   motion.Pose(2,true);Check(motion.rotor==null||motion.rotor.localRotation.Equals(Quaternion.Euler(0,0,0)),"reduced detail is static "+kind);
  }
  Console.WriteLine(passed+" passed / 0 failed");
 }
}
namespace UnityEngine {
 public class Object {public bool destroyed;public static void Destroy(Object o){o.destroyed=true;}}
 public enum PrimitiveType{Cube,Sphere,Cylinder,Capsule}
 public struct Vector3 {public float x,y,z;public Vector3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}public static Vector3 zero=>new Vector3();public static Vector3 one=>new Vector3(1,1,1);public static Vector3 operator*(Vector3 v,float s)=>new Vector3(v.x*s,v.y*s,v.z*s);}
 // Store authored Euler arguments; this stub makes no claims about Unity quaternions.
 public struct Quaternion {public float x,y,z;public static Quaternion Euler(float x,float y,float z)=>new Quaternion{x=x,y=y,z=z};}
 public struct Color {
  public float r,g,b;public Color(float r,float g,float b){this.r=r;this.g=g;this.b=b;}
  public static Color black=>new Color();public static Color white=>new Color(1,1,1);
  public static Color operator*(Color c,float f)=>new Color(c.r*f,c.g*f,c.b*f);
  public static bool operator==(Color a,Color b)=>a.Equals(b);public static bool operator!=(Color a,Color b)=>!a.Equals(b);
  public override bool Equals(object o)=>o is Color c&&r==c.r&&g==c.g&&b==c.b;public override int GetHashCode()=>HashCode.Combine(r,g,b);
 }
 public static class ColorUtility {public static bool TryParseHtmlString(string s,out Color c){c=new Color(Convert.ToInt32(s.Substring(1,2),16)/255f,Convert.ToInt32(s.Substring(3,2),16)/255f,Convert.ToInt32(s.Substring(5,2),16)/255f);return true;}}
 public static class Mathf {public const float PI=(float)Math.PI;public static float Sin(float f)=>(float)Math.Sin(f);public static float Cos(float f)=>(float)Math.Cos(f);public static float Sqrt(float f)=>(float)Math.Sqrt(f);}
 public class Shader{public static Shader Find(string s)=>new Shader();}
 public class Material {public Color color;public Dictionary<string,Color> colors=new Dictionary<string,Color>();public Material(Shader s){}public void EnableKeyword(string s){}public void SetColor(string s,Color c){colors[s]=c;}public void SetFloat(string s,float v){}}
 public class MaterialPropertyBlock {public Dictionary<string,Color> colors=new Dictionary<string,Color>();public void SetColor(string s,Color c){colors[s]=c;}}
 public class Collider:Object {public bool enabled=true;}
 public class Renderer {public Material sharedMaterial;public MaterialPropertyBlock block;public void SetPropertyBlock(MaterialPropertyBlock b){block=b;}}
 public class Transform {public GameObject gameObject;public Transform parent;public List<Transform> children=new List<Transform>();public Vector3 localPosition,localScale=Vector3.one;public Quaternion localRotation;public void SetParent(Transform p,bool world){parent=p;p.children.Add(this);}}
 public class GameObject {
  public static List<GameObject> All=new List<GameObject>();public string name;public Transform transform;public PrimitiveType? primitive;public Collider collider;public Renderer renderer;
  public GameObject(string name){this.name=name;transform=new Transform{gameObject=this};All.Add(this);}
  public static GameObject CreatePrimitive(PrimitiveType p)=>new GameObject(p.ToString()){primitive=p,collider=new Collider(),renderer=new Renderer()};
  public T GetComponent<T>() where T:class=>typeof(T)==typeof(Collider)?collider as T:renderer as T;
 }
}

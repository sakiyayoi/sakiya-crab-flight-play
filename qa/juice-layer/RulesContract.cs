using System;
using System.Collections.Generic;
using System.Linq;

// Minimal numeric drawing types only; no Unity runtime/scene/PhysX is executed.
namespace UnityEngine {
 public struct Vector3 {
  public float x,y,z;public Vector3(float a,float b,float c){x=a;y=b;z=c;}
  public static Vector3 operator +(Vector3 a,Vector3 b)=>new Vector3(a.x+b.x,a.y+b.y,a.z+b.z);
  public static Vector3 operator *(Vector3 a,float b)=>new Vector3(a.x*b,a.y*b,a.z*b);
 }
 public struct Color {public float r,g,b,a;public Color(float x,float y,float z,float w=1){r=x;g=y;b=z;a=w;}}
 public class Transform {public Vector3 right=new Vector3(1,0,0),up=new Vector3(0,1,0);}
 public class Camera {public Transform transform=new Transform();}
 public static class Mathf {public static float Sin(float x)=>(float)Math.Sin(x);public static float Cos(float x)=>(float)Math.Cos(x);}
}
public static class PortVisuals {public static readonly UnityEngine.Color Lantern=new UnityEngine.Color(1,.70196f,.27843f);}
public sealed partial class MashimashiPresentationR27 {
 public struct Mark {public int kind,material;public float born,life,size,angle,turn,stretch;public UnityEngine.Vector3 p,v;public UnityEngine.Color color;}
 public readonly List<Mark> marks=new List<Mark>(JuiceRules.MarkLimit);
 int MaterialID(string kind)=>kind=="glass"?1:kind=="wood"?3:2;
}
public static class Contract {
 static int passed;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);passed++;}
 static bool Near(double a,double b)=>Math.Abs(a-b)<.000001;
 public static void Main(){
  var s=new JuiceRules.Sequence();
  Check(s.Record(1,0)&&s.Count==1,"first break");
  Check(!s.Record(1,0)&&s.Count==1,"same event cannot duplicate");
  Check(!s.TakePeak(0,false,false),"ordinary never stops");
  Check(s.Record(2,1.5)&&s.Count==2,"inclusive window");
  Check(s.Record(3,3.00001)&&s.Count==1,"window expiry");
  Check(!s.Record(4,double.NaN),"invalid clock");
  s.Reset();Check(s.Record(1,0)&&s.Count==1,"retry resets cursor");
  for(int i=2;i<=10;i++)s.Record(i,i*.02);
  Check(s.Milestone&&s.TakePeak(.2,false,false),"tenth break peak");
  Check(!s.TakePeak(.21,true,true),"overlapping peaks do not extend stop");
  Check(s.TakePeak(1.2,true,false),"hero after cooldown");
  var clock=new CrabFeelV1.PresentationClock();clock.Request(0,JuiceRules.PeakSeconds,JuiceRules.PeakScale);
  Check(Near(clock.Filter(.28,false,true),.07),"0.25 scale for 0.28s");
  Check(Near(clock.Filter(.1,false,true),.1),"normal time resumes");
  clock.Request(0,.28,.25);Check(clock.Filter(2,true,true)==0&&Near(clock.SlowRemaining,.28),"pause freezes remaining duration");
  Check(Near(clock.Filter(.1,false,false),.1)&&clock.SlowRemaining==0,"off resets slowdown");
  Check(JuiceRules.Shake(1,false,false,false)==0,"tiny no shake");
  Check(JuiceRules.Shake(3,false,false,false)<JuiceRules.Shake(8,false,false,false),"size proportional");
  Check(JuiceRules.Shake(100,false,false,false)<=JuiceRules.NormalShakeMax,"ordinary amplitude capped");
  Check(JuiceRules.Shake(1,true,false,false)>JuiceRules.Shake(100,false,true,false),"hero larger than landmark");
  var pulse=new JuiceRules.ShakePulse();Check(pulse.Request(.04,false),"start shake");pulse.Advance(.1);
  Check(!pulse.Request(.04,false)&&Near(pulse.Age,.1),"contacts do not refresh shake");
  pulse.Advance(.03);var d=CrabFeelV1.CameraSafety.Sample(pulse.Age,pulse.Duration,pulse.Amplitude,0,true,true,false,1);
  Check(d.x==0&&d.y==0,"quiet gap after decay");
  Check(pulse.Request(.18,true)&&pulse.Age==0,"hero promotes small shake");pulse.Reset();
  Check(pulse.Amplitude==0&&pulse.Duration==0,"comfort/reset clears pulse");
  foreach(var flags in new[]{new[]{false,true,false},new[]{true,false,false},new[]{true,true,true}}){
   d=CrabFeelV1.CameraSafety.Sample(.02,.2,.18,0,flags[0],flags[1],flags[2],1);
   Check(d.x==0&&d.y==0,"off/shake-off/reduced zero");
  }
  var drawing=new MashimashiPresentationR27();var color=new UnityEngine.Color(.3f,.6f,.2f);var camera=new UnityEngine.Camera();
  for(int i=0;i<10000;i++)drawing.JuiceParticles(new UnityEngine.Vector3(i,1,2),color,"wood",camera,i);
  Check(drawing.marks.Count==512&&drawing.marks.Capacity==512,"10k bursts bounded without list growth");
  Check(drawing.marks[0].born==9987&&drawing.marks.Last().born==9999,"oldest evicted first");
  var last=drawing.marks.Where(m=>m.born==9999).ToArray();
  Check(last.Length==40&&last.Count(m=>m.kind==0)==24&&last.Count(m=>m.kind==5)==8&&last.Count(m=>m.kind==6)==8,"three layers survive cap");
  Check(last.Where(m=>m.kind==0).All(m=>m.color.r==color.r&&m.color.g==color.g&&m.color.b==color.b),"actual object RGB preserved");
  Check(last.Where(m=>m.kind==5).All(m=>m.color.r==1&&m.color.g==1&&m.color.b==1),"white smoke");
  Check(last.Where(m=>m.kind==6).All(m=>Near(m.color.g,179d/255)&&Near(m.color.b,71d/255)),"lantern spark RGB");
  Check(last.All(m=>m.life<=.65f&&m.life>0),"finite lifetime");
  Console.WriteLine(passed+" passed / 0 failed (actual rules/clock/burst, numeric Unity stubs; no player)");
 }
}

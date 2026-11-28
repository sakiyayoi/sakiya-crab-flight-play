using System;
using System.Linq;
using UnityEngine;
class ReadabilityContracts {
 static int count;
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);count++;Console.WriteLine("PASS "+name);}
 static void Near(double a,double b,string name)=>Check(Math.Abs(a-b)<.00001,name);
 static GameObject Find(string name)=>GameObject.All.Single(o=>o.name==name);
 static void Main(){
  var clock=new ObjectCaptureTimeline();
  Check(!clock.Active,"ordinary end has no capture wait");clock.Begin(100,true);
  Check(clock.Current==ObjectCaptureTimeline.Stage.Hold,"death begins in render hold");
  clock.Advance(110,true,true,false);Check(clock.Current==ObjectCaptureTimeline.Stage.Hold,"delayed rendering cannot eat capture time");
  clock.Advance(110.7,false,true,false);Check(clock.Current==ObjectCaptureTimeline.Stage.Capture,"hold expiry enters capture");
  clock.Advance(111,false,true,false);Near(clock.PullProgress,.5,"halfway through 600ms pull");
  clock.Advance(111.301,false,true,false);Check(clock.CaptionVisible&&clock.Active,"capture ends with large subtitle");
  clock.Advance(112.50,false,true,false);Check(clock.Active&&clock.CaptionVisible,"subtitle still present at 1199ms");
  clock.Advance(112.502,false,true,false);Check(!clock.Active&&clock.Current==ObjectCaptureTimeline.Stage.Done,"1200ms subtitle completes before result");
  clock.Clear();Check(!clock.Active&&clock.Current==ObjectCaptureTimeline.Stage.Idle,"retry clears capture and cause presentation state");
  foreach(string setting in new[]{"off","OS reduced","native reduced"}){
   clock.Begin(0,false);Check(clock.CaptionVisible&&clock.Active,setting+" keeps subtitle only");
   clock.Advance(1.199,false,false,false);Check(clock.CaptionVisible,setting+" preserves full 1.2 seconds");
   clock.Advance(1.201,false,false,false);Check(!clock.Active,setting+" releases result");
  }
  foreach(bool held in new[]{true,false}){
   clock.Begin(0,true);clock.Advance(.7,held,true,false);clock.Advance(.8,false,false,false);
   Check(clock.CaptionVisible&&clock.Age==0,"settings change during "+(held?"hold":"capture")+" goes to caption without movement");
   clock.Advance(2,false,false,false);Check(!clock.Active,"changed settings do not replay capture");
  }
  clock.Begin(0,false);clock.Advance(.4,false,false,false);clock.Advance(20,false,false,true);
  Near(clock.Age,.4,"pause preserves subtitle elapsed time");clock.Advance(20.5,false,false,false);Check(clock.CaptionVisible,"resume uses remaining duration");
  clock.Advance(20.9,false,false,false);Check(!clock.Active,"resume eventually releases result");
  foreach(ObjectRules.Kind kind in Enum.GetValues(typeof(ObjectRules.Kind))){
   GameObject.All.Clear();var root=ObjectVisual.Build(kind,out var material,out var motion);
   var before=root.transform.localPosition;var scale=root.transform.localScale;var rotation=root.transform.localRotation;
   motion.Pose(.125f,false);
   Check(root.transform.localPosition.Equals(before)&&root.transform.localScale.Equals(scale)&&root.transform.localRotation.Equals(rotation),kind+" child pose never changes root/trigger");
   if(kind==ObjectRules.Kind.Boost){
    Check(motion.marks.Length==3&&motion.marks.All(t=>t.localPosition.x>0),"three wind streaks travel local +X; mirrored root turns them outward");
    float x=motion.marks[0].localPosition.x;motion.Pose(.15f,false);Check(motion.marks[0].localPosition.x>x,"wind continuously advances outward");
    Near(motion.rotor.localRotation.z,108,"fan remains fast 720 deg/s");
   }
   if(kind==ObjectRules.Kind.Bounce){Check(motion.soft.localScale.x<1&&motion.soft.localScale.y>1,"pan stretches and narrows like a spring");}
   if(kind==ObjectRules.Kind.Spin){Check(Find("sushi-topping").transform.localPosition.x!=0&&Find("sushi-topping").transform.localPosition.z!=0,"off-center topping breaks half-turn symmetry");}
   if(kind==ObjectRules.Kind.Brake){
    Check(GameObject.All.Count(o=>o.name.StartsWith("mochi-sticky-strand-"))==3,"three thin downward sticky strands");
    Check(motion.soft.localScale.y>1&&motion.soft.localScale.y<1.04f,"mochi slow small wobble");
   }
   if(kind==ObjectRules.Kind.Blast){Check(motion.marks.Length==3&&motion.marks.All(t=>t.localPosition.y>1.02f),"three sparks originate at fuse tip");}
   if(kind==ObjectRules.Kind.Death){Check(motion.net.localPosition.y>1&&motion.net.localRotation.z!=0,"raised net sways slowly");}
   motion.Pose(1,true);
   Check(motion.marks==null||motion.marks.All(t=>!t.gameObject.activeSelf),kind+" off/reduced suppresses marks");
   Check(motion.soft==null||motion.soft.localScale.Equals(Vector3.one),kind+" off/reduced restores static scale");
   Check(motion.net==null||motion.net.localRotation.Equals(Quaternion.Euler(0,0,0)),kind+" off/reduced stops net sway");
   Check(GameObject.All.Where(o=>o.primitive.HasValue).All(o=>!o.collider.enabled&&o.collider.destroyed),kind+" added art has zero colliders");
   Check(GameObject.All.Count<128,kind+" bounded shape budget remains below original cap");
  }
  Console.WriteLine(count+" passed / 0 failed");
 }
}

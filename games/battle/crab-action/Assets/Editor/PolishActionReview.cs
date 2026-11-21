using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
public static class PolishActionReview {
 const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
 [Serializable]public class FixedCase{public string name;public int trial;public float before,after;public string beforeEvent,afterEvent;}
 [Serializable]public class MatrixCase{public string name;public ActionState state;}
 [Serializable]public class Review{public string status="PASS_FIXED_REGRESSIONS_AND_20_NATIVE_CONDITIONS";public FixedCase[] regressions;public MatrixCase[] conditions;public string scope="Actual Unity queries, fixed1/90 native steps; visual correspondence on20conditions not independently rendered; physical phone separate";}
 static object Field(object target,string name){return target.GetType().GetField(name,Flags).GetValue(target);}
 static void Put(object target,string name,object value){target.GetType().GetField(name,Flags).SetValue(target,value);}
 static object Call(object target,string name,params object[] values){return target.GetType().GetMethod(name,Flags).Invoke(target,values);}
 static Component Controller(bool before){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);Component g=before?(Component)new GameObject("BeforePolish").AddComponent<BeforePolishActionGame>():new GameObject("AfterPolish").AddComponent<CrabActionGame>();Put(g,"crabShader",Shader.Find("CustomCrabBattle/ChitinR3"));Put(g,"groundShader",Shader.Find("Standard"));Put(g,"aiEnabled",false);Call(g,"Initialize");return g;}
 static ActionState State(object g){return(ActionState)Call(g,"State");}
 static void Step(object g,int n){for(int i=0;i<n;i++)Call(g,"Tick",1f/90);}
 static object Weapon(object g,string unit,int side){return((Array)Field(Field(g,unit),"weapons")).GetValue(side);}
 static void Reset(object g,string left,string right,Vector3 p,Vector3 e){Call(g,"Customize","");Call(g,"Configure",JsonUtility.ToJson(new ActionConfig{left=left,right=right}));Call(g,"Begin","");Call(g,"ReviewPlace",p,e);}
 static float ArmFollowError(bool before){var g=Controller(before);var m=Field(Field(g,"player"),"model");Transform body=(Transform)Field(m,"Body"),arm=(Transform)Field(Weapon(g,"player",0),"root");Vector3 local=body.InverseTransformPoint(arm.position);Call(m,"Animate",1f,-1f,.24f,true);return Vector3.Distance(arm.position,body.TransformPoint(local));}
 static ActionState SimultaneousFinish(bool before){var g=Controller(before);Reset(g,"sword","cannon",Vector3.zero,new Vector3(0,0,2.15f));Put(Field(g,"enemy"),"hp",28f);Call(g,"AttackLeft","");Call(g,"AttackRight","");Put(Weapon(g,"player",0),"clock",.29f);Put(Weapon(g,"player",1),"clock",.175f);Step(g,1);return State(g);}
 static float InitialOverlap(bool before){var g=Controller(before);Reset(g,"sword","cannon",Vector3.zero,new Vector3(0,0,6));Transform tip=(Transform)Field(Weapon(g,"player",1),"tip");Vector3 at=tip.position;Call(g,"ReviewPlace",Vector3.zero,new Vector3(at.x,0,at.z));Call(g,"AttackRight","");Step(g,90);return State(g).enemyHp;}
 static float Telegraph(bool before){var g=Controller(before);Reset(g,"sword","cannon",new Vector3(0,0,-4),new Vector3(0,0,4));object enemy=Field(g,"enemy"),weapon=Weapon(g,"enemy",0);Call(g,"Attack",enemy,0);for(int i=1;i<=90;i++){Step(g,1);if((float)Field(weapon,"clock")>=.17f)return i/90f;}throw new Exception("Enemy never became active");}
 static float FinishTrail(bool before){var g=Controller(before);Reset(g,"sword","cannon",Vector3.zero,new Vector3(0,0,2.15f));Put(Field(g,"enemy"),"hp",28f);Call(g,"AttackLeft","");Put(Weapon(g,"player",0),"clock",.29f);Step(g,1);if(State(g).phase!="win")throw new Exception("Trail fixture must finish");return((LineRenderer)Field(Weapon(g,"player",0),"trail")).enabled?1:0;}
 public static void Run(){
  var fixes=new List<FixedCase>();for(int trial=1;trial<=3;trial++){int trialStart=fixes.Count;float oldArm=ArmFollowError(true),newArm=ArmFollowError(false);if(oldArm<.1f||newArm>.0001f)throw new Exception("Body-relative mount regression "+oldArm+" -> "+newArm);fixes.Add(new FixedCase{name="arm-follows-hit-and-down-body",before=oldArm,after=newArm});
  var oldFinish=SimultaneousFinish(true);var newFinish=SimultaneousFinish(false);if(oldFinish.phase!="win"||newFinish.phase!="win"||newFinish.eventName!="win"||oldFinish.eventName=="win")throw new Exception("Simultaneous finish fixture "+oldFinish.phase+"/"+oldFinish.eventName+" -> "+newFinish.phase+"/"+newFinish.eventName);fixes.Add(new FixedCase{name="simultaneous-lethal-attack-keeps-win-event",beforeEvent=oldFinish.eventName,afterEvent=newFinish.eventName});
  float oldOverlap=InitialOverlap(true),newOverlap=InitialOverlap(false);if(newOverlap!=76)throw new Exception("Visible muzzle overlap should hit once "+newOverlap);fixes.Add(new FixedCase{name="visible-projectile-initial-overlap",before=oldOverlap,after=newOverlap});
  float oldTell=Telegraph(true),newTell=Telegraph(false);if(newTell<.35f)throw new Exception("Enemy telegraph too short "+newTell);fixes.Add(new FixedCase{name="enemy-first-active-seconds",before=oldTell,after=newTell});
  float oldTrail=FinishTrail(true),newTrail=FinishTrail(false);if(oldTrail!=1||newTrail!=0)throw new Exception("Finished weapon trail must stop "+oldTrail+" -> "+newTrail);fixes.Add(new FixedCase{name="lethal-hit-trail-disabled",before=oldTrail,after=newTrail});for(int i=trialStart;i<fixes.Count;i++)fixes[i].trial=trial;}
  var g=(CrabActionGame)Controller(false);var cases=new List<MatrixCase>();
  foreach(string kind in new[]{"sword","cannon","creature"})foreach(int side in new[]{0,1})foreach(bool shouldHit in new[]{true,false}){
   float d=kind=="cannon"?6:kind=="creature"?2.7f:2.15f;Vector3 enemy=shouldHit?new Vector3(0,0,d):kind=="cannon"?new Vector3(5,0,6):new Vector3(0,0,7);
   Reset(g,kind,kind,Vector3.zero,enemy);if(side==0)g.AttackLeft("");else g.AttackRight("");Step(g,180);var s=g.State();float damage=kind=="cannon"?24:kind=="creature"?19:28;
   if(s.enemyHp!=(shouldHit?100-damage:100)||s.hits!=(shouldHit?1:0)||s.misses!=(shouldHit?0:1))throw new Exception("Matrix contact "+kind+" "+side+" "+shouldHit+" "+JsonUtility.ToJson(s));
   cases.Add(new MatrixCase{name=kind+"-"+side+(shouldHit?"-stationary-contact-once":"-whiff"),state=s});
  }
  foreach(string kind in new[]{"sword","cannon","creature"})foreach(int direction in new[]{-1,1}){
   Reset(g,kind,kind,Vector3.zero,new Vector3(0,0,3));g.Move(direction+",0");g.AttackLeft("");Step(g,180);g.Move("0,0");var s=g.State();float damage=kind=="cannon"?24:kind=="creature"?19:28;
   if(s.hits>1||(s.enemyHp!=100&&s.enemyHp!=100-damage)||Mathf.Abs(s.x)<.2f)throw new Exception("Max-speed movement duplicate/damage "+kind);
   cases.Add(new MatrixCase{name=kind+"-max-player-speed-strafe-"+direction,state=s});
  }
  Reset(g,"sword","sword",new Vector3(8,0,-1.5f),new Vector3(8,0,.65f));g.AttackLeft("");Step(g,90);if(g.State().hits!=1)throw new Exception("Wall melee must contact once");cases.Add(new MatrixCase{name="wall-edge-sword-contact",state=g.State()});
  Reset(g,"cannon","cannon",new Vector3(0,0,2),new Vector3(0,0,8.3f));g.AttackRight("");Step(g,180);if(g.State().hits!=1)throw new Exception("Wall projectile must contact once");cases.Add(new MatrixCase{name="wall-edge-cannon-contact",state=g.State()});
  if(cases.Count!=20)throw new Exception("Expected20conditions");Directory.CreateDirectory("../evidence");File.WriteAllText("../evidence/polish1-native-review.json",JsonUtility.ToJson(new Review{regressions=fixes.ToArray(),conditions=cases.ToArray()},true));
 }
 public static void ReviewAndWeb(){Run();ActionReview.Run();BuildAction.Prepare();var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName="../delivery/UnityWeb/polish1-release",target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});File.WriteAllText("../evidence/polish1-build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Polish WebGL failed");foreach(string extension in new[]{"data","framework.js","loader.js","wasm"})File.Move("../delivery/UnityWeb/polish1-release/Build/polish1-release."+extension,"../delivery/UnityWeb/polish1-release/Build/UnityWeb."+extension);foreach(string name in new[]{"index.html","host.js","sound.js"})File.Copy("../web-polish1/"+name,"../delivery/UnityWeb/polish1-release/"+name,true);}
}

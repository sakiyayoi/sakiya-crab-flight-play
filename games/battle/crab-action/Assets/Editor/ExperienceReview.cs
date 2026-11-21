using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
public static class ExperienceReview {
 [Serializable]public class Trial{public string revision,policy,result,left,right;public int fps,hits,misses,punishes,guards;public float hp,enemyHp,seconds;}
 [Serializable]public class Report{public string scope="Actual native controllers, same input policies/HP/equipment; candidate initial separation6 vs baseline8. Not human fun or touch performance.";public bool pauseFreezesRules,resumeWorks,resetWorks;public int errors;public Trial[] trials;}
 static CrabActionGame New(){var g=BuildAction.Prepare();g.Initialize();return g;}
 static string Move(Camera c,Vector3 d){var f=Vector3.ProjectOnPlane(c.transform.forward,Vector3.up).normalized;var r=Vector3.ProjectOnPlane(c.transform.right,Vector3.up).normalized;return Vector3.Dot(d,r).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+Vector3.Dot(d,f).ToString("F4",System.Globalization.CultureInfo.InvariantCulture);}
 static Trial Run(bool old,string policy,int fps,string l="sword",string r="cannon"){
  Exchange1BaselineGame b=null;CrabActionGame g=null;
  if(old){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);b=new GameObject("Frozen baseline").AddComponent<Exchange1BaselineGame>();b.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");b.groundShader=Shader.Find("Standard");b.Initialize();b.Configure(JsonUtility.ToJson(new ActionConfig{left=l,right=r}));b.Begin("");}
  else{g=New();g.Configure(JsonUtility.ToJson(new ActionConfig{left=l,right=r}));g.Begin("");}
  Func<ActionState>s=old?(Func<ActionState>)(()=>b.State()):()=>g.State();Action left=old?(Action)(()=>b.AttackLeft("")):()=>g.AttackLeft("");Action right=old?(Action)(()=>b.AttackRight("")):()=>g.AttackRight("");Action<float>tick=old?(Action<float>)(x=>b.Tick(x)):x=>g.Tick(x);var camera=old?b.ReviewCamera:g.ReviewCamera;
  for(int f=0;f<fps*40&&s().phase=="battle";f++){
   var v=s();
   if(policy=="stationary-both"){left();right();}
   else if(policy=="cue-exact-position"){
    var d=new Vector3(v.enemyX-v.x,0,v.enemyZ-v.z);float distance=d.magnitude;var dir=distance>.001f?d.normalized:Vector3.forward;var desired=Vector3.zero;
    if(v.enemyIntent.Contains("横へ")||v.enemyIntent=="踏み込み！")desired=new Vector3(dir.z,0,-dir.x);
    else if(v.enemyOpen){desired=distance>2.2f?dir:Vector3.zero;left();right();}else if(distance<2)desired=-dir;
    if(v.enemyOpen&&distance<=2.2f&&((int)Math.Floor(v.time/.08f))%2==0)desired=dir;
    if(old)b.Move(Move(camera,desired));else g.Move(Move(camera,desired));
   }
   tick(1f/fps);
  }
  var a=s();return new Trial{revision=old?"exchange1-ready":"experience2",policy=policy,fps=fps,left=l,right=r,result=a.phase,hp=a.hp,enemyHp=a.enemyHp,seconds=a.time,hits=a.hits,misses=a.misses,punishes=a.punishHits,guards=a.guardHits};
 }
 static void PauseContract(){
  var g=New();g.Begin("");if(Mathf.Abs(g.State().z+3)>.001f||Mathf.Abs(g.State().enemyZ-3)>.001f)throw new Exception("Pacing separation mismatch");g.Tick(.3f);g.Move("1,0");g.Pause("1");var before=g.State();g.AttackLeft("");g.Move("1,0");g.Tick(2);var after=g.State();
  if(!after.paused||after.time!=before.time||after.x!=before.x||after.z!=before.z||after.hp!=before.hp||after.enemyHp!=before.enemyHp||g.ReadProbe().lc>=0)throw new Exception("Pause must freeze all action and reject inputs");g.Pause("0");g.Tick(.1f);if(g.State().paused||g.State().time<=after.time)throw new Exception("Resume failed");g.Begin("");if(g.State().paused||g.State().hp!=100||g.State().hits!=0)throw new Exception("Retry reset failed");g.Pause("1");g.Customize("");if(g.State().paused||g.State().phase!="customize")throw new Exception("Home reset failed");
 }
 public static void ReviewAndWeb(){
  Directory.CreateDirectory("../evidence");var trials=new List<Trial>();int errors=0;
  Application.LogCallback log=(text,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;};Application.logMessageReceived+=log;
  try{ActionReview.Run();PauseContract();foreach(int fps in new[]{30,60,120})foreach(string p in new[]{"stationary-both","cue-exact-position"}){trials.Add(Run(true,p,fps));trials.Add(Run(false,p,fps));}foreach(string l in new[]{"sword","cannon","creature"})foreach(string r in new[]{"sword","cannon","creature"})trials.Add(Run(false,"cue-exact-position",60,l,r));}
  finally{Application.logMessageReceived-=log;File.WriteAllText("../evidence/experience2-native.json",JsonUtility.ToJson(new Report{trials=trials.ToArray(),errors=errors,pauseFreezesRules=errors==0,resumeWorks=errors==0,resetWorks=errors==0},true));}
  if(errors>0)throw new Exception("Native errors="+errors);
  var game=BuildAction.Prepare();var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");
  PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カスタムカニバトル・操作比較";PlayerSettings.bundleVersion="experience2";PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;
  const string dest="../delivery/UnityWeb/experience2";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/experience2-build.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Build failed");
  foreach(string ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/experience2."+ext,dest+"/Build/UnityWeb."+ext);
  foreach(string f in Directory.GetFiles("../web-template"))File.Copy(f,dest+"/"+Path.GetFileName(f),true);
 }
}

using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// Same actual controller/scene, fixed policies. These are not human playtests.
public static class CommitmentReview {
 [Serializable] public class Trial {
  public string revision,policy,left,right,result; public int fps,hits,misses,punishes,guards,enemyAttacks,maxConcurrentArms,acceptedInputFrames;
  public float hp,enemyHp,time,firstAttackTime=-1,lastAttackTime=-1;
 }
 [Serializable] public class Report {
  public string question="Can one committed strike preserve a counter window without slowing a deliberate exchange?";
  public string scope="Native Unity actual swept contacts; identical arena/HP/enemy. Input policies are fixtures, not human fun ratings.";
  public string changedAxis="Existing .65s action/recovery is shared across arms, no queued attack. Other rules unchanged.";
  public int errors; public Trial[] trials; public bool acceptanceContract,contactRegression;
 }
 static CrabActionGame NewGame(string left="sword",string right="cannon"){
  var g=BuildAction.Prepare();g.Initialize();g.Configure(JsonUtility.ToJson(new ActionConfig{left=left,right=right}));return g;
 }
 static Exchange1BaselineGame Baseline(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var g=new GameObject("Frozen exchange1 baseline").AddComponent<Exchange1BaselineGame>();g.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");g.groundShader=Shader.Find("Standard");g.Initialize();return g;
 }
 static string MoveInput(Camera c,Vector3 world){
  var forward=Vector3.ProjectOnPlane(c.transform.forward,Vector3.up).normalized;var right=Vector3.ProjectOnPlane(c.transform.right,Vector3.up).normalized;
  return Vector3.Dot(world,right).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+Vector3.Dot(world,forward).ToString("F4",System.Globalization.CultureInfo.InvariantCulture);
 }
 static void Grab(Camera c,string name){
  var rt=RenderTexture.GetTemporary(960,640,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var target=c.targetTexture;var active=RenderTexture.active;c.aspect=1.5f;c.targetTexture=rt;c.Render();RenderTexture.active=rt;
  var image=new Texture2D(960,640,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,640),0,0);image.Apply();File.WriteAllBytes("../evidence/"+name,image.EncodeToJPG(92));UnityEngine.Object.DestroyImmediate(image);c.targetTexture=target;RenderTexture.active=active;RenderTexture.ReleaseTemporary(rt);
 }
 static Trial RunTrial(bool old,string policy,int fps,string left="sword",string right="cannon"){
  var before=old?Baseline():null;var g=old?null:NewGame(left,right);
  Func<ActionState> state=old?(Func<ActionState>)(()=>before.State()):()=>g.State();
  Action attackLeft=old?(Action)(()=>before.AttackLeft("")):()=>g.AttackLeft("");
  Action attackRight=old?(Action)(()=>before.AttackRight("")):()=>g.AttackRight("");
  Action<float> tick=old?(Action<float>)(dt=>before.Tick(dt)):dt=>g.Tick(dt);
  var camera=old?before.ReviewCamera:g.ReviewCamera;
  if(old)before.Begin("");else g.Begin("");
  var t=new Trial{revision=old?"exchange1-ready":"commitment1",policy=policy,fps=fps,left=left,right=right};
  float prevLeft=-1,prevRight=-1;bool captured=false;
  for(int frame=0;frame<fps*40&&state().phase=="battle";frame++){
   var s=state();
   if(policy=="spam-both-every-frame"){attackLeft();attackRight();}
   else if(policy=="read-and-punish"){
    var delta=new Vector3(s.enemyX-s.x,0,s.enemyZ-s.z);float distance=delta.magnitude;var dir=distance>.001f?delta.normalized:Vector3.forward;var desired=Vector3.zero;
    if(s.enemyIntent.Contains("横へ")||s.enemyIntent=="踏み込み！")desired=new Vector3(dir.z,0,-dir.x);
    else if(s.enemyOpen){desired=distance>2.2f?dir:Vector3.zero;attackLeft();attackRight();}
    else if(distance<2)desired=-dir;
    if(s.enemyOpen&&distance<=2.2f&&((int)Math.Floor(s.time/.08f))%2==0)desired=dir;
    if(old)before.Move(MoveInput(camera,desired));else g.Move(MoveInput(camera,desired));
   }
   {
    float lc=old?before.ReadProbe().lc:g.ReadProbe().lc,rc=old?before.ReadProbe().rc:g.ReadProbe().rc;int concurrent=(lc>=0?1:0)+(rc>=0?1:0);t.maxConcurrentArms=Math.Max(t.maxConcurrentArms,concurrent);
    if((lc>=0&&(prevLeft<0||lc<prevLeft))||(rc>=0&&(prevRight<0||rc<prevRight))){t.acceptedInputFrames++;if(t.firstAttackTime<0)t.firstAttackTime=s.time;t.lastAttackTime=s.time;}
    prevLeft=lc;prevRight=rc;
   }
   tick(1f/fps);s=state();
   if(!captured&&!old&&fps==60&&left=="sword"&&right=="cannon"&&s.enemyOpen){Grab(camera,"commitment1-"+policy+"-recovery.jpg");captured=true;}
  }
  var f=state();t.result=f.phase;t.hp=f.hp;t.enemyHp=f.enemyHp;t.time=f.time;t.hits=f.hits;t.misses=f.misses;t.punishes=f.punishHits;t.guards=f.guardHits;t.enemyAttacks=f.enemyAttacks;
  return t;
 }
 static void AcceptanceContract(){
  var g=NewGame();g.aiEnabled=false;g.Begin("");g.ReviewPlace(Vector3.zero,new Vector3(0,0,7));
  g.AttackLeft("");g.AttackRight("");var p=g.ReadProbe();if(p.lc<0||p.rc>=0)throw new Exception("Two arms accepted on same frame");
  for(int i=0;i<45;i++){g.AttackRight("");g.Tick(1f/90);if(g.ReadProbe().rc>=0)throw new Exception("Recovery bypassed by opposite arm");}
  for(int i=0;i<25;i++)g.Tick(1f/90);
  if(g.ReadProbe().lc>=0||g.ReadProbe().rc>=0)throw new Exception("Ignored input was queued");
  g.AttackRight("");if(g.ReadProbe().rc<0)throw new Exception("Fresh post-recovery input ignored");
 }
 public static void Run(){
  Directory.CreateDirectory("../evidence");var trials=new List<Trial>();int errors=0;bool acceptance=false,contacts=false;
  Application.LogCallback log=(text,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;};Application.logMessageReceived+=log;
  try{
   foreach(int fps in new[]{30,60,120}){
    trials.Add(RunTrial(true,"spam-both-every-frame",fps));
    trials.Add(RunTrial(true,"read-and-punish",fps));
    trials.Add(RunTrial(false,"spam-both-every-frame",fps));
    trials.Add(RunTrial(false,"read-and-punish",fps));
   }
   trials.Add(RunTrial(false,"idle",60));
   AcceptanceContract();acceptance=true;ActionReview.Run();contacts=true;
   foreach(string a in new[]{"sword","cannon","creature"})foreach(string b in new[]{"sword","cannon","creature"}){
    trials.Add(RunTrial(false,"spam-both-every-frame",60,a,b));trials.Add(RunTrial(false,"read-and-punish",60,a,b));
   }
  }finally{
   Application.logMessageReceived-=log;
   File.WriteAllText("../evidence/commitment-review.json",JsonUtility.ToJson(new Report{trials=trials.ToArray(),errors=errors,acceptanceContract=acceptance,contactRegression=contacts},true));
  }
  if(errors>0)throw new Exception("Native errors="+errors);
 }
 public static void Web(){
  var game=BuildAction.Prepare();var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");
  PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カスタムカニバトル・一打比較";PlayerSettings.bundleVersion="commitment1";
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;
  const string dest="../delivery/UnityWeb/commitment1";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Build failed");
  foreach(string ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/commitment1."+ext,dest+"/Build/UnityWeb."+ext);
  foreach(string f in Directory.GetFiles("../web-template"))File.Copy(f,dest+"/"+Path.GetFileName(f),true);
 }
}

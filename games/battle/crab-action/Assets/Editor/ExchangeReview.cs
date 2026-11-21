using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// Deterministic Editor fixtures are evidence about rules, not human playtest.
public static class ExchangeReview {
 [Serializable] public class Trial {
  public string revision,policy,result; public int fps,hits,misses,punishes,guards,attacks;
  public float hp,enemyHp,time; public string[] frames;
 }
 [Serializable] public class Report {public string status;public string question="Does reading committed attacks create useful movement and punish choices?";public string scope="Native actual controller; scripted input policies, not browser/phone/human play";public Trial[] trials;public int errors;}
 static CrabActionGame NewGame(){var g=BuildAction.Prepare();g.Initialize();g.Configure("{\"left\":\"sword\",\"right\":\"cannon\"}");return g;}
 static OrganicBaselineGame OldGame(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var g=new GameObject("Organic baseline").AddComponent<OrganicBaselineGame>();g.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");g.groundShader=Shader.Find("Standard");g.Initialize();return g;
 }
 static void Grab(Camera camera,string filename){
  var rt=RenderTexture.GetTemporary(960,640,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
  var old=camera.targetTexture;var prior=RenderTexture.active;camera.aspect=1.5f;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
  var image=new Texture2D(960,640,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,640),0,0);image.Apply();File.WriteAllBytes(filename,image.EncodeToJPG(92));UnityEngine.Object.DestroyImmediate(image);
  camera.targetTexture=old;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);
 }
 static string MoveInput(Camera camera,Vector3 world){
  var forward=Vector3.ProjectOnPlane(camera.transform.forward,Vector3.up).normalized;var right=Vector3.ProjectOnPlane(camera.transform.right,Vector3.up).normalized;
  return Vector3.Dot(world,right).ToString("F4",System.Globalization.CultureInfo.InvariantCulture)+","+Vector3.Dot(world,forward).ToString("F4",System.Globalization.CultureInfo.InvariantCulture);
 }
 static Trial RunTrial(bool before,string policy,int fps){
  var old=before?OldGame():null;var game=before?null:NewGame();
  Func<ActionState> state=before?(Func<ActionState>)(()=>old.State()):()=>game.State();
  Action begin=before?(Action)(()=>old.Begin("")):()=>game.Begin("");
  Action left=before?(Action)(()=>old.AttackLeft("")):()=>game.AttackLeft("");
  Action right=before?(Action)(()=>old.AttackRight("")):()=>game.AttackRight("");
  Action<float> tick=before?(Action<float>)(dt=>old.Tick(dt)):dt=>game.Tick(dt);
  var camera=before?old.ReviewCamera:game.ReviewCamera;begin();var frames=new List<string>();bool windup=false,opening=false,twist=false;
  for(int frame=0;frame<fps*40&&state().phase=="battle";frame++){
   var s=state();
   if(policy=="spam") {left();right();}
   else if(policy=="read-and-punish") {
    Vector3 delta=new Vector3(s.enemyX-s.x,0,s.enemyZ-s.z);float distance=delta.magnitude;Vector3 dir=distance>.001f?delta.normalized:Vector3.forward;Vector3 desired=Vector3.zero;
    if(s.enemyIntent.Contains("横へ" )||s.enemyIntent=="踏み込み！")desired=new Vector3(dir.z,0,-dir.x);
    else if(s.enemyOpen){desired=distance>2.2f?dir:Vector3.zero;left();right();}
    else if(distance<2.0f)desired=-dir;
    // Turn by the existing movement control; no auto-aim or hidden pose setter.
    if(s.enemyOpen&&distance<=2.2f&&((int)Math.Floor(s.time/.08f))%2==0)desired=dir;
    game.Move(MoveInput(camera,desired));
   }
   tick(1f/fps);s=state();
   if(!before){
    string label=null;
    if(!windup&&s.enemyAttacks==1&&s.enemyIntent.Contains("横へ")){windup=true;label="teach-windup";}
    if(!opening&&s.enemyOpen){opening=true;label="test-recovery";}
    if(!twist&&s.enemyAttacks==3&&s.enemyIntent.Contains("伸びる")){twist=true;label="twist-reach";}
    if(label!=null){var name="exchange-frames/"+policy+"-"+fps+"-"+label+".jpg";Grab(camera,"../evidence/"+name);frames.Add(name);}
   }
  }
  var final=state();return new Trial{revision=before?"organic1":"exchange1",policy=policy,fps=fps,result=final.phase,hp=final.hp,enemyHp=final.enemyHp,time=final.time,hits=final.hits,misses=final.misses,punishes=final.punishHits,guards=final.guardHits,attacks=final.enemyAttacks,frames=frames.ToArray()};
 }
 public static void Run(){
  Directory.CreateDirectory("../evidence/exchange-frames");var trials=new List<Trial>();int errors=0;
  Application.LogCallback log=(text,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;};Application.logMessageReceived+=log;
  try{
   ActionReview.Run(); // Real contacts,9 swaps, rematch and no timed HP drain.
   foreach(int fps in new[]{30,60,120}){trials.Add(RunTrial(true,"spam",fps));trials.Add(RunTrial(false,"spam",fps));trials.Add(RunTrial(false,"read-and-punish",fps));}
   trials.Add(RunTrial(false,"idle",60));
  }finally{Application.logMessageReceived-=log;File.WriteAllText("../evidence/exchange-review.json",JsonUtility.ToJson(new Report{status=trials.Count==10&&errors==0?"COMPLETED_NATIVE_POLICY_OBSERVATIONS":"INCOMPLETE_OR_FAILED",trials=trials.ToArray(),errors=errors},true));}
  if(errors>0)throw new Exception("Exchange native errors="+errors);
 }
 public static void ReviewAndWeb(){
  Run();ContactsAtFrameRates();RenderedContactReview.Run();Polish3CueReview.Run();
  var game=BuildAction.Prepare();var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");
  PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カスタムカニバトル・交換実験";PlayerSettings.bundleVersion="exchange1";
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;
  PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;
  const string dest="../delivery/UnityWeb/exchange1-ready";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Exchange WebGL build requires success/errors0");
  foreach(string ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/exchange1-ready."+ext,dest+"/Build/UnityWeb."+ext);
  foreach(string file in Directory.GetFiles("../web-template"))File.Copy(file,dest+"/"+Path.GetFileName(file),true);
 }
 [Serializable] public class Contact {public string kind;public int fps,hits;public bool shouldHit;public float enemyHp,firstHitTime=-1;}
 [Serializable] public class ContactReport {public string scope="Same time0 attack input and static targets; actual swept queries at30/60/120 Tick grouping. Reactive policy timing is reported separately.";public Contact[] cases;}
 static void ContactsAtFrameRates(){
  var cases=new List<Contact>();
  foreach(string kind in new[]{"sword","cannon","creature"})foreach(bool hit in new[]{true,false}){
   float referenceTime=-1;
   foreach(int fps in new[]{30,60,120}){
    var g=NewGame();g.aiEnabled=false;g.Customize("");g.Configure(JsonUtility.ToJson(new ActionConfig{left=kind,right=kind}));g.Begin("");
    float d=kind=="cannon"?6:kind=="creature"?2.7f:2.15f;
    g.ReviewPlace(Vector3.zero,hit?new Vector3(0,0,d):kind=="cannon"?new Vector3(5,0,6):new Vector3(0,0,7));
    var c=new Contact{kind=kind,fps=fps,shouldHit=hit};g.ReviewObserver=(stage,a,b,r)=>{if(stage=="hit"&&c.firstHitTime<0)c.firstHitTime=g.State().time;};g.AttackLeft("");
    for(int frame=0;frame<2*fps;frame++)g.Tick(1f/fps);var s=g.State();c.hits=s.hits;c.enemyHp=s.enemyHp;g.ReviewObserver=null;
    if(c.hits!=(hit?1:0))throw new Exception("Frame grouping changes contact: "+kind+" "+fps);
    if(hit){if(referenceTime<0)referenceTime=c.firstHitTime;else if(Mathf.Abs(referenceTime-c.firstHitTime)>1f/90+.0001f)throw new Exception("Contact clock diverged: "+kind);}
    cases.Add(c);
   }
  }
  File.WriteAllText("../evidence/contact-frame-rates.json",JsonUtility.ToJson(new ContactReport{cases=cases.ToArray()},true));
 }
}

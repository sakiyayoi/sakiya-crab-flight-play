using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// One native regression/geometry review and one WebGL build entry for this candidate.
public static class TelegraphReview {
 [Serializable]public class Check {public string name;public bool pass;public string detail;}
 [Serializable]public class Report {public string scope="Unity native geometry fixtures and deterministic input-policy comparisons. Browser visibility, physical touch and moving-opponent hit guarantees are separate.";public string revision="crab-action-r1-telegraph1";public Check[] checks;public int unityErrors;}
 static readonly List<Check> checks=new List<Check>();
 static void Require(string name,bool okay,string detail){checks.Add(new Check{name=name,pass=okay,detail=detail});if(!okay)throw new Exception(name+": "+detail);}
 static CrabActionGame New(){var game=BuildAction.Prepare();game.Initialize();return game;}
 static bool Near(Vector3 a,Vector3 b,float tolerance=.002f){return Vector3.Distance(a,b)<=tolerance;}
 static void GeometryFixture(){
  var body=new GameObject("pure body fixture").transform;body.position=new Vector3(2,.7f,-1);body.rotation=Quaternion.Euler(0,33,0);
  var root=new GameObject("pure weapon fixture").transform;root.SetParent(body,false);Vector3 rest=new Vector3(.65f,0,.45f),scale=Vector3.one;Quaternion baseRotation=Quaternion.Euler(4,13,0);
  foreach(string kind in new[]{"sword","creature","cannon"})foreach(float clock in new[]{0f,.17f,.18f,.30f,.43f}){
   var pose=AttackGeometry.PoseAt(kind,clock,rest,baseRotation,scale,false,false);root.localPosition=pose.position;root.localRotation=pose.rotation;root.localScale=pose.scale;
   Vector3 tip=new Vector3(kind=="creature"?.15f:0,0,kind=="cannon"?1.9f:kind=="creature"?2.32f:2.2f);
   Matrix4x4 predicted=AttackGeometry.MatrixAt(body.localToWorldMatrix,kind,clock,rest,baseRotation,scale,false,false);
   var fromActual=AttackGeometry.Contact(kind,root.localToWorldMatrix,tip);var fromSample=AttackGeometry.Contact(kind,predicted,tip);
   Require("pose-geometry-"+kind+"-"+clock,Near(fromActual.shaftStart,fromSample.shaftStart)&&Near(fromActual.tip,fromSample.tip)&&fromActual.radius==fromSample.radius,"forecast matrix must produce the same shaft/tip/radius as the actual transform");
   if(kind=="cannon"){var actual=AttackGeometry.Cannon(root.localToWorldMatrix,tip);var sample=AttackGeometry.Cannon(predicted,tip);Require("muzzle-geometry-"+clock,Near(actual.muzzle,sample.muzzle)&&Near(actual.direction,sample.direction)&&Near(actual.direction,root.forward),"shot must start at visible tip and follow actual root forward");}
  }
  UnityEngine.Object.DestroyImmediate(body.gameObject);
 }
 struct Segment {public string stage;public Vector3 a,b;public float radius;}
 static void ForecastVsContact(string kind,bool right=false){
  var game=New();game.aiEnabled=false;game.Configure(JsonUtility.ToJson(new ActionConfig{left=right?"cannon":kind,right=right?kind:"cannon"}));game.Begin("");game.ReviewPlace(Vector3.zero,new Vector3(0,0,7));
  var planned=new List<Segment>();var actual=new List<Segment>();game.ReviewForecastObserver=(stage,a,b,r)=>{if(stage=="player:"+kind+":shaft"||stage=="player:cannon")planned.Add(new Segment{stage=stage,a=a,b=b,radius=r});};game.ReviewObserver=(stage,a,b,r)=>{if(stage==kind+":shaft"||stage=="cannon:travel")actual.Add(new Segment{stage=stage,a=a,b=b,radius=r});};
  if(right)game.AttackRight("");else game.AttackLeft("");game.Tick(1f/90);Require(kind+(right?"-right":"-left")+"-forecast-visible",game.ReviewVisibleForecastCount()>0&&planned.Count>0,"accepted attack must show a forecast before its active contact frame");
  game.ReviewForecastObserver=null;for(int i=0;i<70&&actual.Count==0;i++)game.Tick(1f/90);
  float closest=float.PositiveInfinity;
  // The rendered arc joins samples continuously. Native 90Hz active clocks do not
  // coincide with the 13 visual sample endpoints, so compare interpolated shafts.
  foreach(var hit in actual)for(int j=0;j<planned.Count-1;j++){
   var a=planned[j];var b=planned[j+1];var delta=b.b-a.b;
   float t=delta.sqrMagnitude>.000001f?Mathf.Clamp01(Vector3.Dot(hit.b-a.b,delta)/delta.sqrMagnitude):0;
   if(Mathf.Abs(hit.radius-a.radius)<.0001f)closest=Mathf.Min(closest,Mathf.Max(Vector3.Distance(hit.a,Vector3.Lerp(a.a,b.a,t)),Vector3.Distance(hit.b,Vector3.Lerp(a.b,b.b,t))));
  }
  Require(kind+(right?"-right":"-left")+"-stationary-contact-coverage",actual.Count>0&&closest<.04f,"actual query to interpolated forecast max endpoint distance="+closest+"m; tolerance .04m covers animated body bob and chord approximation, not collision-radius inflation");
 }
 static void CannonVsShot(){
  var game=New();game.aiEnabled=false;game.Begin("");game.ReviewPlace(Vector3.zero,new Vector3(0,0,7));
  var planned=new List<Segment>();var actual=new List<Segment>();game.ReviewForecastObserver=(stage,a,b,r)=>{if(stage=="player:cannon")planned.Add(new Segment{stage=stage,a=a,b=b,radius=r});};game.ReviewObserver=(stage,a,b,r)=>{if(stage=="cannon:travel")actual.Add(new Segment{stage=stage,a=a,b=b,radius=r});};
  game.AttackRight("");game.Tick(1f/90);Require("cannon-forecast-visible",game.ReviewVisibleForecastCount()>0&&planned.Count>0,"cannon line must exist during charge");game.ReviewForecastObserver=null;
  for(int i=0;i<70&&actual.Count==0;i++)game.Tick(1f/90);
  bool match=false;foreach(var shot in actual)foreach(var line in planned)if(Mathf.Abs(shot.radius-line.radius)<.0001f&&Near(shot.a,line.a,.11f)&&Vector3.Dot((shot.b-shot.a).normalized,(line.b-line.a).normalized)>.995f)match=true;
  Require("cannon-muzzle-and-direction",actual.Count>0&&match,"first actual shot must start on forecast muzzle and use forecast direction");
 }
 static void EnemyLungeForecast(){
  var game=New();game.Begin("");game.ReviewPlace(Vector3.zero,new Vector3(0,0,2.9f));var starts=new List<Vector3>();
  game.ReviewForecastObserver=(stage,a,b,r)=>{if(stage=="enemy:sword:shaft")starts.Add(a);};
  for(int i=0;i<180&&starts.Count<13;i++)game.Tick(1f/90);
  float span=0;if(starts.Count>=13)for(int i=1;i<13;i++)span=Mathf.Max(span,Vector3.Distance(starts[0],starts[i]));
  Require("enemy-committed-lunge-preview",starts.Count>=13&&span>.35f&&game.ReviewVisibleForecastCount()>0,"enemy sword must show a sampled reach that includes forward lunge, not a fixed straight arrow");
 }
 static ActionState Policy(bool visible,string loadout,string position,string movement,float frame=1f/90){
  var game=New();game.aiEnabled=false;game.Configure(JsonUtility.ToJson(new ActionConfig{left=loadout,right="cannon"}));game.Begin("");game.ForecastVisibility(visible?"1":"0");
  Vector3 player=position=="wall"?new Vector3(7.1f,0,-1):Vector3.zero;Vector3 enemy=position=="wall"?new Vector3(7.9f,0,1.9f):new Vector3(0,0,2.7f);game.ReviewPlace(player,enemy);game.Move(movement);game.AttackLeft("");game.AttackRight("");
  for(int i=0;i<Mathf.CeilToInt(2f/frame);i++)game.Tick(frame);Require("toggle-visible-"+visible+"-"+loadout+"-"+position+"-"+frame,visible||game.ReviewVisibleForecastCount()==0,"OFF must leave no visible lines");return game.State();
 }
 static void PolicyParity(){foreach(string kind in new[]{"sword","creature"})foreach(string position in new[]{"center","wall"})foreach(string move in new[]{"0,0","1,0"}){
  var on=Policy(true,kind,position,move);var off=Policy(false,kind,position,move);
  Require("policy-parity-"+kind+"-"+position+"-"+move,on.phase==off.phase&&on.hp==off.hp&&on.enemyHp==off.enemyHp&&on.hits==off.hits&&on.misses==off.misses&&Near(new Vector3(on.x,0,on.z),new Vector3(off.x,0,off.z),.0001f),"ON/OFF must not change HP, hit count, result or movement");
 }}
 static void BurstFrameParity(){var on=Policy(true,"sword","wall","1,0",.08f);var off=Policy(false,"sword","wall","1,0",.08f);Require("burst-frame-policy-parity",on.phase==off.phase&&on.hp==off.hp&&on.enemyHp==off.enemyHp&&on.hits==off.hits&&on.misses==off.misses&&Near(new Vector3(on.x,0,on.z),new Vector3(off.x,0,off.z),.0001f),"large input-frame bursts must keep same collision policy with lines ON/OFF");}
 static void Capture(CrabActionGame game,string file,int width,int height){
  var camera=game.ReviewCamera;camera.aspect=(float)width/height;game.ReviewRefreshCamera();
  var target=new RenderTexture(width,height,24);var before=RenderTexture.active;camera.targetTexture=target;camera.Render();RenderTexture.active=target;
  var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();File.WriteAllBytes(file,pixels.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=before;UnityEngine.Object.DestroyImmediate(pixels);UnityEngine.Object.DestroyImmediate(target);
 }
 static void RenderComparison(){
  string folder="../evidence/telegraph-render";Directory.CreateDirectory(folder);
  foreach(string kind in new[]{"sword","cannon","creature"}){
   var game=New();game.Configure(JsonUtility.ToJson(new ActionConfig{left=kind,right="cannon"}));game.Begin("");game.ReviewPlace(Vector3.zero,new Vector3(0,0,2.9f));
   for(int i=0;i<180&&game.ReviewVisibleForecastCount()==0;i++)game.Tick(1f/90);
   game.AttackLeft("");game.AttackRight("");game.Tick(1f/90);Require("simultaneous-lines-"+kind,game.ReviewVisibleForecastCount()>=2,"player and enemy attack forecasts must be present together");
   var before=game.State();Capture(game,folder+"/"+kind+"-on-portrait.png",390,844);Capture(game,folder+"/"+kind+"-on-landscape.png",844,390);
   game.ForecastVisibility("0");Capture(game,folder+"/"+kind+"-off-portrait.png",390,844);Capture(game,folder+"/"+kind+"-off-landscape.png",844,390);var after=game.State();
   Require("render-only-no-rules-"+kind,before.time==after.time&&before.hp==after.hp&&before.enemyHp==after.enemyHp,"same scene/pose/HP/time for lines ON and OFF render comparison");
  }
 }
 public static void ReviewAndWeb(){
  Directory.CreateDirectory("../evidence");checks.Clear();int errors=0;Application.LogCallback log=(message,stack,type)=>{if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors++;};Application.logMessageReceived+=log;
  try{GeometryFixture();ForecastVsContact("sword");ForecastVsContact("creature");ForecastVsContact("sword",true);ForecastVsContact("creature",true);CannonVsShot();EnemyLungeForecast();PolicyParity();BurstFrameParity();RenderComparison();ActionReview.Run();Require("unity-errors",errors==0,"native error/exception/assert count="+errors);}
  finally{Application.logMessageReceived-=log;File.WriteAllText("../evidence/telegraph-native.json",JsonUtility.ToJson(new Report{checks=checks.ToArray(),unityErrors=errors},true));}
  var game=BuildAction.Prepare();var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");
  PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カスタムカニバトル・予測線比較";PlayerSettings.bundleVersion="telegraph1";PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;
  const string dest="../delivery/UnityWeb/telegraph1";Directory.CreateDirectory(dest);
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/telegraph-build.txt",result.summary.result+" bytes="+result.summary.totalSize+" errors="+result.summary.totalErrors+" duration="+result.summary.totalTime);
  if(result.summary.result!=BuildResult.Succeeded||result.summary.totalErrors!=0)throw new Exception("Telegraph WebGL build failed");
  foreach(string ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/telegraph1."+ext,dest+"/Build/UnityWeb."+ext);
  foreach(string file in Directory.GetFiles("../web-template"))File.Copy(file,dest+"/"+Path.GetFileName(file),true);
 }
}

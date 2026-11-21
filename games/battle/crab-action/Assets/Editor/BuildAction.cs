using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
public static class BuildAction{
 public static CrabActionGame Prepare(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var game=new GameObject("CrabActionGame").AddComponent<CrabActionGame>();game.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");game.groundShader=Shader.Find("Standard");if(!game.crabShader||!game.groundShader)throw new System.Exception("Required shaders missing");EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");return game;}
 public static void ReviewOnly(){ActionComparison.Run();ActionReview.Run();}
 public static void ReviewAndWeb(){ActionComparison.Run();ActionReview.Run();Web();}
 public static void Web(){Prepare();PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カスタムカニバトル・アクション候補";PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=800;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;Directory.CreateDirectory("../delivery");Directory.CreateDirectory("../evidence");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName="../delivery/UnityWeb",target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});File.WriteAllText("../evidence/build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Action WebGL build failed");foreach(var name in new[]{"index.html","host.js","sound.js"})File.Copy("../web-template/"+name,"../delivery/UnityWeb/"+name,true);}
}

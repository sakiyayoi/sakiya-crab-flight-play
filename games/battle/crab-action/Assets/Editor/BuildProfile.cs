using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildProfile {
 public static void Web(){var g=BuildAction.Prepare();var p=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();p.game=g;UnityEditor.SceneManagement.EditorSceneManager.SaveScene(g.gameObject.scene,"Assets/ActionArena.unity");const string dest="../delivery/UnityWeb/profile-p2";var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});File.WriteAllText("../evidence/profile-build.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Observation build failed");foreach(var ext in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/profile-p2."+ext,dest+"/Build/UnityWeb."+ext);foreach(var f in Directory.GetFiles("../web-polish2"))File.Copy(f,dest+"/"+Path.GetFileName(f),true);}
}

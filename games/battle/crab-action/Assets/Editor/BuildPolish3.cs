using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;

public static class BuildPolish3 {
 public static void ReviewAndWeb(){
  // Native fixtures cover contacts/loadouts and cue timing. They are regressions,
  // not input-to-render or browser performance measurements.
  ActionReview.Run();PolishActionReview.Run();RenderedContactReview.Run();CameraVisibilityReview.Run();Polish3CueReview.Run();
  var game=BuildAction.Prepare();
  var probe=new GameObject("ActionFrameProbe").AddComponent<ActionFrameProbe>();probe.game=game;
  EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/ActionArena.unity");
  PlayerSettings.bundleVersion="crab-action-r1-polish3";
  const string dest="../delivery/UnityWeb/polish3";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/polish3-build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Polish3 build requires successful report without errors");
  foreach(string extension in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/polish3."+extension,dest+"/Build/UnityWeb."+extension);
  foreach(var file in Directory.GetFiles("../web-polish3"))File.Copy(file,dest+"/"+Path.GetFileName(file),true);
 }
}

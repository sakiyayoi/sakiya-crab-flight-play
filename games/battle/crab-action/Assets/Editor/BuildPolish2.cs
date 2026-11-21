using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
public static class BuildPolish2 {
 public static void ReviewAndWeb(){
  ActionReview.Run();PolishActionReview.Run();RenderedContactReview.Run();CameraVisibilityReview.Run();
  BuildAction.Prepare();PlayerSettings.bundleVersion="crab-action-r1-polish2";
  const string dest="../delivery/UnityWeb/polish2";Directory.CreateDirectory(dest);
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/ActionArena.unity"},locationPathName=dest,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText("../evidence/polish2-build-result.txt",report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded||report.summary.totalErrors!=0)throw new Exception("Polish2 build requires successful report without errors");
  foreach(string extension in new[]{"data","framework.js","loader.js","wasm"})File.Move(dest+"/Build/polish2."+extension,dest+"/Build/UnityWeb."+extension);
  foreach(var file in Directory.GetFiles("../web-polish2"))File.Copy(file,dest+"/"+Path.GetFileName(file),true);
 }
}

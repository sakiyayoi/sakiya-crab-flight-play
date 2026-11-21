using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

// New restoration entry point, not a recovered r49 source file.
public static class ReconstructionBuildR1 {
 const string Version = "kani-recon-r32-ceiling49-r1";
 [Serializable] class Receipt { public string version=Version, classification="Partial reconstruction from actual r32, not original or equivalent r49", result; public int errors,warnings; public long bytes; }
 public static void Web() {
  var project=Directory.GetParent(Application.dataPath).FullName;
  var output=Path.GetFullPath(Path.Combine(project,"../../../../web-r1"));
  var evidence=Path.GetFullPath(Path.Combine(project,"../../../../evidence"));
  Directory.CreateDirectory(evidence);
  var g=BuildFlight.PrepareScene();
  EditorSceneManager.SaveScene(g.gameObject.scene,"Assets/FlightField.unity");
  PlayerSettings.bundleVersion=Version;
  PlayerSettings.companyName="SakiyaPrototype";
  PlayerSettings.productName="カニとばし・再構成R1";
  PlayerSettings.defaultWebScreenWidth=1280;
  PlayerSettings.defaultWebScreenHeight=800;
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
  PlayerSettings.WebGL.decompressionFallback=false;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes=new[]{"Assets/FlightField.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.None });
  File.WriteAllText(Path.Combine(evidence,"build01-result.json"),JsonUtility.ToJson(new Receipt { result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize },true));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Restoration candidate WebGL build failed");
  foreach(var file in Directory.GetFiles("../web-template"))File.Copy(file,Path.Combine(output,Path.GetFileName(file)),true);
  File.WriteAllText(Path.Combine(output,"RESTORATION-NOTE.txt"),"Partial restoration candidate: r32 source plus the available r49 ceiling-sign statement. Not recovered original r49. Missing r33-r48 Unity source/Assets are not synthesized. V4/new model not applied.\n");
 }
}

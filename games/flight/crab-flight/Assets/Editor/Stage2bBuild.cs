using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEditor.Build.Reporting;
public static class RotorBurstBuild {
 public static void Web(){
  var g=BuildFlight.PrepareScene();var panel=g.gameObject.AddComponent<FlightTuningPanel>();panel.inspectorPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Plugins/RuntimeInspector/RuntimeInspector.prefab");if(!panel.inspectorPrefab)throw new Exception("Inspector prefab unavailable");
  EditorSceneManager.SaveScene(g.gameObject.scene,"Assets/FlightField.unity");
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=false;
  var target="../delivery/UnityWeb/ceiling-r9";Directory.CreateDirectory(target);var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=target,target=BuildTarget.WebGL,options=BuildOptions.None});
  Directory.CreateDirectory("../evidence");File.WriteAllText("../evidence/web-build-result.json",JsonUtility.ToJson(new Result{state=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize},true));if(report.summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed");
 }
 [Serializable]class Result{public string state;public int errors,warnings;public long bytes;}
}

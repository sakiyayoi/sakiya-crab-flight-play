using System;using System.IO;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEditor.Build.Reporting;
public static class FloorBuildR10 {
 public static void R27Feel(){PlayerSettings.bundleVersion="kani-v0.7-retry-r32m";Build("kani-v0.7-retry-r32m",false);}
 public static void PlayableV1(){PlayerSettings.bundleVersion="kani-v0.7-retry-r32m";Build("kani-v0.7-retry-r32m",false);}
 public static void R24ContactProbe(){PlayerSettings.bundleVersion="crab-flight-r24-contact-probe";Build("crab-flight-r24-contact-probe",true);}
 public static void R23InputQA(){PlayerSettings.bundleVersion="crab-flight-r23-input-ack3";Build("crab-flight-r23-input-ack3",false);}
 public static void R11Diagnostics(){Build("floor-cause-r11",true);}
 public static void R11Release(){Build("world-r11",false);}
 public static void R12Release(){Build("world-r12",false);}
 public static void R13Release(){Build("world-r13",false);}
 public static void R17Diagnostic(){Build("world-r17-diag",true);}
 public static void R19VfxRelease(){Build("world-r19-vfx1",false);}
 public static void R19Release(){Build("world-r19",false);}
 public static void R18Release(){Build("world-r18",false);}
 public static void R17Release(){Build("world-r17",false);}
 public static void R16Release(){Build("world-r16",false);}
 public static void R15Release(){Build("world-r15",false);}
 public static void R14Release(){Build("world-r14",false);}
 public static void Diagnostic(){Build("floor-probe-r10",true);}
 public static void Release(){Build("world-r10",false);}
 static void Build(string name,bool diagnostic){var g=BuildFlight.PrepareScene();EditorSceneManager.SaveScene(g.gameObject.scene,"Assets/FlightField.unity");PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=false;var target="../delivery/UnityWeb/"+name;Directory.CreateDirectory(target);var o=new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=target,target=BuildTarget.WebGL,options=BuildOptions.None,extraScriptingDefines=diagnostic?new[]{"FLOOR_DIAGNOSTICS"}:new string[0]};var report=BuildPipeline.BuildPlayer(o);Directory.CreateDirectory("../evidence");File.WriteAllText("../evidence/"+name+"-build.json",JsonUtility.ToJson(new Result{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize},true));if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Web build failed");}
 [Serializable]class Result{public string result;public int errors,warnings;public long bytes;}
}

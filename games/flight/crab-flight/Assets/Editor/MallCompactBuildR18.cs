using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;

public static class MallCompactBuildR18 {
 [Serializable]class SourceStamp{public string commit,sha256,scope="Build-time C# identity; final source snapshot also binds generated scene, metadata and resources";}
 [Serializable] class Receipt {public string version="kani-r18-mall-compact-20261112",classification="New bold king-crab prototype on a reconstructed r32 base, not original r49 or V4 as-is",result;public int errors,warnings;public long bytes;}
 public static void Web(){
  var project=Directory.GetParent(Application.dataPath).FullName;
  var source=Path.GetFullPath(Path.Combine(project,"../../.."));
  var output=Path.GetFullPath(Path.Combine(source,"../private-web/candidate"));
  var evidence=Path.GetFullPath(Path.Combine(source,"../evidence/web-build01"));Directory.CreateDirectory(evidence);
  var info=new System.Diagnostics.ProcessStartInfo("/usr/bin/git","rev-parse HEAD"){WorkingDirectory=source,RedirectStandardOutput=true,RedirectStandardError=true,UseShellExecute=false};string commit;
  using(var git=System.Diagnostics.Process.Start(info)){commit=git.StandardOutput.ReadToEnd().Trim();git.WaitForExit();if(git.ExitCode!=0||commit.Length!=40)throw new Exception("Cannot bind source commit for this build");}
  string codeHash;using(var sha=System.Security.Cryptography.SHA256.Create())codeHash=BitConverter.ToString(sha.ComputeHash(Directory.GetFiles(Application.dataPath,"*.cs",SearchOption.AllDirectories).OrderBy(x=>x,StringComparer.Ordinal).SelectMany(File.ReadAllBytes).ToArray())).Replace("-","").ToLowerInvariant();
  File.WriteAllText(Path.Combine(Application.dataPath,"Resources/SourceIdentityR21.json"),JsonUtility.ToJson(new SourceStamp{commit=commit,sha256=codeHash},true));AssetDatabase.ImportAsset("Assets/Resources/SourceIdentityR21.json",ImportAssetOptions.ForceUpdate);
  var game=BuildFlight.PrepareScene();EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/FlightField.unity");
  PlayerSettings.bundleVersion="kani-r18-mall-compact-20261112";PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カニとばし・大胆タラバR2";
  PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=800;
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=false;
  var build=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText(Path.Combine(evidence,"build01-result.json"),JsonUtility.ToJson(new Receipt{result=build.summary.result.ToString(),errors=build.summary.totalErrors,warnings=build.summary.totalWarnings,bytes=(long)build.summary.totalSize},true));
  if(build.summary.result!=BuildResult.Succeeded)throw new Exception("King-bold WebGL build failed");
  foreach(var file in Directory.GetFiles(Path.Combine(source,"games/flight/web-template")))File.Copy(file,Path.Combine(output,Path.GetFileName(file)),true);
  foreach(var file in Directory.GetFiles(output)){var ext=Path.GetExtension(file);if(ext==".js"||ext==".html"||ext==".txt")File.WriteAllText(file,File.ReadAllText(file).Replace("kani-finish-r18","kani-r18-mall-compact-20261112"));}
  var loaders=Directory.GetFiles(Path.Combine(output,"Build"),"*.loader.js");if(loaders.Length!=1)throw new Exception("Unexpected loader count");
  var builtName=Path.GetFileName(loaders[0]).Replace(".loader.js","");var host=Path.Combine(output,"host.js");File.WriteAllText(host,File.ReadAllText(host).Replace("Build/kani-r18-mall-compact-20261112.","Build/"+builtName+"."));
  foreach(var relative in new[]{"recovery/runtime-assets/SoundPack","voice"}){
   var input=Path.Combine(source,relative);var target=Path.Combine(output,relative=="voice"?"voice":"SoundPack");
   foreach(var file in Directory.GetFiles(input,"*",SearchOption.AllDirectories)){var to=Path.Combine(target,Path.GetRelativePath(input,file));Directory.CreateDirectory(Path.GetDirectoryName(to));File.Copy(file,to,true);}
  }
  File.Copy(Path.Combine(source,"CREDITS.md"),Path.Combine(output,"CREDITS.md"),true);
  File.WriteAllText(Path.Combine(output,"PROTOTYPE-NOTE.txt"),"Private R18 target-only pending visual Collider lifecycle diagnostic. Enable only at localhost ?primitiveTrace=1. No gameplay collision, destruction or sleep change. Private R18 main/debris Scene timing diagnostic. Existing elapsed ticks partitioned without extra clock reads; full flight recorded in preallocated8192 rows with explicit dropped count. This is not a performance fix or formal10 pass. Private R18 frame-distributed preparation experiment. Fresh physics scenes/contacts/actors and original precision/quality/chains retained. Renderer clone/shape creation/body setup work is spread after Retry and before ready; no work removed from full timing window, no hidden warmup or first-throw delay. Total preparation CPU is not promised lower; ready latency may grow. Native original consecutive2 physical streams/actor meshes exact and deferred input/Pause/reset boundaries checked; WebGL performance acceptance still pending. R16 world reuse remains rejected. R17 immutable life display mesh cache retained. Shake/audio defaultsOFF. Old616.6/frame stalls/M14/iPhone/listening unresolved or unconfirmed. Partial reconstructed source, not recovered original r49. No public deployment.\n");
 }
}

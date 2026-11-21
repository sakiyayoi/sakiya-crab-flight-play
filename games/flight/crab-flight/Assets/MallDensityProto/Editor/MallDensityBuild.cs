using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEditor.Build.Reporting;
public static class MallDensityBuild {
 [Serializable]class Stamp{public string commit="isolated-density-prototype",sha256,scope="All C# at build time. Derived from r18-mall-compact working source; not an adopted commit.";}
 [Serializable]class Receipt{public string version=MallDensityProto.Revision,result,sourceSHA256;public int errors,warnings;public long bytes;public double seconds;}
 public static void Web(){
  MallDensityProto.Enabled=true;var root=Path.GetFullPath("../../../..");var source=Path.GetFullPath("../../..");var output=root+"/WebGL";if(Directory.Exists(root+"/evidence/build02"))throw new Exception("Use a fresh build evidence directory");Directory.CreateDirectory(root+"/evidence/build02");
  string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Directory.GetFiles(Application.dataPath,"*.cs",SearchOption.AllDirectories).OrderBy(x=>x,StringComparer.Ordinal).SelectMany(File.ReadAllBytes).ToArray())).Replace("-","").ToLowerInvariant();
  File.WriteAllText("Assets/Resources/SourceIdentityR21.json",JsonUtility.ToJson(new Stamp{sha256=hash},true));AssetDatabase.ImportAsset("Assets/Resources/SourceIdentityR21.json",ImportAssetOptions.ForceUpdate);
  var game=BuildFlight.PrepareScene();EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/FlightField.unity");
  PlayerSettings.bundleVersion=MallDensityProto.Revision;PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カニとばし・モール増量試作";
  PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=800;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=false;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=output,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText(root+"/evidence/build02/result.json",JsonUtility.ToJson(new Receipt{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,sourceSHA256=hash},true));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed");
  foreach(var file in Directory.GetFiles(source+"/games/flight/web-template"))File.Copy(file,output+"/"+Path.GetFileName(file),true);
  var loader=Directory.GetFiles(output+"/Build","*.loader.js").Single();var name=Path.GetFileName(loader).Replace(".loader.js","");
  foreach(var f in Directory.GetFiles(output)){if(new[]{".js",".html",".txt"}.Contains(Path.GetExtension(f)))File.WriteAllText(f,File.ReadAllText(f).Replace("kani-finish-r18",MallDensityProto.Revision).Replace("Build/"+MallDensityProto.Revision+".","Build/"+name+"."));}
  foreach(var relative in new[]{"recovery/runtime-assets/SoundPack","voice"}){var input=source+"/"+relative;var target=output+"/"+(relative=="voice"?"voice":"SoundPack");foreach(var file in Directory.GetFiles(input,"*",SearchOption.AllDirectories)){var to=target+"/"+Path.GetRelativePath(input,file);Directory.CreateDirectory(Path.GetDirectoryName(to));File.Copy(file,to,true);}}
  File.Copy(source+"/CREDITS.md",output+"/CREDITS.md",true);File.WriteAllText(output+"/PROTOTYPE-NOTE.txt","Isolated mall density prototype. Added targets only in x=[28,135). Original physics and shop retained. Local use only; not published. See ../PROTO-REPORT2.md.\n");
 }
}

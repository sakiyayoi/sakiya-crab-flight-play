using System;using System.IO;using System.Linq;using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using UnityEditor.Build.Reporting;
public static class NeonProtoBuild {
 [Serializable]class Stamp{public string commit="isolated-neon-prototype",sha256,scope="All C# at build time. Derived from r18-mall-compact working source; not an adopted commit.";}
 [Serializable]class Receipt{public string version=NeonProto.Revision,result,sourceSHA256;public int errors,warnings;public long bytes;public double seconds;}
 public static void PortPrototype(){Build("port-prototype-build01",BuildOptions.None);}
 public static void Web(){Build("build-neon08",BuildOptions.None);}
 public static void Recovery(){Build("build-neon10",BuildOptions.CleanBuildCache);}
 public static void Stage45(){Build("stage45-build01",BuildOptions.None);}
 public static void Stage6(){Build("stage6-build01",BuildOptions.None);}
 public static void Stage65Tune(){Build(Environment.GetEnvironmentVariable("NEON_STAGE65_TUNE_BUILD")??"stage65-tune-build01",BuildOptions.None);}
 public static void Stage65(){Build("stage65-build01",BuildOptions.None);}
 public static void Stage5(){Build("stage5-build01",BuildOptions.None);}
 public static void Stage46(){Build("stage46-build01",BuildOptions.None);}
 static void Build(string evidenceName,BuildOptions options){
  NeonProto.Enabled=true;MallDensityProto.Enabled=true;var root=Path.GetFullPath("../../../..");var source=Path.GetFullPath("../../..");bool port=evidenceName.StartsWith("port-prototype-");string version=port?PortLayout.Revision:NeonProto.Revision;var output=root+(port?"/WebGL-port-prototype":evidenceName.StartsWith("stage65-tune-")?"/WebGL-stage65":evidenceName.StartsWith("stage65-")?"/WebGL-stage65":evidenceName.StartsWith("stage6-")?"/WebGL-stage6":evidenceName.StartsWith("stage5-")?"/WebGL-stage5":evidenceName.StartsWith("stage46-")?"/WebGL-stage46":"/WebGL-stage45");if(Directory.Exists(output))throw new Exception("Refuse to overwrite existing WebGL output");if(new DriveInfo(root).AvailableFreeSpace<5L*1024*1024*1024)throw new Exception("Less than 5 GiB free; build stopped");if(Directory.Exists(root+"/evidence/"+evidenceName))throw new Exception("Use a fresh build evidence directory");Directory.CreateDirectory(root+"/evidence/"+evidenceName);
  string hash;using(var sha=System.Security.Cryptography.SHA256.Create())hash=BitConverter.ToString(sha.ComputeHash(Directory.GetFiles(Application.dataPath,"*.cs",SearchOption.AllDirectories).OrderBy(x=>x,StringComparer.Ordinal).SelectMany(File.ReadAllBytes).ToArray())).Replace("-","").ToLowerInvariant();
  File.WriteAllText("Assets/Resources/SourceIdentityR21.json",JsonUtility.ToJson(new Stamp{sha256=hash},true));AssetDatabase.ImportAsset("Assets/Resources/SourceIdentityR21.json",ImportAssetOptions.ForceUpdate);
  var game=BuildFlight.PrepareScene();if(evidenceName.StartsWith("stage65-tune-"))game.gameObject.AddComponent<FlightTuningPanel>();EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/FlightField.unity");
  PlayerSettings.bundleVersion=version;PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カニとばし・ネオン試作";
  PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=800;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.decompressionFallback=false;
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=output,target=BuildTarget.WebGL,options=options});
  File.WriteAllText(root+"/evidence/"+evidenceName+"/result.json",JsonUtility.ToJson(new Receipt{version=version,result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=(long)report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,sourceSHA256=hash},true));
  if(report.summary.result!=BuildResult.Succeeded)throw new Exception("WebGL build failed");
  foreach(var file in Directory.GetFiles(source+"/games/flight/web-template"))File.Copy(file,output+"/"+Path.GetFileName(file),true);
  var loader=Directory.GetFiles(output+"/Build","*.loader.js").Single();var name=Path.GetFileName(loader).Replace(".loader.js","");
  foreach(var f in Directory.GetFiles(output)){if(new[]{".js",".html",".txt"}.Contains(Path.GetExtension(f)))File.WriteAllText(f,File.ReadAllText(f).Replace("kani-finish-r18",version).Replace("Build/"+version+".","Build/"+name+"."));}
  foreach(var relative in new[]{"recovery/runtime-assets/SoundPack","voice","games/flight/web-template/fonts"}){var input=source+"/"+relative;var target=output+"/"+Path.GetFileName(relative);foreach(var file in Directory.GetFiles(input,"*",SearchOption.AllDirectories)){var to=target+"/"+Path.GetRelativePath(input,file);Directory.CreateDirectory(Path.GetDirectoryName(to));File.Copy(file,to,true);}}
  File.AppendAllText(output+"/index.html","\n<!-- "+(port?"Port prototype 600-900m; dynamic camera, role effects and visual tokens v2":evidenceName.StartsWith("stage65-tune-")?"Neon stage6.5 real-screen tuning mode: tune=1, 19 sliders, live feel and between-throw physics, feel.json export":evidenceName.StartsWith("stage65-")?"Neon stage6.5 feel particleMultiplier 4.0 proof build":"Neon stage6 ten new breakable families and decreasing density; tidy sparks retained")+"; sourceSHA256="+hash+" -->\n");
  File.Copy(source+"/CREDITS.md",output+"/CREDITS.md",true);File.WriteAllText(output+"/PROTOTYPE-NOTE.txt",port?"港区画プロトタイプ。+x 600-900m、役・動的カメラ・視覚v2。実装報告と同版のNative検査を参照。ビルド成功はゲーム品質合格ではありません。\n":"Isolated neon display prototype. Eight render-only legs, additive baked glow, 64 signs across nine sectors. Stage 6: ten new destructible families and graded +x density; compact sparks and spin-gated bubbles/Ghost removal retained; no spin arcs, 24 pooled sparks, back glow and authored extra breakables outside mall. No post-process Bloom. Original physics, input and shop retained. Local use only; not published. See ../NEON-REPORT-STAGE6.md.\n");
 }
}

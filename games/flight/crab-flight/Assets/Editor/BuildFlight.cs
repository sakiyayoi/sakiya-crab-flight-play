using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
public static class BuildFlight {
 public static int DefaultCityLayout=2;public static float DefaultSecondaryFraction=.25f;
 [MenuItem("Tools/Crab Flight/Prepare Candidate")]
 public static CrabFlightGame PrepareScene(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var game=new GameObject("CrabFlightGame").AddComponent<CrabFlightGame>();game.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");game.groundShader=Shader.Find("Standard");game.cityLayout=DefaultCityLayout;game.secondaryThresholdFraction=DefaultSecondaryFraction;game.cityR2Shader=Shader.Find("CrabCityDensity/R2Surface");game.cityR2Albedo=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CrabCityDensityR2/Textures/CityAlbedoAtlas.png");game.cityR2Metal=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CrabCityDensityR2/Textures/CityMetalSmoothAtlas.png");
  game.fragmentFadeShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/CrabCityDensityR2/Shaders/BrokenMetalVisibility.shader");if(!game.fragmentFadeShader)throw new System.Exception("Fragment visibility shader unavailable");
  var effectAsset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/CrabVfxPack/Prefabs/CrabDestructionVfx.prefab");if(!effectAsset||!effectAsset.GetComponent<CrabDestructionVfx.CrabVfxRig>())throw new System.Exception("VFX prefab import failed");game.effectsPrefab=effectAsset.GetComponent<CrabDestructionVfx.CrabVfxRig>();
  if(!game.crabShader||!game.groundShader)throw new System.Exception("Required original shader missing");
  EditorSceneManager.SaveScene(game.gameObject.scene,"Assets/FlightField.unity");return game;
 }
 public static void ReviewAndWeb(){RigidFlightReview.Run();Web();}
 public static void Web(){Web("../delivery/UnityWeb");}
 public static void Web(string location){
  PrepareScene();PlayerSettings.companyName="SakiyaPrototype";PlayerSettings.productName="カニとばし・飛距離試作";
  PlayerSettings.defaultWebScreenWidth=1280;PlayerSettings.defaultWebScreenHeight=800;PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;PlayerSettings.WebGL.dataCaching=false;PlayerSettings.runInBackground=true;PlayerSettings.colorSpace=ColorSpace.Linear;PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.WebGL,ScriptingImplementation.IL2CPP);PlayerSettings.stripEngineCode=false;
  Directory.CreateDirectory("../delivery");Directory.CreateDirectory("../evidence");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/FlightField.unity"},locationPathName=location,target=BuildTarget.WebGL,options=BuildOptions.CleanBuildCache});
  File.WriteAllText(Path.GetFullPath("../evidence/build-result.txt"),report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors+" duration="+report.summary.totalTime);
  if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Flight WebGL build failed");
  string dest=Path.GetFullPath(location);var loaders=Directory.GetFiles(Path.Combine(dest,"Build"),"*.loader.js");if(loaders.Length!=1)throw new System.Exception("Unexpected loader count");string name=Path.GetFileName(loaders[0]).Replace(".loader.js","");
  File.Copy("../web-template/index.html",Path.Combine(dest,"index.html"),true);File.Copy("../web-template/sound.js",Path.Combine(dest,"sound.js"),true);File.WriteAllText(Path.Combine(dest,"host.js"),File.ReadAllText("../web-template/host.js").Replace("UnityWeb.",name+"."));
 }
}

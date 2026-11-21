#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CrabCityDensityR2 {
 public static class R2PreviewMenu {
  [MenuItem("Crab City Density/R2 isolated Dense preview")]
  static void Dense(){Preview("Dense");}
  [MenuItem("Crab City Density/R2 isolated Readable preview")]
  static void Readable(){Preview("Readable");}
  static void Preview(string profile){
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
   const string dir="Assets/CrabCityDensityR2/";
   var g=AssetDatabase.LoadAssetAtPath<TextAsset>(dir+"Data/CityGeometryR2Compact.json");
   var l=AssetDatabase.LoadAssetAtPath<TextAsset>(dir+"Data/CityPlacement"+profile+".json");
   var a=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"Textures/CityAlbedoAtlas.png");
   var m=AssetDatabase.LoadAssetAtPath<Texture2D>(dir+"Textures/CityMetalSmoothAtlas.png");
   var root=CityDensityPack.Build(g,l,a,m,Shader.Find("CrabCityDensity/R2Surface"),CityDensityPack.Detail.Balanced);
   var co=new GameObject("R2 preview camera");var cam=co.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.612f;cam.transform.position=new Vector3(78.6f,7.41f,-16);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));cam.backgroundColor=new Color(.35f,.65f,.71f);cam.clearFlags=CameraClearFlags.SolidColor;
   CityParallaxBands.Attach(root,cam);var sun=new GameObject("R2 preview sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(42,-25,0);sun.intensity=1.2f;
   Selection.activeGameObject=root;SceneView.FrameLastActiveSceneView();
  }
 }
}
#endif

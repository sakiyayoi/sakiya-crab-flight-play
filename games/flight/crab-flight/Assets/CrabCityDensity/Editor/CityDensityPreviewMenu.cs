#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace CrabCityDensity {
 public static class CityDensityPreviewMenu {
  [MenuItem("Crab City Density/Create isolated preview scene")]
  static void Preview(){
   // Additive scene only; no save, no close, no edit of the current shared scene.
   var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
   UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
   var g=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/CrabCityDensity/Data/CityGeometry.json");
   var l=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/CrabCityDensity/Data/CityPlacement.json");
   var root=CityDensityPack.Build(g,l,Shader.Find("Standard"),CityDensityPack.Detail.Balanced);
   var co=new GameObject("Preview camera");var cam=co.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.612f;cam.transform.position=new Vector3(78.6f,7.41f,-16);cam.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));cam.backgroundColor=new Color(.35f,.65f,.71f);cam.clearFlags=CameraClearFlags.SolidColor;
   CityParallaxBands.Attach(root,cam);
   var sun=new GameObject("Preview sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.rotation=Quaternion.Euler(42,-25,0);sun.intensity=1.2f;
   Selection.activeGameObject=root;SceneView.FrameLastActiveSceneView();
  }
 }
}
#endif

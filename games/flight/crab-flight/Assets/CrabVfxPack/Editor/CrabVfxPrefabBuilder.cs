#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CrabDestructionVfx.Editor {
 public static class CrabVfxPrefabBuilder {
  // Explicit menu only. Import never writes a scene, quality setting, or any existing asset.
  [MenuItem("Tools/Crab VFX/Create Independent Prefab")]
  public static void Create(){
   const string folder="Assets/CrabVfxGenerated";
   if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets","CrabVfxGenerated");
   string p=AssetDatabase.GenerateUniqueAssetPath(folder+"/CrabVfxProfile.asset");
   var profile=ScriptableObject.CreateInstance<CrabVfxProfile>();AssetDatabase.CreateAsset(profile,p);
   var shader=Shader.Find("Crab/DestructionInk");
   if(!shader){Debug.LogError("DestructionInk shader missing; profile was saved, prefab not created.");return;}
   var root=new GameObject("Crab destruction VFX (visual only)");
   var rig=root.AddComponent<CrabVfxRig>();rig.profile=profile;rig.inkShader=shader;
   string prefab=AssetDatabase.GenerateUniqueAssetPath(folder+"/CrabDestructionVfx.prefab");
   PrefabUtility.SaveAsPrefabAsset(root,prefab);Object.DestroyImmediate(root);AssetDatabase.SaveAssets();
   Selection.activeObject=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
   Debug.Log("Created VFX prefab "+prefab+". Connect Observe/Emit calls explicitly; no scene changed.");
  }
 }
}
#endif

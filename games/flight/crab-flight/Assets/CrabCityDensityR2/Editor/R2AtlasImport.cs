#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace CrabCityDensityR2 {
 public sealed class R2AtlasImport : AssetPostprocessor {
  void OnPreprocessTexture(){
   if(!assetPath.StartsWith("Assets/CrabCityDensityR2/Textures/"))return;
   var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;
   importer.sRGBTexture=assetPath.Contains("Albedo");importer.alphaSource=TextureImporterAlphaSource.FromInput;
   importer.alphaIsTransparency=false;importer.mipmapEnabled=false; // Isolated atlas: no cross-tile mip bleed.
   importer.isReadable=false;
   importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
   importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.Uncompressed;
  }
 }
}
#endif

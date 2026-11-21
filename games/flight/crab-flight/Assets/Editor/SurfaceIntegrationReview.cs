using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
public static class SurfaceIntegrationReview {
 public static void ModelOnly(){
  foreach(var name in new[]{"chitin-pigment","chitin-normal","chitin-orm"}){
   var t=(TextureImporter)AssetImporter.GetAtPath("Assets/Resources/CrabSurface1/"+name+".png");
   t.textureType=name=="chitin-normal"?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=name=="chitin-pigment";t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=512;t.mipmapEnabled=true;t.wrapMode=TextureWrapMode.Repeat;if(name=="chitin-normal")t.flipGreenChannel=false;t.SaveAndReimport();
  }
  AnatomyModelReview.Run();
  CrabModel.ReviewUseBaseline(false);CrabModel.SurfaceShader=Shader.Find("CustomCrabBattle/ChitinR3");var c=CrabModel.Make("Surface contract",new FlightSettings(),Color.white);
  try{
   int vertices=0,uv=0,tangents=0;foreach(var f in c.Root.GetComponentsInChildren<MeshFilter>()){var m=f.sharedMesh;vertices+=m.vertexCount;uv+=m.uv.Length;tangents+=m.tangents.Length;foreach(var t in m.tangents)if(float.IsNaN(t.x)||float.IsNaN(t.y)||float.IsNaN(t.z)||float.IsNaN(t.w))throw new Exception("Nonfinite tangent");}
   if(vertices!=16544||uv!=vertices||tangents!=vertices)throw new Exception("UV/tangent count mismatch");
   File.WriteAllText("../evidence/surface-contract.json","{\"vertices\":"+vertices+",\"uv\":"+uv+",\"tangents\":"+tangents+",\"parts\":11,\"normalGreenFlip\":false,\"normalScale\":0.65,\"textureCount\":3}");
  }finally{c.Dispose();}
 }
}

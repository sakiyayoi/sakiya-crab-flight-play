using System;
using UnityEngine;

// Supplemental UV data for the supplied CCB2 archive; no binary-format replacement.
public static class CrabSurfaceBinding {
 [Serializable] public class MeshUV {public int mesh,material;public string name;public float[] uv;}
 [Serializable] public class UVSet {public string archiveSHA256;public MeshUV[] meshes;}
 static UVSet data;
 public static bool Enabled=true;public static bool DetailR6=true;
 public static void RefreshDefinition(Material m,int index){m.SetFloat("_NormalScale",DetailR6?.95f:.65f);m.SetFloat("_SmoothBias",!DetailR6?0:index==4?.10f:index==2?-.04f:index==0||index==5?.055f:.015f);}
 static UVSet Data {get {if(data==null){var a=Resources.Load<TextAsset>("CrabSurface1/uv0");if(!a)throw new InvalidOperationException("Surface UV resource missing");data=JsonUtility.FromJson<UVSet>(a.text);}return data;}}
 public static Vector2[] UV(int index,string name,int count){
  var d=Data;if(d.meshes==null||index<0||index>=d.meshes.Length)throw new InvalidOperationException("Surface UV mesh absent");var m=d.meshes[index];
  if(m.mesh!=index||m.name!=name||m.uv==null||m.uv.Length!=count*2)throw new InvalidOperationException("Surface UV/archive mismatch: "+name);
  var uv=new Vector2[count];for(int i=0;i<count;i++){float x=m.uv[i*2],y=m.uv[i*2+1];if(float.IsNaN(x)||float.IsNaN(y)||float.IsInfinity(x)||float.IsInfinity(y))throw new InvalidOperationException("Non-finite surface UV");uv[i]=new Vector2(x,y);}return uv;
 }
 public static void Material(Material m,int index,bool active){
  RefreshDefinition(m,index);bool use=Enabled&&active&&(index==0||index==1||index==5);m.SetFloat("_SurfaceMaps",use?1:0);if(!use)return;
  var pigment=Resources.Load<Texture2D>("CrabSurface1/chitin-pigment");var normal=Resources.Load<Texture2D>("CrabSurface1/chitin-normal");var orm=Resources.Load<Texture2D>("CrabSurface1/chitin-orm");
  if(!pigment||!normal||!orm)throw new InvalidOperationException("Surface textures missing");m.SetTexture("_MainTex",pigment);m.SetTexture("_BumpMap",normal);m.SetTexture("_ORM",orm);m.SetFloat("_NormalScale",DetailR6?.95f:.65f);
 }
}

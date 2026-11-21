using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Shared baked gradients; every animated sign owns its own material.
public static class NeonGlowTexture {
 static readonly Dictionary<Color32,Texture2D> cache=new Dictionary<Color32,Texture2D>();
 static Mesh quad;
 public static Texture2D Get(Color color){
  Color32 key=color;if(cache.TryGetValue(key,out var existing)&&existing)return existing;
  const int size=128;var pixels=new Color[size*size];
  for(int y=0;y<size;y++)for(int x=0;x<size;x++){
   float r=new Vector2((x+.5f)/size*2-1,(y+.5f)/size*2-1).magnitude;
   float edge=Mathf.Clamp01(1-r),alpha=edge*edge*Mathf.Exp(-r*r*3);
   Color c=Color.Lerp(color,Color.white,Mathf.Pow(edge,12)*.8f);c.a=alpha;pixels[y*size+x]=c;
  }
  var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){name="Neon baked glow "+ColorUtility.ToHtmlStringRGB(color),wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
  texture.SetPixels(pixels);texture.Apply(false,true);cache[key]=texture;return texture;
 }
 public static Material Material(Color color){
  var shader=Resources.Load<Shader>("NeonProto/NeonAdditive");if(!shader)throw new InvalidOperationException("Neon additive shader missing");
  return new Material(shader){name="Neon additive glow",mainTexture=Get(color),color=Color.white};
 }
 public static MeshRenderer Quad(string name,Transform parent,Color color){
  if(!quad){quad=new Mesh{name="Neon render-only quad"};quad.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};quad.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};quad.colors=new[]{Color.white,Color.white,Color.white,Color.white};quad.triangles=new[]{0,2,1,0,3,2};quad.RecalculateBounds();}
  var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=quad;
  var r=o.AddComponent<MeshRenderer>();r.sharedMaterial=Material(color);r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;return r;
 }
}

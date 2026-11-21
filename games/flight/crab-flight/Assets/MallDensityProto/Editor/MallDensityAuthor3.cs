using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Model=BoutiqueModelBuilderR18.Model;

public static class MallDensityAuthor3 {
 const string Root="Assets/MallDensityProto/Resources/MallDensityProto";
 [Serializable] public class Box {public float[] c,s;public int material;public float radius;}
 [Serializable] public class Rod {public float[] a,b;public int material;public float width;}
 [Serializable] public class Lathe {public float[] c,profile;public int material,sides;}
 [Serializable] public class Group {public string name,kind;public Box[] boxes;public Rod[] rods;public Lathe[] lathes;}
 [Serializable] public class Design {public Group[] groups;}
 static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
 static Color Hex(string s){ColorUtility.TryParseHtmlString(s,out var c);return c;}
 static T Save<T>(T value,string path)where T:UnityEngine.Object{
  var old=AssetDatabase.LoadAssetAtPath<T>(path);if(old){EditorUtility.CopySerialized(value,old);UnityEngine.Object.DestroyImmediate(value);return old;}
  AssetDatabase.CreateAsset(value,path);return value;
 }
 static Material Atlas(string name,Color[] palette){
  var tex=new Texture2D(64,64,TextureFormat.RGBA32,false){name=name+" atlas",filterMode=FilterMode.Point};
  var metal=new Texture2D(64,64,TextureFormat.RGBA32,false){name=name+" roughness",filterMode=FilterMode.Point};
  for(int y=0;y<64;y++)for(int x=0;x<64;x++){int i=x/16+y/16*4;tex.SetPixel(x,y,palette[i]);metal.SetPixel(x,y,new Color(0,0,0,.24f));}
  tex.Apply();metal.Apply();tex=Save(tex,Root+"/"+name+"-atlas.asset");metal=Save(metal,Root+"/"+name+"-metal.asset");
  var mat=new Material(Shader.Find("CrabCityDensity/R2Surface")){name=name};mat.SetTexture("_MainTex",tex);mat.SetTexture("_MetallicGlossMap",metal);mat.SetColor("_Color",Color.white);
  return Save(mat,Root+"/"+name+".mat");
 }
 public static void Append(Transform root){
  var data=Resources.Load<TextAsset>("MallDensityProto/Decor3");
  if(!data)throw new InvalidOperationException("先に tools/author-density3.py を実行してください");
  var design=JsonUtility.FromJson<Design>(data.text);
  if(design==null||design.groups==null||design.groups.Length!=11)throw new InvalidOperationException("飾りの追加は11組です");
  var palette=new[]{"#F9B7C6","#A9E6C8","#FFF3D6","#FFFDF6","#FBE38E","#F78FB0","#7ED3B2","#F6C6A8","#E8B4D8","#BDE0FE","#F49AC1","#8FE0D0","#E5738F","#CDEAC0","#D9C7A7","#B08968"}.Select(Hex).ToArray();
  var cuteMaterial=Atlas("cutebackground",palette);
  foreach(var g in design.groups){
   var m=new Model(g.name);
   foreach(var b in g.boxes)m.Box(-1,b.material,V(b.c),V(b.s),b.radius);
   foreach(var r in g.rods)m.Rod(-1,r.material,V(r.a),V(r.b),r.width);
   foreach(var l in g.lathes){
    var profile=new Vector2[l.profile.Length/2];
    for(int i=0;i<profile.Length;i++)profile[i]=new Vector2(l.profile[i*2],l.profile[i*2+1]);
    m.Lathe(-1,l.material,V(l.c),profile,l.sides);
   }
   MallDensityAuthor.AddModel(root,m,Vector3.zero,cuteMaterial);
  }
 }
}

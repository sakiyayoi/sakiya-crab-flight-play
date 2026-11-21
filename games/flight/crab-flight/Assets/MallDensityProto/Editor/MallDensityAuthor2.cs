using System;
using UnityEngine;
using Model=BoutiqueModelBuilderR18.Model;

// v1の20組を保持し、既存モール形状へ取り付ける14組を追加する。
public static class MallDensityAuthor2 {
 [Serializable] public class Box {public float[] c,s;public int material;public float radius;}
 [Serializable] public class Rod {public float[] a,b;public int material;public float width;}
 [Serializable] public class Lathe {public float[] c,profile;public int material,sides;}
 [Serializable] public class Group {public string name,kind;public Box[] boxes;public Rod[] rods;public Lathe[] lathes;}
 [Serializable] public class Design {public Group[] groups;}
 static Vector3 V(float[] a)=>new Vector3(a[0],a[1],a[2]);
 public static void Append(Transform root,Material material){
  var data=Resources.Load<TextAsset>("MallDensityProto/Decor2");
  if(!data)throw new InvalidOperationException("先に tools/author-density2.py を実行してください");
  var design=JsonUtility.FromJson<Design>(data.text);
  if(design.groups.Length!=14)throw new InvalidOperationException("飾りの追加は14組です");
  foreach(var g in design.groups){
   var m=new Model(g.name);
   foreach(var b in g.boxes)m.Box(-1,b.material,V(b.c),V(b.s),b.radius);
   foreach(var r in g.rods)m.Rod(-1,r.material,V(r.a),V(r.b),r.width);
   foreach(var l in g.lathes){
    var profile=new Vector2[l.profile.Length/2];
    for(int i=0;i<profile.Length;i++)profile[i]=new Vector2(l.profile[i*2],l.profile[i*2+1]);
    m.Lathe(-1,l.material,V(l.c),profile,l.sides);
   }
   MallDensityAuthor.AddModel(root,m,Vector3.zero,material);
  }
 }
}

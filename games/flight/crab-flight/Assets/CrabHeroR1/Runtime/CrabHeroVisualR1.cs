using System;using System.Collections.Generic;using UnityEngine;
// Opt-in render layer. Call AFTER CrabModel.Make: all native part bounds are already cached.
// Never changes the source meshes, transforms, colliders, rigidbody or joint parameters.
public sealed class CrabHeroVisualR1 : MonoBehaviour {
 readonly List<Mesh> meshes=new List<Mesh>();readonly List<Material> materials=new List<Material>();
 readonly List<MeshRenderer> hidden=new List<MeshRenderer>();readonly List<GameObject> visuals=new List<GameObject>();
 public static CrabHeroVisualR1 Attach(CrabModel model){
  if(model==null||!model.Root)throw new ArgumentNullException("model");
  var existing=model.Root.GetComponent<CrabHeroVisualR1>();if(existing)return existing;
  var layer=model.Root.gameObject.AddComponent<CrabHeroVisualR1>();layer.Build(model);return layer;
 }
 static float Bell(float x,float c,float w){float q=(x-c)/w;return Mathf.Exp(-q*q);}
 void Build(CrabModel model){
  var shader=Shader.Find("CrabHeroR1/Chitin");if(!shader)throw new InvalidOperationException("Hero chitin shader missing; include Assets/CrabHeroR1");
  foreach(var mf in model.Root.GetComponentsInChildren<MeshFilter>()){
   var renderer=mf.GetComponent<MeshRenderer>();if(!renderer||!mf.sharedMesh)continue;
   string n=mf.name;bool shell=n.StartsWith("CarapacePart"),rim=shell&&n.EndsWith("surface 5"),claw=n.StartsWith("Claw_"),leg=n.StartsWith("Leg_"),tip=n.EndsWith("surface 3"),membrane=n.EndsWith("surface 2"),eye=n.EndsWith("surface 4"),belly=n.StartsWith("BodyCore")&&n.EndsWith("surface 1");
   var mesh=Instantiate(mf.sharedMesh);mesh.name="HeroR1 "+n;var v=mesh.vertices;var nn=mesh.normals;var col=new Color[v.Length];var bounds=mesh.bounds;
   for(int i=0;i<v.Length;i++){
    Vector3 p=v[i];Color color=new Color(.33f,.10f,.04f);
    if(shell){
     float x=Mathf.Abs(p.x)/1.19f,z=(p.z+.04f)/.74f;
     if(!rim){float upper=Mathf.Clamp01((p.y-.08f)/.42f);p.y+=.035f*upper*Bell(p.x,0,.72f);p.y-=.018f*upper*(Bell(p.x,-.38f,.105f)+Bell(p.x,.38f,.105f))*Bell(p.z,-.04f,.62f);
      float edge=Mathf.Clamp01((x-.72f)/.28f)*(1-upper*.65f);color=Color.Lerp(new Color(.29f,.07f,.03f),new Color(.48f,.20f,.075f),edge);
      float cervical=Bell(p.z,.37f,.045f)*Bell(p.x,0,.72f)*upper;color*=1-.24f*cervical;
      float natural=.94f+.06f*Mathf.Sin(p.x*12+Mathf.Sin(p.z*17));color*=natural;
     }else {float lobe=Bell(p.z,-.45f,.06f)+Bell(p.z,-.18f,.06f)+Bell(p.z,.12f,.06f)+Bell(p.z,.37f,.06f);p.x*=1+.035f*lobe*Mathf.Clamp01((Mathf.Abs(p.x)-.65f)/.45f);color=new Color(.52f,.27f,.11f);}
    }else if(claw&&!membrane){
     bool finger=n.Contains("DactylHinge");float t=Mathf.InverseLerp(bounds.min.z,bounds.max.z,p.z);float palm=finger?Mathf.Sin(t*Mathf.PI)*.045f:Bell(p.z,.76f,.32f)*.065f;
     p.y+=nn[i].y*palm;p.x+=nn[i].x*palm*.65f;
     float ivory=Mathf.Clamp01((t-.58f)/.32f);color=Color.Lerp(new Color(.40f,.115f,.04f),new Color(.65f,.39f,.18f),ivory*.75f);
     float dark=Mathf.Clamp01((t-.91f)/.09f);color=Color.Lerp(color,new Color(.075f,.038f,.024f),dark*.88f);
     color*=.88f+.12f*Mathf.Clamp01(nn[i].y+.35f);if(finger)p=Quaternion.Euler(0,n.Contains("_L")?-8f:8f,0)*p;
    }else if(leg&&!membrane&&!tip){p+=nn[i]*.014f;float t=Mathf.InverseLerp(0,bounds.max.y-bounds.min.y,p.y-bounds.min.y);color=Color.Lerp(new Color(.26f,.07f,.03f),new Color(.49f,.185f,.065f),t);}
    else if(membrane)color=new Color(.075f,.038f,.025f);
    else if(tip)color=new Color(.11f,.065f,.036f);
    else if(eye){color=new Color(.012f,.019f,.017f);float center=p.x<0?-.42f:.42f;p.x=center+(p.x-center)*1.14f;p.y=.321f+(p.y-.321f)*1.14f;}
    else if(belly)color=new Color(.40f,.25f,.13f);
    else color=new Color(.31f,.10f,.035f);
    v[i]=p;col[i]=color;
   }
   mesh.vertices=v;mesh.colors=col;mesh.RecalculateNormals();if(mesh.uv.Length==v.Length)mesh.RecalculateTangents();mesh.RecalculateBounds();meshes.Add(mesh);
   var mat=new Material(shader){name="HeroR1 "+renderer.sharedMaterial.name};mat.SetColor("_Color",Color.white);mat.SetFloat("_Roughness",eye?.18f:membrane?.8f:rim?.41f:.48f);
   bool mapped=!eye&&!membrane&&!tip;mat.SetFloat("_Mapped",mapped?1:0);if(mapped){mat.SetTexture("_MainTex",Resources.Load<Texture2D>("CrabSurface1/chitin-pigment"));mat.SetTexture("_BumpMap",Resources.Load<Texture2D>("CrabSurface1/chitin-normal"));mat.SetTexture("_ORM",Resources.Load<Texture2D>("CrabSurface1/chitin-orm"));}materials.Add(mat);
   var o=new GameObject("HeroR1 visual");o.transform.SetParent(mf.transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var rr=o.AddComponent<MeshRenderer>();rr.sharedMaterial=mat;rr.shadowCastingMode=renderer.shadowCastingMode;rr.receiveShadows=renderer.receiveShadows;hidden.Add(renderer);renderer.enabled=false;visuals.Add(o);
  }
 }
 void OnDestroy(){foreach(var r in hidden)if(r)r.enabled=true;foreach(var o in visuals)if(o)Release(o);foreach(var m in meshes)if(m)Release(m);foreach(var m in materials)if(m)Release(m);}
 static void Release(UnityEngine.Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
}

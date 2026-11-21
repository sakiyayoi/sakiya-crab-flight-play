using System;using System.Collections.Generic;using UnityEngine;
// Opt-in render layer. Call AFTER CrabModel.Make: all native part bounds are already cached.
// Never changes the source meshes, transforms, colliders, rigidbody or joint parameters.
public sealed class CrabHeroVisualR2 : MonoBehaviour {
 readonly List<Mesh> meshes=new List<Mesh>();readonly List<Material> materials=new List<Material>();
 readonly List<MeshRenderer> hidden=new List<MeshRenderer>();readonly List<GameObject> visuals=new List<GameObject>();
 public static CrabHeroVisualR2 Attach(CrabModel model){
  if(model==null||!model.Root)throw new ArgumentNullException("model");
  foreach(var oldMesh in model.Root.GetComponentsInChildren<MeshFilter>())if(oldMesh.name=="HeroR1 visual")throw new InvalidOperationException("Remove r1 visual layer before attaching r2");
  var existing=model.Root.GetComponent<CrabHeroVisualR2>();if(existing)return existing;
  var layer=model.Root.gameObject.AddComponent<CrabHeroVisualR2>();layer.Build(model);return layer;
 }
 static float Bell(float x,float c,float w){float q=(x-c)/w;return Mathf.Exp(-q*q);}
 void Build(CrabModel model){
  var shader=Shader.Find("CrabHeroR2/Chitin");if(!shader)throw new InvalidOperationException("Hero chitin shader missing; include Assets/CrabHeroR2");
  var sources=model.Root.GetComponentsInChildren<MeshFilter>();var outline=new float[256];
  foreach(var f in sources)if(f.name=="CarapacePart / surface 0")foreach(var v0 in f.sharedMesh.vertices){float angle=Mathf.Atan2(v0.z/.74f,v0.x/1.19f);int bin=Mathf.RoundToInt((angle+Mathf.PI)/(2*Mathf.PI)*256)%256;outline[bin]=Mathf.Max(outline[bin],Mathf.Sqrt(v0.x*v0.x/(1.19f*1.19f)+v0.z*v0.z/(.74f*.74f)));}
  var sampled=(float[])outline.Clone();for(int k=0;k<256;k++)if(sampled[k]<.4f){for(int step=1;step<128;step++){float a0=outline[(k+step)%256],b0=outline[(k-step+256)%256];if(a0>.4f||b0>.4f){sampled[k]=Mathf.Max(a0,b0);break;}}}
  foreach(var mf in sources){
   var renderer=mf.GetComponent<MeshRenderer>();if(!renderer||!mf.sharedMesh||!renderer.enabled||mf.name.StartsWith("HeroR"))continue;
   string n=mf.name;bool shell=n.StartsWith("CarapacePart"),rim=shell&&n.EndsWith("surface 5"),claw=n.StartsWith("Claw_"),leg=n.StartsWith("Leg_"),tip=n.EndsWith("surface 3"),membrane=n.EndsWith("surface 2"),eye=n.EndsWith("surface 4"),belly=n.StartsWith("BodyCore")&&n.EndsWith("surface 1");
   var mesh=Instantiate(mf.sharedMesh);mesh.name="HeroR2 "+n;var v=mesh.vertices;var nn=mesh.normals;var col=new Color[v.Length];var bounds=mesh.bounds;
   for(int i=0;i<v.Length;i++){
    Vector3 p=v[i];Color color=new Color(.30f,.095f,.040f);
    if(shell){
     // Brown-crab reference: broad, low crown and a rounded fronto-lateral margin.
     float upper=Mathf.Clamp01((p.y-.085f)/.44f);
     if(!rim&&p.y>.085f)p.y=.085f+(p.y-.085f)*.68f;
     float a=Mathf.Atan2(p.z/.74f,p.x/1.19f),front=Mathf.Max(0,Mathf.Sin(a));
     float angular=(a+Mathf.PI)/(2*Mathf.PI)*256;int lo=Mathf.FloorToInt(angular)%256,hi=(lo+1)%256;float edgeRadius=Mathf.Max(.1f,Mathf.Lerp(sampled[lo],sampled[hi],angular-Mathf.Floor(angular)));
     float radius=Mathf.Sqrt(p.x*p.x/(1.19f*1.19f)+p.z*p.z/(.74f*.74f))/edgeRadius;
     float rounded=1+.013f*Mathf.Cos(18*a)*Mathf.Clamp01((Mathf.Sin(a)+.20f)*2);
     p.x=1.17f*Mathf.Cos(a)*radius*rounded;p.z=.74f*Mathf.Sin(a)*radius*rounded*(1-.075f*Mathf.Pow(front,10));
     float edge=Mathf.Clamp01((radius-.68f)/.32f);
     if(rim)color=new Color(.44f,.205f,.085f);
     else{
      color=Color.Lerp(new Color(.245f,.068f,.029f),new Color(.39f,.145f,.050f),edge*.7f);
      float region=Bell(p.x,-.38f,.15f)+Bell(p.x,.38f,.15f);p.y-=.012f*upper*region*Bell(p.z,-.02f,.55f);
      color*=1-.13f*Bell(p.z,.32f,.06f)*Bell(p.x,0,.70f)*upper;
      color*=.96f+.04f*Mathf.Sin(p.x*11+Mathf.Sin(p.z*14));
     }
    }else if(claw&&!membrane){
     bool finger=n.Contains("DactylHinge");float side=n.Contains("_L")?-1:1;
     // Wide propodus, continuous hooked fixed finger, dark distal pincer surfaces.
     float bulk=finger?Mathf.Sin(Mathf.InverseLerp(0,.72f,p.z)*Mathf.PI)*.020f:Bell(p.z,.82f,.31f)*.093f;
     p.y+=nn[i].y*bulk;p.x+=nn[i].x*bulk*.72f;
     float dorsal=Mathf.Clamp01((nn[i].y+.25f)/.65f);color=Color.Lerp(new Color(.49f,.34f,.185f),new Color(.345f,.087f,.038f),dorsal);
     float dark=finger?Mathf.SmoothStep(0,1,Mathf.InverseLerp(.27f,.50f,p.z)):Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.12f,1.38f,p.z));
     color=Color.Lerp(color,new Color(.022f,.018f,.016f),dark);
     if(finger)p=Quaternion.Euler(0,side*6,0)*p;
    }else if(leg&&!membrane&&!tip){
     // Broader, dorsoventrally compressed merus/carpus with a visible narrow joint.
     float proximal=1-Mathf.Clamp01((Mathf.Abs(p.x)-.45f)/.7f);p+=new Vector3(nn[i].x,nn[i].y*.62f,nn[i].z)*Mathf.Lerp(.012f,.035f,proximal);
     float dorsal=Mathf.Clamp01((nn[i].y+.25f)/.65f);color=Color.Lerp(new Color(.43f,.28f,.15f),new Color(.33f,.083f,.041f),dorsal);
    }else if(membrane)color=new Color(.15f,.076f,.042f);
    else if(tip)color=new Color(.06f,.038f,.024f);
    else if(eye){p.y=.08f+(p.y-.08f)*.52f+.014f;p.z=.58f+(p.z-.58f)*.65f+.035f;color=new Color(.009f,.014f,.012f);}
    else if(belly)color=new Color(.47f,.325f,.18f);
    else{
     color=new Color(.30f,.09f,.037f);
     if(n.StartsWith("BodyCore")&&Mathf.Abs(p.x)<.60f&&p.z>.55f&&p.y>.07f){p.y=.08f+(p.y-.08f)*.52f;p.z=.58f+(p.z-.58f)*.65f;}
    }
    v[i]=p;col[i]=color;
   }
   mesh.vertices=v;mesh.colors=col;mesh.RecalculateNormals();if(mesh.uv.Length==v.Length)mesh.RecalculateTangents();mesh.RecalculateBounds();meshes.Add(mesh);
   var mat=new Material(shader){name="HeroR2 "+renderer.sharedMaterial.name};mat.SetColor("_Color",Color.white);mat.SetFloat("_Roughness",eye?.18f:membrane?.82f:rim?.5f:.56f);
   bool mapped=!eye&&!membrane&&!tip;mat.SetFloat("_Mapped",mapped?1:0);if(mapped){mat.SetTexture("_MainTex",Resources.Load<Texture2D>("CrabSurface1/chitin-pigment"));mat.SetTexture("_BumpMap",Resources.Load<Texture2D>("CrabSurface1/chitin-normal"));mat.SetTexture("_ORM",Resources.Load<Texture2D>("CrabSurface1/chitin-orm"));}materials.Add(mat);
   var o=new GameObject("HeroR2 visual");o.transform.SetParent(mf.transform,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var rr=o.AddComponent<MeshRenderer>();rr.sharedMaterial=mat;rr.shadowCastingMode=renderer.shadowCastingMode;rr.receiveShadows=renderer.receiveShadows;hidden.Add(renderer);renderer.enabled=false;visuals.Add(o);
  }
 }
 public void Detach(){Restore();Release(this);}
 void OnDestroy(){Restore();}
 void Restore(){foreach(var r in hidden)if(r)r.enabled=true;foreach(var o in visuals)if(o)Release(o);foreach(var m in meshes)if(m)Release(m);foreach(var m in materials)if(m)Release(m);hidden.Clear();visuals.Clear();meshes.Clear();materials.Clear();}
 static void Release(UnityEngine.Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
}

using System.Collections.Generic;
using UnityEngine;

// Authored road detail only. Gameplay contacts stay in RigidCrabFlight.
public static class Street3Presentation {
#if UNITY_EDITOR
 public static bool ReviewBaseline;
#endif
 public static void Build(Transform parent,Shader shader){
#if UNITY_EDITOR
  if(ReviewBaseline)return;
#endif
  var root=new GameObject("Street3 roadway, seams and contact cues");root.transform.SetParent(parent,false);
  var owner=root.AddComponent<CrabCityDensityR2.CityDensityOwnedAssets>();
  var baseGround=new Surface(root.transform,owner,shader,"district ground",new Color(.34f,.36f,.33f),.06f);
  var asphalt=new Surface(root.transform,owner,shader,"rough tarmac",new Color(.16f,.19f,.20f),.10f);
  var paint=new Surface(root.transform,owner,shader,"worn road paint",new Color(.72f,.65f,.38f),.05f);
  var seams=new Surface(root.transform,owner,shader,"road joins",new Color(.10f,.12f,.12f),.02f);
  var curb=new Surface(root.transform,owner,shader,"curb tops",new Color(.48f,.49f,.43f),.12f);
  foreach(int side in new[]{-1,1}){
   baseGround.Box(new Vector3(side*179,-.12f,0),new Vector3(306,.20f,80));
   asphalt.Box(new Vector3(side*179,-.035f,0),new Vector3(306,.12f,7.4f));
   for(float x=28;x<330;x+=4){
    paint.Box(new Vector3(side*x,.032f,-2.6f),new Vector3(1.9f,.012f,.10f));
    seams.Box(new Vector3(side*x,.030f,.20f),new Vector3(.035f,.008f,6.6f));
    curb.Box(new Vector3(side*x,.015f,3.45f),new Vector3(3.92f,.05f,.25f));
   }
   for(float x=51;x<=55;x+=.55f)paint.Box(new Vector3(side*x,.032f,-.3f),new Vector3(.30f,.014f,5.7f));
   for(float x=86;x<=89;x+=.55f)paint.Box(new Vector3(side*x,.032f,-.3f),new Vector3(.30f,.014f,5.7f));
  }
  baseGround.Finish();asphalt.Finish();paint.Finish();seams.Finish();curb.Finish();
 }
 sealed class Surface {
  readonly Transform parent;readonly CrabCityDensityR2.CityDensityOwnedAssets owner;readonly Material material;readonly List<Vector3> v=new List<Vector3>();readonly List<int> t=new List<int>();readonly string name;
  public Surface(Transform parent,CrabCityDensityR2.CityDensityOwnedAssets owner,Shader shader,string name,Color color,float gloss){this.parent=parent;this.owner=owner;this.name=name;material=new Material(shader){name="Street3 "+name,color=color};material.SetFloat("_Glossiness",gloss);owner.materials.Add(material);}
  public void Box(Vector3 center,Vector3 size){
   var e=size*.5f;var corners=new[]{new Vector3(-e.x,-e.y,-e.z),new Vector3(e.x,-e.y,-e.z),new Vector3(e.x,e.y,-e.z),new Vector3(-e.x,e.y,-e.z),new Vector3(-e.x,-e.y,e.z),new Vector3(e.x,-e.y,e.z),new Vector3(e.x,e.y,e.z),new Vector3(-e.x,e.y,e.z)};
   int[] faces={0,3,2,1,4,5,6,7,0,4,7,3,1,2,6,5,3,7,6,2,0,1,5,4};
   for(int f=0;f<6;f++){int start=v.Count;for(int k=0;k<4;k++)v.Add(center+corners[faces[f*4+k]]);t.AddRange(new[]{start,start+1,start+2,start,start+2,start+3});}
  }
  public void Finish(){var mesh=new Mesh{name="Street3 "+name};mesh.SetVertices(v);mesh.SetTriangles(t,0);mesh.RecalculateNormals();mesh.RecalculateBounds();owner.meshes.Add(mesh);var o=new GameObject(name);o.transform.SetParent(parent,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=o.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;}
 }
}

public sealed partial class RigidCrabFlight {
 float PanelImpactWeight(BreakPanel p){
  float kg=p.city2?p.cityProposal.massKgSuggestion*Mathf.Abs(p.visual.transform.lossyScale.x*p.visual.transform.lossyScale.y*p.visual.transform.lossyScale.z):p.type=="wall"?250:p.type=="metal"?110:p.type=="wood"?45:p.type=="glass"?14:p.type=="food"?.2f:2;
  return Mathf.Clamp01((Mathf.Log10(Mathf.Max(2,kg))-.7f)/1.7f);
 }
}

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

// Same narrow API as r1's procedural model. Authored r3 geometry, not primitive substitutes.
public sealed class BeforePolishCrabModel {
 public Transform Root,Body;
 public float ForwardReach {get;private set;}
 public static Shader SurfaceShader,ArenaShader;
 static CrabR3Archive archive;
 static Material[] materials;
 static readonly Dictionary<Color,Material> arenaMaterials=new Dictionary<Color,Material>();
 readonly List<Mesh> ownedMeshes=new List<Mesh>();
 readonly List<Transform> arms=new List<Transform>(),legs=new List<Transform>(),fingers=new List<Transform>();
 readonly List<Quaternion> armBases=new List<Quaternion>(),legBases=new List<Quaternion>(),fingerBases=new List<Quaternion>();
 readonly List<int> armSides=new List<int>(),fingerSides=new List<int>();
 Vector3 bodyPosition;Quaternion bodyRotation;
 static CrabR3Archive Data {
  get {
   if(archive==null){var bytes=Resources.Load<TextAsset>("CrabR3/crab-r3");if(!bytes)throw new InvalidOperationException("r3 model resource missing");archive=CrabR3Archive.Read(bytes.bytes);}
   return archive;
  }
 }
 static Material[] Materials(){
  if(materials!=null)return materials;
  var shader=SurfaceShader?SurfaceShader:Shader.Find("CustomCrabBattle/ChitinR3");if(!shader)throw new InvalidOperationException("r3 chitin shader missing");
  materials=new Material[Data.Materials.Length];
  for(int i=0;i<materials.Length;i++){var m=Data.Materials[i];materials[i]=new Material(shader){name=m.Name};materials[i].SetColor("_Color",new Color(m.Color[0],m.Color[1],m.Color[2],m.Color[3]));materials[i].SetFloat("_Roughness",m.Roughness);}
  return materials;
 }
 public static BeforePolishCrabModel Make(string name,CrabBuild build,Color legacyActorColor){
  // Preserve authored r3 materials; legacy bright actor tint does not recolor exchangeable parts.
  var data=Data;var settings=new CrabR3Parameters(build.claw,build.shell,build.legs);var model=new BeforePolishCrabModel();var nodes=new Transform[data.Nodes.Length];var mats=Materials();model.ForwardReach=Mathf.Max(0,data.BoundsAt(settings)[5]);
  for(int i=0;i<nodes.Length;i++){
   var spec=data.Nodes[i];var obj=new GameObject(i==0?name:spec.Name);var tr=obj.transform;nodes[i]=tr;if(spec.Parent>=0)tr.SetParent(nodes[spec.Parent],false);var pose=data.PoseAt(i,settings);tr.localPosition=new Vector3(pose[0],pose[1],pose[2]);tr.localRotation=new Quaternion(pose[3],pose[4],pose[5],pose[6]);tr.localScale=new Vector3(pose[7],pose[8],pose[9]);
   if(i==0)model.Root=tr;
   if(spec.Body){model.Body=tr;model.bodyPosition=tr.localPosition;model.bodyRotation=tr.localRotation;}
   if(spec.PartRoot&&spec.Kind==2){model.arms.Add(tr);model.armBases.Add(tr.localRotation);model.armSides.Add(spec.Side);}
   if(spec.PartRoot&&spec.Kind==3){model.legs.Add(tr);model.legBases.Add(tr.localRotation);}
   if(spec.Dactyl){model.fingers.Add(tr);model.fingerBases.Add(tr.localRotation);model.fingerSides.Add(spec.Side);}
   if(spec.Mesh>=0){
    var source=data.Meshes[spec.Mesh];var positions=data.PositionsAt(spec.Mesh,settings);var normals=data.NormalsAt(spec.Mesh,settings);var vertices=new Vector3[source.VertexCount];var nn=new Vector3[source.VertexCount];var colors=new Color[source.VertexCount];
    for(int j=0;j<vertices.Length;j++){vertices[j]=new Vector3(positions[j*3],positions[j*3+1],positions[j*3+2]);nn[j]=new Vector3(normals[j*3],normals[j*3+1],normals[j*3+2]);colors[j]=new Color(source.Colors[j*4],source.Colors[j*4+1],source.Colors[j*4+2],source.Colors[j*4+3]);}
    var mesh=new Mesh{name="r3 "+source.Name};if(source.VertexCount>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.vertices=vertices;mesh.normals=nn;mesh.colors=colors;mesh.triangles=source.Indices;mesh.RecalculateBounds();obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=mats[source.Material];model.ownedMeshes.Add(mesh);
   }
  }
  if(!model.Body||model.arms.Count!=2||model.legs.Count!=8||model.fingers.Count!=2){model.Dispose();throw new InvalidOperationException("r3 articulation links incomplete");}
  return model;
 }
 public void Animate(float time,float attack,float hit,bool down){
  float prep=attack>=0&&attack<.68f?Mathf.Sin(attack/.68f*Mathf.PI*.5f):attack>=.68f&&attack<1.12f?-(1-(attack-.68f)/.44f)*.55f:0;
  Body.localPosition=bodyPosition+Vector3.up*(Mathf.Sin(time*2)*.025f);Body.localRotation=bodyRotation*Quaternion.Euler(hit>0?Mathf.Sin(hit*55)*hit*30:0,0,down?80:0);
  for(int i=0;i<arms.Count;i++)arms[i].localRotation=armBases[i]*Quaternion.Euler(-prep*58,prep*armSides[i]*12,0);
  float close=attack>=.68f&&attack<.85f?1:attack>=.85f&&attack<1.12f?1-(attack-.85f)/.27f:0;
  for(int i=0;i<fingers.Count;i++){var open=fingerBases[i]*Quaternion.Euler(0,Mathf.Max(0,prep)*fingerSides[i]*10,0);fingers[i].localRotation=Quaternion.Slerp(open,Quaternion.identity,close);}
  for(int i=0;i<legs.Count;i++)legs[i].localRotation=legBases[i]*Quaternion.Euler(Mathf.Sin(time*4+i)*3,Mathf.Sin(time*4+i)*4,0);
 }
 public void AnimateFlight(float time,float airborne,float landing){
  Body.localPosition=bodyPosition+Vector3.up*(Mathf.Sin(time*2)*.012f-landing*.14f);Body.localRotation=bodyRotation;
  for(int i=0;i<arms.Count;i++)arms[i].localRotation=armBases[i]*Quaternion.Euler(-airborne*16,airborne*armSides[i]*5,0);
  for(int i=0;i<fingers.Count;i++)fingers[i].localRotation=fingerBases[i];
  for(int i=0;i<legs.Count;i++)legs[i].localRotation=legBases[i]*Quaternion.Euler(-airborne*12+Mathf.Sin(time*4+i)*(airborne>0?1.2f:2),landing*12,0);
 }
 public void ChargePose(float progress){
  float amount=Mathf.Sin(Mathf.Clamp01(progress)*Mathf.PI*.5f);
  Body.localPosition=bodyPosition-Vector3.up*(amount*.16f);
  for(int i=0;i<arms.Count;i++)arms[i].localRotation=armBases[i]*Quaternion.Euler(amount*18,amount*armSides[i]*5,0);
  for(int i=0;i<legs.Count;i++)legs[i].localRotation=legBases[i]*Quaternion.Euler(amount*9,amount*7,0);
 }
 public Transform ReplaceArm(int side){
  int desired=side==0?-1:1,index=armSides.FindIndex(value=>value==desired);if(index<0)throw new InvalidOperationException("Original arm attachment missing");
  var old=arms[index];var anchor=new GameObject(side==0?"Left interchangeable arm":"Right interchangeable arm").transform;anchor.SetParent(Root,false);anchor.localPosition=Root.InverseTransformPoint(old.position);old.gameObject.SetActive(false);return anchor;
 }
 public void Walk(float time,float strength){
  strength=Mathf.Clamp01(strength);Body.localPosition=bodyPosition+Vector3.up*(Mathf.Abs(Mathf.Sin(time*11))*.035f*strength);
  for(int i=0;i<legs.Count;i++){float wave=Mathf.Sin(time*11+i*Mathf.PI*.65f)*strength;legs[i].localRotation=legBases[i]*Quaternion.Euler(wave*10,wave*13,0);}
 }
 public void SetRoleRing(Color color){
  var obj=new GameObject("Ground role marker");obj.transform.SetParent(Root,false);
  var line=obj.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.startWidth=line.endWidth=.035f;line.numCornerVertices=2;
  for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,new Vector3(Mathf.Cos(a)*1.03f,.045f,Mathf.Sin(a)*1.03f));}
  if(!arenaMaterials.TryGetValue(color,out var mat)){mat=new Material(ArenaShader?ArenaShader:Shader.Find("Standard"));mat.color=color;mat.SetFloat("_Glossiness",0);arenaMaterials[color]=mat;}
  line.sharedMaterial=mat;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
 }
 public void Own(Mesh mesh){if(mesh)ownedMeshes.Add(mesh);}
 static void Release(UnityEngine.Object value){
#if UNITY_EDITOR
 if(!Application.isPlaying){UnityEngine.Object.DestroyImmediate(value);return;}
#endif
 UnityEngine.Object.Destroy(value);
 }
 public void Dispose(){if(Root)Release(Root.gameObject);foreach(var mesh in ownedMeshes)if(mesh)Release(mesh);ownedMeshes.Clear();}
 public static GameObject Shape(string name,Transform parent,PrimitiveType type,Vector3 pos,Vector3 size,Color color){
  var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;var collider=o.GetComponent<Collider>();
#if UNITY_EDITOR
if(collider)Release(collider);
#else
if(collider)UnityEngine.Object.Destroy(collider);
#endif

  if(!arenaMaterials.TryGetValue(color,out var material)){var shader=ArenaShader?ArenaShader:Shader.Find("Standard");material=new Material(shader);material.color=color;material.SetFloat("_Glossiness",.28f);arenaMaterials[color]=material;}o.GetComponent<Renderer>().sharedMaterial=material;return o;
 }
}

#endif

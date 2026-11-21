using System;
using System.Collections.Generic;
using UnityEngine;

// Same narrow API as r1's procedural model. Authored r3 geometry, not primitive substitutes.
public sealed class CrabModel {
 public Transform Root,Body;
 public float ForwardReach {get;private set;}
 public static Shader SurfaceShader,ArenaShader;
 static CrabR3Archive archive;
#if CRAB_KING_BOLD_R2
 const bool KingPrototype=true;
#else
 const bool KingPrototype=false;
#endif
#if UNITY_EDITOR
 static bool reviewBaseline;
 public static void ReviewUseBaseline(bool baseline){archive=null;materials=null;reviewBaseline=baseline;}
#endif
 static Material[] materials;
 static readonly Dictionary<Color,Material> arenaMaterials=new Dictionary<Color,Material>();
 readonly List<Mesh> ownedMeshes=new List<Mesh>();
 readonly List<Transform> arms=new List<Transform>(),legs=new List<Transform>(),fingers=new List<Transform>();
 readonly List<Quaternion> armBases=new List<Quaternion>(),legBases=new List<Quaternion>(),fingerBases=new List<Quaternion>();
 readonly List<int> armSides=new List<int>(),fingerSides=new List<int>();
 readonly List<CrabPartProxy> collisionParts=new List<CrabPartProxy>();
 public IReadOnlyList<CrabPartProxy> CollisionParts=>collisionParts;
 Vector3 bodyPosition;Quaternion bodyRotation;
 static CrabR3Archive Data {
  get {
   if(archive==null){var bytes=Resources.Load<TextAsset>(KingPrototype?"CrabKingBoldR2/king-bold-r2":"CrabR3/crab-r3");
#if UNITY_EDITOR
 if(reviewBaseline&&!KingPrototype)bytes=UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Editor/BaselineData/crab-r3.bytes");
#endif
 if(!bytes)throw new InvalidOperationException("r3 model resource missing");archive=CrabR3Archive.Read(bytes.bytes);}
   return archive;
  }
 }
 static bool SurfaceActive {get {
 if(KingPrototype)return false;
#if UNITY_EDITOR
 return CrabSurfaceBinding.Enabled&&!reviewBaseline;
#else
 return CrabSurfaceBinding.Enabled;
#endif
 }}
 static Material[] Materials(){
  if(materials!=null){for(int i=0;i<materials.Length;i++){if(!KingPrototype)CrabSurfaceBinding.RefreshDefinition(materials[i],i);}return materials;}
  var shader=SurfaceShader?SurfaceShader:Shader.Find("CustomCrabBattle/ChitinR3");if(!shader)throw new InvalidOperationException("r3 chitin shader missing");
  materials=new Material[Data.Materials.Length];
  for(int i=0;i<materials.Length;i++){var m=Data.Materials[i];materials[i]=new Material(shader){name=m.Name};var color=new Color(m.Color[0],m.Color[1],m.Color[2],m.Color[3]);if(KingPrototype){var role=m.Name.ToLowerInvariant();if(role.Contains("burgundy"))color=new Color(.88f,.14f,.055f,1);else if(role.Contains("pale"))color=new Color(.98f,.74f,.42f,1);else if(role.Contains("dark")||role.Contains("eyes"))color=new Color(.065f,.035f,.025f,1);}materials[i].SetColor("_Color",color);materials[i].SetFloat("_Roughness",KingPrototype?.43f:m.Roughness);if(!KingPrototype)CrabSurfaceBinding.Material(materials[i],i,SurfaceActive);}
  return materials;
 }
 public static CrabModel Make(string name,CrabBuild build,Color legacyActorColor){
  // Preserve authored r3 materials; legacy bright actor tint does not recolor exchangeable parts.
  var data=Data;var settings=new CrabR3Parameters(build.claw,build.shell,build.legs);var model=new CrabModel();var nodes=new Transform[data.Nodes.Length];var mats=Materials();model.ForwardReach=Mathf.Max(0,data.BoundsAt(settings)[5]);
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
    if(!KingPrototype)CrabSculptR6.Apply(source.Name,vertices,nn);
    var mesh=new Mesh{name="r3 "+source.Name};if(source.VertexCount>65535)mesh.indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;mesh.vertices=vertices;mesh.normals=nn;mesh.colors=colors;mesh.triangles=source.Indices;if(SurfaceActive){mesh.uv=CrabSurfaceBinding.UV(spec.Mesh,source.Name,source.VertexCount);mesh.RecalculateTangents();}mesh.RecalculateBounds();obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=mats[source.Material];model.ownedMeshes.Add(mesh);
   }
  }
  if(KingPrototype)for(int i=0;i<nodes.Length;i++)if(data.Nodes[i].PartRoot){
   var part=nodes[i];byte kind=data.Nodes[i].Kind;
   if(kind==1)part.localScale*=1.16f;
   else if(kind==2)part.localScale*=1.45f;
   else if(kind==3){part.localScale*=1.08f;ThickenKingLegR2(part);}
  }
  for(int i=0;i<nodes.Length;i++)if(data.Nodes[i].PartRoot){
   var tr=nodes[i];var b=new Bounds();bool first=true;
   foreach(var mf in tr.GetComponentsInChildren<MeshFilter>()){
    Transform owner=mf.transform;while(owner&& !IsPart(owner,nodes,data))owner=owner.parent;if(owner!=tr)continue;
    var mb=mf.sharedMesh.bounds;for(int c=0;c<8;c++){var q=mb.center+Vector3.Scale(mb.extents,new Vector3((c&1)==0?-1:1,(c&2)==0?-1:1,(c&4)==0?-1:1));q=tr.InverseTransformPoint(mf.transform.TransformPoint(q));if(first){b=new Bounds(q,Vector3.zero);first=false;}else b.Encapsulate(q);}
   }
   if(!first)model.collisionParts.Add(new CrabPartProxy{Id=data.Nodes[i].PartId,Kind=data.Nodes[i].Kind,Part=tr,LocalBounds=b});
  }
  if(model.collisionParts.Count!=11)throw new InvalidOperationException("11 anatomical collision bounds required");
  if(!model.Body||model.arms.Count!=2||model.legs.Count!=(KingPrototype?6:8)||model.fingers.Count!=2){model.Dispose();throw new InvalidOperationException("r3 articulation links incomplete");}
  return model;
 }
 // Preserve the authored six-leg path and joints; widen cross-sections rather
 // than adding more geometry or replacing the king-crab silhouette with boxes.
 static void ThickenKingLegR2(Transform part){
  var filters=part.GetComponentsInChildren<MeshFilter>();Vector3 sum=Vector3.zero;int count=0;
  foreach(var filter in filters){var toPart=part.worldToLocalMatrix*filter.transform.localToWorldMatrix;foreach(var p in filter.sharedMesh.vertices){sum+=toPart.MultiplyPoint3x4(p);count++;}}
  if(count==0||sum.sqrMagnitude<.000001f)return;var axis=sum.normalized;
  foreach(var filter in filters){var mesh=filter.sharedMesh;var points=mesh.vertices;var toPart=part.worldToLocalMatrix*filter.transform.localToWorldMatrix;var fromPart=toPart.inverse;
   for(int i=0;i<points.Length;i++){var p=toPart.MultiplyPoint3x4(points[i]);var along=axis*Vector3.Dot(axis,p);points[i]=fromPart.MultiplyPoint3x4(along+(p-along)*1.4f);}
   mesh.vertices=points;mesh.RecalculateNormals();mesh.RecalculateBounds();
  }
 }
 static bool IsPart(Transform tr,Transform[] nodes,CrabR3Archive data){for(int i=0;i<nodes.Length;i++)if(data.Nodes[i].PartRoot&&nodes[i]==tr)return true;return false;}
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
 public void BracePose(float amount){
  Body.localPosition=bodyPosition-Vector3.up*(amount*.12f);
  for(int i=0;i<arms.Count;i++)arms[i].localRotation=armBases[i]*Quaternion.Euler(amount*36,amount*armSides[i]*7,0);
  for(int i=0;i<legs.Count;i++)legs[i].localRotation=legBases[i]*Quaternion.Euler(amount*16,amount*10,0);
 }
 public void KickImpactPose(float amount){
  Body.localPosition=bodyPosition+Vector3.up*(amount*.08f);
  for(int i=0;i<arms.Count;i++)arms[i].localRotation=armBases[i]*Quaternion.Euler(-amount*30,amount*armSides[i]*8,0);
  for(int i=0;i<legs.Count;i++)legs[i].localRotation=legBases[i]*Quaternion.Euler(-amount*20,amount*5,0);
 }
 public void SetRoleRing(Color color){
  var obj=new GameObject("Ground role marker");obj.transform.SetParent(Root,false);
  var line=obj.AddComponent<LineRenderer>();line.useWorldSpace=false;line.loop=true;line.positionCount=48;line.startWidth=line.endWidth=.035f;line.numCornerVertices=2;
  for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;line.SetPosition(i,new Vector3(Mathf.Cos(a)*1.03f,.045f,Mathf.Sin(a)*1.03f));}
  var desired=ArenaShader?ArenaShader:Shader.Find("Standard");if(!arenaMaterials.TryGetValue(color,out var mat)||!mat||mat.shader!=desired){mat=new Material(desired);mat.color=color;mat.SetFloat("_Glossiness",0);arenaMaterials[color]=mat;}
  line.sharedMaterial=mat;line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
 }
 static void Release(UnityEngine.Object item){
#if UNITY_EDITOR
  if(!Application.isPlaying){UnityEngine.Object.DestroyImmediate(item);return;}
#endif
  UnityEngine.Object.Destroy(item);
 }
 public void Dispose(){if(Root)Release(Root.gameObject);foreach(var mesh in ownedMeshes)if(mesh)Release(mesh);ownedMeshes.Clear();}
 public static GameObject Shape(string name,Transform parent,PrimitiveType type,Vector3 pos,Vector3 size,Color color){
  var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=pos;o.transform.localScale=size;var collider=o.GetComponent<Collider>();if(collider){PrimitiveColliderTraceR18.CreatedDestroyScheduled(collider);Release(collider);}
  var shader=ArenaShader?ArenaShader:Shader.Find("Standard");if(!arenaMaterials.TryGetValue(color,out var material)||!material||material.shader!=shader){material=new Material(shader);material.color=color;material.SetFloat("_Glossiness",.28f);arenaMaterials[color]=material;}o.GetComponent<Renderer>().sharedMaterial=material;return o;
 }
}

public sealed class CrabPartProxy {public string Id;public byte Kind;public Transform Part;public Bounds LocalBounds;}

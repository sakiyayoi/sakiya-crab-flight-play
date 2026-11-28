using System.Collections.Generic;
using UnityEngine;

// Copies only meshes/material references. No cloned scripts, Rigidbody, Collider
// or physical Transform is ever moved. Owned objects are released on retry/disable.
public sealed class ObjectCaptureView : MonoBehaviour {
 readonly List<MeshRenderer> hidden=new List<MeshRenderer>();
 GameObject crabCopy,fisherCopy;
 Material fisherMaterial;
 Transform net;
 Vector3 start,target,netStart;
 Quaternion netRotation;
 Camera view;
 public void Begin(Transform crab,Transform owner,Vector3 origin,Vector3 fisher,Camera camera){
  Clear();view=camera;start=origin;float side=fisher.x<0?-1:1;
  target=fisher+new Vector3(-.5f*side,.2f,-.2f)*ObjectRules.ObjectScale;
  crabCopy=new GameObject("Captured crab render copy");crabCopy.transform.position=start;
  foreach(var r in crab.GetComponentsInChildren<MeshRenderer>())Copy(r);
  // Port eyes and neon glow are render-only siblings of the crab tree.
  foreach(var r in owner.GetComponentsInChildren<MeshRenderer>())
   if(r.name.StartsWith("Port readable eye ")||r.name=="Neon crab back glow")Copy(r);
  fisherCopy=ObjectVisual.Build(ObjectRules.Kind.Death,out fisherMaterial,out var motion);
  fisherCopy.name="Capture fisherman render copy";fisherCopy.transform.position=fisher;
  fisherCopy.transform.localScale=new Vector3(side,1,1);net=motion.net;netStart=net.position;netRotation=net.rotation;
 }
 void Copy(MeshRenderer source){
  if(!source.enabled||!source.gameObject.activeInHierarchy||hidden.Contains(source))return;
  var mesh=source.GetComponent<MeshFilter>();if(!mesh||!mesh.sharedMesh)return;
  var o=new GameObject("Capture mesh "+source.name);o.transform.SetParent(crabCopy.transform,false);
  o.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);o.transform.localScale=source.transform.lossyScale;
  o.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;var r=o.AddComponent<MeshRenderer>();r.sharedMaterials=source.sharedMaterials;
  var block=new MaterialPropertyBlock();source.GetPropertyBlock(block);r.SetPropertyBlock(block);
  r.shadowCastingMode=source.shadowCastingMode;r.receiveShadows=source.receiveShadows;
  hidden.Add(source);source.enabled=false;
 }
 public void Pose(ObjectCaptureTimeline.Stage stage,float progress,bool paused){
  if(!crabCopy)return;
  // The existing neon painter may re-enable a sibling glow; hide it after that.
  foreach(var r in hidden)if(r)r.enabled=false;
  if(stage==ObjectCaptureTimeline.Stage.Hold||paused)return;
  float t=stage==ObjectCaptureTimeline.Stage.Caption?1:Mathf.Clamp01(progress);
  float pull=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.2f,1,t));
  float struggle=Mathf.Sin(t*Mathf.PI*12)*.07f*Mathf.Sin(t*Mathf.PI);
  crabCopy.transform.position=Vector3.Lerp(start,target,pull)+view.transform.up*struggle;
  crabCopy.transform.rotation=Quaternion.AngleAxis(struggle*60,view.transform.forward);
  // First cast the fisher's net over the crab, then pull the covered crab in.
  net.position=Vector3.Lerp(netStart,crabCopy.transform.position-view.transform.forward*.9f,Mathf.Clamp01(t/.2f));
  net.rotation=Quaternion.Slerp(netRotation,view.transform.rotation,Mathf.Clamp01(t/.2f));
  net.localScale=Vector3.one*Mathf.Lerp(1,1.7f,Mathf.Clamp01(t/.2f));
 }
 public void Clear(){
  foreach(var r in hidden)if(r)r.enabled=true;hidden.Clear();
  if(crabCopy){crabCopy.SetActive(false);Destroy(crabCopy);}if(fisherCopy){fisherCopy.SetActive(false);Destroy(fisherCopy);}if(fisherMaterial)Destroy(fisherMaterial);
  crabCopy=fisherCopy=null;fisherMaterial=null;net=null;
 }
 void OnDisable(){Clear();}
 void OnDestroy(){Clear();}
}

using System.Collections.Generic;using System.Linq;using UnityEngine;
public sealed partial class RigidCrabFlight {
 public MeshRenderer[] VisualTargetsFeelR27(string id,Vector3 focus){
  var roots=fragments.Where(o=>o&&o.name.Contains(" for "+id)).ToArray();
  if(string.IsNullOrEmpty(id)){var p=panels.Where(p=>!p.broken&&p.visual).OrderBy(p=>(p.position-focus).sqrMagnitude).FirstOrDefault();roots=p!=null?new[]{p.visual}:new GameObject[0];}
  return roots.SelectMany(o=>o.GetComponentsInChildren<MeshRenderer>()).Where(r=>!r.name.StartsWith("R27 visual")&&r.GetComponent<MeshFilter>()&&(r.enabled||r.GetComponentInChildren<FeelVisualProxyR27>())).ToArray();
 }
}
public sealed class FeelVisualProxyR27:MonoBehaviour { }
public sealed partial class CrabFlightGame {
 sealed class ObjectFeelR27{public MeshRenderer original,copy;public bool enabled;public float born,life,distance;public MaterialPropertyBlock before;public bool flashed;}
 readonly Dictionary<MeshRenderer,ObjectFeelR27> feelObjectsR27=new Dictionary<MeshRenderer,ObjectFeelR27>();
 ObjectFeelR27 ObjectVisualR27(MeshRenderer original){if(feelObjectsR27.TryGetValue(original,out var v))return v;var child=new GameObject("R27 visual "+original.name);child.transform.SetParent(original.transform,false);child.AddComponent<FeelVisualProxyR27>();child.AddComponent<MeshFilter>().sharedMesh=original.GetComponent<MeshFilter>().sharedMesh;var copy=child.AddComponent<MeshRenderer>();copy.sharedMaterials=original.sharedMaterials;copy.shadowCastingMode=original.shadowCastingMode;v=new ObjectFeelR27{original=original,copy=copy,enabled=original.enabled};original.enabled=false;feelObjectsR27.Add(original,v);return v;}
 void RestoreObjectFlashR27(){RestoreLifeFlashR6();foreach(var x in feelObjectsR27.Values)if(x.copy&&x.flashed){x.copy.SetPropertyBlock(x.before);x.flashed=false;}}
 void ClearObjectFeelR27(){RestoreLifeFlashR6();foreach(var x in feelObjectsR27.Values){if(x.original)x.original.enabled=x.enabled;if(x.copy)Destroy(x.copy.gameObject);}feelObjectsR27.Clear();}
 bool FlashObjectR27(FeelCueR27 cue,Color color){bool any=FlashLifeObjectR6(cue);foreach(var r in world.VisualTargetsFeelR27(cue.objectId,crab.Root.position)){var x=ObjectVisualR27(r);x.before=new MaterialPropertyBlock();x.copy.GetPropertyBlock(x.before);var block=new MaterialPropertyBlock();x.copy.GetPropertyBlock(block);block.SetColor("_Color",color);block.SetTexture("_MainTex",Texture2D.whiteTexture);x.copy.SetPropertyBlock(block);x.flashed=true;any=true;}return any;}
 bool KnockbackObjectR27(FeelCueR27 cue,float distance){bool any=false;foreach(var r in world.VisualTargetsFeelR27(cue.objectId,crab.Root.position)){var x=ObjectVisualR27(r);x.born=Time.realtimeSinceStartup;x.life=feelR27.drawing.knockbackMs/1000;x.distance=distance;any=true;}return any;}
 void DrawObjectFeelR27(float now){foreach(var key in feelObjectsR27.Keys.ToArray()){var x=feelObjectsR27[key];if(!x.original||!x.copy){feelObjectsR27.Remove(key);continue;}float q=x.life>0?Mathf.Clamp01((now-x.born)/x.life):1;var offset=FeelEffectsR27?cam.transform.right*(direction*x.distance*Mathf.Sin(q*Mathf.PI)):Vector3.zero;x.copy.transform.localPosition=x.original.transform.InverseTransformVector(offset);}}
}

using System;using System.Collections.Generic;using UnityEngine;
public sealed partial class RigidCrabFlight {
#if UNITY_EDITOR
 [Serializable]public class MotionR22 {public float time,x,y,low,remaining,spent,spin,impulse;public string part,surface,stage,fault;public bool contact;public int groundBounces,impacts;public Vector3 before,after,omega,normal;}
 [Serializable]public class IntrusionWitnessR22 {public float time,proxyLow,visibleLow;public Vector3 position;public Quaternion rotation;public string mesh;}
 [Serializable]public class MotionReportR22 {public List<MotionR22> rows;public IntrusionWitnessR22 worst;}
 readonly List<MotionR22> motionR22=new List<MotionR22>();IntrusionWitnessR22 intrusionR22;float nextMotionR22;
 public string ReviewMotionR22JSON()=>JsonUtility.ToJson(new MotionReportR22{rows=motionR22,worst=intrusionR22});
#endif
 [System.Diagnostics.Conditional("UNITY_EDITOR")]void ResetMotionR22(){
#if UNITY_EDITOR
 motionR22.Clear();intrusionR22=null;nextMotionR22=0;
#endif
 }
 [System.Diagnostics.Conditional("UNITY_EDITOR")]void RecordMotionR22(){
#if UNITY_EDITOR
 RecordStepR24();float low=Lowest();if(hasContact||low<.5f||Time>=nextMotionR22){nextMotionR22=Time+.05f;motionR22.Add(new MotionR22{time=Time,x=direction*Body.position.x,y=Body.position.y,low=low,remaining=RotorRemaining,spent=OverdriveSpent,spin=SpinRps,impulse=bestImpulse,part=contactPart!=null?contactPart.Id:"",surface=contactSurface,stage=Stage.ToString(),fault=AbortReason,contact=hasContact,groundBounces=OverdriveGroundBounces,impacts=ImpactID,before=incoming,after=Body.linearVelocity,omega=Body.angularVelocity,normal=contactNormal});}
 if(low<-.008f&&(intrusionR22==null||low<intrusionR22.proxyLow)){float visible=float.PositiveInfinity;string name="";foreach(var filter in root.GetComponentsInChildren<MeshFilter>()){var renderer=filter.GetComponent<MeshRenderer>();if(!renderer||!renderer.enabled||!renderer.gameObject.activeInHierarchy)continue;var m=filter.transform.localToWorldMatrix;foreach(var v in filter.sharedMesh.vertices){var q=m.MultiplyPoint3x4(v);if(q.y<visible){visible=q.y;name=filter.transform.parent.name;}}}intrusionR22=new IntrusionWitnessR22{time=Time,proxyLow=low,visibleLow=visible,position=Body.position,rotation=Body.rotation,mesh=name};}
#endif
 }
}

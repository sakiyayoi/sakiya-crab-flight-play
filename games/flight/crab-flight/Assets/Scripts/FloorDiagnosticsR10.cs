#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
using System;using System.Collections.Generic;using UnityEngine;
[Serializable] public sealed class FloorShapeR10 {public string id,type;public float aabbLow,supportLow,maxPenetration,pointVelocityY;public Vector3 separationDirection,supportPoint;}
[Serializable] public sealed class FloorFrameR10 {public float time,aabbLow,supportLow,impulse,maxDepenetration;public Vector3 contactPoint,contactNormal;public bool hasContact;public string contactSurface;public Vector3 position,velocity,omega;public Quaternion rotation;public FloorShapeR10[] shapes;}
[Serializable] public sealed class FloorSubstepR17 {public float time,low,pointVy,impulse;public int count,index,requested;public Vector3 position,velocity,omega,normal;}
[Serializable] public sealed class FloorReportR10 {public FloorFrameR10[] frames,worstFrames;public FloorSubstepR17[] substeps,worstSubsteps;}
public sealed partial class RigidCrabFlight {
 readonly Queue<FloorSubstepR17> floorSubstepsR17=new Queue<FloorSubstepR17>();FloorSubstepR17[] worstSubstepsR17=new FloorSubstepR17[0];float worstSubstepLowR17=float.PositiveInfinity;
 readonly Queue<FloorFrameR10> floorFramesR10=new Queue<FloorFrameR10>();FloorFrameR10[] worstFloorFramesR11=new FloorFrameR10[0];float worstSupportR11=float.PositiveInfinity;
 float GeometricSupportY(Collider shape){var t=shape.transform;var scale=t.lossyScale;scale=new Vector3(Mathf.Abs(scale.x),Mathf.Abs(scale.y),Mathf.Abs(scale.z));
  if(shape is BoxCollider b){var e=b.size*.5f;return t.TransformPoint(b.center).y-Mathf.Abs(t.TransformVector(new Vector3(e.x,0,0)).y)-Mathf.Abs(t.TransformVector(new Vector3(0,e.y,0)).y)-Mathf.Abs(t.TransformVector(new Vector3(0,0,e.z)).y);}
  if(shape is CapsuleCollider c){int a=c.direction;float along=a==0?scale.x:a==1?scale.y:scale.z,across=a==0?Mathf.Max(scale.y,scale.z):a==1?Mathf.Max(scale.x,scale.z):Mathf.Max(scale.x,scale.y),radius=c.radius*across;var axis=t.rotation*(a==0?Vector3.right:a==1?Vector3.up:Vector3.forward);return t.TransformPoint(c.center).y-radius-Mathf.Abs(axis.y)*Mathf.Max(0,c.height*along*.5f-radius);}
  if(shape is MeshCollider)return GeometricSupportPointR11(shape).y;throw new InvalidOperationException("Unexpected primitive");
 }

 public string FloorDiagnosticsJSON()=>JsonUtility.ToJson(new FloorReportR10{frames=floorFramesR10.ToArray(),worstFrames=worstFloorFramesR11,substeps=floorSubstepsR17.ToArray(),worstSubsteps=worstSubstepsR17});
 void ResetFloorDiagnosticsR10(){floorSubstepsR17.Clear();worstSubstepsR17=new FloorSubstepR17[0];worstSubstepLowR17=float.PositiveInfinity;floorFramesR10.Clear();worstFloorFramesR11=new FloorFrameR10[0];worstSupportR11=float.PositiveInfinity;}
 [System.Diagnostics.Conditional("FLOOR_DIAGNOSTICS")]
 [System.Diagnostics.Conditional("UNITY_EDITOR")]
 void ObserveFloorSubstepR17(int index,int count,int requested){float low=Lowest();if(low>.6f)return;float pointVy=0,minimum=float.PositiveInfinity;foreach(var c in colliders){var point=GeometricSupportPointR11(c);if(point.y<minimum){minimum=point.y;pointVy=Body.GetPointVelocity(point).y;}}var x=new FloorSubstepR17{time=Time+StepSeconds*(index+1)/(float)count,index=index,count=count,requested=requested,low=minimum,pointVy=pointVy,impulse=bestImpulse,normal=contactNormal,position=Body.position,velocity=Body.linearVelocity,omega=Body.angularVelocity};floorSubstepsR17.Enqueue(x);while(floorSubstepsR17.Count>128)floorSubstepsR17.Dequeue();if(minimum<worstSubstepLowR17){worstSubstepLowR17=minimum;worstSubstepsR17=floorSubstepsR17.ToArray();}}
 public string RotorDiagnosticsJSON()=>JsonUtility.ToJson(new RotorRunR17{capacity=RotorCapacity,remaining=RotorRemaining,reclaimed=ReclaimedRotorJ,spent=OverdriveSpent,impulses=OverdriveImpulses.ToArray()});
 [Serializable] public class RotorRunR17{public float capacity,remaining,reclaimed,spent;public OverdriveImpulse[] impulses;}
 [System.Diagnostics.Conditional("FLOOR_DIAGNOSTICS")]
 [System.Diagnostics.Conditional("UNITY_EDITOR")]
 void RecordFloorDiagnosticsR10(float low){if(low>1.3f)return;var rows=new List<FloorShapeR10>();float min=float.PositiveInfinity;var floors=new List<Collider>();foreach(var o in Scene.GetRootGameObjects())foreach(var c in o.GetComponentsInChildren<Collider>())if(c.name.StartsWith("Physical "))floors.Add(c);
  foreach(var c in colliders){float support=GeometricSupportY(c);min=Mathf.Min(min,support);var supportPoint=GeometricSupportPointR11(c);var row=new FloorShapeR10{supportPoint=supportPoint,pointVelocityY=Body.GetPointVelocity(supportPoint).y,id=Parts[c.GetInstanceID()].Id,type=c.GetType().Name,aabbLow=c.bounds.min.y,supportLow=support};foreach(var floor in floors){Vector3 direction;float depth;if(UnityEngine.Physics.ComputePenetration(c,c.transform.position,c.transform.rotation,floor,floor.transform.position,floor.transform.rotation,out direction,out depth)&&depth>row.maxPenetration){row.maxPenetration=depth;row.separationDirection=direction;}}rows.Add(row);}
  floorFramesR10.Enqueue(new FloorFrameR10{maxDepenetration=Body.maxDepenetrationVelocity,contactPoint=contactPoint,contactNormal=contactNormal,time=Time,aabbLow=low,supportLow=min,position=Body.position,velocity=Body.linearVelocity,omega=Body.angularVelocity,rotation=Body.rotation,hasContact=hasContact,contactSurface=contactSurface,impulse=bestImpulse,shapes=rows.ToArray()});while(floorFramesR10.Count>64)floorFramesR10.Dequeue();if(min<worstSupportR11){worstSupportR11=min;worstFloorFramesR11=floorFramesR10.ToArray();}
 }
}
public sealed partial class CrabFlightGame {
#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
 // Accepted-input fixture only: normal launch initialization, no pose/bits/trajectory override.
 public void ReviewReplayAccepted(string json){if(phase=="flight")return;var saved=JsonUtility.FromJson<FlightLaunchReceipt>(json);Prepare("");Configure(saved.settingsJson);direction=saved.direction;launchSpinRps=saved.spin;launchChargeEnergy=saved.charge;spinAngle=displaySpinAngle=saved.phase;crab.ChargePose(saved.charge);requestedLaunchSource="saved-accepted-fixture";Launch("");}
#endif
}
#endif
#if !FLOOR_DIAGNOSTICS && !UNITY_EDITOR
public sealed partial class RigidCrabFlight {void ResetFloorDiagnosticsR10(){} [System.Diagnostics.Conditional("FLOOR_DIAGNOSTICS")]void RecordFloorDiagnosticsR10(float low){} [System.Diagnostics.Conditional("FLOOR_DIAGNOSTICS")]void ObserveFloorSubstepR17(int index,int count,int requested){} }
#endif

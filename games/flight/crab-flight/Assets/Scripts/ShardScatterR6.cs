using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]public sealed class ScatterReceiptR6 {
 public string objectId;public float time,budget,scale;public double linear,angular,total;public int physical;public Vector3 point,normal;public Vector3[] velocity,omega;
}
public sealed partial class RigidCrabFlight {
 public static bool DirectionalShardsR6=true;
 readonly List<ScatterReceiptR6> scatterReceiptsR6=new List<ScatterReceiptR6>(96);
 public double ScatterMaxEnergyExcessR6{get;private set;}
 public int ScatterEventsR6=>scatterReceiptsR6.Count;
 // Unity inertiaTensor is diagonal in inertiaTensorRotation, relative to body rotation.
 // Include actual compound-box rotation energy; never add an unbounded force/torque.
 static double RotationalEnergyR6(Rigidbody rb){var w=Quaternion.Inverse(rb.rotation*rb.inertiaTensorRotation)*rb.angularVelocity;var I=rb.inertiaTensor;return .5*((double)I.x*w.x*w.x+(double)I.y*w.y*w.y+(double)I.z*w.z*w.z);}
 static double KineticEnergyR6(Rigidbody rb)=>.5*rb.mass*(double)rb.linearVelocity.sqrMagnitude+RotationalEnergyR6(rb);
 void ScatterShardsR6(List<Rigidbody> bodies,Vector3 point,Vector3 normal,Vector3 incoming,float budget,string id){
  if(bodies.Count==0)return;var flow=incoming.sqrMagnitude>.0001f?incoming.normalized:Vector3.right*direction;var n=normal.sqrMagnitude>.0001f?normal.normalized:-flow;float speed=Mathf.Min(incoming.magnitude,18);
  double proposed=0;
  for(int i=0;i<bodies.Count;i++){
   var rb=bodies[i];var offset=rb.worldCenterOfMass-point;var radial=Vector3.ProjectOnPlane(offset,flow);
   if(radial.sqrMagnitude<.0025f)radial=Vector3.Cross(flow,i%2==0?Vector3.up:Vector3.forward)*(i%2==0?1:-1);
   radial.Normalize();var v=flow*speed*(.72f+(i%3)*.08f)+radial*speed*.32f+Vector3.up*speed*(.12f+(i%2)*.07f);
   var axis=Vector3.Cross(offset,flow);if(axis.sqrMagnitude<.01f)axis=Vector3.Cross(n,Vector3.up);if(axis.sqrMagnitude<.01f)axis=Vector3.forward;
   rb.maxAngularVelocity=18;rb.ResetInertiaTensor();rb.linearVelocity=Vector3.ClampMagnitude(v,18);rb.angularVelocity=axis.normalized*(4+(i%3)*2)+flow*(i%2==0?1.5f:-1.5f);proposed+=KineticEnergyR6(rb);
  }
  // A 0.01% numerical reserve prevents float rounding from spending more than the budget.
  float scale=proposed>Math.Max(0,budget)?(float)Math.Sqrt(Math.Max(0,budget)/proposed)*.9999f:1;
  var rec=new ScatterReceiptR6{objectId=id,time=Time,budget=budget,scale=scale,physical=bodies.Count,point=point,normal=n,velocity=new Vector3[bodies.Count],omega=new Vector3[bodies.Count]};
  for(int i=0;i<bodies.Count;i++){var rb=bodies[i];rb.linearVelocity*=scale;rb.angularVelocity*=scale;rec.linear+=.5*rb.mass*(double)rb.linearVelocity.sqrMagnitude;rec.angular+=RotationalEnergyR6(rb);rec.velocity[i]=rb.linearVelocity;rec.omega[i]=rb.angularVelocity;}
  rec.total=rec.linear+rec.angular;ScatterMaxEnergyExcessR6=Math.Max(ScatterMaxEnergyExcessR6,rec.total-budget);if(scatterReceiptsR6.Count<128)scatterReceiptsR6.Add(rec);
 }
}

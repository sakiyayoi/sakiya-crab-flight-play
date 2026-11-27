using UnityEngine;

public sealed partial class RigidCrabFlight {
 // Read-only access to the live visual target: no predicted/duplicate movement,
 // collider queries, consumption, simulation state writes or authored data edits.
 public bool NextObjectCameraTarget(float x,int travelDirection,out Vector3 position){
  position=Vector3.zero;float nearest=float.PositiveInfinity;
  if(!UseObjectRedesign)return false;
  foreach(var a in actionObjects){
   if(a.consumed)continue;
   var p=a.visual.transform.position;float ahead=(p.x-x)*travelDirection;
   if(ahead<0||ahead>=nearest)continue;
   nearest=ahead;position=p;
  }
  return !float.IsPositiveInfinity(nearest);
 }
}

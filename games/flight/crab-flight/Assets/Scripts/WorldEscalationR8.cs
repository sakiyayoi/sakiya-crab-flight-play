using System;using System.Linq;using UnityEngine;using C2=CrabCityDensityR2;
// Existing contacts become regional landmarks; no new bodies, rewards or energy.
// Renderer and collision both inherit this same placement scale.
public static class WorldEscalationR8 {
 public static bool Enabled=true;
 public static C2.CityDensityPack.Placement[] Layout(C2.CityDensityPack.Placement[] source){
  if(!Enabled)return source;return source.Select(p=>{
   float x=Mathf.Abs(p.p[0]);Vector3 growth=Vector3.one;
   if(p.breakable&&p.asset=="delivery_van"&&Mathf.Abs(x-344)<.05f)growth=new Vector3(1.4f,1.4f,1.4f);
   else if(p.breakable&&p.asset=="port_lattice_loader_r10"&&Mathf.Abs(x-750.8f)<.05f)growth=new Vector3(1.7f,2.2f,1.5f);
   else if(p.breakable&&p.asset=="irrigation_tank_r12"&&Mathf.Abs(x-1605)<.05f)growth=Vector3.one*2.6f;
   else if(p.breakable&&p.asset=="port_lattice_loader_r10"&&Mathf.Abs(x-2110)<.05f)growth=new Vector3(1.9f,3.0f,1.5f);
   else if(p.breakable&&p.asset=="solar_array_r12"&&Mathf.Abs(x-2740)<.05f)growth=new Vector3(1.6f,4.0f,1.8f);
   // The world grows past the mall, but these existing charge targets stayed
   // only 3m high and were routinely below the actual country flight corridor.
   // Grow their visible geometry and collision together: same unique IDs,
   // target count and finite one-break resource payment, never an idle refill.
   else if(p.breakable&&p.asset=="flywheel_accumulator_r12"&&x>=1300)
    growth=Vector3.one*(x<2050?2.5f:x<2680?3.5f:4.5f);
   if(growth==Vector3.one)return p;
   return new C2.CityDensityPack.Placement{id=p.id,asset=p.asset,band=p.band,p=p.p,s=new[]{p.s[0]*growth.x,p.s[1]*growth.y,p.s[2]*growth.z},yaw=p.yaw,breakable=p.breakable,chunk=p.chunk,chainGroup=p.chainGroup};
  }).ToArray();
 }
}

#if UNITY_EDITOR
public sealed partial class RigidCrabFlight {
 public string ReviewLandmarkR8(string id){
  var p=panels.Find(x=>x.id==id);if(p==null)throw new Exception("Missing actual landmark "+id);
  Body.rotation=Quaternion.Euler(0,90,0);root.rotation=Body.rotation;UnityEngine.Physics.SyncTransforms();
  var bounds=p.shapes[0].bounds;foreach(var c in p.shapes)bounds.Encapsulate(c.bounds);
  float radius=0;foreach(var c in colliders)radius=Mathf.Max(radius,c.bounds.extents.x);
  Body.position=new Vector3(direction>0?bounds.min.x-radius-1:bounds.max.x+radius+1,Mathf.Max(4,bounds.center.y*.45f),0);root.position=Body.position;UnityEngine.Physics.SyncTransforms();lockedComZ=Body.worldCenterOfMass.z;
  Body.linearVelocity=new Vector3(direction*45,0,0);Body.angularVelocity=new Vector3(0,.28f,-.96f).normalized*(direction*8*Mathf.PI*2);Body.WakeUp();return id;
 }
}
#endif

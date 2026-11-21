using System;using UnityEngine;
// Resolve a rapidly moving compound against the ground before deep overlap.
// Same shapes, materials, energy, outer 1/240 clock and safety thresholds.
public sealed partial class RigidCrabFlight {
 public bool UseFloorContactSteps=true;
 public int FloorContactExtraSteps {get;private set;}
 public int FloorContactClampedSteps {get;private set;}
 public float FloorSweepRadius=>floorSweepRadius;public float MinGeometricSupportR21 {get;private set;}
 readonly System.Collections.Generic.Dictionary<Mesh,Vector3[]> hullVerticesR21=new System.Collections.Generic.Dictionary<Mesh,Vector3[]>();
 float floorSweepRadius,previousFloorLow=float.PositiveInfinity;int deepFloorSteps,supportDeepFloorSteps;
 void ResetFloorContactSteps(){ResetSupportGeometryR14();ResetMotionR22();FloorContactExtraSteps=FloorContactClampedSteps=deepFloorSteps=supportDeepFloorSteps=0;previousFloorLow=float.PositiveInfinity;MinGeometricSupportR21=float.PositiveInfinity;floorSweepRadius=Mathf.Max(1,RotationRadius());var com=Body.worldCenterOfMass;foreach(var c in colliders){var b=c.bounds;for(int k=0;k<8;k++){var corner=b.center+Vector3.Scale(b.extents,new Vector3((k&1)==0?-1:1,(k&2)==0?-1:1,(k&4)==0?-1:1));floorSweepRadius=Mathf.Max(floorSweepRadius,Vector3.Distance(com,corner));}}}
 Vector3 GeometricSupportPointR11(Collider shape){if(!CalmSupportPerfR3.Active)return GeometricSupportPointRawCalmR3(shape);long tick=CalmSupportPerfR3.Stamp();var point=GeometricSupportPointRawCalmR3(shape);CalmSupportPerfR3.Ticks+=CalmSupportPerfR3.Stamp()-tick;CalmSupportPerfR3.Calls++;return point;}
 Vector3 GeometricSupportPointLegacyR14(Collider shape){var t=shape.transform;if(shape is BoxCollider b){var q=t.TransformPoint(b.center);var e=b.size*.5f;var ax=t.TransformVector(new Vector3(e.x,0,0));var ay=t.TransformVector(new Vector3(0,e.y,0));var az=t.TransformVector(new Vector3(0,0,e.z));q+=ax.y>=0?-ax:ax;q+=ay.y>=0?-ay:ay;q+=az.y>=0?-az:az;return q;}if(shape is CapsuleCollider c){var s=t.lossyScale;s=new Vector3(Mathf.Abs(s.x),Mathf.Abs(s.y),Mathf.Abs(s.z));int i=c.direction;float along=i==0?s.x:i==1?s.y:s.z,across=i==0?Mathf.Max(s.y,s.z):i==1?Mathf.Max(s.x,s.z):Mathf.Max(s.x,s.y);float r=c.radius*across;var axis=t.rotation*(i==0?Vector3.right:i==1?Vector3.up:Vector3.forward);return t.TransformPoint(c.center)-axis*(axis.y>=0?1:-1)*Mathf.Max(0,c.height*along*.5f-r)-Vector3.up*r;}if(shape is MeshCollider mesh){if(!hullVerticesR21.TryGetValue(mesh.sharedMesh,out var points)){points=mesh.sharedMesh.vertices;hullVerticesR21.Add(mesh.sharedMesh,points);}var matrix=t.localToWorldMatrix;Vector3 best=Vector3.up*float.PositiveInfinity;foreach(var v in points){var q=matrix.MultiplyPoint3x4(v);if(q.y<best.y)best=q;}return best;}throw new InvalidOperationException("Unknown primitive");}
 void SimulateFloorContact(){
  int count=1,requested=1;
  float low=Lowest();
  // Do not perturb ordinary contact/bounces: intervene only after persistent
  // real overlap deepens twice in the same intrusion episode (not necessarily
  // consecutive); latch until support clears -0.12m. Safety stop remains -0.45m.
  if(low>-.12f)deepFloorSteps=supportDeepFloorSteps=0;
  else if(low<-.25f&&low<previousFloorLow-.002f){supportDeepFloorSteps++;if(Body.linearVelocity.y<0)deepFloorSteps++;}
  previousFloorLow=low;
  // Preserve ordinary contacts; rescue a rotating support only when its
  // real point velocity predicts crossing the unchanged -0.45m safety plane.
  bool tipRisk=false;
  if(UseFloorContactSteps&&supportDeepFloorSteps>=2&&low<-.25f){Collider lowest=null;float y=float.PositiveInfinity;foreach(var c in colliders)if(c.bounds.min.y<y){y=c.bounds.min.y;lowest=c;}float pointVy=Body.GetPointVelocity(GeometricSupportPointR11(lowest)).y;tipRisk=low+Mathf.Min(0,pointVy)*StepSeconds<-.45f;}
  // The enhanced rotor can move a compound tip downward while its COM rises.
  // Start the same bounded solver substeps before that real support enters
  // the floor; waiting for deep overlap is too late at the new angular speed.
  // The motion budget matches the existing8mm contact offset, not40mm.
  //64 is a bounded contact-local budget; the previous8 exhausted the
  // requested11/12 steps in the observed35deg intrusion. No pose correction.
  // A rising current support can cease being the lowest vertex during this
  // step. Keep the conservative whole-body sweep, not its instantaneous tip.
  // The 16deg case crossed13.2cm when the narrow tip filter returned to1step.
  bool rotatingEntry=UseFloorContactSteps&&UseOverdrive&&UseEnhancedSpinR16&&
   low+(Mathf.Min(0,Body.linearVelocity.y)-Body.angularVelocity.magnitude*floorSweepRadius)*StepSeconds<.04f;
  // Contact position solving can deepen an existing overlap while both COM
  // and the instantaneous support velocity point upward (baseline35deg).
  // An observed persistent real overlap must therefore use bounded contact
  // substeps independently of that velocity prediction.
  bool overlapRecovery=UseFloorContactSteps&&supportDeepFloorSteps>=2&&low<-.25f;
  if(UseFloorContactSteps&&(deepFloorSteps>=2||tipRisk||rotatingEntry||overlapRecovery)){
   // Tangential movement can hit a second object and torque a grounded part
   // during this step. Budget the full translation, not only vertical fall.
   float travel=(Body.linearVelocity.magnitude+Body.angularVelocity.magnitude*floorSweepRadius)*StepSeconds;
   requested=Mathf.CeilToInt(Mathf.Max(travel,overlapRecovery?-low:0)/.008f);if(requested>64)FloorContactClampedSteps++;count=Mathf.Clamp(requested,1,64);
  }
  PhysXProfilerTraceR18.BeginScene(1,Scene.handle);
  for(int i=0;i<count;i++){
#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
BeforeSubR24();
#endif
PrimitiveColliderTraceR18.BeforeMain(Scene,Time,"floor");long calmSimStamp=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;Physics.Simulate(StepSeconds/count);if(CalmSupportPerfR3.Active){long ticks=CalmSupportPerfR3.Stamp()-calmSimStamp;CalmSupportPerfR3.SimulateTicks+=ticks;CalmSupportPerfR3.MainSimulateTicks+=ticks;CalmSupportPerfR3.MainSimulateCalls++;}
#if FLOOR_DIAGNOSTICS || UNITY_EDITOR
AfterSubR24(i,count);
#endif
ObserveFloorSubstepR17(i,count,requested);foreach(var c in colliders)MinGeometricSupportR21=Mathf.Min(MinGeometricSupportR21,GeometricSupportPointR11(c).y);}
  PhysXProfilerTraceR18.EndScene(1);
  FloorContactExtraSteps+=count-1;
 }
}
public sealed partial class CrabFlightGame {public bool useFloorContactSteps=true;}

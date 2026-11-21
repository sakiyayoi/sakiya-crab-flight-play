using System;using System.Collections.Generic;using UnityEngine;
[Serializable]public class ReclaimEventR12{public string id;public float time,joules,before,after;public bool secondary;}
public sealed partial class RigidCrabFlight{
 public bool UseProgressionR12;
 public float ReclaimedRotorJ{get;private set;}public int ReclaimedNodes{get;private set;}public float LastReclaimJ{get;private set;}
 public readonly List<ReclaimEventR12> ReclaimEventsR12=new List<ReclaimEventR12>();
 void ResetReclaimR12(){ReclaimedRotorJ=LastReclaimJ=0;ReclaimedNodes=0;ReclaimEventsR12.Clear();}
 void RecoverRotorFromBreakR12(string id,bool secondary){
  if(!UseProgressionR12||!id.StartsWith("r12-flywheel-",StringComparison.Ordinal))return;
  // One authored accumulator pays once because the existing panel is already marked broken.
  // Arcade stored environmental work, not a claim of energy conservation in real destruction.
  float energy=9000,before=RotorRemaining;RotorRemaining+=energy;RotorCapacity+=energy;ReclaimedRotorJ+=energy;LastReclaimJ=energy;ReclaimedNodes++;
  ReclaimEventsR12.Add(new ReclaimEventR12{id=id,time=Time,joules=energy,before=before,after=RotorRemaining,secondary=secondary});
 }
}
public sealed partial class CrabFlightGame{public bool useProgressionR12=true;}

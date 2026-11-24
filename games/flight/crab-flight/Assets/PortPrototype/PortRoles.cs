using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable] public sealed class PortRoleReceipt {
 public string id,role,cause;public float time;public Vector3 before,after;public bool reversed;
}
// Authored effects only. Existing collider creation, material masses, threshold and
// debris-contact chain rules remain unchanged. A wave is explicitly not a contact.
public sealed partial class RigidCrabFlight {
 sealed class PortWave {public BreakPanel target;public Vector3 from;public float due;public bool support;public string cause;}
 readonly List<PortWave> portWaves=new List<PortWave>();
 readonly HashSet<string> portScheduled=new HashSet<string>();
 readonly HashSet<string> portChainScheduled=new HashSet<string>();
 readonly HashSet<BreakPanel> portSupportPending=new HashSet<BreakPanel>();
 readonly HashSet<BreakPanel> portWavePending=new HashSet<BreakPanel>();
 readonly List<BreakPanel> portDirectRoles=new List<BreakPanel>();
 public readonly List<PortRoleReceipt> PortRoleReceipts=new List<PortRoleReceipt>();
 bool portAirBoost;bool portGroundThisStep;
 public bool PortReversalReady=>portAirBoost;
 const float PortWaveImpulsePerMass=2f,PortWaveDelay=.065f;
 void ObservePortContact(int collider,Vector3 normal){
  if(!PortLayout.Enabled)return;
  if(!breakByCollider.ContainsKey(collider)&&normal.y>.45f){portAirBoost=false;portGroundThisStep=true;}
 }
 void QueuePortWaves(){
  if(!PortLayout.Enabled)return;
  for(int i=portWaves.Count-1;i>=0;i--){var w=portWaves[i];if((NewLoopActive?newLoopWaveClock:Time)<w.due)continue;portWaves.RemoveAt(i);
   var p=w.target;if(NewLoopActive){QueueNewLoopWave(w);continue;}if(p.broken||p.cityProposal.massKgSuggestion>100||Vector3.Distance(w.from,p.position)>10)continue;
   if(primaryPending.Contains(p)&&breakImpulses.TryGetValue(p,out var contactImpulse)&&contactImpulse/Body.mass>=p.threshold)continue;
   // Deterministic authored blast, with its own finite impulse, tested against the unchanged full threshold.
   pendingBreaks[p]=p.position+Vector3.up;breakNormals[p]=Vector3.up;breakImpulses[p]=Body.mass*PortWaveImpulsePerMass;secondarySources.Remove(p);portWavePending.Add(p);if(w.support)portSupportPending.Add(p);
  }
 }
 void ObservePortBreak(BreakPanel p,bool wave,bool secondary){
  var e=PortLayout.Find(p.id);if(e==null||NewLoopActive&&(!NewLoopReleased||NewLoopParked))return;
  if(!wave&&!secondary&&e.role.Length>0)portDirectRoles.Add(p);
  if(NewLoopActive){
   // ON has already scheduled the entire authored graph at ignition. Do not let
   // ordinary wave/support targets enroll more objects outside its radius/count.
   if(!wave&&!secondary&&!newLoopDirectPending.ContainsKey(p))ScheduleNewLoopWave(p);
   if(wave)PortRoleReceipts.Add(new PortRoleReceipt{id=p.id,role="chain",cause=NewLoopBreakCause(p,true,secondary),time=Time,before=Body.linearVelocity,after=Body.linearVelocity});
   return;
  }
  // A destroyed support cannot leave its authored upper tier suspended. This uses
  // the same finite role-effect path; it never edits masses, colliders or thresholds.
  foreach(var child in panels){var c=PortLayout.Find(child.id);
   if(NewLoopActive&&!NewLoopTarget(child))continue;
   if(c==null||c.support!=e.key||child.broken||!child.city2||portScheduled.Contains(child.id))continue;
   portScheduled.Add(child.id);portWaves.Add(new PortWave{target=child,from=p.position,due=Time+PortWaveDelay,support=true});
  }
  if(wave)PortRoleReceipts.Add(new PortRoleReceipt{id=p.id,role="chain",cause=portSupportPending.Contains(p)?"support-collapse":e.chain,time=Time,before=Body.linearVelocity,after=Body.linearVelocity});
  bool trigger=e.role.StartsWith("boost",StringComparison.Ordinal)||e.role=="jackpot";
  bool enrolled=portChainScheduled.Contains(p.id);
  if(e.chain.Length==0||(!enrolled&&!trigger)||secondary&&!enrolled)return;
  portScheduled.Add(p.id);
  if(NewLoopActive&&ScheduleNewLoopWave(p))return;
  foreach(var next in panels){var n=PortLayout.Find(next.id);if(NewLoopActive&&!NewLoopTarget(next))continue;
   if(n==null||n.chain!=e.chain||next.broken||!next.city2||next.cityProposal.massKgSuggestion>100)continue;
   if(Vector3.Distance(p.position,next.position)>8)continue;
   portChainScheduled.Add(next.id);if(portScheduled.Contains(next.id))continue;
   portScheduled.Add(next.id);portWaves.Add(new PortWave{target=next,from=p.position,due=Time+PortWaveDelay});
  }
 }
 void ApplyPortRoles(){
  foreach(var p in portDirectRoles){var e=PortLayout.Find(p.id);if(e==null||NewLoopActive&&(!NewLoopReleased||NewLoopParked||NewLoopBeamActive))continue;var v=Body.linearVelocity;var before=v;bool reversed=false;
   var result=PortRules.Role(e.role,new PortRules.Velocity(v.x,v.y,v.z),direction,portGroundThisStep,ref portAirBoost,out reversed);
   v=new Vector3(result.x,result.y,result.z);
   Body.linearVelocity=v;
   PortRoleReceipts.Add(new PortRoleReceipt{id=p.id,role=e.role,cause="direct-contact",time=Time,before=before,after=v,reversed=reversed});
  }
  portDirectRoles.Clear();portWavePending.Clear();portSupportPending.Clear();portGroundThisStep=false;
 }
 void ResetPortRoles(){portWaves.Clear();portScheduled.Clear();portChainScheduled.Clear();portWavePending.Clear();portSupportPending.Clear();portDirectRoles.Clear();PortRoleReceipts.Clear();portAirBoost=portGroundThisStep=false;}
}

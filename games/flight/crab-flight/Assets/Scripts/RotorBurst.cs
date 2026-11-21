using System;
using System.Collections.Generic;
using UnityEngine;

// Explicit arcade flywheel credit from the ten-second input, not a claim of
// physically conserved Rigidbody kinetic energy. Only launch and actual, unique
// primary destruction or bounded automatic ground bounce can spend it. Nothing is earned by waiting or ground taps.
[Serializable] public sealed class RotorBurstEvent {
 public string objectId;public float time,energySpent,remaining,beforeForward,afterForward;public Vector3 position;
}
public sealed partial class RigidCrabFlight {
 #if UNITY_EDITOR
 public static float ReviewCoreRotorCoefficient=-1;public static bool ReviewSuppressCoreObjectBurst;
#endif
 public bool UseRotorBursts=true;public bool UseCeilingRicochet=true;public bool UseReadableSurfaces=true;
 public readonly List<RotorBurstEvent> BurstEvents=new List<RotorBurstEvent>();
 public float RotorCapacity {get;private set;}public float RotorRemaining {get;private set;}public float RotorPulseCapacity{get;private set;}
 public float RotorFraction=>RotorCapacity>0?RotorRemaining/RotorCapacity:0;
 readonly HashSet<string> burstObjects=new HashSet<string>();float lastBurstTime=-100;
 public int DestructionBoostCount=>BurstEvents.Count;
 public float LastBurstGain {get;private set;}
 void ResetRotor(){RotorPulseCapacity=RotorCapacity=RotorRemaining=LastBurstGain=0;lastBurstTime=-100;BurstEvents.Clear();burstObjects.Clear();ResetOverdriveBudget();ResetReclaimR12();}
 void ChargeRotor(float spin){
  ResetRotor();if(!UseRotorBursts)return;
  float ratio=Mathf.Max(0,Mathf.Abs(spin)/22),speed=(float)settings.LaunchSpeed;
  float coreCoefficient=.56f;
#if UNITY_EDITOR
  if(ReviewCoreRotorCoefficient>=0)coreCoefficient=ReviewCoreRotorCoefficient;
#endif
  // Calibrate the per-contact pulse independently of finite total stock.
  // Extra charge is not an automatic stronger opening collision or a refill.
  RotorPulseCapacity=(UseOverdrive?.50f:.30f)*Body.mass*speed*speed*(Mathf.Log(1+ratio*ratio)/Mathf.Log(2));
  RotorCapacity=RotorRemaining=(UseOverdrive?(UseCoreDestruction?coreCoefficient:.50f):.30f)*Body.mass*speed*speed*(Mathf.Log(1+ratio*ratio)/Mathf.Log(2));
  // Launch keeps the actual swipe angle. First real break releases charge forward.
 }
 void ReleaseBreakRotor(string id,float weight){
#if UNITY_EDITOR
  if(ReviewSuppressCoreObjectBurst&&id.StartsWith("core-",StringComparison.Ordinal))return;
#endif
  if(!UseRotorBursts||!burstObjects.Add(id)||Time-lastBurstTime<.09f)return;
  if(UseOverdrive)SpendOverdriveRotor(id,RotorPulseCapacity*(.065f+.13f*Mathf.Clamp01(weight)),weight);else SpendRotor(id,RotorCapacity*(.10f+.15f*Mathf.Clamp01(weight)),false);
 }
 void SpendRotor(string id,float request,bool launch){
  float before=direction*Body.linearVelocity.x;if(RotorRemaining<=.001f||before<.5f)return;
  float amount=Mathf.Min(RotorRemaining,Mathf.Max(0,request));var v=Body.linearVelocity;
  float limit=Mathf.Max(0,.5f*Body.mass*(72*72-before*before));amount=Mathf.Min(amount,limit);if(amount<=.001f)return;
  float after=Mathf.Sqrt(before*before+2*amount/Body.mass);
  if(launch){float scale=after/Mathf.Max(.01f,before);v*=scale;}else v.x=direction*after;
  float was=RotorRemaining;RotorRemaining-=amount;
  // Spending charge visibly takes rotation out of the real body as well.
  Body.angularVelocity*=Mathf.Sqrt(Mathf.Clamp01(RotorRemaining/Mathf.Max(.001f,was)));
  Body.linearVelocity=v;Body.WakeUp();LastBurstGain=after-before;lastBurstTime=Time;
  BurstEvents.Add(new RotorBurstEvent{objectId=id,time=Time,energySpent=amount,remaining=RotorRemaining,beforeForward=before,afterForward=after,position=Body.position});
 }
}

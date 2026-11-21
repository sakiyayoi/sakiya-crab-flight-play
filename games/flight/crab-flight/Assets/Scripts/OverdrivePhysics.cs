using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class OverdriveImpulse {
 public string source;public float time,forwardJ,verticalJ,spinJ,spent,remaining,beforeSpeed,afterSpeed,beforeSpin,afterSpin;
}
// Candidate arcade flywheel: one finite charge pays for all added translation/rotation.
// This is not a physically conserved model of a real crab. No idle refill or chain-count bonus.
public sealed partial class RigidCrabFlight {
 public bool UseOverdrive;
 public const float OverdriveBodySpinLimit=28;
 public readonly List<OverdriveImpulse> OverdriveImpulses=new List<OverdriveImpulse>();
 public int OverdriveGroundBounces {get;private set;}
 // Keep the original three-bounce budget on the short prototype. The authored
 // nine-section route needs a finite whole-route allowance; every bounce still
 // spends the same launch/environment stock and obeys the same contact guards.
 public int OverdriveGroundLimit=>UseRouteBlockoutR11?Mathf.Max(3,WorldRouteR11.Data.sectors.Length*3):3;
 public float OverdriveSpent {get;private set;}
#if UNITY_EDITOR
 public float ReviewContactSpinLimitR17=40;
#endif
 bool overdriveFloorReady;float lastOverdriveBounce=-100;
 public void EnableOverdriveWorld(){
  if(!UseOverdrive||overdriveFloorReady)return;overdriveFloorReady=true;
  var mat=Material("Overdrive extended sand",.56f,.42f,.06f);
  foreach(float sign in new[]{-1f,1f})Floor("sand",new Vector3(sign*1000,-.25f,0),new Vector3(1200,.5f,20),mat);
 }
 void ResetOverdriveBudget(){OverdriveImpulses.Clear();OverdriveSpent=0;OverdriveGroundBounces=0;lastOverdriveBounce=-100;}
 bool SpendOverdriveRotor(string id,float request,float weight,bool ground=false){
  var v=Body.linearVelocity;float before=direction*v.x;if(!UseOverdrive||RotorRemaining<=.001f||before<(ground?4:.5f))return false;
  float available=Mathf.Min(RotorRemaining,Mathf.Max(0,request));if(available<=.001f)return false;
  float omega=Body.angularVelocity.magnitude;var axis=omega>.01f?Body.angularVelocity/omega:new Vector3(0,.28f,-.96f).normalized*direction;
  var local=Quaternion.Inverse(Body.rotation*Body.inertiaTensorRotation)*axis;var inertia=Body.inertiaTensor;float effective=Mathf.Max(.0001f,Vector3.Dot(Vector3.Scale(local,local),inertia));
  float spinLimit=ActiveBodySpinLimitR16;
#if UNITY_EDITOR
  if(UseEnhancedSpinR16&&id!="launch")spinLimit=ReviewContactSpinLimitR17;
#endif
  float nextOmega=Mathf.Min(spinLimit*Mathf.PI*2,Mathf.Sqrt(omega*omega+2*available*.18f/effective));nextOmega=Mathf.Max(omega,nextOmega);
  float spinJ=Mathf.Max(0,.5f*effective*(nextOmega*nextOmega-omega*omega));
  float verticalBudget=Mathf.Min(available-spinJ,available*(ground?.32f:.14f));
  bool ceiling=id.StartsWith("ceiling-")&&!CeilingBreakthrough;float targetY=(ground?5.6f:3.2f+2.4f*Mathf.Clamp01(weight))*(ceiling?-1:1);
  float nextY=ceiling?Mathf.Min(v.y,targetY):Mathf.Max(v.y,targetY);
  float desiredJ=Mathf.Max(0,.5f*Body.mass*(nextY*nextY-v.y*v.y));
  if(desiredJ>verticalBudget){float magnitude=Mathf.Sqrt(v.y*v.y+2*Mathf.Max(0,verticalBudget)/Body.mass);nextY=Mathf.Sign(nextY)*magnitude;}
  float verticalJ=Mathf.Max(0,.5f*Body.mass*(nextY*nextY-v.y*v.y));
  float forwardBudget=Mathf.Max(0,available-spinJ-verticalJ),after=Mathf.Min(80,Mathf.Sqrt(before*before+2*forwardBudget/Body.mass));after=Mathf.Max(before,after);
  float forwardJ=Mathf.Max(0,.5f*Body.mass*(after*after-before*before)),spent=Mathf.Min(available,forwardJ+verticalJ+spinJ);if(spent<=.001f)return false;
  v.x=direction*after;v.y=nextY;Body.linearVelocity=v;Body.angularVelocity=axis*nextOmega;Body.WakeUp();RotorRemaining=Mathf.Max(0,RotorRemaining-spent);OverdriveSpent+=spent;LastBurstGain=after-before;
  OverdriveImpulses.Add(new OverdriveImpulse{source=id,time=Time,forwardJ=forwardJ,verticalJ=verticalJ,spinJ=spinJ,spent=spent,remaining=RotorRemaining,beforeSpeed=before,afterSpeed=after,beforeSpin=omega/(2*Mathf.PI),afterSpin=nextOmega/(2*Mathf.PI)});
  if(!ground){lastBurstTime=Time;BurstEvents.Add(new RotorBurstEvent{objectId=id,time=Time,energySpent=spent,remaining=RotorRemaining,beforeForward=before,afterForward=after,position=Body.position});}
  return true;
 }
 void OverdriveGroundBounce(){
  if(!UseOverdrive||OverdriveGroundBounces>=OverdriveGroundLimit||Time-lastOverdriveBounce<.38f||incoming.y> -3||incomingSpin<2||contactNormal.y<.35f||(contactSurface!="sand"&&contactSurface!="float"))return;
  if(SpendOverdriveRotor("ground-"+(OverdriveGroundBounces+1),RotorPulseCapacity*.065f,.5f,true)){OverdriveGroundBounces++;lastOverdriveBounce=Time;}
 }
}

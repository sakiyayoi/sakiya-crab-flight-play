using System;
using UnityEngine;

// Ten seconds of active, distance-and-time-normalized hand work. Waiting adds no energy.
[Serializable] public sealed class SpinWorkInput { public float distance,seconds; public int direction=1; }
public sealed class SpinCharge {
 public const float Duration=10;
 // The short-loop input uses this SAME Work/Energy stock. Legacy Add/Tick is unchanged when false.
 public bool NewLoop, Held;
 public const float NewLoopChargeSeconds=NewLoopRules.ChargeSeconds;
 public bool OverSpun {get;private set;}
 public float OverSpinAge {get;private set;}
 public float Dizzy=>NewLoop&&OverSpun&&Held&&OverSpinAge<NewLoopRules.DizzySeconds?1:0;
 public bool BeamArmed=>NewLoop&&Energy>=1&&!OverSpun;
 public int Tier=>NewLoopRules.Tier(Energy);
 public float PowerScale=1,BodyLimit=22;public bool ContinuousWork,EnhancedSpinR16;public float Work {get;private set;}
 public float Elapsed {get;private set;} public float Energy {get;private set;}
 public bool Started {get;private set;} public bool Ready=>Started&&(NewLoop||Elapsed>=Duration);
 public float SpinRps=>Started?PowerScale*(ContinuousWork?.4f+21.6f*Mathf.Sqrt(Work):.4f+21.6f*Mathf.Pow(Energy,.7f)):0;
 public float BodySpinRps=>Started?(PowerScale>1?(EnhancedSpinR16?SoftBodyRps(SpinRps,BodyLimit):Mathf.Min(BodyLimit,SpinRps)):.4f+21.6f*Mathf.Pow(Energy,.7f)):0;
 // Requested flywheel work keeps growing; this soft body headroom is a solver limit, not a stock refill.
 public static float SoftBodyRps(float requested,float limit){float knee=28;return requested<=knee?requested:knee+(limit-knee)*(1-Mathf.Exp(-(requested-knee)/(limit-knee)));}
 public void Reset(){Elapsed=Energy=Work=0;Started=false;Held=false;OverSpun=false;OverSpinAge=0;}
 public void Begin(){Started=true;if(NewLoop)Held=true;}
 public void Tick(float dt){
  if(NewLoop){
   if(Started&&Held&&float.IsFinite(dt)){
    float t=Mathf.Clamp(dt,0,.1f);Elapsed+=t;
    if(Energy>=1||OverSpun){OverSpun=true;OverSpinAge+=t;Energy=Mathf.Max(0,Energy-t*NewLoopRules.DecayPerSecond);}
    else Energy=Mathf.Min(1,Energy+t/NewLoopChargeSeconds);
    Work=Energy; // same stock; overspin can never refill within this throw
   }
   return;
  }
  if(Started&&!Ready)Elapsed=Mathf.Min(Duration,Elapsed+Mathf.Clamp(dt,0,.1f));
 }
 public void Add(float distance,float seconds){
  if(NewLoop||!Started||Ready||!float.IsFinite(distance)||!float.IsFinite(seconds)||seconds<=0)return;
  float dt=Mathf.Clamp(seconds,1f/240,.25f),travel,speed;
  if(EnhancedSpinR16){double raw=Math.Max(0,(double)distance)/dt;speed=(float)(raw<=6?raw:6+3*Math.Log(1+(raw-6)/3));travel=speed*dt;}
  else{travel=Mathf.Clamp(distance,0,6*dt);speed=travel/dt;}
  float added=travel*.09f*(.35f+.65f*Mathf.Clamp01(speed/5));Work+=added;Energy=Mathf.Clamp01(ContinuousWork?Work:Energy+added);
 }
}

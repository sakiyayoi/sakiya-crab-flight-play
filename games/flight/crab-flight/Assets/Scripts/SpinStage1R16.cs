using UnityEngine;
public sealed partial class CrabFlightGame {
 public bool useEnhancedSpinR16=true;
#if UNITY_EDITOR
 public static bool ReviewLegacySpinR16;
 bool EnhancedSpinR16=>useEnhancedSpinR16&&!ReviewLegacySpinR16;
#else
 bool EnhancedSpinR16=>useEnhancedSpinR16;
#endif
 float BodySpinForStateR16()=>simulation!=null?simulation.SpinRps:spinCharge.Started?spinCharge.BodySpinRps:0;
 float RequestedSpinForStateR16()=>simulation!=null?launchSpinRps:spinCharge.Started?spinCharge.SpinRps:0;
}
public sealed partial class RigidCrabFlight {
 public bool UseEnhancedSpinR16;
 public const float EnhancedBodySpinLimitR16=40;
 float ActiveBodySpinLimitR16=>UseEnhancedSpinR16?EnhancedBodySpinLimitR16:OverdriveBodySpinLimit;
 float AngularSafetyR16=>UseEnhancedSpinR16?EnhancedBodySpinLimitR16*Mathf.PI*2+1:200;
 // Keep the original inertia. Additional real spin is purchased once from
 // actual input-derived stock, never counted as a destruction/boost event.
 void FundLaunchSpinR16(float requested){
  float afterOmega=Body.angularVelocity.magnitude,baseOmega=Mathf.Min(OverdriveBodySpinLimit,Mathf.Abs(requested))*Mathf.PI*2;
  if(afterOmega<=baseOmega)return;Vector3 axis=Body.angularVelocity/afterOmega;
  Vector3 local=Quaternion.Inverse(Body.rotation*Body.inertiaTensorRotation)*axis;float effective=Mathf.Max(.0001f,Vector3.Dot(Vector3.Scale(local,local),Body.inertiaTensor));
  float extra=.5f*effective*(afterOmega*afterOmega-baseOmega*baseOmega),spent=Mathf.Min(RotorRemaining,extra);afterOmega=Mathf.Sqrt(baseOmega*baseOmega+2*spent/effective);
  Body.angularVelocity=axis*afterOmega;RotorRemaining=Mathf.Max(0,RotorRemaining-spent);OverdriveSpent+=spent;
  OverdriveImpulses.Add(new OverdriveImpulse{source="launch-spin",time=0,spinJ=spent,spent=spent,remaining=RotorRemaining,beforeSpeed=Body.linearVelocity.magnitude,afterSpeed=Body.linearVelocity.magnitude,beforeSpin=baseOmega/(Mathf.PI*2),afterSpin=afterOmega/(Mathf.PI*2)});
 }

}

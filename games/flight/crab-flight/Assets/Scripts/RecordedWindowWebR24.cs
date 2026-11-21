#if FLOOR_DIAGNOSTICS
using System;using System.Linq;using System.Runtime.InteropServices;using UnityEngine;
public sealed partial class CrabFlightGame {
 [Serializable]class WindowInputR24{public string id,launchBody,joints;public float angle,power,spin,charge,phase;public int direction;public float[] dts;}
 [Serializable]class WindowInputsR24{public WindowInputR24[] cases;}
 [Serializable]class WindowResultR24{public string id,source,initialBody,initialJointSHA,firstContact,final,trace;public bool finished;public int frames;}
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern void FlightR24Report(string json);
#endif
 // Diagnostic only: saved accepted launch values and its exact supplied Tick window.
 // No pose/velocity override, no extra ticks to force a result, no score correction.
 public void ReviewRecordedWindowR24(string id){
  var c=JsonUtility.FromJson<WindowInputsR24>(Resources.Load<TextAsset>("ReplayInputR24").text).cases.Single(x=>x.id==id);
  Prepare("");settings.angle=c.angle;settings.power=c.power;settings.claw=settings.shell=settings.legs=45;settings.upgradeLevel=0;direction=c.direction;launchSpinRps=c.spin;launchChargeEnergy=c.charge;spinAngle=displaySpinAngle=c.phase;crab.ChargePose(c.charge);requestedLaunchSource="saved-accepted-window-r24";Launch("");simulation.ReviewObserveR24=true;
  var r=new WindowResultR24{id=id,source=SourceIdentityR23(),initialBody=launchReceipt.bodyJson,initialJointSHA=launchReceipt.jointSHA256};
  foreach(var dt in c.dts){if(Finished)break;Tick(dt);r.frames++;}
  r.finished=Finished;r.firstContact=simulation.FirstContactProbe;r.final=JsonUtility.ToJson(State());r.trace=simulation.ReviewTraceR24();
#if UNITY_WEBGL && !UNITY_EDITOR
  FlightR24Report(JsonUtility.ToJson(r));
#endif
 }
}
#endif

#if UNITY_EDITOR
using System;using System.Collections;using System.IO;using System.Collections.Generic;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
public sealed partial class FlightMeasurementPlayMode {
 [Serializable]class R30Command{public int id;public string method,payload;}
 [Serializable]class R30PowerRow{public float target,work,charge,actual,launchSpeed;public string receipt,acks;}
 [Serializable]class R30PowerReport{public List<R30PowerRow> rows=new List<R30PowerRow>();}
 [UnityTest,Timeout(240000)]public IEnumerator R30AcceptedPowerTarget(){
  var report=new R30PowerReport();Directory.CreateDirectory(Evidence);
  foreach(float target in new[]{55f,100f})foreach(float work in new[]{.002f,.05f}){
   yield return NewRetrySubject(false,true,true,true,true,true,true);int id=0;
   Action<string,string> command=(m,p)=>Call(current,"ReceiveInputR23",JsonUtility.ToJson(new R30Command{id=++id,method=m,payload=p}));
   command("Configure","{\"angle\":40,\"power\":"+target+",\"claw\":45,\"shell\":45,\"legs\":45}");command("BeginAim","");command("ReleaseLaunch","{\"angle\":40,\"power\":"+target+",\"direction\":1,\"eligible\":true}");
   Assert.That((string)Field(State(current),"phase"),Is.EqualTo("ready"),"uncharged release must not launch");Assert.That((string)Field(State(current),"launchRejectReason"),Is.EqualTo("charge-not-ready"));
   command("BeginSpinCharge","");for(int i=0;i<601;i++){command("AddSpinWork","{\"distance\":"+work+",\"seconds\":0.016666668,\"direction\":1}");Call(current,"Tick",1f/60);}
   File.WriteAllText(Path.Combine(Evidence,"target-before-release.json"),"{\"target\":"+target+",\"work\":"+work+",\"state\":"+JsonUtility.ToJson(State(current))+"}");
   command("BeginAim","");command("ReleaseLaunch","{\"angle\":40,\"power\":"+target+",\"direction\":1,\"eligible\":true}");
   var state=State(current);var launch=Field(state,"launchReceipt");var body=(Rigidbody)Property(Property(current,"ReviewSimulation"),"Body");var row=new R30PowerRow{target=target,work=work,charge=F(launch,"charge"),actual=F(launch,"power"),launchSpeed=body.linearVelocity.magnitude,receipt=JsonUtility.ToJson(launch),acks=JsonUtility.ToJson(state)};report.rows.Add(row);
   Assert.That((string)Field(state,"phase"),Is.EqualTo("flight"));Assert.That(F(launch,"targetPower"),Is.EqualTo(target));Assert.That(row.actual,Is.EqualTo(target));float expected=target*.49f*(.86f+45*.0048f)/Mathf.Pow(.8f+45*.012f+45*.004f,.32f);Assert.That(row.launchSpeed,Is.EqualTo(expected).Within(.001));
   Call(current,"ShutdownReviewPhysics");current=null;yield return null;
  }
  File.WriteAllText(Path.Combine(Evidence,"accepted-power-target.json"),JsonUtility.ToJson(report,true));
 }
}
#endif

using System;using System.Collections.Generic;using UnityEngine;
[Serializable] public sealed class ThrowInputR21 {public string method,payload;public int tick;}
[Serializable] public sealed class ThrowTicksR21 {public float dt;public int count;}
[Serializable] public sealed class ThrowRecordR21 {
 public string schema="flight.throw.r21.1",id,revision,sourceIdentity,status,stopReason,firstContact;
 public FlightLaunchReceipt launch;public List<ThrowInputR21> inputs;public List<ThrowTicksR21> ticks;
 public float distance,physicsSeconds,suppliedTickSeconds,minimumBounds,minimumSupport,remainingRotor;
 public int actualBreaks,secondaryBreaks;public Vector3 finalPosition,finalVelocity,finalOmega;
}
public sealed partial class CrabFlightGame {
 readonly List<ThrowInputR21> auditInputs=new List<ThrowInputR21>();readonly List<ThrowTicksR21> auditTicks=new List<ThrowTicksR21>();
 readonly string auditSession=Guid.NewGuid().ToString("N");int auditTick;float auditSupplied;ThrowRecordR21 activeThrow;string latestThrowAudit="";bool auditReportDirtyR32=true;
 void AuditInput(string method,string payload){if(phase=="ready"||phase=="flight")auditInputs.Add(new ThrowInputR21{method=method,payload=payload,tick=auditTick});}
 void AuditTick(float dt){if(phase!="flight"&&!(phase=="ready"&&spinCharge.Started))return;auditTick++;auditSupplied+=dt;if(auditTicks.Count>0&&auditTicks[auditTicks.Count-1].dt==dt)auditTicks[auditTicks.Count-1].count++;else auditTicks.Add(new ThrowTicksR21{dt=dt,count=1});}
 void AuditLaunch(){var identity=Resources.Load<TextAsset>("SourceIdentityR21");activeThrow=new ThrowRecordR21{revision=Application.version,id=auditSession+"-"+attempts,sourceIdentity=identity?identity.text:"UNSTAMPED",status="launched",launch=launchReceipt,inputs=auditInputs,ticks=auditTicks};latestThrowAudit=JsonUtility.ToJson(activeThrow);auditReportDirtyR32=true;PersistNativeThrowR21(latestThrowAudit);}
 void AuditFinish(){if(activeThrow==null||activeThrow.status!="launched")return;activeThrow.status="result";activeThrow.distance=lastDistance;activeThrow.stopReason=simulation.AbortReason.Length>0?simulation.AbortReason:simulation.LastEvent;activeThrow.physicsSeconds=simulation.Time;activeThrow.suppliedTickSeconds=auditSupplied;activeThrow.minimumBounds=simulation.MinBottom;activeThrow.minimumSupport=simulation.MinGeometricSupportR21;activeThrow.remainingRotor=simulation.RotorRemaining;activeThrow.actualBreaks=simulation.BreakEvents.Count;activeThrow.secondaryBreaks=simulation.CitySecondaryBreaks;activeThrow.firstContact=simulation.FirstContactProbe;activeThrow.finalPosition=simulation.Body.position;activeThrow.finalVelocity=simulation.Body.linearVelocity;activeThrow.finalOmega=simulation.Body.angularVelocity;latestThrowAudit=JsonUtility.ToJson(activeThrow);auditReportDirtyR32=true;PersistNativeThrowR21(latestThrowAudit);}
 void AuditPrepare(){if(activeThrow!=null&&activeThrow.status=="launched"){activeThrow.status="interrupted";activeThrow.stopReason="prepare-before-result";latestThrowAudit=JsonUtility.ToJson(activeThrow);auditReportDirtyR32=true;PersistNativeThrowR21(latestThrowAudit);}else latestThrowAudit="";auditReportDirtyR32=true;activeThrow=null;auditInputs.Clear();auditTicks.Clear();auditTick=0;auditSupplied=0;AuditInput("Prepare","");}
 static void PersistNativeThrowR21(string json){
#if !UNITY_WEBGL || UNITY_EDITOR
  var folder=System.IO.Path.Combine(Application.persistentDataPath,"throw-receipts-r21");System.IO.Directory.CreateDirectory(folder);System.IO.File.AppendAllText(System.IO.Path.Combine(folder,"throws.jsonl"),json+"\n");
#endif
 }
}

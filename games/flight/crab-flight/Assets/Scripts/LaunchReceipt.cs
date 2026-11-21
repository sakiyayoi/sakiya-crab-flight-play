using System;using System.Collections.Generic;using UnityEngine;
[Serializable]public sealed class FlightLaunchReceipt {public string source,settingsJson,bodyJson;public float angle,targetPower,power,spin,charge,phase;public int direction,upgrade,serial;public bool reduced;public int[] jointBits;public string jointSHA256;}
[Serializable]public sealed class RigidLaunchReceipt {public int[] bodyBits,inertiaBits,shapeBits;public string shapeSHA256;public int fragments,cityFragments,breaks;public string[] shapeIDs;}
[Serializable]public sealed class FirstContactReceipt {public float time,impulse;public string part,surface,bodyJson;public Vector3 point,normal,incoming;}
public static class LaunchFingerprint {public static string Hash(int[] bits){var bytes=new byte[bits.Length*4];for(int i=0;i<bits.Length;i++)for(int k=0;k<4;k++)bytes[i*4+k]=(byte)((uint)bits[i]>>(8*k));using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}public static int[] Full(int[] bits){
#if UNITY_EDITOR
return bits;
#else
return null;
#endif
}}
public sealed partial class RigidCrabFlight {
 public string FirstContactProbe="";
 public string LaunchBodyReceipt(){var b=Body;var a=new[]{b.position.x,b.position.y,b.position.z,b.rotation.x,b.rotation.y,b.rotation.z,b.rotation.w,b.linearVelocity.x,b.linearVelocity.y,b.linearVelocity.z,b.angularVelocity.x,b.angularVelocity.y,b.angularVelocity.z,b.mass};var tensor=b.inertiaTensor;var q=b.inertiaTensorRotation;var c=b.centerOfMass;var inertia=new[]{c.x,c.y,c.z,tensor.x,tensor.y,tensor.z,q.x,q.y,q.z,q.w};var bits=new List<int>();var ids=new List<string>();foreach(var shape in colliders){var t=shape.transform;var p=t.localPosition;var rot=t.localRotation;var scale=t.localScale;ids.Add(Parts[shape.GetInstanceID()].Id);foreach(float f in new[]{p.x,p.y,p.z,rot.x,rot.y,rot.z,rot.w,scale.x,scale.y,scale.z})bits.Add(BitConverter.SingleToInt32Bits(f));if(shape is BoxCollider box){foreach(float f in new[]{box.center.x,box.center.y,box.center.z,box.size.x,box.size.y,box.size.z})bits.Add(BitConverter.SingleToInt32Bits(f));}else if(shape is CapsuleCollider cap){foreach(float f in new[]{cap.center.x,cap.center.y,cap.center.z,cap.radius,cap.height,(float)cap.direction})bits.Add(BitConverter.SingleToInt32Bits(f));}else if(shape is MeshCollider mesh){foreach(var v in mesh.sharedMesh.vertices)foreach(float f in new[]{v.x,v.y,v.z})bits.Add(BitConverter.SingleToInt32Bits(f));} }return JsonUtility.ToJson(new RigidLaunchReceipt{bodyBits=Array.ConvertAll(a,BitConverter.SingleToInt32Bits),inertiaBits=Array.ConvertAll(inertia,BitConverter.SingleToInt32Bits),shapeBits=LaunchFingerprint.Full(bits.ToArray()),shapeSHA256=LaunchFingerprint.Hash(bits.ToArray()),shapeIDs=ids.ToArray(),fragments=ActiveFragments,cityFragments=CityFragments,breaks=BreakEvents.Count});}
 void CaptureFirstContactProbe(){if(FirstContactProbe.Length!=0||!hasContact||contactPart==null)return;FirstContactProbe=JsonUtility.ToJson(new FirstContactReceipt{time=Time,impulse=bestImpulse,part=contactPart.Id,surface=contactSurface,point=contactPoint,normal=contactNormal,incoming=incoming,bodyJson=LaunchBodyReceipt()});}
}
public sealed partial class CrabFlightGame {
 string requestedLaunchSource="direct";FlightLaunchReceipt launchReceipt;
 int[] LaunchJointBits(){var values=new List<int>();foreach(var t in crab.Root.GetComponentsInChildren<Transform>()){if(t==crab.Root)continue;foreach(float f in new[]{t.localPosition.x,t.localPosition.y,t.localPosition.z,t.localRotation.x,t.localRotation.y,t.localRotation.z,t.localRotation.w,t.localScale.x,t.localScale.y,t.localScale.z})values.Add(BitConverter.SingleToInt32Bits(f));}return values.ToArray();}
}

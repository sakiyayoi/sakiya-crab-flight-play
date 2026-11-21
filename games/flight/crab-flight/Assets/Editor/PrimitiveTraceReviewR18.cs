using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrimitiveTraceReviewR18 {
 public static void Run(){
  R18TimingComparisonReview.Run();
  var dir=Path.GetFullPath("../../../../evidence");Directory.CreateDirectory(dir);
  File.WriteAllText(Path.Combine(dir,"native-normal-primitive-trace.json"),PrimitiveColliderTraceR18.JSON());
  PrimitiveColliderTraceR18.ReviewEnabled=true;PrimitiveColliderTraceR18.ReviewReset();GameObject go=null;
  try{
   go=new GameObject("Lifecycle-only control, no physics step");var c=go.AddComponent<BoxCollider>();var rb=go.AddComponent<Rigidbody>();rb.isKinematic=true;
   PrimitiveColliderTraceR18.CreatedDestroyScheduled(c);PrimitiveColliderTraceR18.BeforeMain(go.scene,0,"control",true);
   if(PrimitiveColliderTraceR18.EligibleMain!=1)throw new Exception("Enabled same-scene candidate not identified");
   PrimitiveColliderTraceR18.ObserveMainPair(1234567,1234568,0);if(PrimitiveColliderTraceR18.Contacts!=0)throw new Exception("Unrelated synthetic pair matched");
   PrimitiveColliderTraceR18.ObserveMainPair(c.GetInstanceID(),1234567,0);if(PrimitiveColliderTraceR18.Contacts!=1)throw new Exception("Known synthetic pair not matched");
   c.enabled=false;PrimitiveColliderTraceR18.Mutated();PrimitiveColliderTraceR18.BeforeMain(go.scene,0,"control-disabled",true);
   if(PrimitiveColliderTraceR18.EligibleMain!=0)throw new Exception("Disabled candidate remains eligible");
   PrimitiveColliderTraceR18.ObserveMainPair(c.GetInstanceID(),1234567,0);if(PrimitiveColliderTraceR18.Contacts!=1)throw new Exception("Disabled candidate added a pair");
   c.enabled=true;PrimitiveColliderTraceR18.Mutated();PrimitiveColliderTraceR18.BeforeMain(default(Scene),0,"control-nonmatching-scene",true);
   if(PrimitiveColliderTraceR18.EligibleMain!=0)throw new Exception("Nonmatching Scene remains eligible");
   UnityEngine.Object.DestroyImmediate(c);PrimitiveColliderTraceR18.BeforeMain(go.scene,0,"control-absent",true);
   if(PrimitiveColliderTraceR18.PendingCount!=0||PrimitiveColliderTraceR18.Dropped!=0)throw new Exception("Lifecycle absence/drop mismatch");
   File.WriteAllText(Path.Combine(dir,"lifecycle-control01.json"),PrimitiveColliderTraceR18.JSON());
  }finally{if(go)UnityEngine.Object.DestroyImmediate(go);PrimitiveColliderTraceR18.ReviewEnabled=false;}
 }
}

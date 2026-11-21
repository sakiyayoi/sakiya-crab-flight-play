#if UNITY_EDITOR
using System;using System.Collections;using System.Collections.Generic;using System.Linq;using System.IO;using NUnit.Framework;using UnityEngine;using UnityEngine.TestTools;
public sealed partial class FlightMeasurementPlayMode {
 [UnityTest,Timeout(120000)]public IEnumerator R27FeelLifecycle(){
  yield return NewRetrySubject(false,true,true,true,true,true,true);
  Call(current,"LoadFeelR27",File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"feel.json")));
  Call(current,"SetFeelR27","{\"preset\":\"mashimashi\",\"shake\":true,\"reducedMotion\":false}");
  Call(current,"TestFire","{\"event\":\"launch\",\"occ\":1}");
  for(int i=0;i<20;i++){Call(current,"Tick",1f/60);yield return null;}
  Call(current,"TestFire","{\"event\":\"glass_break\",\"occ\":2}");
  for(int i=0;i<40;i++){Call(current,"Tick",1f/60);yield return null;}
  Directory.CreateDirectory(Evidence);File.WriteAllText(Path.Combine(Evidence,"lifecycle.json"),JsonUtility.ToJson(State(current),true));
  Assert.That((bool)Field(Field(State(current),"feel"),"loaded"),Is.True);
 }
 [UnityTest,Timeout(120000)]public IEnumerator R27ReviewRepairs(){
  yield return NewRetrySubject(false,true,true,true,true,true,true);
  Call(current,"LoadFeelR27",File.ReadAllText(Path.Combine(Application.streamingAssetsPath,"feel.json")));
  Call(current,"SetFeelR27","{\"preset\":\"mashimashi\",\"shake\":true,\"reducedMotion\":false}");
  StartRouteChargeR11(12,0,2,1);
  Call(current,"BeginFeelFrameR27");
  var crabRoot=(Transform)Field(Field(current,"crab"),"Root");
  var colliders=crabRoot.GetComponentsInChildren<Collider>(true);var matrices=colliders.Select(c=>c.transform.localToWorldMatrix).ToArray();
  var renderers=crabRoot.GetComponentsInChildren<MeshRenderer>();var scales=renderers.ToDictionary(r=>r.transform,r=>r.transform.localScale);
  var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
  current.GetType().GetField("feelSquashStartR27",flags).SetValue(current,Time.realtimeSinceStartup-.09f);
  current.GetType().GetField("feelSquashR27",flags).SetValue(current,.3f);
  Call(current,"FinishFeelFrameR27",0f);
  var applied=(IDictionary)Field(current,"feelMeshScalesR27");Assert.That(applied.Count,Is.GreaterThan(0));
  foreach(DictionaryEntry a in applied){var t=(Transform)a.Key;foreach(DictionaryEntry b in applied)if(t!=(Transform)b.Key)Assert.That(t.IsChildOf((Transform)b.Key),Is.False,"squash twice on ancestor and child");Assert.That(t.localScale.x/scales[t].x,Is.LessThanOrEqualTo(1.30001f));}
  foreach(var r in renderers.Where(r=>!r.enabled))Assert.That(r.transform.localScale,Is.EqualTo(scales[r.transform]),"hidden donor changed");
  for(int n=0;n<colliders.Length;n++)Assert.That(colliders[n].transform.localToWorldMatrix,Is.EqualTo(matrices[n]),"collider matrix changed");
  int scaled=applied.Count;Call(current,"BeginFeelFrameR27");
  Call(current,"TestFire","{\"event\":\"chain\",\"chain\":7,\"occ\":707}");
  Assert.That((float)Field(current,"feelShakeDurationR27"),Is.GreaterThan(0));
  Call(current,"SetFeelR27","{\"preset\":\"off\",\"shake\":true}");
  foreach(var name in new[]{"feelShakeStartR27","feelSquashStartR27","feelZoomStartR27"})Assert.That((float)Field(current,name),Is.EqualTo(-10));
  Call(current,"SetFeelR27","{\"preset\":\"mashimashi\",\"shake\":true}");
  Call(current,"ResetFeelR27");
  foreach(var name in new[]{"feelShakeStartR27","feelSquashStartR27","feelZoomStartR27"})Assert.That((float)Field(current,name),Is.EqualTo(-10));
  Directory.CreateDirectory(Evidence);File.WriteAllText(Path.Combine(Evidence,"review-repairs.json"),"{\"scaledVisuals\":"+scaled+",\"collidersUnchanged\":"+colliders.Length+",\"ancestorDoubleScale\":false,\"chainDurationPositive\":true,\"resetAndOffCleared\":true}");
  Call(current,"ShutdownReviewPhysics");current=null;yield return null;
 }

}
#endif

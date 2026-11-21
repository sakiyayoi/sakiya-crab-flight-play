using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Crab/debris contacts were already excluded. Keep the crab's exact floor
// substeps, while debris advances at the existing outer 1/240 second clock.
// Static contact shapes are mirrored, not rendered or scored a second time.
public sealed partial class RigidCrabFlight {
 Scene debrisSceneCalmR4;PhysicsScene debrisPhysicsCalmR4;
 readonly Dictionary<BoxCollider,BoxCollider> debrisStaticsCalmR4=new Dictionary<BoxCollider,BoxCollider>();
 bool SeparateDebrisCalmR4=>debrisSceneCalmR4.IsValid();
 Scene FragmentSceneCalmR4=>SeparateDebrisCalmR4?debrisSceneCalmR4:Scene;
 void InitDebrisSceneCalmR4(){
#if UNITY_EDITOR
  // Existing edit-mode fixtures use the default physics scene. They do not
  // certify this player-only split; verify it in the actual WebGL player.
  if(!Application.isPlaying)return;
#endif
  debrisSceneCalmR4=SceneManager.CreateScene("Debris fixed240 "+System.Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics3D));
  debrisPhysicsCalmR4=debrisSceneCalmR4.GetPhysicsScene();
 }
 BoxCollider MirrorDebrisStaticCalmR4(BoxCollider source){
  if(!SeparateDebrisCalmR4)return null;
  var obj=new GameObject("Debris contact proxy "+source.gameObject.name);
  SceneManager.MoveGameObjectToScene(obj,debrisSceneCalmR4);
  obj.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);obj.transform.localScale=source.transform.lossyScale;
  var target=obj.AddComponent<BoxCollider>();target.center=source.center;target.size=source.size;target.sharedMaterial=source.sharedMaterial;target.contactOffset=source.contactOffset;target.providesContacts=source.providesContacts;
  debrisStaticsCalmR4.Add(source,target);return target;
 }
 void SetPanelContactsCalmR4(BreakPanel panel,bool enabled){
  foreach(var shape in panel.shapes){shape.enabled=enabled;BoxCollider proxy;if(debrisStaticsCalmR4.TryGetValue(shape,out proxy))proxy.enabled=enabled;}
 }
 void StepDebrisCalmR4(){
  if(!SeparateDebrisCalmR4)return;
  PhysXProfilerTraceR18.BeginScene(2,debrisSceneCalmR4.handle);
  long stamp=CalmSupportPerfR3.Active?CalmSupportPerfR3.Stamp():0;
  debrisPhysicsCalmR4.Simulate(StepSeconds);
  if(CalmSupportPerfR3.Active){long ticks=CalmSupportPerfR3.Stamp()-stamp;CalmSupportPerfR3.SimulateTicks+=ticks;CalmSupportPerfR3.DebrisSimulateTicks+=ticks;CalmSupportPerfR3.DebrisSimulateCalls++;}
  PhysXProfilerTraceR18.EndScene(2);
 }
 void DisposeDebrisSceneCalmR4(){if(SeparateDebrisCalmR4&&debrisSceneCalmR4.isLoaded)SceneManager.UnloadSceneAsync(debrisSceneCalmR4);debrisStaticsCalmR4.Clear();}
 public string ReviewDebrisSceneCalmR4(){
  int mismatch=0;foreach(var pair in debrisStaticsCalmR4){var a=pair.Key;var b=pair.Value;if(!a||!b||a.center!=b.center||a.size!=b.size||a.transform.position!=b.transform.position||a.transform.rotation!=b.transform.rotation||a.transform.lossyScale!=b.transform.lossyScale||a.sharedMaterial!=b.sharedMaterial||a.contactOffset!=b.contactOffset||a.enabled!=b.enabled)mismatch++;}
  return JsonUtility.ToJson(new DebrisSceneAuditCalmR4{separate=SeparateDebrisCalmR4,staticShapes=debrisStaticsCalmR4.Count,staticMismatches=mismatch,physicalFragments=fragments.Count,cityFragments=CityFragments,secondaryBreaks=CitySecondaryBreaks,stepSeconds=StepSeconds});
 }
 [System.Serializable]sealed class DebrisSceneAuditCalmR4{public bool separate;public int staticShapes,staticMismatches,physicalFragments,cityFragments,secondaryBreaks;public float stepSeconds;}
}

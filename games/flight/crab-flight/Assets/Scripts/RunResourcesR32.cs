using System;using UnityEngine;using UnityEngine.SceneManagement;
[Serializable]public sealed class FeelResourcesR32{public int particles,rings,ghosts,speedLines,residue;public float maxRemainingSeconds;}
[Serializable]public sealed class RunResourcesR32{public int sceneCount,liveObjects,meshes,materials,physicsMaterials,rigidbodies,colliders,visualProxies,scaleRestores,flashQueue,eventWindow,auditInputs,auditTicks;public FeelResourcesR32 drawing;public float realtime;}
public sealed partial class CrabFlightGame {
 string resourceAuditR32="";
 // Explicit diagnostic command only, outside the frame-time measurement window.
 public void ReportRunResourcesR32(string _){var r=new RunResourcesR32{sceneCount=SceneManager.sceneCount,meshes=Resources.FindObjectsOfTypeAll<Mesh>().Length,materials=Resources.FindObjectsOfTypeAll<Material>().Length,physicsMaterials=Resources.FindObjectsOfTypeAll<PhysicsMaterial>().Length,rigidbodies=UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Length,colliders=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None).Length,visualProxies=feelObjectsR27.Count,scaleRestores=feelMeshScalesR27.Count,flashQueue=feelFlashQueueR27.Count,eventWindow=eventStream.Count,auditInputs=auditInputs.Count,auditTicks=auditTicks.Count,drawing=feelDrawingR27?feelDrawingR27.ReadResources(Time.realtimeSinceStartup):null,realtime=Time.realtimeSinceStartup};foreach(var o in Resources.FindObjectsOfTypeAll<GameObject>())if(o.scene.IsValid())r.liveObjects++;resourceAuditR32=JsonUtility.ToJson(r);Report();resourceAuditR32="";}
}


using System;using System.Collections.Generic;using System.Text;using UnityEngine;
namespace CrabLifeActors {
// Game-lifetime immutable display meshes only; never retains actors, state or physics.
public sealed class LifeGeometryCacheR17:IDisposable {
 readonly Dictionary<string,Mesh> meshes=new Dictionary<string,Mesh>();
 public int Builds{get;private set;}public int Hits{get;private set;}public int Count=>meshes.Count;
 static string Key(ActorKind kind,Transform root){var b=new StringBuilder(144);b.Append((int)kind);var matrix=root.localToWorldMatrix;for(int i=0;i<16;i++)b.Append('/').Append(BitConverter.SingleToInt32Bits(matrix[i]).ToString("x8"));return b.ToString();}
 public Mesh Acquire(ActorKind kind,Transform root,Func<Mesh> build,out bool callerOwns){
  var key=Key(kind,root);Mesh mesh;if(meshes.TryGetValue(key,out mesh)&&mesh){Hits++;callerOwns=false;return mesh;}
  mesh=build();Builds++;if(meshes.Count>=64){callerOwns=true;return mesh;}
  meshes[key]=mesh;callerOwns=false;return mesh;
 }
 public void Dispose(){foreach(var mesh in meshes.Values)if(mesh){if(Application.isPlaying)UnityEngine.Object.Destroy(mesh);else UnityEngine.Object.DestroyImmediate(mesh);}meshes.Clear();}
}
}

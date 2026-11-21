using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using C2=CrabCityDensityR2;

public static class GiftStoreReviewR7 {
 [Serializable]class View{public bool enabled;public float x;public Vector3 camera;public float cameraSize;public int renderers,submeshDraws,triangles,contactTargets,bodies,colliders;public string colliderSHA,launchBody;}
 [Serializable]class Receipt{public string scope="Native real MainCamera renders; staged crab pose, no natural flight/FPS claim",error;public View[] views;public bool physicsExact,launchExact;public int newShaders,newPhysicsComponents;}
 public static void DrawBefore()=>DrawMode(false,"native06-before");
 public static void DrawAfter()=>DrawMode(true,"native07-after");
 // Each mode uses one Editor process. The old city-art static mesh cache is not valid after destroying a review world.
 static void DrawMode(bool selected,string folder){
  var dir=Path.GetFullPath("../../../../gift-r7-evidence/"+folder);Directory.CreateDirectory(dir);var receipt=new Receipt();var rows=new List<View>();CrabFlightGame game=null;
  try{GiftStoreBuilderR7.Generate();
   foreach(bool enabled in new[]{selected}){
    GiftStoreR7.Enabled=enabled;UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));game.ReviewReplayAcceptedCalmR3(File.ReadAllText("../../../../accepted-input-r2b.json"));
    var launch=game.State().launchReceipt.bodyJson;var camera=game.ReviewCamera;camera.aspect=1.6f;camera.orthographicSize=9.5f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
    var roots=game.ReviewSimulation.Scene.GetRootGameObjects();var cols=roots.SelectMany(r=>r.GetComponentsInChildren<Collider>(true)).ToArray();var bodies=roots.SelectMany(r=>r.GetComponentsInChildren<Rigidbody>(true)).ToArray();string hash=ColliderHash(cols);
    foreach(float x in new[]{45f,61.8f,80f}){
     camera.transform.position=new Vector3(x,9.6f,-26);game.ReviewCrab.Root.position=new Vector3(x-8,3.3f,0);Capture(camera,dir,(enabled?"after-":"before-")+x.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture));
     var active=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();rows.Add(new View{enabled=enabled,x=x,camera=camera.transform.position,cameraSize=camera.orthographicSize,renderers=active.Length,submeshDraws=active.Sum(r=>r.sharedMaterials.Length),triangles=active.Sum(r=>{var f=r.GetComponent<MeshFilter>();return f&&f.sharedMesh?f.sharedMesh.triangles.Length/3:0;}),bodies=bodies.Length,colliders=cols.Length,contactTargets=game.State().cityTargets,colliderSHA=hash,launchBody=launch});
    }
    game.ShutdownReviewPhysics();UnityEngine.Object.DestroyImmediate(game.gameObject);game=null;
   }
   receipt.views=rows.ToArray();receipt.physicsExact=rows.Select(r=>r.colliderSHA).Distinct().Count()==1&&rows.Select(r=>r.bodies).Distinct().Count()==1;receipt.launchExact=rows.Select(r=>r.launchBody).Distinct().Count()==1;
   foreach(var prefab in new[]{"gift_large_0","gift_large_1","gift_large_2","gift_small_0","gift_small_1","gift_small_2","gift_surroundings"}){var model=Resources.Load<GameObject>("GiftStoreR7/"+prefab);receipt.newPhysicsComponents+=model.GetComponentsInChildren<Collider>(true).Length+model.GetComponentsInChildren<Rigidbody>(true).Length;}
   if(!receipt.physicsExact||!receipt.launchExact||receipt.newPhysicsComponents!=0)throw new Exception("Renderer-only protection check failed");
  }catch(Exception e){receipt.error=e.ToString();throw;}finally{GiftStoreR7.Enabled=true;if(game)game.ShutdownReviewPhysics();File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(receipt,true));}
 }
 static string ColliderHash(Collider[] colliders){var items=colliders.Select(c=>{string shape=c is BoxCollider b?"box"+b.center.ToString("R")+b.size.ToString("R"):c is SphereCollider s?"sphere"+s.center.ToString("R")+s.radius.ToString("R"):c is CapsuleCollider p?"capsule"+p.center.ToString("R")+p.radius.ToString("R")+p.height.ToString("R"):c is MeshCollider m?"mesh"+(m.sharedMesh?m.sharedMesh.vertexCount:0)+m.convex:"other";return c.name+"|"+c.transform.position.ToString("R")+"|"+c.transform.rotation.ToString("R")+"|"+c.transform.lossyScale.ToString("R")+"|"+shape+"|"+c.enabled+"|"+c.isTrigger+"|"+c.contactOffset.ToString("R");}).OrderBy(x=>x,StringComparer.Ordinal);using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n",items)))).Replace("-","").ToLowerInvariant();}
 static void Capture(Camera camera,string dir,string name){var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(dir+"/"+name+".png",im.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
}

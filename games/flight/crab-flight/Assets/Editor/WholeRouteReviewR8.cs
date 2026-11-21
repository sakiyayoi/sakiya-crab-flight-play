using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using UnityEngine;
using C2=CrabCityDensityR2;

public static class WholeRouteReviewR8 {
 [Serializable]class View{public bool enabled;public float x;public Vector3 camera;public float cameraSize;public int renderers,submeshDraws,triangles,contactTargets,bodies,colliders;public string colliderSHA,launchBody;}
 [Serializable]class Contact{public string target,fault;public int breaks,secondary,remainingAfterReset;public bool targetHit;public float seconds;public string debris;}
 [Serializable]class Receipt{public Contact[] contacts;public string scope="Native real MainCamera renders; staged crab pose, no natural flight/FPS claim",error;public View[] views;public bool physicsExact,launchExact;public int newShaders,newPhysicsComponents;}
 public static void Draw()=>DrawMode(true,"whole-route-after");
 public static void Before()=>DrawMode(false,"whole-route-landmark-before");
 static void DrawMode(bool enabled,string folder){
  WorldEscalationR8.Enabled=enabled;
  var dir=Path.GetFullPath("../../../../finish-r8-evidence/"+folder);Directory.CreateDirectory(dir);var rows=new List<View>();var contacts=new List<Contact>();CrabFlightGame game=null;
  try{UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;game.LoadFeelR27(File.ReadAllText("Assets/StreamingAssets/feel.json"));game.ReviewReplayAcceptedCalmR3(File.ReadAllText("../../../../accepted-input-r2b.json"));
   string launch=game.State().launchReceipt.bodyJson;var camera=game.ReviewCamera;camera.aspect=1.6f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
   var roots=game.ReviewSimulation.Scene.GetRootGameObjects();var cols=roots.SelectMany(r=>r.GetComponentsInChildren<Collider>(true)).ToArray();var bodies=roots.SelectMany(r=>r.GetComponentsInChildren<Rigidbody>(true)).ToArray();string hash=ColliderHash(cols);
   foreach(var v in WorldRouteR11.Data.sectors){float x=v.id=="office"?344:v.id=="port"?750.8f:v.id=="viaduct"?1058:v.id=="country"?1605:v.id=="launchrange"?2110:v.id=="orbit"?2740:v.viewX;float size=v.id=="office"?11:v.id=="port"?13:v.id=="viaduct"?16:v.id=="country"?14:v.id=="launchrange"?20:v.id=="orbit"?22:v.viewSize;camera.orthographicSize=size;camera.transform.position=new Vector3(x,size*.73f,-35);game.ReviewCrab.Root.position=new Vector3(x-size*.85f,3.3f,0);Capture(camera,dir,v.id);
    var active=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.enabled&&r.gameObject.activeInHierarchy).ToArray();rows.Add(new View{enabled=true,x=camera.transform.position.x,camera=camera.transform.position,cameraSize=camera.orthographicSize,renderers=active.Length,submeshDraws=active.Sum(r=>r.sharedMaterials.Length),triangles=active.Sum(r=>{var f=r.GetComponent<MeshFilter>();return f&&f.sharedMesh?f.sharedMesh.triangles.Length/3:0;}),bodies=bodies.Length,colliders=cols.Length,contactTargets=game.State().cityTargets,colliderSHA=hash,launchBody=launch});
   }
   foreach(string target in new[]{"r5-extension-+1-contact-04-delivery_van","far-r10-new-1-752-port_lattice_loader_r10","r14-viaduct-roof","r11-country-1-1-0-hvac_unit","r11-launchrange-1-0-0-port_lattice_loader_r10","r11-orbit-1-0-0-facade_break_panel"}){
    game.ReviewReplayAcceptedCalmR3(File.ReadAllText("../../../../accepted-input-r2b.json"));game.ReviewSimulation.ReviewLandmarkR8(target);
    for(int i=0;i<180&&!game.Finished;i++)game.Tick(1f/60);
    var state=game.State();var row=new Contact{target=target,breaks=state.destructionEvent,secondary=state.chainBreaks,seconds=state.time,fault=state.physicsFault,targetHit=game.ReviewSimulation.BreakEvents.Any(e=>e.id==target),debris=game.ReviewSimulation.ReviewDebrisSceneCalmR4()};
    game.Prepare("");row.remainingAfterReset=game.State().fragments;contacts.Add(row);
   }
  }finally{if(game)game.ShutdownReviewPhysics();File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(new Receipt{contacts=contacts.ToArray(),views=rows.ToArray(),physicsExact=rows.Select(r=>r.colliderSHA).Distinct().Count()==1,launchExact=rows.Select(r=>r.launchBody).Distinct().Count()==1},true));}
 }
 static string ColliderHash(Collider[] colliders){var items=colliders.Select(c=>{string shape=c is BoxCollider b?"box"+b.center.ToString("R")+b.size.ToString("R"):c is SphereCollider s?"sphere"+s.center.ToString("R")+s.radius.ToString("R"):c is CapsuleCollider p?"capsule"+p.center.ToString("R")+p.radius.ToString("R")+p.height.ToString("R"):c is MeshCollider m?"mesh"+(m.sharedMesh?m.sharedMesh.vertexCount:0)+m.convex:"other";return c.name+"|"+c.transform.position.ToString("R")+"|"+c.transform.rotation.ToString("R")+"|"+c.transform.lossyScale.ToString("R")+"|"+shape+"|"+c.enabled+"|"+c.isTrigger+"|"+c.contactOffset.ToString("R");}).OrderBy(x=>x,StringComparer.Ordinal);using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n",items)))).Replace("-","").ToLowerInvariant();}
 static void Capture(Camera camera,string dir,string name){var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(dir+"/"+name+".png",im.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
}

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
public static class Charge2Review {
 [Serializable] public class Street {public bool density;public int batches,vertices,triangles,colliders;}
 [Serializable] public class Streets {public string scope="Same camera renderer-only street comparison, not added physical obstacles.";public List<Street> variants=new List<Street>();}
 public static void RunAndWeb(){
  AnatomyModelReview.Run();StreetViews();ChargeReview.Review();
  string path="../delivery/UnityWeb/charge2-model01-ready";BuildFlight.Web(path);File.Copy("../web-template/launch-input.js",path+"/launch-input.js",true);
 }
 public static void FinalWeb(){AnatomyModelReview.Run();string path="../delivery/UnityWeb/charge2-final01-ready";BuildFlight.Web(path);File.Copy("../web-template/launch-input.js",path+"/launch-input.js",true);}
 static void StreetViews(){
  var report=new Streets();Directory.CreateDirectory("../evidence/street");
  foreach(bool dense in new[]{false,true}){
   RestaurantVisuals.DensityEnabled=dense;var g=BuildFlight.PrepareScene();
   try{
    g.Initialize();var root=GameObject.Find("Restaurant authored environment - visuals only");var meshes=root.GetComponentsInChildren<MeshFilter>().Where(f=>f.GetComponent<Renderer>().enabled).ToArray();
    report.variants.Add(new Street{density=dense,batches=meshes.Length,vertices=meshes.Sum(m=>m.sharedMesh.vertexCount),triangles=meshes.Sum(m=>m.sharedMesh.triangles.Length/3),colliders=root.GetComponentsInChildren<Collider>().Length});
    var c=g.ReviewCamera;c.aspect=844f/390;c.orthographic=true;c.orthographicSize=8;
    foreach(float x in new[]{35f,75f}){
     c.transform.position=new Vector3(x,10,-16);c.transform.rotation=Quaternion.LookRotation(new Vector3(0,-5.3f,16));
     var rt=RenderTexture.GetTemporary(844,390,24);var previous=RenderTexture.active;c.targetTexture=rt;c.Render();RenderTexture.active=rt;var t=new Texture2D(844,390,TextureFormat.RGB24,false);t.ReadPixels(new Rect(0,0,844,390),0,0);t.Apply();File.WriteAllBytes("../evidence/street/"+(dense?"dense":"baseline")+"-x"+x+".png",t.EncodeToPNG());c.targetTexture=null;RenderTexture.active=previous;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.DestroyImmediate(t);
    }
   }finally{g.ShutdownReviewPhysics();}
  }
  RestaurantVisuals.DensityEnabled=true;File.WriteAllText("../evidence/street/result.json",JsonUtility.ToJson(report,true));
 }
}

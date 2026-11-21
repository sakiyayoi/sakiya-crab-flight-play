using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class MallAreasReviewR5 {
 [Serializable]class Row{public string family,source;public int targets,large;public Vector3 cameraPosition;public float cameraSize;}
 [Serializable]class Receipt{public string scope="Native same-camera real rendering, staged crab pose; not natural contact or browser performance",error;public Row[] views;public int generatedAssets;}
 public static void DrawAndBuild(){Draw();KingBoldBuildR2.Web();}
 public static void Draw(){
  MallAreasBuilderR5.Generate();string dir=Path.GetFullPath("../../../../areas-r5-evidence/native06");Directory.CreateDirectory(dir);var rows=new List<Row>();var receipt=new Receipt();CrabFlightGame game=null;
  try{foreach(bool enabled in new[]{true}){
   MallAreasR5.Enabled=enabled;UnityEngine.Random.InitState(42043);game=BuildFlight.PrepareScene();game.Initialize();game.enabled=false;
   var camera=game.ReviewCamera;camera.aspect=1.6f;camera.orthographicSize=9.5f;camera.transform.rotation=Quaternion.LookRotation(new Vector3(0,-.18f,1));
   foreach(float x in new[]{45f,80f,119f}){
    camera.transform.position=new Vector3(x,7.0f,-26);game.ReviewCrab.Root.position=new Vector3(x-8,3.3f,0);
    string name=(enabled?"after-":"before-")+MallAreasR5.Family(x);Capture(camera,dir,name);
    var targets=UnityEngine.Object.FindObjectsByType<CrabCityDensityR2.CityVisualBreakable>(FindObjectsSortMode.None).Where(v=>Mathf.Abs(v.transform.position.x-x)<17&&v.transform.position.y<2).ToArray();
    rows.Add(new Row{family=MallAreasR5.Family(x),source=enabled?"AreasR5":"CalmR4 baseline path",targets=targets.Length,large=targets.Count(t=>t.assetId.EndsWith("_large")),cameraPosition=camera.transform.position,cameraSize=camera.orthographicSize});
   }
   game.ShutdownReviewPhysics();UnityEngine.Object.DestroyImmediate(game.gameObject);game=null;
  }receipt.views=rows.ToArray();receipt.generatedAssets=6;
  }catch(Exception e){receipt.error=e.ToString();throw;}
  finally{MallAreasR5.Enabled=true;if(game)game.ShutdownReviewPhysics();File.WriteAllText(dir+"/receipt.json",JsonUtility.ToJson(receipt,true));}
 }
 static void Capture(Camera c,string dir,string name){var rt=new RenderTexture(1280,800,24);var im=new Texture2D(1280,800,TextureFormat.RGB24,false);c.targetTexture=rt;c.Render();RenderTexture.active=rt;im.ReadPixels(new Rect(0,0,1280,800),0,0);im.Apply();File.WriteAllBytes(dir+"/"+name+".png",im.EncodeToPNG());c.targetTexture=null;RenderTexture.active=null;UnityEngine.Object.DestroyImmediate(im);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
}

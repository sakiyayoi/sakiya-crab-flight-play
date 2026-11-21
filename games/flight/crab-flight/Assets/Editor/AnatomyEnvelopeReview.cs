using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
public static class AnatomyEnvelopeReview {
 public static void Generate(){
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);CrabModel.ReviewUseBaseline(true);CrabModel.SurfaceShader=Shader.Find("CustomCrabBattle/ChitinR3");CrabModel.ArenaShader=Shader.Find("Standard");var data=new CrabEnvelopeData{anchors=new[]{0f,20f,25f,30f,45f,100f},parts=new CrabEnvelopePart[11]};
  for(int i=0;i<data.anchors.Length;i++){float a=data.anchors[i];var crab=CrabModel.Make("Preserved physics datum",new FlightSettings{claw=a,shell=a,legs=a},Color.white);for(int j=0;j<crab.CollisionParts.Count;j++){var p=crab.CollisionParts[j];if(i==0)data.parts[j]=new CrabEnvelopePart{id=p.Id,centers=new Vector3[data.anchors.Length],sizes=new Vector3[data.anchors.Length]};if(data.parts[j].id!=p.Id)throw new Exception("Envelope order changed");data.parts[j].centers[i]=p.LocalBounds.center;data.parts[j].sizes[i]=p.LocalBounds.size;}crab.Dispose();}
  File.WriteAllText("Assets/Resources/CrabPhysicsEnvelope.json",JsonUtility.ToJson(data,true));AssetDatabase.ImportAsset("Assets/Resources/CrabPhysicsEnvelope.json",ImportAssetOptions.ForceUpdate);CrabModel.ReviewUseBaseline(false);
 }
}

using System;
using System.IO;
using UnityEngine;
using UnityEditor.SceneManagement;
public static class ActionComparison{
 [Serializable]class Report{public string condition="same r3 model; default sword/cannon; player stationary (0,-4); opponent (0,4); 540 ticks of 1/90s; AI on; no player attacks";public ActionState before,after;public float oldInterval=.8f,newInterval=1.35f,oldWeaponClockScale=1,newWeaponClockScale=.64f;}
 public static void Run(){EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var old=new GameObject("Baseline comparison only").AddComponent<BaselineActionGame>();old.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");old.groundShader=Shader.Find("Standard");old.Initialize();old.Begin("");for(int i=0;i<540;i++)old.Tick(1f/90);var before=old.State();EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var current=new GameObject("Current comparison only").AddComponent<CrabActionGame>();current.crabShader=Shader.Find("CustomCrabBattle/ChitinR3");current.groundShader=Shader.Find("Standard");current.Initialize();current.Begin("");for(int i=0;i<540;i++)current.Tick(1f/90);var after=current.State();if(after.hp<=before.hp)throw new Exception("New pacing did not give additional reaction time");File.WriteAllText("../evidence/pressure-comparison.json",JsonUtility.ToJson(new Report{before=before,after=after},true));}
}

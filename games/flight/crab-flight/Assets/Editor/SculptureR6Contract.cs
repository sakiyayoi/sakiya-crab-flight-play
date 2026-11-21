using System;using System.IO;using System.Collections.Generic;using UnityEngine;using UnityEditor;
public static class SculptureR6Contract {
 [Serializable]public class Row {public int claw,shell,legs,meshes,triangles,parts,changedVertices,changedNormals;public float maxDisplacement;public bool boundsSame=true,attachmentSame=true,uvSame=true,indexSame=true;}
 [Serializable]public class Report{public string revision="r6-spectacle",state="ACTUAL_UNITY_MESH_CONTRACT_VERIFIED";public List<Row> rows=new List<Row>();}
 static void Need(bool b,string s){if(!b)throw new Exception(s);}
 public static void Run(){
  CrabModel.SurfaceShader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/Shaders/ChitinR3.shader");CrabModel.ArenaShader=Shader.Find("Standard");var report=new Report();
  foreach(int c in new[]{0,45,100})foreach(int s in new[]{0,45,100})foreach(int l in new[]{0,45,100}){
   var cfg=new CrabBuild{claw=c,shell=s,legs=l};CrabModel.ReviewUseBaseline(false);CrabSculptR6.Enabled=false;CrabSurfaceBinding.DetailR6=false;var old=CrabModel.Make("old",cfg,Color.white);CrabSculptR6.Enabled=true;CrabSurfaceBinding.DetailR6=true;var next=CrabModel.Make("next",cfg,Color.white);
   var row=new Row{claw=c,shell=s,legs=l};report.rows.Add(row);var a=old.Root.GetComponentsInChildren<MeshFilter>();var b=next.Root.GetComponentsInChildren<MeshFilter>();row.meshes=b.Length;Need(a.Length==35&&a.Length==b.Length,"35 meshes");
   for(int i=0;i<a.Length;i++){var am=a[i].sharedMesh;var bm=b[i].sharedMesh;var av=am.vertices;var bv=bm.vertices;var an=am.normals;var bn=bm.normals;var au=am.uv;var bu=bm.uv;var ai=am.triangles;var bi=bm.triangles;Need(av.Length==bv.Length&&ai.Length==bi.Length&&au.Length==bu.Length,"mesh array contract");row.triangles+=bi.Length/3;row.boundsSame&=am.bounds.Equals(bm.bounds);
    for(int k=0;k<av.Length;k++){float delta=Vector3.Distance(av[k],bv[k]);if(delta>0)row.changedVertices++;if(!an[k].Equals(bn[k]))row.changedNormals++;row.maxDisplacement=Mathf.Max(row.maxDisplacement,delta);Need(float.IsFinite(bv[k].x)&&float.IsFinite(bv[k].y)&&float.IsFinite(bv[k].z),"nonfinite sculpt");Need(Mathf.Abs(bn[k].magnitude-1)<.0001f,"unit normal");}
    for(int k=0;k<ai.Length;k++)row.indexSame&=ai[k]==bi[k];for(int k=0;k<au.Length;k++)row.uvSame&=au[k].Equals(bu[k]);
   }
   row.parts=next.CollisionParts.Count;Need(row.parts==11&&old.CollisionParts.Count==11,"11 parts");for(int i=0;i<row.parts;i++){var x=old.CollisionParts[i];var y=next.CollisionParts[i];row.attachmentSame&=x.Id==y.Id&&x.Kind==y.Kind&&x.LocalBounds.Equals(y.LocalBounds)&&x.Part.localToWorldMatrix.Equals(y.Part.localToWorldMatrix);}
   Need(row.boundsSame&&row.attachmentSame&&row.uvSame&&row.indexSame,"bound/attachment/UV/index changed");Need(row.triangles==32568&&row.changedVertices>0&&row.maxDisplacement<=.0221f,"sculpt/topology budget");old.Dispose();next.Dispose();
  }
  Directory.CreateDirectory("../evidence");File.WriteAllText("../evidence/actual-unity-model-contract.json",JsonUtility.ToJson(report,true));Debug.Log("R6 ACTUAL MESH CONTRACT 27/27");
 }
}

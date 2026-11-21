using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public static class NeonStage5FilterReview {
 [Serializable]class CheckRow {public string name,meshSHA256;public bool gate,pass;public int vertices,ghostMarks,visibleGhosts,bubbleMarks,visibleBubbles;}
 [Serializable]class Receipt {public bool pass;public string scope="Fixed clock render fixture: ghost + impact ring + flash + water + dust. Same cue list and geometry, only Ghost and water/bubble draw omitted inside gate.";public int fullVertices,noGhostBubbleVertices;public string fullSHA256,noGhostBubbleSHA256;public List<CheckRow> checks=new List<CheckRow>();}
 public static void Check(CrabFlightGame game,string dir){
  var result=new Receipt();GameObject node=null;
  try{
   var existing=game.GetComponentInChildren<MashimashiPresentationR27>();
   var cfg=JsonUtility.FromJson<FeelConfig>(File.ReadAllText("Assets/StreamingAssets/feel.json"));
   node=new GameObject("Stage5 cue boundary fixture");var draw=node.AddComponent<MashimashiPresentationR27>();draw.Initialize(existing.GetComponent<MeshRenderer>().sharedMaterial.shader,cfg);
   const float born=10,now=10.08f;var camera=game.ReviewCamera;var p=game.ReviewCrab.Root.position;
   draw.Ghost(p,camera,born,game.ReviewCrab.Root,new Vector3(30,0,0));draw.Ring(p,born);draw.Flash(camera,Color.yellow,p);
   draw.Particles(p,3,"water",camera,born,Vector3.right);draw.Particles(p,3,"dust",camera,born,Vector3.right);
   draw.ReviewFrozenDraw(camera,now,false,false,false);result.fullSHA256=Hash(draw);result.fullVertices=Mesh(draw).vertexCount;
   draw.ReviewFrozenDraw(camera,now,false,true,true);result.noGhostBubbleSHA256=Hash(draw);result.noGhostBubbleVertices=Mesh(draw).vertexCount;
   if(result.fullSHA256==result.noGhostBubbleSHA256||result.noGhostBubbleVertices<=0)throw new Exception("Fixture must contain both ghost and protected impact/bubble geometry");
   string resources=JsonUtility.ToJson(draw.ReadResources(now));
   foreach(string name in new[]{"flight-positive","flight-negative","aim-negative","charge-positive","ready-unarmed","result","reduced","effects-off","threshold","neon-disabled"}){
    bool expected=name=="flight-positive"||name=="flight-negative"||name=="aim-negative"||name=="charge-positive";
    string phase=name=="result"?"result":name.StartsWith("ready")||name.StartsWith("aim")||name.StartsWith("charge")?"ready":"flight";
    bool eligible=name.StartsWith("aim")||name.StartsWith("charge");float spin=name=="threshold"?1:name.Contains("negative")?-20:20;
    NeonProto.Enabled=name!="neon-disabled";bool gate=NeonProto.SpinGate(phase,spin,name=="reduced",name!="effects-off",eligible);
    draw.ReviewSpinGate(gate);draw.ReviewFrozenDraw(camera,now,false,false,false);
    string hash=Hash(draw);bool pass=gate==expected&&hash==(expected?result.noGhostBubbleSHA256:result.fullSHA256)&&draw.NeonGhostMarks==1&&draw.NeonVisibleGhosts==(expected?0:1)&&draw.NeonBubbleMarks==3&&draw.NeonVisibleBubbles==(expected?0:3)&&JsonUtility.ToJson(draw.ReadResources(now))==resources;
    result.checks.Add(new CheckRow{name=name,gate=gate,pass=pass,meshSHA256=hash,vertices=Mesh(draw).vertexCount,ghostMarks=draw.NeonGhostMarks,visibleGhosts=draw.NeonVisibleGhosts,bubbleMarks=draw.NeonBubbleMarks,visibleBubbles=draw.NeonVisibleBubbles});if(!pass)throw new Exception("Ghost filter boundary failed: "+name);
   }
   // Explicitly restore the full renderer after the spin gate closes.
   draw.ReviewSpinGate(false);draw.ReviewFrozenDraw(camera,now,false,false,false);
   if(Hash(draw)!=result.fullSHA256)throw new Exception("Gate exit did not restore original mesh");result.pass=true;
  }finally{NeonProto.Enabled=true;if(node)UnityEngine.Object.DestroyImmediate(node);File.WriteAllText(dir+"/stage5-filter-boundaries.json",JsonUtility.ToJson(result,true));}
 }
 static Mesh Mesh(MashimashiPresentationR27 draw)=>draw.GetComponent<MeshFilter>().sharedMesh;
 static string Hash(MashimashiPresentationR27 draw){
  var m=Mesh(draw);var data=m.vertices.SelectMany(v=>new[]{v.x,v.y,v.z}).Concat(m.colors.SelectMany(c=>new[]{c.r,c.g,c.b,c.a})).SelectMany(BitConverter.GetBytes).Concat(m.triangles.SelectMany(BitConverter.GetBytes)).ToArray();
  using(var sha=System.Security.Cryptography.SHA256.Create())return BitConverter.ToString(sha.ComputeHash(data)).Replace("-","").ToLowerInvariant();
 }
}

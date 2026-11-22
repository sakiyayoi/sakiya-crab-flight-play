using System;
using UnityEngine;

[Serializable]public sealed class NeonSignSector {public string id,color;public int count;}
public sealed class NeonSectionArt {
 public const int SignCount=64,MaxStrongGlows=3;
 public const float StrongThreshold=.62f;
 readonly MeshRenderer[] signs=new MeshRenderer[SignCount];
 readonly bool[] irregular=new bool[SignCount];
 public readonly NeonSignSector[] Sectors;
 float clock;
 public readonly Transform Root;
 public int GlowCount=>signs.Length;
 public int StrongCount{get;private set;}
 public NeonSectionArt(Transform owner,Camera camera){
  camera.backgroundColor=NeonProto.Background;
  Root=new GameObject("Neon world signs render only").transform;Root.SetParent(owner,false);
  var supports=NeonPlacementSupports.Build(Root);
  var sectors=WorldRouteR11.Data.sectors;Sectors=new NeonSignSector[sectors.Length];
  int[] counts={1,3,12,10,8,8,8,8,6};int[] palette={3,5,-1,2,4,1,5,0,3};int index=0;
  for(int s=0;s<sectors.Length;s++){
   var sector=sectors[s];bool mall=sector.id=="mall";
   Sectors[s]=new NeonSignSector{id=sector.id,count=counts[s],color=mall?"six-color cycle":sector.id=="orbit"?"#25DFFF / #8B5CFF":"#"+ColorUtility.ToHtmlStringRGB(NeonProto.Palette[palette[s]])};
   for(int j=0;j<counts[s];j++,index++){
    int color=mall?j%6:sector.id=="orbit"&&j%2==1?2:palette[s];
    signs[index]=NeonGlowTexture.Quad("Neon sign "+sector.id+" "+j,Root,NeonProto.Palette[color]);
    float x=mall?Mathf.Lerp(32,130,j/11f):Mathf.Lerp(sector.start,sector.end,(j+.5f)/counts[s]);
    float width=sector.id=="tank"?3:7;
    signs[index].transform.localPosition=new Vector3(x,3.5f+(j*3%7),6.5f);
    signs[index].transform.localScale=new Vector3(width,sector.id=="tank"?3:5,1);
    if(sector.id=="orbit"&&j==0){
     var sign=supports.Data.orbitSign0;
     signs[index].transform.localPosition=NeonPlacementSupports.Vector(sign.p);
     signs[index].transform.localScale=NeonPlacementSupports.Vector(sign.size);
    }
    if(PortLayout.Enabled&&(sector.id=="port"||sector.id=="tank"))signs[index].enabled=false;
    irregular[index]=j==(mall?5:counts[s]/2);
   }
  }
  if(index!=SignCount)throw new InvalidOperationException("Neon sign count mismatch");
  Tick(0,false,true);
 }
 public void Tick(float dt,bool reduced,bool effects){
  clock+=Mathf.Max(0,dt);StrongCount=0;
  // Rotate the allocation order slowly so distant sectors can also reach their peak.
  int first=(Mathf.FloorToInt(clock*.65f)%signs.Length+signs.Length)%signs.Length;
  for(int k=0;k<signs.Length;k++){
   int i=(first+k)%signs.Length;float alpha=.42f;
   if(effects&&!reduced){alpha=.42f+.40f*Mathf.Pow(.5f+.5f*Mathf.Sin(clock*.65f+i*2.399963f),3);
    if(irregular[i])alpha*=.75f+.25f*Mathf.PerlinNoise(clock*.8f,17.1f+i);
   }
   if(alpha>StrongThreshold){if(StrongCount>=MaxStrongGlows)alpha=StrongThreshold;else StrongCount++;}
   signs[i].sharedMaterial.color=new Color(1,1,1,alpha);
  }
 }
 public void Dispose(){foreach(var r in signs)if(r)NeonProto.Release(r.sharedMaterial);if(Root)NeonProto.Release(Root.gameObject);}
}

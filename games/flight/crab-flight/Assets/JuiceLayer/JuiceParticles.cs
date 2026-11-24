using UnityEngine;

public sealed partial class MashimashiPresentationR27 {
 static readonly Color JuiceLantern=new Color(1,179f/255,71f/255);
 // PortParticles explosive branch from 911bc64: only its three drawing layers.
 // Reserve a complete burst; eviction is insertion order, so the newest hit survives.
 public int JuiceParticles(Vector3 p,Color objectColor,string material,Camera cam,float now){
  int remove=JuiceRules.Evict(marks.Count,JuiceRules.MarksPerBreak);
  if(remove>0)marks.RemoveRange(0,remove);
  int shape=MaterialID(material);if(shape==5)shape=1;
  objectColor.a=1;
  for(int i=0;i<JuiceRules.MarksPerBreak;i++){
   float angle=i*2.399963f;int layer=JuiceRules.Layer(i);
   float speed=layer==5?2.2f:layer==6?14+i%5:12+i%7*1.6f;
   var velocity=cam.transform.right*Mathf.Cos(angle)*speed+cam.transform.up*(Mathf.Sin(angle)*speed+3);
   marks.Add(new Mark{kind=layer,material=shape,born=now,
    life=layer==5?JuiceRules.SmokeLife:layer==6?JuiceRules.SparkLife:JuiceRules.ShardLife,
    p=p,v=velocity,size=layer==5?.65f:layer==6?.09f:.22f,angle=angle,turn=8+i%5,stretch=1.1f,
    color=layer==5?new Color(1,1,1,.72f):layer==6?JuiceLantern:objectColor});
  }
  return JuiceRules.MarksPerBreak;
 }
}

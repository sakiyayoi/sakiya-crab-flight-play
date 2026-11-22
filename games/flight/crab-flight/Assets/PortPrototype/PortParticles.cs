using UnityEngine;

public sealed partial class MashimashiPresentationR27 {
 public int PortParticles(Vector3 p,int count,string asset,Camera cam,float now,Vector3 velocity){
  int added=0;var family=PortLayout.FamilyColor(asset);
  for(int i=0;i<count&&marks.Count<512;i++){
   float angle=i*2.399963f;int layer=i%5<3?0:i%5==3?5:6;
   var v=cam.transform.right*Mathf.Cos(angle)*(layer==5?.8f:3)+cam.transform.up*(1.5f+Mathf.Sin(angle)*2);
   marks.Add(new Mark{kind=layer,material=asset.Contains("crate")?3:2,born=now,life=layer==5?.65f:.48f,p=p,v=v+Vector3.ClampMagnitude(velocity,20)*.05f,size=layer==5?.45f:layer==6?.08f:.16f,angle=angle,turn=2+i%5,stretch=1.1f,color=layer==5?new Color(1,1,1,.72f):layer==6?PortVisuals.Lantern:i%3==0?family:new Color(.88f,.84f,.74f)});added++;
  }return added;
 }
 void PortPuff(Vector3 p,Vector3 u,Vector3 v,Mark m,Color c,float t){
  if(m.kind==5){for(int j=0;j<3;j++){float a=j*2.094f;Fan(p+(u*Mathf.Cos(a)+v*Mathf.Sin(a))*m.size*.3f,u,v,m.size*(.65f+t*.6f),12,.98f,c);}}
  else Fan(p,u,v,m.size*(1-t*.5f),5,.7f,c);
 }
}

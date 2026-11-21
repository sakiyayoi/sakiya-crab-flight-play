using UnityEngine;

// Reused line objects only. They have no colliders and never participate in rules.
public sealed class AttackForecast {
 const int Segments=12;
 readonly LineRenderer[] sweep=new LineRenderer[Segments],shaft=new LineRenderer[5];
 readonly LineRenderer marker,leadingSweep;
 readonly bool hostile;
 public bool Visible {get;private set;}
 public AttackForecast(Transform owner,Material material,bool enemy){
  hostile=enemy;
  for(int i=0;i<sweep.Length;i++)sweep[i]=Make(owner,material,"Forecast sweep "+i);
  for(int i=0;i<shaft.Length;i++)shaft[i]=Make(owner,material,"Forecast shaft "+i);
  leadingSweep=Make(owner,material,"Forecast first tip sweep");marker=Make(owner,material,enemy?"Forecast arrow":"Forecast endpoint");
 }
 static LineRenderer Make(Transform owner,Material material,string name){
  var obj=new GameObject(name);obj.transform.SetParent(owner,false);var line=obj.AddComponent<LineRenderer>();line.useWorldSpace=true;line.sharedMaterial=material;line.numCapVertices=3;line.numCornerVertices=2;line.enabled=false;return line;
 }
 static void Segment(LineRenderer line,Vector3 a,Vector3 b,float width){line.startWidth=line.endWidth=width;line.positionCount=2;line.SetPosition(0,a);line.SetPosition(1,b);line.enabled=true;}
 public void Hide(){Visible=false;foreach(var line in sweep)line.enabled=false;foreach(var line in shaft)line.enabled=false;leadingSweep.enabled=false;marker.enabled=false;}
 public void Melee(AttackGeometry.Volume[] samples,int count,Vector3 previousTip){
  Hide();if(count<2)return;Visible=true;float width=samples[0].radius*2;
  Segment(leadingSweep,previousTip,samples[0].tip,width);
  if(!hostile){var arc=sweep[0];arc.startWidth=arc.endWidth=width;arc.positionCount=count;for(int i=0;i<count;i++)arc.SetPosition(i,samples[i].tip);arc.enabled=true;}
  else for(int i=0;i<count-1&&i<sweep.Length;i++)if(i%2==0)Segment(sweep[i],samples[i].tip,samples[i+1].tip,width);
  for(int i=0;i<shaft.Length;i++){int at=Mathf.RoundToInt(i*(count-1)/(float)(shaft.Length-1));if(!hostile||i%2==0)Segment(shaft[i],samples[at].shaftStart,samples[at].tip,width);}
  End(samples[count-1].tip,samples[count-1].tip-samples[count-2].tip,width);
 }
 public void Cannon(AttackGeometry.Ray ray,float visibleRange){
  Hide();Visible=true;Vector3 end=ray.muzzle+ray.direction*visibleRange;int count=hostile?8:1;
  for(int i=0;i<count;i++){if(hostile&&i%2==1)continue;Vector3 a=Vector3.Lerp(ray.muzzle,end,i/(float)count),b=Vector3.Lerp(ray.muzzle,end,(i+1)/(float)count);Segment(sweep[i],a,b,ray.radius*2);}
  End(end,ray.direction,ray.radius*2);
 }
 void End(Vector3 point,Vector3 direction,float width){
  var heading=Vector3.ProjectOnPlane(direction,Vector3.up).normalized;if(heading.sqrMagnitude<.001f)heading=Vector3.forward;
  if(hostile){var side=new Vector3(heading.z,0,-heading.x);float length=Mathf.Max(.32f,width*1.4f);marker.startWidth=marker.endWidth=Mathf.Min(.12f,width*.35f);marker.positionCount=3;marker.SetPosition(0,point-heading*length+side*length*.55f);marker.SetPosition(1,point);marker.SetPosition(2,point-heading*length-side*length*.55f);}
  else{marker.startWidth=marker.endWidth=Mathf.Min(.10f,width*.3f);marker.positionCount=17;for(int i=0;i<17;i++){float angle=i*Mathf.PI*2/16;marker.SetPosition(i,point+new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle))*Mathf.Max(.20f,width*.6f));}}
  marker.enabled=true;
 }
}

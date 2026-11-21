using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
// Drawing only. No simulation RNG, forces, transforms of gameplay bodies, or score writes.
public sealed partial class MashimashiPresentationR27:MonoBehaviour {
 struct Mark{public int kind,material;public float born,life,size,angle,turn,stretch;public Vector3 p,v;public Color color;}
 readonly List<Mark> marks=new List<Mark>();readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> triangles=new List<int>();Mesh mesh;Material ink;FeelConfig cfg;bool flash;Color flashColor;Vector3 flashPosition;Vector3 rotorPosition;float rotorAngle,rotorStrength;bool rotor;
 public int Active=>marks.Count;
 public bool RotorVisible=>rotor;
 public FeelResourcesR32 ReadResources(float now){var r=new FeelResourcesR32();foreach(var m in marks){if(m.kind==0)r.particles++;else if(m.kind==1)r.rings++;else if(m.kind==2)r.ghosts++;else if(m.kind==3)r.speedLines++;else if(m.kind==4)r.residue++;r.maxRemainingSeconds=Mathf.Max(r.maxRemainingSeconds,m.life-(now-m.born));}return r;}
 public void Initialize(Shader shader,FeelConfig config){cfg=config;mesh=new Mesh{name="Crab r28 comic impact mesh"};mesh.indexFormat=IndexFormat.UInt32;mesh.MarkDynamic();ink=new Material(shader);ink.renderQueue=3050;gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=ink;GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;}
 public void Clear(){marks.Clear();rotor=flash=false;if(mesh)mesh.Clear();}
 int MaterialID(string kind)=>kind=="glass"?1:kind=="metal"?2:kind=="wood"?3:kind=="dust"?4:kind=="water"||kind=="bubble"?5:kind=="confetti"?6:kind=="food"?8:kind=="star"?9:7;
 Color MaterialColor(int material,int n)=>material==1?(n%3==0?new Color(.13f,.46f,.53f,.85f):new Color(.54f,.92f,.93f,.85f)):material==2?(n%2==0?new Color(.32f,.4f,.49f,1):new Color(.83f,.86f,.8f,1)):material==3?(n%3==0?new Color(.3f,.12f,.055f,1):new Color(.8f,.39f,.16f,1)):material==4?new Color(.66f,.59f,.46f,.30f):material==5?new Color(.3f,.82f,.94f,.65f):material==6?(n%2==0?new Color(1,.35f,.12f,.85f):new Color(1,.8f,.2f,.85f)):material==8?(n%3==0?new Color(.3f,.6f,.12f,.9f):new Color(.95f,.38f,.08f,.9f)):new Color(1,.89f,.62f,.9f);
 public int Particles(Vector3 p,int count,string kind,Camera cam,float now,Vector3 impactVelocity){int added=0,id=MaterialID(kind);float heading=Mathf.Atan2(Vector3.Dot(impactVelocity,cam.transform.up),Vector3.Dot(impactVelocity,cam.transform.right));if(impactVelocity.sqrMagnitude<1)heading=.6f;
  for(int i=0;i<count&&marks.Count<512;i++){float seed=(i*.618034f)%1,fan=(seed-.5f)*2.3f,angle=heading+fan+(i%7==0?Mathf.PI:0);float speed=cfg.drawing.particleSpeedM*(.5f+(i%6)*.23f);if(id==4)speed*=.3f;var v=cam.transform.right*Mathf.Cos(angle)*speed+cam.transform.up*(Mathf.Sin(angle)*speed+2.2f);marks.Add(new Mark{kind=0,material=id,born=now,life=cfg.drawing.particleLifeMs/1000*(.7f+(i%5)*.16f),p=p,v=v,size=cfg.drawing.particleSizeM*(.7f+(i%4)*.6f),angle=seed*6.28f,turn=(i%2==0?1:-1)*(2+i%7),stretch=1.2f+(i%3),color=MaterialColor(id,i)});added++;}return added;}
 public void Ring(Vector3 p,float now,bool glass=false){if(marks.Count<512)marks.Add(new Mark{kind=1,born=now,life=cfg.drawing.ringLifeMs/1000,p=p,size=cfg.drawing.ringRadiusM*(glass?1.3f:1),angle=now%1,color=new Color(1,.52f,.12f,.8f)});}
 public void Ghost(Vector3 p,Camera cam,float now,Transform hero,Vector3 velocity){p-=velocity.sqrMagnitude>.01f?velocity.normalized*.5f:Vector3.zero;if(marks.Count<512){Vector3 axis=hero.right;marks.Add(new Mark{kind=2,born=now,life=cfg.drawing.ghostLifeMs/1000,p=p,size=1.05f,angle=Mathf.Atan2(Vector3.Dot(axis,cam.transform.up),Vector3.Dot(axis,cam.transform.right)),color=new Color(.1f,.74f,.88f,.11f)});}}
 public void Speed(Vector3 p,Camera cam,float now,int direction){for(int i=0;i<6&&marks.Count<512;i++)marks.Add(new Mark{kind=3,born=now,life=cfg.drawing.ghostLifeMs/1000,p=p+cam.transform.up*((i-2.5f)*.58f)-cam.transform.right*(direction*(1.7f+i%2)),v=cam.transform.right*(-direction*cfg.drawing.particleSpeedM*.6f),size=1.1f+(i%3)*.7f,color=new Color(.56f,.9f,.94f,.42f)});}
 public void Residue(Vector3 p,string material,Camera cam,float now,float duration){if(marks.Count<512)marks.Add(new Mark{kind=4,material=MaterialID(material),born=now,life=duration,p=new Vector3(p.x,.035f,p.z),size=cfg.drawing.residueSizeM,angle=now*3,color=MaterialColor(MaterialID(material),0)});}
 public void Flash(Camera cam,Color color,Vector3 position){flash=true;flashPosition=position;flashColor=color;flashColor.a=cfg.drawing.flashAlpha;}
 public void Rotor(Vector3 p,Transform hero,Camera cam,float strength,bool enabled){rotor=enabled&&!OverdrivePresentation.NeonSpinStyle;rotorPosition=p;rotorStrength=Mathf.Clamp01(strength);var axis=hero.right;rotorAngle=Mathf.Atan2(Vector3.Dot(axis,cam.transform.up),Vector3.Dot(axis,cam.transform.right));}
 void Tri(Vector3 a,Vector3 b,Vector3 c,Color color){int i=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);colors.Add(color);colors.Add(color);colors.Add(color);triangles.Add(i);triangles.Add(i+1);triangles.Add(i+2);}
 void Quad(Vector3 p,Vector3 u,Vector3 v,Color c){Tri(p-u-v,p+u-v,p+u+v,c);Tri(p-u-v,p+u+v,p-u+v,c);}
 void Fan(Vector3 p,Vector3 u,Vector3 v,float radius,int points,float inner,Color c,float angle=0){for(int i=0;i<points*2;i++){float a=angle+i*Mathf.PI/points,b=angle+(i+1)*Mathf.PI/points,ra=i%2==0?radius:radius*inner,rb=i%2==0?radius*inner:radius;Tri(p,p+(u*Mathf.Cos(a)+v*Mathf.Sin(a))*ra,p+(u*Mathf.Cos(b)+v*Mathf.Sin(b))*rb,c);}}
 void Arc(Vector3 p,Vector3 u,Vector3 v,float radius,float width,float start,float sweep,Color c){int n=Mathf.Max(5,Mathf.CeilToInt(cfg.budget.ringSegments*Mathf.Abs(sweep)/6.283f));for(int i=0;i<n;i++){float a=start+sweep*i/n,b=start+sweep*(i+1)/n;var x=u*Mathf.Cos(a)+v*Mathf.Sin(a);var y=u*Mathf.Cos(b)+v*Mathf.Sin(b);Tri(p+x*(radius-width),p+x*(radius+width),p+y*(radius+width),c);Tri(p+x*(radius-width),p+y*(radius+width),p+y*(radius-width),c);}}
 void Shard(Vector3 p,Vector3 u,Vector3 v,Mark m,Color c,float t){float a=m.angle+m.turn*t,sz=m.size*(1-t*.35f);var x=(u*Mathf.Cos(a)+v*Mathf.Sin(a))*sz;var y=(-u*Mathf.Sin(a)+v*Mathf.Cos(a))*sz;Color edge=new Color(.06f,.14f,.17f,c.a*.8f);
  if(m.material==4){Fan(p,u,v,sz*(1+t*2),7,.88f,c);return;}
  if(m.material==1){Tri(p-x*1.1f-y*.7f,p+x*m.stretch-y*.3f,p-x*.2f+y,c);Tri(p-x*1.1f-y*.7f,p+x*m.stretch-y*.3f,p-x*1.1f-y*.5f,edge);}
  else if(m.material==2){Quad(p,x*m.stretch,y*.28f,edge);Quad(p-y*.08f,x*m.stretch*.88f,y*.17f,c);}
  else if(m.material==3){Quad(p,x*m.stretch,y*.38f,edge);Tri(p-x*m.stretch+y*.3f,p+x*m.stretch,p-x*.7f-y*.3f,c);}
  else if(m.material==5)Arc(p,u,v,sz,.025f,a,3.7f,c);
  else if(m.material==6){Quad(p,x*1.2f,y*.45f,c);}
  else if(m.material==8)Fan(p,x,y,1,5,.7f,c,a);else if(m.material==9)Fan(p,x,y,1,5,.4f,c,a);else{Tri(p-x-y*.5f,p+x*1.25f-y*.25f,p+x*.3f+y,c);Arc(p,x.normalized,y.normalized,sz,.04f,a,1.5f,c);}
 }
 void CrabShadow(Vector3 p,Vector3 u,Vector3 v,float radius,float angle,Color c){var x=(u*Mathf.Cos(angle)+v*Mathf.Sin(angle))*radius;var y=(-u*Mathf.Sin(angle)+v*Mathf.Cos(angle))*radius;Fan(p,x,y*.62f,1,8,.9f,c);for(int side=-1;side<=1;side+=2){Fan(p+x*side*.88f+y*.55f,x*.55f,y*.5f,1,3,.55f,c);for(int k=0;k<3;k++)Tri(p+x*side*.5f+y*(.25f-k*.3f),p+x*side*(1.25f-k*.07f)+y*(-.12f-k*.29f),p+x*side*.45f+y*(.12f-k*.3f),c);}}
 public void Draw(Camera cam,float now){vertices.Clear();colors.Clear();triangles.Clear();var u=cam.transform.right;var v=cam.transform.up;var front=-cam.transform.forward*4;
  for(int i=marks.Count-1;i>=0;i--){var m=marks[i];float age=now-m.born;if(age>m.life){marks.RemoveAt(i);continue;}float t=Mathf.Clamp01(age/m.life);var p=m.p+m.v*age+front;if(m.kind==0)p-=v*(m.material==4?.2f:4.8f)*age*age;var c=m.color;c.a*=Mathf.Pow(1-t,.7f);
   if(m.kind==1){float radius=m.size*(.22f+t*.85f);if(t<.52f&&!NeonSkipImpactFans){Color dark=new Color(.055f,.12f,.13f,c.a*.78f);Fan(p,u,v,radius*.82f,9,.4f,dark,m.angle);Fan(p,u,v,radius*.65f,9,.35f,c,m.angle);}for(int k=0;k<3;k++)Arc(p,u,v,radius,.08f*(1-t)+.02f,m.angle+k*2.094f,1.35f,c);}
   else if(m.kind==2){if(!NeonSkipGhosts)CrabShadow(p,u,v,m.size,m.angle,c);}
   else if(m.kind==3){Tri(p-u*m.size,p+u*m.size,p+u*m.size*.15f+v*.045f,c);}
   else if(!(NeonSkipBubbles&&m.material==5))Shard(p,u,v,m,c,t);
  }
  if(rotor){var p=rotorPosition+front;var c=new Color(.2f,.85f,.94f,.24f+rotorStrength*.1f);for(int i=0;i<3;i++)Arc(p,u,v,1.5f+i*.12f,.04f+i*.008f,rotorAngle+i*2.094f,1.25f,c);}
  // The master flash gate still owns this one phase. Keep it at the impact, not across the scene.
  if(flash&&!NeonSkipImpactFans){var p=flashPosition+front;var dark=new Color(.04f,.10f,.12f,flashColor.a*2);Fan(p,u,v,2.35f,8,.22f,dark);var c=flashColor;c.a*=2.5f;Fan(p,u,v,1.9f,8,.24f,c);flash=false;}
  mesh.Clear();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
 }
 void OnDestroy(){if(mesh)Destroy(mesh);if(ink)Destroy(ink);}
}

using System;

namespace CrabDestructionVfx {
 // Shared by the Unity mesh adapter and offline preview. 84 vertices per mark is a hard ceiling.
 public sealed class CrabVfxGeometry {
  public const int VerticesPerMark=84;
  public readonly V3[] Vertices;public readonly Ink[] Colors;public readonly int[] Indices;
  public int VertexCount {get;private set;} public int IndexCount {get;private set;}
  Fleck current;
  public CrabVfxGeometry(int pool){Vertices=new V3[pool*VerticesPerMark];Colors=new Ink[Vertices.Length];Indices=new int[pool*VerticesPerMark*3];}
  public void Build(CrabVfxCore core){
   VertexCount=IndexCount=0;
   // Dust/ghost first; opaque-ish chips last. No per-frame sort or allocation.
   for(int pass=0;pass<2;pass++)for(int i=0;i<core.Particles.Length;i++){
    Fleck p=core.Particles[i];if(!p.live||p.Alpha<=.001f)continue;
    bool soft=p.mark==Mark.Dust||p.mark==Mark.Ghost;if((pass==0)!=soft)continue;current=p;
    switch(p.mark){
     case Mark.Dust:Fan(7,0,0,1,1,true);break;
     case Mark.Ghost:Crab();break;
     case Mark.Arc:Arc();break;
     case Mark.Shard:Triangle(-.75f,-.4f,.86f,-.18f,-.15f,.9f);break;
     case Mark.Splinter:Quad(-.9f,-.22f,.65f,-.17f,1,.05f,-.6f,.23f);break;
     case Mark.Chip:Quad(-.7f,-.45f,.8f,-.3f,.55f,.5f,-.55f,.3f);break;
     case Mark.Masonry:Fan(5,0,0,1.15f,.8f);break;
     case Mark.Ceramic:Ceramic();break;
     case Mark.Food:Fan(6,0,0,.72f,.6f);break;
     default:Quad(-1,-.12f,.8f,-.035f,1,.035f,-.8f,.12f);break;
    }
   }
  }
  int Vertex(float x,float y,float alpha=1){
   double a=current.angle*Math.PI/180;float xx=x*current.size*current.aspect,yy=y*current.size;
   int k=VertexCount++;Vertices[k]=current.position+new V3(xx*(float)Math.Cos(a)-yy*(float)Math.Sin(a),xx*(float)Math.Sin(a)+yy*(float)Math.Cos(a));
   var c=current.ink;c.a=current.Alpha*alpha;Colors[k]=c;return k;
  }
  void Tri(int a,int b,int c){Indices[IndexCount++]=a;Indices[IndexCount++]=b;Indices[IndexCount++]=c;}
  void Triangle(float a,float b,float c,float d,float e,float f){int k=Vertex(a,b);Vertex(c,d);Vertex(e,f);Tri(k,k+1,k+2);}
  void Quad(float a,float b,float c,float d,float e,float f,float g,float h){int k=Vertex(a,b);Vertex(c,d);Vertex(e,f);Vertex(g,h);Tri(k,k+1,k+2);Tri(k,k+2,k+3);}
  void Fan(int n,float x,float y,float rx,float ry,bool soft=false){
   int start=Vertex(x,y);for(int i=0;i<n;i++){
    double a=i*Math.PI*2/n;float jag=(i%2==0?1:.88f);Vertex(x+(float)Math.Cos(a)*rx*jag,y+(float)Math.Sin(a)*ry*jag,soft?0:1);
   }
   for(int i=0;i<n;i++)Tri(start,start+1+i,start+1+(i+1)%n);
  }
  void Arc(){
   int k=VertexCount;for(int i=0;i<=8;i++){
    double a=(-48+i*12)*Math.PI/180;float fade=(float)Math.Sin(i*Math.PI/8);
    Vertex((float)Math.Cos(a)*1.065f,(float)Math.Sin(a)*1.065f,fade);
    Vertex((float)Math.Cos(a)*.985f,(float)Math.Sin(a)*.985f,fade);
   }
   for(int i=0;i<8;i++){int v=k+i*2;Tri(v,v+1,v+2);Tri(v+1,v+3,v+2);}
  }
  void Ceramic(){
   // A thick curved rim reads differently from glass triangles or flat metal.
   for(int i=0;i<3;i++){
    double a=(-48+i*32)*Math.PI/180,b=(-16+i*32)*Math.PI/180;
    Quad((float)Math.Cos(a), (float)Math.Sin(a), (float)Math.Cos(b),(float)Math.Sin(b),
         (float)Math.Cos(b)*.52f,(float)Math.Sin(b)*.52f,(float)Math.Cos(a)*.52f,(float)Math.Sin(a)*.52f);
   }
  }
  void Crab(){
   Fan(8,0,0,.72f,.43f);
   for(int sign=-1;sign<=1;sign+=2){
    Triangle(sign*.50f,.24f,sign*.76f,.49f,sign*.96f,.36f);
    // Open pincer notch, not a filled circular blob.
    Triangle(sign*.77f,.46f,sign*.80f,.93f,sign*1.16f,.68f);
    Triangle(sign*.98f,.40f,sign*1.30f,.64f,sign*1.08f,.82f);
    for(int j=0;j<4;j++){
     float y=.18f-j*.14f;Quad(sign*.52f,y,sign*(.98f+j*.065f),y-.20f,sign*(1.04f+j*.065f),y-.15f,sign*.59f,y+.035f);
    }
   }
  }
 }
}

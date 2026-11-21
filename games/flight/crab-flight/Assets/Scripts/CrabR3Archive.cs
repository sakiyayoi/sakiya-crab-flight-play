using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

// Pure data reader/planner. It has no Unity native calls and can be checked offline.
public struct CrabR3Parameters {
 public float Claw,Shell,Legs;
 public CrabR3Parameters(float claw,float shell,float legs){Claw=Safe(claw);Shell=Safe(shell);Legs=Safe(legs);}
 public static float Safe(float x){return float.IsNaN(x)||float.IsInfinity(x)?45f:Math.Max(0f,Math.Min(100f,x));}
 public float For(byte kind){return kind==1?Shell:kind==2?Claw:kind==3?Legs:45f;}
}
public sealed class CrabR3MaterialData {public string Name;public float[] Color;public float Roughness;}
public sealed class CrabR3MeshData {
 public string Name;public int Material,VertexCount;public byte Kind;
 public float[] Positions,Normals,Colors;public int[] Indices;public float[][] PositionAnchors,NormalAnchors;
}
public sealed class CrabR3NodeData {
 public string Name,PartId;public int Parent,Mesh,Side;public byte Kind;public bool PartRoot,Body,Dactyl;
 public float[] Pose;public float[][] PoseAnchors;
}
public sealed class CrabR3Archive {
 public float DefaultValue;public float[] Anchors;public CrabR3MaterialData[] Materials;public CrabR3MeshData[] Meshes;public CrabR3NodeData[] Nodes;
 static void Need(bool ok,string text){if(!ok)throw new InvalidDataException(text);}
 static int Count(BinaryReader r,int min,int max){int n=r.ReadInt32();Need(n>=min&&n<=max,"Crab r3 count out of range");return n;}
 static float Number(BinaryReader r){float f=r.ReadSingle();Need(!float.IsNaN(f)&&!float.IsInfinity(f)&&Math.Abs(f)<10000,"Crab r3 nonfinite data");return f;}
 static float[] Floats(BinaryReader r,int n){float[] a=new float[n];for(int i=0;i<n;i++)a[i]=Number(r);return a;}
 static bool Flag(BinaryReader r){byte b=r.ReadByte();Need(b<=1,"Crab r3 invalid flag");return b==1;}
 static byte Kind(BinaryReader r){byte b=r.ReadByte();Need(b<=3,"Crab r3 invalid parameter binding");return b;}
 static string Text(BinaryReader r){int n=Count(r,0,4096);byte[] b=r.ReadBytes(n);Need(b.Length==n,"Crab r3 truncated string");return new UTF8Encoding(false,true).GetString(b);}
 static float[] PoseRead(BinaryReader r){float[] p=Floats(r,10);double norm=0;for(int i=3;i<7;i++)norm+=p[i]*p[i];Need(Math.Abs(norm-1)<.001,"Crab r3 invalid rotation");for(int i=7;i<10;i++)Need(p[i]>0&&p[i]<100,"Crab r3 invalid scale");return p;}
 public static CrabR3Archive Read(byte[] bytes){
  Need(bytes!=null&&bytes.Length>=32&&bytes.Length<=64000000,"Crab r3 invalid payload length");
  using(var stream=new MemoryStream(bytes,false))using(var r=new BinaryReader(stream)){
   Need(Encoding.ASCII.GetString(r.ReadBytes(4))=="CCB2","Crab r3 invalid magic");Need(r.ReadInt32()==1,"Crab r3 unsupported schema");
   var a=new CrabR3Archive();a.DefaultValue=Number(r);Need(a.DefaultValue==45,"Crab r3 wrong baseline");int anchors=Count(r,2,16);a.Anchors=Floats(r,anchors);Need(a.Anchors[0]==0&&a.Anchors[anchors-1]==100,"Crab r3 endpoint anchors missing");for(int i=1;i<anchors;i++)Need(a.Anchors[i]>a.Anchors[i-1],"Crab r3 unordered anchors");
   a.Materials=new CrabR3MaterialData[Count(r,1,64)];for(int i=0;i<a.Materials.Length;i++){var m=new CrabR3MaterialData{Name=Text(r),Color=Floats(r,4),Roughness=Number(r)};Need(m.Roughness>=0&&m.Roughness<=1,"Crab r3 roughness invalid");a.Materials[i]=m;}
   a.Meshes=new CrabR3MeshData[Count(r,1,1024)];int totalVertices=0;
   for(int i=0;i<a.Meshes.Length;i++){
    var m=new CrabR3MeshData{Name=Text(r)};m.Material=Count(r,0,a.Materials.Length-1);m.Kind=Kind(r);m.VertexCount=Count(r,3,100000);totalVertices+=m.VertexCount;Need(totalVertices<1000000,"Crab r3 vertex budget exceeded");int indexCount=Count(r,3,600000);Need(indexCount%3==0,"Crab r3 incomplete triangle");m.Positions=Floats(r,m.VertexCount*3);m.Normals=Floats(r,m.VertexCount*3);m.Colors=Floats(r,m.VertexCount*4);m.Indices=new int[indexCount];for(int j=0;j<indexCount;j++)m.Indices[j]=Count(r,0,m.VertexCount-1);
    if(Flag(r)){Need(m.Kind==1||m.Kind==3,"Crab r3 unexpected mesh deformation");m.PositionAnchors=new float[anchors][];m.NormalAnchors=new float[anchors][];for(int j=0;j<anchors;j++){m.PositionAnchors[j]=Floats(r,m.VertexCount*3);m.NormalAnchors[j]=Floats(r,m.VertexCount*3);}}
    a.Meshes[i]=m;
   }
   a.Nodes=new CrabR3NodeData[Count(r,1,2048)];var parts=new HashSet<string>();int body=0;
   for(int i=0;i<a.Nodes.Length;i++){
    var n=new CrabR3NodeData{Name=Text(r),Parent=r.ReadInt32(),Mesh=r.ReadInt32(),PartId=Text(r),Kind=Kind(r),Side=r.ReadInt32(),PartRoot=Flag(r),Body=Flag(r),Dactyl=Flag(r)};
    Need(i==0?n.Parent==-1:n.Parent>=0&&n.Parent<i,"Crab r3 invalid parent or cycle");Need(n.Mesh>=-1&&n.Mesh<a.Meshes.Length,"Crab r3 invalid mesh link");Need(n.Side>=-1&&n.Side<=1,"Crab r3 invalid side");if(n.PartRoot)Need(n.PartId.Length>0&&parts.Add(n.PartId),"Crab r3 duplicate part id");if(n.Body)body++;n.Pose=PoseRead(r);if(Flag(r)){n.PoseAnchors=new float[anchors][];for(int j=0;j<anchors;j++)n.PoseAnchors[j]=PoseRead(r);}a.Nodes[i]=n;
   }
   Need(parts.Count==11&&body==1,"Crab r3 attachment contract incomplete");Need(stream.Position==stream.Length,"Crab r3 trailing data");return a;
  }
 }
 void Bracket(float value,out int low,out int high,out float t){value=CrabR3Parameters.Safe(value);for(int i=0;i<Anchors.Length;i++)if(value==Anchors[i]){low=high=i;t=0;return;}for(int i=1;i<Anchors.Length;i++)if(value<Anchors[i]){low=i-1;high=i;t=(value-Anchors[low])/(Anchors[high]-Anchors[low]);return;}low=high=Anchors.Length-1;t=0;}
 float[] Interpolate(float[] baseline,float[][] anchors,byte kind,CrabR3Parameters p,bool normals){
  float value=p.For(kind);float[] result;
  if(anchors==null||value==DefaultValue)result=(float[])baseline.Clone();else{int low,high;float t;Bracket(value,out low,out high,out t);result=new float[baseline.Length];for(int i=0;i<result.Length;i++)result[i]=low==high?anchors[low][i]:anchors[low][i]+(anchors[high][i]-anchors[low][i])*t;}
  if(normals)for(int i=0;i<result.Length;i+=3){double len=Math.Sqrt(result[i]*result[i]+result[i+1]*result[i+1]+result[i+2]*result[i+2]);Need(len>1e-8,"Crab r3 collapsed normal");for(int j=0;j<3;j++)result[i+j]=(float)(result[i+j]/len);}
  return result;
 }
 public float[] PoseAt(int node,CrabR3Parameters p){var n=Nodes[node];return Interpolate(n.Pose,n.PoseAnchors,n.Kind,p,false);}
 public float[] PositionsAt(int mesh,CrabR3Parameters p){var m=Meshes[mesh];return Interpolate(m.Positions,m.PositionAnchors,m.Kind,p,false);}
 public float[] NormalsAt(int mesh,CrabR3Parameters p){var m=Meshes[mesh];return Interpolate(m.Normals,m.NormalAnchors,m.Kind,p,true);}
 // Pure equivalent of Unity TRS, used only to check transport against saved GLB world points.
 public static double[] Matrix(float[] p){double x=p[3],y=p[4],z=p[5],w=p[6],sx=p[7],sy=p[8],sz=p[9];return new[]{(1-2*y*y-2*z*z)*sx,(2*x*y-2*z*w)*sy,(2*x*z+2*y*w)*sz,p[0],(2*x*y+2*z*w)*sx,(1-2*x*x-2*z*z)*sy,(2*y*z-2*x*w)*sz,p[1],(2*x*z-2*y*w)*sx,(2*y*z+2*x*w)*sy,(1-2*x*x-2*y*y)*sz,p[2],0d,0d,0d,1d};}
 public static double[] Multiply(double[] a,double[] b){var c=new double[16];for(int y=0;y<4;y++)for(int x=0;x<4;x++)for(int k=0;k<4;k++)c[y*4+x]+=a[y*4+k]*b[k*4+x];return c;}
 public float[] BoundsAt(CrabR3Parameters p){
  var world=WorldMatrices(p);var bounds=new[]{float.PositiveInfinity,float.PositiveInfinity,float.PositiveInfinity,float.NegativeInfinity,float.NegativeInfinity,float.NegativeInfinity};
  for(int i=0;i<Nodes.Length;i++)if(Nodes[i].Mesh>=0){var vertices=PositionsAt(Nodes[i].Mesh,p);for(int v=0;v<vertices.Length;v+=3)for(int row=0;row<3;row++){double x=world[i][row*4+3];for(int k=0;k<3;k++)x+=world[i][row*4+k]*vertices[v+k];bounds[row]=Math.Min(bounds[row],(float)x);bounds[row+3]=Math.Max(bounds[row+3],(float)x);}}
  return bounds;
 }
 public double[][] WorldMatrices(CrabR3Parameters p){var result=new double[Nodes.Length][];for(int i=0;i<Nodes.Length;i++){var m=Matrix(PoseAt(i,p));result[i]=Nodes[i].Parent<0?m:Multiply(result[Nodes[i].Parent],m);}return result;}
}

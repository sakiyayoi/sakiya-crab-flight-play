using UnityEngine;
// Authored candidate deformation, applied only to a new runtime mesh copy.
// All extremal vertices and AABBs remain fixed; 11 mounts/13 compound colliders stay unchanged.
public static class CrabSculptR6 {
 public static bool Enabled=true;
 public static int ChangedVertices;public static float MaxDisplacement;
 static float Offset(Vector3 p,Bounds b,bool shell){
  float h=b.size.y;if(h<.01f)return 0;float t=(p.y-b.min.y)/h;if(t<=.0001f||t>=.9999f)return 0;
  float x=(p.x-b.center.x)/Mathf.Max(.001f,b.extents.x),z=(p.z-b.center.z)/Mathf.Max(.001f,b.extents.z);
  float edge=Mathf.Clamp01((1-Mathf.Abs(x))*8)*Mathf.Clamp01((1-Mathf.Abs(z))*8);
  float shape;
  if(shell){float g1=Mathf.Exp(-Mathf.Pow((Mathf.Abs(x)-.34f)/.11f,2)-Mathf.Pow((z+.02f)/.65f,2));float g2=Mathf.Exp(-Mathf.Pow(x/.6f,2)-Mathf.Pow((z-.48f)/.13f,2));shape=-.022f*g1-.012f*g2;}
  else{shape=.013f*Mathf.Exp(-Mathf.Pow(x/.72f,2)-Mathf.Pow((z+.26f)/.4f,2))-.008f*Mathf.Exp(-Mathf.Pow((z-.18f)/.13f,2));}
  return shape*(4*t*(1-t))*edge;
 }
 public static void Apply(string name,Vector3[] vertices,Vector3[] normals){
  if(!Enabled||vertices.Length==0)return;bool shell=name=="CarapacePart / surface 0";bool palm=name=="Claw_LPart / surface 0"||name=="Claw_RPart / surface 0";if(!shell&&!palm)return;
  var b=new Bounds(vertices[0],Vector3.zero);for(int i=1;i<vertices.Length;i++)b.Encapsulate(vertices[i]);float e=.001f;
  for(int i=0;i<vertices.Length;i++){var p=vertices[i];float dy=Offset(p,b,shell);if(Mathf.Abs(dy)<.0000001f)continue;float dx=(Offset(p+Vector3.right*e,b,shell)-Offset(p-Vector3.right*e,b,shell))/(2*e),ddy=(Offset(p+Vector3.up*e,b,shell)-Offset(p-Vector3.up*e,b,shell))/(2*e),dz=(Offset(p+Vector3.forward*e,b,shell)-Offset(p-Vector3.forward*e,b,shell))/(2*e);var n=normals[i];float ny=n.y/(1+ddy);normals[i]=new Vector3(n.x-dx*ny,ny,n.z-dz*ny).normalized;p.y=Mathf.Clamp(p.y+dy,b.min.y,b.max.y);vertices[i]=p;ChangedVertices++;MaxDisplacement=Mathf.Max(MaxDisplacement,Mathf.Abs(dy));}
 }
}

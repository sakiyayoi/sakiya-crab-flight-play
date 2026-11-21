using UnityEngine;

// Shared world geometry for collision queries and their visual forecast. A forecast
// samples future poses from the current positions; moving targets are never promised hits.
public static class AttackGeometry {
 public const float ShotRadius=.20f,ShotSpeed=16f,ShotLifetime=1.4f;
 public struct Volume {public Vector3 shaftStart,tip;public float radius;public Volume(Vector3 a,Vector3 b,float r){shaftStart=a;tip=b;radius=r;}}
 public struct Ray {public Vector3 muzzle,direction;public float radius,range;public Ray(Vector3 p,Vector3 d,float r,float length){muzzle=p;direction=d;radius=r;range=length;}}
 public struct PoseValue {public Vector3 position,scale;public Quaternion rotation;public PoseValue(Vector3 p,Quaternion q,Vector3 s){position=p;rotation=q;scale=s;}}
 public static PoseValue PoseAt(string kind,float t,Vector3 rest,Quaternion restRotation,Vector3 restScale,bool hostile,bool guard){
  float swing=0,extension=1;
  if(t>0){
   if(kind=="sword")swing=t<.17f?Mathf.Lerp(0,-65,t/.17f):t<.43f?Mathf.Lerp(-65,65,(t-.17f)/.26f):Mathf.Lerp(65,0,(t-.43f)/.22f);
   if(kind=="creature")extension=1+Mathf.Sin(Mathf.Clamp01(t/.55f)*Mathf.PI)*.55f;
  }
  var recoil=kind=="cannon"&&t>.18f&&t<.4f?restRotation*Vector3.forward*(Mathf.Sin((t-.18f)/.22f*Mathf.PI)*.12f):Vector3.zero;
  return new PoseValue(rest-recoil,restRotation*Quaternion.Euler(hostile&&guard&&t<.17f?-23:0,swing,0),Vector3.Scale(restScale,new Vector3(1,1,extension)));
 }
 public static Matrix4x4 MatrixAt(Matrix4x4 body,string kind,float t,Vector3 rest,Quaternion restRotation,Vector3 restScale,bool hostile,bool guard){var pose=PoseAt(kind,t,rest,restRotation,restScale,hostile,guard);return body*Matrix4x4.TRS(pose.position,pose.rotation,pose.scale);}
 public static Volume Contact(string kind,Matrix4x4 root,Vector3 localTip){return new Volume(root.MultiplyPoint3x4(kind=="creature"?Vector3.zero:new Vector3(0,0,.7f)),root.MultiplyPoint3x4(localTip),kind=="creature"?.28f:.17f);}
 public static Ray Cannon(Matrix4x4 root,Vector3 localTip){return new Ray(root.MultiplyPoint3x4(localTip),root.MultiplyVector(Vector3.forward).normalized,ShotRadius,ShotSpeed*ShotLifetime);}
}

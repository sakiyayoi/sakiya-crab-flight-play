using System;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
// Lightweight original visual actors. Birds and people never have hitboxes, score, force or destruction events.
// Explicit Tick keeps pause/reduced-motion/reset under host control. No automatic Update.
public sealed class FarWorldLifeR10:MonoBehaviour {
 public const string Revision="far-world-life-r10-candidate-1";
 public float Age=>age;public int StartledCount{get{int n=0;foreach(bool v in startled)if(v)n++;return n;}}
 public int BirdCount=>BirdX.Length*6-(RigidCrabFlight.LifeContactsEnabledR6?12:0);public int PeopleCount=>RigidCrabFlight.LifeContactsEnabledR6?0:PeopleX.Length*2;public int RendererCount=>3;public int TriangleCount{get;private set;}
 readonly List<Vector3>[] vertices={new List<Vector3>(8000),new List<Vector3>(8000),new List<Vector3>(8000)};
 readonly List<int>[] indices={new List<int>(14000),new List<int>(14000),new List<int>(14000)};
 readonly List<UnityEngine.Object> owned=new List<UnityEngine.Object>();readonly Mesh[] meshes=new Mesh[3];
 readonly bool[] startled=new bool[PeopleX.Length*2];readonly float[] surpriseAt=new float[PeopleX.Length*2];float age;bool ready;
 static readonly Vector3[] Signs={new Vector3(-1,-1,-1),new Vector3(1,-1,-1),new Vector3(1,1,-1),new Vector3(-1,1,-1),new Vector3(-1,-1,1),new Vector3(1,-1,1),new Vector3(1,1,1),new Vector3(-1,1,1)};
 static readonly int[] Face={0,3,2,1,4,5,6,7,0,4,7,3,1,2,6,5,3,7,6,2,0,1,5,4};
 static readonly Vector3[] Oct={Vector3.up,Vector3.right,Vector3.forward,Vector3.left,Vector3.back,Vector3.down};
 static readonly int[] OctTriangles={0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1};
 static readonly float[] BirdX={45,165,330,650,800,970,1150,1275};static readonly float[] PeopleX={15,60,145,280,628,705,827,906,1008,1150,1270};
 public static FarWorldLifeR10 Build(Transform parent,Shader shader){if(!parent||!shader)throw new ArgumentNullException("Visual life needs city origin and existing shader");var old=parent.Find(Revision);if(old)return old.GetComponent<FarWorldLifeR10>();var root=new GameObject(Revision);root.transform.SetParent(parent,false);var life=root.AddComponent<FarWorldLifeR10>();life.Initialize(shader);return life;}
 void Initialize(Shader shader){Color[] colors={new Color(.82f,.84f,.76f),new Color(.12f,.19f,.22f),new Color(.84f,.40f,.10f)};for(int i=0;i<3;i++){var mat=new Material(shader){name="Original bird and worker palette "+i,color=colors[i]};if(mat.HasProperty("_MainTex"))mat.SetTexture("_MainTex",Texture2D.whiteTexture);if(mat.HasProperty("_MetallicGlossMap"))mat.SetTexture("_MetallicGlossMap",Texture2D.blackTexture);owned.Add(mat);var mesh=new Mesh{name="Original life dynamic batch "+i,indexFormat=IndexFormat.UInt32};mesh.MarkDynamic();meshes[i]=mesh;owned.Add(mesh);var go=new GameObject("Visual actors palette "+i);go.transform.SetParent(transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=mat;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}ready=true;ResetVisuals();}
 public void ResetVisuals(){age=0;Array.Clear(startled,0,startled.Length);Array.Clear(surpriseAt,0,surpriseAt.Length);if(ready)Tick(0,new Vector3(0,-10000,0),false);}
 // dt must be host gameplay elapsed seconds: zero while paused; never integrate physical contact here.
 public void Tick(float dt,Vector3 crabPosition,bool reducedMotion){if(!ready)return;age+=Mathf.Clamp(dt,0,.05f);for(int i=0;i<3;i++){vertices[i].Clear();indices[i].Clear();}
  int bird=0;for(int side=-1;side<=1;side+=2)foreach(float bx in BirdX)for(int k=0;k<3;k++){
   if(RigidCrabFlight.ReplacesBirdR6(bx,k)){bird++;continue;}float phase=age*(2.2f+k*.17f)+bird*.72f;float flap=Mathf.Sin(phase)*(reducedMotion?.15f:.65f);var c=new Vector3(side*(bx+k*3.8f)+Mathf.Sin(age*.43f+bird)*2.8f,15+(bird%4)*8+Mathf.Sin(age*.60f+bird)*(reducedMotion?.15f:.70f),5.5f+(k%2)*1.5f);
   O(0,c,new Vector3(.88f,.38f,.42f),Quaternion.identity);O(2,c+new Vector3(side*.47f,.04f,0),new Vector3(.24f,.10f,.17f),Quaternion.identity);
   Wing(c,side,flap,-1);Wing(c,side,flap,1);bird++;
  }
  int human=0;for(int side=-1;side<=1;side+=2)foreach(float px in PeopleX){
   if(RigidCrabFlight.LifeContactsEnabledR6){human++;continue;}var home=new Vector3(side*px,0,4.5f+(human%3)*.65f);if(dt>0&&!startled[human]&&Mathf.Abs(crabPosition.x-home.x)<13&&crabPosition.y<12){startled[human]=true;surpriseAt[human]=age;}
   float surpriseTime=Mathf.Max(0,age-surpriseAt[human]);float gesture=startled[human]?Mathf.SmoothStep(0,1,surpriseTime/.18f):0;float step=reducedMotion?0:Mathf.Sin(age*6+human)*.08f;float away=gesture*(reducedMotion?.2f:Mathf.Min(2.0f,surpriseTime*1.8f));var c=home+new Vector3(away*side,gesture*(reducedMotion?0:Mathf.Sin(Mathf.Clamp01(surpriseTime/.55f)*Mathf.PI)*.25f),0);float scale=.90f+(human%3)*.10f;
   B(2,c+new Vector3(0,1.14f*scale,0),new Vector3(.55f,.70f,.35f)*scale,Quaternion.identity);
   O(0,c+new Vector3(0,1.80f*scale,0),new Vector3(.42f,.46f,.40f)*scale,Quaternion.identity);
   B(0,c+new Vector3(0,2.01f*scale,0),new Vector3(.50f,.10f,.43f)*scale,Quaternion.identity);
   for(int sign=-1;sign<=1;sign+=2){
    B(1,c+new Vector3(sign*.16f*scale,.44f*scale,0),new Vector3(.20f,.78f,.25f)*scale,Quaternion.Euler(0,0,sign*step*30));
    float armAngle=sign*(gesture*125+10);var shoulder=c+new Vector3(sign*.34f*scale,1.35f*scale,0);var q=Quaternion.Euler(0,0,armAngle);B(2,shoulder+q*new Vector3(0,-.25f*scale,0),new Vector3(.18f,.58f,.20f)*scale,q);O(0,shoulder+q*new Vector3(0,-.58f*scale,0),new Vector3(.19f,.20f,.18f)*scale,Quaternion.identity);
   }human++;
  }
  int triangles=0;for(int i=0;i<3;i++){meshes[i].SetVertices(vertices[i]);meshes[i].SetTriangles(indices[i],0);meshes[i].RecalculateNormals();meshes[i].RecalculateBounds();triangles+=indices[i].Count/3;}TriangleCount=triangles;
 }
 void B(int m,Vector3 c,Vector3 s,Quaternion rotation){for(int face=0;face<6;face++){int a=vertices[m].Count;for(int k=0;k<4;k++)vertices[m].Add(c+rotation*Vector3.Scale(Signs[Face[face*4+k]],s*.5f));Quad(m,a);}}
 void O(int m,Vector3 c,Vector3 s,Quaternion rotation){int a=vertices[m].Count;for(int i=0;i<Oct.Length;i++)vertices[m].Add(c+rotation*Vector3.Scale(Oct[i],s*.5f));for(int i=0;i<OctTriangles.Length;i++)indices[m].Add(a+OctTriangles[i]);}
 void Quad(int m,int a){indices[m].Add(a);indices[m].Add(a+1);indices[m].Add(a+2);indices[m].Add(a);indices[m].Add(a+2);indices[m].Add(a+3);}
 void Wing(Vector3 c,int direction,float flap,int side){int a=vertices[1].Count;vertices[1].Add(c+new Vector3(-direction*.12f,.05f,0));vertices[1].Add(c+new Vector3(direction*.14f,.06f,0));vertices[1].Add(c+new Vector3(-direction*.35f,flap*.7f,side*1.02f));indices[1].Add(a);indices[1].Add(a+1);indices[1].Add(a+2);indices[1].Add(a+2);indices[1].Add(a+1);indices[1].Add(a);}
 void OnDestroy(){foreach(var a in owned)if(a){if(Application.isPlaying)UnityEngine.Object.Destroy(a);else UnityEngine.Object.DestroyImmediate(a);}}
}

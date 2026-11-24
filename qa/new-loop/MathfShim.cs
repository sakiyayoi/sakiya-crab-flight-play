// Only the scalar Mathf operations used by SpinCharge. No physics/Unity simulation is mocked.
using System;
namespace UnityEngine { public static class Mathf {
 public static float Clamp(float v,float lo,float hi)=>Math.Clamp(v,lo,hi);
 public static float Clamp01(float v)=>Clamp(v,0,1);
 public static float Min(float a,float b)=>Math.Min(a,b);
 public static float Max(float a,float b)=>Math.Max(a,b);
 public static float Sqrt(float v)=>(float)Math.Sqrt(v);
 public static float Pow(float a,float b)=>(float)Math.Pow(a,b);
 public static float Exp(float a)=>(float)Math.Exp(a);
}}

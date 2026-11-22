Shader "PortPrototype/Hero" {
 Properties {_Color("Shell",Color)=(1,.192157,.192157,1)}
 SubShader { Tags {"RenderType"="Opaque"}
 Pass {Cull Front
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  float4 vert(appdata_base v):SV_POSITION{v.vertex.xyz+=v.normal*.025;return UnityObjectToClipPos(v.vertex);}
  fixed4 frag():SV_Target{return fixed4(.141176,.117647,.180392,1);}
  ENDCG
 }
 Pass {Cull Back
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  fixed4 _Color;
  struct v2f {float4 p:SV_POSITION;float3 n:TEXCOORD0;};
  v2f vert(appdata_base v){v2f o;o.p=UnityObjectToClipPos(v.vertex);o.n=UnityObjectToWorldNormal(v.normal);return o;}
  fixed4 frag(v2f i):SV_Target{float light=dot(normalize(i.n),normalize(float3(-.3,.8,-.5)));return fixed4(_Color.rgb*(light>.3?1:light>-.3?.85:.70),1);}
  ENDCG
 } }
}

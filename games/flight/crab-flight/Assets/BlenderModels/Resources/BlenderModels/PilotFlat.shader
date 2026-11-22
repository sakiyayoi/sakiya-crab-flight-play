Shader "Sakiya/BlenderPilotFlat" {
 Properties { _Color("Tint",Color)=(1,1,1,1) }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  struct appdata {float4 vertex:POSITION;float3 normal:NORMAL;fixed4 color:COLOR;};
  struct v2f {float4 pos:SV_POSITION;fixed4 color:COLOR;float3 normal:TEXCOORD0;};
  fixed4 _Color;
  v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);o.color=v.color;return o;}
  fixed4 frag(v2f i):SV_Target {
   // Two discrete matte planes; no specular, texture noise, or gradient.
   float shade=dot(normalize(i.normal),normalize(float3(-.4,.8,-.6)))>.05?1:.65;
   return fixed4(i.color.rgb*_Color.rgb*shade,1);
  }
  ENDCG
 } }
}

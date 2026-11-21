Shader "NeonProto/Unlit" {
 Properties { _Color("Color",Color)=(1,1,1,1) }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  float4 _Color;
  float4 vert(appdata_base v):SV_POSITION{return UnityObjectToClipPos(v.vertex);}
  fixed4 frag():SV_Target{return _Color;}
  ENDCG
 } }
}

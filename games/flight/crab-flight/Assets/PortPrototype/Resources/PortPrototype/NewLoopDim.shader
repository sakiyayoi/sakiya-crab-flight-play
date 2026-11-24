Shader "PortPrototype/NewLoopDim" {
 Properties { _Opacity("Scene dim",Range(0,.15))=0 }
 SubShader { Pass {
  ZTest Always ZWrite Off Cull Off Blend SrcAlpha OneMinusSrcAlpha
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  float _Opacity;
  float4 vert(appdata_base v):SV_POSITION { return float4(v.vertex.xy,0,1); }
  fixed4 frag():SV_Target { return fixed4(0,0,0,_Opacity); }
  ENDCG
 } }
}

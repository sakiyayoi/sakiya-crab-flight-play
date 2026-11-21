Shader "NeonProto/Scenery" {
 SubShader { Tags { "RenderType"="Opaque" } Pass { Cull Off
  CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  struct v2f {float4 pos:SV_POSITION;fixed4 color:COLOR;};
  v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}
  fixed4 frag(v2f i):SV_Target{return i.color;}
  ENDCG
 } }
}

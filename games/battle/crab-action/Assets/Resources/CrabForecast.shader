Shader "CustomCrabBattle/Forecast" {
 Properties { _Color("Forecast color",Color)=(0,0.7,1,0.35) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   ZTest LEqual
   Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   fixed4 _Color;
   struct appdata { float4 vertex:POSITION; };
   struct v2f { float4 position:SV_POSITION; };
   v2f vert(appdata v) { v2f o; o.position=UnityObjectToClipPos(v.vertex); return o; }
   fixed4 frag(v2f input):SV_Target { return _Color; }
   ENDCG
  }
 }
}

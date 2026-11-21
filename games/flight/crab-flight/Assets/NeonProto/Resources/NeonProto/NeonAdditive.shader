Shader "NeonProto/Additive" {
 Properties { _MainTex("Baked glow",2D)="white"{} _Color("Tint",Color)=(1,1,1,1) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Pass {
   Blend SrcAlpha One ZWrite Off Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _Color;
   struct v2f { float4 pos:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR; };
   v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord;o.color=v.color;return o;}
   fixed4 frag(v2f i):SV_Target{return tex2D(_MainTex,i.uv)*_Color*i.color;}
   ENDCG
  }
 }
}

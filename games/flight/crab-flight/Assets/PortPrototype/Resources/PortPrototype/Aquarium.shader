Shader "PortPrototype/Aquarium" {
 SubShader {Tags {"Queue"="Geometry-10" "RenderType"="Opaque"} Pass {Cull Off
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  struct v2f {float4 p:SV_POSITION;float2 uv:TEXCOORD0;};
  v2f vert(appdata_base v){v2f o;o.p=UnityObjectToClipPos(v.vertex);o.uv=v.vertex.xy+.5;return o;}
  fixed4 frag(v2f i):SV_Target{
   fixed3 c=lerp(fixed3(.10,.20,.31),fixed3(.24,.43,.52),smoothstep(0,1,i.uv.y));
   for(int k=0;k<12;k++){float2 p=float2(frac(k*.618+.12),frac(k*.393+.17));float d=length((i.uv-p)*float2(1.6,1));float r=.006+frac(k*.37)*.009;c+=fixed3(.20,.25,.24)*(1-smoothstep(.001,.004,abs(d-r)))*.55;}
   c+=(frac(sin(dot(i.uv,float2(127.1,311.7)))*43758.5453)-.5)*.012;
   return fixed4(c,1);
  }
  ENDCG
 } }
}

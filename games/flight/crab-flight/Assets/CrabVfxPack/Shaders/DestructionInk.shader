Shader "Crab/DestructionInk" {
 Properties { _AccentFocus ("Optional actor clear centre", Vector) = (0,0,0,0) }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
  Pass {
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off ZTest LEqual Cull Off
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct appdata {float4 vertex:POSITION;float4 color:COLOR;};
   float4 _AccentFocus;
   struct v2f {float4 vertex:SV_POSITION;float4 color:COLOR;float4 screen:TEXCOORD0;};
   v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.screen=ComputeScreenPos(o.vertex);return o;}
   fixed4 frag(v2f i):SV_Target{
    fixed4 ink=i.color;
    // Zero is the existing VFX rig's default: its original output is unchanged.
    // Only the owned Overdrive material supplies a shell-centre exclusion.
    if(_AccentFocus.w>0){float2 delta=i.screen.xy/i.screen.w-_AccentFocus.xy;delta.x*=_AccentFocus.w;ink.a*=smoothstep(_AccentFocus.z*.75,_AccentFocus.z,length(delta));}
    return ink;
   }
   ENDCG
  }
 }
 Fallback Off
}

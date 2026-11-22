Shader "PortPrototype/Atmosphere" {
 Properties {_Far("Baked far card",2D)="black"{} _Mid("Baked mid card",2D)="black"{} }
 SubShader {Tags {"RenderType"="Opaque"} Pass {Cull Off
 CGPROGRAM
 #pragma target 3.0
  #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _Far,_Mid;
 struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;};
 v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;return o;}
 fixed4 frag(v2f i):SV_Target {
  float x=(i.world.x-600)/300,y=i.world.y;
  float t=saturate(y/70);
  fixed3 sky=lerp(fixed3(.27451,.34510,.49804),fixed3(.07451,.10980,.22745),smoothstep(0,1,t));
  float2 moon=float2((i.world.x-687)/5.4,(y-42)/5.4);
  float disc=1-smoothstep(.94,1,length(moon));sky=lerp(sky,fixed3(.86,.89,.85),disc*.8);
  float2 cell=floor(float2(i.world.x*.37,y*.5));float hash=frac(sin(dot(cell,float2(127.1,311.7)))*43758.5453);
  float star=step(.988,hash)*(1-smoothstep(.03,.12,length(frac(float2(i.world.x*.37,y*.5))-.5)))*step(20,y);
  sky+=star*.3;
  float4 far=tex2D(_Far,float2(x,saturate(y/55)));
  float4 mid=tex2D(_Mid,float2(x,saturate(y/39)));
  sky=lerp(sky,fixed3(.13725,.18431,.32157),far.a);
  sky=lerp(sky,mid.rgb,mid.a);
  sky=lerp(sky,fixed3(.27451,.34510,.49804),.22*(1-smoothstep(-4,12,y)));
  float grain=frac(sin(dot(i.world.xy,float2(83.17,13.47)))*41719)-.5;
  return fixed4(sky+grain*.012,1);
 }
 ENDCG
 }}
}

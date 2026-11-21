Shader "NeonProto/BackdropAtlas" {
 Properties {
  _Color("Tint",Color)=(1,1,1,1)
  _FragmentReadability("Broken wood tone",Range(0,1))=0
  _MetalDebrisReadability("Broken metal tone",Range(0,1))=0
  _MainTex("Authored albedo atlas",2D)="white"{}
  _MetallicGlossMap("Metallic R / Smoothness A",2D)="black"{}
  _NeonGateMin("Absolute X start",Float)=28
  _NeonGateMax("Absolute X end",Float)=135
  _NeonLow("Night low",Color)=(.0196,.0235,.102,1)
  _NeonHigh("Night high",Color)=(.1647,.0863,.3137,1)
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 250
  CGPROGRAM
  #pragma surface surf Standard
  #pragma target 3.0
  sampler2D _MainTex,_MetallicGlossMap;
  float _NeonGateMin,_NeonGateMax;
  fixed4 _Color,_NeonLow,_NeonHigh;half _FragmentReadability,_MetalDebrisReadability;
  struct Input {float2 uv_MainTex;float3 worldPos;};
  void surf(Input IN,inout SurfaceOutputStandard o) {
   fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;
   fixed4 p=tex2D(_MetallicGlossMap,IN.uv_MainTex);
   half value=dot(c.rgb,half3(.2126,.7152,.0722));
   c.rgb=lerp(c.rgb,value*half3(.70,.79,.84),_FragmentReadability);
   c.rgb=lerp(c.rgb,c.rgb*half3(.46,.60,.67),_MetalDebrisReadability);
   // Static background remap; original R2 surface equations remain exact outside the selected sector.
   // No time, flicker, extra light or post processing. DepthTone is included in c above.
   if(abs(IN.worldPos.x)>=_NeonGateMin && abs(IN.worldPos.x)<_NeonGateMax){
    half3 navy=lerp(_NeonLow.rgb,_NeonHigh.rgb,saturate(value)*.6);
    half peak=max(c.r,max(c.g,c.b));
    half3 hue=peak>0?c.rgb*(max(navy.r,max(navy.g,navy.b))/peak):navy;
    c.rgb=lerp(navy,hue,.12);p.r=0;p.a=min(p.a,.12);
   }
   o.Albedo=c.rgb;o.Metallic=p.r;o.Smoothness=p.a;o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}

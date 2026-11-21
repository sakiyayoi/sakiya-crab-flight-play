Shader "NeonProto/BackdropSurface" {
 Properties {
  _Color("Original tint",Color)=(1,1,1,1)
  _MainTex("Original albedo",2D)="white"{}
  _Metallic("Metallic",Range(0,1))=0
  _Glossiness("Smoothness",Range(0,1))=.5
  _EmissionColor("Original emission",Color)=(0,0,0,1)
  _NeonDarkColor("Night tint",Color)=(.05,.04,.15,1)
  _NeonDarkEmission("Night emission",Color)=(0,0,0,1)
  _NeonGateMin("Absolute X start",Float)=0
  _NeonGateMax("Absolute X end",Float)=3220
 }
 SubShader {
  Tags {"RenderType"="Opaque"} LOD 200
  CGPROGRAM
  #pragma surface surf Standard
  #pragma target 3.0
  #pragma shader_feature_local _EMISSION
  sampler2D _MainTex;fixed4 _Color,_NeonDarkColor,_EmissionColor,_NeonDarkEmission;
  half _Metallic,_Glossiness;float _NeonGateMin,_NeonGateMax;
  struct Input {float2 uv_MainTex;float3 worldPos;};
  void surf(Input IN,inout SurfaceOutputStandard o){
   bool night=abs(IN.worldPos.x)>=_NeonGateMin&&abs(IN.worldPos.x)<_NeonGateMax;
   fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*(night?_NeonDarkColor:_Color);
   o.Albedo=c.rgb;o.Alpha=c.a;o.Metallic=_Metallic;o.Smoothness=night?min(_Glossiness,.18):_Glossiness;
   #ifdef _EMISSION
   o.Emission=night?_NeonDarkEmission.rgb:_EmissionColor.rgb;
   #endif
  }
  ENDCG
 }
 FallBack "Diffuse"
}

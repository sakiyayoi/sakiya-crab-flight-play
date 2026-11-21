Shader "CrabCityDensity/BrokenMetalVisibility" {
 Properties {
  _Color("Tint",Color)=(1,1,1,1)
  _FragmentReadability("Broken wood tone",Range(0,1))=0
  _MetalDebrisReadability("Broken metal tone",Range(0,1))=0
  _FragmentOpacity("Camera visibility opacity",Range(0,1))=1
  _MainTex("Authored albedo atlas",2D)="white"{}
  _MetallicGlossMap("Metallic R / Smoothness A",2D)="black"{}
 }
 SubShader {
  Tags {"RenderType"="Transparent" "Queue"="Transparent"}
  LOD 250
  ZWrite Off
  CGPROGRAM
  #pragma surface surf Standard alpha:fade
  #pragma target 3.0
  sampler2D _MainTex,_MetallicGlossMap;
  fixed4 _Color;half _FragmentReadability,_MetalDebrisReadability,_FragmentOpacity;
  struct Input {float2 uv_MainTex;};
  void surf(Input IN,inout SurfaceOutputStandard o){fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;fixed4 p=tex2D(_MetallicGlossMap,IN.uv_MainTex);half value=dot(c.rgb,half3(.2126,.7152,.0722));c.rgb=lerp(c.rgb,value*half3(.70,.79,.84),_FragmentReadability);c.rgb=lerp(c.rgb,c.rgb*half3(.46,.60,.67),_MetalDebrisReadability);o.Albedo=c.rgb;o.Metallic=p.r;o.Smoothness=p.a;o.Alpha=_FragmentOpacity;}
  ENDCG
 }
 FallBack Off
}

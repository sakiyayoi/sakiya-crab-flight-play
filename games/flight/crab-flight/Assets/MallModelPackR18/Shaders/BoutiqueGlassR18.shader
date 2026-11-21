Shader "MallModelR18/Glass" {
 Properties {_Color("Tint and transmission",Color)=(.54,.74,.78,.13) _Glossiness("Polished glass",Range(0,1))=.9 _Metallic("Reflection",Range(0,1))=.1 _MetalDebrisReadability("Metal debris",Range(0,1))=0 _FragmentReadability("Wood debris",Range(0,1))=0}
 SubShader {Tags {"Queue"="Transparent" "RenderType"="Transparent"} LOD 180
 ZWrite Off Cull Off
 CGPROGRAM
 #pragma surface surf Standard alpha:fade keepalpha
 #pragma target 3.0
 fixed4 _Color;half _Glossiness,_Metallic,_MetalDebrisReadability;struct Input{float3 viewDir;};
 void surf(Input IN,inout SurfaceOutputStandard o){half rim=pow(1-saturate(abs(dot(normalize(IN.viewDir),half3(0,0,1)))),3);o.Albedo=_Color.rgb*(1-_MetalDebrisReadability*.25);o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Alpha=saturate(_Color.a+rim*.16);}
 ENDCG
 } FallBack "Transparent/Diffuse"
}

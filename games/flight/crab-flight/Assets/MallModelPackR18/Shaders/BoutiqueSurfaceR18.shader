Shader "MallModelR18/Surface" {
 Properties {_Color("Tint",Color)=(1,1,1,1) _MainTex("Surface grain",2D)="white"{} _BumpMap("Fine normal",2D)="bump"{} _NormalScale("Normal",Range(0,1))=.25 _Metallic("Metal",Range(0,1))=0 _Glossiness("Smoothness",Range(0,1))=.3 _EmissionColor("Local diffuser",Color)=(0,0,0,1) _FragmentReadability("Wood debris",Range(0,1))=0 _MetalDebrisReadability("Metal debris",Range(0,1))=0}
 SubShader {Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap;fixed4 _Color,_EmissionColor;half _NormalScale,_Metallic,_Glossiness,_FragmentReadability,_MetalDebrisReadability;
 struct Input {float2 uv_MainTex;float2 uv_BumpMap;fixed4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o){fixed3 c=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;half ao=IN.color.r;half l=dot(c,half3(.2126,.7152,.0722));c=lerp(c,l*half3(.70,.79,.84),_FragmentReadability);c=lerp(c,c*half3(.46,.60,.67),_MetalDebrisReadability);o.Albedo=c*lerp(.76,1,ao);o.Normal=normalize(lerp(half3(0,0,1),UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap)),_NormalScale));o.Metallic=_Metallic;o.Smoothness=_Glossiness;o.Occlusion=ao;o.Emission=_EmissionColor.rgb;o.Alpha=1;}
 ENDCG
 } FallBack "Standard"
}

Shader "CrabLifeActors/VertexChitin" { Properties {} SubShader { Tags { "RenderType"="Opaque" } LOD 150
CGPROGRAM
#pragma surface surf Standard vertex:vert
#pragma target 3.0
struct Input{float4 color:COLOR;};void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.color=v.color;}
void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Smoothness=.22;o.Metallic=0;o.Emission=o.Albedo*.02;o.Alpha=1;}
ENDCG
} Fallback "Diffuse" }

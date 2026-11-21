Shader "CustomCrabBattle/ChitinR3" {
 Properties { _Color ("Authored base color",Color)=(1,1,1,1) _Roughness ("Roughness",Range(0,1))=.5 }
 SubShader {
  Tags { "RenderType"="Opaque" } LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows vertex:vert
  #pragma target 3.0
  fixed4 _Color; half _Roughness;
  struct Input { float4 vertexColor : COLOR; };
  void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.vertexColor=v.color;}
  void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=_Color.rgb*IN.vertexColor.rgb;o.Metallic=0;o.Smoothness=1-_Roughness;o.Alpha=1;}
  ENDCG
 }
 Fallback "Diffuse"
}

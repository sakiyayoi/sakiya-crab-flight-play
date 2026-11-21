Shader "CrabHeroR2/Chitin" {
 Properties { _Color("Chitin",Color)=(1,1,1,1) _Roughness("Roughness",Range(0,1))=.5 _MainTex("Existing pigment",2D)="white"{} _BumpMap("Existing normal",2D)="bump"{} _ORM("Existing ORM",2D)="white"{} _Mapped("Mapped",Float)=0 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap,_ORM;fixed4 _Color;half _Roughness,_Mapped;
 struct Input {float4 vertexColor:COLOR;float2 uv_MainTex;float3 localPos;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.vertexColor=v.color;o.localPos=v.vertex.xyz;}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float grain=sin(IN.localPos.x*43+sin(IN.localPos.z*31))*sin(IN.localPos.y*57+IN.localPos.z*29);
  o.Albedo=IN.vertexColor.rgb*_Color.rgb*(.985+.015*grain);o.Metallic=0;o.Occlusion=1;o.Smoothness=1-_Roughness;
  if(_Mapped>.5){fixed3 orm=tex2D(_ORM,IN.uv_MainTex).rgb;o.Albedo*=lerp(fixed3(.82,.82,.82),tex2D(_MainTex,IN.uv_MainTex).rgb,.38);o.Occlusion=lerp(.7,1,orm.r);o.Smoothness=lerp(1-_Roughness,1-orm.g,.28);fixed3 n=UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));n.xy*=.55;n.z=sqrt(1-saturate(dot(n.xy,n.xy)));o.Normal=n;}
  o.Emission=o.Albedo*.015;o.Alpha=1;
 }
 ENDCG
 } Fallback "Diffuse"
}

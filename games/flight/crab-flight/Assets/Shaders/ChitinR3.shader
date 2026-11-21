Shader "CustomCrabBattle/ChitinR3" {
 Properties { _HitWhite("Master-gated actor flash",Range(0,1))=0  _SmoothBias ("R6 coat definition",Range(-.2,.2))=0 _Color ("Authored base color",Color)=(1,1,1,1) _Roughness ("Roughness",Range(0,1))=.5 _MainTex("Pigment",2D)="white"{} _BumpMap("Chitin normal",2D)="bump"{} _ORM("Occlusion Roughness Metallic",2D)="white"{} _NormalScale("Normal strength",Range(0,1))=.65 _SurfaceMaps("Surface maps",Float)=0 _ActorReadability("Actor readability",Range(0,1))=0 }
 SubShader { Tags { "RenderType"="Opaque" } LOD 200
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 fixed4 _Color;half _HitWhite,_Roughness,_SurfaceMaps,_NormalScale,_ActorReadability,_SmoothBias;sampler2D _MainTex,_BumpMap,_ORM;
 struct Input {float4 vertexColor:COLOR;float3 localPos;float2 uv_MainTex;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.vertexColor=v.color;o.localPos=v.vertex.xyz;}
 void surf(Input IN,inout SurfaceOutputStandard o){float grain=sin(IN.localPos.x*43+sin(IN.localPos.z*31))*sin(IN.localPos.y*57+IN.localPos.z*29);o.Albedo=_Color.rgb*IN.vertexColor.rgb*(.96+.04*grain);o.Metallic=0;o.Smoothness=saturate(1-_Roughness+grain*.055);o.Alpha=1;
 if(_SurfaceMaps>.5){fixed3 orm=tex2D(_ORM,IN.uv_MainTex).rgb;o.Albedo*=tex2D(_MainTex,IN.uv_MainTex).rgb;o.Occlusion=orm.r;o.Smoothness=1-orm.g;o.Metallic=orm.b;fixed3 n=UnpackNormal(tex2D(_BumpMap,IN.uv_MainTex));n.xy*=_NormalScale;n.z=sqrt(1-saturate(dot(n.xy,n.xy)));o.Normal=n;}o.Smoothness=saturate(o.Smoothness+_SmoothBias);o.Albedo=lerp(o.Albedo,sqrt(max(o.Albedo,0))*fixed3(1.08,.84,.72),_ActorReadability*.60);o.Albedo=lerp(o.Albedo,fixed3(1,1,1),_HitWhite);}
 ENDCG
 } Fallback "Diffuse"
}

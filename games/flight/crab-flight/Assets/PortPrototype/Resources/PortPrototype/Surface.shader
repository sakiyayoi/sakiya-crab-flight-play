Shader "PortPrototype/Surface" {
 Properties { _Family("Family accent",Color)=(.7,.5,.3,1) _Span("Local X span",Vector)=(0,1,0,0) _Lamp("Nearest lantern",Vector)=(0,0,0,0) }
 SubShader { Tags { "RenderType"="Opaque" }
 Pass { Cull Front
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  float4 vert(appdata_base v):SV_POSITION { v.vertex.xyz+=v.normal*.024;return UnityObjectToClipPos(v.vertex); }
  fixed4 frag():SV_Target{return fixed4(.141176,.117647,.180392,1);}
  ENDCG
 }
 Pass { Cull Back
  CGPROGRAM
  #pragma target 3.0
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  fixed4 _Family;float4 _Span,_Lamp;
  struct v2f {float4 pos:SV_POSITION;float3 world:TEXCOORD0;float3 n:TEXCOORD1;float3 local:TEXCOORD2;fixed4 color:COLOR;};
  v2f vert(appdata_full v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.n=UnityObjectToWorldNormal(v.normal);o.local=v.vertex.xyz;o.color=v.color;return o;}
  fixed4 frag(v2f i):SV_Target {
   float lum=dot(i.color.rgb,float3(.2126,.7152,.0722));
   float stripe=saturate((i.local.x-_Span.x)/max(.001,_Span.y));
   fixed3 paper=lerp(fixed3(.30,.34,.43),fixed3(1,.953,.839),lum*.8);
   fixed3 base=lerp(paper,_Family.rgb,step(.70,stripe)*.82);
   float lit=dot(normalize(i.n),normalize(float3(-.3,.8,-.5)));
   float shade=lit>.45?1:lit>-.1?.80:.61;
   float lamp=saturate(1-distance(i.world,_Lamp.xyz)/9)*_Lamp.w;
   float rim=pow(1-saturate(dot(normalize(i.n),normalize(_WorldSpaceCameraPos-i.world))),4)*lamp*.12;
   return fixed4(base*shade+fixed3(1,.702,.278)*rim,1);
  }
  ENDCG
 } }
}

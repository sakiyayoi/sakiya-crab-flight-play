Shader "MallModelR18/Contact shade" {Properties {_Color("Shade",Color)=(.025,.04,.045,.45)} SubShader {Tags {"Queue"="Transparent-10" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha ZWrite Off Cull Off Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct appdata{float4 vertex:POSITION;fixed4 color:COLOR;};struct v2f{float4 pos:SV_POSITION;fixed4 color:COLOR;};fixed4 _Color;
 v2f vert(appdata v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.color=v.color;return o;}fixed4 frag(v2f i):SV_Target{return fixed4(_Color.rgb,_Color.a*i.color.a);}
 ENDCG}}}

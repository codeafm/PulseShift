Shader "Pulse/Spark" {Properties{_Color("Tint",Color)=(1,1,1,1)} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"}Blend SrcAlpha One ZWrite Off Cull Off Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
fixed4 _Color;struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};struct vf{float4 p:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};vf vert(app v){vf o;o.p=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}fixed4 frag(vf i):SV_Target{float r=length(i.uv-.5)*2;float a=pow(saturate(1-r),2);return fixed4(i.color.rgb*2,i.color.a*a);}
ENDCG}}}

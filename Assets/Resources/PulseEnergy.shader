Shader "Pulse/Energy" {Properties{_Color("Color",Color)=(0,.65,1,1)} SubShader{Tags{"Queue"="Transparent" "RenderType"="Transparent"}Blend One One ZWrite Off Cull Off Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
fixed4 _Color;struct app{float4 vertex:POSITION;fixed4 color:COLOR;};struct vf{float4 p:SV_POSITION;fixed4 color:COLOR;};vf vert(app v){vf o;o.p=UnityObjectToClipPos(v.vertex);o.color=v.color*_Color;return o;}fixed4 frag(vf i):SV_Target{return fixed4(i.color.rgb*i.color.a,i.color.a);}
ENDCG}}}

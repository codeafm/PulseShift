Shader "Pulse/Cliff foliage" { SubShader {Tags{"RenderType"="Opaque"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows
 struct Input{float4 color:COLOR;};
 void surf(Input IN,inout SurfaceOutputStandard o){o.Albedo=IN.color.rgb;o.Metallic=0;o.Smoothness=.15;o.Alpha=1;}
 ENDCG
 } Fallback "Diffuse" }

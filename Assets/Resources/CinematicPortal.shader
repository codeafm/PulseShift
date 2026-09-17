Shader "Pulse/Liquid portal" {
 Properties { _Tint("Energy colour",Color)=(0.02,0.65,1,1) }
 SubShader {Tags{"Queue"="Transparent" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass {CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 float _Clock;float4 _Tint;
 struct app{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 v2f vert(app v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 half4 frag(v2f i):SV_Target{
  float2 p=(i.uv-.5)*2;float r=length(p);float a=atan2(p.y,p.x);float t=_Clock*.35;
  float distortion=sin(p.x*17+p.y*11-t)*.22+sin(p.y*29-p.x*9+t)*.12;
  float spiral=pow(saturate(sin(a*3+r*17-t*3+distortion)),5);float fine=pow(saturate(sin(a*5-r*31+t*2+distortion)),14);
  float rim=exp(-pow((r-.88)*58,2));float inner=exp(-pow((r-.77)*85,2));
  float mask=(1-smoothstep(.89,.97,r));float3 deep=lerp(float3(.003,.014,.14),float3(.015,.16,.75),smoothstep(.05,.85,r));
  float3 c=deep+float3(.01,.62,1.6)*spiral*smoothstep(.04,.3,r)*.95+float3(.03,.27,.8)*fine*.42;
  c+=float3(.07,1.5,2.2)*(rim+inner*.55);c+=float3(.005,.16,.55)*exp(-r*9);
  float intensity=dot(c,float3(.15,.5,.35));
  c=lerp(_Tint.rgb*.09,_Tint.rgb*1.45,saturate(intensity));
  c+=lerp(_Tint.rgb,float3(.55,.85,1),.6)*rim*.9;
  c*=smoothstep(0,.13,r)*.9+.1;
  return half4(c,mask);
 }
 ENDCG}
 }Fallback Off
}

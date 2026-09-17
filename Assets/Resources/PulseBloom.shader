Shader "Hidden/PulseBloom" {
Properties { _MainTex ("Source",2D)="white"{} }
SubShader { Cull Off ZWrite Off ZTest Always
CGINCLUDE
#pragma target 3.0
#pragma multi_compile_local __ PULSE_LITE
#include "UnityCG.cginc"
sampler2D _MainTex,_Glow,_CameraDepthNormalsTexture;float4 _MainTex_TexelSize;
float _Impact,_Motion,_FeelTime,_Bloom,_Exposure,_Contrast;float4 _Focus;
int _OcclusionSamples;
float4 bright(v2f_img i):SV_Target {float3 c=tex2D(_MainTex,i.uv).rgb;return float4(max(c-1.05,0),1);}
float4 blur(v2f_img i):SV_Target {float2 d=_MainTex_TexelSize.xy*1.8;float4 c=tex2D(_MainTex,i.uv)*.2;c+=tex2D(_MainTex,i.uv+d)*.2;c+=tex2D(_MainTex,i.uv-d)*.2;c+=tex2D(_MainTex,i.uv+float2(d.x,-d.y))*.2;c+=tex2D(_MainTex,i.uv+float2(-d.x,d.y))*.2;return c;}
float4 combine(v2f_img i):SV_Target {float occ=0;
#if !defined(PULSE_LITE)
float d;float3 n;DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv),d,n);
for(int j=0;j<_OcclusionSamples;j++){float a=j*2.399;float2 off=float2(cos(a),sin(a))*_MainTex_TexelSize.xy*(4+j*14.4/max(1,_OcclusionSamples));float sd;float3 sn;DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,i.uv+off),sd,sn);float delta=d-sd;occ+=step(.00007,delta)*(1-smoothstep(.0001,.0025,delta));}
#endif
float2 delta=i.uv-_Focus.xy;float radius=length(delta);float ripple=sin(radius*32-_FeelTime*19)*exp(-radius*4)*_Impact*_Motion*.0014;
float2 uv=clamp(i.uv+normalize(delta+float2(.00001,.00001))*ripple,.001,.999);float2 split=(uv-.5)*_Impact*_Motion*.0018;
float3 scene=float3(tex2D(_MainTex,uv+split).r,tex2D(_MainTex,uv).g,tex2D(_MainTex,uv-split).b);
float3 c=(scene*(1-occ/max(1,_OcclusionSamples)*.48)+tex2D(_Glow,i.uv).rgb*(_Bloom+_Impact*.04))*_Exposure;c=(c*(2.51*c+.03))/(c*(2.43*c+.59)+.14);c=pow(max(c,0),_Contrast);float v=1-(.18+_Impact*.05)*pow(length(i.uv-.5)*1.4,2);return float4(c*v,1);}
float4 antialias(v2f_img i):SV_Target {
#if defined(PULSE_LITE)
 return float4(tex2D(_MainTex,i.uv).rgb,1);
#else
 float2 px=_MainTex_TexelSize.xy;float3 luma=float3(.299,.587,.114);
 float3 center=tex2D(_MainTex,i.uv).rgb;
 float nw=dot(tex2D(_MainTex,i.uv+px*float2(-1,1)).rgb,luma),ne=dot(tex2D(_MainTex,i.uv+px*float2(1,1)).rgb,luma);
 float sw=dot(tex2D(_MainTex,i.uv+px*float2(-1,-1)).rgb,luma),se=dot(tex2D(_MainTex,i.uv+px*float2(1,-1)).rgb,luma),mid=dot(center,luma);
 float low=min(mid,min(min(nw,ne),min(sw,se))),high=max(mid,max(max(nw,ne),max(sw,se)));
 if(high-low<max(.025,high*.09))return float4(center,1);
 float2 dir=float2(-((nw+ne)-(sw+se)),(nw+sw)-(ne+se));float reduce=max((nw+ne+sw+se)*.03125,.0078125);
 dir=clamp(dir/(min(abs(dir.x),abs(dir.y))+reduce),-6,6)*px;
 float3 a=.5*(tex2D(_MainTex,i.uv+dir*(-1.0/6)).rgb+tex2D(_MainTex,i.uv+dir*(1.0/6)).rgb);
 float3 b=a*.5+.25*(tex2D(_MainTex,i.uv-dir*.5).rgb+tex2D(_MainTex,i.uv+dir*.5).rgb);float lb=dot(b,luma);
 float3 result=lb<low||lb>high?a:b;return float4(result,1);
#endif
}
ENDCG
Pass { CGPROGRAM
#pragma vertex vert_img
#pragma fragment bright
ENDCG }
Pass { CGPROGRAM
#pragma vertex vert_img
#pragma fragment blur
ENDCG }
Pass { CGPROGRAM
#pragma vertex vert_img
#pragma fragment combine
ENDCG }
Pass { CGPROGRAM
#pragma vertex vert_img
#pragma fragment antialias
ENDCG }
}}

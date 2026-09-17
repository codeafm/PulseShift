Shader "Pulse/Mobile living mist" {
 Properties { _Density("Density",Range(0,1))=.7 _Clock("Time",Float)=0 _Aspect("Aspect",Float)=.56 _Near("Foreground",Float)=0 }
 SubShader { Tags {"Queue"="Transparent+50" "RenderType"="Transparent"} Blend SrcAlpha OneMinusSrcAlpha Cull Off ZWrite Off
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 float _Density,_Clock,_Aspect,_Near;
 output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 float hash(float2 p){float3 a=frac(float3(p.xyx)*.1031);a+=dot(a,a.yzx+33.33);return frac((a.x+a.y)*a.z);}
 float noise(float2 p){float2 n=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(n),hash(n+float2(1,0)),f.x),lerp(hash(n+float2(0,1)),hash(n+1),f.x),f.y);}
 float cloud(float2 p){return noise(p)*.55+noise(p*2.03+5.7)*.28+noise(p*4.01+17.3)*.17;}
 half4 frag(output i):SV_Target {
  float2 p=(i.uv-.5)*float2(_Aspect*5,5);float t=_Clock*(.026+_Near*.012);
  float n=cloud(p+float2(t,-t*.37)+cloud(p*.7+float2(-t*.3,0))*.8);
  float folds=smoothstep(.31,.78,n);float edge=smoothstep(.12,.49,abs(i.uv.x-.5));
  float lower=1-smoothstep(.03,.53,i.uv.y);float mask=lerp(.3+edge*.65+lower*.35,edge*.8+lower*.6,_Near);
  float clearCenter=1-.7*(1-smoothstep(.08,.32,abs(i.uv.x-.5)))*smoothstep(.2,.45,i.uv.y);
  float alpha=folds*mask*clearCenter*_Density*lerp(.34,.075,_Near);
  half3 tint=lerp(half3(.016,.042,.11),half3(.085,.13,.25),saturate(n+sin(p.x+t)*.15));
  return half4(tint,alpha);
 }
 ENDCG }
 } Fallback Off
}

Shader "Pulse/MenuBackdrop" {
 Properties {
  _MainTex ("Background", 2D) = "white" {}
  _Exposure ("Background brightness", Range(.25,1)) = .62
  _Saturation ("Background saturation", Range(0,1.5)) = .9
  _Vignette ("Edge darkening", Range(0,1)) = .34
  _TopShade ("Top UI shade", Range(0,.6)) = .22
  _BottomShade ("Bottom UI shade", Range(0,.6)) = .30
  _LifeClock ("Atmosphere time", Float) = 0
 }
 SubShader {
  Tags { "Queue"="Background" "RenderType"="Opaque" }
  Cull Off ZWrite Off ZTest Always
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex;float4 _MainTex_ST;float _Exposure,_Saturation,_Vignette,_TopShade,_BottomShade,_LifeClock;
   struct Input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
   struct Output {float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
   Output vert(Input v){Output o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=TRANSFORM_TEX(v.uv,_MainTex);return o;}
   fixed4 frag(Output i):SV_Target {
    float3 c=tex2D(_MainTex,i.uv).rgb;
    float l=dot(c,float3(.2126,.7152,.0722));c=lerp(l.xxx,c,_Saturation);
    float2 edge=(i.uv-.5)*float2(1.32,.78);float vignette=saturate(dot(edge,edge)*1.35);
    float top=smoothstep(.68,.98,i.uv.y)*_TopShade;
    float bottom=(1-smoothstep(.02,.28,i.uv.y))*_BottomShade;
    float side=(1-smoothstep(.02,.20,min(i.uv.x,1-i.uv.x)))*.12;
    c*=max(.18,_Exposure*(1-_Vignette*vignette)*(1-top)*(1-bottom)*(1-side));
    // Light drifting through the lower cloud banks; never warp the painted landing platform.
    float2 uv=i.uv;float t=_LifeClock;
    float billow=sin(uv.x*19+uv.y*31+t*.12+sin(uv.y*17-t*.09)*2);
    billow*=sin(uv.x*9-uv.y*23-t*.07);
    float cloudMask=(1-smoothstep(.25,.64,uv.y))*smoothstep(.015,.15,uv.y);
    float edgeMask=smoothstep(.07,.35,abs(uv.x-.47));
    c+=float3(.08,.15,.24)*pow(saturate(billow*.5+.5),3)*cloudMask*edgeMask*.22;
    float shafts=pow(saturate(sin(uv.x*22+uv.y*8+t*.055)),12);
    c+=float3(.22,.17,.12)*shafts*smoothstep(.2,.45,uv.y)*(1-smoothstep(.48,.7,uv.y))*.055;
    return fixed4(c,1);
   }
   ENDCG
  }
 }
}

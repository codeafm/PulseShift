Shader "Pulse/Clouds" {
 Properties {_MainTex("Archipelago backdrop",2D)="white"{} _MistTint("Mist tint",Color)=(.1,.18,.34,1) _UseBackdrop("Use artwork",Float)=0 _ScreenAspect("Screen aspect",Float)=.5625}
 SubShader {Tags{"Queue"="Background"} Cull Off ZWrite Off Pass { CGPROGRAM
 #pragma vertex vert_img
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize,_MistTint;float _UseBackdrop,_ScreenAspect;
 float hash(float2 p){float3 a=frac(float3(p.xyx)*.1031);a+=dot(a,a.yzx+33.33);return frac((a.x+a.y)*a.z);}
 float noise(float2 p){float2 n=floor(p),f=frac(p);f=f*f*(3-2*f);return lerp(lerp(hash(n),hash(n+float2(1,0)),f.x),lerp(hash(n+float2(0,1)),hash(n+1),f.x),f.y);}
 half4 frag(v2f_img i):SV_Target {
  float2 p=i.uv*float2(4,7)+float2(_Time.y*.014,-_Time.y*.006);float n=noise(p)*.6+noise(p*2.03+5)*.28+noise(p*4.01)*.12;
  if(_UseBackdrop<.5)return half4(lerp(_MistTint.rgb*.32,_MistTint.rgb*1.5,smoothstep(.3,.8,n)),1);
  float aspect=_MainTex_TexelSize.z/max(1,_MainTex_TexelSize.w);float2 crop=float2(min(1,_ScreenAspect/aspect),min(1,aspect/_ScreenAspect));
  float2 uv=(i.uv-.5)*crop*.975+.5+float2(sin(_Time.y*.045)*.002,cos(_Time.y*.031)*.002);
  half3 art=tex2D(_MainTex,uv).rgb;half3 theme=lerp(half3(.16,.26,.46),_MistTint.rgb,.35);
  // Preserve dark values; additive blue fog used to flatten the whole mobile image.
  half3 mist=theme*(smoothstep(.48,.85,n)*.018);
  return half4(art*.63+mist,1);
 }
 ENDCG }}}

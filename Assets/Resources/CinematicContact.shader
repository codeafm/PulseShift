Shader "Pulse/CinematicContact" {
 SubShader { Tags { "Queue"="Transparent" "RenderType"="Transparent" } Blend One One ZWrite Off Cull Off
  Pass { CGPROGRAM
  #pragma vertex vert
  #pragma fragment frag
  #include "UnityCG.cginc"
  struct v2f{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};
  v2f vert(appdata_base v){v2f o;o.pos=UnityObjectToClipPos(v.vertex);o.uv=v.texcoord.xy;return o;}
  float4 frag(v2f i):SV_Target{float r=length(i.uv*2-1);float pool=exp(-r*r*5)*.42;float rim=exp(-pow((r-.73)*42,2))*2.1;float halo=exp(-pow((r-.73)*9,2))*.25;return float4(float3(.012,.52,1)*(pool+rim+halo)*(1-smoothstep(.9,1,r)),0);}
  ENDCG }
 }
}

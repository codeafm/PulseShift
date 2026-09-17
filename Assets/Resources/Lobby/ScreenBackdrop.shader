Shader "Pulse/UI Screen Backdrop" {
 Properties {
  [PerRendererData] _MainTex("Texture",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _StencilComp("Stencil comparison",Float)=8 _Stencil("Stencil ID",Float)=0
  _StencilOp("Stencil operation",Float)=0 _StencilWriteMask("Write mask",Float)=255
  _StencilReadMask("Read mask",Float)=255 _ColorMask("Color mask",Float)=15
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True"}
  Stencil {Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask]}
  Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
  Blend SrcAlpha OneMinusSrcAlpha ColorMask [_ColorMask]
  Pass {CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
   #include "UnityCG.cginc"
   #include "UnityUI.cginc"
   struct app {float4 vertex:POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;};
   struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;fixed4 color:COLOR;float4 local:TEXCOORD1;};
   sampler2D _MainTex;fixed4 _Color;float4 _ClipRect;
   output vert(app v){output o;o.local=v.vertex;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color*_Color;return o;}
   fixed4 frag(output i):SV_Target {
    fixed4 c=tex2D(_MainTex,i.uv)*i.color;
    #ifdef UNITY_UI_CLIP_RECT
     c.a*=UnityGet2DClipping(i.local.xy,_ClipRect);
    #endif
    return c;
   }
  ENDCG}
 }
}

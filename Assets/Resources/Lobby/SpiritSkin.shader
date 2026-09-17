Shader "Pulse/Spirit Skin" {
 Properties { _MainTex("Albedo",2D)="white"{} _BumpMap("Normals",2D)="bump"{} _EmissionMap("Emission",2D)="black"{} _Tint("Spirit colour",Color)=(0,0.8,1,1) _SkinIndex("Skin",Float)=0 _IsHero("Hero material",Float)=0 _Motion("Flame motion",Float)=1 }
 SubShader {Tags{"RenderType"="Opaque"} LOD 250
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap,_EmissionMap;fixed4 _Tint;float _SkinIndex,_IsHero,_Motion;
 struct Input {float2 uv_MainTex;float2 uv_BumpMap;float3 localPosition;float3 viewDir;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPosition=v.vertex.xyz;
  float tip=smoothstep(.88,1.38,v.vertex.y)*_IsHero*_Motion;
  v.vertex.x+=sin(v.vertex.y*12+_Time.y*2.3+v.vertex.z*8)*.009*tip;
  // A slightly fuller suit gives the small character a stronger silhouette.
  v.vertex.x*=1+.08*_IsHero*(1-smoothstep(.39,.48,v.vertex.y));
 }
 void surf(Input IN,inout SurfaceOutputStandard o){
  fixed3 base=tex2D(_MainTex,IN.uv_MainTex).rgb;fixed3 glow=tex2D(_EmissionMap,IN.uv_MainTex).rgb;
  float bright=max(base.r,max(base.g,base.b)),energy=max(glow.r,max(glow.g,glow.b));
  float suit=_IsHero*(1-smoothstep(.4,.47,IN.localPosition.y));
  o.Albedo=lerp(base,bright*lerp(_Tint.rgb,float3(.08,.1,.16),.12),.98)*lerp(1,.33,suit);
  o.Normal=UnpackNormal(tex2D(_BumpMap,IN.uv_BumpMap));o.Metallic=lerp(.25,.38,suit);o.Smoothness=lerp(.42,.68,suit);
  float flame=_IsHero*smoothstep(.75,.93,IN.localPosition.y);
  float ribbons=.65+.35*sin(IN.localPosition.x*75+sin(IN.localPosition.y*12)*3+IN.localPosition.z*21);
  o.Emission=energy*_Tint.rgb*lerp(1.6,2.2*ribbons,flame)*lerp(1,.15,suit);o.Alpha=1;
  if(_IsHero>.5){
   float rim=pow(1-saturate(dot(normalize(IN.viewDir),o.Normal)),2.8);
   float flow=.5+.5*sin(IN.localPosition.y*27-IN.localPosition.x*42+sin(IN.localPosition.z*21+IN.localPosition.y*9)*2-_Time.y*1.7*_Motion);
   float3 core=lerp(_Tint.rgb,float3(.18,.78,1),step(1.5,_SkinIndex)*.5);
   core=lerp(core,float3(.78,.94,1),.28);
   float3 fire=lerp(_Tint.rgb,core,flow*.65+rim*.25);
   float galactic=step(1.5,_SkinIndex)*(1-step(2.5,_SkinIndex));
   float height=smoothstep(.83,1.36,IN.localPosition.y);
   float filament=pow(saturate(flow),10);
   fire=lerp(fire,lerp(float3(.035,.6,1),float3(.5,.075,1),height),galactic*.82);
   o.Albedo=lerp(o.Albedo,lerp(float3(.009,.018,.045),_Tint.rgb,.22)*(.6+bright*.5),suit);
   o.Emission=energy*lerp(_Tint.rgb,core,.42)*lerp(1.7,.12,suit);
   o.Albedo=lerp(o.Albedo,fire*.16,flame);
   o.Emission+=flame*(fire*(.62+flow*.24+rim*.8)+core*filament*.65);
   float3 cell=floor(IN.localPosition*155);float star=frac(sin(dot(cell,float3(12.9898,78.233,37.719)))*43758.5453);
   o.Emission+=galactic*step(.994,star)*float3(.45,.7,1)*(.5+flame)*.7;
   o.Emission+=rim*_Tint.rgb*.22*suit;
   float eyes=(1-smoothstep(.065,.085,abs(IN.localPosition.y-.616)))*smoothstep(.21,.23,-IN.localPosition.z);
   o.Emission+=eyes*energy*lerp(_Tint.rgb,float3(.55,.96,1),.55)*1.15;
   o.Smoothness=lerp(o.Smoothness,.48,flame);
  }
 }
 ENDCG
 } FallBack "Standard"
}

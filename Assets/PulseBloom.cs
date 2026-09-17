using UnityEngine;
public class PulseBloom:MonoBehaviour {
 [Range(0,1)]public float bloom=.2f;
 [Range(.5f,2)]public float exposure=.92f;
 [Range(.8f,1.3f)]public float contrast=1.07f;
 Material material;
 PulseCameraFeel cachedFeel;
 Camera effectCamera;bool ownsDepthNormals;
 void OnEnable(){
  // The authored menu preview does not run OnRenderImage. Do not introduce tone
  // mapping, exposure or screen-space shadows only when entering Play/builds.
  var stage=GetComponentInParent<LobbyStage>();
  if(stage&&stage.lobby&&stage.lobby.standaloneScene){enabled=false;return;}
  effectCamera=GetComponent<Camera>();ConfigureDepth();
 }
 void ConfigureDepth(){
  if(!effectCamera)effectCamera=GetComponent<Camera>();
  if(MobilePerformance.Lite){if(ownsDepthNormals){effectCamera.depthTextureMode&=~DepthTextureMode.DepthNormals;ownsDepthNormals=false;}}
  else if((effectCamera.depthTextureMode&DepthTextureMode.DepthNormals)==0){effectCamera.depthTextureMode|=DepthTextureMode.DepthNormals;ownsDepthNormals=true;}
 }
 void OnRenderImage(RenderTexture src,RenderTexture dst){
  if(!material){var shader=Resources.Load<Shader>("PulseBloom");if(!shader||!shader.isSupported){Graphics.Blit(src,dst);return;}material=new Material(shader);}
  if(!cachedFeel)TryGetComponent(out cachedFeel);var feel=cachedFeel;material.SetFloat("_Impact",feel?feel.Amount*feel.intensity:0);material.SetFloat("_Motion",feel&&!feel.reducedMotion?1:0);material.SetFloat("_FeelTime",feel?feel.Clock:0);material.SetVector("_Focus",feel?new Vector4(feel.Focus.x,feel.Focus.y,0,0):new Vector4(.5f,.5f,0,0));
  material.SetFloat("_Bloom",bloom);material.SetFloat("_Exposure",exposure);material.SetFloat("_Contrast",contrast);
  ConfigureDepth();bool lite=MobilePerformance.Lite;
  if(lite)material.EnableKeyword("PULSE_LITE");else material.DisableKeyword("PULSE_LITE");
  // Keep shader output linear on every platform; the destination handles sRGB encoding.
  var format=SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)?RenderTextureFormat.ARGBHalf:RenderTextureFormat.Default;
  bool mobile=Application.isMobilePlatform;int divisor=mobile?4:3;
  material.SetInt("_OcclusionSamples",mobile?6:12);
  RenderTexture a=null,b=null;
  if(lite)material.SetTexture("_Glow",Texture2D.blackTexture);
  else{
   a=RenderTexture.GetTemporary(Mathf.Max(1,src.width/divisor),Mathf.Max(1,src.height/divisor),0,format);b=RenderTexture.GetTemporary(a.width,a.height,0,format);
   a.filterMode=b.filterMode=FilterMode.Bilinear;
   Graphics.Blit(src,a,material,0);for(int i=0;i<(mobile?1:3);i++){Graphics.Blit(a,b,material,1);Graphics.Blit(b,a,material,1);}material.SetTexture("_Glow",a);
  }
  // Bound expensive post-processing, not the screen/UI resolution. Preserve tone mapping.
  float effectScale=mobile?Mathf.Min(1,(lite?720f:900f)/Mathf.Max(1,src.width)):1;
  var composite=RenderTexture.GetTemporary(Mathf.Max(1,Mathf.RoundToInt(src.width*effectScale)),Mathf.Max(1,Mathf.RoundToInt(src.height*effectScale)),0,format);composite.filterMode=FilterMode.Bilinear;Graphics.Blit(src,composite,material,2);
  bool priorSrgbWrite=GL.sRGBWrite;
  GL.sRGBWrite=QualitySettings.activeColorSpace==ColorSpace.Linear&&(dst==null||dst.sRGB);
  try{Graphics.Blit(composite,dst,material,3);}finally{GL.sRGBWrite=priorSrgbWrite;}
  RenderTexture.ReleaseTemporary(composite);if(a)RenderTexture.ReleaseTemporary(a);if(b)RenderTexture.ReleaseTemporary(b);
 }
 void OnDestroy(){if(material)Destroy(material);}
}

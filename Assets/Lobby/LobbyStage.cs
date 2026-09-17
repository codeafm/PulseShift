using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class LobbyStage:MonoBehaviour {
 public PulseLobby lobby;public Camera view;public Transform heroGroup,portalGroup,bridgeA,bridgeB,backdrop,spirit;
 [Header("Menu background only")]
 [Tooltip("Texture point where the hero's feet must meet the painted foreground stone.")]
 public Vector2 paintedLandingUv=new Vector2(.47f,.35f);
 public bool keepPaintedLandingUnderHero=true;
 [Header("Editable background framing")]
 public Vector2 backgroundOffset;
 [Range(.8f,1.5f)]public float backgroundZoom=1;
 public bool reducedMotion;Vector2 size;float clock,nextIdleGesture=1.4f;int reactionIndex;Material skin,portalSkin;
 readonly Dictionary<Transform,Vector3[]> neutralGeometry=new();
 MaterialPropertyBlock backgroundProperties;
 ReferenceMotion[] cachedMotions;
 void OnEnable(){cachedMotions=null;}
 void LateUpdate(){
  if(!lobby||!lobby.IsOpen||!backdrop)return;var renderer=backdrop.GetComponent<Renderer>();
  if(!renderer||renderer.sharedMaterial.shader.name!="Pulse/MenuBackdrop")return;
  if(backgroundProperties==null)backgroundProperties=new MaterialPropertyBlock();renderer.GetPropertyBlock(backgroundProperties);
  backgroundProperties.SetFloat("_LifeClock",reducedMotion?0:clock);renderer.SetPropertyBlock(backgroundProperties);
 }
 public void Build(){
  var cameraObject=new GameObject("Menu camera • separate from gameplay camera");cameraObject.transform.SetParent(transform,false);view=cameraObject.AddComponent<Camera>();view.transform.SetPositionAndRotation(new Vector3(0,10,-16),Quaternion.Euler(34,0,0));view.fieldOfView=30;view.nearClipPlane=.1f;view.farClipPlane=120;view.clearFlags=CameraClearFlags.SolidColor;view.backgroundColor=new Color(.015f,.02f,.07f);view.allowHDR=true;view.cullingMask=1<<28;view.gameObject.AddComponent<PulseBloom>();
  var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);quad.name="Archipelago • imagegen distant backdrop";DestroyCollider(quad);backdrop=quad.transform;backdrop.SetParent(view.transform,false);backdrop.localPosition=new Vector3(0,0,80);var material=new Material(Shader.Find("Unlit/Texture")){name="Lobby / cosmic background"};material.mainTexture=Resources.Load<Texture2D>("Lobby/ArchipelagoBackdrop");quad.GetComponent<Renderer>().sharedMaterial=material;
  heroGroup=Group("Hero island • editable");Put("Normal",heroGroup,new Vector3(0,-.12f,0),1.15f);spirit=Put("Spirit",heroGroup,new Vector3(0,.16f,0),2.75f);spirit.name="Hero • living Blender model";
  portalGroup=Group("Portal island • editable");Put("Normal",portalGroup,new Vector3(0,-.05f,0),1.4f);Put("Portal",portalGroup,new Vector3(0,.17f,0),1.4f);
  bridgeA=Group("Stepping stone A");Put("Rotate",bridgeA,Vector3.zero,1);bridgeB=Group("Stepping stone B");Put("Normal",bridgeB,Vector3.zero,.85f);
  foreach(var model in GetComponentsInChildren<ReferenceMotion>(true))model.menuActor=true;
  var light=new GameObject("Lobby • soft cyan key").AddComponent<Light>();light.transform.SetParent(transform,false);light.type=LightType.Directional;light.color=new Color(.48f,.77f,1);light.intensity=.9f;light.transform.rotation=Quaternion.Euler(45,-22,0);light.cullingMask=1<<28;
  foreach(var node in GetComponentsInChildren<Transform>(true))node.gameObject.layer=28;
 }
 static void DestroyCollider(GameObject g){if(Application.isPlaying)Destroy(g.GetComponent<Collider>());else DestroyImmediate(g.GetComponent<Collider>());}
 Transform Group(string name){var t=new GameObject(name).transform;t.SetParent(transform,false);return t;}
 Transform Put(string id,Transform parent,Vector3 pos,float scale){var t=ReferenceArt.Make(id,Vector3.zero).transform;t.SetParent(parent,false);t.localPosition=pos;t.localScale=Vector3.one*scale;return t;}
 public void SetVisible(bool visible){
  // The saved Stage root can be disabled in Scene; enabling its children alone is not enough.
  if(gameObject.activeSelf!=visible)gameObject.SetActive(visible);
  if(lobby.standaloneScene){if(view){view.enabled=visible;var atmosphere=view.GetComponent<PulseAtmosphere>();if(atmosphere)atmosphere.reducedMotion=reducedMotion;var bloom=view.GetComponent<PulseBloom>();if(bloom)bloom.enabled=false;}return;}
  foreach(Transform child in transform){bool show=visible;if(child==bridgeA||child==bridgeB)show&=lobby.LayoutHeight>=1000;if(child.gameObject.activeSelf!=show)child.gameObject.SetActive(show);}
  if(view){view.enabled=visible;view.cullingMask=1<<28;PulseAtmosphere.Ensure(view).reducedMotion=reducedMotion;}
 }
 public void ResizeIfNeeded(){if(size.x!=Screen.width||size.y!=Screen.height)Frame();}
 public void Frame(){if(!view||!lobby.safeRoot)return;size=new Vector2(MobileViewport.Width,MobileViewport.Height);view.aspect=MobileViewport.Width/(float)Mathf.Max(1,MobileViewport.Height);float h=2*Mathf.Tan(view.fieldOfView*Mathf.Deg2Rad*.5f)*80;
  var background=backdrop.GetComponent<Renderer>().sharedMaterial;var texture=background.mainTexture;float imageAspect=texture?texture.width/(float)texture.height:2f/3;
  if(background.shader.name=="Pulse/Clouds"){background.SetFloat("_UseBackdrop",1);background.SetFloat("_ScreenAspect",imageAspect);}
  float fillWidth=Mathf.Max(h*view.aspect,h*imageAspect);backdrop.localScale=new Vector3(fillWidth*1.045f,fillWidth/imageAspect*1.045f,1);backdrop.localPosition=new Vector3(0,0,80);
  if(lobby.standaloneScene){var authored=GetComponent<AuthoredMenuArt>();if(authored)authored.Frame(view);if(keepPaintedLandingUnderHero&&texture&&texture.name=="MenuCastleVista")AlignPaintedLanding();backdrop.localScale*=backgroundZoom;backdrop.localPosition+=new Vector3(backgroundOffset.x,backgroundOffset.y,0);return;}
  float width=lobby.LayoutWidth,height=lobby.LayoutHeight;
  float heroHeight=Mathf.Min(Mathf.Clamp(height*.41f,315,475),height-555);float heroY=height-237-heroHeight*.5f;
  Fit(heroGroup,width*.4f,heroY,width*.71f,heroHeight);
  float portalHeight=Mathf.Clamp(height*.235f,200,275);float portalY=Mathf.Max(310+portalHeight*.5f,Mathf.Lerp(382,414,Mathf.InverseLerp(840,1170,height)));
  Fit(portalGroup,width*.64f,portalY,width*.43f,portalHeight);
  Fit(bridgeA,width*.69f,portalY+portalHeight*.6f,90,80);Fit(bridgeB,width*.59f,portalY+portalHeight*.86f,75,65);
 }
 void AlignPaintedLanding(){
  if(!backdrop||!spirit||!view)return;
  var authored=GetComponent<AuthoredMenuArt>();
  var heroBounds=authored&&authored.framingGeometry!=null&&authored.framingGeometry.Length>0?ScreenComposition.Bounds(authored.artwork,view,authored.FrameGeometry):ScreenComposition.Bounds(spirit,view,ScreenComposition.Geometry(spirit));
  var desired=new Vector2(heroBounds.center.x,heroBounds.yMin);
  Vector2 Landing(){return view.WorldToScreenPoint(backdrop.TransformPoint(new Vector3(paintedLandingUv.x-.5f,paintedLandingUv.y-.5f,0)));}
  var delta=desired-Landing();
  // Add only the overscan needed for the offset, so no phone can reveal an empty edge.
  float required=1.02f+2*Mathf.Max(Mathf.Abs(delta.x)/Mathf.Max(1,MobileViewport.Width),Mathf.Abs(delta.y)/Mathf.Max(1,MobileViewport.Height));
  if(required>1.045f){float factor=Mathf.Clamp(required/1.045f,1,1.16f);backdrop.localScale=new Vector3(backdrop.localScale.x*factor,backdrop.localScale.y*factor,1);delta=desired-Landing();}
  float unitsPerPixel=(2*Mathf.Tan(view.fieldOfView*Mathf.Deg2Rad*.5f)*80)/Mathf.Max(1,view.pixelHeight);
  backdrop.localPosition=new Vector3(delta.x*unitsPerPixel,delta.y*unitsPerPixel,80);
 }
 void Fit(Transform root,float x,float y,float width,float height){
  if(!neutralGeometry.TryGetValue(root,out var points)){points=ScreenComposition.Geometry(root);neutralGeometry[root]=points;}
  var bottomLeft=lobby.LayoutPoint(x-width*.5f,y+height*.5f);var topRight=lobby.LayoutPoint(x+width*.5f,y-height*.5f);
  ScreenComposition.Fit(root,view,points,Rect.MinMaxRect(bottomLeft.x,bottomLeft.y,topRight.x,topRight.y));
 }
 public Rect ModelBounds(Transform root)=>ScreenComposition.Bounds(root,view,neutralGeometry.TryGetValue(root,out var p)?p:ScreenComposition.Geometry(root));
 public void ApplySkin(int index){if(spirit){SkinAppearance.Apply(spirit,false,index);spirit.GetComponent<SkinAppearance>().SetMotion(!reducedMotion);}}
 public void ApplyPortalSkin(int index){var portal=portalGroup?portalGroup.GetComponentsInChildren<ReferenceMotion>(true).FirstOrDefault(m=>m.modelId=="Portal"):null;if(portal)SkinAppearance.Apply(portal.transform,true,index);}
 public void Celebrate(){if(!spirit||reducedMotion)return;spirit.GetComponent<ReferenceMotion>().MenuReact(2);nextIdleGesture=clock+4.5f;}
 public string Greet(){
  if(!spirit)return "Искра рядом";
  int kind=reactionIndex++%3;if(!reducedMotion)spirit.GetComponent<ReferenceMotion>().MenuReact(kind);nextIdleGesture=clock+4.5f;
  return kind==0?"Искра машет тебе рукой!":kind==1?"Искра радуется встрече!":"Искра зовёт в приключение!";
 }
 void Update(){if(!lobby.IsOpen)return;clock+=Time.unscaledDeltaTime;if(cachedMotions==null)cachedMotions=GetComponentsInChildren<ReferenceMotion>(true);foreach(var m in cachedMotions)if(m&&m.enabled==reducedMotion)m.enabled=!reducedMotion;if(!reducedMotion&&clock>=nextIdleGesture&&spirit){spirit.GetComponent<ReferenceMotion>().MenuReact(reactionIndex++%2);nextIdleGesture=clock+Random.Range(4.5f,7.5f);}bool paintedPlatform=backdrop&&backdrop.GetComponent<Renderer>().sharedMaterial.shader.name=="Pulse/MenuBackdrop";if(paintedPlatform)backdrop.localPosition=new Vector3(backdrop.localPosition.x,backdrop.localPosition.y,80);else if(!reducedMotion){var p=new Vector2(Input.mousePosition.x/Mathf.Max(1,Screen.width)-.5f,Input.mousePosition.y/Mathf.Max(1,Screen.height)-.5f);backdrop.localPosition=new Vector3(Mathf.Sin(clock*.17f)*.2f+p.x*.35f,Mathf.Cos(clock*.14f)*.15f+p.y*.25f,80);}}
 void OnDestroy(){if(skin)Destroy(skin);if(portalSkin)Destroy(portalSkin);}
}

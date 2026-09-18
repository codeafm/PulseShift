using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Self-contained, resolution-independent animated startup/loading screen.</summary>
[ExecuteAlways, DefaultExecutionOrder(-1000)]
public sealed class PulseSplashScreen : MonoBehaviour {
 const string Destination="PULSESHIFT_Menu";
 const float MinimumShowTime=4.25f;
 CanvasGroup root;
 RectTransform logo,core,ringA,ringB,flare,progressFill;
 Text status,percent;
 RectTransform[] motes;
 Vector2[] moteOrigin;
 float shownProgress;
 bool fadingOut;
 float animationStarted;
 RectTransform loadingGlint;
 RectTransform safeLayout,breathingLight;
 bool referenceLayout;

 const string PreviewName="EDITOR PREVIEW • generated";
 void Awake(){
  if(!Application.isPlaying){EnsureEditorPreview();return;}
  RemoveEditorPreview();Application.targetFrameRate=60;
  animationStarted=Time.unscaledTime;
  var cameraObject=new GameObject("Splash clear camera",typeof(Camera));
  var camera=cameraObject.GetComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.008f,.018f,.055f);camera.cullingMask=0;
  // A Screen Space Overlay canvas must be a scene root. Parenting it to the
  // controller leaves it at the default 100x100 RectTransform on mobile.
  var runtime=new GameObject("Runtime splash UI",typeof(RectTransform));
  BuildInterface(runtime.transform);
  StartCoroutine(Load());
 }

 void EnsureEditorPreview(){
  if(GameObject.Find(PreviewName))return;
  // Keep the preview canvas at scene root too, so Device Simulator gets the
  // real full-screen rect instead of a small blue panel over the menu.
  var preview=new GameObject(PreviewName,typeof(RectTransform));preview.hideFlags=HideFlags.DontSaveInBuild|HideFlags.DontSaveInEditor;
  BuildInterface(preview.transform);root.alpha=1;UpdateProgress(.64f);status.text="СОЕДИНЯЕМ ОСТРОВА";
 }
 void ClearVisualReferences(){
  motes=null;moteOrigin=null;root=null;
  logo=core=ringA=ringB=flare=progressFill=null;
  loadingGlint=safeLayout=breathingLight=null;status=percent=null;
  referenceLayout=false;
 }
 void RemoveEditorPreview(){
  // Clear managed references before Unity destroys their native objects.
  ClearVisualReferences();
  var preview=GameObject.Find(PreviewName);if(preview)DestroyImmediate(preview);
 }
 void OnDisable(){if(!Application.isPlaying)RemoveEditorPreview();}

 void BuildInterface(Transform host){
  ClearVisualReferences();
  var canvas=host.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=30000;
  var scaler=host.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1080,1920);scaler.matchWidthOrHeight=.5f;
  host.gameObject.AddComponent<GraphicRaycaster>();root=host.gameObject.AddComponent<CanvasGroup>();root.alpha=0;
  var artwork=Resources.Load<Texture2D>("Splash/PulseFullscreen");
  if(artwork){BuildReference(host,artwork);return;}

  var backdrop=Node("Deep space",host,Vector2.zero,Vector2.one);backdrop.gameObject.AddComponent<SplashBackdrop>();
  var grid=Node("Energy grid",host,Vector2.zero,Vector2.one);grid.gameObject.AddComponent<SplashGrid>().color=new Color(.08f,.58f,1f,.12f);
  flare=AuraNode("Aura",host,new Color(.05f,.58f,1f,.34f));Anchor(flare,new Vector2(.5f,.58f),new Vector2(720,720));flare.gameObject.AddComponent<CanvasGroup>();

  motes=new RectTransform[18];moteOrigin=new Vector2[motes.Length];
  for(int i=0;i<motes.Length;i++){
   float x=Mathf.Repeat(i*.618034f+.11f,1f),y=Mathf.Repeat(i*.381966f+.19f,1f);
   motes[i]=ImageNode("Signal "+(i+1),host,i%3==0?new Color(.88f,.25f,1f,.7f):new Color(.08f,.78f,1f,.65f));
   motes[i].anchorMin=motes[i].anchorMax=new Vector2(x,y);motes[i].sizeDelta=Vector2.one*(i%4==0?7:4);motes[i].localRotation=Quaternion.Euler(0,0,45);moteOrigin[i]=new Vector2(0,0);
  }

  var brand=Node("Brand",host,new Vector2(.08f,.27f),new Vector2(.92f,.77f));
  logo=brand;
  ringB=RingNode("Outer orbit",brand,new Color(.12f,.7f,1f,.5f),5,11);Anchor(ringB,new Vector2(.5f,.6f),new Vector2(400,400));
  ringA=RingNode("Inner orbit",brand,new Color(.65f,.18f,1f,.48f),4,7);Anchor(ringA,new Vector2(.5f,.6f),new Vector2(310,310));
  core=ImageNode("Pulse core",brand,new Color(.12f,.83f,1f,.92f));Anchor(core,new Vector2(.5f,.6f),new Vector2(78,78));core.localRotation=Quaternion.Euler(0,0,45);
  var core2=ImageNode("Core light",core,new Color(.84f,.98f,1f,1));Anchor(core2,new Vector2(.5f,.5f),new Vector2(26,26));core2.localRotation=Quaternion.Euler(0,0,45);

  var title=TextNode("Title",brand,"PULSE<color=#35DFFF>SHIFT</color>",86,FontStyle.Bold,new Color(.96f,.98f,1f));
  SetRect(title.rectTransform,new Vector2(0,.13f),new Vector2(1,.36f));title.alignment=TextAnchor.MiddleCenter;
  var tracking=TextNode("Tagline",brand,"СИНХРОНИЗАЦИЯ МИРОВ",22,FontStyle.Bold,new Color(.43f,.72f,.92f));
  SetRect(tracking.rectTransform,new Vector2(0,.04f),new Vector2(1,.16f));tracking.alignment=TextAnchor.MiddleCenter;

  var loading=Node("Loading",host,new Vector2(.13f,.075f),new Vector2(.87f,.25f));
  status=TextNode("Status",loading,"ИНИЦИАЛИЗАЦИЯ",21,FontStyle.Bold,new Color(.65f,.79f,.9f));SetRect(status.rectTransform,new Vector2(0,.58f),new Vector2(.8f,1));status.alignment=TextAnchor.MiddleLeft;
  percent=TextNode("Percent",loading,"00%",21,FontStyle.Bold,new Color(.3f,.86f,1f));SetRect(percent.rectTransform,new Vector2(.8f,.58f),new Vector2(1,1));percent.alignment=TextAnchor.MiddleRight;
  var rail=ImageNode("Progress rail",loading,new Color(.12f,.22f,.34f,.8f));SetRect(rail,new Vector2(0,.40f),new Vector2(1,.52f));
  progressFill=ImageNode("Progress energy",rail,new Color(.12f,.78f,1f,1));progressFill.anchorMin=Vector2.zero;progressFill.anchorMax=new Vector2(0,1);progressFill.offsetMin=new Vector2(3,3);progressFill.offsetMax=new Vector2(-3,-3);progressFill.pivot=new Vector2(0,.5f);
  var hint=TextNode("Hint",loading,"НАСТРАИВАЕМ ПОРТАЛЫ И ЭНЕРГЕТИЧЕСКИЕ СВЯЗИ",15,FontStyle.Normal,new Color(.35f,.51f,.66f));SetRect(hint.rectTransform,new Vector2(0,0),new Vector2(1,.3f));hint.alignment=TextAnchor.MiddleCenter;
 }

 IEnumerator Load(){
  // Render the first frame before starting any expensive menu deserialization.
  yield return null;
  yield return new WaitForEndOfFrame();
  float started=Time.realtimeSinceStartup;
  if(!Application.CanStreamedLevelBeLoaded(Destination)){status.text="МЕНЮ НЕ ДОБАВЛЕНО В СБОРКУ";Debug.LogError("PULSE_SPLASH: missing menu scene");yield break;}
  Debug.Log("PULSE_SPLASH_VISIBLE");
  var op=SceneManager.LoadSceneAsync(Destination,LoadSceneMode.Single);
  if(op==null){status.text="СЦЕНА МЕНЮ НЕ НАЙДЕНА";yield break;}
  op.allowSceneActivation=false;
  while(true){
   float elapsed=Time.realtimeSinceStartup-started;
   float actual=Mathf.Clamp01(op.progress/.9f);
   float presentation=Mathf.Clamp01(elapsed/(MinimumShowTime-.35f));
   shownProgress=Mathf.MoveTowards(shownProgress,Mathf.Min(actual,presentation),Time.unscaledDeltaTime*.55f);
   UpdateProgress(shownProgress);
   if(actual>=1f&&elapsed>=MinimumShowTime)break;
   yield return null;
  }
  shownProgress=1;UpdateProgress(1);status.text="ГОТОВО";yield return new WaitForSecondsRealtime(.18f);
  fadingOut=true;
  for(float t=0;t<.38f;t+=Time.unscaledDeltaTime){root.alpha=1-t/.38f;yield return null;}
  op.allowSceneActivation=true;
 }

 void Update(){
  // The root can disappear first during scene unload or editor preview teardown.
  if(!root)return;
  if(root&&!fadingOut)root.alpha=Mathf.MoveTowards(root.alpha,1,Time.unscaledDeltaTime*2.4f);
  if(safeLayout&&Screen.width>0&&Screen.height>0){var area=Screen.safeArea;safeLayout.anchorMin=new Vector2(area.xMin/Screen.width,area.yMin/Screen.height);safeLayout.anchorMax=new Vector2(area.xMax/Screen.width,area.yMax/Screen.height);}
  if(breathingLight)breathingLight.localScale=Vector3.one*(1+.09f*Mathf.Sin(Time.unscaledTime*1.5f));
  if(referenceLayout&&motes!=null)for(int i=0;i<motes.Length;i++){var r=motes[i];if(!r)continue;float clock=Time.unscaledTime;r.anchoredPosition=new Vector2(Mathf.Sin(clock*.3f+i)*16,Mathf.Repeat(clock*12+i*37,220)-110);r.localScale=Vector3.one*(.5f+.5f*Mathf.Sin(clock*1.2f+i));}
  if(loadingGlint){float x=.5f+.48f*Mathf.Sin(Time.unscaledTime*2);loadingGlint.anchorMin=loadingGlint.anchorMax=new Vector2(x,.5f);}
  if(!core||!logo||!ringA||!ringB||!flare||motes==null||moteOrigin==null)return;
  float t=Time.unscaledTime-animationStarted;
  if(!fadingOut&&root.alpha<1)root.alpha=Mathf.MoveTowards(root.alpha,1,Time.unscaledDeltaTime*2.4f);
  float entrance=Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.9f));logo.localScale=Vector3.one*(.88f+.12f*entrance);
  core.localScale=Vector3.one*(1+Mathf.Sin(t*4.4f)*.09f);core.Rotate(0,0,42*Time.unscaledDeltaTime);
  ringA.Rotate(0,0,-17*Time.unscaledDeltaTime);ringB.Rotate(0,0,9*Time.unscaledDeltaTime);
  flare.localScale=Vector3.one*(.94f+Mathf.Sin(t*1.7f)*.06f);
  for(int i=0;i<motes.Length;i++){
   if(!motes[i]||i>=moteOrigin.Length)continue;
   float phase=t*(.28f+(i%4)*.04f)+i*.7f;
   motes[i].anchoredPosition=moteOrigin[i]+new Vector2(Mathf.Sin(phase)*12,Mathf.Cos(phase*.73f)*28);
   var particle=motes[i].GetComponent<Image>();if(!particle)continue;
   var c=particle.color;c.a=.2f+.55f*(.5f+.5f*Mathf.Sin(t*1.6f+i));particle.color=c;
  }
 }

 void BuildReference(Transform host,Texture2D texture){
  referenceLayout=true;
  var background=ImageNode("Opaque midnight",host,new Color(.002f,.005f,.015f,1));SetRect(background,Vector2.zero,Vector2.one);
  // Exactly one artwork layer: no enlarged duplicate or repeated logo.
  var composition=Node("Single fullscreen artwork",host,Vector2.zero,Vector2.one);
  var fit=composition.gameObject.AddComponent<AspectRatioFitter>();fit.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;fit.aspectRatio=(float)texture.width/texture.height;
  var art=composition.gameObject.AddComponent<RawImage>();art.texture=texture;art.uvRect=new Rect(0,0,1,1);art.raycastTarget=false;
  breathingLight=AuraNode("Breathing crystal light",composition,new Color(.1f,.65f,1,.16f));Anchor(breathingLight,new Vector2(.515f,.66f),new Vector2(210,210));
  motes=new RectTransform[14];for(int i=0;i<motes.Length;i++){motes[i]=AuraNode("Floating energy "+i,composition,new Color(.3f,.8f,1,.7f));Anchor(motes[i],new Vector2(.13f+Mathf.Repeat(i*.173f,.74f),.22f+Mathf.Repeat(i*.113f,.55f)),Vector2.one*(8+i%3*3));}
  safeLayout=Node("Safe area",host,Vector2.zero,Vector2.one);
  var shade=Node("Lower contrast veil",host,Vector2.zero,new Vector2(1,.30f));shade.gameObject.AddComponent<SplashEdgeFade>();shade.SetSiblingIndex(safeLayout.GetSiblingIndex());
  var panel=Node("Loading console",safeLayout,new Vector2(.12f,.12f),new Vector2(.88f,.235f));
  status=TextNode("Loading caption",panel,"ЗАГРУЗКА",28,FontStyle.Normal,new Color(.75f,.9f,1));SetRect(status.rectTransform,new Vector2(.025f,.62f),new Vector2(.78f,1));status.alignment=TextAnchor.MiddleLeft;
  percent=TextNode("Real progress",panel,"0%",32,FontStyle.Bold,new Color(.55f,.95f,1));SetRect(percent.rectTransform,new Vector2(.78f,.62f),new Vector2(.975f,1));percent.alignment=TextAnchor.MiddleRight;
  var border=BevelNode("Luminous chamfer frame",panel,new Color(.08f,.65f,.86f,1));SetRect(border,new Vector2(0,.27f),new Vector2(1,.54f));
  var rail=BevelNode("Recessed midnight track",border,new Color(.006f,.022f,.052f,1));SetRect(rail,Vector2.zero,Vector2.one);rail.offsetMin=new Vector2(3,3);rail.offsetMax=new Vector2(-3,-3);
  var inset=Node("Energy inset",rail,Vector2.zero,Vector2.one);inset.offsetMin=new Vector2(5,5);inset.offsetMax=new Vector2(-5,-5);
  progressFill=BevelNode("Cyan energy fill",inset,new Color(.015f,.72f,1,1));SetRect(progressFill,Vector2.zero,new Vector2(0,1));
  var highlight=ImageNode("Fill highlight",progressFill,new Color(.5f,1,1,.65f));SetRect(highlight,new Vector2(.025f,.72f),new Vector2(.975f,.87f));
  loadingGlint=AuraNode("Energy shimmer",progressFill,new Color(.75f,1,1,.8f));Anchor(loadingGlint,new Vector2(.5f,.5f),new Vector2(32,32));
  for(int i=0;i<11;i++){var tick=ImageNode("Calibration "+i,panel,new Color(.24f,.65f,.85f,i%5==0?.65f:.24f));Anchor(tick,new Vector2(.035f+i*.093f,.14f),new Vector2(2,i%5==0?9:5));}
  var credit=TextNode("Studio signature",safeLayout,"C O D E A F M   G A M I N G",25,FontStyle.Normal,new Color(.7f,.82f,.92f));SetRect(credit.rectTransform,new Vector2(.1f,.048f),new Vector2(.9f,.083f));credit.alignment=TextAnchor.MiddleCenter;
 }

 void UpdateProgress(float p){progressFill.anchorMax=new Vector2(p,1);percent.text=Mathf.RoundToInt(p*100).ToString("00")+"%";status.text=referenceLayout?"ЗАГРУЗКА":p<.3f?"ИНИЦИАЛИЗАЦИЯ":p<.72f?"СОЕДИНЯЕМ ОСТРОВА":p<.96f?"СТАБИЛИЗИРУЕМ ПОРТАЛ":"ПОЧТИ ГОТОВО";}
 static RectTransform Node(string name,Transform parent,Vector2 min,Vector2 max){var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer));var r=(RectTransform)g.transform;r.SetParent(parent,false);SetRect(r,min,max);return r;}
 static RectTransform ImageNode(string name,Transform parent,Color color){var r=Node(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f));r.gameObject.AddComponent<Image>().color=color;return r;}
 static RectTransform BevelNode(string name,Transform parent,Color color){var r=Node(name,parent,Vector2.zero,Vector2.one);r.gameObject.AddComponent<SplashBevel>().color=color;return r;}
 static RectTransform AuraNode(string name,Transform parent,Color color){var r=Node(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f));r.gameObject.AddComponent<SplashAura>().color=color;return r;}
 static RectTransform RingNode(string name,Transform parent,Color color,float width,int gaps){var r=Node(name,parent,new Vector2(.5f,.5f),new Vector2(.5f,.5f));var ring=r.gameObject.AddComponent<SplashRing>();ring.color=color;ring.width=width;ring.gaps=gaps;return r;}
 static Text TextNode(string name,Transform parent,string value,int size,FontStyle style,Color color){var r=Node(name,parent,Vector2.zero,Vector2.one);var t=r.gameObject.AddComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.fontStyle=style;t.color=color;t.supportRichText=true;t.resizeTextForBestFit=true;t.resizeTextMinSize=Mathf.Max(10,size/2);t.resizeTextMaxSize=size;return t;}
 static void Anchor(RectTransform r,Vector2 anchor,Vector2 size){r.anchorMin=r.anchorMax=anchor;r.sizeDelta=size;r.anchoredPosition=Vector2.zero;}
 static void SetRect(RectTransform r,Vector2 min,Vector2 max){r.anchorMin=min;r.anchorMax=max;r.offsetMin=r.offsetMax=Vector2.zero;}
}

public sealed class SplashBevel:Graphic {
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();var r=rectTransform.rect;if(r.width<=0||r.height<=0)return;float cut=Mathf.Min(r.height*.5f,r.width*.15f);
  Vector2[] points={new Vector2(r.xMin,r.center.y),new Vector2(r.xMin+cut,r.yMax),new Vector2(r.xMax-cut,r.yMax),new Vector2(r.xMax,r.center.y),new Vector2(r.xMax-cut,r.yMin),new Vector2(r.xMin+cut,r.yMin)};
  v.AddVert(r.center,color,Vector2.zero);foreach(var p in points)v.AddVert(p,color,Vector2.zero);for(int i=0;i<6;i++)v.AddTriangle(0,i+1,(i+1)%6+1);
 }
}

public sealed class SplashEdgeFade:Graphic {
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();var r=rectTransform.rect;var dark=new Color(.002f,.005f,.015f,1);var clear=dark;clear.a=0;
  v.AddVert(new Vector3(r.xMin,r.yMin),dark,Vector2.zero);v.AddVert(new Vector3(r.xMin,r.yMax),clear,Vector2.up);v.AddVert(new Vector3(r.xMax,r.yMax),clear,Vector2.one);v.AddVert(new Vector3(r.xMax,r.yMin),dark,Vector2.right);v.AddTriangle(0,1,2);v.AddTriangle(0,2,3);
 }
}

public sealed class SplashBackdrop:Graphic {
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();Add(vh,rectTransform.rect,new Color(.008f,.018f,.055f),new Color(.025f,.055f,.14f));}
 static void Add(VertexHelper v,Rect r,Color bottom,Color top){int i=v.currentVertCount;v.AddVert(new Vector3(r.xMin,r.yMin),bottom,Vector2.zero);v.AddVert(new Vector3(r.xMin,r.yMax),top,Vector2.up);v.AddVert(new Vector3(r.xMax,r.yMax),top,Vector2.one);v.AddVert(new Vector3(r.xMax,r.yMin),bottom,Vector2.right);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
}

/// <summary>Soft circular glow; unlike a plain Image it has no rectangular edge.</summary>
public sealed class SplashAura:Graphic {
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();const int steps=64;float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;Color edge=color;edge.a=0;
  vh.AddVert(Vector3.zero,color,new Vector2(.5f,.5f));
  for(int i=0;i<=steps;i++){float a=i*Mathf.PI*2/steps;vh.AddVert(new Vector3(Mathf.Cos(a)*radius,Mathf.Sin(a)*radius),edge,new Vector2(.5f+Mathf.Cos(a)*.5f,.5f+Mathf.Sin(a)*.5f));}
  for(int i=0;i<steps;i++)vh.AddTriangle(0,i+1,i+2);
 }
}

public sealed class SplashRing:Graphic {
 public float width=5;public int gaps=9;
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();const int steps=96;float outer=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f,inner=outer-width;
  for(int s=0;s<steps;s++){if((s/(steps/gaps))%3==2)continue;float a=s*Mathf.PI*2/steps,b=(s+1)*Mathf.PI*2/steps;int i=vh.currentVertCount;Vector2 ca=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),cb=new Vector2(Mathf.Cos(b),Mathf.Sin(b));vh.AddVert(ca*inner,color,Vector2.zero);vh.AddVert(ca*outer,color,Vector2.up);vh.AddVert(cb*outer,color,Vector2.one);vh.AddVert(cb*inner,color,Vector2.right);vh.AddTriangle(i,i+1,i+2);vh.AddTriangle(i,i+2,i+3);}
 }
}

public sealed class SplashGrid:Graphic {
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;float spacing=Mathf.Max(42,r.width/12f),thick=1.2f;for(float x=r.xMin;x<r.xMax;x+=spacing)Quad(vh,new Rect(x,r.yMin,thick,r.height));for(float y=r.yMin;y<r.yMax;y+=spacing)Quad(vh,new Rect(r.xMin,y,r.width,thick));}
 void Quad(VertexHelper v,Rect r){int i=v.currentVertCount;v.AddVert(new Vector3(r.xMin,r.yMin),color,Vector2.zero);v.AddVert(new Vector3(r.xMin,r.yMax),color,Vector2.up);v.AddVert(new Vector3(r.xMax,r.yMax),color,Vector2.one);v.AddVert(new Vector3(r.xMax,r.yMin),color,Vector2.right);v.AddTriangle(i,i+1,i+2);v.AddTriangle(i,i+2,i+3);}
}

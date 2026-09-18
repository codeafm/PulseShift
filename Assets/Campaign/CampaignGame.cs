using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PulseCampaign;
using UnityEngine.EventSystems;

public enum CampaignCameraMode { SceneTransform, AutomaticFit }

public partial class CampaignGame:MonoBehaviour {
 [Min(1)] public int previewLevel=1;
 [SerializeField,HideInInspector] int generatedPreviewLevel=1;
 [SerializeField,HideInInspector] int generatedCampaignVersion=1;
 public bool fitLevelToScreen=true;
 public bool resumeProgress=true;
 [Range(1,1.7f)] public float actorReadabilityScale=1.35f;
 public bool startInLobby=true;
 [Tooltip("Main menu lives in PULSESHIFT_Menu; do not create an embedded lobby.")]
 public bool useSeparateMenuScene;
 public PulseLobby lobby;
 public LobbyMode ActiveMode=>useSeparateMenuScene?PulseSceneFlow.Mode:lobby?lobby.Mode:LobbyMode.Campaign;
 public int ActiveEndlessCount=>useSeparateMenuScene?PulseSceneFlow.EndlessCount:lobby?lobby.EndlessCount:0;
 public int ActiveModeTarget=>useSeparateMenuScene?PulseSceneFlow.ModeTarget:lobby?lobby.ModeTarget:0;
 PulseProfile UserProfile=>useSeparateMenuScene?PulseSceneFlow.Profile:lobby?lobby.Profile:null;
 Material equippedSkin,equippedPortal;CampaignHUD[] presentationHuds;
 [SerializeField,HideInInspector] public CampaignHUD gameplayHUD;
 [Tooltip("SceneTransform keeps the camera position and rotation you set in Scene, including when levels change.")]
 public CampaignCameraMode cameraMode=CampaignCameraMode.SceneTransform;
 public TextAsset campaignFile;
 public Transform editableBoard;
 public Camera gameCamera;
 Campaign campaign;Level level;State state;GameObject liveBoard,hero;CampaignIsland[] islands;GameObject[] crystals,guards;
 readonly Stack<(State state,int turns)> history=new();
 int levelIndex,turns,unlocked=1,menuPage;bool busy,paused,levelMenu,showLesson,dashSelected,testing,hintUsed;int hintVersion;
 string message="";AudioSource sound,music;AudioClip jumpClip,collectClip,deadClip,gameOverClip,bonusClip,gameMusicClip,passageClip;Vector2 lastSize;Rect lastSafeArea;
 float resultDelay;
 Coroutine activeTurn;
 public Level CurrentLevel=>level??Data.levels[Mathf.Clamp(previewLevel-1,0,Data.levels.Length-1)];
 public State CurrentState=>state;
 public bool Busy=>busy;
 public bool Paused=>paused;
 public bool MenuOpen=>levelMenu;
 public bool LessonOpen=>showLesson;
 public bool LobbyOpen=>lobby&&lobby.IsOpen;
 public bool MotionPaused=>paused||levelMenu||showLesson||LobbyOpen||PulseAds.Fullscreen;
 public bool Victorious=>level!=null&&Rules.Won(level,state);
 public bool ResultVisible=>resultDelay<=0;
 public bool CanPlay=>level!=null&&!busy&&!MotionPaused&&!state.dead&&!Victorious;
 public bool CanUndo=>!busy&&history.Count>0;
 public bool DashSelected=>dashSelected;
 public int Turns=>turns;
 public Transform ActiveBoardForVerification=>liveBoard?liveBoard.transform:null;
 public int Unlocked=>unlocked;
 public int MenuPage=>menuPage;
 public int GemCount=>CountGems();
 public int EarnedStars=>turns<=CurrentLevel.optimalTurns&&!hintUsed?3:turns<=CurrentLevel.optimalTurns+3?2:1;
 public string Message=>message;
 string SaveKey(string suffix)=>(Data.formatVersion>=2?"PulseCampaign.Variety2.":"PulseCampaign.")+suffix;
 public int SavedStars(int id)=>PlayerPrefs.GetInt(SaveKey("Stars."+id),0);
 public void RequestUI(CampaignUICommand command,int value=0){
  if(PulseAds.Fullscreen)return;
  if(useSeparateMenuScene&&PulseSceneFlow.Loading)return;
  if(LobbyOpen&&command!=CampaignUICommand.Home)return;
  if(TryResultInterstitial(command,value))return;
  switch(command){
   case CampaignUICommand.Home:if(!busy||paused){if(busy){if(activeTurn!=null)StopCoroutine(activeTurn);activeTurn=null;busy=false;if(history.Count>0)history.Pop();foreach(var motion in liveBoard.GetComponentsInChildren<ReferenceMotion>(true))motion.ResetPose();SyncVisuals(true);}paused=false;if(useSeparateMenuScene){PulseSceneFlow.Home();break;}lobby=PulseLobby.Ensure(this);lobby.Open();}break;
   case CampaignUICommand.Pulse:if(CanPlay)Act(new Command(ActionKind.Pulse));break;
   case CampaignUICommand.Wait:if(CanPlay)Act(new Command(ActionKind.Wait));break;
   case CampaignUICommand.Freeze:if(CanPlay)Act(new Command(ActionKind.Freeze));break;
   case CampaignUICommand.Dash:if(CanPlay&&state.dashes>0){dashSelected=!dashSelected;message=dashSelected?"Выбери подсвеченный остров через один":"Рывок отменён";SyncVisuals(false);}break;
   case CampaignUICommand.Undo:if(CanUndo&&!levelMenu&&!showLesson)Undo();break;
   case CampaignUICommand.Hint:if(CanPlay)Hint();break;
   case CampaignUICommand.Pause:paused=!paused;Tone(420);break;
   case CampaignUICommand.Levels:if(!busy){levelMenu=true;menuPage=levelIndex/20;Tone(580);}break;
   case CampaignUICommand.CloseLevels:levelMenu=false;break;
   case CampaignUICommand.PreviousPage:menuPage=Mathf.Max(0,menuPage-1);break;
   case CampaignUICommand.NextPage:menuPage=Mathf.Min((Data.levels.Length-1)/20,menuPage+1);break;
   case CampaignUICommand.SelectLevel:if(!busy&&value>=0&&value<Data.levels.Length&&value<unlocked)LoadLevel(value);break;
   case CampaignUICommand.Restart:if(!busy||paused){if(activeTurn!=null){StopCoroutine(activeTurn);activeTurn=null;}LoadLevel(levelIndex);}break;
   case CampaignUICommand.Primary:if(busy&&!paused)break;if(showLesson)showLesson=false;else if(state.dead)Undo();else if(Victorious){if(ActiveMode!=LobbyMode.Campaign){if(useSeparateMenuScene)PulseSceneFlow.Advance(this);else lobby.AdvanceMode();}else if(levelIndex+1<Data.levels.Length)LoadLevel(levelIndex+1);else{levelMenu=true;menuPage=levelIndex/20;}}else paused=false;break;
  }
 }
 public Campaign Data {get{if(campaign==null){if(!campaignFile)campaignFile=Resources.Load<TextAsset>("Campaign/levels");campaign=JsonUtility.FromJson<Campaign>(campaignFile.text);}return campaign;}}
 public void BakePreview(){
  if(editableBoard){if(Application.isPlaying)Destroy(editableBoard.gameObject);else DestroyImmediate(editableBoard.gameObject);}
  previewLevel=Mathf.Clamp(previewLevel,1,Data.levels.Length);generatedPreviewLevel=previewLevel;generatedCampaignVersion=Data.formatVersion;level=Data.levels[previewLevel-1];editableBoard=CampaignVisuals.MakeBoard(level).transform;
  if(!gameCamera)CreatePresentation();EnsureCameraFeel();
  BindBoard(editableBoard.gameObject);state=Rules.Initial(level);SyncVisuals(true);if(cameraMode==CampaignCameraMode.AutomaticFit)FitCamera();else RefreshCameraBackdrop();FitCurrentBoard();CampaignHUD.Ensure(this);
 }
 void CreatePresentation(){
  gameCamera=new GameObject("Main Camera • campaign framing").AddComponent<Camera>();gameCamera.tag="MainCamera";gameCamera.fieldOfView=22;gameCamera.nearClipPlane=.1f;gameCamera.farClipPlane=180;gameCamera.clearFlags=CameraClearFlags.SolidColor;gameCamera.backgroundColor=new Color(.015f,.03f,.085f);gameCamera.allowHDR=true;gameCamera.gameObject.AddComponent<PulseBloom>();
  gameCamera.transform.SetPositionAndRotation(new Vector3(-.5f,22,-28.7f),Quaternion.Euler(38,1,0));
  var bg=GameObject.CreatePrimitive(PrimitiveType.Quad);bg.name="Cloud backdrop";if(Application.isPlaying)Destroy(bg.GetComponent<Collider>());else DestroyImmediate(bg.GetComponent<Collider>());bg.transform.SetParent(gameCamera.transform,false);bg.transform.localPosition=new Vector3(0,0,100);bg.GetComponent<Renderer>().sharedMaterial=new Material(Resources.Load<Shader>("PulseClouds"));
  RenderSettings.ambientLight=new Color(.28f,.32f,.46f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.055f,.085f,.19f);RenderSettings.fogDensity=.016f;
  var key=new GameObject("Moon key • editable").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.25f;key.color=new Color(.8f,.86f,1);key.transform.rotation=Quaternion.Euler(48,-35,0);key.shadows=LightShadows.Soft;
  var fill=new GameObject("Amber rim • editable").AddComponent<Light>();fill.type=LightType.Directional;fill.intensity=.6f;fill.color=new Color(1,.67f,.4f);fill.transform.rotation=Quaternion.Euler(30,145,0);CampaignVisuals.CreateScenery();
 }
 void Start(){
  MobilePerformance.Apply(useSeparateMenuScene?PulseSceneFlow.Profile:UserProfile,true);testing=Environment.GetCommandLineArgs().Contains("-campaignVerify")||Environment.GetCommandLineArgs().Contains("-lobbyVerify")||Environment.GetCommandLineArgs().Contains("-mobileVerify");
  testing|=useSeparateMenuScene&&PulseSceneFlow.Testing;
  sound=gameObject.AddComponent<AudioSource>();SetupGameAudio();if(!gameCamera)CreatePresentation();EnsureCameraFeel();if(!gameCamera.GetComponent<AudioListener>())gameCamera.gameObject.AddComponent<AudioListener>();
  unlocked=testing?Data.levels.Length:Mathf.Clamp(PlayerPrefs.GetInt(SaveKey("Unlocked"),1),1,Data.levels.Length);
  int start=resumeProgress&&previewLevel==1?PlayerPrefs.GetInt(SaveKey("Current"),1):previewLevel;
  LoadLevel(useSeparateMenuScene?PulseSceneFlow.TakeLevel(Mathf.Clamp(start-1,0,Data.levels.Length-1)):testing?0:Mathf.Clamp(start-1,0,Data.levels.Length-1));
  CampaignHUD.Ensure(this);
  if(useSeparateMenuScene){SetLobbyVisible(false);ApplyPlayerSkin();var settings=UserProfile;gameCamera.GetComponent<PulseCameraFeel>().reducedMotion=settings.reducedMotion;PulseAtmosphere.Ensure(gameCamera).reducedMotion=settings.reducedMotion;return;}
  lobby=PulseLobby.Ensure(this);lobby.Initialize(testing);ApplyPlayerSkin();
  if(Environment.GetCommandLineArgs().Contains("-campaignVerify")){showLesson=false;StartCoroutine(VerifyRuntime());}
  else if(Environment.GetCommandLineArgs().Contains("-lobbyVerify")){showLesson=false;lobby.Open();StartCoroutine(lobby.VerifyLobby());}
  else if(Environment.GetCommandLineArgs().Contains("-mobileVerify")){showLesson=false;lobby.Open();StartCoroutine(lobby.VerifyMobile());}
  else if(startInLobby&&previewLevel==1)lobby.Open();
 }
 void BindBoard(GameObject board){
  liveBoard=board;islands=board.GetComponentsInChildren<CampaignIsland>(true).OrderBy(n=>n.index).ToArray();hero=board.transform.Find("Hero").gameObject;
  hero.transform.localScale=Vector3.one*actorReadabilityScale;
  crystals=new GameObject[3];for(int i=0;i<3;i++)crystals[i]=board.transform.Find("Crystal "+i).gameObject;
  guards=new GameObject[level.patrols.Length];for(int i=0;i<guards.Length;i++)guards[i]=board.transform.Find("Guard "+i).gameObject;
  foreach(var marker in islands){int gem=level.islands[marker.index].gem;if(gem>=0)crystals[gem].transform.position=marker.transform.position+Vector3.up*.18f*board.transform.lossyScale.y;}
  board.transform.Find("Exit portal").position=islands[level.goal].transform.position+Vector3.up*.18f*board.transform.lossyScale.y;
 }
 void LoadLevel(int index){
  ResetAdVictory();
  if(!testing)StopAllCoroutines();
  hintVersion++;if(liveBoard&&(!editableBoard||liveBoard!=editableBoard.gameObject)){liveBoard.SetActive(false);Destroy(liveBoard);}levelIndex=index;level=Data.levels[index];
  GameObject board;
  if(editableBoard&&previewLevel==level.id&&generatedPreviewLevel==level.id&&generatedCampaignVersion==Data.formatVersion&&editableBoard.GetComponentInChildren<ReferenceIdentity>(true)){board=Instantiate(editableBoard.gameObject);board.SetActive(true);}else board=CampaignVisuals.MakeBoard(level);
  if(editableBoard)editableBoard.gameObject.SetActive(false);BindBoard(board);
  state=Rules.Initial(level);turns=0;resultDelay=0;history.Clear();busy=paused=levelMenu=dashSelected=hintUsed=false;
  showLesson=!testing&&(level.id==1||level.id==5||level.id==15||level.id==25||level.id==56||(level.id-1)%10==0);message=level.challenge??level.lesson;
  SyncVisuals(true);if(cameraMode==CampaignCameraMode.AutomaticFit)FitCamera();else RefreshCameraBackdrop();
  gameCamera.ResetAspect();FitCurrentBoard();gameCamera.GetComponent<PulseCameraFeel>()?.ResetFeel();ApplyBackdropTheme();ApplyPlayerSkin();ApplyPortalSkin();
  if(!testing&&ActiveMode==LobbyMode.Campaign){PlayerPrefs.SetInt(SaveKey("Current"),level.id);PlayerPrefs.Save();}
  if(InterstitialEligible)PulseAds.Instance?.PrepareInterstitial(true);
 }
 Vector3 Position(int index)=>islands[index].transform.position+Vector3.up*.18f*liveBoard.transform.lossyScale.y;
 void SyncVisuals(bool placeActors){
  var predictedEnemies=Rules.PreviewEnemies(level,state);
  for(int i=0;i<islands.Length;i++){
   var marker=islands[i];var node=level.islands[i];bool gone=(state.collapsed&(1u<<i))!=0;CampaignVarietyArt.SyncSurface(marker.transform,node,state,gone);
   if(node.kind==TileKind.Phase)CampaignVisuals.Deck(marker.transform).localRotation=Quaternion.Euler(0,node.phase==state.phase?0:45,0);
   var signs=marker.transform.Find("Trap and route markers");var spikes=CampaignVisuals.Spikes(marker.transform);if(spikes)spikes.localScale=new Vector3(1,Rules.SpikesUp(node,state.tick)?1:.035f,1);
   var highlight=signs.Find("Move highlight");var line=highlight.GetComponent<LineRenderer>();line.widthMultiplier=dashSelected?.037f:.022f;var preview=Rules.Apply(level,state,new Command(dashSelected?ActionKind.Dash:ActionKind.Move,i),out _,out _);highlight.gameObject.SetActive(!gone&&i!=state.cell&&!state.dead&&!Victorious&&preview!=Result.Invalid);line.sharedMaterial=CampaignVisuals.Energy(preview==Result.Dead?"Dangerous landing":"Safe landing",preview==Result.Dead?new Color(1,.08f,.025f,.6f):new Color(.025f,.55f,1,.7f));
   var future=signs.Find("Guard next step");future.gameObject.SetActive(Rules.Threatened(level,predictedEnemies,i));
   CampaignVarietyArt.SyncTrap(marker.transform,node,state);
  }
  for(int g=0;g<3;g++)crystals[g].SetActive((state.gems&(1<<g))==0);
  if(placeActors){hero.transform.position=Position(state.cell);for(int i=0;i<guards.Length;i++)guards[i].transform.position=Position(Rules.EnemyCell(level,state,i));}
  CampaignVarietyArt.SyncEnemies(level,state,islands,guards);
 }
 public void FitCamera(){
  if(cameraMode!=CampaignCameraMode.AutomaticFit)return;
  if(islands==null||islands.Length==0)return;Vector3 center=Vector3.zero;foreach(var n in islands)center+=n.transform.position;center/=islands.Length;center.y+=.25f;
  var direction=new Vector3(.1f,.63f,-.78f).normalized;gameCamera.transform.rotation=Quaternion.LookRotation(-direction,Vector3.up);
  float aspect=Application.isPlaying?Mathf.Max(.2f,gameCamera.aspect):9f/16;gameCamera.aspect=aspect;
  // Fit actual artwork, then centre it in the free area BETWEEN the HUD and controls.
  // A zoom-only fit around the board origin left a large empty band above the portal.
  var points=new List<Vector3>();
  foreach(Transform child in liveBoard.transform){
   var renderers=child.GetComponentsInChildren<Renderer>(true).Where(r=>r is MeshRenderer||r is SkinnedMeshRenderer).ToArray();if(renderers.Length==0)continue;
   var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
   for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)points.Add(bounds.center+Vector3.Scale(bounds.extents,new Vector3(x,y,z)));
  }
  float distance=20;
  for(int step=0;step<32;step++){
   gameCamera.transform.position=center+direction*distance;
   Vector2 min=Vector2.one*100,max=Vector2.one*-100;
   foreach(var p in points){var v=gameCamera.WorldToViewportPoint(p);min=Vector2.Min(min,v);max=Vector2.Max(max,v);}
   float height=2*Mathf.Tan(gameCamera.fieldOfView*Mathf.Deg2Rad/2)*distance;
   var offset=(min+max)*.5f-new Vector2(.5f,.5175f);
   center+=gameCamera.transform.right*(offset.x*height*aspect)+gameCamera.transform.up*(offset.y*height);
   float fit=Mathf.Max((max.x-min.x)/.86f,(max.y-min.y)/.595f);
   distance*=Mathf.Lerp(1,Mathf.Clamp(fit,.7f,1.5f),.6f);
  }
  gameCamera.transform.position=center+direction*distance;
  RefreshCameraBackdrop();
  lastSize=new Vector2(Screen.width,Screen.height);
 }
 void FitCurrentBoard(){if(Data.formatVersion>=2&&fitLevelToScreen){CampaignFraming.Fit(liveBoard.transform,gameCamera);CampaignVarietyArt.SyncEnemies(level,state,islands,guards);if(Application.isPlaying)Physics.SyncTransforms();}}
 void EnsureCameraFeel(){if(!gameCamera)return;if(!gameCamera.GetComponent<PulseCameraFeel>())gameCamera.gameObject.AddComponent<PulseCameraFeel>();PulseAtmosphere.Ensure(gameCamera);}
 void ApplyBackdropTheme(){
  if(!Application.isPlaying||!gameCamera)return;var bg=gameCamera.transform.Find("Cloud backdrop");if(!bg)return;var m=bg.GetComponent<Renderer>().material;
  var colors=new[]{new Color(.10f,.18f,.34f),new Color(.075f,.24f,.20f),new Color(.27f,.16f,.09f),new Color(.19f,.11f,.30f),new Color(.09f,.23f,.33f)};
  m.SetColor("_MistTint",colors[level.biome%colors.Length]);
 }
 void RefreshCameraBackdrop(){
  if(!gameCamera)return;var bg=gameCamera.transform.Find("Cloud backdrop");if(!bg)return;
  float distance=Mathf.Abs(bg.localPosition.z);float h=gameCamera.orthographic?gameCamera.orthographicSize*2:2*Mathf.Tan(gameCamera.fieldOfView*Mathf.Deg2Rad/2)*distance;
  float aspect=Application.isPlaying?gameCamera.aspect:9f/16;bg.localScale=new Vector3(h*aspect,h,1);
  var renderer=bg.GetComponent<Renderer>();if(Application.isPlaying){var material=renderer.material;material.SetTexture("_MainTex",Resources.Load<Texture2D>("Lobby/ArchipelagoBackdrop"));material.SetFloat("_UseBackdrop",1);material.SetFloat("_ScreenAspect",aspect);}
 }
 void Update(){
  if(PulseAds.Fullscreen)return;
  if(level==null||!Application.isPlaying||LobbyOpen)return;if(lastSize.x!=Screen.width||lastSize.y!=Screen.height||lastSafeArea!=MobileViewport.SafeArea){lastSafeArea=MobileViewport.SafeArea;gameCamera.ResetAspect();if(cameraMode==CampaignCameraMode.AutomaticFit)FitCamera();else RefreshCameraBackdrop();FitCurrentBoard();lastSize=new Vector2(Screen.width,Screen.height);}
  if(!MotionPaused)resultDelay=Mathf.Max(0,resultDelay-Time.deltaTime);
  if(Input.GetKeyDown(KeyCode.Escape))paused=!paused;
  if(busy||paused||levelMenu||showLesson||testing)return;
  if(Input.GetKeyDown(KeyCode.Z)){RequestUI(CampaignUICommand.Undo);return;}
  if(state.dead||Rules.Won(level,state))return;
  if(Input.GetKeyDown(KeyCode.Space))Act(new Command(ActionKind.Pulse));
  if(Input.GetKeyDown(KeyCode.W))Act(new Command(ActionKind.Wait));
  if(Input.GetKeyDown(KeyCode.F))Act(new Command(ActionKind.Freeze));
  if(Input.GetKeyDown(KeyCode.D))RequestUI(CampaignUICommand.Dash);
  ProcessMoveInput();
 }
 void Act(Command command){if(busy||MotionPaused||state.dead||Rules.Won(level,state))return;var result=Rules.Apply(level,state,command,out var next,out var why);if(result==Result.Invalid){message=why;return;}activeTurn=StartCoroutine(AnimateTurn(command,next,result,why));}
 IEnumerator AnimateTurn(Command command,State next,Result result,string why){
  busy=true;dashSelected=false;hintVersion++;history.Push((state,turns));Vector3 from=hero.transform.position,to=Position(next.cell);var guardFrom=guards.Select(g=>g.transform.position).ToArray();
  var actor=hero.GetComponent<ReferenceMotion>();bool moving=command.kind==ActionKind.Move||command.kind==ActionKind.Dash;bool forced=moving&&command.target!=next.cell;Vector3 entry=forced?Position(command.target):to;
  if(command.kind==ActionKind.Pulse||command.kind==ActionKind.Freeze){var color=command.kind==ActionKind.Freeze?new Color(.35f,.8f,1):new Color(.03f,.6f,1);PulseEffects.Wave(liveBoard.transform,from,color);PulseEffects.Burst(liveBoard.transform,from+Vector3.up*.5f,color,30);}
  if(command.kind==ActionKind.Wait)PulseEffects.Wave(liveBoard.transform,from,new Color(.06f,.45f,.8f),.65f);
  if(moving)PlayEffect(jumpClip);else if(command.kind==ActionKind.Pulse)PlayEffect(passageClip);else Tone(550);if(command.kind==ActionKind.Pulse||command.kind==ActionKind.Freeze)gameCamera.GetComponent<PulseCameraFeel>()?.Kick(.7f,from);
  float duration=forced?.72f:moving?.52f:.5f;int trail=-1;
  for(float t=0;t<1;){
   if(MotionPaused){yield return null;continue;}float u=t*t*(3-2*t);
   if(forced){float a=Mathf.Clamp01(t/.62f);hero.transform.position=t<.62f?Vector3.Lerp(from,entry,a)+Vector3.up*Mathf.Sin(a*Mathf.PI)*.43f:Vector3.Lerp(entry,to,(t-.62f)/.38f)+Vector3.up*.08f;}
   else{float naturalArc=Mathf.Pow(Mathf.Sin(t*Mathf.PI),.82f);hero.transform.position=Vector3.Lerp(from,to,u)+Vector3.up*naturalArc*(command.kind==ActionKind.Dash?.86f:moving?.55f:.1f);}
   actor.Travel(t,(to-from).normalized);
   for(int i=0;i<guards.Length;i++){var dest=Position(Rules.EnemyCell(level,next,i));guards[i].transform.position=Vector3.Lerp(guardFrom[i],dest,u);if(Vector3.Distance(guardFrom[i],dest)>.01f){guards[i].transform.position+=Vector3.up*Mathf.Sin(t*Mathf.PI)*.18f;guards[i].GetComponent<ReferenceMotion>().Travel(t,(dest-guardFrom[i]).normalized);}}
   if(command.kind==ActionKind.Pulse)for(int i=0;i<islands.Length;i++)if(level.islands[i].kind==TileKind.Phase){float a=level.islands[i].phase==state.phase?0:45,b=level.islands[i].phase==next.phase?0:45;CampaignVisuals.Deck(islands[i].transform).localRotation=Quaternion.Euler(0,Mathf.Lerp(a,b,u),0);}
   for(int i=0;i<islands.Length;i++){if(level.islands[i].kind==TileKind.Spikes){var p=CampaignVisuals.Spikes(islands[i].transform);p.localScale=new Vector3(1,Mathf.Lerp(Rules.SpikesUp(level.islands[i],state.tick)?1:.035f,Rules.SpikesUp(level.islands[i],next.tick)?1:.035f,u),1);}if((next.collapsed&(1u<<i))!=0&&(state.collapsed&(1u<<i))==0)islands[i].transform.Find("Reference visual").localPosition=Vector3.down*u*.6f;}
   if(moving&&(command.kind==ActionKind.Dash||forced)&&Mathf.FloorToInt(t*5)>trail){trail=Mathf.FloorToInt(t*5);PulseEffects.Burst(liveBoard.transform,hero.transform.position+Vector3.up*.4f,new Color(.025f,.6f,1),8,.25f);}
   t+=Time.deltaTime/duration;yield return null;
  }
  for(int g=0;g<3;g++)if((next.gems&(1<<g))!=0&&(state.gems&(1<<g))==0){PulseEffects.Burst(liveBoard.transform,crystals[g].transform.position+Vector3.up*.45f,new Color(1,.6f,.04f),32);PlayEffect(collectClip);}
  for(int i=0;i<islands.Length;i++)if((next.collapsed&(1u<<i))!=0&&(state.collapsed&(1u<<i))==0)PulseEffects.Burst(liveBoard.transform,Position(i),new Color(.21f,.3f,.45f),25,.65f);
  if(next.switches!=state.switches){
   var green=new Color(.06f,1,.32f);PulseEffects.Wave(liveBoard.transform,to,green,1.35f);PulseEffects.Burst(liveBoard.transform,to+Vector3.up*.25f,green,28,.6f);if(command.kind!=ActionKind.Pulse)PlayEffect(passageClip);
   for(int i=0;i<islands.Length;i++)if(level.islands[i].kind==TileKind.Gate&&(next.switches&(1<<level.islands[i].channel))!=0)PulseEffects.Burst(liveBoard.transform,Position(i)+Vector3.up*.35f,green,18,.55f);
  }
  if(next.tick!=state.tick){
   for(int i=0;i<islands.Length;i++)if(level.islands[i].kind==TileKind.Laser&&Rules.LaserOn(level.islands[i],next.tick))PulseEffects.Burst(liveBoard.transform,Position(i)+Vector3.up*.25f,new Color(1,.08f,.015f),12,.3f);
   for(int i=0;i<guards.Length;i++)if(level.patrols[i].kind==EnemyKind.Sentinel&&Rules.SentinelFiring(level.patrols[i],next.tick))foreach(int target in level.patrols[i].targets)PulseEffects.Burst(liveBoard.transform,Position(target)+Vector3.up*.2f,new Color(1,.3f,.025f),12,.35f);
  }
  state=next;turns++;SyncVisuals(true);busy=false;actor.Land();foreach(var guard in guards)guard.GetComponent<ReferenceMotion>().Land();
  if(moving){PulseEffects.Wave(liveBoard.transform,to,new Color(.025f,.45f,.7f),.65f);gameCamera.GetComponent<PulseCameraFeel>()?.Kick(.22f,to);}
  message=result==Result.Dead?why:Rules.Won(level,state)?"Все кристаллы спасены":$"Фаза {(state.phase==0?"голубая":"янтарная")} · ритм {state.tick+1}/4";
  if(result==Result.Dead){gameCamera.GetComponent<PulseCameraFeel>()?.Kick(.85f,to);actor.Defeat();PulseEffects.Burst(liveBoard.transform,to+Vector3.up*.7f,new Color(.85f,.025f,.08f),52,1.15f);PulseEffects.Wave(liveBoard.transform,to,new Color(1,.025f,.045f),1.8f);PulseHaptics.SoftDeath(UserProfile==null||UserProfile.vibration);resultDelay=.5f;PlayEffect(deadClip);StartCoroutine(PlayGameOverSound());}
  if(result==Result.Won){gameCamera.GetComponent<PulseCameraFeel>()?.Kick(.6f,to);actor.Celebrate();PulseEffects.Burst(liveBoard.transform,to+Vector3.up*.8f,new Color(1,.66f,.055f),64,1.4f);PulseEffects.Wave(liveBoard.transform,to,new Color(.015f,.8f,1),2.5f);resultDelay=.45f;RecordVictory();RecordAdVictory();Tone(1046);}
  if(result==Result.Dead||result==Result.Won)RecordInterstitialResult(result==Result.Won);
 }
 void Undo(){if(busy||history.Count==0)return;hintVersion++;gameCamera.GetComponent<PulseCameraFeel>()?.ResetFeel();var previous=history.Pop();state=previous.state;turns=previous.turns;paused=false;resultDelay=0;foreach(var i in islands)i.transform.Find("Reference visual").localPosition=Vector3.zero;foreach(var m in liveBoard.GetComponentsInChildren<ReferenceMotion>(true))m.ResetPose();SyncVisuals(true);message="Ход отменён";}
 void RecordVictory(){if(useSeparateMenuScene)PulseSceneFlow.RecordVictory(level.id,turns,EarnedStars);if(testing){if(!useSeparateMenuScene&&Environment.GetCommandLineArgs().Contains("-lobbyVerify"))lobby?.RecordVictory(level.id,turns,EarnedStars);return;}if(!useSeparateMenuScene)lobby?.RecordVictory(level.id,turns,EarnedStars);if(ActiveMode!=LobbyMode.Campaign)return;int priorUnlocked=unlocked;unlocked=Mathf.Max(unlocked,Mathf.Min(Data.levels.Length,level.id+1));if(unlocked>priorUnlocked)PlayEffect(bonusClip);int stars=turns<=level.optimalTurns&&!hintUsed?3:turns<=level.optimalTurns+3?2:1;PlayerPrefs.SetInt(SaveKey("Unlocked"),unlocked);PlayerPrefs.SetInt(SaveKey("Stars."+level.id),Mathf.Max(stars,PlayerPrefs.GetInt(SaveKey("Stars."+level.id),0)));PlayerPrefs.Save();}
 async void Hint(){if(busy||state.dead)return;int version=++hintVersion;message="Ищу безопасный ход…";State start=state;Level selected=level;var result=await System.Threading.Tasks.Task.Run(()=>Solver.Solve(selected,start,200000));if(!this||version!=hintVersion)return;hintUsed=true;if(result.path==null||result.path.Length==0){message="Из этого положения пути нет. Отмени ход или начни заново.";return;}var cmd=result.path[0];message=cmd.kind==ActionKind.Move?$"Следующий ход: остров {cmd.target+1}":cmd.kind==ActionKind.Dash?$"Рывок на остров {cmd.target+1}":cmd.kind==ActionKind.Pulse?"Используй импульс":cmd.kind==ActionKind.Freeze?"Останови время":"Подожди один ход";if(cmd.target>=0){var ring=islands[cmd.target].transform.Find("Trap and route markers/Move highlight");ring.gameObject.SetActive(true);ring.GetComponent<LineRenderer>().widthMultiplier=.075f;}}
 void Tone(float hz){if(!sound||testing||UserProfile!=null&&UserProfile.sound<=0)return;int count=15000;var data=new float[count];for(int i=0;i<count;i++){float t=i/44100f,envelope=Mathf.Min(1,t/.012f)*Mathf.Exp(-t*10)*(1f-i/(float)count);data[i]=(Mathf.Sin(t*hz*2*Mathf.PI)+.22f*Mathf.Sin(t*hz*4*Mathf.PI))*.065f*envelope;}var clip=AudioClip.Create("Pulse bell",count,1,44100,false);clip.SetData(data,0);sound.PlayOneShot(clip,UserProfile!=null?UserProfile.sound:1);Destroy(clip,1);}
 void SetupGameAudio(){
  passageClip=Resources.Load<AudioClip>("Audio/PassageToggle");
  music=gameObject.AddComponent<AudioSource>();music.loop=true;music.playOnAwake=false;gameMusicClip=Resources.Load<AudioClip>("Audio/GameMusic");jumpClip=Resources.Load<AudioClip>("Audio/Jump");collectClip=Resources.Load<AudioClip>("Audio/CollectDiamond");deadClip=Resources.Load<AudioClip>("Audio/ModelDead");gameOverClip=Resources.Load<AudioClip>("Audio/GameOver");bonusClip=Resources.Load<AudioClip>("Audio/Bonus");music.clip=gameMusicClip;ApplyAudioSettings();if(!testing&&gameMusicClip)music.Play();
 }
 public void ApplyAudioSettings(){float background=UserProfile!=null?UserProfile.music:.3f;if(music)music.volume=background;}
 void PlayEffect(AudioClip clip){if(!clip||testing||!sound||clip==jumpClip&&UserProfile!=null&&!UserProfile.jumpSound)return;sound.PlayOneShot(clip,UserProfile!=null?UserProfile.sound:1);}
 IEnumerator PlayGameOverSound(){yield return new WaitForSecondsRealtime(.55f);PlayEffect(gameOverClip);}
 public void RefreshPresentationHuds(){presentationHuds=FindObjectsByType<CampaignHUD>(FindObjectsInactive.Include,FindObjectsSortMode.InstanceID).Where(h=>h.gameObject.scene==gameObject.scene).ToArray();}
 bool? performanceMenuVisible;
 public void SetLobbyVisible(bool visible){
  if(performanceMenuVisible!=visible){performanceMenuVisible=visible;if(UserProfile!=null)MobilePerformance.Apply(UserProfile,!visible);}
  if(liveBoard&&liveBoard.activeSelf==visible)liveBoard.SetActive(!visible);
  if(editableBoard&&editableBoard.gameObject!=liveBoard&&editableBoard.gameObject.activeSelf)editableBoard.gameObject.SetActive(false);
  if(gameCamera){gameCamera.enabled=!visible;gameCamera.cullingMask&=~(1<<28);}
  if(music&&!testing){if(visible&&music.isPlaying)music.Pause();else if(!visible&&!music.isPlaying)music.UnPause();}
  if(presentationHuds==null)RefreshPresentationHuds();
  if(!gameplayHUD)gameplayHUD=presentationHuds.FirstOrDefault(h=>h);
  foreach(var hud in presentationHuds){
   if(!hud)continue;hud.game=this;bool show=!visible&&hud==gameplayHUD;
   if(hud.gameObject.activeSelf!=show)hud.gameObject.SetActive(show);
   hud.GetComponent<Canvas>().enabled=show;
  }
 }
 public void BeginLobbyLevel(int index){LoadLevel(Mathf.Clamp(index,0,Data.levels.Length-1));}
 public void ApplyPlayerSkin(){if(hero&&UserProfile!=null)SkinAppearance.Apply(hero.transform,false,UserProfile.selectedSkin);}
 public void ApplyPortalSkin(){if(!liveBoard||UserProfile==null)return;var portal=liveBoard.transform.Find("Exit portal");if(portal)SkinAppearance.Apply(portal,true,UserProfile.selectedPortal);}
 void OnDestroy(){if(equippedSkin)Destroy(equippedSkin);if(equippedPortal)Destroy(equippedPortal);}
 int CountGems(){int n=0;for(int i=0;i<3;i++)if((state.gems&(1<<i))!=0)n++;return n;}
}

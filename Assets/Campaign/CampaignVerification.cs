using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PulseCampaign;

// Runs only in the standalone player with -campaignVerify. It never saves player progress.
public partial class CampaignGame {
 [Serializable] class RuntimeReport {
  public int levelsPassed,clickTargetsChecked,interactionChecks,uiRaycastChecks,animatedSolutionTurns,modelChecks;
  public string[] importedModels;
  public string captureMethod="Unity Camera.Render plus native Canvas overlay into a render texture; hidden-window verification";
  public bool success;
  public string[] failures;
 }
 readonly List<string> verificationFailures=new();
 int verificationInteractions,verificationUI,verificationModels,verificationAnimated;
 void Check(bool ok,string description){if(ok){verificationInteractions++;return;}verificationFailures.Add(description);Debug.LogError("INTEGRATED_CHECK_FAILED: "+description);}
 void ClickUI(CampaignUICommand command){
  var hud=FindFirstObjectByType<CampaignHUD>();hud.Refresh();Canvas.ForceUpdateCanvases();
  var button=hud.CommandButton(command);var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerDownHandler);
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerUpHandler);
  ExecuteEvents.Execute(button.gameObject,pointer,ExecuteEvents.pointerClickHandler);
 }
 void VerifyButtonRay(CampaignUICommand command){
  var hud=FindFirstObjectByType<CampaignHUD>();hud.Refresh();Canvas.ForceUpdateCanvases();var button=hud.CommandButton(command);
  var rect=(RectTransform)button.transform;var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
  var hits=new List<RaycastResult>();hud.GetComponent<GraphicRaycaster>().Raycast(pointer,hits);
  if(hits.Count==0||hits[0].gameObject!=button.gameObject)verificationFailures.Add("Canvas raycast missed "+command);else verificationUI++;
 }
 Texture2D CaptureVerificationFrame(){
  // Hidden Windows swapchains can return black screenshots. Render the same world/post-FX
  // explicitly, then its real Canvas as an unprocessed overlay. No art or UI is substituted.
  var hud=FindFirstObjectByType<CampaignHUD>();var canvas=hud.GetComponent<Canvas>();
  var oldMode=canvas.renderMode;var oldCanvasCamera=canvas.worldCamera;float oldPlane=canvas.planeDistance;
  var uiNodes=hud.GetComponentsInChildren<Transform>(true);var oldLayers=uiNodes.Select(t=>t.gameObject.layer).ToArray();
  var previousTarget=gameCamera.targetTexture;var previousActive=RenderTexture.active;
  var target=new RenderTexture(Screen.width,Screen.height,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
  var uiObject=new GameObject("Verification-only overlay camera");var uiCamera=uiObject.AddComponent<Camera>();uiCamera.enabled=false;
  try{
   target.Create();gameCamera.targetTexture=target;gameCamera.Render();gameCamera.targetTexture=previousTarget;
   uiCamera.targetTexture=target;uiCamera.clearFlags=CameraClearFlags.Depth;uiCamera.cullingMask=1<<31;uiCamera.orthographic=true;uiCamera.orthographicSize=5;uiCamera.nearClipPlane=.01f;uiCamera.farClipPlane=2;uiCamera.allowHDR=false;
   foreach(var node in uiNodes)node.gameObject.layer=31;
   canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=uiCamera;canvas.planeDistance=.5f;Canvas.ForceUpdateCanvases();uiCamera.Render();
   RenderTexture.active=target;var texture=new Texture2D(Screen.width,Screen.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,Screen.width,Screen.height),0,0);texture.Apply();return texture;
  }finally{
   gameCamera.targetTexture=previousTarget;RenderTexture.active=previousActive;
   canvas.renderMode=oldMode;canvas.worldCamera=oldCanvasCamera;canvas.planeDistance=oldPlane;
   for(int i=0;i<uiNodes.Length;i++)uiNodes[i].gameObject.layer=oldLayers[i];Canvas.ForceUpdateCanvases();
   uiCamera.targetTexture=null;Destroy(uiObject);target.Release();Destroy(target);
  }
 }
 IEnumerator VerifyScreenshot(string name){
  yield return new WaitForSeconds(.22f);yield return new WaitForEndOfFrame();var tex=CaptureVerificationFrame();
  if(!tex){verificationFailures.Add("Screenshot failed: "+name);yield break;}
  int pink=tex.GetPixels32().Count(p=>p.r>247&&p.b>247&&p.g<12);
  Check(tex.GetPixels32().Count(p=>p.r>20||p.g>20||p.b>20)>tex.width*tex.height*.05f,"Empty or black render in "+name);
  Check(pink<tex.width*tex.height*.006f,"Error-shader magenta pixels in "+name+": "+pink);
  System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(Application.dataPath+"/../../"+name+".png"),ImageConversion.EncodeToPNG(tex));Destroy(tex);
 }
 public void VerificationUnlock(int value){if(testing)unlocked=Mathf.Clamp(value,1,Data.levels.Length);}
 public IEnumerator PlayWitnessForLobbyTest(){if(!testing)yield break;showLesson=false;foreach(var command in level.solution){Act(command);while(busy)yield return null;if(state.dead)yield break;}yield return new WaitForSeconds(.7f);}
 IEnumerator VerifyRuntime(){
  int passed=0,clicks=0;var seen=new HashSet<string>();
  var seenTiles=new HashSet<TileKind>();var seenEnemies=new HashSet<EnemyKind>();
  var savedCameraPosition=gameCamera.transform.position;var savedCameraRotation=gameCamera.transform.rotation;float savedFov=gameCamera.fieldOfView;
  Check(cameraMode==CampaignCameraMode.SceneTransform&&Vector3.Distance(savedCameraPosition,new Vector3(-.5f,22,-28.7f))<.001f&&Quaternion.Angle(savedCameraRotation,Quaternion.Euler(38,1,0))<.01f,"The game did not start with the requested Scene camera transform");
  foreach(var id in new[]{"Normal","Move","Rotate","Trap","Breakable","Spirit","Shadow","Crystal","Portal"}){
   var model=Resources.Load<ReferenceModelAsset>("ReferenceModels/"+id);
   bool valid=model&&model.material&&model.material.shader.isSupported&&model.parts.Length>0&&model.parts.All(p=>p.mesh&&p.mesh.vertexCount>0);
   if(valid)foreach(var map in new[]{"_MainTex","_BumpMap","_MetallicGlossMap","_EmissionMap"})valid&=model.material.GetTexture(map)!=null;
   if(valid&&(id=="Spirit"||id=="Shadow"))valid=model.parts.Any(p=>p.mesh.blendShapeCount==2);
   if(valid)verificationModels++;else verificationFailures.Add("Baked Blender geometry/material/animation missing: "+id);
  }
  Check(Shader.Find("UI/Default")&&Shader.Find("UI/Default").isSupported,"Native UI shader unavailable");
  for(int i=0;i<Data.levels.Length;i++){
   LoadLevel(i);showLesson=false;yield return null;
   foreach(var id in liveBoard.GetComponentsInChildren<ReferenceIdentity>(true))seen.Add(id.modelId);
   foreach(var tile in level.islands)seenTiles.Add(tile.kind);foreach(var enemy in level.patrols)seenEnemies.Add(enemy.kind);
   if(i<10)yield return VerifyScreenshot("variety-layout-"+(i+1));
   if(i==11||i==27||i==30||i==44||i==64||i==93||i==99)yield return VerifyScreenshot("integrated-level-"+(i+1));
   try{
    CampaignGenerator.ValidateDefinition(level);Physics.SyncTransforms();
    if(Vector3.Distance(gameCamera.transform.position,savedCameraPosition)>.001f||Quaternion.Angle(gameCamera.transform.rotation,savedCameraRotation)>.01f||Mathf.Abs(gameCamera.fieldOfView-savedFov)>.001f)throw new Exception("Level loading changed the manually positioned camera");
    foreach(var island in islands){
     var point=gameCamera.WorldToScreenPoint(island.transform.position);
     if(!MobileViewport.BoardPixels.Contains(point))throw new Exception("Island outside clickable viewport: "+island.index);
     if(!Physics.Raycast(gameCamera.ScreenPointToRay(point),out var hit)||hit.collider.GetComponentInParent<CampaignIsland>()!=island)throw new Exception("Island centre occluded: "+island.index);
     clicks++;
    }
    VerifyStableSurfaces();
    foreach(var command in level.solution){var outcome=Rules.Apply(level,state,command,out var next,out var why);if(outcome==Result.Dead||outcome==Result.Invalid)throw new Exception(why);state=next;turns++;SyncVisuals(true);VerifyStableSurfaces();if(Vector3.Distance(hero.transform.position,Position(state.cell))>.001f)throw new Exception("Actor/view desync");for(int guard=0;guard<guards.Length;guard++)if(Vector3.Distance(guards[guard].transform.position,Position(Rules.EnemyCell(level,state,guard)))>.001f)throw new Exception("Enemy/view desync");}
    if(!Rules.Won(level,state)||crystals.Any(g=>g.activeSelf))throw new Exception("Victory or gem rendering failed");
    state=Rules.Initial(level);SyncVisuals(true);if(crystals.Any(g=>!g.activeSelf))throw new Exception("Restart does not restore crystals");
    passed++;Debug.Log("INTEGRATED_RUNTIME_PASS "+level.id);
   }catch(Exception e){verificationFailures.Add("Level "+level.id+": "+e.Message);Debug.LogError(verificationFailures.Last());}
   yield return null;
  }
  Check(seen.Count==9,"Not all nine imported model types were used by the campaign");
  Check(seenTiles.Count==10&&seenEnemies.Count==3,"The new traps or enemy behaviors are missing from the campaign");
  Check(Data.levels.Select(l=>l.layout).Distinct().Count()==10&&Data.levels.Select(CampaignGenerator.TopologySignature).Distinct().Count()==100,"The campaign lacks 100 different route graphs");
  int previousPreview=previewLevel;previewLevel=65;LoadLevel(64);yield return null;Check(islands.Length==level.islands.Length&&guards.Length==level.patrols.Length,"Changing Preview Level without rebaking reused a stale board");previewLevel=previousPreview;
  LoadLevel(64);showLesson=false;yield return null;
  foreach(var cmd in new[]{CampaignUICommand.Pause,CampaignUICommand.Pulse,CampaignUICommand.Wait,CampaignUICommand.Undo,CampaignUICommand.Freeze,CampaignUICommand.Dash,CampaignUICommand.Hint})VerifyButtonRay(cmd);
  Check(!gameplayHUD.safeRoot.Find("Home")&&!gameplayHUD.safeRoot.Find("Levels"),"Standalone navigation buttons remain on the game screen");
  ClickUI(CampaignUICommand.Pause);yield return new WaitForSeconds(.25f);VerifyButtonRay(CampaignUICommand.Home);VerifyButtonRay(CampaignUICommand.Levels);yield return VerifyScreenshot("cinematic-pause");ClickUI(CampaignUICommand.Home);yield return null;Check(LobbyOpen&&!Busy,"Pause menu did not return home");lobby.Hide();LoadLevel(64);yield return null;
  var visual=hero.transform.Find("Reference visual");var skin=hero.GetComponentInChildren<SkinnedMeshRenderer>();var initialPose=visual.localPosition;float initialMorph=skin.GetBlendShapeWeight(0);
  yield return new WaitForSeconds(.18f);
  Check(visual.localPosition!=initialPose||Mathf.Abs(skin.GetBlendShapeWeight(0)-initialMorph)>.01f,"Blender idle poses are not animated");
  State saved=state;int savedTurns=turns;var first=level.solution[0];Rules.Apply(level,state,first,out var expected,out _);
  Act(first);yield return null;ClickUI(CampaignUICommand.Pause);yield return new WaitForEndOfFrame();
  var pausedPosition=hero.transform.position;var pausedVisual=visual.localPosition;var pausedMorph=skin.GetBlendShapeWeight(0);
  yield return new WaitForSeconds(.14f);
  Check(paused&&hero.transform.position==pausedPosition&&visual.localPosition==pausedVisual&&Mathf.Abs(skin.GetBlendShapeWeight(0)-pausedMorph)<.001f&&state.Key()==saved.Key(),"Native pause did not freeze the in-flight turn and Blender pose");
  VerifyButtonRay(CampaignUICommand.Primary);ClickUI(CampaignUICommand.Primary);
  Check(!paused,"Native Continue did not resume an in-flight turn");
  float deadline=Time.realtimeSinceStartup+3;while(busy&&Time.realtimeSinceStartup<deadline)yield return null;
  Check(!busy&&state.Key()==expected.Key()&&turns==savedTurns+1,"Animated action did not match the rules");
  ClickUI(CampaignUICommand.Undo);
  Check(state.Key()==saved.Key()&&state.dead==saved.dead&&turns==savedTurns&&Vector3.Distance(hero.transform.position,Position(state.cell))<.001f,"Native Undo did not restore the complete turn");
  Act(new Command(ActionKind.Move,state.cell));Check(!busy&&state.Key()==saved.Key(),"Invalid world input consumed a turn");
  foreach(var pair in new[]{(CampaignUICommand.Wait,ActionKind.Wait),(CampaignUICommand.Pulse,ActionKind.Pulse),(CampaignUICommand.Freeze,ActionKind.Freeze)}){
   LoadLevel(64);yield return null;Rules.Apply(level,state,new Command(pair.Item2),out expected,out _);ClickUI(pair.Item1);
   deadline=Time.realtimeSinceStartup+3;while(busy&&Time.realtimeSinceStartup<deadline)yield return null;
   Check(!busy&&state.Key()==expected.Key()&&turns==1,"Native button failed: "+pair.Item1);
   ClickUI(CampaignUICommand.Undo);Check(state.Key()==Rules.Initial(level).Key()&&turns==0,"Undo failed after "+pair.Item1);
  }
  LoadLevel(64);yield return null;ClickUI(CampaignUICommand.Dash);Check(dashSelected,"Native Dash selector did not activate");
  int target=-1;for(int i=0;i<islands.Length;i++){var r=Rules.Apply(level,state,new Command(ActionKind.Dash,i),out expected,out _);if(r!=Result.Invalid&&r!=Result.Dead){target=i;break;}}
  Check(target>=0,"No valid dash test target");if(target>=0){Rules.Apply(level,state,new Command(ActionKind.Dash,target),out expected,out _);Act(new Command(dashSelected?ActionKind.Dash:ActionKind.Move,target));while(busy)yield return null;Check(state.Key()==expected.Key()&&state.dashes==0,"Dash charge/animation did not match the rules");ClickUI(CampaignUICommand.Undo);}
  ClickUI(CampaignUICommand.Hint);deadline=Time.realtimeSinceStartup+5;while(message=="Ищу безопасный ход…"&&Time.realtimeSinceStartup<deadline)yield return null;
  Check(message!="Ищу безопасный ход…"&&hintUsed,"Native Hint did not return a solution step");
  // A whole route runs through the real animation coroutine. Deliberately take a
  // fatal action once and undo it to check defeat, morph reset and restored charges.
  LoadLevel(11);yield return null;bool deathChecked=false;
  foreach(var command in level.solution){
   Rules.Apply(level,state,command,out expected,out _);
   if(command.kind==ActionKind.Pulse)ClickUI(CampaignUICommand.Pulse);else if(command.kind==ActionKind.Wait)ClickUI(CampaignUICommand.Wait);else Act(command);
   while(busy)yield return null;
   Check(state.Key()==expected.Key()&&state.dead==expected.dead,"Animated witness diverged on turn "+turns);verificationAnimated++;
   var fatalActions=Rules.Actions(level,state).Where(a=>Rules.Apply(level,state,a,out _,out _)==Result.Dead).ToArray();
   if(!deathChecked&&fatalActions.Length>0){
    saved=state;savedTurns=turns;Act(fatalActions[0]);while(busy)yield return null;yield return new WaitForSeconds(.65f);
    Check(state.dead,"A dangerous action did not trigger defeat");yield return VerifyScreenshot("integrated-defeat");
    ClickUI(CampaignUICommand.Primary);yield return null;
    Check(!state.dead&&state.Key()==saved.Key()&&turns==savedTurns&&hero.transform.Find("Reference visual").localScale.x>.7f,"Defeat Undo did not restore the actor and charges");deathChecked=true;
   }
  }
  Check(deathChecked,"Defeat/Undo test was not exercised");Check(Victorious&&GemCount==3,"Animated route did not reach the portal");
  yield return new WaitForSeconds(.8f);var hud=FindFirstObjectByType<CampaignHUD>();hud.Refresh();
  Check(hud.dialogFade.alpha>.95f&&hud.starsRoot.activeSelf&&hud.primaryText.text=="СЛЕДУЮЩИЙ ОСТРОВ","Victory UI did not open correctly");yield return VerifyScreenshot("integrated-victory");
  ClickUI(CampaignUICommand.Primary);yield return null;Check(level.id==13&&!Victorious&&turns==0,"Next-level button did not reset the board");
  ClickUI(CampaignUICommand.Pause);yield return new WaitForSeconds(.2f);ClickUI(CampaignUICommand.Levels);yield return null;Check(levelMenu,"Level picker did not open");ClickUI(CampaignUICommand.NextPage);yield return null;Check(menuPage==1,"Level picker page did not advance");yield return VerifyScreenshot("integrated-level-select");
  var slot=hud.levelButtons[0];ExecuteEvents.Execute(slot.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);yield return null;
  Check(level.id==21&&!levelMenu,"Level tile did not open level 21");
  showLesson=true;yield return null;Check(MotionPaused,"Lesson did not suspend gameplay");ClickUI(CampaignUICommand.Primary);Check(!showLesson,"Lesson start button failed");
  var cameraFeel=gameCamera.GetComponent<PulseCameraFeel>();Check(cameraFeel!=null,"Camera reaction component missing");cameraFeel.Kick(.8f,hero.transform.position);yield return null;
  Check(cameraFeel.Amount>0&&Mathf.Abs(gameCamera.fieldOfView-22)<.001f&&gameCamera.transform.position==savedCameraPosition,"Camera effects changed the chosen lens or position");
  paused=true;float heldImpact=cameraFeel.Amount;yield return null;yield return null;Check(Mathf.Abs(cameraFeel.Amount-heldImpact)<.001f,"Camera effect continued during pause");paused=false;
  yield return VerifyScreenshot("variety-camera-effect");yield return new WaitForSeconds(.7f);Check(cameraFeel.Amount<.001f,"Camera reaction did not settle");
  // Late-game witnesses exercise moving hunters, relay/gate and mixed traps in actual animation.
  foreach(int late in new[]{45,94}){
   LoadLevel(late-1);showLesson=false;yield return null;
   foreach(var command in level.solution){
    Rules.Apply(level,state,command,out expected,out _);Act(command);deadline=Time.realtimeSinceStartup+3;
    while(busy&&Time.realtimeSinceStartup<deadline)yield return null;
    Check(!busy&&state.Key()==expected.Key()&&!state.dead,"Late animated route diverged: level "+late+" turn "+turns);verificationAnimated++;
    for(int i=0;i<guards.Length;i++)Check(Vector3.Distance(guards[i].transform.position,Position(Rules.EnemyCell(level,state,i)))<.001f,"Animated enemy missed its destination");
   }
   Check(Victorious,"Late animated witness did not reach the portal: "+late);
  }
  var customCameraPosition=savedCameraPosition+new Vector3(.25f,.1f,.15f);var customCameraRotation=Quaternion.Euler(38.5f,.7f,.2f);
  gameCamera.transform.SetPositionAndRotation(customCameraPosition,customCameraRotation);LoadLevel(0);yield return null;gameCamera.ResetAspect();RefreshCameraBackdrop();FitCamera();
  Check(Vector3.Distance(gameCamera.transform.position,customCameraPosition)<.001f&&Quaternion.Angle(gameCamera.transform.rotation,customCameraRotation)<.01f,"Later manual camera edits were overwritten by loading, aspect refresh or automatic-fit calls");
  gameCamera.transform.SetPositionAndRotation(savedCameraPosition,savedCameraRotation);
  LoadLevel(27);yield return null;Act(level.solution[0]);yield return null;ClickUI(CampaignUICommand.Pause);yield return new WaitForSeconds(.2f);ClickUI(CampaignUICommand.Home);yield return null;Check(LobbyOpen&&!Busy,"Cannot leave through Pause during an animated move");lobby.Hide();LoadLevel(27);yield return null;Check(state.Key()==Rules.Initial(level).Key(),"Returning after interrupted move left a stale state");
  var report=new RuntimeReport{levelsPassed=passed,clickTargetsChecked=clicks,interactionChecks=verificationInteractions,uiRaycastChecks=verificationUI,animatedSolutionTurns=verificationAnimated,modelChecks=verificationModels,importedModels=seen.OrderBy(s=>s).ToArray(),success=verificationFailures.Count==0,failures=verificationFailures.ToArray()};
  System.IO.File.WriteAllText(System.IO.Path.GetFullPath(Application.dataPath+"/../../integrated-runtime-verification.json"),JsonUtility.ToJson(report,true));
  Debug.Log($"INTEGRATED_RUNTIME_VERIFIED {passed}/{Data.levels.Length}, {clicks} world targets, {verificationUI} UI targets, {verificationInteractions} checks, {verificationModels} baked models");Application.Quit(report.success?0:1);
 }
 void VerifyStableSurfaces(){
  var contact=hero.transform.Find("Soft contact radiance");
  if(!contact||contact.GetComponent<Renderer>().sharedMaterial.shader.name!="Pulse/CinematicContact")throw new Exception("Hero skin overwrote contact-light material");
  foreach(var tile in islands){
   var n=level.islands[tile.index];var body=ReferenceArt.Part(tile.transform,"Base");
   if(!tile.gameObject.activeInHierarchy||!body||!body.gameObject.activeInHierarchy||!body.GetComponent<Renderer>().enabled)throw new Exception("Platform foundation disappeared: "+tile.index);
   if(n.kind==TileKind.Rift||n.kind==TileKind.Phase||n.kind==TileKind.Fragile){
    var seal=tile.transform.Find("Surface sealed • not walkable");bool expected=!Rules.Passable(level,state,tile.index);
    if(!seal||seal.gameObject.activeSelf!=expected)throw new Exception("Closed-surface marker disagrees with rules: "+tile.index);
   }
  }
 }
}

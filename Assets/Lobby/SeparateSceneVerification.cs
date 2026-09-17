using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class SeparateSceneVerification:MonoBehaviour {
 [Serializable] class Report{public bool success;public int checks,levels,transitions;public string[] failures;public bool physicalDeviceTested=false;}
 readonly List<string> failures=new();int checks,levels,transitions;float started;bool finished;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
 static void Boot(){var args=Environment.GetCommandLineArgs();bool utility=args.Contains("-backdropCapture")||args.Contains("-menuPagesCapture");if((!PulseSceneFlow.Testing&&!utility)||FindFirstObjectByType<SeparateSceneVerification>())return;var g=new GameObject("QA • separate scenes");DontDestroyOnLoad(g);g.AddComponent<SeparateSceneVerification>();}
 void Check(bool ok,string message){checks++;if(!ok){failures.Add(message);Debug.LogError("SCENE_CHECK_FAILED: "+message);}}
 PulseLobby Menu=>FindFirstObjectByType<PulseLobby>();CampaignGame Game=>FindFirstObjectByType<CampaignGame>();
 IEnumerator Ready(bool menu){float deadline=Time.realtimeSinceStartup+15;while((PulseSceneFlow.Loading||(menu?Menu==null||!Menu.IsOpen:Game==null||Game.ActiveBoardForVerification==null))&&Time.realtimeSinceStartup<deadline)yield return null;Check(menu?Menu&&Menu.IsOpen:Game&&Game.ActiveBoardForVerification,"Scene did not become ready");yield return null;}
 void Ownership(bool menu){
  Check(SceneManager.sceneCount==1,"Old scene was not unloaded");
  Check((Menu!=null)==menu&&(Game!=null)!=menu,"Menu and game controllers coexist");
  Check(FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Duplicate EventSystem");
  Check(FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(a=>a.enabled)==1,"Duplicate or missing AudioListener");
  Check(Camera.allCameras.Count(c=>c.enabled&&c.targetTexture==null)==1,"Duplicate world camera");
 }
 void Click(LobbyAction action,int value=0){
  var b=Menu.GetComponentsInChildren<LobbyButton>().First(x=>x.action==action&&x.value==value&&x.GetComponent<Button>().interactable);
  Click(b);
 }
 void Click(LobbyButton b){
  var r=(RectTransform)b.transform;Canvas.ForceUpdateCanvases();var pointer=new PointerEventData(EventSystem.current){position=RectTransformUtility.WorldToScreenPoint(null,r.TransformPoint(r.rect.center)),button=PointerEventData.InputButton.Left};
  var hits=new List<RaycastResult>();Menu.canvas.GetComponent<GraphicRaycaster>().Raycast(pointer,hits);Check(hits.Count>0&&hits[0].gameObject.GetComponentInParent<LobbyButton>()==b,"Menu ray missed "+b.name+" -> "+string.Join(" / ",hits.Take(3).Select(h=>h.gameObject.name)));b.GetComponent<Button>().onClick.Invoke();
 }
 void Capture(string file,bool menu){var tex=menu?Menu.CaptureMenuForVerification():Game.CaptureSceneForVerification();File.WriteAllBytes(Path.Combine(Application.dataPath,"../../"+file+".png"),ImageConversion.EncodeToPNG(tex));Destroy(tex);}
 IEnumerator BackgroundOnly(){
  yield return Ready(true);
  foreach(var format in new[]{new Vector2Int(432,936),new Vector2Int(450,800),new Vector2Int(378,924)}){
   Screen.SetResolution(format.x,format.y,false);yield return new WaitForSeconds(.3f);MobileViewport.TestSafeArea=new Rect(8,18,Screen.width-16,Screen.height-48);Menu.Layout();Menu.stage.Frame();yield return null;
   var stage=Menu.stage;var hero=ScreenComposition.Bounds(stage.spirit,stage.view,ScreenComposition.Geometry(stage.spirit));Vector2 landing=stage.view.WorldToScreenPoint(stage.backdrop.TransformPoint(new Vector3(stage.paintedLandingUv.x-.5f,stage.paintedLandingUv.y-.5f,0)));
   Check(Vector2.Distance(new Vector2(hero.center.x,hero.yMin),landing)<4,"Background landing missed the hero on "+format);var bg=ScreenComposition.Bounds(stage.backdrop,stage.view,ScreenComposition.Geometry(stage.backdrop));Check(bg.xMin<=0&&bg.yMin<=0&&bg.xMax>=Screen.width&&bg.yMax>=Screen.height,"Background exposed an edge on "+format);if(format==new Vector2Int(450,800))Capture("background-phone-preview",true);
  }
  MobileViewport.TestSafeArea=null;var report=new Report{success=failures.Count==0,checks=checks,levels=0,transitions=0,failures=failures.ToArray()};File.WriteAllText(Path.Combine(Application.dataPath,"../../background-only-verification.json"),JsonUtility.ToJson(report,true));Debug.Log("BACKGROUND_ONLY_VERIFIED "+report.success+" / "+checks+" checks");Application.Quit(report.success?0:1);
 }
 IEnumerator MenuPagesOnly(){yield return Ready(true);Menu.Click(LobbyAction.Gift);yield return new WaitForSeconds(.35f);Capture("menu-gift-preview",true);Menu.ClosePopup();Menu.Click(LobbyAction.Settings);yield return new WaitForSeconds(.35f);Capture("menu-settings-preview",true);Menu.ClosePopup();Menu.OpenVideoReward(false);yield return new WaitForSeconds(.35f);Capture("menu-reward-preview",true);Debug.Log("MENU_PAGES_CAPTURED");Application.Quit(0);}
 IEnumerator Start(){
  started=Time.realtimeSinceStartup;if(Environment.GetCommandLineArgs().Contains("-backdropCapture")){yield return BackgroundOnly();yield break;}if(Environment.GetCommandLineArgs().Contains("-menuPagesCapture")){yield return MenuPagesOnly();yield break;}Application.logMessageReceived+=Log;
  string profileBefore=PlayerPrefs.GetString("PulseLobby.Profile.v1","");int unlockedBefore=PlayerPrefs.GetInt("PulseCampaign.Variety2.Unlocked",1),currentBefore=PlayerPrefs.GetInt("PulseCampaign.Variety2.Current",1);
  yield return Ready(true);Ownership(true);
  var backdropTexture=Menu.stage.backdrop.GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
  if(backdropTexture&&backdropTexture.name=="MenuCosmosBackdrop"){
   Check(backdropTexture.width==874&&backdropTexture.height==1800,"Supplied menu background was resized or replaced");
   Check(!Menu.stage.portalGroup.gameObject.activeSelf&&!Menu.stage.bridgeA.gameObject.activeSelf&&!Menu.stage.bridgeB.gameObject.activeSelf,"3D islands duplicate the painted background");
   Check(Menu.stage.GetComponent<AuthoredMenuArt>().artwork==Menu.stage.spirit,"Menu composition is not framing the live hero");
   Check(Menu.stage.heroGroup.Cast<Transform>().Where(t=>t!=Menu.stage.spirit).All(t=>!t.gameObject.activeSelf),"3D hero platform duplicates the painted platform");
   var backgroundMaterial=Menu.stage.backdrop.GetComponent<Renderer>().sharedMaterial;
   Check(backgroundMaterial.shader.name=="Pulse/MenuBackdrop"&&backgroundMaterial.GetFloat("_Exposure")<=.65f,"Bright menu background reduces UI contrast");
   var authored=Menu.stage.GetComponent<AuthoredMenuArt>();Check(authored.lockArtworkPosition,"Live hero anchor is not locked");
   var anchorPosition=Menu.stage.spirit.localPosition;var backgroundPosition=Menu.stage.backdrop.localPosition;yield return new WaitForSeconds(.8f);
   Check(Vector3.Distance(Menu.stage.spirit.localPosition,anchorPosition)<.0001f,"Menu hero drifted away from the painted platform");
   Check(Vector3.Distance(Menu.stage.backdrop.localPosition,backgroundPosition)<.0001f,"Painted platform drifted under the anchored hero");
   var skin=Menu.stage.spirit.GetComponentInChildren<SkinnedMeshRenderer>();Check(!skin||Mathf.Abs(skin.GetBlendShapeWeight(0))+Mathf.Abs(skin.GetBlendShapeWeight(1))<.01f,"Menu blend shapes shift the hero away from its anchor");
  }
  if(backdropTexture&&backdropTexture.name=="MenuCastleVista"){
   Check(backdropTexture.width==941&&backdropTexture.height==1672,"Castle menu background was resized or replaced");var lighting=Menu.stage.GetComponent<MenuHeroLighting>();
   Check(lighting&&lighting.hero==Menu.stage.spirit&&lighting.overhead&&lighting.rim&&lighting.contact,"Menu hero light rig is incomplete");
   var authored=Menu.stage.GetComponent<AuthoredMenuArt>();var bounds=ScreenComposition.Bounds(Menu.stage.spirit,Menu.stage.view,ScreenComposition.Geometry(Menu.stage.spirit));var safe=MobileViewport.SafeArea;float expected=safe.x+(authored.viewport.x+authored.viewport.width*.5f)*safe.width;
   Check(Mathf.Abs(bounds.center.x-expected)<4,"Live hero is not centered on the foreground stone");
  }
  // Manual UI and model edits must survive updates and opening/closing a popup.
  var button=(RectTransform)Menu.safeRoot.Find("Settings");var position=button.anchoredPosition;var dimensions=button.sizeDelta;var model=Menu.stage.heroGroup;var modelPosition=model.localPosition;
  button.anchoredPosition+=new Vector2(3,2);button.sizeDelta+=Vector2.one*2;model.localPosition+=Vector3.right*.035f;
  Menu.Layout();Menu.stage.Frame();yield return null;Menu.Click(LobbyAction.Settings);yield return null;Menu.ClosePopup();
  Check(button.anchoredPosition==position+new Vector2(3,2)&&button.sizeDelta==dimensions+Vector2.one*2,"Menu layout overwrote authored UI");Check(model.localPosition==modelPosition+Vector3.right*.035f,"Framing overwrote an authored model transform");button.anchoredPosition=position;button.sizeDelta=dimensions;model.localPosition=modelPosition;
  foreach(var format in new[]{new Vector2Int(432,936),new Vector2Int(396,880),new Vector2Int(414,896),new Vector2Int(450,800),new Vector2Int(648,864),new Vector2Int(378,924)}){
   Screen.SetResolution(format.x,format.y,false);yield return new WaitForSeconds(.25f);
   MobileViewport.TestSafeArea=new Rect(8,18,Screen.width-16,Screen.height-48);Menu.Layout();Menu.stage.Frame();Canvas.ForceUpdateCanvases();
   foreach(var b in Menu.safeRoot.GetComponentsInChildren<LobbyButton>()){
    var corners=new Vector3[4];((RectTransform)b.transform).GetWorldCorners(corners);var min=RectTransformUtility.WorldToScreenPoint(null,corners[0]);var max=RectTransformUtility.WorldToScreenPoint(null,corners[2]);var safe=MobileViewport.SafeArea;
    Check(min.x>=safe.xMin-2&&max.x<=safe.xMax+2&&min.y>=safe.yMin-2&&max.y<=safe.yMax+2,"Authored control clipped: "+b.name+" "+format);
   }
   if(backdropTexture&&backdropTexture.name=="MenuCastleVista"){
    var stage=Menu.stage;var hero=ScreenComposition.Bounds(stage.spirit,stage.view,ScreenComposition.Geometry(stage.spirit));
    Vector2 landing=stage.view.WorldToScreenPoint(stage.backdrop.TransformPoint(new Vector3(stage.paintedLandingUv.x-.5f,stage.paintedLandingUv.y-.5f,0)));
    Check(Vector2.Distance(new Vector2(hero.center.x,hero.yMin),landing)<4,"Background landing missed the hero on "+format);
    if(format==new Vector2Int(450,800))Capture("background-phone-preview",true);
   }
  }
  MobileViewport.TestSafeArea=null;Screen.SetResolution(432,936,false);yield return new WaitForSeconds(.3f);Capture("separate-menu-preview",true);
  if(Menu.safeRoot.Find("Profile").GetComponent<LobbyPlate>().sprite){
   foreach(var plate in Menu.safeRoot.GetComponentsInChildren<LobbyPlate>()){
    Check(plate.sprite||plate.hitAreaOnly,"Atlas style missing: "+plate.name);
    if(plate.sprite)Check((plate.sprite.texture.name=="SkinMenuButton"&&plate.sprite.texture.width==1254&&plate.sprite.texture.height==1254)||(plate.sprite.texture.width==2172&&plate.sprite.texture.height==724),"UI artwork was resized: "+plate.name);
   }
   foreach(var reward in new[]{("Crystal wallet/Add crystals",false),("Resonance wallet/Add resonance",true)}){Click(Menu.safeRoot.Find(reward.Item1).GetComponent<LobbyButton>());yield return new WaitForSeconds(.22f);Check(Menu.Page=="Награда"&&Menu.PendingPulseReward==reward.Item2,"Reward plus is not connected: "+reward.Item1);Click(LobbyAction.Close);yield return null;}
   string rewardDay=Menu.Profile.rewardDay;int oldCrystalRewards=Menu.Profile.crystalRewardsToday,oldPulseRewards=Menu.Profile.resonanceRewardsToday,oldCrystals=Menu.Profile.crystals,oldResonance=Menu.Profile.resonance;Menu.Profile.rewardDay=Menu.Today;Menu.Profile.crystalRewardsToday=Menu.Profile.resonanceRewardsToday=0;
   Click(Menu.safeRoot.Find("Crystal wallet/Add crystals").GetComponent<LobbyButton>());yield return null;Click(LobbyAction.WatchReward);yield return new WaitForSecondsRealtime(3.25f);Check(Menu.Profile.crystals==oldCrystals+10&&Menu.Profile.crystalRewardsToday==1,"Crystal reward button did not grant 10");Click(LobbyAction.Close);
   Click(Menu.safeRoot.Find("Resonance wallet/Add resonance").GetComponent<LobbyButton>());yield return null;Click(LobbyAction.WatchReward);yield return new WaitForSecondsRealtime(3.25f);Check(Menu.Profile.resonance==oldResonance+1&&Menu.Profile.resonanceRewardsToday==1,"Pulse reward button did not grant 1");Click(LobbyAction.Close);
   Menu.Profile.crystals=oldCrystals;Menu.Profile.resonance=oldResonance;Menu.Profile.crystalRewardsToday=Menu.Profile.resonanceRewardsToday=0;for(int i=0;i<5;i++){Check(Menu.Profile.ClaimVideoReward(false,Menu.Today),"Crystal reward stopped before daily limit");Check(Menu.Profile.ClaimVideoReward(true,Menu.Today),"Pulse reward stopped before daily limit");}Check(!Menu.Profile.ClaimVideoReward(false,Menu.Today)&&!Menu.Profile.ClaimVideoReward(true,Menu.Today),"Reward exceeded five views per day");Check(Menu.Profile.crystals==oldCrystals+50&&Menu.Profile.resonance==oldResonance+5,"Reward amount is incorrect");Menu.Profile.rewardDay=rewardDay;Menu.Profile.crystalRewardsToday=oldCrystalRewards;Menu.Profile.resonanceRewardsToday=oldPulseRewards;Menu.Profile.crystals=oldCrystals;Menu.Profile.resonance=oldResonance;
   string priorGift=Menu.Profile.giftDay;Menu.Profile.giftDay=Menu.Today;Menu.Save();Check(!Menu.giftBadge.activeSelf,"Claimed gift still shows notification");Menu.Profile.giftDay=priorGift;Menu.Save();Check(Menu.giftBadge.activeSelf,"Ready gift has no notification");
  }
  Check(!Menu.GetComponentsInChildren<LobbyButton>().Any(b=>b.action==LobbyAction.Collection||b.action==LobbyAction.Stats),"Removed statistics or collection button is still active");
  foreach(var action in new[]{LobbyAction.Settings,LobbyAction.Profile,LobbyAction.Skins,LobbyAction.Gift}){if(!Menu.GetComponentsInChildren<LobbyButton>().Any(b=>b.action==action))continue;Click(action);yield return new WaitForSeconds(.22f);Check(Menu.Page!="","No page opened for "+action);Click(LobbyAction.Close);yield return null;}
  Menu.Profile.playerName="Scene QA";Menu.Profile.crystals=450;Click(LobbyAction.Skins);yield return new WaitForSeconds(.22f);Click(LobbyAction.BuySkin,1);yield return new WaitForSeconds(.22f);Check(Menu.Profile.selectedSkin==1,"Skin selection failed");Click(LobbyAction.Close);
  var profile=Menu.Profile;Click(LobbyAction.Play);transitions++;yield return Ready(false);Ownership(false);Check(PulseSceneFlow.Profile==profile&&profile.selectedSkin==1,"Session profile was lost at Play");
  for(int i=0;i<Game.Data.levels.Length;i++){
   Game.BeginLobbyLevel(i);yield return null;
   try{checks+=Game.VerifySeparateSceneLevel();levels++;}catch(Exception e){Check(false,"Level "+(i+1)+": "+e.Message);}
  }
  Game.BeginLobbyLevel(27);yield return null;Capture("separate-game-preview",false);
  Game.RequestUI(CampaignUICommand.Pause);yield return null;Game.RequestUI(CampaignUICommand.Home);transitions++;yield return Ready(true);Ownership(true);Check(Menu.Profile==profile&&Menu.Profile.playerName=="Scene QA","Profile not restored in menu");
  // Explicit selection, then return through pause during an in-flight move.
  Menu.Click(LobbyAction.Campaign);for(int i=0;i<3;i++)Menu.Click(LobbyAction.PreviousPage);yield return new WaitForSeconds(.22f);Click(LobbyAction.SelectLevel,27);transitions++;yield return Ready(false);Check(Game.CurrentLevel.id==28,"Selected level did not transfer between scenes");Game.StartSceneMoveVerification();yield return null;Game.RequestUI(CampaignUICommand.Pause);Game.RequestUI(CampaignUICommand.Home);transitions++;yield return Ready(true);Ownership(true);
  Menu.Click(LobbyAction.Events);yield return new WaitForSeconds(.22f);Click(LobbyAction.StartChallenge);transitions++;yield return Ready(false);Check(Game.ActiveMode==LobbyMode.Challenge,"Challenge mode lost in transition");yield return Game.PlayWitnessForLobbyTest();Check(Game.Victorious,"Challenge did not reach victory");Game.RequestUI(CampaignUICommand.Primary);transitions++;yield return Ready(true);Ownership(true);
  Menu.Click(LobbyAction.Endless);yield return new WaitForSeconds(.22f);Click(LobbyAction.StartEndless);transitions++;yield return Ready(false);int prior=Game.CurrentLevel.id;yield return Game.PlayWitnessForLobbyTest();Check(Game.Victorious&&PulseSceneFlow.EndlessCount==1,"Endless victory did not update session");Game.RequestUI(CampaignUICommand.Primary);yield return null;Check(Game.CurrentLevel.id!=prior&&Game.ActiveMode==LobbyMode.Endless,"Endless next level failed");Game.RequestUI(CampaignUICommand.Pause);Game.RequestUI(CampaignUICommand.Home);transitions++;yield return Ready(true);Ownership(true);
  Check(profileBefore==PlayerPrefs.GetString("PulseLobby.Profile.v1","")&&unlockedBefore==PlayerPrefs.GetInt("PulseCampaign.Variety2.Unlocked",1)&&currentBefore==PlayerPrefs.GetInt("PulseCampaign.Variety2.Current",1),"QA modified player saves");Finish();
 }
 void Log(string message,string stack,LogType type){if(type==LogType.Exception)failures.Add(message);}
 void Update(){if(!finished&&started>0&&Time.realtimeSinceStartup-started>360){failures.Add("Scene verification timed out");Finish();}}
 void Finish(){if(finished)return;finished=true;var report=new Report{success=failures.Count==0,checks=checks,levels=levels,transitions=transitions,failures=failures.ToArray()};File.WriteAllText(Path.Combine(Application.dataPath,"../../separate-scenes-verification.json"),JsonUtility.ToJson(report,true));Debug.Log("SEPARATE_SCENES_RUNTIME_VERIFIED "+report.success+" / "+checks+" checks / "+levels+" levels / "+transitions+" transitions");Application.Quit(report.success?0:1);}
 void OnDestroy(){Application.logMessageReceived-=Log;}
}

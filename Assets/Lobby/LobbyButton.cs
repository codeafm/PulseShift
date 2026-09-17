using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum LobbyAction {None,Play,Home,Levels,Hero,Achievements,Settings,Profile,Crystals,Resonance,Gift,Events,Tasks,Shop,Collection,Stats,Skins,Campaign,Challenge,Endless,Close,ClaimGift,ClaimTask,ClaimAchievement,BuySkin,BuyPortal,SkinHeroTab,SkinPortalTab,SkinPrevious,SkinNext,SaveName,ToggleMotion,StartChallenge,StartEndless,SelectLevel,NextPage,PreviousPage,Greet,WatchReward,ToggleVibration,ToggleFps,CycleQuality,Support,Privacy,PreviewSkin}
[RequireComponent(typeof(Button))]
public class LobbyButton:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler,ICanvasRaycastFilter {
 public PulseLobby lobby;public LobbyAction action;public int value;public bool primary;
 // Child artwork must never enlarge the Play button hit area over the hero.
 public bool IsRaycastLocationValid(Vector2 point,Camera eventCamera)=>action!=LobbyAction.Play||RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform,point,eventCamera);
 Button button;LobbyPlate plate;bool hover,down;float emphasis;Vector3 authoredScale=Vector3.one;
 void Awake(){authoredScale=transform.localScale;button=GetComponent<Button>();plate=GetComponent<LobbyPlate>();button.onClick.AddListener(Trigger);}
 public void Trigger(){if(!button||!button.interactable||!lobby)return;lobby.Click(action,value);if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);}
 void OnDisable(){hover=down=false;transform.localScale=authoredScale;}
 void Update(){if(!button)return;float target=button.interactable?(down?.963f:hover?1.028f:1):1;if(primary&&button.interactable&&!lobby.Profile.reducedMotion)target+=Mathf.Sin(Time.unscaledTime*1.7f)*.007f;transform.localScale=Vector3.Lerp(transform.localScale,authoredScale*target,1-Mathf.Exp(-Time.unscaledDeltaTime*22));emphasis=Mathf.MoveTowards(emphasis,hover||down?1:0,Time.unscaledDeltaTime*7);if(plate){if(!Mathf.Approximately(plate.emphasis,emphasis)){plate.emphasis=emphasis;plate.SetVerticesDirty();}plate.color=button.interactable?Color.white:new Color(.43f,.48f,.6f,.85f);}}
 public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=down=false;}public void OnPointerDown(PointerEventData e){if(e.button==PointerEventData.InputButton.Left)down=true;}public void OnPointerUp(PointerEventData e){down=false;}
}

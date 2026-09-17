using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum CampaignUICommand {None,Pulse,Wait,Undo,Freeze,Dash,Hint,Pause,Levels,Primary,Restart,SelectLevel,CloseLevels,PreviousPage,NextPage,Home}
[RequireComponent(typeof(Button))]
public class NeonActionButton:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IPointerDownHandler,IPointerUpHandler {
 public CampaignGame game;public CampaignUICommand command;public int value;
 bool hover,down;Button button;NeonGraphic frame;Color tint;
 void Awake(){button=GetComponent<Button>();frame=GetComponent<NeonGraphic>();if(frame)tint=frame.accent;button.onClick.AddListener(Trigger);}
 public void Trigger(){if(button&&button.interactable&&game)game.RequestUI(command,value);if(EventSystem.current)EventSystem.current.SetSelectedGameObject(null);}
 void Update(){if(!button)return;float s=button.interactable?(down?.94f:hover?1.055f:1):1;if(command==CampaignUICommand.Pulse&&button.interactable&&!down)s*=1+Mathf.Sin(Time.unscaledTime*2.1f)*.018f;transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*s,1-Mathf.Exp(-Time.unscaledDeltaTime*18));if(frame)frame.Tint(button.interactable?tint:new Color(.12f,.19f,.27f,.65f));}
 public void OnPointerEnter(PointerEventData e){hover=true;}public void OnPointerExit(PointerEventData e){hover=down=false;}public void OnPointerDown(PointerEventData e){down=true;}public void OnPointerUp(PointerEventData e){down=false;}
}

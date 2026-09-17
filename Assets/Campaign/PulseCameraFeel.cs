using UnityEngine;

// Screen-space reactions only: the Scene camera Transform and FOV remain untouched.
[RequireComponent(typeof(Camera))]
public class PulseCameraFeel:MonoBehaviour {
 [Range(0,1)] public float intensity=.6f;
 public bool reducedMotion;
 public float Amount{get;private set;}
 public float Clock{get;private set;}
 public Vector2 Focus{get;private set;}=new Vector2(.5f,.5f);
 CampaignGame game;Camera view;
 float nextGameSearch;
 void Awake(){view=GetComponent<Camera>();}
 public void Kick(float power,Vector3 point){if(!view)view=GetComponent<Camera>();Amount=Mathf.Max(Amount,Mathf.Clamp01(power));var p=view.WorldToViewportPoint(point);Focus=new Vector2(p.x,p.y);}
 public void ResetFeel(){Amount=0;}
 void Update(){if(!game&&Time.unscaledTime>=nextGameSearch){game=FindFirstObjectByType<CampaignGame>();nextGameSearch=Time.unscaledTime+1;}if(game&&game.MotionPaused)return;Clock+=Time.deltaTime;Amount=Mathf.MoveTowards(Amount,0,Time.deltaTime*1.9f);}
}

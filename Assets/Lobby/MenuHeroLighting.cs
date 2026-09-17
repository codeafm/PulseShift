using UnityEngine;

[DefaultExecutionOrder(1100)]
public class MenuHeroLighting:MonoBehaviour {
 public LobbyStage stage;public Transform hero;public Camera view;public Light overhead,rim,contact;
 [Range(0,8)]public float overheadIntensity=3.2f;
 [Range(0,4)]public float rimIntensity=.85f;
 [Range(0,5)]public float contactIntensity=1.35f;
 public bool animateLight=true;
 public Color keyColor=new Color(.76f,.87f,1),rimColor=new Color(1,.65f,.36f);
 void LateUpdate(){RefreshLighting();}
 public void RefreshLighting(){
  if(!hero||!view||!overhead||!rim||!contact)return;float scale=Mathf.Max(.45f,hero.lossyScale.x);var focus=hero.position+view.transform.up*.28f*scale;
  overhead.color=keyColor;rim.color=rimColor;
  if(stage)MenuHeroAura.Ensure(stage);
  var appearance=hero.GetComponent<SkinAppearance>();if(appearance)appearance.SetMotion(animateLight&&stage&&!stage.reducedMotion);
  overhead.transform.position=focus+view.transform.up*3.2f*scale-view.transform.forward*1.7f*scale-view.transform.right*.25f*scale;overhead.transform.LookAt(focus);overhead.range=8*scale;overhead.spotAngle=52;
  rim.transform.position=focus-view.transform.right*1.5f*scale+view.transform.up*.45f*scale+view.transform.forward*.5f*scale;rim.range=4.2f*scale;
  contact.transform.position=hero.position-view.transform.up*.06f*scale-view.transform.forward*.18f*scale;contact.range=3.1f*scale;
  float wave=animateLight&&stage&&!stage.reducedMotion?Mathf.Sin(Time.unscaledTime*2.15f):0;
  overhead.intensity=overheadIntensity*(1+wave*.045f);rim.intensity=rimIntensity;contact.intensity=contactIntensity*(1+wave*.13f);
 }
}

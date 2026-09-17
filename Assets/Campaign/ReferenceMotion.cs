using UnityEngine;

public class ReferenceMotion:MonoBehaviour {
 public string modelId;public bool menuActor;
 Transform visual,vortex;SkinnedMeshRenderer skin;CampaignGame game;
 float clock,reaction,death,victory,menuGesture;int menuGestureKind;bool moving;float moveProgress;Vector3 moveDirection;
 Renderer liquid;MaterialPropertyBlock liquidProperties;
 bool bound;float nextGameSearch;
 void Awake(){Bind();}
 void Bind(){
  if(!bound){visual=transform.Find("Reference visual");skin=GetComponentInChildren<SkinnedMeshRenderer>();vortex=visual?visual.Find("Vortex"):null;bound=visual!=null;}
  if(!menuActor&&!game&&Time.unscaledTime>=nextGameSearch){game=FindFirstObjectByType<CampaignGame>();nextGameSearch=Time.unscaledTime+1;}
 }
 public void Travel(float progress,Vector3 direction){moving=true;moveProgress=progress;moveDirection=direction;}
 public void Land(){moving=false;reaction=1;}
 public void Defeat(){moving=false;death=.001f;}
 public void Celebrate(){moving=false;victory=.001f;}
 public void MenuReact(int kind){Bind();moving=false;death=victory=reaction=0;menuGesture=.001f;menuGestureKind=Mathf.Abs(kind)%3;}
 public void ResetPose(){Bind();death=victory=reaction=menuGesture=0;moving=false;if(visual){visual.localPosition=Vector3.zero;visual.localScale=Vector3.one;visual.localRotation=Quaternion.identity;}}
 void LateUpdate(){Advance(menuActor?Time.unscaledDeltaTime:Time.deltaTime);}
 public void Advance(float dt){
  Bind();if(!visual||!menuActor&&game&&game.MotionPaused)return;dt=Mathf.Max(0,dt);clock+=dt;
  if(skin){float morph=menuActor?32:100;skin.SetBlendShapeWeight(0,Mathf.Max(0,Mathf.Sin(clock*2.1f))*morph);skin.SetBlendShapeWeight(1,Mathf.Max(0,-Mathf.Sin(clock*2.1f))*morph);}
  if(modelId=="Portal"){if(vortex){vortex.localRotation=Quaternion.identity;var surface=vortex.Find("Dimensional water • animated vortex");if(surface){liquid=surface.GetComponent<Renderer>();if(liquidProperties==null)liquidProperties=new MaterialPropertyBlock();liquid.GetPropertyBlock(liquidProperties);liquidProperties.SetFloat("_Clock",clock);liquid.SetPropertyBlock(liquidProperties);}}return;}
  if(modelId=="Crystal"){visual.localRotation=Quaternion.Euler(0,clock*24,0);visual.localPosition=Vector3.up*(.06f+Mathf.Sin(clock*2)*.045f);return;}
  if(menuActor&&menuGesture>0){
   menuGesture+=dt;float duration=menuGestureKind==2?1.35f:1.05f,t=Mathf.Clamp01(menuGesture/duration),arc=Mathf.Sin(t*Mathf.PI),wave=Mathf.Sin(t*Mathf.PI*6)*arc;
   float jump=(menuGestureKind==0?.18f:menuGestureKind==1?.21f:.24f)*arc;
   visual.localPosition=Vector3.up*jump;
   visual.localRotation=Quaternion.Euler(-arc*(menuGestureKind==2?10:5),wave*(menuGestureKind==1?17:11),-wave*(menuGestureKind==0?13:20));
   float squash=Mathf.Sin(t*Mathf.PI*2)*.055f;visual.localScale=new Vector3(1+squash,1-squash,1+squash);
   if(skin){skin.SetBlendShapeWeight(0,Mathf.Clamp01(arc+wave*.2f)*82);skin.SetBlendShapeWeight(1,Mathf.Clamp01(arc-wave*.2f)*58);}
   if(t>=1)ResetPose();return;
  }
  if(death>0){death+=dt;float t=Mathf.Clamp01(death/.7f);visual.localScale=Vector3.one*Mathf.Lerp(1,.08f,t);visual.localPosition=Vector3.up*(-t*.22f);visual.localRotation=Quaternion.Euler(0,0,t*130);return;}
  if(victory>0){victory+=dt;visual.localPosition=Vector3.up*(.1f+Mathf.Abs(Mathf.Sin(victory*6))*.18f);visual.localRotation=Quaternion.Euler(0,Mathf.Sin(victory*3)*16,Mathf.Sin(victory*6)*8);return;}
  float squish=0;if(reaction>0){reaction=Mathf.Max(0,reaction-dt*3.5f);squish=Mathf.Sin((1-reaction)*Mathf.PI*2)*reaction*.13f;}
  if(moving){float arc=Mathf.Sin(moveProgress*Mathf.PI),velocity=Mathf.Cos(moveProgress*Mathf.PI),stretch=arc*.085f;float landing=Mathf.Exp(-Mathf.Pow((moveProgress-.94f)/.075f,2))*.075f;visual.localScale=new Vector3(1-stretch*.42f+landing,1+stretch-landing,1-stretch*.42f+landing);visual.localRotation=Quaternion.Euler(-velocity*7-arc*5,moveDirection.x*(10+arc*8),-moveDirection.x*arc*8);visual.localPosition=Vector3.up*(arc*.045f);}
  else{visual.localScale=new Vector3(1+squish,1-squish,1+squish);visual.localPosition=Vector3.up*Mathf.Sin(clock*2.5f)*.027f;visual.localRotation=Quaternion.Euler(0,Mathf.Sin(clock*1.3f)*4,Mathf.Sin(clock*2)*2);}
 }
}

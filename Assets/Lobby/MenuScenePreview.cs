using UnityEngine;

// Edit-mode preview calls the same layout as the player. Never loads or writes a profile.
[ExecuteAlways,DefaultExecutionOrder(1200)]
public sealed class MenuScenePreview:MonoBehaviour {
 public PulseLobby lobby;
 public bool previewEnabled=true;
 public bool animatePreview=true;
 float previewClock,nextGesture=2;
 Vector3 posePosition,poseScale;Quaternion poseRotation;Transform animatedVisual;bool hasPose;
 SkinnedMeshRenderer previewMesh;float[] blendWeights;
 public void Animate(float dt){
  if(Application.isPlaying||!previewEnabled||!animatePreview||!lobby||!lobby.stage||lobby.stage.reducedMotion)return;
  var stage=lobby.stage;if(!stage.spirit)return;var motion=stage.spirit.GetComponent<ReferenceMotion>();if(!motion)return;
  if(!hasPose){animatedVisual=stage.spirit.Find("Reference visual");if(!animatedVisual)return;posePosition=animatedVisual.localPosition;poseRotation=animatedVisual.localRotation;poseScale=animatedVisual.localScale;hasPose=true;}
  if(blendWeights==null){previewMesh=stage.spirit.GetComponentInChildren<SkinnedMeshRenderer>();if(previewMesh&&previewMesh.sharedMesh){blendWeights=new float[previewMesh.sharedMesh.blendShapeCount];for(int i=0;i<blendWeights.Length;i++)blendWeights[i]=previewMesh.GetBlendShapeWeight(i);}}
  previewClock+=dt;if(previewClock>=nextGesture){motion.MenuReact((int)(previewClock/5)%3);nextGesture=previewClock+5;}
  motion.Advance(dt);MenuHeroAura.Ensure(stage).Advance(dt);
 }
 public void RestorePose(){
  if(hasPose&&animatedVisual){var motion=lobby&&lobby.stage&&lobby.stage.spirit?lobby.stage.spirit.GetComponent<ReferenceMotion>():null;if(motion)motion.ResetPose();animatedVisual.localPosition=posePosition;animatedVisual.localRotation=poseRotation;animatedVisual.localScale=poseScale;}
  hasPose=false;
  if(previewMesh&&blendWeights!=null)for(int i=0;i<blendWeights.Length;i++)previewMesh.SetBlendShapeWeight(i,blendWeights[i]);blendWeights=null;
 }
 public void Refresh(){
  if(Application.isPlaying||!previewEnabled||!lobby||!lobby.standaloneScene||!lobby.stage||!lobby.stage.view||!lobby.safeRoot)return;
  var stage=lobby.stage;
  Canvas.ForceUpdateCanvases();lobby.Layout();stage.Frame();
  var light=stage.GetComponent<MenuHeroLighting>();if(light)light.RefreshLighting();
  var aura=MenuHeroAura.Ensure(stage);aura.Advance(0);
 }
 void Update(){Refresh();}
 void OnDisable(){RestorePose();Unsubscribe();}
 void OnEnable(){Subscribe();}
 void Subscribe(){
 #if UNITY_EDITOR
  UnityEditor.EditorApplication.update-=EditorTick;UnityEditor.EditorApplication.update+=EditorTick;
  UnityEditor.SceneManagement.EditorSceneManager.sceneSaving-=BeforeSave;UnityEditor.SceneManagement.EditorSceneManager.sceneSaving+=BeforeSave;
  editorTime=UnityEditor.EditorApplication.timeSinceStartup;
 #endif
 }
 void Unsubscribe(){
 #if UNITY_EDITOR
  UnityEditor.EditorApplication.update-=EditorTick;UnityEditor.SceneManagement.EditorSceneManager.sceneSaving-=BeforeSave;
 #endif
 }
 #if UNITY_EDITOR
 double editorTime;
 void BeforeSave(UnityEngine.SceneManagement.Scene scene,string path){if(scene==gameObject.scene)RestorePose();}
 void EditorTick(){
  double now=UnityEditor.EditorApplication.timeSinceStartup;float dt=(float)(now-editorTime);
  if(Application.isPlaying||UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode||Application.isBatchMode){RestorePose();editorTime=now;return;}
  if(dt<1f/30)return;editorTime=now;
  if(!animatePreview||!previewEnabled){RestorePose();return;}
  Animate(Mathf.Min(dt,.05f));UnityEditor.EditorApplication.QueuePlayerLoopUpdate();UnityEditor.SceneView.RepaintAll();
 }
 #endif
}

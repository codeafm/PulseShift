using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using PulseCampaign;

public partial class PulseLobby {
 [Header("Standalone menu scene")]
 [Tooltip("Use the saved menu objects without recreating or rearranging them.")]
 public bool standaloneScene;
 public string playCaption="ИГРАТЬ";
 public Campaign MenuData=>game?game.Data:PulseSceneFlow.Data;
 public int MenuUnlocked=>game?game.Unlocked:PulseSceneFlow.Unlocked;
 public int MenuStars(int id)=>game?game.SavedStars(id):PulseSceneFlow.Stars(id);
 void Start(){
  if(!standaloneScene)return;
  // Legacy regression still uses the retained integrated scene in desktop QA builds only.
  var args=Environment.GetCommandLineArgs();
  if(args.Any(a=>a=="-campaignVerify"||a=="-lobbyVerify"||a=="-mobileVerify")&&Application.CanStreamedLevelBeLoaded("PULSESHIFT_Integrated")){SceneManager.LoadScene("PULSESHIFT_Integrated");return;}
  Application.targetFrameRate=60;Initialize(PulseSceneFlow.Testing);Open();
 }
 public Texture2D CaptureMenuForVerification()=>CaptureFrame(stage.view,canvas,540,1170);
}

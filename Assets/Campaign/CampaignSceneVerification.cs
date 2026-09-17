using UnityEngine;
using System;
using PulseCampaign;

public partial class CampaignGame {
 public Texture2D CaptureSceneForVerification()=>CaptureVerificationFrame();
 public void StartSceneMoveVerification(){if(testing)Act(level.solution[0]);}
 public int VerifySeparateSceneLevel(){
  if(!testing)throw new InvalidOperationException("QA only");int checks=0;Physics.SyncTransforms();
  foreach(var island in islands){var point=gameCamera.WorldToScreenPoint(island.transform.position);if(!MobileViewport.BoardPixels.Contains(point)||!Physics.Raycast(gameCamera.ScreenPointToRay(point),out var hit)||hit.collider.GetComponentInParent<CampaignIsland>()!=island)throw new Exception("Island is not tappable: "+island.index);checks++;}
  foreach(var command in level.solution){var result=Rules.Apply(level,state,command,out var next,out var reason);if(result==Result.Dead||result==Result.Invalid)throw new Exception(reason);state=next;turns++;SyncVisuals(true);VerifyStableSurfaces();checks++;}
  if(!Rules.Won(level,state))throw new Exception("Witness did not win");return checks+1;
 }
}

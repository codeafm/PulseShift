using UnityEngine;
using UnityEngine.EventSystems;
using PulseCampaign;

public partial class CampaignGame {
 Vector2 movePointerStart;
 int movePointerId=int.MinValue;
 bool movePointerActive;

 void ProcessMoveInput(){
  if(Input.touchCount>0||movePointerId>=0){
   if(!movePointerActive){
    for(int i=0;i<Input.touchCount;i++){
     var touch=Input.GetTouch(i);
     if(touch.phase!=TouchPhase.Began)continue;
     BeginMovePointer(touch.position,touch.fingerId,EventSystem.current&&EventSystem.current.IsPointerOverGameObject(touch.fingerId));
     break;
    }
   }
   if(movePointerActive&&movePointerId>=0){
    bool found=false;
    for(int i=0;i<Input.touchCount;i++){
     var touch=Input.GetTouch(i);
     if(touch.fingerId!=movePointerId)continue;
     found=true;
     if(touch.phase==TouchPhase.Ended)CompleteMovePointer(touch.position);
     else if(touch.phase==TouchPhase.Canceled)ResetMovePointer();
     break;
    }
    if(!found)ResetMovePointer();
   }
   return;
  }
  if(Input.GetMouseButtonDown(0))BeginMovePointer(Input.mousePosition,-1,EventSystem.current&&EventSystem.current.IsPointerOverGameObject());
  if(movePointerActive&&movePointerId==-1&&Input.GetMouseButtonUp(0))CompleteMovePointer(Input.mousePosition);
 }

 void BeginMovePointer(Vector2 position,int pointerId,bool overUi){
  if(overUi)return;
  movePointerStart=position;movePointerId=pointerId;movePointerActive=true;
 }

 void CompleteMovePointer(Vector2 position){
  Vector2 delta=position-movePointerStart;
  ResetMovePointer();
  float swipeThreshold=Mathf.Clamp(Screen.dpi>0?Screen.dpi*.12f:36,28,64);
  if(delta.magnitude>=swipeThreshold){TrySwipeMove(delta);return;}
  TryTapMove(position);
 }

 void ResetMovePointer(){movePointerActive=false;movePointerId=int.MinValue;}

 void TrySwipeMove(Vector2 swipe){
  var kind=dashSelected?ActionKind.Dash:ActionKind.Move;
  Vector2 origin=gameCamera.WorldToScreenPoint(Position(state.cell));
  Vector2 direction=swipe.normalized;
  int best=-1;float bestScore=.55f;
  for(int i=0;i<islands.Length;i++){
   if(i==state.cell||Rules.Apply(level,state,new Command(kind,i),out _,out _)==Result.Invalid)continue;
   Vector2 toward=(Vector2)gameCamera.WorldToScreenPoint(Position(i))-origin;
   if(toward.sqrMagnitude<1)continue;
   float alignment=Vector2.Dot(direction,toward.normalized);
   // Prefer the island most closely aligned with the finger, then the nearer one.
   float score=alignment-toward.magnitude*.00002f;
   if(score>bestScore){bestScore=score;best=i;}
  }
  if(best>=0)Act(new Command(kind,best));
  else message="В этом направлении нет доступного острова";
 }

 void TryTapMove(Vector2 position){
  var kind=dashSelected?ActionKind.Dash:ActionKind.Move;
  CampaignIsland tapped=null;float nearestHit=float.MaxValue;
  foreach(var hit in Physics.RaycastAll(gameCamera.ScreenPointToRay(position))){
   var island=hit.collider.GetComponentInParent<CampaignIsland>();
   if(island&&hit.distance<nearestHit){tapped=island;nearestHit=hit.distance;}
  }
  if(tapped&&Rules.Apply(level,state,new Command(kind,tapped.index),out _,out _)!=Result.Invalid){Act(new Command(kind,tapped.index));return;}
  // Small islands are hard to hit with a finger, so accept a nearby valid island too.
  int best=-1;float bestDistance=Mathf.Pow(Mathf.Clamp(Screen.dpi>0?Screen.dpi*.24f:72,56,112),2);
  for(int i=0;i<islands.Length;i++){
   if(i==state.cell||Rules.Apply(level,state,new Command(kind,i),out _,out _)==Result.Invalid)continue;
   Vector2 point=gameCamera.WorldToScreenPoint(Position(i));float distance=(point-position).sqrMagnitude;
   if(distance<bestDistance){bestDistance=distance;best=i;}
  }
  if(best>=0)Act(new Command(kind,best));
 }
}

using System;
using UnityEngine;
using UnityEngine.UI;

// Keeps the page and its render targets alive throughout a slide. One latest
// request is retained when the player swipes faster than the animation finishes.
public sealed class SkinCarousel:MonoBehaviour {
 PulseLobby owner;RawImage current,incoming;bool portal,pendingPortal,pending;
 int index,pendingIndex,pendingDirection=1,direction=1;float elapsed;
 public Action<bool,int> Changed;public Action Settled;
 public bool IsMoving=>incoming;
 public int RequestedIndex=>pending?pendingIndex:index;
 public bool RequestedPortal=>pending?pendingPortal:portal;
 public void Initialize(PulseLobby lobby,bool category,int skin){owner=lobby;portal=category;index=skin;current=Create(category,skin);}
 RawImage Create(bool category,int skin){
  var rect=new GameObject("Carousel model",typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(transform,false);rect.sizeDelta=((RectTransform)transform).rect.size;
  rect.gameObject.layer=gameObject.layer;var image=rect.gameObject.AddComponent<RawImage>();image.color=Color.white;
  rect.gameObject.AddComponent<SkinShowcase>().Initialize(owner,image,category,skin,(category?PulseLobby.PortalColors:PulseLobby.SkinColors)[skin]);return image;
 }
 public void Select(bool category,int skin,int sign){
  skin=Mathf.Clamp(skin,0,4);
  if(IsMoving){pending=category!=portal||skin!=index;pendingPortal=category;pendingIndex=skin;pendingDirection=sign;return;}
  if(category==portal&&skin==index)return;
  current.GetComponent<SkinShowcase>().Freeze();
  incoming=Create(category,skin);portal=category;index=skin;direction=sign<0?-1:1;elapsed=0;
  Position(0);Changed?.Invoke(portal,index);
 }
 void Position(float p){float distance=((RectTransform)transform).rect.width;
  current.rectTransform.anchoredPosition=new Vector2(-direction*distance*p,0);
  incoming.rectTransform.anchoredPosition=new Vector2(direction*distance*(1-p),0);
 }
 void Update(){Advance(Time.unscaledDeltaTime);}
 public void Advance(float dt){
  if(!incoming)return;elapsed+=Mathf.Max(0,dt);float t=Mathf.Clamp01(elapsed/.48f);
  // Quintic easing has zero velocity and acceleration at both ends.
  Position(t*t*t*(t*(t*6-15)+10));
  if(t<1)return;current.gameObject.SetActive(false);if(Application.isPlaying)Destroy(current.gameObject);else DestroyImmediate(current.gameObject);
  current=incoming;incoming=null;current.rectTransform.anchoredPosition=Vector2.zero;
  if(pending){pending=false;Select(pendingPortal,pendingIndex,pendingDirection);}
  if(!incoming)Settled?.Invoke();
 }
}

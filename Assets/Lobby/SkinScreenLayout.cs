using UnityEngine;
using UnityEngine.UI;

// Converts canvas corners through the authored menu hierarchy, including safe-area scaling.
public sealed class SkinScreenLayout:MonoBehaviour {
 Canvas canvas;RectTransform composition;RawImage background;
 public void Initialize(Canvas owner,RectTransform content,RawImage backdrop){canvas=owner;composition=content;background=backdrop;Refresh();}
 void LateUpdate(){Refresh();}
 void Refresh(){
  if(!canvas||!composition)return;var rect=(RectTransform)transform;var parent=(RectTransform)rect.parent;
  var corners=new Vector3[4];((RectTransform)canvas.transform).GetWorldCorners(corners);
  var lo=parent.InverseTransformPoint(corners[0]);var hi=parent.InverseTransformPoint(corners[2]);
  rect.position=(corners[0]+corners[2])*.5f;rect.sizeDelta=new Vector2(hi.x-lo.x,hi.y-lo.y);
  background.rectTransform.sizeDelta=rect.sizeDelta;background.rectTransform.anchoredPosition=Vector2.zero;
  if(background.texture){float a=rect.rect.width/rect.rect.height,t=background.texture.width/(float)background.texture.height;
   background.uvRect=a<t?new Rect((1-a/t)*.5f,0,a/t,1):new Rect(0,(1-t/a)*.5f,1,t/a);}
  float scale=Mathf.Min(rect.rect.width/540,rect.rect.height/960);composition.localScale=Vector3.one*scale;
 }
}

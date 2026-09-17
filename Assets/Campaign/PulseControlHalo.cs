using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Soft native Canvas light; not dependent on scene post processing or HDR support.
[RequireComponent(typeof(CanvasRenderer))]
public class PulseControlHalo:MaskableGraphic {
 public bool circular;
 public static void Attach(RectTransform button,bool circle){
  if(button.Find("Atmospheric rim light"))return;
  var g=new GameObject("Atmospheric rim light",typeof(RectTransform));var r=(RectTransform)g.transform;
  r.SetParent(button,false);r.SetAsFirstSibling();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*-10;r.offsetMax=Vector2.one*10;
  var halo=g.AddComponent<PulseControlHalo>();halo.circular=circle;halo.raycastTarget=false;
 }
 protected override void OnPopulateMesh(VertexHelper vh){
  vh.Clear();var rect=rectTransform.rect;float x=rect.width*.5f-14,y=rect.height*.5f-14;
  var points=new List<Vector2>();
  if(circular){float r=Mathf.Min(x,y);for(int i=0;i<80;i++){float a=i*Mathf.PI/40;points.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r);}}
  else{float cut=Mathf.Min(15,Mathf.Min(x,y)*.26f);points.AddRange(new[]{new Vector2(-x+cut,-y),new Vector2(x-cut,-y),new Vector2(x,-y+cut),new Vector2(x,y-cut),new Vector2(x-cut,y),new Vector2(-x+cut,y),new Vector2(-x,y-cut),new Vector2(-x,-y+cut)});}
  for(int band=10;band>=1;band--){float width=band*1.65f;var c=new Color(.01f,.63f,1,band==1?.8f:.045f*(1-band/12f));
   for(int i=0;i<points.Count;i++){var a=points[i];var b=points[(i+1)%points.Count];var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;int k=vh.currentVertCount;
    vh.AddVert(a-n,c,Vector2.zero);vh.AddVert(a+n,c,Vector2.zero);vh.AddVert(b+n,c,Vector2.zero);vh.AddVert(b-n,c,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);
   }
  }
 }
}

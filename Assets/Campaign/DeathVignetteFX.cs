using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class DeathVignetteFX:MaskableGraphic {
 public float intensity;
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();Rect r=rectTransform.rect;float pulse=.88f+.12f*Mathf.Sin(Time.unscaledTime*4.2f);Color red=new Color(.9f,.015f,.025f,intensity*pulse);
  const int layers=9;for(int i=0;i<layers;i++){float t=i/(float)layers,edge=18+i*16,alpha=(1-t)*(1-t)*.16f;Color c=new Color(red.r,red.g,red.b,red.a*alpha);Quad(v,r.xMin,r.yMin,r.xMin+edge,r.yMax,c);Quad(v,r.xMax-edge,r.yMin,r.xMax,r.yMax,c);Quad(v,r.xMin+edge,r.yMax-edge,r.xMax-edge,r.yMax,c);Quad(v,r.xMin+edge,r.yMin,r.xMax-edge,r.yMin+edge,c);}
 }
 void Update(){if(intensity>0)SetVerticesDirty();}
 static void Quad(VertexHelper v,float x0,float y0,float x1,float y1,Color c){int k=v.currentVertCount;v.AddVert(new Vector2(x0,y0),c,Vector2.zero);v.AddVert(new Vector2(x0,y1),c,Vector2.zero);v.AddVert(new Vector2(x1,y1),c,Vector2.zero);v.AddVert(new Vector2(x1,y0),c,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
}

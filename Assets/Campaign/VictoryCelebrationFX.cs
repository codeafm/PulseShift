using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class VictoryCelebrationFX:MaskableGraphic {
 static readonly Vector2[] centers={new(-185,286),new(178,300),new(-218,92),new(218,112),new(0,355)};
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();float time=Time.unscaledTime;
  for(int burst=0;burst<centers.Length;burst++){
   float phase=Mathf.Repeat(time*.42f+burst*.23f,1),radius=Mathf.Sin(phase*Mathf.PI)*58+8;
   float fade=1-Mathf.Clamp01(phase);Color tint=burst%2==0?new Color(.05f,.8f,1,fade*.9f):new Color(1,.64f,.08f,fade*.9f);
   for(int ray=0;ray<12;ray++){float a=ray*Mathf.PI/6+burst*.37f;Vector2 d=new(Mathf.Cos(a),Mathf.Sin(a));Line(v,centers[burst]+d*radius*.35f,centers[burst]+d*radius,2.2f,tint);Diamond(v,centers[burst]+d*radius,3.4f+4*(1-phase),tint);}
  }
  for(int i=0;i<28;i++){
   float seed=i*17.17f,travel=Mathf.Repeat(time*(.16f+(i%5)*.018f)+i*.137f,1);float x=Mathf.Sin(seed)*235,y=-360+travel*760;
   float pulse=.35f+.65f*Mathf.Abs(Mathf.Sin(time*3+seed));Color tint=i%3==0?new Color(1,.72f,.18f,pulse):new Color(.12f,.75f,1,pulse);
   Diamond(v,new Vector2(x,y),2.2f+(i%4),tint);
  }
 }
 void Update(){if(isActiveAndEnabled)SetVerticesDirty();}
 static void Diamond(VertexHelper v,Vector2 p,float r,Color c){int k=v.currentVertCount;v.AddVert(p+Vector2.up*r,c,Vector2.zero);v.AddVert(p+Vector2.right*r*.55f,c,Vector2.zero);v.AddVert(p+Vector2.down*r,c,Vector2.zero);v.AddVert(p+Vector2.left*r*.55f,c,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
 static void Line(VertexHelper v,Vector2 a,Vector2 b,float width,Color c){Vector2 d=(b-a).normalized,n=new(-d.y,d.x);n*=width*.5f;int k=v.currentVertCount;v.AddVert(a-n,c,Vector2.zero);v.AddVert(a+n,c,Vector2.zero);v.AddVert(b+n,c,Vector2.zero);v.AddVert(b-n,c,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}
}

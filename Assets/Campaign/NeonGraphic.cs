using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public enum NeonSymbol {Panel,Disc,Pulse,Chevron,Wait,Undo,Pause,Grid,Crystal,Star,Hint,Freeze,Next}
[RequireComponent(typeof(CanvasRenderer))]
public class NeonGraphic:MaskableGraphic {
 public NeonSymbol symbol;
 public Color accent=new Color(.025f,.72f,1,1);
 public bool filled=true,spin;
 public float phase;
 public Color glass=new Color(.012f,.032f,.078f,.94f);
 string Art=>symbol==NeonSymbol.Disc&&rectTransform.rect.width<30?null:symbol==NeonSymbol.Panel&&rectTransform.rect.width/Mathf.Max(1,rectTransform.rect.height)<1.5f?"Square":CinematicUI.Neon(symbol,filled);
 public override Texture mainTexture=>CinematicUI.Texture(Art)?CinematicUI.Texture(Art):base.mainTexture;
 protected override void OnPopulateMesh(VertexHelper v){
  var tint=color;if(symbol!=NeonSymbol.Crystal&&symbol!=NeonSymbol.Star)tint.a*=accent.a;
  if(CinematicUI.Draw(v,rectTransform.rect,tint,Art,Art=="Plate",symbol==NeonSymbol.Pulse?phase:0))return;
  v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height,r=Mathf.Min(w,h)*.43f;
  if(symbol==NeonSymbol.Panel||symbol==NeonSymbol.Disc){
   var p=new List<Vector2>();
   if(symbol==NeonSymbol.Disc)for(int i=0;i<64;i++){float a=i*Mathf.PI/32;p.Add(new Vector2(Mathf.Cos(a)*r,Mathf.Sin(a)*r));}
   else{float x=w*.5f-4,y=h*.5f-4,c=Mathf.Min(16,Mathf.Min(x,y)*.32f);p.AddRange(new[]{new Vector2(-x+c,-y),new Vector2(x-c,-y),new Vector2(x,-y+c),new Vector2(x,y-c),new Vector2(x-c,y),new Vector2(-x+c,y),new Vector2(-x,y-c),new Vector2(-x,-y+c)});}
   if(filled)Fill(v,p,glass);
   for(int layer=4;layer>=1;layer--){var c=accent;c.a*=.028f*(5-layer);Stroke(v,p,layer*2.5f,c,true);}
   Stroke(v,p,1.3f,accent,true);return;
  }
  var aColor=accent;
  if(symbol==NeonSymbol.Pulse){for(int j=0;j<2;j++){var p=new List<Vector2>();for(int i=0;i<70;i++){float t=i/69f,a=t*8.3f+j*Mathf.PI+phase;p.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*(1-t*.95f));}Stroke(v,p,Mathf.Max(2,r*.11f),aColor,false);}return;}
  if(symbol==NeonSymbol.Crystal){Vector2 top=new Vector2(0,r),bottom=new Vector2(0,-r),left=new Vector2(-r*.44f,0),right=new Vector2(r*.44f,0);Fill(v,new[]{bottom,right,top,left},new Color(1,.38f,.018f,1));Fill(v,new[]{bottom,Vector2.zero,top,left},new Color(1,.71f,.12f,1));Stroke(v,new[]{bottom,right,top,left},1.5f,new Color(1,.92f,.45f,1),true);Stroke(v,new[]{left,Vector2.zero,right},1.2f,new Color(1,.94f,.6f,1),false);return;}
  if(symbol==NeonSymbol.Star){var p=new List<Vector2>();for(int i=0;i<10;i++){float a=Mathf.PI/2+i*Mathf.PI/5;p.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*(i%2==0?1:.45f));}Fill(v,p,aColor);Stroke(v,p,1.3f,new Color(1,.9f,.5f,accent.a),true);return;}
  if(symbol==NeonSymbol.Pause){Stroke(v,new[]{new Vector2(-r*.36f,-r*.68f),new Vector2(-r*.36f,r*.68f)},r*.3f,aColor,false);Stroke(v,new[]{new Vector2(r*.36f,-r*.68f),new Vector2(r*.36f,r*.68f)},r*.3f,aColor,false);return;}
  if(symbol==NeonSymbol.Grid){for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++){var p=new Vector2(x*r*.65f,y*r*.65f);Fill(v,new[]{p+new Vector2(-r*.17f,-r*.17f),p+new Vector2(r*.17f,-r*.17f),p+new Vector2(r*.17f,r*.17f),p+new Vector2(-r*.17f,r*.17f)},aColor);}return;}
  if(symbol==NeonSymbol.Chevron){for(int i=0;i<2;i++)Stroke(v,new[]{new Vector2(-r*.7f,-r*.32f+i*r*.62f),new Vector2(0,r*.2f+i*r*.62f),new Vector2(r*.7f,-r*.32f+i*r*.62f)},r*.22f,aColor,false);return;}
  if(symbol==NeonSymbol.Wait){Stroke(v,new[]{new Vector2(-r*.62f,r*.85f),new Vector2(r*.62f,r*.85f),new Vector2(r*.5f,r*.42f),new Vector2(-r*.45f,-r*.48f),new Vector2(-r*.62f,-r*.85f),new Vector2(r*.62f,-r*.85f),new Vector2(r*.5f,-r*.42f),new Vector2(-r*.45f,r*.48f),new Vector2(-r*.62f,r*.85f)},r*.105f,aColor,false);return;}
  if(symbol==NeonSymbol.Freeze){for(int i=0;i<6;i++){float a=i*Mathf.PI/3;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),n=new Vector2(-d.y,d.x);Stroke(v,new[]{Vector2.zero,d*r},r*.08f,aColor,false);Stroke(v,new[]{d*r*.55f+n*r*.2f,d*r*.73f,d*r*.55f-n*r*.2f},r*.07f,aColor,false);}return;}
  if(symbol==NeonSymbol.Undo){var p=new List<Vector2>();for(int i=0;i<40;i++){float a=-1.4f+i/39f*4.3f;p.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*.73f);}Stroke(v,p,r*.14f,aColor,false);Stroke(v,new[]{new Vector2(-r*.92f,r*.73f),p[p.Count-1],new Vector2(-r*.24f,r*.31f)},r*.14f,aColor,false);return;}
  if(symbol==NeonSymbol.Next){Stroke(v,new[]{new Vector2(-r*.4f,-r*.65f),new Vector2(r*.4f,0),new Vector2(-r*.4f,r*.65f)},r*.15f,aColor,false);return;}
  var q=new List<Vector2>();for(int i=0;i<48;i++){float a=i*Mathf.PI/24;q.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*.7f);}Stroke(v,q,r*.07f,aColor,true);Stroke(v,new[]{new Vector2(0,-r),new Vector2(0,r)},r*.06f,aColor,false);Stroke(v,new[]{new Vector2(-r,0),new Vector2(r,0)},r*.06f,aColor,false);
 }
 void Fill(VertexHelper v,IList<Vector2> p,Color c){int b=v.currentVertCount;Vector2 center=Vector2.zero;foreach(var x in p)center+=x;center/=p.Count;v.AddVert(center,c*color,Vector2.zero);foreach(var x in p)v.AddVert(x,c*color,Vector2.zero);for(int i=0;i<p.Count;i++)v.AddTriangle(b,b+1+i,b+1+(i+1)%p.Count);}
 void Stroke(VertexHelper v,IList<Vector2> p,float width,Color c,bool closed){int count=closed?p.Count:p.Count-1;for(int i=0;i<count;i++){Vector2 a=p[i],b=p[(i+1)%p.Count],d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;int k=v.currentVertCount;v.AddVert(a-n,c*color,Vector2.zero);v.AddVert(a+n,c*color,Vector2.zero);v.AddVert(b+n,c*color,Vector2.zero);v.AddVert(b-n,c*color,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}}
 void Update(){if(spin){phase+=Time.unscaledDeltaTime*.35f;SetVerticesDirty();}}
 public void Tint(Color c){if(accent!=c){accent=c;SetVerticesDirty();}}
}

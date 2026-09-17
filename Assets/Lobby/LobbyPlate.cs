using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[RequireComponent(typeof(CanvasRenderer))]
public class LobbyPlate:Image {
 public Color accent=new Color(.15f,.62f,1),top=new Color(.055f,.09f,.22f,.96f),bottom=new Color(.012f,.02f,.065f,.97f);
 public bool primary;public float emphasis;
 [Tooltip("Invisible but clickable overlay for an icon already painted into a panel.")]
 public bool hitAreaOnly;
 public bool clean;
 string Art=>primary?"PrimaryPlate":rectTransform.rect.width/Mathf.Max(1,rectTransform.rect.height)<1.5f?"Square":"Plate";
 public override Texture mainTexture=>clean?Texture2D.whiteTexture:sprite?base.mainTexture:CinematicUI.Texture(Art)?CinematicUI.Texture(Art):base.mainTexture;
 protected override void OnPopulateMesh(VertexHelper v){
  if(hitAreaOnly){v.Clear();var r=rectTransform.rect;v.AddVert(new Vector3(r.xMin,r.yMin),Color.clear,Vector2.zero);v.AddVert(new Vector3(r.xMin,r.yMax),Color.clear,Vector2.up);v.AddVert(new Vector3(r.xMax,r.yMax),Color.clear,Vector2.one);v.AddVert(new Vector3(r.xMax,r.yMin),Color.clear,Vector2.right);v.AddTriangle(0,1,2);v.AddTriangle(0,2,3);return;}
  if(sprite){base.OnPopulateMesh(v);return;}
  if(!clean&&CinematicUI.Draw(v,rectTransform.rect,color,Art,Art!="Square"))return;
  v.Clear();float x=rectTransform.rect.width*.5f-2,y=rectTransform.rect.height*.5f-2,c=Mathf.Min(primary?20:11,y*.35f);
  var points=new[]{new Vector2(-x+c,-y),new Vector2(x-c,-y),new Vector2(x,-y+c),new Vector2(x,y-c),new Vector2(x-c,y),new Vector2(-x+c,y),new Vector2(-x,y-c),new Vector2(-x,-y+c)};
  for(int i=5;i>=1;i--)Stroke(v,points,i*2.2f,new Color(accent.r,accent.g,accent.b,(primary?.035f:.012f)*(6-i)),true);
  int start=v.currentVertCount;v.AddVert(Vector3.zero,Color.Lerp(bottom,top,.5f)*color,Vector2.zero);
  foreach(var p in points)v.AddVert(p,Color.Lerp(bottom,top,Mathf.InverseLerp(-y,y,p.y))*color,Vector2.zero);
  for(int i=0;i<points.Length;i++)v.AddTriangle(start,start+1+i,start+1+(i+1)%points.Length);
  Stroke(v,points,primary?2:1,new Color(accent.r,accent.g,accent.b,.7f+emphasis*.3f),true);
  if(primary){var inset=new Vector2[8];for(int i=0;i<8;i++)inset[i]=points[i]*new Vector2((x-6)/x,(y-6)/y);Stroke(v,inset,1.1f,new Color(.35f,.9f,1,.65f),true);}
  Stroke(v,new[]{new Vector2(-x+c+5,y-1),new Vector2(-x+c+30,y-1)},2,new Color(.55f,.95f,1,.9f),false);
 }
 void Stroke(VertexHelper v,IList<Vector2> p,float width,Color tint,bool closed){for(int i=0;i<(closed?p.Count:p.Count-1);i++){var a=p[i];var b=p[(i+1)%p.Count];var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*width*.5f;int k=v.currentVertCount;v.AddVert(a-n,tint*color,Vector2.zero);v.AddVert(a+n,tint*color,Vector2.zero);v.AddVert(b+n,tint*color,Vector2.zero);v.AddVert(b-n,tint*color,Vector2.zero);v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);}}
}

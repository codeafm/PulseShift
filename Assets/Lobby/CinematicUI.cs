using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// Native Canvas graphics backed by separately modelled/rendered Blender elements.
// Text, layout, navigation, pointer targets and press animations remain native UI.
public static class CinematicUI {
 static readonly Dictionary<string,Texture2D> textures=new();
 public static Texture2D Texture(string name){if(string.IsNullOrEmpty(name))return null;if(!textures.TryGetValue(name,out var t)){t=Resources.Load<Texture2D>("CinematicUI/"+name);textures[name]=t;}return t;}
 public static string Neon(NeonSymbol symbol,bool filled){switch(symbol){case NeonSymbol.Panel:return "Plate";case NeonSymbol.Disc:return filled?"Disc":null;case NeonSymbol.Pulse:return "Pulse";case NeonSymbol.Crystal:return "Crystal";case NeonSymbol.Pause:return "Pause";case NeonSymbol.Wait:return "Wait";case NeonSymbol.Undo:return "Undo";case NeonSymbol.Star:return "Star";default:return null;}}
 public static bool Draw(VertexHelper v,Rect rect,Color tint,string name,bool sliced=false,float angle=0){
  var texture=Texture(name);if(!texture)return false;v.Clear();
  if(sliced){
   float border=Mathf.Min(26,Mathf.Min(rect.width,rect.height)*.26f);
   float[] x={rect.xMin,rect.xMin+border,rect.xMax-border,rect.xMax};float[] y={rect.yMin,rect.yMin+border,rect.yMax-border,rect.yMax};
   // Exclude the transparent studio margin so a Button's visible frame matches its hit rectangle.
   float[] u={.05f,.15f,.85f,.95f},w={.065f,.35f,.65f,.935f};
   for(int row=0;row<3;row++)for(int col=0;col<3;col++)Quad(v,x[col],y[row],x[col+1],y[row+1],u[col],w[row],u[col+1],w[row+1],tint,0);
  }else if(name=="Square")Quad(v,rect.xMin,rect.yMin,rect.xMax,rect.yMax,.065f,.085f,.935f,.915f,tint,angle);
  else if(name=="Disc")Quad(v,rect.xMin,rect.yMin,rect.xMax,rect.yMax,.05f,.05f,.95f,.95f,tint,angle);
  else Quad(v,rect.xMin,rect.yMin,rect.xMax,rect.yMax,0,0,1,1,tint,angle);
  return true;
 }
 static void Quad(VertexHelper v,float x0,float y0,float x1,float y1,float u0,float v0,float u1,float v1,Color c,float angle){
  int k=v.currentVertCount;var points=new[]{new Vector2(x0,y0),new Vector2(x1,y0),new Vector2(x1,y1),new Vector2(x0,y1)};var uv=new[]{new Vector2(u0,v0),new Vector2(u1,v0),new Vector2(u1,v1),new Vector2(u0,v1)};
  for(int i=0;i<4;i++){var p=points[i];if(angle!=0)p=new Vector2(p.x*Mathf.Cos(angle)-p.y*Mathf.Sin(angle),p.x*Mathf.Sin(angle)+p.y*Mathf.Cos(angle));v.AddVert(p,c,uv[i]);}v.AddTriangle(k,k+1,k+2);v.AddTriangle(k,k+2,k+3);
 }
}

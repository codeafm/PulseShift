using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
public enum LobbyIcon {Gear,Gift,Crown,Tasks,Shop,Star,Stats,Brush,Home,Levels,Hero,Trophy,Mountain,Clock,Infinity,Play,Plus,Close,Arrow}
[RequireComponent(typeof(CanvasRenderer))]
public class LobbyGlyph:MaskableGraphic {
 public LobbyIcon icon;
 public override Texture mainTexture=>CinematicUI.Texture(icon.ToString())?CinematicUI.Texture(icon.ToString()):base.mainTexture;
 VertexHelper mesh;float radius;Color ink;
 protected override void OnPopulateMesh(VertexHelper v){
  if(CinematicUI.Draw(v,rectTransform.rect,new Color(1,1,1,color.a),icon.ToString()))return;
  v.Clear();mesh=v;ink=color;radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.43f;
  switch(icon){
   case LobbyIcon.Plus:Line(.15f,-.7f,0,.7f,0);Line(.15f,0,-.7f,0,.7f);break;
   case LobbyIcon.Close:Line(.12f,-.55f,-.55f,.55f,.55f);Line(.12f,-.55f,.55f,.55f,-.55f);break;
   case LobbyIcon.Arrow:Line(.15f,-.28f,-.65f,.38f,0,-.28f,.65f);break;
   case LobbyIcon.Play:Poly(-.6f,-.85f,.85f,0,-.6f,.85f);break;
   case LobbyIcon.Home:Poly(-.85f,.05f,0,.85f,.85f,.05f,.65f,-.08f,0,.53f,-.65f,-.08f);Poly(-.57f,-.07f,-.18f,-.07f,-.18f,-.8f,-.57f,-.8f);Poly(.18f,-.8f,.57f,-.8f,.57f,-.07f,.18f,-.07f);Poly(-.18f,-.05f,.18f,-.05f,.18f,.4f,-.18f,.4f);break;
   case LobbyIcon.Gear:for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Vector2 d=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),n=new Vector2(-d.y,d.x);Poly(d*.56f+n*.19f,d*.92f+n*.17f,d*.92f-n*.17f,d*.56f-n*.19f);}Ring(0,0,.58f,.25f);break;
   case LobbyIcon.Gift:Poly(-.75f,-.75f,-.1f,-.75f,-.1f,.12f,-.75f,.12f);Poly(.1f,-.75f,.75f,-.75f,.75f,.12f,.1f,.12f);Poly(-.86f,.18f,.86f,.18f,.86f,.43f,-.86f,.43f);Line(.11f,0,.46f,-.35f,.88f,-.65f,.76f,-.45f,.51f,0,.46f,.35f,.88f,.65f,.76f,.45f,.51f,0,.46f);break;
   case LobbyIcon.Crown:Poly(-.76f,-.4f,-.96f,.54f,-.38f,.16f,0,.86f,.38f,.16f,.96f,.54f,.76f,-.4f);Line(.14f,-.75f,-.62f,.75f,-.62f);break;
   case LobbyIcon.Tasks:Line(.12f,-.38f,.67f,-.7f,.67f,-.7f,-.78f,.7f,-.78f,.7f,.67f,.38f,.67f);Poly(-.32f,.48f,.32f,.48f,.32f,.85f,-.32f,.85f);Line(.15f,-.36f,-.08f,-.08f,-.35f,.4f,.18f);break;
   case LobbyIcon.Shop:Poly(-.83f,.2f,.83f,.2f,.6f,.85f,-.6f,.85f);Line(.1f,-.65f,.15f,-.65f,-.75f,.65f,-.75f,.65f,.15f);Poly(-.2f,-.75f,.2f,-.75f,.2f,-.28f,-.2f,-.28f);break;
   case LobbyIcon.Star:{var points=new List<Vector2>();for(int i=0;i<10;i++){float a=Mathf.PI/2+i*Mathf.PI/5;points.Add(new Vector2(Mathf.Cos(a),Mathf.Sin(a))*(i%2==0?.92f:.43f));}Poly(points.ToArray());break;}
   case LobbyIcon.Stats:Poly(-.8f,-.7f,-.4f,-.7f,-.4f,.3f,-.8f,.3f);Poly(-.2f,-.7f,.2f,-.7f,.2f,.86f,-.2f,.86f);Poly(.4f,-.7f,.8f,-.7f,.8f,.56f,.4f,.56f);break;
   case LobbyIcon.Brush:Poly(-.1f,-.2f,.32f,-.06f,.8f,.76f,.5f,.96f,-.05f,.24f);Poly(-.16f,-.22f,-.4f,-.23f,-.55f,-.53f,-.76f,-.74f,-.35f,-.76f,-.08f,-.49f);break;
   case LobbyIcon.Levels:Line(.12f,-.65f,-.5f,.65f,-.5f,0,.62f,-.65f,-.5f);Disk(-.65f,-.5f,.24f);Disk(.65f,-.5f,.24f);Disk(0,.62f,.24f);break;
   case LobbyIcon.Hero:Poly(-.65f,-.25f,-.55f,.45f,-.3f,.16f,.02f,1,.22f,.45f,.36f,.65f,.68f,-.15f,.49f,-.62f,0,-.84f,-.49f,-.62f);Color prior=ink;ink=new Color(.01f,.035f,.11f,1);Poly(-.45f,-.26f,-.2f,-.15f,0,-.34f,.2f,-.15f,.45f,-.26f,.3f,-.6f,0,-.7f,-.3f,-.6f);ink=prior;Disk(-.2f,-.41f,.075f);Disk(.2f,-.41f,.075f);break;
   case LobbyIcon.Trophy:Poly(-.55f,.7f,.55f,.7f,.42f,-.13f,0,-.43f,-.42f,-.13f);Line(.12f,-.58f,.53f,-.89f,.53f,-.8f,-.02f,-.43f,-.19f);Line(.12f,.58f,.53f,.89f,.53f,.8f,-.02f,.43f,-.19f);Line(.17f,0,-.34f,0,-.75f);Line(.14f,-.48f,-.82f,.48f,-.82f);break;
   case LobbyIcon.Mountain:Poly(-.96f,-.7f,-.33f,.72f,.04f,.18f,.35f,.94f,.95f,-.7f);break;
   case LobbyIcon.Clock:Ring(0,0,.8f,.13f);Line(.14f,0,.53f,0,0,.36f,-.22f);break;
   case LobbyIcon.Infinity:{var p=new List<float>();for(int i=0;i<=90;i++){float t=i/90f*Mathf.PI*2;p.Add(Mathf.Cos(t)*.94f);p.Add(Mathf.Sin(t)*Mathf.Cos(t)*.85f);}Line(.13f,p.ToArray());break;}
  }
 }
 void Poly(params float[] p){var q=new Vector2[p.Length/2];for(int i=0;i<q.Length;i++)q[i]=new Vector2(p[i*2],p[i*2+1]);Poly(q);}
 void Poly(params Vector2[] p){int k=mesh.currentVertCount;Vector2 center=Vector2.zero;foreach(var a in p)center+=a;center/=p.Length;mesh.AddVert(center*radius,ink,Vector2.zero);foreach(var a in p)mesh.AddVert(a*radius,ink,Vector2.zero);for(int i=0;i<p.Length;i++)mesh.AddTriangle(k,k+1+i,k+1+(i+1)%p.Length);}
 void Disk(float x,float y,float r){var p=new Vector2[32];for(int i=0;i<32;i++){float a=i*Mathf.PI/16;p[i]=new Vector2(x+Mathf.Cos(a)*r,y+Mathf.Sin(a)*r);}Poly(p);}
 void Ring(float x,float y,float r,float width){var p=new float[130];for(int i=0;i<=64;i++){float a=i*Mathf.PI/32;p[i*2]=x+Mathf.Cos(a)*r;p[i*2+1]=y+Mathf.Sin(a)*r;}Line(width,p);}
 void Line(float width,params float[] p){for(int i=0;i<p.Length-2;i+=2){Vector2 a=new Vector2(p[i],p[i+1])*radius,b=new Vector2(p[i+2],p[i+3])*radius;var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*radius*width*.5f;int k=mesh.currentVertCount;mesh.AddVert(a-n,ink,Vector2.zero);mesh.AddVert(a+n,ink,Vector2.zero);mesh.AddVert(b+n,ink,Vector2.zero);mesh.AddVert(b-n,ink,Vector2.zero);mesh.AddTriangle(k,k+1,k+2);mesh.AddTriangle(k,k+2,k+3);}}
}

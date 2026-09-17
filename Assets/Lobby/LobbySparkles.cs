using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(CanvasRenderer))]
public class LobbySparkles:MaskableGraphic {
 public PulseLobby lobby;float clock;
 void Update(){if(!lobby||!lobby.IsOpen||lobby.Profile.reducedMotion)return;clock+=Time.unscaledDeltaTime;SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper v){
  v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;if(w<20||h<20)return;
  // Three depths: distant points are slow; nearer motes drift faster and have soft halos.
  for(int i=0;i<60;i++){
   float depth=1+i%3;float x=Mathf.Repeat(i*137.7f+Mathf.Sin(clock*.08f+i)*depth*9,w-10)-(w-10)*.5f;
   float y=Mathf.Repeat(i*317.3f+clock*(1.5f+depth*1.3f),h-10)-(h-10)*.5f;
   float alpha=(.15f+Mathf.Sin(clock*(.4f+i%4*.12f)+i)*.1f);
   Dot(v,new Vector2(x,y),.7f+depth*.45f,new Color(.42f,.78f,1,alpha));
   if(i%9==0)Dot(v,new Vector2(x,y),6,new Color(.22f,.56f,1,alpha*.19f));
  }
  float phase=Mathf.Repeat(clock+5,19);if(phase<1.5f){float t=phase/1.5f;Vector2 p=new Vector2(Mathf.Lerp(-w*.32f,w*.28f,t),Mathf.Lerp(h*.32f,h*.15f,t));
   Color c=new Color(.6f,.86f,1,Mathf.Sin(t*Mathf.PI)*.42f);Dot(v,p,2.1f,c);
   for(int j=1;j<=12;j++){c.a*=.78f;Dot(v,p+new Vector2(-j*3,j*1.8f),1.6f,c);}
  }
 }
 static void Dot(VertexHelper v,Vector2 p,float size,Color c){int k=v.currentVertCount;v.AddVert(p,c,Vector2.zero);c.a=0;v.AddVert(p+new Vector2(-size,-size),c,Vector2.zero);v.AddVert(p+new Vector2(-size,size),c,Vector2.zero);v.AddVert(p+new Vector2(size,size),c,Vector2.zero);v.AddVert(p+new Vector2(size,-size),c,Vector2.zero);for(int j=0;j<4;j++)v.AddTriangle(k,k+1+j,k+1+(j+1)%4);}
}

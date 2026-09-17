using UnityEngine;

// A small, pooled 3D effect: it follows the feet without affecting hero framing.
public sealed class MenuHeroAura:MonoBehaviour {
 public LobbyStage stage;
 [Range(.1f,1)]public float radius=.48f;
 [Range(0,2)]public float intensity=1;
 Transform root;Material material;LineRenderer[] rings,rays,motes;float clock;
 public static MenuHeroAura Ensure(LobbyStage stage){var aura=stage.GetComponent<MenuHeroAura>();if(!aura)aura=stage.gameObject.AddComponent<MenuHeroAura>();aura.stage=stage;return aura;}
 LineRenderer Line(string name,int points,float width){
  var o=new GameObject(name){hideFlags=HideFlags.DontSave};o.layer=stage.spirit.gameObject.layer;o.transform.SetParent(root,false);
  var line=o.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=false;line.positionCount=points;line.widthMultiplier=width;line.numCapVertices=2;
  line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;return line;
 }
 void Prepare(){
  if(root)return;var shader=Resources.Load<Shader>("PulseEnergy");if(!shader)return;
  material=new Material(shader){name="Hero aura • soft energy",hideFlags=HideFlags.DontSave};material.SetColor("_Color",Color.white);
  root=new GameObject("Hero aura • concentric light and rising sparks"){hideFlags=HideFlags.DontSave}.transform;root.SetParent(transform,false);
  rings=new LineRenderer[4];for(int i=0;i<rings.Length;i++)rings[i]=Line("Ground halo "+i,97,i==3?.065f:.008f);
  rays=new LineRenderer[9];for(int i=0;i<rays.Length;i++)rays[i]=Line("Rising light "+i,3,.0025f);
  motes=new LineRenderer[18];for(int i=0;i<motes.Length;i++)motes[i]=Line("Orbiting spark "+i,5,.006f);
 }
 void LateUpdate(){Advance(Application.isPlaying?Time.unscaledDeltaTime:0);}
 public void Advance(float dt){
  if(!stage||!stage.spirit)return;Prepare();if(!root)return;
  if(!stage.reducedMotion)clock+=dt;
  root.position=stage.spirit.position;root.rotation=Quaternion.Euler(15,0,0);root.localScale=Vector3.one;
  // Lines are local to the hero but are not descendants of its fitted bounds.
  root.localScale=stage.spirit.lossyScale.x/Mathf.Max(.0001f,transform.lossyScale.x)*Vector3.one;
  Color tint=PulseLobby.SkinColors[stage.lobby!=null?Mathf.Clamp(stage.lobby.Profile.selectedSkin,0,4):0];
  tint=Color.Lerp(tint,new Color(.12f,.8f,1),.25f);float pulse=1+Mathf.Sin(clock*1.8f)*.08f;
  for(int k=0;k<rings.Length;k++){
   float r=radius*(k==0?.72f:k==1?.9f:1.06f);var c=tint*(k==3?.16f:1.5f)*intensity*pulse;c.a=1;rings[k].startColor=rings[k].endColor=c;
   for(int j=0;j<97;j++){float a=j*Mathf.PI*2/96;rings[k].SetPosition(j,new Vector3(Mathf.Cos(a)*r,.012f+k*.001f,Mathf.Sin(a)*r));}
  }
  for(int i=0;i<rays.Length;i++){
   float a=i*2.39996f;float h=.18f+Mathf.Repeat(clock*.09f+i*.137f,.8f);var p=new Vector3(Mathf.Cos(a)*radius,.02f,Mathf.Sin(a)*radius);
   rays[i].SetPosition(0,p);rays[i].SetPosition(1,p+Vector3.up*h);rays[i].SetPosition(2,p+Vector3.up*(h+.12f));
   rays[i].startColor=new Color(tint.r,tint.g,tint.b,.25f*intensity);rays[i].endColor=Color.clear;
  }
  for(int i=0;i<motes.Length;i++){
   float a=i*2.39996f+clock*(i%2==0?.19f:-.13f);float h=.04f+Mathf.Repeat(i*.071f+clock*.052f,.8f);float r=radius*(.85f+Mathf.Sin(i*5.7f)*.18f);
   var p=new Vector3(Mathf.Cos(a)*r,h,Mathf.Sin(a)*r);float s=i%5==0?.018f:.007f;
   motes[i].SetPosition(0,p+Vector3.up*s);motes[i].SetPosition(1,p+Vector3.right*s*.5f);motes[i].SetPosition(2,p-Vector3.up*s);motes[i].SetPosition(3,p-Vector3.right*s*.5f);motes[i].SetPosition(4,p+Vector3.up*s);
   var c=Color.Lerp(tint,Color.white,.4f);c.a=Mathf.Sin(Mathf.Clamp01((h-.04f)/.8f)*Mathf.PI)*.8f*intensity;motes[i].startColor=motes[i].endColor=c;
  }
 }
 void OnDestroy(){if(root)Dispose(root.gameObject);if(material)Dispose(material);}
 static void Dispose(Object o){if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
}

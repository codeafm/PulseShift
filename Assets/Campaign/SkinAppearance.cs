using UnityEngine;
using System.Collections.Generic;

// Owns skin materials so transparent VFX never receive the opaque mesh atlas.
public sealed class SkinAppearance:MonoBehaviour {
 readonly List<Material> owned=new(); readonly List<Mesh> meshes=new();
 Transform details; int selected=-1; bool isPortal;
 Material heroMaterial;
 public void SetMotion(bool enabled){if(heroMaterial)heroMaterial.SetFloat("_Motion",enabled?1:0);}
 public static void Apply(Transform root,bool portal,int index){
  var skin=root.GetComponent<SkinAppearance>();if(!skin)skin=root.gameObject.AddComponent<SkinAppearance>();
  skin.Set(portal,Mathf.Clamp(index,0,4));
 }
 void Release(Object o){if(!o)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
 void Clear(){if(details){details.gameObject.SetActive(false);Release(details.gameObject);}foreach(var m in owned)Release(m);owned.Clear();foreach(var m in meshes)Release(m);meshes.Clear();}
 void OnDestroy(){Clear();}
 Material Mat(string name,Color color,float emission=0,float metal=.35f){
  var m=new Material(Shader.Find("Standard")){name=name,color=color};m.SetFloat("_Metallic",metal);m.SetFloat("_Glossiness",.58f);
  if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);}owned.Add(m);return m;
 }
 void Set(bool portal,int index){
  if(selected==index&&isPortal==portal)return;Clear();selected=index;isPortal=portal;
  var visual=transform.Find("Reference visual");if(!visual)return;
  if(portal)CinematicArt.Dress(gameObject,"Portal");
  var source=Resources.Load<ReferenceModelAsset>("ReferenceModels/"+(portal?"Portal":"Spirit")).material;
  var color=(portal?PulseLobby.PortalColors:PulseLobby.SkinColors)[index];
  var skin=new Material(Resources.Load<Shader>("Lobby/SpiritSkin")){name=(portal?"Portal":"Hero")+" / skin "+index};owned.Add(skin);
  foreach(var p in new[]{"_MainTex","_BumpMap","_EmissionMap"})skin.SetTexture(p,source.GetTexture(p));skin.SetColor("_Tint",color);skin.SetFloat("_SkinIndex",index);
  // Only the actual baked Model receives the atlas. Never descendants of Vortex.
  skin.SetFloat("_IsHero",portal?0:1);
  heroMaterial=portal?null:skin;
  var body=visual.Find("Model");if(body){var r=body.GetComponent<Renderer>();if(r){r.enabled=true;r.sharedMaterial=index==0&&portal?source:skin;}}
  var vortex=visual.Find("Vortex");
  if(portal&&vortex){
   var old=vortex.GetComponent<Renderer>();if(old)old.enabled=false;vortex.localRotation=Quaternion.identity;
   var surface=vortex.Find("Dimensional water • animated vortex");if(surface){var r=surface.GetComponent<Renderer>();var mat=new Material(Resources.Load<Shader>("CinematicPortal")){name="Vortex / skin "+index};owned.Add(mat);mat.SetColor("_Tint",color);r.sharedMaterial=mat;}
  }
  details=new GameObject("Skin details").transform;details.SetParent(visual,false);
  var alloy=Mat("Skin / dark polished alloy",Color.Lerp(new Color(.025f,.04f,.07f),color,.16f));
  var glow=Mat("Skin / luminous inlay",color,1.5f);var bright=Mat("Skin / bright armor",Color.Lerp(color,Color.white,.68f),0,.7f);
  if(portal){PortalDetails(index,alloy,glow,bright,vortex?vortex.localPosition:new Vector3(0,1.08f,0));}
  else{HeroDetails(index,alloy,glow,bright);}
  foreach(var t in details.GetComponentsInChildren<Transform>(true))t.gameObject.layer=gameObject.layer;
 }
 GameObject Piece(string name,PrimitiveType type,Vector3 position,Vector3 scale,Material material){
  var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(details,false);o.transform.localPosition=position;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;
  var collider=o.GetComponent<Collider>();if(collider){collider.enabled=false;Release(collider);}return o;
 }
 void Gem(string name,Vector3 pos,Vector3 scale,Material material,float angle=0){
  var mesh=new Mesh{name=name};mesh.vertices=new[]{new Vector3(0,.65f,0),new Vector3(-.4f,0,0),new Vector3(0,0,-.3f),new Vector3(.4f,0,0),new Vector3(0,0,.3f),new Vector3(0,-.5f,0)};
  mesh.triangles=new[]{0,2,1,0,3,2,0,4,3,0,1,4,5,1,2,5,2,3,5,3,4,5,4,1};mesh.RecalculateNormals();meshes.Add(mesh);
  var o=new GameObject(name);o.transform.SetParent(details,false);o.transform.localPosition=pos;o.transform.localScale=scale;o.transform.localRotation=Quaternion.Euler(0,0,angle);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 void PortalDetails(int index,Material alloy,Material glow,Material bright,Vector3 center){
  if(index==0)return;
  if(index==1||index==4){for(int i=0;i<9;i++){float a=(20+i*17.5f)*Mathf.Deg2Rad;var pos=center+new Vector3(Mathf.Cos(a)*.91f,Mathf.Sin(a)*.91f,-.08f);Gem(index==1?"Sun crown":"Ice crystal",pos,new Vector3(.2f,i==4?.48f:.28f,.24f),index==1?glow:bright,a*Mathf.Rad2Deg-90);}}
  if(index==2){for(int i=0;i<7;i++){float a=(15+i*25)*Mathf.Deg2Rad;Gem("Floating void fragment",center+new Vector3(Mathf.Cos(a)*1.02f,Mathf.Sin(a)*1.02f,0),new Vector3(.22f,.28f,.19f),alloy,i*31);}}
  if(index==3){var leaf=Mat("Emerald leaves",new Color(.06f,.28f,.055f),0,.05f);for(int i=0;i<22;i++){float a=i*Mathf.PI*2/22;var pos=center+new Vector3(Mathf.Cos(a)*.9f,Mathf.Sin(a)*.9f,-.06f);var o=Piece("Carved leaf",PrimitiveType.Sphere,pos,new Vector3(.11f,.22f,.045f),leaf);o.transform.localRotation=Quaternion.Euler(0,0,-a*Mathf.Rad2Deg);}}
 }
 void HeroDetails(int index,Material alloy,Material glow,Material bright){
  Gem("Heart crystal",new Vector3(0,.3f,-.102f),new Vector3(.105f,.14f,.06f),glow);
  if(index==0)return;
  if(index==1){for(int side=-1;side<=1;side+=2){var o=Piece("Flame shoulder armor",PrimitiveType.Sphere,new Vector3(side*.14f,.34f,0),new Vector3(.17f,.12f,.2f),alloy);Gem("Amber shoulder inlay",new Vector3(side*.14f,.38f,-.07f),new Vector3(.07f,.12f,.06f),glow);}}
  if(index==2){for(int i=0;i<16;i++){float a=i*2.39996f;float h=.14f+(i%7)*.09f;Gem("Starlight",new Vector3(Mathf.Cos(a)*.2f,h,-.105f),Vector3.one*(i%3==0?.027f:.015f),glow);}}
  if(index==3){for(int side=-1;side<=1;side+=2){var ear=Piece("Tech headphone",PrimitiveType.Cylinder,new Vector3(side*.235f,.75f,0),new Vector3(.2f,.045f,.2f),bright);ear.transform.localRotation=Quaternion.Euler(0,0,90);ear=Piece("Headphone cyan core",PrimitiveType.Cylinder,new Vector3(side*.283f,.75f,0),new Vector3(.135f,.012f,.135f),glow);ear.transform.localRotation=Quaternion.Euler(0,0,90);Piece("Tech shoulder shell",PrimitiveType.Sphere,new Vector3(side*.14f,.34f,0),new Vector3(.18f,.14f,.2f),bright);}}
  if(index==4){
   var cape=Mat("Shadow / velvet cloak",new Color(.085f,.012f,.14f),0,.12f);var mesh=new Mesh{name="Tailored shadow cape"};var v=new List<Vector3>();var tri=new List<int>();
   for(int row=0;row<6;row++)for(int col=0;col<9;col++){float t=row/5f,u=col/8f;v.Add(new Vector3((u-.5f)*Mathf.Lerp(.25f,.68f,t),.51f-t*.47f,.10f+t*.12f+Mathf.Sin(u*Mathf.PI*6)*.025f*t));}
   for(int r=0;r<5;r++)for(int c=0;c<8;c++){int a=r*9+c;tri.AddRange(new[]{a,a+9,a+1,a+1,a+9,a+10,a,a+1,a+9,a+1,a+10,a+9});}mesh.SetVertices(v);mesh.SetTriangles(tri,0);var normals=new Vector3[v.Count];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.back;mesh.normals=normals;meshes.Add(mesh);var o=new GameObject("Shadow cloak");o.transform.SetParent(details,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;o.AddComponent<MeshRenderer>().sharedMaterial=cape;
  }
 }
}

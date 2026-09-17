using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PulseGame : MonoBehaviour {
 [SerializeField] Transform editableLevel;
 [SerializeField] Camera authoredCamera;
 int modelIndex;
 Texture2D circle;
 [SerializeField] int artVersion;
 Vector3 BoardPoint(int r,int c)=>new Vector3((c-1)*2.15f+(r%2)*.62f,0,(r-2.5f)*2.6f);
 Transform MovingDeck(GameObject platform){var d=platform.transform.Find("Deck • moving surface");return d?d:platform.transform;}
 public void CreateEditableScene(){
  artVersion=3;
  var root=new GameObject("LEVEL • editable models");editableLevel=root.transform;modelIndex=0;
  for(int r=0;r<6;r++)for(int c=0;c<3;c++){var o=PulseArt.Make("Platform",BoardPoint(r,c));o.name=$"{modelIndex++:00}_Platform";o.transform.SetParent(editableLevel,true);if(r==2||r==4){foreach(var tr in o.GetComponentsInChildren<Transform>())if(tr.name.StartsWith("Foundation")||tr.name.StartsWith("Rock"))tr.gameObject.SetActive(false);MovingDeck(o).localRotation=Quaternion.Euler(0,0,32);}if((r+c)%3!=0)foreach(var tr in o.GetComponentsInChildren<Transform>())if(tr.name.Contains("Deck_Recess")||tr.name.Contains("Deck_Inlay"))tr.gameObject.SetActive(false);}
  string[] names={"Spark","Portal","Crystal","Crystal","Crystal"};int[] cells={4,16,3,8,13};
  for(int i=0;i<names.Length;i++){int r=cells[i]/3,c=cells[i]%3;var o=PulseArt.Make(names[i],BoardPoint(r,c)+Vector3.up*.18f);o.name=$"{modelIndex++:00}_{names[i]}";o.transform.SetParent(editableLevel,true);if(i==0){o.transform.localScale=Vector3.one*1.12f;PulseAura.Create(o);}if(i==1)o.transform.localScale=Vector3.one*1.12f;}
  for(int i=0;i<2;i++){var o=PulseArt.Make("Enemy",BoardPoint(i==0?3:4,i==0?0:2)+Vector3.up*.18f);o.name=$"{modelIndex++:00}_Enemy";o.transform.SetParent(editableLevel,true);}
  authoredCamera=new GameObject("Main Camera • portrait framing").AddComponent<Camera>();authoredCamera.tag="MainCamera";authoredCamera.transform.position=new Vector3(.32f,12,-14);authoredCamera.transform.LookAt(new Vector3(.32f,0,0));authoredCamera.orthographic=true;authoredCamera.orthographicSize=7.6f;authoredCamera.clearFlags=CameraClearFlags.SolidColor;authoredCamera.backgroundColor=new Color(.025f,.045f,.12f);authoredCamera.allowHDR=true;authoredCamera.gameObject.AddComponent<PulseBloom>();
  var bg=GameObject.CreatePrimitive(PrimitiveType.Quad);bg.name="Animated cloud backdrop";DestroyImmediate(bg.GetComponent<Collider>());bg.transform.SetParent(authoredCamera.transform,false);bg.transform.localPosition=new Vector3(0,0,60);bg.transform.localScale=new Vector3(100,100,1);bg.GetComponent<Renderer>().sharedMaterial=new Material(Resources.Load<Shader>("PulseClouds"));
  SetupLight();PulseScenery.Create();
 }
 void SetupLight(){RenderSettings.ambientLight=new Color(.23f,.28f,.44f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.045f,.075f,.18f);RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.026f;var l=new GameObject("Moonlight • editable").AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.3f;l.color=new Color(.7f,.8f,1);l.transform.rotation=Quaternion.Euler(45,-35,0);l.shadows=LightShadows.Soft;var fill=new GameObject("Warm rim • editable").AddComponent<Light>();fill.type=LightType.Directional;fill.color=new Color(1,.65f,.4f);fill.intensity=.55f;fill.transform.rotation=Quaternion.Euler(25,150,0);}
 class Tile { public Vector3 p; public GameObject go; public bool on=true; }
 readonly List<Tile> tiles=new(); readonly Dictionary<int,GameObject> gems=new();
 readonly List<GameObject> foes=new(); readonly List<int> foeCells=new();
 GameObject hero,gate; Camera cam; int cell,level=1,collected,moves=12; bool busy,ended,won; string message="Click an adjacent island to move. Space sends a pulse.";
 Material beam; AudioSource sound; int total=10;
 void Start(){
  Application.targetFrameRate=60; QualitySettings.antiAliasing=4;
  if(!editableLevel)CreateEditableScene();cam=authoredCamera;editableLevel.gameObject.SetActive(false);
  beam=new Material(Shader.Find("Standard"));beam.color=Color.cyan;beam.EnableKeyword("_EMISSION");beam.SetColor("_EmissionColor",Color.cyan*3);
  cam.clearFlags=CameraClearFlags.SolidColor;
  cam.gameObject.AddComponent<AudioListener>();sound=gameObject.AddComponent<AudioSource>(); LoadLevel();
  if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"-pulseSmoke")>=0)StartCoroutine(Smoke());
 }
 GameObject Model(string name,Vector3 pos){
  string key=$"{modelIndex++:00}_{name}";var source=editableLevel?editableLevel.Find(key):null;
  var go=source?Instantiate(source.gameObject):PulseArt.Make(name,pos);go.transform.SetParent(null,true);if(source)go.transform.position=source.position;go.name=name;return go;
  /* Legacy FBX material conversion retained in Blender source.
  foreach(var r in go.GetComponentsInChildren<Renderer>()){
   var mats=r.materials;foreach(var m in mats){
    var n=m.name.ToLower();Color c=n.Contains("cyan")?new Color(.02f,.8f,1):n.Contains("amber")?new Color(1,.55f,.03f):n.Contains("eyes")?new Color(.8f,.06f,1):n.Contains("violet")?new Color(.16f,.03f,.3f):n.Contains("silver")?new Color(.25f,.34f,.45f):new Color(.07f,.11f,.19f);
    m.shader=Shader.Find("Standard");m.color=c;m.SetFloat("_Metallic",.45f);m.SetFloat("_Glossiness",.55f);
    if(n.Contains("cyan")||n.Contains("amber")||n.Contains("eyes")){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*2);}
   }r.materials=mats;
  } return go; */
 }
 void LoadLevel(){
  StopAllCoroutines();foreach(var t in tiles)Destroy(t.go);tiles.Clear();foreach(var g in gems.Values)Destroy(g);gems.Clear();foreach(var f in foes)Destroy(f);foes.Clear();foeCells.Clear();if(hero)Destroy(hero);if(gate)Destroy(gate);
  modelIndex=0;busy=ended=won=false;collected=0;cell=4;moves=12+level;message="Collect 3 memories, then reach the portal. Click neighbouring islands.";
  for(int r=0;r<6;r++)for(int c=0;c<3;c++){
   Vector3 p=BoardPoint(r,c);var go=Model("Platform",p);var box=go.AddComponent<BoxCollider>();box.center=new Vector3(0,-.1f,0);box.size=new Vector3(1.55f,.5f,1.4f);tiles.Add(new Tile{p=go.transform.position,go=go});
  }
  for(int i=0;i<tiles.Count;i++)if(i/3==2||i/3==4){tiles[i].on=false;MovingDeck(tiles[i].go).localRotation=Quaternion.Euler(0,0,32);}
  hero=Model("Spark",tiles[cell].p+Vector3.up*.18f);gate=Model("Portal",tiles[16].p+Vector3.up*.18f);
  var glow=hero.AddComponent<Light>();glow.type=LightType.Point;glow.color=Color.cyan;glow.range=2;glow.intensity=.6f;
  var trail=hero.AddComponent<TrailRenderer>();trail.material=beam;trail.time=.4f;trail.startWidth=.13f;trail.endWidth=0;trail.minVertexDistance=.04f;
  int[] picks={3,8,12+(level%3)};foreach(int i in picks)gems[i]=Model("Crystal",tiles[i].p+Vector3.up*.18f);
  if(level>=1){foeCells.Add(9);foes.Add(Model("Enemy",tiles[9].p+Vector3.up*.18f));}
  if(level>=1){foeCells.Add(14);foes.Add(Model("Enemy",tiles[14].p+Vector3.up*.18f));}
 }
 bool Adj(int a,int b)=>a!=b&&Vector3.Distance(tiles[a].p,tiles[b].p)<3.1f;
 void Update(){
  if(cam){cam.orthographicSize=Mathf.Max(7.6f,3.55f/Mathf.Max(.2f,cam.aspect));var bg=cam.transform.Find("Animated cloud backdrop");if(bg)bg.localScale=new Vector3(cam.orthographicSize*2*cam.aspect,cam.orthographicSize*2,1);}
  foreach(var pair in gems){pair.Value.transform.Rotate(0,40*Time.deltaTime,0);pair.Value.transform.position=tiles[pair.Key].p+Vector3.up*(.24f+Mathf.Sin(Time.time*2)*.08f);}
  if(!busy&&hero)hero.transform.position=tiles[cell].p+Vector3.up*(.2f+Mathf.Sin(Time.time*3)*.045f);
  if(ended||busy)return;
  if(Input.GetKeyDown(KeyCode.Space))StartCoroutine(Pulse());
  if(Input.GetMouseButtonDown(0)&&Input.mousePosition.y<Screen.height-110&&Input.mousePosition.y>110){
   if(Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition),out var hit)){
    int target=tiles.FindIndex(t=>t.go==hit.collider.gameObject);
    if(target>=0&&Adj(cell,target)&&tiles[target].on)StartCoroutine(Move(target));
   }
  }
 }
 IEnumerator Move(int target){
  busy=true;Vector3 from=hero.transform.position,to=tiles[target].p+Vector3.up*.2f;
  for(float t=0;t<1;t+=Time.deltaTime*4){hero.transform.position=Vector3.Lerp(from,to,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.35f;yield return null;}
  cell=target;if(gems.TryGetValue(cell,out var gem)){Destroy(gem);gems.Remove(cell);collected++;Tone(600+collected*150);}
  Check();busy=false;
 }
 IEnumerator Pulse(){
  if(moves<=0)yield break;busy=true;moves--;Tone(220);
  var ring=new GameObject("Expanding pulse");var line=ring.AddComponent<LineRenderer>();line.material=beam;line.positionCount=65;line.startWidth=.055f;line.endWidth=.055f;
  var changed=new List<int>();for(int i=0;i<tiles.Count;i++)if(Adj(cell,i)&&i!=16){tiles[i].on=!tiles[i].on;changed.Add(i);}
  for(float t=0;t<1;t+=Time.deltaTime*2){
   for(int j=0;j<65;j++){float a=j*Mathf.PI*2/64;line.SetPosition(j,tiles[cell].p+new Vector3(Mathf.Cos(a)*t*3,.3f,Mathf.Sin(a)*t*3));}
   foreach(int i in changed){var tr=MovingDeck(tiles[i].go);tr.localRotation=Quaternion.Euler(0,0,tiles[i].on?(1-t)*32:t*32);}
   yield return null;
  }Destroy(ring);
  for(int e=0;e<foes.Count;e++){
   int best=foeCells[e];float dist=Vector3.Distance(tiles[best].p,tiles[cell].p);
   for(int i=0;i<tiles.Count;i++)if(tiles[i].on&&Adj(foeCells[e],i)){float d=Vector3.Distance(tiles[i].p,tiles[cell].p);if(d<dist){dist=d;best=i;}}
   foeCells[e]=best;foes[e].transform.position=tiles[best].p+Vector3.up*.2f;
  }
  message="Pulse changed nearby platforms. Shadows moved closer.";Check();busy=false;
 }
 void Check(){
  if(foeCells.Contains(cell)){ended=true;won=false;message="The shadow caught your Spark.";Tone(90);}
  else if(cell==16&&collected==3){ended=true;won=true;message=level==total?"All islands restored!":"Island restored!";Tone(1000);}
 }
 IEnumerator Smoke(){
  yield return new WaitForSeconds(2);
  yield return new WaitForEndOfFrame();var screen=ScreenCapture.CaptureScreenshotAsTexture();if(screen){System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Unity-full-preview.png")),ImageConversion.EncodeToPNG(screen));Destroy(screen);}
  var rt=new RenderTexture(540,960,24,RenderTextureFormat.DefaultHDR);cam.targetTexture=rt;cam.aspect=540f/960;cam.orthographicSize=7.6f;cam.Render();RenderTexture.active=rt;
  var tex=new Texture2D(540,960,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,540,960),0,0);if(QualitySettings.activeColorSpace==ColorSpace.Linear){var pixels=tex.GetPixels();for(int k=0;k<pixels.Length;k++)pixels[k]=pixels[k].gamma;tex.SetPixels(pixels);}tex.Apply();System.IO.File.WriteAllBytes(System.IO.Path.GetFullPath(System.IO.Path.Combine(Application.dataPath,"../../Unity-art-preview.png")),ImageConversion.EncodeToPNG(tex));
  cam.targetTexture=null;RenderTexture.active=null;Destroy(tex);Destroy(rt);
  yield return Move(3);yield return Move(4);yield return Pulse();
  if(collected!=1||cell!=4||moves!=12||!tiles[7].on)throw new System.Exception("Gameplay smoke test failed");
  Debug.Log("PULSE_SMOKE_OK: models rendered, movement and platform pulse verified");Application.Quit();
 }
 void Tone(float hz){int count=6000;float[] data=new float[count];for(int i=0;i<count;i++)data[i]=Mathf.Sin(i*hz*2*Mathf.PI/44100)*.12f*(1f-(float)i/count);var clip=AudioClip.Create("Pulse tone",count,1,44100,false);clip.SetData(data,0);sound.PlayOneShot(clip);Destroy(clip,1);}
 void OnGUI(){
  DrawInterface();return;
  /*
  float s=Mathf.Max(.6f,Screen.height/900f);GUI.matrix=Matrix4x4.Scale(new Vector3(s,s,1));float w=Screen.width/s,h=Screen.height/s;
  var title=new GUIStyle(GUI.skin.label){fontSize=26,alignment=TextAnchor.MiddleCenter};title.normal.textColor=Color.cyan;
  var text=new GUIStyle(GUI.skin.label){fontSize=18,alignment=TextAnchor.MiddleCenter,wordWrap=true};var button=new GUIStyle(GUI.skin.button){fontSize=20};
  GUI.Box(new Rect(0,0,w,100),"");GUI.Label(new Rect(0,8,w,35),"P U L S E S H I F T",title);GUI.Label(new Rect(0,50,w,35),$"ISLAND {level:00}     MEMORIES {collected}/3     PULSES {moves}",text);
  GUI.Box(new Rect(0,h-110,w,110),"");GUI.Label(new Rect(10,h-108,w-20,43),message,text);
  GUI.enabled=!busy&&!ended&&moves>0;if(GUI.Button(new Rect(w/2-105,h-59,210,48),"PULSE  [SPACE]",button))StartCoroutine(Pulse());GUI.enabled=true;
  if(GUI.Button(new Rect(12,h-55,100,40),"Restart"))LoadLevel();
  if(ended){GUI.Box(new Rect(w/2-200,h/2-100,400,200),"");GUI.Label(new Rect(w/2-190,h/2-80,380,60),message,title);if(GUI.Button(new Rect(w/2-130,h/2+10,260,60),won?"Continue":"Try again",button)){if(won)level=level%total+1;LoadLevel();}} */
 }
 void DrawInterface(){
  float s=Mathf.Min(Screen.width/540f,Screen.height/960f);GUI.matrix=Matrix4x4.Scale(new Vector3(s,s,1));float w=Screen.width/s,h=Screen.height/s;
  if(!circle){circle=new Texture2D(128,128,TextureFormat.RGBA32,false);for(int y=0;y<128;y++)for(int x=0;x<128;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(63.5f,63.5f));float ring=Mathf.Clamp01(2-Mathf.Abs(d-59));circle.SetPixel(x,y,new Color(.05f,.75f,1,Mathf.Max(ring,d<58?.2f:0)));}circle.Apply();}
  var title=new GUIStyle(GUI.skin.label){fontSize=23,alignment=TextAnchor.MiddleLeft};title.normal.textColor=new Color(.75f,.91f,1);
  var label=new GUIStyle(GUI.skin.label){fontSize=20,alignment=TextAnchor.MiddleCenter};label.normal.textColor=new Color(.77f,.88f,1);
  float safeTop=(Screen.height-Screen.safeArea.yMax)/s;
  GUI.Label(new Rect(25,28+safeTop,260,40),"P U L S E S H I F T",title);GUI.DrawTexture(new Rect(w-84,20+safeTop,58,58),circle);GUI.Label(new Rect(w-84,25+safeTop,58,45),level.ToString("00"),label);
  label.fontSize=17;GUI.Label(new Rect(20,80+safeTop,w-40,30),$"MEMORIES   {collected} / 3                 PULSES   {moves}",label);
  var button=new GUIStyle(){fontSize=25,alignment=TextAnchor.MiddleCenter};button.normal.textColor=Color.white;
  GUI.DrawTexture(new Rect(w/2-58,h-153,116,116),circle);GUI.enabled=!busy&&!ended&&moves>0;if(GUI.Button(new Rect(w/2-58,h-153,116,116),"PULSE",button))StartCoroutine(Pulse());GUI.enabled=true;
  GUI.DrawTexture(new Rect(w/2-164,h-124,62,62),circle);button.fontSize=20;if(GUI.Button(new Rect(w/2-164,h-124,62,62),"R",button))LoadLevel();
  label.fontSize=15;GUI.Label(new Rect(20,h-33,w-40,25),"Tap an adjacent island  ·  Space to pulse",label);
  if(ended){GUI.color=new Color(.04f,.07f,.16f,.96f);GUI.DrawTexture(new Rect(w/2-210,h/2-100,420,200),Texture2D.whiteTexture);GUI.color=Color.white;label.fontSize=22;GUI.Label(new Rect(w/2-200,h/2-75,400,60),message,label);if(GUI.Button(new Rect(w/2-130,h/2+5,260,65),won?"CONTINUE":"TRY AGAIN",button)){if(won)level=level%total+1;LoadLevel();}}
 }
}

using UnityEngine;
using PulseCampaign;
using System.Collections.Generic;

public static class CampaignVarietyArt {
 static readonly Dictionary<string,Material> palette=new();
 static readonly Color[] tints={new Color(.68f,.7f,.73f),new Color(.65f,.73f,.7f),new Color(.74f,.70f,.65f),new Color(.7f,.66f,.75f),new Color(.67f,.72f,.75f)};
 public static Color EnemyColor(EnemyKind kind)=>kind==EnemyKind.Hunter?new Color(1,.12f,.045f):kind==EnemyKind.Sentinel?new Color(1,.65f,.06f):new Color(.62f,.12f,1);
 static Transform Group(Transform parent,string name){var g=new GameObject(name).transform;g.SetParent(parent,false);return g;}
 static void Solid(Transform parent,string name,Vector3 position,Vector3 scale){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=position;g.transform.localScale=scale;if(Application.isPlaying)Object.Destroy(g.GetComponent<Collider>());else Object.DestroyImmediate(g.GetComponent<Collider>());g.GetComponent<Renderer>().sharedMaterial=ReferenceArtMaterial();}
 static Material ReferenceArtMaterial(){if(!palette.TryGetValue("mechanism",out var m)||!m){m=new Material(Shader.Find("Standard")){name="Mechanism / dark alloy",color=new Color(.06f,.105f,.16f)};m.SetFloat("_Metallic",.6f);m.SetFloat("_Glossiness",.45f);palette["mechanism"]=m;}return m;}
 public static void Decorate(Transform island,Island node){
  if(node.kind==TileKind.Laser){var root=Group(island,"Laser mechanism");Solid(root,"Left emitter",new Vector3(-.68f,.34f,0),new Vector3(.13f,.5f,.5f));Solid(root,"Right emitter",new Vector3(.68f,.34f,0),new Vector3(.13f,.5f,.5f));var beams=Group(root,"Discharge");for(int i=0;i<3;i++)CampaignVisuals.Line(beams,"Laser beam "+i,new[]{new Vector3(-.68f,.32f,(i-1)*.2f),new Vector3(.68f,.32f,(i-1)*.2f)},CampaignVisuals.Energy("Laser hot",new Color(1,.08f,.02f,.95f)),.038f);CampaignVisuals.Ring(root,"Charging",.7f,new Color(1,.6f,.015f,.8f),.03f,.18f);}
  if(node.kind==TileKind.Rift){var root=Group(island,"Rift field");CampaignVisuals.Ring(root,"Ghost outline",.81f,new Color(.1f,.6f,1,.55f),.028f,.2f);CampaignVisuals.Ring(root,"Closing warning",.7f,new Color(1,.45f,.04f,.8f),.03f,.22f);}
  if(node.kind==TileKind.Rift||node.kind==TileKind.Phase||node.kind==TileKind.Fragile)EnsureSeal(island);
  if(node.kind==TileKind.Relay){var root=Group(island,"Relay mechanism");CampaignVisuals.Ring(root,"Charged seal",.6f,new Color(.1f,1,.42f,.85f),.04f,.19f);for(int i=0;i<3;i++){float x=(i-1)*.25f;CampaignVisuals.Line(root,"Rune "+i,new[]{new Vector3(x-.1f,.2f,0),new Vector3(x,.2f,.25f),new Vector3(x+.1f,.2f,0)},CampaignVisuals.Energy("Relay green",new Color(.08f,.8f,.28f,.8f)),.034f);}}
  if(node.kind==TileKind.Gate){var root=Group(island,"Gate mechanism");for(int i=0;i<2;i++)Solid(root,"Seal pillar "+i,new Vector3((i*2-1)*.7f,.37f,-.47f),new Vector3(.17f,.7f,.18f));var bars=Group(root,"Energy bars");for(int i=0;i<5;i++){float x=(i-2)*.26f;CampaignVisuals.Line(bars,"Seal bar "+i,new[]{new Vector3(x,.17f,-.5f),new Vector3(x,.75f,-.5f)},CampaignVisuals.Energy("Sealed gate",new Color(.08f,1,.42f,.65f)),.035f);}}
 }
 static Transform EnsureSeal(Transform island){
  var existing=island.Find("Surface sealed • not walkable");if(existing)return existing;
  var root=Group(island,"Surface sealed • not walkable");var color=new Color(1,.24f,.035f,.82f);
  CampaignVisuals.Ring(root,"Locked circuit",.73f,color,.025f,.22f);
  for(int i=0;i<2;i++){float sign=i==0?1:-1;CampaignVisuals.Line(root,"Closed cross "+i,new[]{new Vector3(-.36f,.225f,-.36f*sign),new Vector3(.36f,.225f,.36f*sign)},CampaignVisuals.Energy("Unwalkable amber",color),.042f);}
  return root;
 }
 public static void SyncSurface(Transform island,Island node,State state,bool collapsed){
  // A closed time gate is a sealed surface, not a missing renderer. Undo also restores old scenes.
  island.gameObject.SetActive(true);var body=ReferenceArt.Part(island,"Base");if(body)body.gameObject.SetActive(true);
  var deck=CampaignVisuals.Deck(island);if(deck)foreach(var renderer in deck.GetComponentsInChildren<Renderer>(true))renderer.enabled=true;
  if(node.kind==TileKind.Rift||node.kind==TileKind.Phase||node.kind==TileKind.Fragile){
   bool closed=collapsed||node.kind==TileKind.Rift&&!Rules.RiftOpen(node,state.tick)||node.kind==TileKind.Phase&&node.phase!=state.phase;
   EnsureSeal(island).gameObject.SetActive(closed);
  }
  if(node.kind==TileKind.Fragile){var visual=island.Find("Reference visual");visual.localPosition=collapsed?Vector3.down*.42f:Vector3.zero;visual.localScale=collapsed?new Vector3(.93f,.48f,.93f):Vector3.one;}
 }
 public static void SyncTrap(Transform island,Island node,State state){
  if(node.kind==TileKind.Laser){var root=island.Find("Laser mechanism");root.Find("Discharge").gameObject.SetActive(Rules.LaserOn(node,state.tick));root.Find("Charging").gameObject.SetActive((node.phase+state.tick)%4==2);}
  if(node.kind==TileKind.Rift){bool open=Rules.RiftOpen(node,state.tick);island.Find("Rift field/Closing warning").gameObject.SetActive(open&&!Rules.RiftOpen(node,(state.tick+1)%4));}
  if(node.kind==TileKind.Relay)island.Find("Relay mechanism/Charged seal").localScale=Vector3.one*((state.switches&(1<<node.channel))!=0?1:.65f);
  if(node.kind==TileKind.Gate)island.Find("Gate mechanism/Energy bars").gameObject.SetActive((state.switches&(1<<node.channel))==0);
 }
 public static void DecorateEnemy(GameObject enemy,Patrol definition){
  var color=EnemyColor(definition.kind);var visual=enemy.transform.Find("Reference visual");
  if(definition.kind!=EnemyKind.Patrol)foreach(var renderer in visual.GetComponentsInChildren<Renderer>()){var source=renderer.sharedMaterial;string key="Enemy/"+definition.kind+"/"+source.GetInstanceID();if(!palette.TryGetValue(key,out var mat)||!mat){mat=new Material(source){name=key};mat.SetColor("_EmissionColor",definition.kind==EnemyKind.Hunter?new Color(6,.25f,.08f):new Color(4,1.4f,.35f));palette[key]=mat;}renderer.sharedMaterial=mat;}
  var role=Group(enemy.transform,definition.kind+" • behavior marker");CampaignVisuals.Ring(role,"Role halo",definition.kind==EnemyKind.Sentinel?.46f:.33f,color,.025f,.04f);
  if(definition.kind==EnemyKind.Sentinel){CampaignVisuals.Ring(role,"Sentry crown",.31f,color,.025f,1.23f);CampaignVisuals.Line(enemy.transform,"Sentry aim",new[]{Vector3.zero,Vector3.zero},CampaignVisuals.Energy("Sentry aim neutral",Color.white),.016f).useWorldSpace=true;}
  if(definition.kind==EnemyKind.Hunter){for(int i=0;i<2;i++)CampaignVisuals.Line(role,"Hunter crest "+i,new[]{new Vector3(-.14f,.02f,-.14f-i*.13f),new Vector3(0,.02f,-.3f-i*.13f),new Vector3(.14f,.02f,-.14f-i*.13f)},CampaignVisuals.Energy("Hunter red",color),.03f);}
 }
 public static void SyncEnemies(Level level,State state,CampaignIsland[] islands,GameObject[] enemies){
  for(int i=0;i<enemies.Length;i++){var p=level.patrols[i];if(p.kind!=EnemyKind.Sentinel)continue;var line=enemies[i].transform.Find("Sentry aim").GetComponent<LineRenderer>();bool fire=Rules.SentinelFiring(p,state.tick),warning=Rules.SentinelWarning(p,state.tick);line.enabled=(fire||warning)&&p.targets.Length>0;if(!line.enabled)continue;line.SetPosition(0,enemies[i].transform.position+Vector3.up*.7f*enemies[i].transform.lossyScale.y);line.SetPosition(1,islands[p.targets[0]].transform.position+Vector3.up*.16f);line.startColor=line.endColor=fire?new Color(1,.045f,.015f,.9f):new Color(1,.65f,.07f,.65f);line.widthMultiplier=fire?.045f:.012f;}
 }
 public static void Theme(GameObject board,int biome){
  foreach(var id in board.GetComponentsInChildren<ReferenceIdentity>(true)){
   if(id.modelId=="Spirit"||id.modelId=="Crystal"||id.modelId=="Portal"||id.modelId=="Shadow")continue;
   foreach(var renderer in id.GetComponentsInChildren<Renderer>(true)){if(renderer is LineRenderer)continue;var source=renderer.sharedMaterial;if(source.shader.name!="Standard")continue;string key=source.GetInstanceID()+"/"+biome;if(!palette.TryGetValue(key,out var mat)||!mat){mat=new Material(source){name="Biome "+biome+" / "+source.name,color=tints[biome%tints.Length]};palette[key]=mat;}renderer.sharedMaterial=mat;}
  }
 }
}

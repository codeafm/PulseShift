using System;
using System.Collections.Generic;

namespace PulseCampaign {
 public enum TileKind { Stone, Phase, Spikes, Fragile, Conveyor, Teleport, Laser, Rift, Relay, Gate }
 public enum EnemyKind { Patrol, Sentinel, Hunter }
 public enum ActionKind { Move, Pulse, Wait, Freeze, Dash }
 public enum Result { Invalid, Safe, Dead, Won }
 [Serializable] public class Island {
  public float x,y,z;
  public TileKind kind;
  public int phase, channel, gem=-1, destination=-1;
 }
 [Serializable] public class Link { public int a,b; public Link(){} public Link(int a,int b){this.a=a;this.b=b;} }
 [Serializable] public class Patrol { public int[] route; public int offset;public EnemyKind kind;public int[] targets=Array.Empty<int>(); }
 [Serializable] public struct Command {
  public ActionKind kind; public int target;
  public Command(ActionKind k,int t=-1){kind=k;target=t;}
  public override string ToString()=>kind+(target>=0?" "+target:"");
 }
 [Serializable] public class Level {
  public int id, chapter, seed, start, goal, pulseBudget, freezeCharges, dashCharges;
  public string title,lesson,layout,challenge;
  public int biome;
  public Island[] islands;
  public Link[] links;
  public Patrol[] patrols;
  public Command[] solution;
  public int optimalTurns,verifiedStates;
  internal uint[] adjacency;
 }
 [Serializable] public class Campaign { public int formatVersion=1;public Level[] levels; }
 public struct State {
  public int cell,phase,tick,gems,pulses,freezes,dashes,frozen,switches;
  public uint enemyCells;
  public uint collapsed;
  public bool dead;
  public ulong Key(bool includePulses=true){
   ulong key=(uint)cell|((ulong)(uint)phase<<5)|((ulong)(uint)tick<<6)|((ulong)(uint)gems<<8)|((ulong)collapsed<<11)|((ulong)(uint)freezes<<32)|((ulong)(uint)dashes<<34)|((ulong)(uint)frozen<<36);
   if(includePulses)key|=(ulong)pulses<<38;
   key|=(ulong)enemyCells<<42;key|=(ulong)(uint)switches<<52;
   return key;
  }
 }
 // This transition function is used by the player, level generator, validator and hint solver.
 // No physics, random numbers or frame time can change puzzle outcomes.
 public static class Rules {
  public static State Initial(Level l){var s=new State{cell=l.start,freezes=l.freezeCharges,dashes=l.dashCharges};for(int i=0;i<l.patrols.Length;i++)SetEnemyCell(ref s,i,GuardAt(l.patrols[i],0));Collect(l,ref s);return s;}
  static uint[] Graph(Level l){if(l.adjacency==null){l.adjacency=new uint[l.islands.Length];foreach(var e in l.links){l.adjacency[e.a]|=1u<<e.b;l.adjacency[e.b]|=1u<<e.a;}}return l.adjacency;}
  public static bool Adjacent(Level l,int a,int b)=>a>=0&&b>=0&&a<l.islands.Length&&b<l.islands.Length&&(Graph(l)[a]&(1u<<b))!=0;
  public static int GuardAt(Patrol p,int tick)=>p.route[(tick+p.offset)%p.route.Length];
  public static int EnemyCell(Level l,State s,int i)=>l.patrols[i].kind==EnemyKind.Hunter?(int)((s.enemyCells>>(i*5))&31):GuardAt(l.patrols[i],s.tick);
  static void SetEnemyCell(ref State s,int i,int cell){s.enemyCells=(s.enemyCells&~(31u<<(i*5)))|((uint)cell<<(i*5));}
  public static bool SpikesUp(Island n,int tick)=>((tick+n.phase)%4)<2;
  public static bool LaserOn(Island n,int tick)=>(tick+n.phase)%4==3;
  public static bool RiftOpen(Island n,int tick)=>(tick+n.phase)%4<2;
  public static bool SentinelFiring(Patrol p,int tick)=>(tick+p.offset)%4==2;
  public static bool SentinelWarning(Patrol p,int tick)=>(tick+p.offset)%4==1;
  public static bool Passable(Level l,State s,int target){
   if(target<0||target>=l.islands.Length||(s.collapsed&(1u<<target))!=0)return false;
   var n=l.islands[target];return (n.kind!=TileKind.Phase||n.phase==s.phase)&&(n.kind!=TileKind.Rift||RiftOpen(n,s.tick))&&(n.kind!=TileKind.Gate||(s.switches&(1<<n.channel))!=0);
  }
  static bool GuardOn(Level l,int cell,State s){for(int i=0;i<l.patrols.Length;i++)if(EnemyCell(l,s,i)==cell)return true;return false;}
  static void Collect(Level l,ref State s){var n=l.islands[s.cell];if(n.gem>=0)s.gems|=1<<n.gem;if(n.kind==TileKind.Relay)s.switches|=1<<n.channel;}
  static void Leave(Level l,ref State s){if(l.islands[s.cell].kind==TileKind.Fragile)s.collapsed|=1u<<s.cell;}
  static bool CanDash(Level l,State s,int target){
   if(target==s.cell||Adjacent(l,s.cell,target))return false;
   // Dash crosses exactly one intermediate island; its surface may be missing.
   foreach(var e in l.links){int mid=e.a==s.cell?e.b:e.b==s.cell?e.a:-1;if(mid>=0&&Adjacent(l,mid,target))return true;}return false;
  }
  public static Result Apply(Level l,State current,Command action,out State next,out string reason){
   next=current;reason="";
   if(current.dead||Won(l,current)){reason="Раунд закончен";return Result.Invalid;}
   bool moved=false;
   switch(action.kind){
    case ActionKind.Move:
    case ActionKind.Dash:
     if(!Passable(l,current,action.target)){reason="Платформа закрыта или разрушена";return Result.Invalid;}
     if(action.kind==ActionKind.Move&&!Adjacent(l,current.cell,action.target)){reason="Выбери соседний остров";return Result.Invalid;}
     if(action.kind==ActionKind.Dash&&(current.dashes<=0||!CanDash(l,current,action.target))){reason="Рывок перескакивает через один остров";return Result.Invalid;}
     if(action.kind==ActionKind.Dash)next.dashes--;
     Leave(l,ref next);next.cell=action.target;moved=true;
     if(GuardOn(l,next.cell,current)){next.dead=true;reason="Прыжок прямо к тени";return Result.Dead;}
     Collect(l,ref next);break;
    case ActionKind.Pulse:
     if(current.pulses>=l.pulseBudget){reason="Импульсы закончились";return Result.Invalid;}
     next.pulses++;next.phase^=1;break;
    case ActionKind.Freeze:
     if(next.freezes<=0){reason="Нет зарядов остановки";return Result.Invalid;}
     next.freezes--;next.frozen=3;break;
    case ActionKind.Wait:break;
    default:reason="Неизвестное действие";return Result.Invalid;
   }
   if(next.frozen>0)next.frozen--;else next.tick=(next.tick+1)%4;
   if(!Passable(l,next,next.cell)){next.dead=true;reason=l.islands[next.cell].kind==TileKind.Rift?"Энергетическая печать закрылась на этом такте":"Мост под Искрой закрылся";return Result.Dead;}
   if(moved){
    // Forced transport has a bounded chain and cycle guard. It never grants extra world turns.
    uint visited=0;
    for(int i=0;i<l.islands.Length;i++){
     var n=l.islands[next.cell];
     if(n.kind!=TileKind.Conveyor&&n.kind!=TileKind.Teleport)break;
     if((visited&(1u<<next.cell))!=0){next.dead=true;reason="Петля перемещения";return Result.Dead;}visited|=1u<<next.cell;
     if(!Passable(l,next,n.destination)){next.dead=true;reason="Выход перемещения закрыт";return Result.Dead;}
     Leave(l,ref next);next.cell=n.destination;Collect(l,ref next);
     if(n.kind==TileKind.Teleport)break;
    }
   }
   var tile=l.islands[next.cell];
   AdvanceEnemies(l,current,ref next,action.kind);
   if(tile.kind==TileKind.Spikes&&SpikesUp(tile,next.tick)){next.dead=true;reason="Шипы поднялись на этом ходу";return Result.Dead;}
   if(tile.kind==TileKind.Laser&&LaserOn(tile,next.tick)){next.dead=true;reason="Лазер разрядился на этом такте";return Result.Dead;}
   if(GuardOn(l,next.cell,next)){next.dead=true;reason="Тень перехватила Искру";return Result.Dead;}
   foreach(var p in l.patrols)if(p.kind==EnemyKind.Sentinel&&SentinelFiring(p,next.tick)&&Array.IndexOf(p.targets,next.cell)>=0){next.dead=true;reason="Дозорный выстрелил в отмеченный остров";return Result.Dead;}
   if(Won(l,next))return Result.Won;
   return Result.Safe;
  }
  public static bool Won(Level l,State s)=>!s.dead&&s.cell==l.goal&&s.gems==7;
  static void AdvanceEnemies(Level l,State before,ref State next,ActionKind action){
   if(before.tick==next.tick||action==ActionKind.Pulse)return;
   for(int i=0;i<l.patrols.Length;i++){
    var p=l.patrols[i];if(p.kind!=EnemyKind.Hunter||(next.tick+p.offset)%2!=0)continue;
    int cell=EnemyCell(l,before,i);uint front=1u<<next.cell,seen=front,previous=0,allowed=0;
    for(int n=0;n<l.islands.Length;n++)if(Passable(l,next,n))allowed|=1u<<n;
    // Pursue along a shortest traversable route within three links. Ascending cell
    // index breaks ties deterministically; the solver and danger preview use this too.
    for(int depth=0;depth<=3;depth++){
     if((front&(1u<<cell))!=0){uint choices=Graph(l)[cell]&previous;for(int n=0;n<l.islands.Length;n++)if((choices&(1u<<n))!=0&&!GuardOnOther(l,n,next,i)){SetEnemyCell(ref next,i,n);break;}break;}
     previous=front;uint following=0;for(int n=0;n<l.islands.Length;n++)if((front&(1u<<n))!=0)following|=Graph(l)[n];front=following&allowed&~seen;seen|=front;
    }
   }
  }
  static bool GuardOnOther(Level l,int cell,State s,int except){for(int i=0;i<l.patrols.Length;i++)if(i!=except&&EnemyCell(l,s,i)==cell)return true;return false;}
  public static State PreviewEnemies(Level l,State s){var next=s;if(next.frozen>0)next.frozen--;else next.tick=(next.tick+1)%4;AdvanceEnemies(l,s,ref next,ActionKind.Wait);return next;}
  public static bool Threatened(Level l,State s,int cell){if(GuardOn(l,cell,s))return true;foreach(var p in l.patrols)if(p.kind==EnemyKind.Sentinel&&SentinelFiring(p,s.tick)&&Array.IndexOf(p.targets,cell)>=0)return true;return false;}
  public static IEnumerable<Command> Actions(Level l,State s){
   for(int i=0;i<l.islands.Length;i++)if(Adjacent(l,s.cell,i))yield return new Command(ActionKind.Move,i);
   yield return new Command(ActionKind.Wait);yield return new Command(ActionKind.Pulse);
   if(s.freezes>0)yield return new Command(ActionKind.Freeze);
   if(s.dashes>0)for(int i=0;i<l.islands.Length;i++)if(CanDash(l,s,i))yield return new Command(ActionKind.Dash,i);
  }
 }
 public class SearchResult { public Command[] path;public int explored;public bool exhausted; }
 public static class Solver {
  struct Record {public State state;public int parent;public Command action;}
  public static SearchResult Solve(Level l,State initial,int limit=250000){
   if(Rules.Won(l,initial))return new SearchResult{path=Array.Empty<Command>(),explored=1};
   var records=new List<Record>{new Record{state=initial,parent=-1}};
   var seen=new Dictionary<ulong,int>{{initial.Key(false),initial.pulses}};
   int cursor=0;
   while(cursor<records.Count&&records.Count<limit){var current=records[cursor];int parent=cursor++;
    foreach(var a in Rules.Actions(l,current.state)){
     var result=Rules.Apply(l,current.state,a,out var state,out _);
     if(result==Result.Invalid||result==Result.Dead)continue;
     var key=state.Key(false);if(seen.TryGetValue(key,out int pulses)&&pulses<=state.pulses)continue;seen[key]=state.pulses;
     records.Add(new Record{state=state,parent=parent,action=a});
     if(result==Result.Won){var path=new List<Command>();int at=records.Count-1;while(records[at].parent>=0){path.Add(records[at].action);at=records[at].parent;}path.Reverse();return new SearchResult{path=path.ToArray(),explored=records.Count};}
    }
   }
   return new SearchResult{path=null,explored=records.Count,exhausted=cursor>=records.Count};
  }
  public static State Replay(Level l,Command[] path){var s=Rules.Initial(l);for(int i=0;i<path.Length;i++){var result=Rules.Apply(l,s,path[i],out var next,out var why);if(result==Result.Invalid||result==Result.Dead)throw new InvalidOperationException($"Level {l.id}, step {i}: {why}");s=next;}if(!Rules.Won(l,s))throw new InvalidOperationException($"Level {l.id}: witness does not reach victory");return s;}
 }
}

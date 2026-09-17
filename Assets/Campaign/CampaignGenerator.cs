using System;
using System.Collections.Generic;
using System.Linq;
namespace PulseCampaign {
 public static class CampaignGenerator {
  public static readonly string[] Chapters={"Тропы света","Ритм опасности","Хрупкие маршруты","Тени и дозорные","Печати архипелага","Потоки и разломы","Охота за Искрой","Две стороны времени","Арена механизмов","Сердце архипелага"};
  public static readonly string[] Layouts={"Кольцо","Боковые тропы","Две лестницы","Созвездие","Серпантин","Восьмёрка","Развилки","Лабиринт","Двор с мостами","Три крепости"};
  static readonly string[] Lessons={
   "Собери три кристалла и найди портал. У некоторых островов есть обходные пути. Голубое кольцо — безопасное приземление, красное — опасное.",
   "Шипы активны два такта из четырёх. Лазер сначала заряжается жёлтым, затем стреляет красным на один такт. Считай следующий ход.",
   "Треснувшая плита ломается после ухода. Призрачный остров появляется на два такта и исчезает на два. Не оставайся на нём надолго.",
   "Патруль ходит по маршруту. Золотой дозорный стоит на месте: жёлтый луч предупреждает, красный стреляет на следующий такт.",
   "Зелёная печать открывает ворота с таким же знаком. Найди её до похода к порталу; открытые ворота больше не закрываются.",
   "Стрелка переносит на связанный выход, фиолетовый разлом — на парный остров. Время делает только один шаг. Смотри на линию связи.",
   "Красный охотник замечает Искру в трёх переходах и сближается через ход. Импульс задерживает его на этот ход. Можно остановить время или прыгнуть через остров.",
   "Меняй фазу, следи за исчезающими островами и сбивай охотника импульсом. Не закрывай мост под собой.",
   "Разные враги и механизмы действуют вместе. Подсветка приземления учитывает их следующий ход; отмена возвращает все ресурсы.",
   "Финальные испытания: найди порядок кристаллов, печатей и безопасных переходов. У каждой карты есть проверенное решение."
  };
  class Map {
   public List<Island> n=new();public List<Link> e=new();
   public int Add(float x,float z){n.Add(new Island{x=x,z=z});return n.Count-1;}
   public void Edge(int a,int b){if(a!=b&&!e.Any(v=>v.a==a&&v.b==b||v.a==b&&v.b==a))e.Add(new Link(a,b));}
   public void Path(params int[] p){for(int i=1;i<p.Length;i++)Edge(p[i-1],p[i]);}
  }
  public static Campaign Generate(int count=100){
   if(count<1||count>10000)throw new ArgumentOutOfRangeException(nameof(count));
   var levels=new List<Level>();var signatures=new HashSet<string>();var topologyUses=new Dictionary<string,int>();
   for(int id=1;id<=count;id++){
    Level accepted=null;
    for(int attempt=0;attempt<700;attempt++){
     var l=Candidate(id,attempt);if(!ClearLayout(l))continue;string signature=Signature(l),topology=TopologySignature(l);
     if(signatures.Contains(signature)||topologyUses.TryGetValue(topology,out var used)&&used>=1)continue;
     ValidateDefinition(l);var result=Solver.Solve(l,Rules.Initial(l),120000);
     int minimum=id<=10?6:id<=30?8:id<=60?10:id<=80?11:13;
     if(result.path==null||result.path.Length<minimum||result.path.Length>38)continue;
     int pulseCount=result.path.Count(a=>a.kind==ActionKind.Pulse);
     if(l.islands.Any(n=>n.kind==TileKind.Phase)&&pulseCount==0&&id>=5)continue;
     // New mechanics must occur on the witnessed route, not merely decorate an unused branch.
     var visited=new HashSet<int>{l.start};var s=Rules.Initial(l);
     foreach(var action in result.path){Rules.Apply(l,s,action,out s,out _);visited.Add(s.cell);if(action.target>=0)visited.Add(action.target);}
     if(l.islands.Any(n=>n.kind==TileKind.Relay)&&s.switches==0)continue;
     if(id>=11&&!visited.Any(i=>l.islands[i].kind!=TileKind.Stone&&l.islands[i].kind!=TileKind.Gate))continue;
     l.pulseBudget=Math.Min(15,Math.Max(1,pulseCount+(id<=20?2:id<=70?1:0)));
     l.solution=result.path;l.optimalTurns=result.path.Length;l.verifiedStates=result.explored;
     Solver.Replay(l,l.solution);accepted=l;signatures.Add(signature);topologyUses[topology]=topologyUses.TryGetValue(topology,out used)?used+1:1;break;
    }
    if(accepted==null)throw new InvalidOperationException("No verified diverse candidate for level "+id);
    levels.Add(accepted);Console.WriteLine("GENERATED "+id+" / "+accepted.layout+" / "+accepted.optimalTurns+" turns");
   }
   return new Campaign{formatVersion=2,levels=levels.ToArray()};
  }
  static Level Candidate(int id,int attempt){
   int chapter=Math.Min((id-1)/10,9),family=(id-1)%10;var random=new Random(48071+id*919+attempt*7919);var b=new Map();
   int start=0,goal=0;int rows=4+(chapter>=4?1:0)+(chapter>=8&&random.Next(2)==0?1:0);
   switch(family){
    case 0:{
     int count=8+2*random.Next(chapter>=4?4:3);float rx=count>=14?4:3.5f;
     for(int i=0;i<count;i++){double a=i*Math.PI*2/count;b.Add((float)Math.Sin(a)*rx,-(float)Math.Cos(a)*5.5f);}
     for(int i=0;i<count;i++)b.Edge(i,(i+1)%count);
     start=0;goal=count/2;
     for(int i=0;i<1+chapter/3;i++){int a=random.Next(count),c=(a+2)%count;if(Distance(b.n[a],b.n[c])<5)b.Edge(a,c);}
     if(random.Next(2)==0&&b.n.Count<19){int a=random.Next(count);var n=b.n[a];int tip=b.Add(n.x*1.55f,n.z*1.3f);b.Edge(a,tip);}break;
    }
    case 1:{
     int previous=-1;for(int row=0;row<rows;row++){int hub=b.Add(0,row*2.7f);if(previous>=0)b.Edge(previous,hub);previous=hub;
      if(row>0&&row<rows-1||row==0){int side=random.Next(2)==0?-1:1;b.Edge(hub,b.Add(side*2.5f,row*2.7f));if(random.Next(3)==0)b.Edge(hub,b.Add(-side*2.5f,row*2.7f));}}
     start=0;goal=previous;break;
    }
    case 2:{
     int[,] rail=new int[rows,2];for(int row=0;row<rows;row++)for(int col=0;col<2;col++){rail[row,col]=b.Add((col==0?-1:1)*1.6f,row*2.5f);if(row>0)b.Edge(rail[row-1,col],rail[row,col]);}
     for(int row=0;row<rows;row++)if(row==0||row==rows-1||random.Next(3)!=0)b.Edge(rail[row,0],rail[row,1]);
     start=rail[0,0];goal=rail[rows-1,1];break;
    }
    case 3:{
     int hub=b.Add(0,0);var tips=new List<int>();int arms=5+random.Next(2);
     for(int i=0;i<arms;i++){double a=i*Math.PI*2/arms;int prev=hub,length=2+(chapter>=7&&i==2?1:0);
      for(int j=1;j<=length;j++){int n=b.Add((float)Math.Sin(a)*j*2.7f,(float)Math.Cos(a)*j*2.7f);b.Edge(prev,n);prev=n;}tips.Add(prev);}
     if(random.Next(2)==0)b.Edge(tips[0],tips[1]);start=Enumerable.Range(0,b.n.Count).OrderBy(i=>b.n[i].z).First();goal=tips[0];break;
    }
    case 4:
    case 7:{
     rows=Math.Min(rows,6);int[,] grid=new int[rows,3];for(int row=0;row<rows;row++)for(int col=0;col<3;col++)grid[row,col]=b.Add((col-1)*2.45f,row*2.45f);
     start=grid[0,0];goal=grid[rows-1,rows%2==0?0:2];
     if(family==4){var path=new List<int>();for(int row=0;row<rows;row++)for(int x=0;x<3;x++)path.Add(grid[row,row%2==0?x:2-x]);b.Path(path.ToArray());}
     else{var seen=new HashSet<int>{start};var stack=new Stack<int>();stack.Push(start);while(stack.Count>0){int at=stack.Peek(),row=at/3,col=at%3;var options=new List<int>();if(row>0)options.Add(grid[row-1,col]);if(row<rows-1)options.Add(grid[row+1,col]);if(col>0)options.Add(grid[row,col-1]);if(col<2)options.Add(grid[row,col+1]);options=options.Where(n=>!seen.Contains(n)).ToList();if(options.Count==0){stack.Pop();continue;}int next=options[random.Next(options.Count)];b.Edge(at,next);seen.Add(next);stack.Push(next);}}
     for(int i=0;i<1+random.Next(3)+chapter/4;i++){int row=random.Next(rows-1),col=random.Next(3);b.Edge(grid[row,col],grid[row+1,col]);}
     break;
    }
    case 5:{
     int hub=b.Add(0,0);int lb=b.Add(-2.6f,-2.6f),ll=b.Add(-2.6f,-5.2f),bottom=b.Add(0,-6.6f),rr=b.Add(2.6f,-5.2f),rb=b.Add(2.6f,-2.6f);b.Path(hub,lb,ll,bottom,rr,rb,hub);
     int ul=b.Add(-2.6f,2.6f),tl=b.Add(-2.6f,5.2f),top=b.Add(0,6.6f),tr=b.Add(2.6f,5.2f),ur=b.Add(2.6f,2.6f);b.Path(hub,ul,tl,top,tr,ur,hub);
     if(random.Next(2)==0)b.Edge(lb,rb);if(random.Next(2)==0)b.Edge(ul,ur);
     if(random.Next(2)==0)b.Edge(ll,rr);if(random.Next(2)==0)b.Edge(tl,tr);
     if(chapter>=3){int tip=b.Add(random.Next(2)==0?-2.7f:2.7f,0);b.Edge(hub,tip);}
     start=bottom;goal=top;break;
    }
    case 6:{
     int previous=b.Add(0,0);start=previous;int rooms=2+random.Next(chapter>=4?3:2);
     for(int room=0;room<rooms;room++){float z=room*4.8f;int left=b.Add(-2.25f,z+2.4f),right=b.Add(2.25f,z+2.4f),next=b.Add(0,z+4.8f);b.Edge(previous,left);b.Edge(previous,right);b.Edge(left,next);b.Edge(right,next);if(random.Next(2)==0)b.Edge(left,right);previous=next;}goal=previous;break;
    }
    case 8:{
     int[,] grid=new int[5,3];for(int row=0;row<5;row++)for(int col=0;col<3;col++)grid[row,col]=-1;
     for(int row=0;row<5;row++)for(int col=0;col<3;col++)if(col!=1||row==0||row==4||row==1+random.Next(3))grid[row,col]=b.Add((col-1)*2.5f,row*2.5f);
     for(int row=0;row<5;row++)for(int col=0;col<3;col++){int at=grid[row,col];if(at<0)continue;if(row<4&&grid[row+1,col]>=0)b.Edge(at,grid[row+1,col]);if(col<2&&grid[row,col+1]>=0)b.Edge(at,grid[row,col+1]);}
     start=grid[0,1];goal=grid[4,1];break;
    }
    default:{
     int prev=-1;for(int room=0;room<3;room++){float x=(room%2==0?-1:1)*.65f,z=room*5.2f;int a=b.Add(x-1.35f,z),c=b.Add(x+1.35f,z),d=b.Add(x+1.35f,z+2.5f),e=b.Add(x-1.35f,z+2.5f);b.Path(a,c,d,e,a);if(random.Next(2)==0)b.Edge(a,d);if(chapter>=4&&random.Next(3)==0)b.Edge(c,e);if(prev>=0)b.Edge(prev,random.Next(2)==0?a:c);else start=a;prev=random.Next(2)==0?d:e;}goal=prev;break;
    }
   }
   // An optional reward spur changes the actual route graph, not just its orientation or colour.
   if(b.n.Count<=18&&random.Next(3)!=0){
    int side=random.Next(2)==0?-1:1;float edge=side<0?b.n.Min(n=>n.x):b.n.Max(n=>n.x);
    var boundary=Enumerable.Range(0,b.n.Count).Where(i=>Math.Abs(b.n[i].x-edge)<.1f).ToArray();int root=boundary[random.Next(boundary.Length)];
    int tip=b.Add(b.n[root].x+side*2.6f,b.n[root].z+(random.Next(2)==0?-.8f:.8f));b.Edge(root,tip);
    if(random.Next(3)==0){int end=b.Add(b.n[tip].x,b.n[tip].z+(random.Next(2)==0?-2.6f:2.6f));b.Edge(tip,end);}
   }
   // Radial maps need a portrait silhouette; preserve usable island size on a phone.
   if(family==3)foreach(var node in b.n)node.x*=.72f;
   // Reindex start/goal is unnecessary: the game supports arbitrary node indices.
   float minZ=b.n.Min(n=>n.z),maxZ=b.n.Max(n=>n.z),centerX=(b.n.Min(n=>n.x)+b.n.Max(n=>n.x))*.5f;
   bool mirror=(id+attempt)%2==1;foreach(var n in b.n){n.x=(n.x-centerX)*(mirror?-1:1);n.z-= (minZ+maxZ)*.5f;n.y=(n.z+(maxZ-minZ)*.5f)*.025f;}
   var available=Enumerable.Range(0,b.n.Count).Where(i=>i!=start&&i!=goal).OrderBy(_=>random.Next()).ToList();
   // Spread memories over different sectors, with preference for side branches and dead ends.
   var gemChoices=new List<int>();for(int g=0;g<3;g++){var options=available.Where(i=>!gemChoices.Contains(i)).OrderByDescending(i=>gemChoices.Count==0?Distance(b.n[start],b.n[i]):gemChoices.Min(j=>Distance(b.n[j],b.n[i]))).Take(Math.Max(2,available.Count/3)).ToArray();int chosen=options[random.Next(options.Length)];gemChoices.Add(chosen);b.n[chosen].gem=g;}
   int Take(){if(available.Count==0)return -1;int n=available[0];available.RemoveAt(0);return n;}
   void Place(TileKind kind,int count=1){for(int j=0;j<count;j++){int n=Take();if(n<0)break;b.n[n].kind=kind;b.n[n].phase=kind==TileKind.Phase?1:random.Next(4);}}
   if(id>=5&&(chapter==0||chapter==7||random.Next(3)==0))Place(TileKind.Phase,chapter>=7?2:1);
   if(chapter>=1){if(chapter==1||random.Next(2)==0)Place(TileKind.Spikes,chapter>=8?2:1);if(id>=15&&(chapter==1||random.Next(2)==0))Place(TileKind.Laser);}
   if(chapter>=2){if(chapter==2||random.Next(2)==0)Place(TileKind.Fragile,chapter>=7?2:1);if(id>=25&&(chapter==2||chapter==7||random.Next(3)==0))Place(TileKind.Rift);}
   if(chapter>=4&&(chapter==4||random.Next(2)==0)){int key=Take();if(key>=0){b.n[key].kind=TileKind.Relay;b.n[key].channel=0;b.n[goal].kind=TileKind.Gate;b.n[goal].channel=0;}}
   if(chapter>=5){
    if(chapter==5||random.Next(2)==0){int n=Take();if(n>=0){var exits=b.e.Where(e=>e.a==n||e.b==n).Select(e=>e.a==n?e.b:e.a).Where(i=>i!=start).ToArray();if(exits.Length>0){b.n[n].kind=TileKind.Conveyor;b.n[n].destination=exits[random.Next(exits.Length)];}}}
    if((chapter==5&&id%2==0||chapter>=7&&random.Next(3)==0)&&available.Count>=2){int a=Take(),c=Take();b.n[a].kind=b.n[c].kind=TileKind.Teleport;b.n[a].destination=c;b.n[c].destination=a;}
   }
   var patrols=new List<Patrol>();
   if(chapter>=3){
    int count=chapter>=8?2:1;var occupied=new HashSet<int>();for(int p=0;p<count;p++){
     var pool=Enumerable.Range(0,b.n.Count).Where(i=>i!=start&&i!=goal&&!occupied.Contains(i)&&b.n[i].gem<0&&b.n[i].kind!=TileKind.Relay).OrderBy(_=>random.Next()).ToArray();
     if(pool.Length==0)break;int home=pool[0];var neighbors=b.e.Where(e=>e.a==home||e.b==home).Select(e=>e.a==home?e.b:e.a).Where(i=>i!=start&&i!=goal&&!occupied.Contains(i)).ToArray();if(neighbors.Length==0)continue;
     EnemyKind kind=chapter>=6&&(p==0||random.Next(2)==0)?EnemyKind.Hunter:(id+attempt+p)%2==0?EnemyKind.Sentinel:EnemyKind.Patrol;
     int other=neighbors[random.Next(neighbors.Length)];var enemy=new Patrol{kind=kind,offset=random.Next(4)};
     if(kind==EnemyKind.Patrol){enemy.route=random.Next(2)==0?new[]{home,home,other,other}:new[]{home,other};occupied.Add(other);}
     else{enemy.route=new[]{home};if(kind==EnemyKind.Sentinel)enemy.targets=new[]{other};}
     occupied.Add(home);patrols.Add(enemy);
    }
   }
   return new Level{id=id,chapter=chapter,seed=48071+id*919+attempt*7919,start=start,goal=goal,islands=b.n.ToArray(),links=b.e.ToArray(),patrols=patrols.ToArray(),pulseBudget=10,freezeCharges=chapter>=6?1:0,dashCharges=chapter>=6?1:0,layout=Layouts[family],biome=(chapter+family/2)%5,title=Layouts[family]+" · "+(id).ToString("00"),lesson=Lessons[chapter],challenge=chapter>=4&&b.n[goal].kind==TileKind.Gate?"Найди печать · собери свет":"Три кристалла · один портал"};
  }
  static float Distance(Island a,Island b){float x=a.x-b.x,z=a.z-b.z;return (float)Math.Sqrt(x*x+z*z);}
  static bool ClearLayout(Level l){for(int i=0;i<l.islands.Length;i++)for(int j=i+1;j<l.islands.Length;j++)if(Distance(l.islands[i],l.islands[j])<1.72f)return false;return true;}
  public static string Signature(Level l){var parts=new List<string>();foreach(var n in l.islands)parts.Add($"{(int)n.kind},{n.phase},{n.channel},{n.gem},{n.destination}");foreach(var e in l.links)parts.Add($"{e.a}-{e.b}");foreach(var p in l.patrols)parts.Add(p.kind+":"+string.Join(",",p.route)+"@"+p.offset+":"+string.Join(",",p.targets));return string.Join(";",parts);}
  public static string TopologySignature(Level l){
   uint[] labels=l.islands.Select((_,i)=>(uint)l.links.Count(e=>e.a==i||e.b==i)).ToArray();
   for(int round=0;round<4;round++){var next=new uint[labels.Length];for(int i=0;i<labels.Length;i++){uint hash=2166136261;unchecked{hash=(hash^labels[i])*16777619;foreach(var n in l.links.Where(e=>e.a==i||e.b==i).Select(e=>labels[e.a==i?e.b:e.a]).OrderBy(n=>n))hash=(hash^n)*16777619;}next[i]=hash;}labels=next;}
   return labels.Length+"/"+l.links.Length+"/"+string.Join(",",labels.OrderBy(v=>v));
  }
  public static void ValidateDefinition(Level l){
   if(l.islands==null||l.islands.Length<3||l.islands.Length>20)throw new Exception("Invalid island count");
   if(l.start<0||l.start>=l.islands.Length||l.goal<0||l.goal>=l.islands.Length||l.start==l.goal)throw new Exception("Invalid start/goal");
   if(l.chapter<0||l.chapter>=Chapters.Length)throw new Exception("Unknown campaign chapter");
   if(l.freezeCharges<0||l.freezeCharges>3||l.dashCharges<0||l.dashCharges>3)throw new Exception("Ability charges must fit the solver state");
   if(l.links==null||l.patrols==null||l.patrols.Length>2)throw new Exception("Missing routes or too many enemies");
   foreach(var n in l.islands){if(n==null||!Enum.IsDefined(typeof(TileKind),n.kind))throw new Exception("Unknown island type");if(float.IsNaN(n.x)||float.IsNaN(n.y)||float.IsNaN(n.z)||float.IsInfinity(n.x)||float.IsInfinity(n.y)||float.IsInfinity(n.z))throw new Exception("Invalid island position");if(n.kind==TileKind.Phase&&(n.phase<0||n.phase>1)||n.phase<0||n.phase>3||n.channel<0||n.channel>2)throw new Exception("Invalid trap phase/channel");}
   if(!l.islands.Where(n=>n.gem>=0).Select(n=>n.gem).OrderBy(n=>n).SequenceEqual(new[]{0,1,2}))throw new Exception("Exactly three distinct gems required");
   foreach(var e in l.links)if(e.a<0||e.b<0||e.a>=l.islands.Length||e.b>=l.islands.Length||e.a==e.b)throw new Exception("Invalid link");
   if(l.links.Select(e=>Math.Min(e.a,e.b)+"-"+Math.Max(e.a,e.b)).Distinct().Count()!=l.links.Length)throw new Exception("Duplicate link");
   var reached=new HashSet<int>{l.start};for(int pass=0;pass<l.islands.Length;pass++)foreach(var e in l.links){if(reached.Contains(e.a))reached.Add(e.b);if(reached.Contains(e.b))reached.Add(e.a);}if(reached.Count!=l.islands.Length)throw new Exception("Disconnected island");
   foreach(var p in l.patrols){if(!Enum.IsDefined(typeof(EnemyKind),p.kind)||p.route==null||(p.route.Length!=1&&p.route.Length!=2&&p.route.Length!=4)||p.offset<0||p.offset>3)throw new Exception("Invalid enemy period");foreach(var cell in p.route)if(cell<0||cell>=l.islands.Length||cell==l.start||cell==l.goal)throw new Exception("Invalid enemy island");if(p.kind==EnemyKind.Patrol)for(int i=0;i<p.route.Length;i++)if(p.route[i]!=p.route[(i+1)%p.route.Length]&&!Rules.Adjacent(l,p.route[i],p.route[(i+1)%p.route.Length]))throw new Exception("Patrol crosses a missing connection");if(p.targets==null)throw new Exception("Missing sentry targets");foreach(var cell in p.targets)if(cell<0||cell>=l.islands.Length||!Rules.Adjacent(l,p.route[0],cell))throw new Exception("Invalid sentry attack target");}
   for(int i=0;i<l.islands.Length;i++){var n=l.islands[i];if(n.kind==TileKind.Conveyor||n.kind==TileKind.Teleport){if(n.destination<0||n.destination>=l.islands.Length||n.destination==i)throw new Exception("Invalid transport");if(n.kind==TileKind.Conveyor&&!Rules.Adjacent(l,i,n.destination))throw new Exception("Conveyor must have a connected exit");}if(n.kind==TileKind.Gate&&!l.islands.Any(k=>k.kind==TileKind.Relay&&k.channel==n.channel))throw new Exception("Gate without matching relay");}
   if(l.pulseBudget<0||l.pulseBudget>15)throw new Exception("Invalid energy budget");
  }
 }
}

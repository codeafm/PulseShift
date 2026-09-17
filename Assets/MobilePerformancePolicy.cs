using System;

// Conservative startup hint, not a benchmark. Unknown hardware can be assessed at runtime.
public static class MobilePerformancePolicy {
 public static bool NeedsLite(int memoryMb,int processors,string gpu){
  string name=(gpu??"").ToLowerInvariant();
  return (memoryMb>0&&memoryMb<=4096)||(processors>0&&processors<=4)
   ||name.Contains("mali-g52")||name.Contains("mali-g51")||name.Contains("mali-t")
   ||name.Contains("adreno (tm) 5")||name.Contains("adreno (tm) 610")||name.Contains("adreno (tm) 612");
 }
 public static int TargetFps(bool mobile,bool hardwareLite,bool sessionLite,int quality,bool wants60)
  =>!wants60||mobile&&(hardwareLite||sessionLite||quality==0)?30:60;
 public static bool SlowWindow(float elapsed,int frames,int target)
  =>target==60&&frames>=30&&elapsed>=6&&frames/elapsed<45;
}

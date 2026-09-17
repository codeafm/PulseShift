using UnityEngine;
using PulseCampaign;
public class CampaignIsland:MonoBehaviour {
 public int index;
 public TileKind kind;
 [Tooltip("Connections are defined in Resources/Campaign/levels.json; moving artwork does not change puzzle rules.")]
 public int[] connectedIslands;
}

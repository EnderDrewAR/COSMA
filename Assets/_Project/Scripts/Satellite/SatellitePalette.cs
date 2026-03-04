using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SatellitePalette", menuName = "Satellite/Palette")]
public class SatellitePalette : ScriptableObject 
{
    public List<BlockData> Blocks;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Effect_Tsubaki_Alpha : Alpha_Effect_Base
{
    public Effect_Tsubaki_Alpha(int pos, int r) : base(pos, r) { }
    public override Alpha_Effect_Base Clone() { return new Effect_Tsubaki_Alpha(equipPosition, rarity); }
}

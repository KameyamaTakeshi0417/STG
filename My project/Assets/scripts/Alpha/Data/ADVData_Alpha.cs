using UnityEngine;
using System.Collections.Generic;

namespace Alpha.Data
{
    [CreateAssetMenu(fileName = "NewADVData", menuName = "Alpha/ADV Data")]
    public class ADVData_Alpha : ScriptableObject
    {
        public List<ADVPage_Alpha> pages = new List<ADVPage_Alpha>();
    }
}

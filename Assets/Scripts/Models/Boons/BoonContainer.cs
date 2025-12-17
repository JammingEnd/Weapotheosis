using System.Collections.Generic;
using UnityEngine;

namespace Models.Boons
{
    [CreateAssetMenu(fileName = "BoonContainer", menuName = "Boons/BoonContainer", order = 2)]
    public class BoonContainer : ScriptableObject
    {
        public List<BoonCardSC> AvailableBoons = new List<BoonCardSC>();
        
        private Dictionary<int, BoonCardSC> _lookup;
        
        public void Initialize()
        {
            _lookup = new Dictionary<int, BoonCardSC>();
            for (int i = 0; i < AvailableBoons.Count; i++)
            {
                AvailableBoons[i].BoonId = i;
                _lookup.Add(i, AvailableBoons[i]);
            }
        }
        public BoonCardSC GetBoonById(int id)
        {
            if (_lookup == null)
            {
                Initialize();
            }
            return _lookup[id];
        }
    }
}
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Hammer : Item, ITargeting
    {
        [SerializeField] private int baseRepairStength = 10;
        public void UseOn(ITargetable target)
        {
            if (target is IRepairable)
            {
                (target as IRepairable).Repair(baseRepairStength);
                Debug.Log($"Repairing {target} for {baseRepairStength}");
            }
        }
    }
}
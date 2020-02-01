using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class RepairItem : Item, ITargeting
    {
        public int baseRepairStength;
        public void UseOn(ITargetable target)
        {
            if (timeSinceLastUse >= useCooldown)
            {
                timeSinceLastUse = TimeSpan.Zero;
                if (target is IRepairable)
                {
                    (target as IRepairable).Repair(baseRepairStength);
                    Debug.Log($"Repairing {target} for {baseRepairStength}");
                }
            }
        }
    }
}
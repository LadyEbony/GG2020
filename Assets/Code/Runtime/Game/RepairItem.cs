using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class RepairItem : Item, ITargeting
    {
        public int baseRepairStength;
        public float range;
        public void UseOn(ITargetable target)
        {
            if (timeSinceLastUse >= useCooldown)
            {
                timeSinceLastUse = TimeSpan.Zero;
                if (target is IRepairable)
                {
                    if ((target.GetTarget().transform.position - gameObject.transform.position).magnitude < range)
                    {
                        (target as IRepairable).Repair(baseRepairStength);
                        Debug.Log($"Repairing {target} for {baseRepairStength}");
                    }
                }
            }
        }
    }
}
using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class DamageItem : Item, ITargeting
    {
        public int baseDamageStrength;
        public void UseOn(ITargetable target)
        {
            if (timeSinceLastUse >= useCooldown)
            {
                timeSinceLastUse = TimeSpan.Zero;
                if (target is IRepairable)
                {
                    (target as IRepairable).Repair(baseDamageStrength);
                    Debug.Log($"Repairing {target} for {baseDamageStrength}");
                }
            }
        }
    }
}
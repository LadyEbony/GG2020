using System;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class DamageItem : Item, ITargeting, IRanged
    {
        public int baseDamageStrength;
        public float range;
        public float GetRange() => range;
        public void UseOn(ITargetable target)
        {
            if (timeSinceLastUse >= useCooldown)
            {
                timeSinceLastUse = TimeSpan.Zero;
                if (target is IDamageable)
                {
                    //if ((target.GetTarget().transform.position - gameObject.transform.position).magnitude < range)
                    //{
                        (target as IDamageable).Damage(baseDamageStrength);
                        Debug.Log($"Damaging {target} for {baseDamageStrength}");
                    //}
                    //else
                    //{
                    //    Debug.Log($"{target} is out of range");
                    //}
                }
            }
        }
    }
}
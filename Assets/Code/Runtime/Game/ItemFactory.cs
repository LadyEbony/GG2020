using System;
using UnityEngine;

namespace Code.Runtime.Game
{
    public static class ItemFactory
    {
        public static RepairItem Mortar(GameObject owner)
        {
            RepairItem mortar = owner.AddComponent<RepairItem>();
            mortar.name = "Mortar";
            mortar.baseRepairStength = 20;
            mortar.useCooldown = TimeSpan.FromMilliseconds(1000);
            mortar.range = 2;
            return mortar;
        }

        public static RepairItem Hammer(GameObject owner)
        {
            RepairItem hammer = owner.AddComponent<RepairItem>();
            hammer.name = "Hammer";
            hammer.baseRepairStength = 5;
            hammer.useCooldown = TimeSpan.FromMilliseconds(500);
            hammer.range = 2;
            return hammer;
        }

        public static DamageItem Attack(GameObject owner)
        {
            DamageItem attack = owner.AddComponent<DamageItem>();
            attack.name = "Attack";
            attack.baseDamageStrength = 25;
            attack.useCooldown = TimeSpan.FromMilliseconds(1000);
            attack.range = 3;
            return attack;
        }
    }
}
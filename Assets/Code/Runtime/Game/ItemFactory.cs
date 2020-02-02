using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Runtime.Game
{
    public static class ItemFactory
    {

        public enum ItemType{ Mortar, Hammer, Nailgun, Melee, Ranged}

        public static Dictionary<ItemType, System.Func<GameObject, Item>> SelectByName = new Dictionary<ItemType, Func<GameObject, Item>>(){
          { ItemType.Mortar, g => Mortar(g) },
          { ItemType.Hammer, g => Hammer(g) },
          { ItemType.Nailgun, g => Nailgun(g) },
          { ItemType.Melee, g => MeleeAttack(g) },
          { ItemType.Ranged, g => RangedAttack(g) },
        };

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

        public static RepairItem Nailgun(GameObject owner)
        {
            RepairItem nailgun = owner.AddComponent<RepairItem>();
            nailgun.name = "Nailgun";
            nailgun.baseRepairStength = 3;
            nailgun.useCooldown = TimeSpan.FromMilliseconds(500);
            nailgun.range = 6;
            return nailgun;
        }

        public static DamageItem MeleeAttack(GameObject owner)
        {
            DamageItem attack = owner.AddComponent<DamageItem>();
            attack.name = "Melee Attack";
            attack.baseDamageStrength = 25;
            attack.useCooldown = TimeSpan.FromMilliseconds(700);
            attack.range = 3;
            return attack;
        }
        public static DamageItem RangedAttack(GameObject owner)
        {
            DamageItem attack = owner.AddComponent<DamageItem>();
            attack.name = "Ranged Attack";
            attack.baseDamageStrength = 10;
            attack.useCooldown = TimeSpan.FromMilliseconds(1200);
            attack.range = 10;
            return attack;
        }

    }
}
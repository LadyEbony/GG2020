using System;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Fixer : Player
    {
        public void Start()
        {
            RepairItem hammer = gameObject.AddComponent<RepairItem>();
            hammer.name = "Hammer";
            hammer.baseRepairStength = 5;
            hammer.useCooldown = TimeSpan.FromMilliseconds(500);
            Items.Add(hammer);
            item = hammer;
            RepairItem mortar = gameObject.AddComponent<RepairItem>();
            mortar.name = "Mortar";
            mortar.baseRepairStength = 20;
            mortar.useCooldown = TimeSpan.FromMilliseconds(1000);
            Items.Add(mortar);
        }
    }
}
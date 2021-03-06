﻿using System;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Fixer : Player
    {
        public void Start()
        {
            base.Start();
            RepairItem hammer = ItemFactory.Hammer(gameObject);
            Items.Add(hammer);
            item = hammer;
            currentRange = hammer.range;
            // RepairItem mortar = ItemFactory.Mortar(gameObject);
            // Items.Add(mortar);
        }

        public void Update()
        {
            base.Update();
        }
    }
}
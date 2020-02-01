﻿using System;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Fixer : Player
    {
        public void Start()
        {
            RepairItem hammer = ItemFactory.Hammer(gameObject);
            Items.Add(hammer);
            item = hammer;
            RepairItem mortar = ItemFactory.Mortar(gameObject);
            Items.Add(mortar);
        }
    }
}
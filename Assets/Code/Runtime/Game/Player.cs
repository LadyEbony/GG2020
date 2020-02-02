using System;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

namespace Code.Runtime.Game
{
    public class Player : MonoBehaviour, IShowHealth
    {
        public List<Heldable> Items = new List<Heldable>();

        public int heldItemIndex;

        public ITargetable Target;

        public Heldable item;

        [HideInInspector]
        public BuilderDriver driver;

        public float currentRange;

        public void ShowHealth(GameObject HealthBar)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            currentRange = 0;
        }

        public void Update()
        {
            // items
        if (Input.GetMouseButtonDown(0))
        {
            if (Target != null)
            {
                if (item is ITargeting)
                {
                    (item as ITargeting).UseOn(Target);
                }
                else
                {
                    item?.Use();
                }
            }
            else
            {
                item?.Use();
            }
        }

        if (Input.mouseScrollDelta.y > 0)
            {
                if (Items.Any())
                {
                    heldItemIndex = (heldItemIndex + 1) % Items.Count;
                    item = Items[heldItemIndex];
                    if(item is IRanged)
                    {
                        currentRange = (item as IRanged).GetRange();
                    }
                }
            }
            else if (Input.mouseScrollDelta.y < 0)
            {
                if (Items.Any())
                {
                    heldItemIndex = (heldItemIndex - 1);
                    if (heldItemIndex < 0)
                    {
                        heldItemIndex = Items.Count - 1;
                    }
                    item = Items[heldItemIndex];
                    if(item is IRanged)
                    {
                        currentRange = (item as IRanged).GetRange();
                    }
                }
            }
        }
        
    }
}
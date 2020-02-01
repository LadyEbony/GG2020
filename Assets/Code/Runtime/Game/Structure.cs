using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Code.Runtime.Game.Interfaces;

public class Structure : MonoBehaviour, IRepairable, IDamageable
{
    public int startingHealth;

    public int maxHealth;

    public new string name;

    public int CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            if (value >= maxHealth)
            {
                currentHealth = maxHealth;
            }
            else if (value <= 0)
            {
                currentHealth = 0;
                Die();
            }
            else
            {
                currentHealth = value;
            }
            UpdateVisualDamage();
        }
    }

    private int currentHealth;
    // Start is called before the first frame update
    void Start()
    {
        CurrentHealth = startingHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Repair(int RepairAmount)
    {
        CurrentHealth += RepairAmount;
    }

    public void Damage(int damageAmount)
    {
        CurrentHealth -= damageAmount;
    }

    void Die()
    {
        Debug.Log($"{name} broke.");
    }

    void UpdateVisualDamage()
    {
        gameObject.GetComponent<Renderer>().material
            .SetColor("_Color", Color.Lerp(Color.red, Color.white, (float)CurrentHealth / maxHealth));
    }
    
    
}

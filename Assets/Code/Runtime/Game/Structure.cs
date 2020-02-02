using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Code.Runtime.Game.Interfaces;
using EntityNetwork; 

public class Structure : EntityBase, IAutoSerialize, IAutoDeserialize, IEarlyAutoRegister, IMasterOwnsUnclaimed, IRepairable, IDamageable, IShowHealth {
  
  [Header("Structure Stats")]
  public int startingHealth;
  public int maxHealth;

  [SerializeField]
  [NetVar('c', true, false, 100)]
  private int _currentHealth;
  public int currentHealth{
     get{
      return _currentHealth;
     } set{
      if (value >= maxHealth) {
        _currentHealth = maxHealth;
      } else if (value < 0) {
        _currentHealth = 0;
        Die();
      } else {
        _currentHealth = value;
      }
     }
  }

  public GameObject GetTarget() => gameObject;

  // Start is called before the first frame update
  void Start() {
    currentHealth = startingHealth;
  }

  void Update(){
    // Because this has to be networked, 
    // we gotta make it a big update
    UpdateVisualDamage();
  }

  public void Repair(int repairAmount) {
    RaiseEvent('r', true, repairAmount); 
  }

  [NetEvent('r')]
  private void NetRepair(int repairAmount){
    if (NetworkManager.isMaster)
      currentHealth += repairAmount;
  }

  public void Damage(int damageAmount){
    RaiseEvent('d', true, damageAmount);    
  }

  [NetEvent('d')]
  private void NetDamage(int damageAmount){
    if (NetworkManager.isMaster)
      currentHealth -= damageAmount;
  }

  void Die(){
    Debug.Log($"{name} broke.");
  }

  void UpdateVisualDamage(){
    gameObject.GetComponent<Renderer>().material
        .SetColor("_Color", Color.Lerp(Color.red, Color.white, (float)currentHealth / maxHealth));
  }

    public void ShowHealth(GameObject HealthBar)
    {
        Slider bar = HealthBar.GetComponent<Slider>();
        bar.minValue = 0;
        bar.maxValue = maxHealth;
        bar.value = currentHealth;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour {

  public static TeamManager Instance { get; private set; }

  private void Awake() {
    Instance = this;
  }

  [System.Serializable]
  public class Character{
    public string name;
    public Color outlineColor, innerColor;
    public Sprite fullSprite, headSprite;
  }

  public Character[] Characters;
  public Character GetCharacter(int index) => Characters[index];

}

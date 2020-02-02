using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.EventSystems;

public class ButtonCS : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {

  private Image outlineImage, innerImage, characterImage;

  public bool hovered;
  public int index;

  void Awake(){
    innerImage = transform.Find("Backdrop").GetComponent<Image>();
    outlineImage = transform.Find("Outline").GetComponent<Image>();
    characterImage = transform.Find("Character").GetComponent<Image>();

    innerImage.material = new Material(innerImage.material);
  }

  void Start(){
    var character = TeamManager.Instance.GetCharacter(index);
    innerImage.color = character.innerColor;
    outlineImage.color = character.outlineColor;
    characterImage.sprite = character.headSprite;
  }

  void Update(){
    if (innerImage) {
      innerImage.material.SetColor("_Color", hovered ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white);
    }
  }

  public void OnPointerEnter(PointerEventData eventData) {
    PlayerProperties.playerTeamHovered.SetLocal(index);
    hovered = true;
  }

  public void OnPointerExit(PointerEventData eventData) {
    hovered = false;
  }

  public void OnPointerClick(PointerEventData eventData) {
    PlayerProperties.playerTeam.SetLocal(index);
    PlayerProperties.lobbyStatus.SetLocal(true);
  }
}

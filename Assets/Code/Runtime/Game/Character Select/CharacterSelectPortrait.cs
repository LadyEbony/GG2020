using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using ExitGames.Client.Photon;
using ExitGames.Client.Photon.LoadBalancing;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class CharacterSelectPortrait : MonoBehaviour {

  [Header("Id")]
  public bool local;
  public int playerId;
  public bool ready;

  private Image[] outlineImages;
  private Image[] innerImages;
  private Image headImage, fullImage;
  private TextMeshProUGUI textMesh;

  private void Awake() {
    var children = ComponentExt.GetAllChildren(transform);

    var outlines = new List<Image>();
    var inners = new List<Image>();
    foreach(var t in children){
      var name = t.name;
      if (name.Contains("Outline")){
        outlines.Add(t.GetComponent<Image>());
      } else if (name.Contains("Inner")){
        inners.Add(t.GetComponent<Image>());
      } else {
        switch(name){
          case "Full":
            fullImage = t.GetComponent<Image>();
            break;
          case "Head":
            headImage = t.GetComponent<Image>();
            break;
          case "Text":
            textMesh = t.GetComponent<TextMeshProUGUI>();
            break;
        }
      }
    }

    outlineImages = outlines.ToArray();
    innerImages = inners.ToArray();
  }

  private void Update() {
    Player p;
    if (local){
      p = PlayerProperties.localPlayer;
    } else {
      p = NetworkManager.net.CurrentRoom.GetPlayer(playerId);
      if (p == null) return;
    }

    //ready = ClientEntity.lobbyStatus.Get(p);
    SetPlayer(p);
  }

  private int lastHovered = -1;

  public void SetPlayer(Player player){
    playerId = player.ID;

    var hovered = PlayerProperties.playerTeamHovered.Get(player);
    var selected = PlayerProperties.playerTeam.Get(player);

    // display fully
    if (selected >= 0) {
      var character = TeamManager.Instance.GetCharacter(selected);
      foreach (var o in outlineImages) o.color = character.outlineColor;
      foreach (var i in innerImages) i.color = character.innerColor;

      if (fullImage) {
        fullImage.sprite = character.fullSprite;
        fullImage.color = Color.white;
      }

      if (headImage) {
        headImage.sprite = character.headSprite;
        headImage.color = Color.white;
      }

      if (textMesh) {
        textMesh.text = character.name;
      }
    }
    // half transparent
    else if (hovered >= 0) {
      var character = TeamManager.Instance.GetCharacter(hovered);
      foreach (var o in outlineImages) o.color = character.outlineColor;
      foreach (var i in innerImages) i.color = character.innerColor;

      if (fullImage) {
        fullImage.sprite = character.fullSprite;
        fullImage.color = new Color(1f, 1f, 1f, 0.5f);
      }

      if (headImage) {
        headImage.sprite = character.headSprite;
        headImage.color = new Color(1f, 1f, 1f, 0.5f);
      }

      if (textMesh) {
        textMesh.text = character.name;
      }
    } 
    // nothing
    else {
      foreach (var o in outlineImages) o.color = Color.black;
      foreach (var i in innerImages) i.color = Color.white;

      if (fullImage) {
        fullImage.color = Color.clear;
      }

      if (headImage) {
        headImage.color = Color.clear;
      }

      if (textMesh) {
        textMesh.text = string.Empty;
      }
    }

    if (hovered >= 0)
      lastHovered = hovered;

  }

}

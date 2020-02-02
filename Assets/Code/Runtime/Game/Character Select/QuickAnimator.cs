using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickAnimator : MonoBehaviour {

  public Image image;
  public Sprite[] sprites;
  public float timePerImage = 0.125f;


  // Update is called once per frame
  void Update(){
    var time = Mathf.FloorToInt(Time.time / timePerImage);
    var s = sprites[time % sprites.Length];
    image.sprite = s;
  }
}

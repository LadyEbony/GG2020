using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakEffectTest : MonoBehaviour
{
  public GameObject intactObject;
  public GameObject brokenObject;
  public bool intact = true;

  private void Update()
  {
    if(Input.GetKeyDown(KeyCode.Space) && intact)
    {
      intactObject.SetActive(false);
      brokenObject.SetActive(true);
    }
  }
}

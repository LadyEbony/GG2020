using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject TeamSelectionPanel;
    List<GameObject> builders;
    GameObject destroyer;

    public void AddDestroyerPlayer()
    {
        Debug.Log("Joined kaiju team");
    }

    public void AddBuilderPlayer()
    {
        Debug.Log("Joined repair team");
    }
}

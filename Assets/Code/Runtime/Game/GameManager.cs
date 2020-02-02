using System.Collections;
using System.Collections.Generic;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject TeamSelectionPanel;
    public GameObject HealthBarPanel;
    public Camera MainCamera;
    public GameObject HealthBarPrefab;
    List<GameObject> builders;
    List<Player> builderScripts;
    GameObject destroyer;
    Monster destroyerScript; 
    public List<GameObject> structures;
    public List<Structure> structureScripts;
    Dictionary<GameObject, GameObject> HealthBars;

    public void Start()
    {
        if (HealthBars == null)
        {
            HealthBars = new Dictionary<GameObject, GameObject>();
        }
        foreach (GameObject structure in structures)
        {
            HealthBars.Add(structure, Instantiate(HealthBarPrefab,HealthBarPanel.transform));
        }
    }

    public void Update()
    {
        DisplayHealthBars();
    }

    public void SelectDestroyerPlayer()
    {
        Debug.Log("Joined kaiju team");
        TeamSelectionPanel.SetActive(false);
    }

    public void SelectBuilderPlayer()
    {
        Debug.Log("Joined repair team");
        TeamSelectionPanel.SetActive(false);
    }

    public void AddStructure(GameObject structure)
    {
        Debug.Log("Registered new Structure!");

    }

    public void RegisterHealthBar(GameObject show)
    {

    }

    public void DisplayHealthBars()
    {
        if (destroyer != null && IsScreenPointOnScreen(MainCamera.WorldToViewportPoint(destroyer.transform.position)))
        {
            destroyerScript.ShowHealth(HealthBars[destroyer]);
        }
        if (builders != null)
        {
            for(int i = 0; i < builders.Count; i++)
            {
                if (IsScreenPointOnScreen(MainCamera.WorldToViewportPoint(builders[i].transform.position)))
                {
                    builderScripts[i].ShowHealth(HealthBars[builders[i]]);
                }
            }
        }
        if (structures != null)
        {
            for(int i = 0; i < structures.Count; i++)
            {
                if (IsScreenPointOnScreen(MainCamera.WorldToViewportPoint(structures[i].transform.position)))
                {
                    structureScripts[i].ShowHealth(HealthBars[structures[i]]);
                    HealthBars[structures[i]].GetComponent<RectTransform>().anchoredPosition = MainCamera.WorldToScreenPoint(structures[i].transform.position);
                }
            }
        }
    }

    private bool IsScreenPointOnScreen(Vector3 point)
    {
        return point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f && point.z > 0f;
    }
}

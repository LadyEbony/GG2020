using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    private List<SpawnPoint> builderSpawns;
    private List<SpawnPoint> kaijuSpawns;

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

        builderSpawns = new List<SpawnPoint>();
        kaijuSpawns = new List<SpawnPoint>();
        foreach (var spawnPoint in GameObject.FindObjectsOfType<SpawnPoint>())
        {
            if (spawnPoint.CompareTag("BuilderSpawn"))
            {
                builderSpawns.Add(spawnPoint);
            }
            else if (spawnPoint.CompareTag("KaijuSpawn"))
            {
                kaijuSpawns.Add(spawnPoint);
            }
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
            destroyerScript.ShowHealth(HealthBars[destroyer], MainCamera);
        }
        if (builders != null)
        {
            for(int i = 0; i < builders.Count; i++)
            {
                if (IsScreenPointOnScreen(MainCamera.WorldToViewportPoint(builders[i].transform.position)))
                {
                    builderScripts[i].ShowHealth(HealthBars[builders[i]], MainCamera);
                }
            }
        }
        if (structures != null)
        {
            for(int i = 0; i < structures.Count; i++)
            {
                if (IsScreenPointOnScreen(MainCamera.WorldToViewportPoint(structures[i].transform.position)))
                {
                    structureScripts[i].ShowHealth(HealthBars[structures[i]], MainCamera);
                }
            }
        }
    }

    private bool IsScreenPointOnScreen(Vector3 point)
    {
        return point.x > 0f && point.x < 1f && point.y > 0f && point.y < 1f && point.z > 0f;
    }
    
    public Vector3? GetAvailableSpawnPoint(Player player)
    {
        SpawnPoint[] availableSpawns = new SpawnPoint[0];
        if (player is Fixer)
        {
            availableSpawns = builderSpawns.Where(s => !s.inUse).ToArray();
        }
        else if(player is Monster)
        {
            availableSpawns = kaijuSpawns.Where(s => !s.inUse).ToArray();
        }

        if (availableSpawns.Length > 0)
        {
            return availableSpawns[Random.Range(0, availableSpawns.Length)].Position;
        }
        return null;
    }
}

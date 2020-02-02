using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.Runtime.Game;
using Code.Runtime.Game.Interfaces;
using UnityEditor.UIElements;
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
    [HideInInspector]
    public List<Structure> structures;
    public GameState currentGameState;

    public float waitingTime;
    public float playingTime;
    public float finishTime;
    public float buildingHealthPercentageForBuilderWin;

    private float elapsedStateTime;
    private float OverallBuildingHealth;

    public enum GameState
    {
        Waiting,
        Playing,
        Finish
    }
    
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
        currentGameState = GameState.Waiting;
        elapsedStateTime = 0;
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

        structures = FindObjectsOfType<Structure>().ToList();
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
    
    public void Update()
    {
        elapsedStateTime += Time.deltaTime;
        switch (currentGameState)
        {
            case GameState.Waiting:
                if (elapsedStateTime > waitingTime)
                {
                    currentGameState = GameState.Playing;
                    elapsedStateTime = 0;
                    Debug.Log("Wait time is over");
                }
                break;
            case GameState.Playing:
                if (elapsedStateTime > playingTime)
                {
                    Debug.Log("Play time is over");
                    DetermineWinner();
                    currentGameState = GameState.Finish;
                    elapsedStateTime = 0;
                }
                break;
            case GameState.Finish:
                if (elapsedStateTime > finishTime)
                {
                    //whatever happens after displaying the score
                }
                break;
        }
    }

    void DetermineWinner()
    {
        var structuresCurrentHealth = structures.Sum(s => s.currentHealth);
        var structuresTotalHealth = structures.Sum(s => s.maxHealth);
        var overallBuildingHealth = 100 * (float)structuresCurrentHealth / structuresTotalHealth;
        if (overallBuildingHealth > buildingHealthPercentageForBuilderWin)
        {
            Debug.Log($"The builders won, structures overall health is at " +
                      $"{(int)overallBuildingHealth}");
        }
        else
        {
            Debug.Log($"The Kaiju won, structures overall health is at " +
                      $"{(int)overallBuildingHealth}");
        }
    }
    
}

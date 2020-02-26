using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Web;
using System;
using System.Dynamic;
using NCalc2.Expressions;
using StateMachine;
using UnityEditor.Rendering;
using UnityEngine.Assertions.Must;
using Object = System.Object;


[System.Serializable]
public class Flock : MonoBehaviour
{
    private List<FlockAgent> agents = new List<FlockAgent>();
    private FlockBehavior behavior; 
    
    [Header("Fish")]
   
    private AiSpawner m_AIManager;
    private SpeciesBehavior m_species;
    private FlockAgent m_FlockAgent;
    public FlockAgent m_prey;
    public int population;
    private int m_targetNumber;

    [Header("Species Information")]
    [SerializeField]
    [Range(1f, 100f)] 
    public float driveFactor = 10f;
    [SerializeField]
    [Range(1f, 100f)] 
    public float maxSpeed = 10f;
    [SerializeField]
    [Range(1f, 10f)] 
    public float neighborRadius = 3f;
    [SerializeField]
    [Range(0f, 1f)] 
    public float avoidanceRadiusMultiplier = 0.5f;
   
    public float SquareMaxSpeed { get { return squareMaxSpeed; } }
    public float SquareNeighborRadius { get { return squareNeighborRadius; } }
    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }
    
    private float squareMaxSpeed;
    private float squareNeighborRadius;
    private float squareAvoidanceRadius;
    
    void Start ()
    {  
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
      
        m_targetNumber = m_species.NumberOfAI;
        
        Debug.Log("Neighbor radius: " + squareNeighborRadius*squareNeighborRadius);
        Debug.Log("Avoidance radius: " + squareAvoidanceRadius*squareAvoidanceRadius);

        m_AIManager = transform.parent.GetComponentInParent<AiSpawner>();
    }

    void Update()
    {
        population = agents.Count;
        m_targetNumber = m_species.NumberOfAI;
        
        updatePopulation();
        
        foreach (FlockAgent agent in agents)
        {  
            List<Transform> context = GetNearbyObjects(agent);
            /*
            Renderer rend = agent.GetComponentInChildren<Renderer>();
            rend.material.color = Color.Lerp(Color.white, Color.red, context.Count/7f);
            */
            
            Vector3 move = behavior.CalculateMove(agent, context, this);
            move *= driveFactor;

            if (move.sqrMagnitude > squareMaxSpeed)
            {
                move = move.normalized * maxSpeed;
            }
            agent.Move(move);
        }
    }

    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider[] contextColliders = Physics.OverlapSphere(agent.transform.position, neighborRadius);
        foreach (Collider c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }

        return context;
    }

    public void Initialize(SpeciesBehavior species)
    {
        m_species = species;
        m_FlockAgent = species.m_prefab;
        behavior = species.m_behavior;
        m_prey = species.m_prey;
    }


    public void updatePopulation()
    {
        //if number fish in our species is less than the number the model provided
        if (population < m_targetNumber)
        {
            //find how many fish we are off by
            int popDifference = m_targetNumber - population;
                    
            //and create that many fish
            for (int y = 0; y < popDifference; y++)
            {
                FlockAgent newAgent = Instantiate(
                    m_FlockAgent,
                    m_AIManager.RandomPosition(),
                    Quaternion.Euler(UnityEngine.Random.Range(-20, 20), UnityEngine.Random.Range(0, 360), 0),
                    transform);
                newAgent.name = "Agent " + agents.Count;
                newAgent.Initialize(this);
                agents.Add(newAgent);
                
            }
        }
        //or we remove fish as long as we have too many 
        else
        {
            while (population > m_targetNumber)
            {
                var target = m_FlockAgent.GetComponentInChildren<Transform>();
                Destroy(target.GetChild(target.childCount-1).gameObject); // kill youngest child
                agents.RemoveAt(target.childCount-1);
            }
        }
    }
}



[System.Serializable]
public class SpeciesBehavior
{
    [Header("Main Settings")]
    [SerializeField]
    private bool m_enableSpawner;

    [Header("AI Group Stats")]
    [SerializeField]
    private string m_aiGroupName;
    [SerializeField]
    public FlockAgent m_prefab;
    [SerializeField]
    public FlockAgent m_prey;
    [SerializeField]
    public FlockBehavior m_behavior;
    [SerializeField]
    private int m_numberOfAI;
    
    private AiSpawner m_lake;

    private GameObject m_species;
    
    public SpeciesBehavior(string Name, FlockAgent Prefab, int NumberOfAI, AiSpawner lake)
    {
        this.m_aiGroupName = Name;
        this.m_prefab = Prefab;
        this.m_numberOfAI = NumberOfAI;

        m_lake = lake;
    }

    public string AIGroupName { get { return m_aiGroupName; } }
    public FlockAgent objectPrefab { get { return m_prefab; } }
    
    public FlockAgent Prey { get { return m_prey; } }

    public int NumberOfAI { get { return m_numberOfAI; } }
    public bool enableSpawner { get { return m_enableSpawner; } }

    public GameObject species
    {
        get { return m_species; }
        set { m_species = value; }
    }

    public void setValues(int NumberOfAI)
    {
        this.m_numberOfAI = NumberOfAI;
    }
}

public class AiSpawner : MonoBehaviour
{
    //make a model
    private Model FishModel = new Model("Fishmodel.stmx");    
    
    public List<Transform> Waypoints = new List<Transform>();

    public float spawnTimer { get { return m_SpawnTimer; } }
    public Vector3 spawnArea { get { return m_SpawnArea; } }

    [Header("Global Stats")]
    [Range(0f, 600f)]
    [SerializeField]
    private float m_SpawnTimer;
    [SerializeField]
    private Color m_SpawnColor = new Color(1.00f, 0.000f, 0.00f, 0.300f);
    [SerializeField]
    private Vector3 m_SpawnArea = new Vector3(20f, 10f, 20f);
     
    [Header("AI Group Settings")]
    public SpeciesBehavior[] AIObject = new SpeciesBehavior[5];

    private GameObject[] Species = new GameObject[5];

    
    // Awake is called at the start of the program  
    void Awake()
    {
        m_SpawnArea = new Vector3(FishModel.getVariable("PondSize"), 50f, FishModel.getVariable("PondSize"));
        GetWaypoints();
        CreateAIGroups();
        InvokeRepeating("SpawnNPC", 0.5f, spawnTimer);
    }

    

    void SpawnNPC()
    {
        //get number of fish from the model
        int numberOfFish = (int) FishModel.getVariable("Fish");

        //for every species
        for (int i = 0; i < AIObject.Count(); i++)
        {
            //if species is set to spawn and has a model
            if (AIObject[i].enableSpawner && AIObject[i].objectPrefab != null)
            {
                //update species population information
                AIObject[i].setValues(numberOfFish);
            }
        }
        //Simulate model for one time step
        FishModel.Simulate();
    }

    public Vector3 RandomPosition()
    {
        Vector3 randomPosition = new Vector3(
            UnityEngine.Random.Range(-spawnArea.x, spawnArea.x),
            UnityEngine.Random.Range(-spawnArea.y, spawnArea.y),
            UnityEngine.Random.Range(-spawnArea.z, spawnArea.z)
            );

        randomPosition = transform.TransformPoint(randomPosition * .5f);
        return randomPosition;
    }

    public Vector3 RandomWaypoint()
    {
        int randomWP = UnityEngine.Random.Range(0, (Waypoints.Count - 1));
        Vector3 randomWaypoint = Waypoints[randomWP].transform.position;
        return randomWaypoint;
    }

    void CreateAIGroups()
    {
        for (int i = 0; i < AIObject.Count(); i++)
        {
            GameObject m_AIGroupSpawn;
            m_AIGroupSpawn = new GameObject(AIObject[i].AIGroupName);
            m_AIGroupSpawn.transform.parent = this.gameObject.transform;
            Flock mController =  m_AIGroupSpawn.AddComponent<Flock>();
            
            mController.Initialize(AIObject[i]);
            
            Species[i] = m_AIGroupSpawn;
        } 
    }

    void GetWaypoints()
    {
        Transform[] wpList = this.transform.GetComponentsInChildren<Transform>();
        for (int i = 0; i < wpList.Length; i++)
        {
            if (wpList[i].tag == "Waypoint")
            {
                Waypoints.Add(wpList[i]);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = m_SpawnColor;
        Gizmos.DrawCube(transform.position, spawnArea);
    }
}

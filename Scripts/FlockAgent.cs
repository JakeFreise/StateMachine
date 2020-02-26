using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FlockAgent : MonoBehaviour
{

    private Flock agentFlock;
    private Collider agentCollider;
    private Animator m_animator;
    private float m_animationSpeed;
    private FlockAgent agentPrey;

    public Flock AgentFlock {get{return agentFlock;}}
    
    [SerializeField]
    public FlockAgent AgentPrey {get{return agentPrey;}}
    
    public Collider AgentCollider
    {
        get { return agentCollider; }
    }

    // Start is called before the first frame update
    void Start()
    {
        agentCollider = GetComponent<Collider>();
        m_animator = GetComponent<Animator>();
    }

    void Update()
    {
        m_animator.speed = m_animationSpeed*0.3f + .3f ;
    }

    public void Initialize(Flock flock)
    {
        agentFlock = flock;
        agentPrey = flock.m_prey;
    }

    public void Move(Vector3 velocity)
    {
        transform.forward = velocity;
        transform.position += velocity * Time.deltaTime;
        m_animationSpeed = velocity.magnitude;      
    }

    private void OnCollisionEnter(Collision aCollision)
    {
        if (agentPrey == aCollision.gameObject.GetComponent<FlockAgent>())
        {
            Debug.Log("Ate something");
        }

        transform.right = Vector3.zero;
        transform.up = Vector3.zero;
    }
}

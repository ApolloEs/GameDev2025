using UnityEngine;
using UnityEngine.AI;

public class ZombieChase : MonoBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private Transform player;
    public float chaseDistance = 20f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (player != null && agent != null && agent.isOnNavMesh)
        {
            float distance = Vector3.Distance(transform.position, player.position);

            if (distance <= chaseDistance)
            {
                agent.SetDestination(player.position);
            }
            else
            {
                agent.ResetPath();
            }
        }
    }
}

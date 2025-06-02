using UnityEngine;
using UnityEngine.AI;

public class ZombieStateMachine : MonoBehaviour
{
    public enum ZombieState { Idle, Wander, Chase, Attack, Dead }
    private ZombieState currentState;

    public Transform playerTransform;
    public float chaseDistance = 10f;
    public float attackDistance = 2f;
    public float wanderRadius = 5f;
    public float wanderTimer = 5f;

    private NavMeshAgent agent;
    private Animator animator;
    private float timer;

    float attackCooldown = 1.5f;
    float lastAttackTime;

    // Variables for time manipulation
    private float originalNavSpeed;
    private float originalNavAngularSpeed;
    private float originalAttackCooldown;
    private bool isSlowed = false;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.stoppingDistance = 0.2f;
        currentState = ZombieState.Wander;
        timer = wanderTimer;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogError("Cannot find player object. Make sure it has 'Player' tag.");
            }
        }
    }


    void Update()
    {
        if (currentState != ZombieState.Dead)
        {
            float distance = Vector3.Distance(transform.position, playerTransform.position);

            switch (currentState)
            {
                case ZombieState.Idle:
                    Idle();
                    if (distance <= chaseDistance)
                        SwitchState(ZombieState.Chase);
                    break;

                case ZombieState.Wander:
                    Wander();
                    if (distance <= chaseDistance)
                        SwitchState(ZombieState.Chase);
                    break;

                case ZombieState.Chase:
                    Chase();
                    if (distance <= attackDistance)
                        SwitchState(ZombieState.Attack);
                    else if (distance > chaseDistance + 3f && currentState != ZombieState.Wander)
                        SwitchState(ZombieState.Wander);

                    break;

                case ZombieState.Attack:
                    Attack();

                    if (distance > attackDistance + 0.5f)
                    {
                        agent.isStopped = false;
                        SwitchState(ZombieState.Attack);
                    }

                    break;
            }
        }
    }

    void SwitchState(ZombieState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        Debug.Log("Switching to: " + newState);


        ResetAllTriggers(); // <-- Only reset ONCE when switching state

        switch (newState)
        {
            case ZombieState.Idle:
                animator.SetTrigger("idle");
                break;
            case ZombieState.Wander:
                animator.SetTrigger("walk1");
                break;
            case ZombieState.Chase:
                animator.SetTrigger("run");
                break;
            case ZombieState.Attack:
                animator.SetTrigger("attack");
                break;
        }
    }


    void Idle()
    {
        agent.ResetPath();
    }

    void Wander()
    {
        timer += Time.deltaTime;
        if (timer >= wanderTimer)
        {
            Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
            agent.SetDestination(newPos);
            timer = 0;
        }
    }

    void Chase()
    {
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance > attackDistance)
        {
            if (!agent.pathPending)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }
        }
        else if (currentState != ZombieState.Attack)
        {
            agent.ResetPath();
            agent.isStopped = true;
            SwitchState(ZombieState.Attack);
        }


    }

    void Attack()
    {
        agent.ResetPath();
        agent.isStopped = true;

        Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
        lookDirection.y = 0;
        if (lookDirection != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 5f);

        if (Time.time > lastAttackTime + attackCooldown)
        {
            agent.isStopped = true;
            animator.ResetTrigger("attack");
            animator.SetTrigger("attack");
            lastAttackTime = Time.time;

            // Resume chasing slightly after attacking
            Invoke(nameof(ResumeMovementAfterAttack), 0.6f);
        }
    }



    public void Die(bool fallForward)
    {
        agent.enabled = false;
        currentState = ZombieState.Dead;
        ResetAllTriggers();
        animator.SetTrigger(fallForward ? "fallforward" : "fallback");
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
    }

    void ResetAllTriggers()
    {
        animator.ResetTrigger("idle");
        animator.ResetTrigger("walk");
        animator.ResetTrigger("walk1");
        animator.ResetTrigger("run");
        animator.ResetTrigger("attack");
        animator.ResetTrigger("fallforward");
        animator.ResetTrigger("fallback");
    }

    void ResumeMovementAfterAttack()
    {
        if (currentState == ZombieState.Attack)
        {
            agent.isStopped = false;
            SwitchState(ZombieState.Chase);
        }
    }


    // Time Manipulation methods
    public void ApplyTimeEffect(float timeScale)
    {
        if (!isSlowed)
        {
            // Store original values
            originalNavSpeed = agent.speed;
            originalNavAngularSpeed = agent.angularSpeed;
            originalAttackCooldown = attackCooldown;

            isSlowed = true;
        }

        // Apply slowdown to NavMeshAgent
        agent.speed = originalNavSpeed * timeScale;
        agent.angularSpeed = originalNavAngularSpeed * timeScale;

        // Slow down the animator
        if (animator != null)
        {
            animator.speed = timeScale;
        }

        // Extend cooldowns
        attackCooldown = originalAttackCooldown / timeScale;

        Debug.Log($"Zombie slowed down to {timeScale} of original speed");
    }

    public void RemoveTimeEffect()
    {
        if (isSlowed)
        {
            // Restore original values
            agent.speed = originalNavSpeed;
            agent.angularSpeed = originalNavAngularSpeed;
            attackCooldown = originalAttackCooldown;

            // Reset animator
            if (animator != null)
            {
                animator.speed = 1.0f;
            }

            isSlowed = false;
            Debug.Log("Zombie return to normal speed");
        }
    }
}

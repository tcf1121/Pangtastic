using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class JGH_TEST_NPCMoveSystem : MonoBehaviour
{
    [Header("랜덤 이동 범위")]
    public float range = 10f;
    
    private NavMeshAgent agent;
    private Animator anim;
    private bool isWaiting = false;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        // NavMeshAgent 기본값 세팅
        agent.speed = Random.Range(2f, 4f);
        agent.angularSpeed = 120f;
        agent.acceleration = 6f;
        agent.stoppingDistance = 0.4f;
        agent.baseOffset = 0.18f;
        anim.applyRootMotion = false;

        MoveToRandomPoint();
    }

    void Update()
    {
        if (anim != null)
        {
            bool isMoving = agent.remainingDistance > agent.stoppingDistance;
            anim.SetBool("IsMove", isMoving);
            Debug.Log($"[NPCMoveSystem] IsMove={isMoving}, velocity={agent.velocity}");
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting)
        {
            StartCoroutine(WaitAndMove());
        }
    }
    
    void MoveToRandomPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += transform.position;
    
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }
    
    IEnumerator WaitAndMove()
    {
        isWaiting = true;
        yield return new WaitForSeconds(Random.Range(1f, 3f));
        MoveToRandomPoint();
        isWaiting = false;
    }
}
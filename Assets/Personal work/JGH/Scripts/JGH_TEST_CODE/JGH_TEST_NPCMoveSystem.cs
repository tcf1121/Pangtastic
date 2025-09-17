using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class JGH_TEST_NPCMoveSystem : MonoBehaviour
{
    [Header("랜덤 이동 범위")]
    public float range = 10f;

    [Header("애니메이션 유지 시간")]
    public float wavingDuration = 2f;
    public float pushUpDuration = 4f;
    public float talkingDuration = 3f;
    public float lookAroundDuration = 5f;

    private NavMeshAgent agent;
    private Animator anim;
    private bool isWaiting = false;
    private bool isInAction = false; // 현재 액션 실행 중인지 체크

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
        // ====== 이동 상태 애니메이션 처리 ======
        if (anim != null && !isInAction)
        {
            bool isMoving = agent.remainingDistance > agent.stoppingDistance;
            anim.SetBool("IsMove", isMoving);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !isWaiting && !isInAction)
        {
            StartCoroutine(WaitAndMove());
        }

        // ====== 번호키 입력 감지 ======
        if (!isInAction)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Key1 pressed → IsWaving true");
                StartCoroutine(PlayBoolAndReturn("IsWaving", wavingDuration));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Key2 pressed → IsPushUp true");
                StartCoroutine(PlayBoolAndReturn("IsPushUp", pushUpDuration));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Key3 pressed → IsTalking true");
                StartCoroutine(PlayBoolAndReturn("IsTalking", talkingDuration));
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("Key4 pressed → IsLookAround true");
                StartCoroutine(PlayBoolAndReturn("IsLookAround", lookAroundDuration));
            }
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

    // ====== Bool 파라미터 실행 후 Idle 복귀 ======
    IEnumerator PlayBoolAndReturn(string boolName, float duration)
    {
        isInAction = true;
        agent.isStopped = true; // 이동 멈춤
        anim.SetBool("IsMove", false);

        // 애니메이션 시작
        anim.SetBool(boolName, true);

        // 유지 시간 대기
        yield return new WaitForSeconds(duration);

        // Idle 복귀
        anim.SetBool(boolName, false);
        anim.SetBool("IsMove", false);

        agent.isStopped = false; // 이동 다시 가능
        isInAction = false;
    }
}

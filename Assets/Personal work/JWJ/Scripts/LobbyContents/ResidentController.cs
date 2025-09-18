using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum ResidentState
{
    Idle,
    Move,
    Interact,
    Greet,
    Talk,
    Workout,
    Touched
}

public class ResidentController : MonoBehaviour
{
    [SerializeField] private ResidentSO _resident;

    private ResidentManager _manager;

    private NavMeshAgent _agent;
    private Animator _animator;
    private StateBubble _stateBubble;

    private ResidentState _curState;
    private ResidentState _lastState;
    private Coroutine _curCo;

    private Vector3 _currentDestination;
    private DestinationType _currentDestinationType = DestinationType.None;

    private bool _isTalking = false;
    private bool _isLookingAround = false;
    private bool _isWorkout = false;
    private bool _isTouched = false;

    private float _lastTouchTime = -999f;
    private float _lastTalkTime = -999f;

    private Transform _peerTransform = null;

    public ResidentSO Resident { get { return _resident; } }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _stateBubble = GetComponentInChildren<StateBubble>();
        _manager = FindObjectOfType<ResidentManager>();
        _agent.autoBraking = true;

        if (_manager != null)
        {
            _manager.RegisterResident(this);
            StartCoroutine(WaitForConfigAndStart());
        }
        else
        {
            Debug.LogError("ResidentManager 없음");
        }

    }

    private IEnumerator WaitForConfigAndStart()
    {
        while (_manager == null)
        {
            _manager = FindObjectOfType<ResidentManager>();
            yield return null;
        }
        while (_manager.Config == null)
        {
            yield return null;
        }

        _agent.speed = _manager.Config.MoveSpeed;
        _agent.stoppingDistance = _manager.Config.StoppingDistance;

        ChangeState(ResidentState.Idle);
    }

    private void ChangeState(ResidentState state)
    {
        ClearBools();

        StopRunningCoroutine();
        _lastState = _curState;
        _curState = state;

        if (state != ResidentState.Touched)
        {
            _stateBubble.SetSprite(state);
        }

        if (state == ResidentState.Idle)
        {
            _curCo = StartCoroutine(IdleRoutine());
        }
        else if (state == ResidentState.Move)
        {
            _curCo = StartCoroutine(MoveRoutine());
        }
        else if (state == ResidentState.Interact)
        {
            _curCo = StartCoroutine(InteractRoutine());
        }
        else if (state == ResidentState.Talk)
        {
            _curCo = StartCoroutine(TalkRoutine());
        }
        else if (state == ResidentState.Workout)
        {
            _curCo = StartCoroutine(WorkoutRoutine());
        }
        else if (state == ResidentState.Touched)
        {
            _curCo = StartCoroutine(TouchedRoutine());
        }
    }

    private IEnumerator IdleRoutine()
    {
        //Debug.Log($"현재상태 : {_curState}");

        SetMove(0f, true);

        _stateBubble.IconUI(true);
        yield return new WaitForSeconds(_manager.Config.IdleDuration);
        _stateBubble.IconUI(false);

        Transform nextDestination = _manager.PickNextDestination(_resident, _currentDestinationType);
        _currentDestination = nextDestination.position;

        DestinationPoint destination = nextDestination.GetComponent<DestinationPoint>();

        _currentDestinationType = destination.Type;
        ChangeState(ResidentState.Move);
    }

    private IEnumerator MoveRoutine()
    {
        SetMove(_manager.Config.MoveSpeed, false);
        _agent.SetDestination(_currentDestination);
        //Debug.Log($"{_resident.ResidentName} 이 {_currentDestinationType}로 이동");
     
        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer > _manager.Config.MoveDuration)
            {
                ChangeState(ResidentState.Idle);
                yield break;
            }
            if(_agent.pathPending == false)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (_currentDestinationType == DestinationType.Park)
                    {
                        ChangeState(ResidentState.Workout);
                        yield break;
                    }
                    else
                    {
                        Vector3 dir = _currentDestination - transform.position;
                        dir.y = 0f;

                        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        transform.rotation = look;

                        ChangeState(ResidentState.Interact);
                        yield break;
                    }
                    
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator InteractRoutine()
    {
        _isLookingAround = true;

        SetMove(0f, true);
        _stateBubble.IconUI(true);
        yield return new WaitForSeconds(_manager.Config.InteractDuration);
        _stateBubble.IconUI(false);

        ChangeState(ResidentState.Idle);
        
    }

    private IEnumerator TalkRoutine()
    {
        SetMove(0f, true);
        _isTalking = true;
        //Debug.Log($" {_resident.ResidentName} 대화 시작");

        LookAtEachOther();
        _stateBubble.IconUI(true);
        yield return new WaitForSeconds(_manager.Config.TalkDuration);
        _stateBubble.IconUI(false);

        //Debug.Log($"{_resident.ResidentName} 대화 종료"); 
        _peerTransform = null;

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator WorkoutRoutine()
    {
        SetMove(0f, true);
        //Debug.Log("운동중");
        _isWorkout = true;
        _stateBubble.IconUI(true);
        yield return new WaitForSeconds(_manager.Config.WorkoutDuration);
        _stateBubble.IconUI(false);

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator TouchedRoutine()
    {
        SetMove(0f, true);
        _isTouched = true;

        _stateBubble.ChatUI(_resident ,true);
        yield return new WaitForSeconds(_manager.Config.TouchDuration);
        _stateBubble.ChatUI(_resident, false);

        if (_lastState == ResidentState.Move)
        {
            ChangeState(ResidentState.Move);
        }
        else
        {
            ChangeState(ResidentState.Idle);
        }
    }

    private void StopRunningCoroutine()
    {
        if (_curCo != null)
        {
            StopCoroutine(_curCo);
            _curCo = null;
        }
    }

    private void SetMove(float speed, bool stop)
    {
        _agent.speed = speed;
        if (stop)
        {
            _agent.isStopped = true;
            _agent.ResetPath();
        }
        else
        {
            _agent.isStopped = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log($"{gameObject.name} 콜라이더 충돌 : {other.name}");
        ResidentController otherResident;
        otherResident = other.GetComponentInParent<ResidentController>();

        if (otherResident != null)
        {
            //Debug.Log($"{_resident.ResidentName} 가 {otherResident._resident.ResidentName}를 감지");
            int myId = GetInstanceID();
            int otherId = otherResident.GetInstanceID();

            if (myId < otherId)
            {
                bool willTalk = Random.value <= _manager.Config.TalkChance;
                if (willTalk)
                {
                    TryToTalk(otherResident);
                }
            }
        }
    }

    private bool CanTalkNow()
    {
        if (_curState != ResidentState.Move && _curState != ResidentState.Idle)
        {
            return false;
        }
        if (_isTalking)
        {
            return false;
        }
        if (Time.time - _lastTalkTime < _manager.Config.TalkCooldown)
        {
            return false;
        }
        return true;
    }

    private void TryToTalk(ResidentController other)
    {
        if (other == null)
        {
            return;
        }
        if (!CanTalkNow())
        {
            return;
        }
        if (!other.CanTalkNow())
        {
            return;
        }

        _lastTalkTime = Time.time;
        other._lastTalkTime = Time.time;

        _peerTransform = other.transform;
        other._peerTransform = this.transform;

        
        StartTalkBoth(other);
    }

    private void StartTalkBoth(ResidentController other)
    {
        if (other == null)
        {
            return;
        }
        ChangeState(ResidentState.Talk);
        other.ChangeState(ResidentState.Talk);
    }

    private void LookAtEachOther()
    {
        if (_peerTransform == null)
        {
            return;
        }
        Vector3 dir = _peerTransform.position - transform.position;
        dir.y = 0f;
        
        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = look;
        //Debug.Log($"{_resident.ResidentName}가 {_peerTransform.name}를 바라봄");
    }

    public void OnTouched()
    {
        Debug.Log($"{_resident.ResidentName} 를 터치");
        if (Time.time - _lastTouchTime < _manager.Config.TouchCooldown)
        {
            Debug.Log("터치가 너무 빠름");
            return;
        }
        _lastTouchTime = Time.time;

        if (_isTalking == true)
        {
            Debug.Log("대화 중이라 바쁨");
            return;
        }

        if (_curState != ResidentState.Talk)
        {
            ChangeState(ResidentState.Touched);

            Vector3 dir = Camera.main.transform.position - transform.position;
            dir.y = 0f;

            Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = look;
            return;
        }
    }

    private void Update()
    {
        bool isMoving = false;
        if (_agent != null)
        {
            if (_agent.isStopped == false)
            {
                if (_agent.velocity.sqrMagnitude > 0.01f)
                {
                    isMoving = true;
                }
            }
        }

        _animator.SetBool("IsMove", isMoving);

        bool isWaving = (_isTouched == true);
        _animator.SetBool("IsWaving", isWaving);

        _animator.SetBool("IsJumpingJack", _isWorkout);
        _animator.SetBool("IsTalking", _isTalking);
        _animator.SetBool("IsLookAround", _isLookingAround);
    }

    private void ClearBools()
    {
        _isTalking = false;
        _isLookingAround = false;
        _isWorkout = false;
        _isTouched = false;
    }
}

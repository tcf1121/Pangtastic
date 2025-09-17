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
    private ResidentConfigSO _config;
    private ResidentManager _manager;

    private NavMeshAgent _agent;
    private Animator _animator;
    private StateBubble _stateBubble;

    private ResidentState _curState;
    private ResidentState _lastState;
    private Coroutine _curCo;

    private Vector3 _currentDestination;
    private DestinationType _currentDestinationType;
    private Transform _favoriteDestination;

    private bool _isGreeting = false;
    private bool _isTalking = false;
    private float _lastTouchTime = -999f;
    private float _lastGreetTime = -999f;
    private int _activePeerId = 0;
    private Transform _peerTransform = null;

    public ResidentSO Resident { get { return _resident; } }

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _stateBubble = GetComponentInChildren<StateBubble>();
        _manager = FindObjectOfType<ResidentManager>();
        _agent.autoBraking = true;
        _stateBubble.gameObject.SetActive(false);

        _manager.RegisterResident(this);

        AsyncOperationHandle<ResidentConfigSO> handle = Addressables.LoadAssetAsync<ResidentConfigSO>("ResidentConfigSO");
        handle.Completed += ResidentConfigLoaded;
    }

    private void ResidentConfigLoaded(AsyncOperationHandle<ResidentConfigSO> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _config = handle.Result;
            Debug.Log($"주민 세팅 로드완료.");
            _agent.speed = _config.MoveSpeed;
            _agent.stoppingDistance = _config.StoppingDistance;

            ChangeState(ResidentState.Idle);
        }
        else
        {
            Debug.LogError($"주민 세팅 로드 실패:{handle.OperationException}");
        }
    }

    private void ChangeState(ResidentState state)
    {
        StopRunningCoroutine();
        _lastState = _curState;
        _curState = state;

        _stateBubble.SetSprite(state);

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
        else if (state == ResidentState.Greet)
        {
            _curCo = StartCoroutine(GreetRoutine());
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
        yield return new WaitForSeconds(_config.IdleDuration);
        Transform nextDestination = _manager.PickNextDestination(_resident, _config);
        _currentDestination = nextDestination.position;

        DestinationPoint destination = nextDestination.GetComponent<DestinationPoint>();
        _currentDestinationType = destination.Type;
        ChangeState(ResidentState.Move);
    }

    private IEnumerator MoveRoutine()
    {
        //Debug.Log($"현재상태 : {_curState}");

        SetMove(_config.MoveSpeed, false);
        _agent.SetDestination(_currentDestination);
        Debug.Log($"{_resident.ResidentName} 이 {_currentDestination}로 이동");

        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer > _config.MoveDuration)
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
                        ChangeState(ResidentState.Interact);
                        Vector3 dir = _currentDestination - transform.position;
                        dir.y = 0f;

                        Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        transform.rotation = look;
                        yield break;
                    }
                    
                }
            }
            
            yield return null;
        }
    }

    private IEnumerator InteractRoutine()
    {
        //Debug.Log($"현재상태 : {_curState}");

        SetMove(0f, true);
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_config.InteractDuration);
        _stateBubble.gameObject.SetActive(false);

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator GreetRoutine()
    {
        SetMove(_config.MoveSpeed, false);
        _isGreeting = true;
        Debug.Log($"{_resident.ResidentName} 인사 시작");

        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_config.GreetDuration);
        _stateBubble.gameObject.SetActive(false);


        Debug.Log($"{_resident.ResidentName} 인사 종료");
        _isGreeting = false;

        if (_lastState == ResidentState.Move)
        {
            ChangeState(ResidentState.Move);
        }
        else
        {
            ChangeState(ResidentState.Idle);
        }
    }

    private IEnumerator TalkRoutine()
    {
        SetMove(0f, true);
        _isTalking = true;
        Debug.Log($" {_resident.ResidentName} 대화 시작");

        LookAtEachOther();
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_config.TalkDuration);
        _stateBubble.gameObject.SetActive(true);

        Debug.Log($"{_resident.ResidentName} 대화 종료"); 
        _isTalking = false;
        _peerTransform = null;
        _activePeerId = 0;

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator WorkoutRoutine()
    {
        SetMove(0f, true);
        Debug.Log("운동중");
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_config.WorkoutDuration);
        _stateBubble.gameObject.SetActive(false);

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator TouchedRoutine()
    {
        SetMove(0f, true);
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_config.TouchDuration);
        _stateBubble.gameObject.SetActive(false);

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
        Debug.Log($"{gameObject.name} 콜라이더 충돌 : {other.name}");
        ResidentController otherResident;
        otherResident = other.GetComponentInParent<ResidentController>();
        //other.TryGetComponent<ResidentController>(out otherResident);

        if (otherResident != null)
        {
            Debug.Log($"{_resident.ResidentName} 가 {otherResident._resident.ResidentName}를 감지");
            int myId = GetInstanceID();
            int otherId = otherResident.GetInstanceID();

            if (myId < otherId)
            {
                TryToSayHi(otherResident);
            }
        }
    }

    private bool CanGreetNow()
    {
        if (_curState != ResidentState.Move)
        {
            return false;
        }
        if (_isGreeting || _isTalking)
        {
            return false;
        }
        if (Time.time - _lastGreetTime < _config.GreetCooldown)
        {
            return false;
        }
        return true;
    }

    private void TryToSayHi(ResidentController other)
    {
        if (other == null)
        {
            return;
        }
        if (!CanGreetNow())
        {
            return;
        }
        if (!other.CanGreetNow())
        {
            return;
        }

        _activePeerId = other.GetInstanceID();
        other._activePeerId = GetInstanceID();
        _lastGreetTime = Time.time;
        other._lastGreetTime = Time.time;

        _peerTransform = other.transform;
        other._peerTransform = this.transform;

        Debug.Log($"{_resident.ResidentName} 와 {other._resident.ResidentName} 인사 합의됨");

        StartGreet();
        other.StartGreet();

        bool willTalk = Random.value <= _config.TalkChance;
        if (willTalk)
        {
            Debug.Log($"{_resident.ResidentName} 와 {other._resident.ResidentName} 대화 합의됨");
            StartTalkBoth(other);
        }
        else
        {
            Debug.Log($"{_resident.ResidentName} 와 {other._resident.ResidentName} 대화 합의 안됨");
        }
    }

    private void StartGreet()
    {
        ChangeState(ResidentState.Greet);
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
        Debug.Log($"{_resident.ResidentName}가 {_peerTransform.name}를 바라봄");
    }

    public void OnTouched()
    {
        if (Time.time - _lastTouchTime < _config.TouchCooldown)
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

        if (_isGreeting == true)
        {
            Debug.Log("인사 중이라 바쁨");
            return;
        }

        if (_curState == ResidentState.Move || _curState == ResidentState.Idle)
        {
            ChangeState(ResidentState.Touched);

            Vector3 dir = Camera.main.transform.position - transform.position;
            dir.y = 0f;

            Quaternion look = Quaternion.LookRotation(dir.normalized, Vector3.up);
            transform.rotation = look;
            return;
        }
    }
}

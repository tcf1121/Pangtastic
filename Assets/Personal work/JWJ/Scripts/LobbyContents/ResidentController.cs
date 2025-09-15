using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Purchasing;
using UnityEngine.UI;

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

    [SerializeField] private Transform[] _allDestinations;
    [SerializeField] private int _favoriteDestinationIndex;
    [Range(0f, 1f)][SerializeField] private float _preferredWeight = 0.4f;

    private float _touchCooldown = 0.5f;
    private float _lastTouchTime = -999f;

    private float _moveSpeed = 1f;
    private float _idleDuration = 2f;
    private float _moveDuration = 20f;
    private float _interactDuration = 10f;
    private float _greetDuration = 1.2f;
    private float _talkDuration = 12f;

    private Transform _favoriteDestination;

    private NavMeshAgent _agent;
    private Animator _animator;
    private ResidentState _curState;
    private ResidentState _lastState;
    private Coroutine _curCo;
    private Vector3 _currentDestination;
    public ResidentSO Resident { get { return _resident; } }

    private float _greetCooldown = 3f;
    private float _talkChance = 0.20f;

    private bool _isGreeting = false;
    private bool _isTalking = false;
    private float _lastGreetTime = -999f;
    private int _activePeerId = 0;
    private Transform _peerTransform = null;
    private string _destinationName;

    private StateBubble _stateBubble;


    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponentInChildren<Animator>();
        _stateBubble = GetComponentInChildren<StateBubble>();

        _agent.speed = _moveSpeed;
        _agent.stoppingDistance = 0.6f;
        _agent.autoBraking = true;
        _stateBubble.gameObject.SetActive(false);

        _favoriteDestination = _allDestinations[_favoriteDestinationIndex];
    }

    private void Start()
    {
        ChangeState(ResidentState.Idle);
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
        yield return new WaitForSeconds(_idleDuration);
        PickNextDestination();
        ChangeState(ResidentState.Move);
    }

    private IEnumerator MoveRoutine()
    {
        //Debug.Log($"현재상태 : {_curState}");

        SetMove(_moveSpeed, false);
        _agent.SetDestination(_currentDestination);

        float timer = 0;
        while (true)
        {
            timer += Time.deltaTime;
            if (timer > _moveDuration)
            {
                ChangeState(ResidentState.Idle);
                yield break;
            }
            if(_agent.pathPending == false)
            {
                if (_agent.remainingDistance <= _agent.stoppingDistance)
                {
                    if (_destinationName.ToLower() == "park")
                    {
                        ChangeState(ResidentState.Workout);
                        yield break;
                    }
                    else
                    {
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
        //Debug.Log($"현재상태 : {_curState}");

        SetMove(0f, true);
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_interactDuration);
        _stateBubble.gameObject.SetActive(false);

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator GreetRoutine()
    {
        SetMove(_moveSpeed, false);
        _isGreeting = true;
        Debug.Log($"{_resident.ResidentName} 인사 시작");

        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_greetDuration);
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
        yield return new WaitForSeconds(_talkDuration);
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
        yield return new WaitForSeconds(_interactDuration);
        _stateBubble.gameObject.SetActive(false);

        ChangeState(ResidentState.Idle);
    }

    private IEnumerator TouchedRoutine()
    {
        SetMove(0f, true);
        _stateBubble.gameObject.SetActive(true);
        yield return new WaitForSeconds(_greetDuration);
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

    private void PickNextDestination()
    {
        bool pickPreferred = Random.value <= _preferredWeight;

        if (pickPreferred)
        {
            _currentDestination = _favoriteDestination.position;
            Debug.Log($"{_resident.ResidentName} 목표 지점: {_allDestinations[_favoriteDestinationIndex].name}");
            _destinationName = _allDestinations[_favoriteDestinationIndex].name;
        }
        else
        {
            int idx = Random.Range(0, _allDestinations.Length);
            _currentDestination = _allDestinations[idx].position;
            Debug.Log($"{_resident.ResidentName} 목표 지점: {_allDestinations[idx].name}");
            _destinationName = _allDestinations[idx].name;
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
        ResidentController otherResident;
        other.TryGetComponent<ResidentController>(out otherResident);

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
        if (Time.time - _lastGreetTime < _greetCooldown)
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

        bool willTalk = Random.value <= _talkChance;
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
        if (Time.time - _lastTouchTime < _touchCooldown)
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

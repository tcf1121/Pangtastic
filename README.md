🛠 Pangtastic 팀 프로젝트 컨벤션
Unity Version: 2022.3.6f1

📌 커밋 템플릿 (Commit Template)
타입 (type)	설명
feat	새로운 기능 추가, 기존 기능을 요구사항에 맞게 수정
fix	기능에 대한 버그 수정
set	단순 파일 추가
build	빌드 관련 수정
chore	기타 수정 사항
ci	CI 관련 설정 수정
docs	문서, 주석 관련 수정
style	코드 스타일, 포맷팅 수정
refactor	기능 변화 없이 코드 리팩터링
test	테스트 코드 추가/수정
release	버전 릴리즈
🌿 브랜치 규칙
main 브랜치:
Dev 브랜치에서 하루 작업량을 충돌 없는 상태로 merge하여 관리

dev 브랜치:
오전/오후 작업한 내용을 머지하여 개발 통합 브랜치로 사용

기능별 브랜치:
기능명/이니셜 형식
예시: inventory/SCR

🧱 코드 컨벤션
✅ 네이밍 규칙
요소	규칙	예시
클래스 / 인터페이스	PascalCase	PlayerController, IGameService
메서드	PascalCase	StartGame(), GetData()
변수 / 필드 (private)	camelCase + _ 접두어	_playerName, _currentHealth
상수 / readonly	대문자 + _	MAX_HEALTH, DEFAULT_SPEED
이벤트	PascalCase + 동사	OnDamageTaken, PlayerDied
로컬 변수	camelCase	index, tempScore
enum 타입	PascalCase	PlayerState, Idle, Running 등
제네릭 타입	T 접두어 사용	TEntity, TResult
🔗 참고: C# 식별자 네이밍 공식 문서

✅ 코드 스타일
항목	스타일	예시
중괄호 {}	항상 새 줄	if (x) \n { \n ... \n }
들여쓰기	공백 4칸	VS 기본 설정 사용
공백 규칙	연산자 양옆 공백	x = y + z;
줄바꿈	논리 단위로 함수 간 한 줄 띄움	함수 간 \n\n 구분
파일 당 클래스	1개 클래스 권장	ClassA.cs, ClassB.cs
this. 사용	선택적 (모호할 때만)	this.name = name;
접근 제한자 순서	public → protected → private	
네임스페이스	PascalCase	namespace MyApp.Controllers
🔗 참고: C# 코드 스타일 공식 문서

📦 네임스페이스 규칙
자신의 이니셜을 네임스페이스로 설정
예: Player.cs → namespace SCR { }
🔁 코루틴 규칙
Coroutine 변수:
변수명은 ~Co 형식
예: Coroutine attackCo;

Coroutine 메서드:
함수 이름은 ~Routine() 형식
예: IEnumerator AttackRoutine() { }

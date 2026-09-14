# Pre-Commit Checklist

커밋 전 변경된 Unity C# 파일에서 성능, 유지보수성, 생산성에 영향을 주는 항목을 확인한다.

## 확인 항목

- 사용하지 않는 변수, 필드, 매개변수, 지역 변수 선언이 없는가.
- 사용하지 않는 `using` 선언이 없는가.
- 타입, 멤버, 로컬 변수의 이름이 프로젝트 네이밍 규칙을 따르는가.
- 전역 검색 또는 동적 resource loading API를 사용하지 않았는가.
  - `GameObject.Find`, `FindFirstObjectByType`, `FindAnyObjectByType`, `FindObjectsByType`, `Resources.Load` 사용 여부를 확인한다.
  - 불가피한 초기화 시점 사용은 이유와 호출 범위를 명확히 하고, 결과를 캐시하거나 명시적으로 전달한다.
- 의미가 있는 값이 이름 없는 리터럴로 하드코딩되지 않았는가.
  - 설정값은 `[SerializeField]`로 노출하거나 `const` 또는 `static readonly` 이름으로 의도를 드러낸다.
  - `0`, `1`처럼 의미가 자명한 값은 제외한다.
- `Update()`, `FixedUpdate()`, `LateUpdate()`에서 불필요한 조회나 재계산을 매 프레임 반복하지 않는가.
  - `GetComponent`, `TryGetComponent`, `GetComponents`, 동일 결과의 재계산 여부를 확인한다.
  - 필요한 참조와 결과는 초기화 시점에 캐시하고, 값이 변경될 때만 갱신할 수 있으면 이벤트 기반 처리를 검토한다.
- `Update()` 계열에서 GC allocation을 만들지 않는가.
  - LINQ, 문자열 보간, `new` 컬렉션 또는 배열, 람다 캡처로 인한 할당 여부를 확인한다.
- 반복 생성과 삭제가 필요한 객체에 `Instantiate()`와 `Destroy()`를 직접 반복 사용하지 않는가.
  - 투사체, 적, 이펙트처럼 자주 재사용되는 객체는 pool 사용을 검토한다.
- `Debug.Log`, `Debug.LogWarning`, `Debug.LogError`가 의도치 않게 남아 있지 않은가.
  - 매 프레임 또는 반복 경로의 로그는 제거한다.
- event 구독과 해제가 짝을 이루는가.
  - `OnEnable`에서 구독했다면 `OnDisable`에서 해제한다.
  - 파괴된 객체를 참조하는 callback이 남지 않는지 확인한다.
- 물리 처리와 프레임 처리의 실행 위치가 맞는가.
  - `Rigidbody` 물리 변경은 기본적으로 `FixedUpdate`에서 처리한다.
  - 프레임 독립적인 이동과 타이머에는 `Time.deltaTime`을 사용한다.
- 예외를 일반 제어 흐름으로 사용하지 않는가.
  - 반복 실행 경로에서 발생하는 예외는 성능 저하와 오류 추적 비용을 키운다.
- 필수 `[SerializeField]` 참조가 누락되어 런타임 null 오류를 만들 가능성이 없는가.
  - Inspector 연결 상태를 확인하고, 필요하면 초기화 시점에 검증한다.
- Unity `Object`의 fake null 동작을 우회하지 않는가.
  - 파괴될 수 있는 `UnityEngine.Object`는 `target == null` 또는 `if (!target)`으로 확인한다.
  - null 판정에 `ReferenceEquals`, `is null`, `?.`, `??`를 사용하지 않는다. 이 표현들은 파괴된 Unity 객체를 일반 C# null로 취급하지 않을 수 있다.

## 적용 기준

- 새 경고가 생기면 원인을 제거하거나, 의도된 예외라면 코드에서 이유와 호출 범위를 알 수 있게 한다.
- 기존 코드의 문제를 발견했더라도 현재 변경 범위와 무관하면 별도 작업으로 분리한다.

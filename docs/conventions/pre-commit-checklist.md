# Pre-Commit Checklist

커밋 전 스테이징된 Unity C# 변경과 직접 연결된 Prefab, Scene, ScriptableObject asset에서 맥락 판단이 필요한 위험을 확인한다.

## 확인 항목

- 변경된 조건, early return, 상태 전환, 호출 순서가 기존 동작을 깨지 않는가.
- public 메서드, property, event, `[SerializeField]` field의 이름 또는 타입 변경이 호출자와 직렬화된 데이터의 계약을 깨지 않는가.
- 새로 추가하거나 이름 또는 타입을 변경한 `[SerializeField]` 참조와 component requirement가 관련 Prefab, Scene, ScriptableObject asset의 필수 연결을 잃게 하지 않는가.
- null, 빈 컬렉션, 잘못된 ID 또는 index, pool에서 재사용되는 상태, event 구독 해제처럼 경계 조건과 정리 시점이 안전한가.
- 전역 검색 또는 동적 resource loading API를 사용했다면, 초기화 시점 사용 사유와 호출 범위가 명확한가. 결과를 캐시하거나 명시적으로 전달하는가.
- 의미가 있는 값이 이름 없는 리터럴로 하드코딩되지 않았는가.
  - 설정값은 `[SerializeField]`로 노출하거나 `const` 또는 `static readonly` 이름으로 의도를 드러낸다.
  - `0`, `1`처럼 의미가 자명한 값은 제외한다.
- `Update()`, `FixedUpdate()`, `LateUpdate()`에서 조회 또는 재계산을 한다면, 매 프레임 실행이 필요한가. 필요한 참조와 결과를 초기화 시점에 캐시하거나 값 변경 시에만 갱신할 수 있는가.
- `Update()` 계열의 allocation이 실제 hot path에서 성능 저하를 만들지 않는가.
- 반복 생성과 삭제가 필요한 객체의 수명과 재사용 전략이 실제 사용 빈도에 맞는가.
  - 투사체, 적, 이펙트처럼 자주 재사용되는 객체는 pool 사용을 검토한다.
- 디버그 로그가 남아 있다면, 반복 경로가 아니며 의도와 제거 시점이 명확한가.
- event 구독과 해제가 짝을 이루는가.
  - `OnEnable`에서 구독했다면 `OnDisable`에서 해제한다.
  - 파괴된 객체를 참조하는 callback이 남지 않는지 확인한다.
- 물리 처리와 프레임 처리의 실행 위치가 맞는가.
  - `Rigidbody` 물리 변경은 기본적으로 `FixedUpdate`에서 처리한다.
  - 프레임 독립적인 이동과 타이머에는 `Time.deltaTime`을 사용한다.
- 예외를 일반 제어 흐름으로 사용하지 않는가.
  - 반복 실행 경로에서 발생하는 예외는 성능 저하와 오류 추적 비용을 키운다.
- 파괴될 수 있는 `UnityEngine.Object`를 다룬다면 Unity fake null 동작을 고려했는가.
- 외부 입력, 파일, 네트워크, 인증 또는 비밀값을 변경했다면 검증과 노출 위험을 확인했는가.

## 적용 기준

- 새 경고가 생기면 원인을 제거하거나, 의도된 예외라면 코드에서 이유와 호출 범위를 알 수 있게 한다.
- 기존 코드의 문제를 발견했더라도 현재 변경 범위와 무관하면 별도 작업으로 분리한다.

# Unity C# Code Convention

프로젝트의 코드 스타일을 통일하기 위한 기준이다. 실행 흐름과 공개 API가 먼저 읽히도록 배치한다.

## 네이밍

- 클래스, 파일명: PascalCase
- public 메서드, 프로퍼티: PascalCase
- private 필드: _camelCase
- 로컬 변수, 파라미터: camelCase
- enum, enum 값: PascalCase
- 인터페이스: I 접두어

## 기본 원칙

- 접근 제한자보다 Unity lifecycle과 호출 흐름을 우선한다.
- 외부에서 사용하는 event, property, public method는 private 구현보다 위에 둔다.
- 필드는 선언적 특성(상수, 정적, 읽기 전용, 직렬화 등)에 따라 그룹화한다.
- 같은 구역은 중요도, 실행 순서, 호출 순서로 정렬한다. 알파벳 순서는 큰 열거형이나 매핑처럼 탐색성이 더 중요할 때만 사용한다.

## 클래스 멤버 선언 순서

아래에서 언급하는 순서를 따른다. 같은 구역에서는 public, protected, internal, private 순서로 둔다.
1. constants
2. static readonly/static fields
3. serialized fields
4. public fields (불가피한 경우만)
5. events
6. public properties
7. protected/internal/private properties
8. readonly instance fields
9. mutable instance fields
10. Unity events
11. initialization, lifecycle hook methods
12. interface implementations
13. public methods
14. protected/internal methods
15. private methods
16. editor-only callbacks/helpers
17. nested types

## 필드와 프로퍼티

- const : 외부 API가 아니면 private를 기본으로 한다.
- static : mutable static field는 가급적 사용하지 않는다.
- [SerializeField] : Inspector 표시 순서는 프리팹 / 컴포넌트 참조, 데이터 에셋, 주요 수치, 옵션 플래그, 개발용 설정 순서를 권장한다. private field로 선언하고 읽기 전용 public property를 같이 선언한다.
- public field : Data Transfer Object, Serialization 컨테이너, 기존 코드 호환처럼 명확한 사유가 있을 때만 사용한다.
- property : 외부에서 읽는 API를 먼저 보여준다.
- mutable field : 관련 항목끼리 묶는다.
- editor-only : `#if UNITY_EDITOR`로 감싼다.

## 메서드

### Unity Event Functions

- Unity event 함수는 lifecycle 순서로 배치한다. editor 설정 보정 성격이 강하면 editor-only 영역으로 내려도 된다.

### Private Helpers

- private method는 호출 순서에 따라 배치한다.
- 한 메서드에서만 쓰는 짧은 helper는 메서드 바로 아래에 둘 수 있다.
- 여러 메서드에서 공유하는 helper는 관련 구역 아래에 모은다.
- 순수 계산 함수와 static utility helper는 private 영역의 최하단에 둔다.

### Override Methods

- `override` 메서드는 역할에 따라 해당 구역의 상단에 배치한다.

## Nested Types and Modifiers

- nested type은 기본적으로 클래스 하단에 `enum`, `struct`, `class` 순서로 둔다.
- 클래스 전반에서 핵심인 상태 enum은 필드 위에 둘 수 있다.
- 공유되는 enum과 struct는 별도 파일로 분리를 권장한다.

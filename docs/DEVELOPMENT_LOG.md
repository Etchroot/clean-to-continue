# 개발 기록

## 2026-08-15 — 기획 확정과 기반 구축

**목표**

- Clean to Continue의 기획을 개발 가능한 문서로 정리한다.
- Codex 작업, 사용자 결정과 해커톤 제출 근거를 지속적으로 기록할 구조를 만든다.
- Unity 6.3 LTS, GitHub와 Unity 공식 MCP 연결을 준비한다.

**결정**

- 3개 장비와 4단계 청소 구조를 사용한다.
- Codex가 전체 게임 개발의 주 도구다.
- Unity 인에디터 유료 Assistant보다 무료 공식 MCP 연결을 우선한다.
- GitHub 저장소는 Public, 자체 코드는 MIT License로 공개한다.

**변경 파일**

- 루트 안내, 라이선스와 Git 보호 파일
- `docs/`의 기획·로드맵·연동·교육·HIL 문서
- `submission/`의 신청·영상·에셋·검수 문서

**검증 예정**

- 문서 간 제목, 스테이지, 단계와 추억 장면 일치 여부
- Git 추적 대상에 비밀값과 제3자 원본이 없는지 확인
- GitHub `origin`과 Public 상태 확인
- Unity 설치 후 MCP에서 콘솔과 Scene 읽기 확인

**다음 작업**

- Unity 6.3 LTS 프로젝트 생성과 공식 MCP 연결
- 임시 모델을 사용한 마우스 세로 슬라이스 구현

## 2026-08-15 — Unity 개발 환경과 Universal 3D 프로젝트 구축

**사용자 작업**

- Unity Hub 업데이트 충돌을 확인하고 구버전 Hub 3.4.1의 공식 제거를 승인했다.
- Unity 6.3 LTS, Web Build Support와 함께 설치된 Visual Studio Community 2026을 확인했다.

**Codex 작업**

- Unity Hub 3.20.1과 Unity 6000.3.22f1 설치를 파일과 버전 정보로 검증했다.
- WebGL 지원 모듈과 Unity Personal 라이선스 활성화를 검증했다.
- Unity 공식 `3D URP` 템플릿으로 `Game/` 프로젝트를 만들고 WebGL 대상으로 초기화했다.
- 템플릿의 Input System 1.12.0이 Unity 6.3에서 컴파일되지 않는 문제를 재현하고 공식 지원 버전 1.17.0으로 변경했다.

**검증 결과**

- Unity 6000.3.22f1, URP 17.3.0, Input System 1.17.0이 해석됐다.
- 두 번째 배치 초기화에서 C# 컴파일 오류 0건, 종료 코드 0을 확인했다.
- `Assets/Settings/`에 PC·모바일 URP 설정과 기본 볼륨 프로필이 생성됐다.

**다음 작업**

- Unity 공식 MCP 패키지 설치와 Codex 연결 승인
- Unity 콘솔, 열린 Scene과 GameObject 읽기 테스트

## 2026-08-15 — Unity 공식 MCP Bridge 설치

**Codex 작업**

- `com.unity.ai.assistant` 2.17.0-pre.1을 프로젝트 패키지에 추가했다.
- 패키지 설치 후 C# 컴파일 오류 0건과 배치 종료 코드 0을 확인했다.
- Windows Relay 1.3.14가 `C:\Users\차명근\.unity\relay\relay_win.exe`에 생성된 것을 확인했다.
- Codex 설정에 `unity_mcp` 서버와 `Game` 프로젝트 경로를 추가했다.

**사용자 검증**

- Unity의 `Project Settings > AI > Unity MCP Server`에서 Unity Bridge가 초록색 `Running` 상태임을 직접 확인했다.

**연결 검증**

- 사용자가 공유한 Unity 설정 화면에서 Bridge `Running`과 Codex 클라이언트 `Accepted` 상태를 확인했다.
- 이미 승인된 연결이 `Connected Clients`에 표시되어 별도의 `Pending Connections` 단계가 필요하지 않았다.
- Codex가 Unity 콘솔을 직접 읽어 오류 0건과 경고 2건을 확인했다.
- Codex가 읽기 전용 명령으로 저장 전 기본 씬의 루트 GameObject 2개를 확인했다.
  - `Main Camera`: `Transform`, `Camera`, `AudioListener`
  - `Directional Light`: `Transform`, `Light`, `UniversalAdditionalLightData`
- 명령의 C# 컴파일과 실행이 모두 성공해 Codex–Unity MCP 왕복 연결을 검증했다.

**참고 경고**

- Unity AI 계정 API 접근 지연 경고가 1건 있었으나 유료 Assistant 계정 기능에 관한 것으로 MCP 읽기에는 영향을 주지 않았다.
- Microsoft Store판 Codex 실행 파일의 Windows 서명 정보 해석 경고가 1건 있었으나 클라이언트는 `Accepted`였고 MCP 명령은 정상 실행됐다.

**다음 작업**

- 기본 씬을 프로젝트 에셋으로 저장하고 마우스 세로 슬라이스의 테스트부터 구현

## 2026-08-15 — 청소 시스템과 씬 설계 확정

**사용자 결정**

- 오염 표현은 표면 마스크와 틈새 오브젝트를 함께 쓰는 하이브리드 방식을 사용한다.
- 에어건, 면봉과 헝겊을 오른쪽 이미지 UI에서 자유롭게 바꾸며 3D 도구 모델은 표시하지 않는다.
- 헝겊 청소는 별도 얼룩을 지우는 대신 Unity 조명에 반응하는 광택을 복원한다.
- `Space`를 누르면 진행도와 관계없이 남은 청소 지점이 반짝인다.
- 전체 진행도 90%에서 자동으로 청소를 완료한다.
- 메인 메뉴, 오프닝, 장비별 3개 스테이지와 엔딩의 여섯 씬을 사용한다.

**Codex 보완**

- 물체가 지나치게 어두워지지 않도록 닦기 전에도 확산광은 유지하고 광택과 반사만 낮추는 방식을 제안했다.
- 장비별 씬은 독립시키되 공통 `StageRoot`로 청소 코드와 UI 복사를 방지하도록 설계했다.
- 설정, 크레딧과 추억 이미지는 짧은 흐름을 끊지 않도록 UI 패널로 두었다.

**변경 파일**

- `docs/GAME_DESIGN.md`
- `docs/DEVELOPMENT_ROADMAP.md`
- `docs/NONTECHNICAL_GUIDE.md`
- `docs/ASSET_REQUIREMENTS.md`
- `docs/HUMAN_IN_THE_LOOP.md`
- `submission/FORM_DRAFT.md`
- `submission/SUBMISSION_CHECKLIST.md`
- `submission/CODEX_COLLABORATION.md`

**다음 작업**

- 사용자가 설계와 에셋 요구사항을 검토한다.
- 승인 후 마우스 세로 슬라이스 구현 계획을 작성한다.

## 2026-08-15 — 마우스 세로 슬라이스 구현 계획

**사용자 승인**

- 청소·씬 설계를 승인했다.
- 좌클릭 청소, 우클릭 회전, `Space` 강조와 UI 또는 숫자키 도구 선택을 확정했다.

**Codex 작업**

- 테스트 가능한 규칙 모델에서 Unity 컴포넌트, 씬 조립과 Web 브라우저 검증까지 이어지는 8개 작업 계획을 작성했다.
- 마우스 임시 모델을 사용해 외부 에셋을 기다리지 않고 첫 세로 슬라이스를 완성하도록 범위를 고정했다.
- 키보드·헤드셋·최종 엔딩은 첫 Web 빌드가 검증된 뒤 기존 로드맵 단계에서 구현하도록 분리했다.

**계획 파일**

- `docs/superpowers/plans/2026-08-15-mouse-vertical-slice.md`

**다음 작업**

- 사용자가 실행 방식을 선택한다.
- 선택 후 계획을 작업별 테스트 우선 순서로 구현한다.

## 2026-08-15 — 외부 에셋 기준점과 컴파일 정상화

**사용자 작업**

- Asset Store 패키지를 내려받고 마우스, 키보드, 헤드셋과 책상 프리팹을 직접 선정했다.
- 원본 패키지 이동과 사용하지 않는 데모 스크립트 비활성화를 승인했다.

**Codex 작업**

- 네 프리팹의 메시, UV, 재질, 콜라이더, LOD와 누락 스크립트를 검사했다.
- 선택 프리팹의 의존성이 끊기지 않도록 네 원본 패키지를 `Game/Assets/ThirdParty/Source/` 아래로 Unity 에셋 이동했다.
- 사용하지 않는 `Make Your Gadget assets` 데모 코드가 최신 Unity 프로젝트에서 일으킨 컴파일 오류를 원본 삭제 없이 로컬 어셈블리 제약으로 비활성화했다.
- Unity가 자동 갱신한 URP·품질 설정, TMP Essential Resources와 Scene Template 설정을 프로젝트 기준점으로 포함했다.

**검증 결과**

- 마우스 LOD0 메시에는 353개 정점과 유효한 UV가 있어 UV 기반 먼지·광택 마스크에 사용할 수 있다.
- 네 선택 프리팹이 모두 로드되고 의존성은 `ThirdParty/Source` 아래에서 해석되며 누락 스크립트는 0개다.
- 새 컴파일 이후 Unity는 `isCompiling=False`, `isUpdating=False`였고 MCP로 다시 읽은 콘솔은 오류 0건, 경고 0건이었다.
- `Game/Assets/ThirdParty` 원본은 `.gitignore`로 제외되어 공개 저장소에 포함되지 않는다.

**다음 작업**

- 마우스 세로 슬라이스의 첫 기능인 에어건·면봉·헝겊 선택 규칙을 테스트 우선으로 구현한다.

## 2026-08-15 — 청소 도구 선택 규칙 구현

**Codex 작업**

- 에어건, 면봉과 헝겊을 나타내는 `CleaningTool`과 현재 선택을 관리하는 `ToolSelectionModel`을 구현했다.
- 시작 도구는 에어건이며 같은 도구를 다시 선택할 때 변경 이벤트가 중복 발생하지 않도록 했다.
- 열린 Unity 안에서 EditMode 테스트를 비동기로 실행하고 JSON 결과를 남기는 Editor 명령을 추가했다.

**TDD 기록**

- RED: 구현 전 테스트 컴파일에서 `CleanToContinue.Core`가 존재하지 않는다는 실패를 확인했다.
- 환경 수정: Unity 6의 `TestAssemblies`가 TestRunner 참조를 자동 제공하므로 테스트 asmdef의 중복 직접 참조를 제거했다.
- GREEN: Unity Test Runner에서 도구 선택 테스트 2개 통과, 실패 0개, 건너뜀 0개를 확인했다.

**문제와 해결**

- MCP 동적 명령 안에서 Test Runner를 동기 실행하면 Editor 업데이트 루프가 막혀 Unity 재실행이 필요했다.
- 테스트 실행을 즉시 반환하는 Unity 메뉴 명령과 재연결 가능한 콜백으로 옮겨 이후 테스트가 Editor를 멈추지 않게 했다.

**검증 결과**

- `TestResults/editmode-latest.json`: `Passed`, 통과 2개, 실패 0개, 건너뜀 0개.
- 테스트 완료 후 MCP 콘솔: 오류 0건, 경고 0건.

**다음 작업**

- 세 오염 진행도를 합산하고 90%에서 한 번만 완료하는 규칙을 테스트 우선으로 구현한다.

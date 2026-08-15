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

## 2026-08-15 — 청소 범위와 90% 완료 규칙 구현

**Codex 작업**

- UV 표면을 격자로 나눠 아직 청소되지 않은 칸만 집계하는 `CoverageGrid`를 구현했다.
- 먼지·틈새·광택 진행도를 동일 가중치로 평균내는 `StageProgressModel`과 공통 진행도 계약을 구현했다.
- 전체 평균 90%에서 완료 상태를 잠그고 `Completed` 이벤트를 한 번만 발생시키도록 했다.

**TDD 기록**

- RED: `CoverageGrid`, `IProgressSource`와 `StageProgressModel`이 없다는 테스트 컴파일 실패를 확인했다.
- 첫 GREEN 시도에서 정확히 `0.9`인 세 값을 `float`로 더하면 평균이 내부적으로 `0.8999999`가 되어 경계 테스트 1개가 실패했다.
- 합산만 `double` 정밀도로 수행하도록 원인을 수정한 뒤 전체 EditMode 테스트 8개가 통과했다.

**검증 결과**

- Unity Test Runner: 통과 8개, 실패 0개, 건너뜀 0개.
- 검증 범위: 같은 위치 중복 집계 방지, 범위 밖 UV 고정, 동일 가중 평균, 90% 미만 미완료, 정확히 90% 완료와 완료 이벤트 1회.
- 테스트 완료 후 MCP 콘솔: 오류 0건, 경고 0건.

**다음 작업**

- GPU 마스크에 원형 브러시를 기록하고 먼지 제거와 광택 복원 레이어에 연결한다.

## 2026-08-16 — GPU 먼지·광택 표면 마스크 구현

**사용자 승인**

- CPU 진행 격자와 GPU 시각 마스크를 결합하고 더블 버퍼 방식으로 Web 호환성을 확보하는 설계를 승인했다.

**Codex 작업**

- Compute Shader 없이 두 RenderTexture를 번갈아 사용하는 `RuntimeMaskPainter`와 원형 스탬프 셰이더를 구현했다.
- 한 Renderer에서 먼지와 광택 레이어가 각자의 MaterialPropertyBlock 속성만 수정하도록 `SurfaceMaskLayer`를 구현했다.
- 먼지는 색 혼합으로 제거하고 헝겊 청소는 Smoothness와 실제 URP 조명 반응을 복원하는 `CleanableSurface` 셰이더를 추가했다.
- 기존 Editor 테스트 명령을 EditMode와 PlayMode 양쪽에서 결과 JSON을 남기도록 확장했다.
- 런타임 문자열로 찾는 숨김 스탬프 셰이더가 Web 빌드에서 제거되지 않도록 `Always Included Shaders`에 등록했다.

**TDD와 디버깅 기록**

- RED: `RuntimeMaskPainter`와 `SurfaceMaskLayer`가 없다는 EditMode·PlayMode 테스트 컴파일 실패를 확인했다.
- GPU 마스크 수명주기 테스트가 파괴된 Unity 객체에 다시 접근하는 잘못된 가정으로 실패해, 생성 객체 수가 2개 증가했다가 Dispose 후 원복되는 실제 누수 검사로 수정했다.
- 첫 PlayMode 실행에서 MonoBehaviour 필드 초기화 중 `MaterialPropertyBlock` 네이티브 객체를 생성한 오류를 재현하고, 첫 사용 시 지연 생성하도록 수명주기를 수정했다.

**검증 결과**

- EditMode: 통과 11개, 실패 0개. 숨김 스탬프 셰이더의 Player 빌드 포함 여부도 검사한다.
- PlayMode: 통과 4개, 실패 0개. 동일 Renderer의 먼지·광택 마스크와 기존 속성 보존도 검사한다.
- 임시 URP 장면에서 셰이더 지원 상태와 지역적인 먼지 제거를 다각도 캡처로 확인한 뒤 임시 오브젝트를 제거했다.
- 최종 MCP 콘솔은 오류 0건, 경고 0건이었다.

**구현 메모**

- 계획의 Shader Graph 수식은 버전 관리와 자동 생성 안정성을 위해 같은 URP PBR 수식을 가진 ShaderLab 파일로 구현했다.
- 기본 마스크 해상도는 시각용 512×512, 진행도용 64×64이며 장비별 설정에서 조정할 수 있다.
- Unity의 강제 에셋 새로고침 과정에서 `ProjectSettings.asset`이 현재 Unity 6000.3 직렬화 형식으로 자동 갱신됐으며, 표면 마스크와 무관한 수동 플랫폼 설정 변경은 하지 않았다.

**다음 작업**

- 면봉으로 제거할 틈새 오염 지점과 `Space` 잔여 오염 강조를 구현한다.

## 2026-08-16 — 면봉 틈새 오염과 잔여 오염 강조 구현

**Codex 작업**

- 면봉으로만 줄일 수 있는 개별 `GapDirtSpot`과 여러 지점의 평균 진행도를 제공하는 `GapDirtGroup`을 구현했다.
- 오염량이 줄수록 시각 오브젝트가 원래 크기에서 25%까지 작아지고, 완료 시 해당 청소 Collider만 비활성화되도록 했다.
- 표면의 남은 GPU 마스크와 미완료 틈새 지점을 1.2초 동안 `sin` 곡선으로 반짝이는 `HighlightController`를 구현했다.
- 강조는 `Time.timeScale`과 무관한 시간을 사용하고 청소 진행도를 전혀 바꾸지 않도록 분리했다.

**TDD와 디버깅 기록**

- RED: `CleanToContinue.Gap`과 `CleanToContinue.Highlight`가 없다는 Unity 컴파일 오류 6건을 확인했다.
- 첫 구현 후 `GapDirtGroup`에 공통 진행도 계약의 `Tool` 속성이 빠져 런타임 어셈블리 컴파일이 중단된 정확한 오류를 Editor 로그에서 찾았다.
- 컴파일 중에도 이전 테스트 JSON이 남아 기존 11개가 통과한 것처럼 보일 수 있어, 이후 검증에서는 결과 파일의 갱신 시각과 새 테스트 개수를 함께 확인했다.

**검증 결과**

- EditMode: 통과 15개, 실패 0개. 잘못된 도구 차단, 두 틈새의 평균 진행도, 누락 참조 제외, Collider 완료 처리와 강제 완료를 검사한다.
- PlayMode: 통과 5개, 실패 0개. `timeScale = 0`에서도 미완료 부분만 강조되고 1.2초 후 꺼지며 진행도가 보존되는지 검사한다.
- 임시 URP 장면의 다각도 캡처에서 미완료 표면·틈새는 밝고 완료된 틈새는 어둡게 유지되는 것을 확인했다.
- 코드 리뷰에서 누락된 틈새 참조를 이미 청소된 것으로 계산하는 위험을 발견해, 유효한 지점만 평균에 포함하는 RED→GREEN 회귀 테스트로 수정했다.
- 임시 검증 오브젝트와 재질은 제거했으며 빈 `Untitled` 씬은 저장하지 않은 수정 상태로 남겨 사용자 데이터의 강제 폐기를 피했다.

**다음 작업**

- 좌클릭 청소, 우클릭 회전, `Space` 강조와 숫자키 도구 전환을 연결하는 입력 라우팅을 구현한다.

## 2026-08-16 — 번호형 여섯 씬 골격 선행 구축

**사용자 결정**

- 기능 구현 결과를 Unity에서 바로 확인할 수 있도록 씬 조립을 앞당기고, 씬 파일에 실행 순서 번호를 붙이기로 했다.

**Codex 작업**

- `01.MainMenu`, `02.Opening`, `03.Mouse`, `04.Keyboard`, `05.Headset`, `06.Ending` 씬을 생성했다.
- 모든 씬에 공통 Root, 카메라, 조명, Canvas, EventSystem과 현재 씬을 알려주는 임시 안내 화면을 배치했다.
- 여섯 씬을 Build Settings에 번호 순서로 등록하고 템플릿 `SampleScene`은 파일 삭제 없이 목록에서 제외했다.
- 기존 씬과 사용자 오브젝트를 삭제하지 않고 필요한 골격만 보충하는 `NumberedSceneBuilder` Editor 메뉴를 추가했다.

**TDD와 검증**

- RED 1: 번호형 씬이 없고 Build Settings에 `SampleScene` 하나만 있어 새 테스트 2개가 실패했다.
- GREEN 1: 여섯 씬과 정확한 빌드 순서 생성 후 전체 EditMode 17개가 통과했다.
- RED 2: 사용자가 열어 둔 `01.MainMenu`에서 빌더를 재실행하면 씬을 닫는 실패를 재현했다.
- GREEN 2: 빌더가 직접 연 씬만 닫도록 수정해 전체 EditMode 18개가 통과했다.
- 코드 리뷰에서 실제 `01.MainMenu`를 저장·정리하는 테스트가 사용자 씬을 훼손할 수 있고, 같은 이름의 카메라·UI 설정을 재실행 시 덮어쓸 수 있음을 발견했다.
- RED 3: 사용자 설정 보존과 두 번 실행해도 구조가 중복되지 않는 격리 테스트를 추가하자 공개 `EnsureSkeleton` 계약이 없어 전체 19개 중 1개가 실패했다.
- GREEN 3: 테스트를 저장되지 않는 Preview Scene으로 격리하고, 기존 오브젝트·컴포넌트에는 기본값을 다시 쓰지 않도록 수정해 EditMode 19개가 통과했다.
- RED 4: 재검토에서 열린 씬 보존 테스트가 여전히 실제 여섯 씬을 저장하는 문제와, 기존 오브젝트에 Camera·Light 컴포넌트만 없을 때 Transform을 덮어쓰는 문제를 발견했다. 임시 복사 씬과 컴포넌트 누락 보존 테스트를 추가하자 18개 통과·2개 실패로 정확히 재현됐다.
- GREEN 4: 생명주기 검사를 고유 이름의 임시 씬 복사본으로 옮기고 정확히 그 복사본만 정리했다. 기존 오브젝트에는 Transform·태그를 유지한 채 누락 컴포넌트만 추가하도록 수정해 EditMode 20개와 PlayMode 5개가 모두 통과했다.
- MCP에서 `01.MainMenu`이 활성 씬이며 공통 루트 5개, `SceneTitle` 텍스트와 저장된 깨끗한 상태를 확인했다.
- 최종 확인 시 `01.MainMenu`과 기존 템플릿 `SampleScene`은 모두 열린 채로 깨끗한 상태였고, 프로젝트 오류는 0건이었다. Unity AI 계정 API 접근 지연 경고 1건은 게임 코드와 무관한 패키지 경고로 구분했다.
- 카메라 캡처 도구는 Screen Space Overlay UI를 렌더링하지 못했으므로 캡처 성공을 주장하지 않고 씬 구조·테스트로 검증했다.

**다음 작업**

- `03.Mouse`를 기준으로 좌클릭 청소, 우클릭 회전, `Space` 강조와 숫자키 도구 전환을 연결한다.

## 2026-08-16 — 해커톤 속도 우선 개발 원칙 확정

**사용자 결정**

- 각 기능을 처음부터 완벽하게 다듬기보다 구동되는 최소 수준에서 빠르게 다음 단계로 넘어가고, 전체 게임 흐름을 완성한 뒤 테스트하며 버그와 오류를 하나씩 수정하기로 했다.

**지침 반영**

- `AGENTS.md`에 최소 동작 구현 → 전체 플레이 흐름 연결 → 통합 테스트와 순차 안정화 순서를 추가했다.
- 불필요한 추상화·미세 최적화·최종 폴리싱은 후순위로 두되, 데이터 손실·보안·컴파일 및 실행 차단 오류는 즉시 처리하도록 안전 경계를 명시했다.
- 이후 기능은 핵심 조작이 한 번 작동하는지 먼저 확인하고 다음 기능으로 넘어가며, 미룬 문제는 개발 로그에 남긴다.

**다음 작업**

- `03.Mouse`에서 최소 입력 루프를 먼저 연결한다: 좌클릭 청소, 우클릭 회전, `Space` 강조, 숫자키 도구 전환.

## 2026-08-16 — 입력 라우팅과 장비 회전 구현

**Codex 작업**

- 코드로 만든 Input System 액션에 포인터 위치, 좌·우 버튼, `Space`, 숫자키 `1`·`2`·`3`의 Web 호환 바인딩을 추가했다.
- `StageInteractionController`가 8번 `Cleanable` 레이어만 Raycast하고, 선택된 도구에 맞게 먼지·광택·틈새 오염으로만 청소 입력을 보낸다.
- 우클릭 드래그를 회전에 우선 배정해 같은 프레임의 청소를 막고, `EquipmentRotator`의 세로 회전은 설정 범위 밖으로 나가지 않도록 제한했다.
- UI 위에서 시작한 누름은 해제 전까지 계속 차단하고, 창 포커스가 사라질 때 향후 오디오 협력 컴포넌트에 정지 신호를 보내도록 했다.

**TDD와 디버깅 기록**

- RED: 새 EditMode·PlayMode 테스트를 추가한 뒤 `CleanToContinue.Input`, `EquipmentRotator`, `StageInteractionController`가 없다는 컴파일 오류 5건을 Unity 콘솔에서 확인했다.
- 첫 PlayMode 실행은 5개 실패였다. 원인은 빈 `GapDirtGroup`이 기존 계약에 따라 진행도 100%를 반환하고 면봉 전용 픽스처에 표면 레이어가 없었던 테스트 준비 오류였다.
- 세 종류의 실제 오염을 한 픽스처에 두되 광선이 닿지 않는 위치로 분리해, 도구별 “다른 오염 불변” 조건을 유효하게 검증하도록 수정했다.

**검증 결과**

- 열린 Unity Editor의 EditMode 테스트: 통과 21개, 실패 0개, 건너뜀 0개 (`TestResults/editmode-latest.json`).
- 열린 Unity Editor의 PlayMode 테스트: 통과 10개, 실패 0개, 건너뜀 0개 (`TestResults/playmode-latest.json`).
- 새 PlayMode 다섯 테스트는 에어건→먼지, 헝겊→광택, 면봉→틈새, 우클릭 회전 우선, UI 시작 클릭 차단·해제 후 재입력을 검사한다.
- 최종 Unity 콘솔에는 게임 코드 오류가 없었다. Unity AI Assistant 계정 API 접근 지연 경고 1건은 기존 패키지의 네트워크 경고로 분리했다.

**다음 작업**

- `03.Mouse` 조립 단계에서 이 입력 컴포넌트를 실제 StageRoot·UI·오디오와 연결하고, 물리 마우스/브라우저 입력을 포함한 장면 검증을 수행한다.

## 2026-08-16 — 입력 하이라이트 의존성 회귀 수정

**문제와 수정**

- 코드 검토에서 `StageInputController.Configure`가 받은 `HighlightController`를 저장해도 상호작용 컨트롤러가 존재하면 `Space`가 그쪽의 비어 있는 참조만 호출할 수 있음을 발견했다.
- 직접 설정된 하이라이트를 우선 실행하고, 구성 시 같은 참조를 `StageInteractionController`에도 동기화했다. 직접 참조가 없는 기존 조립 방식은 상호작용 컨트롤러 경로로 안전하게 대체된다.

**TDD와 검증**

- RED: 실제 가상 Keyboard의 `<Keyboard>/space` 이벤트를 보낸 뒤 설정된 표면의 `_HighlightPulse`가 `0`인 회귀를 재현했다. PlayMode는 통과 11개, 실패 1개였다.
- 테스트 어셈블리가 Input System API를 직접 사용하므로 `CleanToContinue.PlayModeTests.asmdef`에 공식 `Unity.InputSystem` 참조를 추가했다.
- GREEN: 실제 Keyboard의 `Space`와 `2`→`3`→`1` 이벤트로 하이라이트 및 도구 선택을 검사한 뒤, PlayMode 테스트 12개가 모두 통과했다.

**다음 작업**

- `03.Mouse` 조립 단계에서 실제 UI 버튼·브라우저 포커스와 함께 입력 루프를 최종 검증한다.

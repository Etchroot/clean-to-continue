# Human-in-the-loop 결정 기록

이 문서는 사용자가 직접 제안·선택·수정한 부분과 Codex가 보완한 부분을 구분한다. 구현 상태는 `기획`, `진행 중`, `구현`, `검증` 중 하나로 기록한다.

| ID | 날짜 | 사용자 요구·결정 | Codex 제안·보완 | 최종 결정 | 상태 | 증거 |
|---|---|---|---|---|---|---|
| HIL-001 | 2026-08-15 | 깨끗해지는 만족감이 핵심인 청소게임을 만들고 싶다. | 하루 제작에는 한정된 오브젝트 중심 구조를 권장했다. | 제한형 3D 청소게임으로 진행한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-002 | 2026-08-15 | 게이머의 책상에서 컴퓨터 장비를 청소한다. | 한 장비 집중을 제안했으나 사용자 의견에 따라 공통 시스템으로 범위를 조정했다. | 마우스, 키보드, 헤드셋 3스테이지로 구성한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-003 | 2026-08-15 | 먼지, 틈새, 얼룩, 완료 보상의 네 단계로 단순화한다. | 장비별로 같은 단계를 재사용하고 강조점을 다르게 한다. | 모든 스테이지가 네 단계 구조를 공유한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-004 | 2026-08-15 | 한쪽에 원형 진행 휠과 퍼센트를 표시한다. | 현재 단계와 도구 아이콘을 함께 표시한다. | 숫자·원형 채움·도구 아이콘을 사용한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-005 | 2026-08-15 | 완료 시 중앙 이미지 한 장과 회상 대사 한 줄을 보여준다. | 세 이미지가 성장 순서가 되도록 연결했다. | 오락기 구경 → 친구와 2인 플레이 → PC방 단체 환호 순서로 구성한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-006 | 2026-08-15 | 사용 경험이 있는 Unity로 만들기로 했다. | 웹 빌드 위험과 다른 엔진을 비교해 Unity를 권장했다. | Unity 6 기반 Web 게임으로 개발한다. | 기획 | `docs/DEVELOPMENT_ROADMAP.md` |
| HIL-007 | 2026-08-15 | 형식적인 가제 대신 짧고 게임다운 이름이 필요하다. | `Clean to Continue` 등 세 후보를 제안했다. | 공식 제목은 `Clean to Continue`다. | 검증 | `README.md` |
| HIL-008 | 2026-08-15 | Codex로 게임 코드 전체를 개발하고 Unity와 최대한 연동한다. | Unity 공식 MCP를 통한 실시간 에디터 연결을 제안했다. | Codex가 주 개발자가 되고 무료 MCP 중심으로 Unity와 연결한다. | 검증 | `docs/CODEX_UNITY_WORKFLOW.md`, `docs/DEVELOPMENT_LOG.md` |
| HIL-009 | 2026-08-15 | GitHub `Etchroot` 계정에 공개 저장소를 만든다. | 에셋 원본 제외와 MIT License를 제안했다. | `clean-to-continue` Public 저장소, MIT License를 사용한다. | 검증 | `LICENSE`, `submission/ASSET_CREDITS.md` |
| HIL-010 | 2026-08-15 | 최신 Unity 6.5 대신 어떤 Editor를 설치할지 확인했다. | Web 빌드와 MCP 안정성을 위해 Unity 6.3 LTS 최신 패치를 권장했다. | Unity 6000.3.22f1과 Web Build Support를 사용한다. | 검증 | `Game/ProjectSettings/ProjectVersion.txt`, `docs/DEVELOPMENT_LOG.md` |
| HIL-011 | 2026-08-15 | Unity 설치와 MCP 설정 과정에서 화면의 설치·제거·Bridge·클라이언트 상태를 직접 확인했다. | Codex가 파일·프로세스·컴파일 로그를 검증하고 Unity 콘솔·씬·GameObject를 MCP로 읽었다. | Unity Bridge는 `Running`, Codex 클라이언트는 `Accepted`이며 별도 `Pending Connections` 승인이 필요 없는 연결 상태임을 확인했다. | 검증 | `docs/DEVELOPMENT_LOG.md`, `docs/CODEX_UNITY_WORKFLOW.md` |
| HIL-012 | 2026-08-15 | 세 도구를 자유롭게 바꾸고 에어건·면봉·헝겊으로 고정하며 3D 도구는 표시하지 않는다. | 오염 마스크와 틈새 오브젝트를 함께 쓰는 하이브리드 구조, 아이콘 상태와 도구별 사운드를 제안했다. | 오른쪽 이미지 버튼으로 세 도구를 선택하고 시각 효과와 사운드로 사용 상태를 전달한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-013 | 2026-08-15 | 메인 메뉴, 오프닝과 엔딩이 필요하며 메뉴에는 시작·설정·크레딧 버튼을 둔다. | 장비별 독립 씬 3개와 공통 `StageRoot`, 설정·크레딧·추억 UI 패널을 제안했다. | `MainMenu`, `Opening`, 장비별 3개, `Ending`의 여섯 씬을 사용한다. | 기획 | `docs/GAME_DESIGN.md` |
| HIL-014 | 2026-08-15 | 별도 얼룩 없이 헝겊 청소 후 광택을 복원하고, `Space`로 잔여 오염을 강조하며 90%에서 자동 완료한다. | 완전한 비조명보다 기본 확산광은 유지하고 마스크로 Smoothness와 반사를 복원하는 방식을 제안했다. | 헝겊은 광택 복원 도구이며 강조 기능은 진행도 제한 없이 사용하고 전체 90%를 완료 기준으로 삼는다. | 기획 | `docs/GAME_DESIGN.md`, `docs/NONTECHNICAL_GUIDE.md` |
| HIL-015 | 2026-08-15 | 마우스 세로 슬라이스의 조작 설계를 확정했다. | 청소와 회전 입력 충돌을 피하도록 좌클릭·우클릭을 분리하고 숫자키 단축키를 제안했다. | 좌클릭 드래그는 청소, 우클릭 드래그는 회전, `Space`는 강조, UI 또는 `1`·`2`·`3`은 도구 선택으로 사용한다. | 기획 | `docs/superpowers/plans/2026-08-15-mouse-vertical-slice.md` |
| HIL-016 | 2026-08-15 | 사용할 마우스·키보드·헤드셋·책상 프리팹을 직접 고르고 `ThirdParty`에 배치했으며, 사용하지 않는 데모 코드 때문에 난 컴파일 오류는 비활성화해도 된다고 승인했다. | 선택 프리팹의 메시·UV·재질·콜라이더와 의존성을 검사하고 원본 패키지를 `ThirdParty/Source`로 모으는 방법을 제안했다. | 선택 프리팹과 그 의존성만 게임에 사용하고, 원본 패키지는 공개 Git에서 제외하며 불필요한 데모 스크립트는 로컬 어셈블리 제약으로 컴파일하지 않는다. | 검증 | `submission/ASSET_CREDITS.md`, `docs/DEVELOPMENT_LOG.md` |
| HIL-017 | 2026-08-16 | CPU 진행도와 GPU 시각 마스크를 결합하고 두 텍스처를 번갈아 쓰는 Web 친화적 방식을 승인했다. | 64×64 진행 격자와 512×512 먼지·광택 마스크, 원본 Material을 보존하는 별도 청소 셰이더를 제안했다. | 먼지와 광택은 독립 GPU 마스크로 표현하고 진행도는 중복 집계를 막는 CPU 격자로 계산한다. | 검증 | `Game/Assets/CleanToContinue/Runtime/Surface`, `docs/DEVELOPMENT_LOG.md` |
| HIL-018 | 2026-08-16 | 기능을 더 만들기 전에 직접 열어 확인할 수 있는 씬을 먼저 만들고 `01.MainMenu`, `02.Opening`, `03.Mouse`처럼 번호를 붙이자고 제안했다. | 여섯 씬 전체를 먼저 생성하고 Build Settings도 같은 순서로 등록하며, 재실행 가능한 골격 빌더로 사용자 오브젝트를 보존하는 방식을 제안했다. | `01.MainMenu`부터 `06.Ending`까지 번호형 씬을 먼저 만들고 이후 기능을 이 씬들에 채운다. | 검증 | `Game/Assets/CleanToContinue/Scenes`, `Game/ProjectSettings/EditorBuildSettings.asset` |

## 새 항목 작성 규칙

사용자 말의 의미를 과장하지 않고 요약한다. 구현 파일이나 테스트가 생기면 증거 열에 실제 경로를 연결하고 상태를 갱신한다.

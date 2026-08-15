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

## 새 항목 작성 규칙

사용자 말의 의미를 과장하지 않고 요약한다. 구현 파일이나 테스트가 생기면 증거 열에 실제 경로를 연결하고 상태를 갱신한다.

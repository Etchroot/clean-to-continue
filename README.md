# Clean to Continue

> Clean the gear. Continue the memory.

**Clean to Continue**는 먼지 쌓인 마우스, 키보드, 헤드셋을 직접 청소하며 게임과 함께 성장한 순간들을 되찾는 짧은 3D 힐링 게임입니다. OpenAI Game Builders Seoul 출품을 목표로 Unity 6와 Codex를 사용해 개발합니다.

## 현재 상태

- 게임 기획과 제목 확정
- 개발·심사 기록 체계 구축
- Unity 6.3 LTS Universal 3D 프로젝트와 Codex–Unity 공식 MCP 연결 검증 완료
- `01.MainMenu`부터 `06.Ending`까지 번호형 씬 골격과 Build Settings 구성 완료
- 다음 핵심 목표: 마우스 스테이지 세로 슬라이스

## 프로젝트 구조

- `Game/`: Unity 프로젝트
- `docs/GAME_DESIGN.md`: 게임 규칙과 연출
- `docs/DEVELOPMENT_ROADMAP.md`: 구현 순서와 완료 기준
- `docs/CODEX_UNITY_WORKFLOW.md`: Codex와 Unity의 연동 방식
- `docs/NONTECHNICAL_GUIDE.md`: 비전공자를 위한 구조 설명
- `docs/ASSET_REQUIREMENTS.md`: 구매·준비할 에셋과 기술 조건
- `docs/HUMAN_IN_THE_LOOP.md`: 사용자의 직접 결정 기록
- `docs/DEVELOPMENT_LOG.md`: 날짜별 개발 기록
- `submission/`: 신청서, 영상, Codex 활용과 검수 자료

## Unity 프로젝트 만들기

Unity Hub에서 Unity 6.3 LTS의 **Universal 3D** 템플릿을 선택합니다.

- 프로젝트 이름: `Game`
- 위치: `C:\Users\차명근\Documents\openaigamebuilders`
- 최종 프로젝트 경로: `C:\Users\차명근\Documents\openaigamebuilders\Game`
- 추가 모듈: Web Build Support

## 문서 읽는 순서

처음 보는 사람은 `GAME_DESIGN` → `DEVELOPMENT_ROADMAP` → `NONTECHNICAL_GUIDE` 순서로 읽으면 됩니다. 개발 과정에서 누가 무엇을 결정했는지는 `HUMAN_IN_THE_LOOP`와 `DEVELOPMENT_LOG`에서 확인할 수 있습니다.

## 라이선스

직접 작성한 코드와 문서는 MIT License로 공개합니다. Unity Asset Store 및 기타 제3자 에셋은 각 제공자의 별도 라이선스를 따르며 이 저장소의 MIT License 대상이 아닙니다.

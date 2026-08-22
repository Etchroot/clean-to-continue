# Clean to Continue

> Clean the gear. Continue the memory.

**Clean to Continue**는 먼지 쌓인 마우스, 키보드, 헤드셋을 직접 청소하며 게임과 함께 성장한 순간들을 되찾는 짧은 3D 힐링 게임입니다. OpenAI Game Builders Seoul 출품을 목표로 Unity 6와 Codex를 사용해 개발합니다.

## 현재 상태

- 마우스·키보드·헤드셋 3개 청소 스테이지 구현 완료
- 인트로, 메인 메뉴, 오프닝, 추억 보상과 엔딩의 전체 흐름 구현 완료
- Unity 6.3 LTS와 Codex–Unity 공식 MCP 연동 개발 완료
- Unity WebGL 및 itch.io 업로드용 ZIP 생성 완료
- 최종 자동 검증: EditMode 26개, PlayMode 63개 통과
- 공개 플레이 URL·썸네일·데모 영상은 제출 폼에 별도 입력 필요

## 프로젝트 구조

```text
openaigamebuilders/
├─ Game/                              # Unity 프로젝트
│  ├─ Assets/
│  │  ├─ CleanToContinue/
│  │  │  ├─ Editor/                  # 씬 생성·테스트·WebGL 빌드 도구
│  │  │  ├─ Runtime/
│  │  │  │  ├─ Audio/               # 배경음·청소음·효과음
│  │  │  │  ├─ Flow/                # 오프닝과 씬 전환
│  │  │  │  ├─ Input/               # 청소·회전 입력
│  │  │  │  ├─ Stage/               # 진행도와 완료 처리
│  │  │  │  ├─ Surface/             # UV·폴리곤 청소 시스템
│  │  │  │  └─ UI/                  # 메뉴·도구·추억 UI
│  │  │  ├─ Scenes/                  # 여섯 개의 게임 씬
│  │  │  ├─ Shaders/                 # 먼지·광택 마스크
│  │  │  └─ Tests/                   # EditMode·PlayMode 테스트
│  │  └─ ThirdParty/                 # 외부 에셋, Git 공개 제외
│  ├─ Packages/                      # Unity 패키지 설정
│  └─ ProjectSettings/               # Unity 프로젝트 설정
├─ docs/                              # 기획·개발·HIL 기록
├─ submission/                        # 해커톤 제출 자료
├─ AGENTS.md                          # Codex 작업 지침
├─ README.md
└─ LICENSE
```

세부 게임 구조는 `docs/NONTECHNICAL_GUIDE.md`, 기획과 구현 과정은 `docs/GAME_DESIGN.md`와 `docs/DEVELOPMENT_LOG.md`에서 확인할 수 있습니다.

## Unity 프로젝트 열기

Unity Hub에서 `Game/` 폴더를 Unity 6000.3.22f1로 엽니다. 새 프로젝트를 만드는 경우 Unity 6.3 LTS의 **Universal 3D** 템플릿과 Web Build Support가 필요합니다.

- 프로젝트 이름: `Game`
- 위치: `C:\Users\차명근\Documents\openaigamebuilders`
- 최종 프로젝트 경로: `C:\Users\차명근\Documents\openaigamebuilders\Game`
- 추가 모듈: Web Build Support

Asset Store 원본은 라이선스 때문에 저장소에 포함되지 않습니다. 로컬 최종 프로젝트의 `Game/Assets/ThirdParty/`에 마우스·키보드·헤드셋·책상 에셋과 미디어 파일을 다시 배치한 뒤 Unity Editor 빌더를 실행해야 최종 화면을 완전히 재현할 수 있습니다. 필요한 항목은 `submission/ASSET_CREDITS.md`에서 확인합니다.

## 조작법

- 좌클릭 드래그: 선택한 도구로 청소
- 우클릭 드래그: 장비 회전
- `Space`: 남은 오염 부분 강조
- 오른쪽 버튼: 에어건 또는 헝겊 선택

## 테스트와 Web 빌드

- Unity 메뉴 `Tools > Clean to Continue > Run EditMode Tests`
- Unity 메뉴 `Tools > Clean to Continue > Run PlayMode Tests`
- Unity 메뉴 `Clean to Continue > Build Web Release`

Web 빌드 결과와 업로드용 ZIP은 라이선스·용량 정책상 Git에 포함하지 않습니다.

## 문서 읽는 순서

처음 보는 사람은 `GAME_DESIGN` → `DEVELOPMENT_ROADMAP` → `NONTECHNICAL_GUIDE` 순서로 읽으면 됩니다. 개발 과정에서 누가 무엇을 결정했는지는 `HUMAN_IN_THE_LOOP`와 `DEVELOPMENT_LOG`에서 확인할 수 있습니다.

## 라이선스

직접 작성한 코드와 문서는 MIT License로 공개합니다. Unity Asset Store 및 기타 제3자 에셋은 각 제공자의 별도 라이선스를 따르며 이 저장소의 MIT License 대상이 아닙니다.

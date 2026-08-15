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

**남은 검증**

- Codex 재시작 후 Unity의 `Pending Connections`에서 최초 연결 허용
- Codex에서 Unity 콘솔, 열린 Scene과 GameObject 정보 읽기

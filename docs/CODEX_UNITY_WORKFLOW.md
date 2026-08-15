# Codex–Unity 작업 방식

## 역할

- **사용자:** 게임 방향, 비주얼, 범위, 에셋 구매와 최종 결과를 결정한다.
- **Codex:** C# 코드, 셰이더, 에디터 도구, 테스트, 웹 빌드, Git과 문서를 구현·검증한다.
- **Unity Editor:** 씬과 에셋을 편집하고 플레이·빌드 결과를 실행한다.
- **Unity 공식 MCP:** Codex가 실행 중인 Unity의 씬, GameObject, 컴포넌트, 프로젝트 설정과 콘솔을 읽고 허용된 에디터 작업을 호출하게 한다.

Unity의 인에디터 유료 Assistant는 핵심 의존성이 아니다. 나중에 시험할 경우 작업 내역을 Codex와 별도로 기록한다.

## 권장 환경

- Unity 6.3 LTS
- Universal 3D 템플릿
- Web Build Support
- `com.unity.ai.assistant` 최신 호환 패키지
- Codex Desktop과 Unity 공식 MCP Server

## MCP 연결

1. Unity에서 `Edit > Project Settings > AI > Unity MCP Server`를 연다.
2. Unity Bridge가 `Running`인지 확인한다.
3. Codex MCP 서버 명령을 `%USERPROFILE%\.unity\relay\relay_win.exe --mcp`로 등록한다.
4. 프로젝트 경로 인수로 `Game/`을 지정해 여러 Unity 인스턴스가 있어도 이 프로젝트에 연결한다.
5. Codex를 다시 시작한다.
6. Unity의 `Pending Connections`에서 사용자가 Codex 연결을 승인한다.
7. Codex에서 콘솔 메시지와 현재 씬 구조를 읽어 연결을 검증한다.

공식 문서: <https://docs.unity3d.com/Packages/com.unity.ai.assistant@2.17/manual/integration/unity-mcp-get-started.html>

## MCP를 사용할 때

- 씬 계층, 컴포넌트와 프로젝트 설정 확인
- Unity 콘솔 오류 수집 및 원인 분석
- 프리팹과 GameObject 구성 검증
- 플레이 모드 전후 상태 확인

중요한 씬 변경은 현재 기획과 일치하는지 확인하고, 변경 파일과 검증 결과를 개발 로그에 남긴다.

## 연결 실패 시 대체 경로

1. Codex가 `Game/Assets`와 `Game/Packages` 파일을 직접 수정한다.
2. Unity가 스크립트를 다시 임포트하도록 에디터를 연다.
3. Unity 명령행 배치 모드로 테스트와 Web 빌드를 실행한다.
4. Editor 로그와 테스트 XML을 Codex가 읽어 수정한다.
5. 연결 문제가 해결되면 MCP 경로로 복귀한다.

## 작업 기록 구분

- `COD`: Codex가 제안하거나 구현한 작업
- `HIL`: 사용자가 직접 제안·선택·수정한 작업
- `UAI`: Unity 인에디터 Assistant가 수행한 작업

제출 자료에는 실제 사용한 도구만 기록하며 자동 생성이나 검증을 과장하지 않는다.

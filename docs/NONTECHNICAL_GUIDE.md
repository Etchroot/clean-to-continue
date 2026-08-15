# 비전공자를 위한 개발 가이드

이 문서는 게임이 커져도 “어떤 파일이 무슨 일을 하고, 무엇을 바꾸면 되는지”를 찾을 수 있게 유지한다.

## 먼저 알아둘 용어

- **Unity 프로젝트:** 게임의 코드, 장면, 이미지와 설정이 들어 있는 `Game/` 폴더다.
- **Scene:** 게임 화면 하나의 구성도다. 카메라, 조명, 장비와 UI가 배치된다.
- **GameObject:** Scene 안에 놓이는 물체의 기본 단위다.
- **Component:** GameObject에 붙여 회전, 표시, 소리 같은 기능을 추가한다.
- **Prefab:** 여러 Scene에서 다시 사용할 수 있게 저장한 완성된 GameObject 묶음이다.
- **Material:** 3D 물체 표면의 색, 반사와 질감을 정한다.
- **UV:** 2D 이미지의 어느 부분을 3D 표면의 어디에 붙일지 나타내는 좌표다.
- **오염 마스크:** 장비 표면의 먼지 또는 아직 광택이 복원되지 않은 위치를 기록하는 흑백 또는 채널 이미지다.
- **광택 마스크:** 헝겊으로 닦은 곳의 반사와 매끄러움을 복원할 위치를 기록하는 흑백 이미지다.
- **Shader:** Material이 화면에 보이는 방식을 계산하는 작은 그래픽 프로그램이다.
- **Web 빌드:** Unity 게임을 브라우저가 실행할 수 있는 WebAssembly와 웹 파일로 변환한 결과다.
- **MCP:** Codex가 파일뿐 아니라 실행 중인 Unity의 Scene과 콘솔 상태를 확인하게 하는 연결 규격이다.

## 게임 구조의 쉬운 설명

게임에는 한 번에 장비 하나가 화면 중앙에 놓인다. 플레이어는 장비를 돌리고 현재 도구로 더러운 부분을 문지른다. 오염이 줄어들면 원형 진행도가 올라간다. 모든 오염을 제거하면 추억 이미지가 나타나고 다음 장비로 넘어간다.

에어건과 헝겊은 장비의 2D 마스크를 지워 넓은 표면 변화를 만든다. 면봉은 키 사이와 힌지에 배치한 작은 오염을 제거한다. 헝겊 청소에서는 별도 얼룩 그림을 지우는 대신 닦은 부분의 원래 광택이 Unity 조명에 다시 반응하도록 만든다. 전체 진행도가 90%에 도달하면 마지막 작은 흔적을 자동 정리해 픽셀 찾기로 플레이가 막히지 않게 한다.

게임 흐름은 `MainMenu` → `Opening` → `MouseStage` → `KeyboardStage` → `HeadsetStage` → `Ending`의 여섯 씬으로 이어진다. 세 장비 씬은 공통 `StageRoot`를 사용하므로 UI와 청소 코드를 복사하지 않는다.

## 현재 개발 환경

- **Unity 6000.3.22f1:** 장기간 지원되는 Unity 6.3 LTS 계열의 실제 Editor 버전이다.
- **Universal Render Pipeline 17.3.0:** PC와 웹에서 3D 조명과 재질을 효율적으로 표현하는 렌더링 방식이다.
- **Input System 1.17.0:** 마우스 드래그, 클릭과 키보드 입력을 읽는 공식 패키지다.
- **Web Build Support:** 완성된 게임을 브라우저용 파일로 변환하는 Unity 모듈이다.
- **Visual Studio Community 2026:** C# 파일을 사람이 직접 읽거나 수정할 때 사용할 수 있는 편집기다. Codex는 이 프로그램이 열려 있지 않아도 프로젝트 파일을 수정할 수 있다.

`Game/Assets`에는 직접 만든 코드·Scene·Material이 들어가고, `Game/Packages`에는 프로젝트가 사용하는 공식 기능의 버전이 기록된다. `Game/ProjectSettings`에는 Unity와 Web 빌드 설정이 저장된다. `Game/Library`는 Unity가 다시 만들 수 있는 임시 캐시이므로 GitHub에는 올리지 않는다.

## 외부 에셋을 두는 위치

- `Game/Assets/ThirdParty/`에는 게임에서 고른 프리팹을 둔다.
- `Game/Assets/ThirdParty/Source/`에는 그 프리팹이 참조하는 원본 모델·재질·텍스처와 패키지 파일을 둔다.
- 이 폴더의 Asset Store 원본은 로컬 Unity 프로젝트에서는 보이지만 공개 GitHub에는 올라가지 않는다.
- Unity 빌드는 Scene이나 사용 중인 프리팹에서 참조한 에셋과 그 의존성을 따라가므로, 패키지 안의 사용하지 않는 일반 모델·텍스처는 보통 최종 빌드에서 제외된다. 단, `Resources`와 `StreamingAssets`처럼 항상 포함되는 특별 폴더는 따로 확인한다.

현재 선택한 마우스는 가까이서 청소하기에 필요한 UV를 가지고 있다. 그래서 표면의 어느 위치를 닦았는지 2D 마스크에 기록할 수 있다. 가져온 장식용 패키지의 사용하지 않는 데모 스크립트는 게임 기능이 아니며 최신 프로젝트에서 컴파일 오류를 냈기 때문에, 원본을 삭제하지 않고 해당 로컬 폴더 안의 어셈블리 설정으로 컴파일 대상에서 제외했다.

## 코드 지도

### 청소 도구 선택 규칙

- **하는 일:** 현재 도구가 에어건·면봉·헝겊 중 무엇인지 기억하고, 다른 도구로 바뀔 때만 변경 신호를 보낸다.
- **사용자에게 보이는 결과:** 게임을 시작하면 에어건이 선택되고, 같은 도구 버튼을 반복해서 눌러도 선택 효과나 사운드가 중복 재생되지 않는다.
- **자주 바꿀 값:** 도구의 종류는 `CleaningTool`에 모여 있다. 현재 세 도구는 기획에서 고정했으므로 임의로 늘리지 않는다.
- **관련 파일:** `Game/Assets/CleanToContinue/Runtime/Core/CleaningTool.cs`, `ToolSelectionModel.cs`
- **확인 방법:** `ToolSelectionModelTests.cs`의 EditMode 테스트 두 개가 기본 선택과 중복 선택 방지를 검사한다.

### Codex용 Unity 테스트 명령

- **하는 일:** 열려 있는 Unity Editor 안에서 EditMode 테스트를 비동기로 시작하고 결과를 `TestResults/editmode-latest.json`에 기록한다.
- **왜 필요한가:** 같은 프로젝트를 배치 모드 Unity로 한 번 더 열지 않고도 Codex가 실제 Unity Test Runner 결과를 확인할 수 있다.
- **관련 파일:** `Game/Assets/CleanToContinue/Editor/EditModeTestCommand.cs`
- **확인 방법:** Unity 메뉴의 `Tools > Clean to Continue > Run EditMode Tests`를 눌러도 같은 검사가 실행된다.

다음 구현에서 장비 회전, 오염, 진행도, 원형 UI, 완료 이미지와 Web 빌드 항목을 같은 형식으로 추가한다.

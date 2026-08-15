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

게임 흐름은 `01.MainMenu` → `02.Opening` → `03.Mouse` → `04.Keyboard` → `05.Headset` → `06.Ending`의 여섯 씬으로 이어진다. 번호는 Unity의 Project 창과 Build Settings에서 실행 순서를 바로 알게 해준다. 세 장비 씬은 공통 `StageRoot`를 사용하므로 UI와 청소 코드를 복사하지 않는다.

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

### 청소 범위와 90% 완료 규칙

- **하는 일:** 장비 표면을 작은 격자로 나눠 각 칸이 아직 더러운지 기억한다. 같은 칸을 반복해서 닦아도 처음 한 번만 진행도에 더한다.
- **사용자에게 보이는 결과:** 마우스를 한곳에서 계속 문질러 진행도를 억지로 채울 수 없고, 실제로 여러 영역을 청소해야 한다.
- **전체 진행도:** 먼지, 틈새와 광택 복원의 진행도를 같은 비중으로 평균낸다. 평균이 90%에 도달하면 완료 상태를 잠그고 완료 신호를 한 번만 보낸다.
- **경계값 보호:** 컴퓨터의 소수 계산 오차 때문에 정확한 90%가 89.999…%로 취급되지 않도록 합산 과정은 더 정밀한 숫자 형식을 사용한다.
- **관련 파일:** `Runtime/Surface/CoverageGrid.cs`, `Runtime/Progress/IProgressSource.cs`, `Runtime/Progress/StageProgressModel.cs`
- **확인 방법:** `CoverageGridTests.cs`와 `StageProgressModelTests.cs`가 반복 청소, 범위 밖 UV, 동일 가중치, 89.9%, 정확한 90%와 완료 1회 조건을 검사한다.

### 먼지 제거와 광택 복원 마스크

- **하는 일:** 클릭한 3D 표면의 UV 좌표에 둥근 브러시 자국을 남긴다. 먼지와 “아직 광택이 복원되지 않은 부분”을 서로 다른 흑백 마스크에 보관한다.
- **왜 텍스처가 두 장씩 필요한가:** GPU는 같은 텍스처를 동시에 안전하게 읽고 쓸 수 없으므로 현재 마스크를 읽어 다른 마스크에 결과를 쓴 다음 두 역할을 바꾼다. 이를 더블 버퍼라고 한다.
- **사용자에게 보이는 결과:** 에어건이 지나가면 회색 먼지가 사라지고, 헝겊이 지나가면 그 자리의 Smoothness가 높아져 Unity 조명과 반사가 선명해진다. 별도의 얼룩 그림은 나타나지 않는다.
- **성능 구조:** 화면 변화는 기본 512×512 GPU 마스크로 부드럽게 표현하고, 진행도는 64×64 CPU 격자로 계산한다. 전체 텍스처 픽셀을 C#에서 매번 수정하지 않아 Web 빌드의 CPU 정지를 줄인다.
- **원본 에셋 보호:** Asset Store의 원본 Material을 직접 바꾸지 않고 `CleanToContinue/Cleanable Surface` 셰이더를 쓰는 별도 Material을 장비 조립 단계에서 만든다.
- **웹 빌드 보호:** 스탬프 셰이더는 코드에서만 이름으로 찾기 때문에 Unity가 “미사용 파일”로 오해하지 않도록 Player 빌드의 항상 포함할 셰이더 목록에 등록했다.
- **관련 파일:** `RuntimeMaskPainter.cs`, `SurfaceMaskLayer.cs`, `Shaders/MaskStamp.shader`, `Shaders/CleanableSurface.shader`
- **확인 방법:** EditMode 테스트는 GPU 스탬프, 임시 텍스처 해제와 Player 빌드 포함을 검사하고, PlayMode 테스트는 실제 MeshCollider UV에서 잘못된 도구 차단·반복 입력·강제 완료·두 마스크의 공존을 검사한다.

### 면봉 틈새 오염과 `Space` 강조

- **틈새 오염:** 마우스 휠과 버튼 사이에 작은 오염 지점을 따로 배치한다. 면봉이 닿을 때만 남은 양이 줄고, 먼지용 에어건이나 헝겊은 영향을 주지 않는다.
- **사용자에게 보이는 결과:** 면봉으로 문지를수록 작은 부스러기가 원래 크기에서 25%까지 줄어든다. 완전히 제거되면 해당 지점의 Collider가 꺼져 더 이상 입력을 가로채지 않는다.
- **진행도:** 예를 들어 같은 크기의 지점 두 개 중 하나를 절반 청소하면 틈새 청소 전체는 25%다. 프리팹에서 참조 하나가 빠져도 그것을 청소 완료로 잘못 계산하지 않으며, 완료 시 `ForceFinish`가 호출되면 남은 지점도 안전하게 정리할 수 있다.
- **잔여 오염 강조:** `Space` 입력을 받으면 표면 마스크와 아직 남은 틈새만 1.2초 동안 부드럽게 밝아졌다가 어두워진다. 이미 완료된 지점은 반짝이지 않고 진행도도 변하지 않는다.
- **일시정지와 Web 대응:** 강조 시간은 게임 속도와 무관한 실제 시간을 사용하므로 일시정지 상태나 느린 프레임에서도 끝난다.
- **관련 파일:** `Runtime/Gap/GapDirtSpot.cs`, `GapDirtGroup.cs`, `Runtime/Highlight/HighlightController.cs`, `Runtime/Surface/SurfaceMaskLayer.cs`
- **확인 방법:** EditMode 테스트 4개가 면봉 전용 진행도와 누락 참조를 검사하고 PlayMode 테스트가 진행도 0%와 완료된 지점이 섞인 상태의 강조를 검사한다.

### 입력 연결과 장비 회전

- **하는 일:** `StageInputController`가 Unity의 공식 Input System으로 마우스 위치·좌클릭·우클릭·`Space`·숫자키 `1`/`2`/`3`을 읽는다. `StageInteractionController`는 좌클릭을 현재 도구에 맞는 오염에만 보내고, 우클릭은 장비 회전에만 보낸다.
- **사용자에게 보이는 결과:** 에어건은 먼지, 헝겊은 광택, 면봉은 틈새 오염에만 반응한다. 우클릭 드래그 중에는 청소가 함께 일어나지 않는다. UI에서 누르기 시작한 클릭은 버튼을 놓을 때까지 장비 뒤쪽을 청소하거나 회전하지 않는다.
- **회전 범위:** `EquipmentRotator`는 기본적으로 위아래 회전값을 `-35`~`55`도 범위에 고정한다. 게임 제작자는 Inspector의 `Min Pitch`, `Max Pitch`, `Sensitivity`로 장비마다 감도와 시야를 조정할 수 있다.
- **청소 대상 구분:** Unity의 8번 레이어 이름은 `Cleanable`이다. 입력 광선은 이 레이어만 검사하므로 책상과 장식물은 클릭해도 청소되지 않는다.
- **포커스 보호:** 브라우저 탭이나 Unity 창이 포커스를 잃으면, 나중에 연결될 도구 루프 사운드에 정지 신호를 보낸다.
- **관련 파일:** `Runtime/Input/StageInputController.cs`, `StageInteractionController.cs`, `EquipmentRotator.cs`, `Tests/EditMode/EquipmentRotatorTests.cs`, `Tests/PlayMode/StageInteractionControllerTests.cs`, `Game/ProjectSettings/TagManager.asset`
- **확인 방법:** Unity의 열린 Editor에서 EditMode 21개와 PlayMode 12개 테스트를 실행한다. 새 PlayMode 테스트는 도구별 Raycast 라우팅, 우클릭 회전 우선, UI 시작 클릭의 해제 전 차단과 실제 키보드 `Space`·`1`·`2`·`3` 입력을 검사한다.

### 번호형 여섯 씬 골격

- **하는 일:** 최종 게임 흐름에 필요한 여섯 씬을 `Assets/CleanToContinue/Scenes`에 미리 만들고 Build Settings에 01~06 순서로 등록한다.
- **현재 화면:** 각 씬은 아직 완성 화면이 아니라 서로 다른 배경색, 씬 이름과 앞으로 들어갈 내용을 설명하는 안내 문구를 보여준다.
- **공통 구조:** 모든 씬에는 `SceneRoot`, `EnvironmentRoot`, `ContentRoot`, `GameplayRoot`, 카메라, 조명, `UIRoot`와 `EventSystem`이 있다.
- **사용자 작업 보호:** `Clean to Continue > Build Numbered Scene Skeletons` 메뉴를 다시 실행해도 사용자가 추가한 오브젝트를 삭제하지 않는다. 이미 열어 둔 씬도 닫지 않고, 기존 카메라·조명·UI의 위치나 색상 같은 설정도 기본값으로 되돌리지 않는다.
- **SampleScene 처리:** Unity 템플릿의 `Assets/Scenes/SampleScene.unity` 파일은 삭제하지 않고 Build Settings에서만 제외했다.
- **관련 파일:** `Editor/NumberedSceneBuilder.cs`, `Tests/EditMode/NumberedSceneScaffoldTests.cs`, `Assets/CleanToContinue/Scenes/*.unity`
- **확인 방법:** Project 창의 `Assets/CleanToContinue/Scenes`에서 원하는 씬을 더블클릭하거나 현재 열려 있는 `01.MainMenu`의 Game 탭을 확인한다. EditMode 테스트는 실제 게임 씬을 수정하지 않는 Preview Scene과 테스트가 만든 임시 복사 씬에서 중복 생성, 사용자 설정과 열린 상태 보존을 검사한 뒤 임시 복사본만 지운다.

다음 구현에서 장비 회전, 입력, 원형 UI, 완료 이미지와 Web 빌드 항목을 같은 형식으로 추가한다.

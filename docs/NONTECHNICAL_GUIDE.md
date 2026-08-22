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

에어건과 헝겊은 장비 표면의 먼지와 무광택 코팅을 각각 제거한다. 헝겊 청소에서는 별도 얼룩 그림을 지우는 대신 닦은 부분의 원래 광택이 Unity 조명에 다시 반응하도록 만든다. 전체 진행도가 90%에 도달하면 마지막 작은 흔적을 자동 정리해 픽셀 찾기로 플레이가 막히지 않게 한다.

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

- **하는 일:** 현재 도구가 에어건·헝겊 중 무엇인지 기억하고, 다른 도구로 바뀔 때만 변경 신호를 보낸다.
- **사용자에게 보이는 결과:** 게임을 시작하면 에어건이 선택되고, 같은 도구 버튼을 반복해서 눌러도 선택 효과나 사운드가 중복 재생되지 않는다.
- **자주 바꿀 값:** 도구의 종류는 `CleaningTool`에 모여 있다. 현재 플레이 UI는 에어건과 헝겊 두 도구만 사용한다.
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
- **전체 진행도:** 먼지 제거와 광택 복원의 진행도를 같은 비중으로 평균낸다. 평균이 90%에 도달하면 완료 상태를 잠그고 완료 신호를 한 번만 보낸다.
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
- **진행도와 화면의 역할 차이:** 진행도용 작은 격자에서 이미 청소한 칸이어도 화면용 고해상도 마스크에는 매 입력의 브러시 자국을 계속 남긴다. 두 해상도가 다르기 때문에 진행도가 중복 증가하지 않는 것과 보이는 먼지가 빠짐없이 지워지는 것은 별도로 처리해야 한다.
- **키보드의 87% 버그:** 기존에는 UV 사각형 전체를 오염으로 계산해 메시가 사용하지 않는 빈 공간도 남은 오염이 됐다. 이제 실제 UV 삼각형과 겹치는 격자 칸만 진행도에 포함한다.
- **헤드셋의 폴리곤 방식:** 선택한 헤드셋 에셋은 모든 UV가 `(0,0)`에 겹쳐 텍스처 마스크를 사용할 수 없다. 그래서 원본 모델은 그대로 두고 먼지·광택 오버레이의 클릭 주변 삼각형만 숨긴다. 에어건과 헝겊은 서로 다른 오버레이를 사용하므로 독립적으로 진행된다.
- **읽기 가능한 복사 메시:** 헤드셋 원본은 `Read/Write`가 꺼져 있으므로 런타임에서 삼각형을 읽을 수 없다. 로컬 `Meshes/Generated`에 읽기 가능한 복사본을 만들고 오버레이에만 연결하며 Asset Store 원본은 수정하지 않는다. 이 파생 메시도 공개 Git에서는 제외하고 빌더로 다시 만든다.
- **원본 외형 보존:** `SurfaceMaterialTransfer`가 원본 프리팹의 Base Map·색·노멀 등 필요한 표면 정보를 프로젝트 전용 청소 재질로 옮긴다. 그 위에 먼지와 광택 마스크만 추가하므로 장비가 단색 자리표시자로 보이지 않는다.

### `Space` 잔여 오염 강조

- **잔여 오염 강조:** `Space` 입력을 받으면 아직 남아 있는 먼지·광택 오버레이만 1.2초 동안 부드럽게 밝아졌다가 어두워진다. 이미 제거된 부분은 오버레이 자체가 없으므로 반짝이지 않고 진행도도 변하지 않는다.
- **일시정지와 Web 대응:** 강조 시간은 게임 속도와 무관한 실제 시간을 사용하므로 일시정지 상태나 느린 프레임에서도 끝난다.
- **관련 파일:** `Runtime/Highlight/HighlightController.cs`, `Runtime/Surface/SurfaceMaskLayer.cs`
- **확인 방법:** PlayMode 테스트가 진행도 0%부터 강조를 사용할 수 있고 강조 뒤에도 진행도가 그대로인지 검사한다.

### 입력 연결과 장비 회전

- **하는 일:** `StageInputController`가 Unity의 공식 Input System으로 마우스 위치·좌클릭·우클릭·`Space`·숫자키 `1`/`2`/`3`을 읽는다. `StageInteractionController`는 좌클릭을 현재 도구에 맞는 오염에만 보내고, 우클릭은 장비 회전에만 보낸다.
- **사용자에게 보이는 결과:** 에어건은 먼지, 헝겊은 광택에만 반응한다. 우클릭 드래그 중에는 청소가 함께 일어나지 않는다. UI에서 누르기 시작한 클릭은 버튼을 놓을 때까지 장비 뒤쪽을 청소하거나 회전하지 않는다.
- **회전 범위:** `EquipmentRotator`는 기본적으로 위아래 회전값을 `-35`~`55`도 범위에 고정한다. 게임 제작자는 Inspector의 `Min Pitch`, `Max Pitch`, `Sensitivity`로 장비마다 감도와 시야를 조정할 수 있다.
- **청소 대상 구분:** Unity의 8번 레이어 이름은 `Cleanable`이다. 입력 광선은 이 레이어만 검사하므로 책상과 장식물은 클릭해도 청소되지 않는다.
- **겹친 대상 선택:** 광선에 여러 Collider가 맞으면 가장 앞의 물체 하나만 고르지 않고 현재 도구가 처리할 수 있는 표면을 거리순으로 찾는다. 책상이나 장식물은 `Cleanable` 레이어가 아니므로 청소를 가로막지 않는다.
- **보이는 면과 클릭 면 일치:** 실제 마우스는 화면에 표시하는 `mouse_LOD0` 메시를 MeshCollider에도 사용한다. 더 거친 `LOD3` 콜라이더를 쓰면 보이는 폴리곤과 UV 클릭 위치가 달라져 먼지가 엉뚱한 곳에서 지워질 수 있다.
- **포커스 보호:** 브라우저 탭이나 Unity 창이 포커스를 잃으면, 나중에 연결될 도구 루프 사운드에 정지 신호를 보낸다.
- **관련 파일:** `Runtime/Input/StageInputController.cs`, `StageInteractionController.cs`, `EquipmentRotator.cs`, `Tests/EditMode/EquipmentRotatorTests.cs`, `Tests/PlayMode/StageInteractionControllerTests.cs`, `Game/ProjectSettings/TagManager.asset`
- **확인 방법:** Unity의 열린 Editor에서 전체 EditMode·PlayMode 테스트를 실행한다. 최종 검증은 EditMode 26개와 PlayMode 63개이며, 도구별 Raycast 라우팅, 우클릭 회전 우선, UI 시작 클릭 차단과 실제 키보드 `Space` 입력 등을 검사한다.

### 번호형 여섯 씬

- **하는 일:** 최종 게임 흐름에 필요한 여섯 씬을 `Assets/CleanToContinue/Scenes`에 미리 만들고 Build Settings에 01~06 순서로 등록한다.
- **현재 화면:** 여섯 씬은 인트로·메뉴·텍스트 오프닝·세 장비 청소·엔딩까지 최종 플레이 흐름으로 구성되어 있다.
- **공통 구조:** 모든 씬에는 `SceneRoot`, `EnvironmentRoot`, `ContentRoot`, `GameplayRoot`, 카메라, 조명, `UIRoot`와 `EventSystem`이 있다.
- **사용자 작업 보호:** `Clean to Continue > Build Numbered Scene Skeletons` 메뉴를 다시 실행해도 사용자가 추가한 오브젝트를 삭제하지 않는다. 이미 열어 둔 씬도 닫지 않고, 기존 카메라·조명·UI의 위치나 색상 같은 설정도 기본값으로 되돌리지 않는다.
- **SampleScene 처리:** Unity 템플릿의 `Assets/Scenes/SampleScene.unity` 파일은 삭제하지 않고 Build Settings에서만 제외했다.
- **관련 파일:** `Editor/NumberedSceneBuilder.cs`, `Tests/EditMode/NumberedSceneScaffoldTests.cs`, `Assets/CleanToContinue/Scenes/*.unity`
- **확인 방법:** Project 창의 `Assets/CleanToContinue/Scenes`에서 원하는 씬을 더블클릭하거나 현재 열려 있는 `01.MainMenu`의 Game 탭을 확인한다. EditMode 테스트는 실제 게임 씬을 수정하지 않는 Preview Scene과 테스트가 만든 임시 복사 씬에서 중복 생성, 사용자 설정과 열린 상태 보존을 검사한 뒤 임시 복사본만 지운다.

### 스테이지 UI·소리·완료 보상 연결

- **하는 일:** `StageController`가 도구 선택, 두 오염 진행도, 입력 잠금, 소리와 추억 패널을 한 흐름으로 연결한다. 전체 진행도가 90%에 처음 도달하면 입력을 잠그고 남은 오염을 자동 정리하며, 진행 휠을 0.35초 동안 100%로 채운 뒤 완료음과 장비별 추억 패널을 한 번만 연다.
- **오른쪽 UI:** `ProgressWheelView`는 원형 채움과 정수 퍼센트를 같은 값으로 표시하며 89.9%를 완료 전 90%로 반올림하지 않는다. `ToolSelectorView`는 에어건·헝겊 선택, 확대와 연한 금색 테두리, 도구별 진행도와 100% 체크 표시를 담당하고 완료 이후 버튼 입력을 막는다.
- **소리:** `PrototypeAudioFactory`는 44.1kHz 에어건·헝겊 소리, 버튼 클릭음과 세 음의 완료 차임을 코드로 만든다. `CleaningAudioController`는 청소 중 선택된 도구만 들리게 전환하고 입력 해제·포커스 상실·UI 위 누름·완료 시 루프를 멈춘다. 게임 영역에서 누른 채 UI 위로 이동하면 소리만 멈추고, UI 밖으로 돌아오면 유효한 누름의 소리가 재개된다. UI에서 시작한 누름은 밖으로 나가도 버튼을 놓기 전까지 무음이다.
- **설정값:** `ctc.masterVolume`, `ctc.musicVolume`, `ctc.sfxVolume`, `ctc.rotationSensitivity`를 사용하며 기본값은 각각 0.8, 0.7, 1.0, 1.0이다.
- **완료 흐름:** 마우스는 키보드로, 키보드는 헤드셋으로, 헤드셋은 엔딩으로 이동한다. 각 완료 화면에는 다음 단계와 메인 메뉴 선택지가 세로로 배치된다.
- **관련 파일:** `Runtime/Stage/StageController.cs`, `Runtime/UI/`, `Runtime/Audio/`, `Tests/PlayMode/StageControllerTests.cs`
- **확인 방법:** 열린 Unity Editor의 PlayMode 테스트가 89.9%에서는 입력이 열려 있고 90%에서 한 번만 잠기며 추억 패널도 한 번만 열리는지 검사한다. 실제 1920×1080·1366×768 배치는 다음 씬 조립 작업에서 확인한다.

### 최종 메뉴·오프닝·세 장비 씬 조립

- **메인 메뉴:** `01.MainMenu`에는 시작·설정·크레딧 버튼과 설정 패널이 있다. 시작은 `02.Opening`으로 이동하고, 음량·효과음·회전 감도는 기존 세 설정 키에 저장된다.
- **짧은 오프닝:** `02.Opening`은 검은 화면 중앙에 네 문장을 3초 간격으로 누적 표시하고, 완료 또는 건너뛰기 버튼으로 `03.Mouse`를 연다.
- **마우스 스테이지:** `03.Mouse`의 `StageRoot`가 입력·청소·회전·진행도·임시 소리·완료 추억 패널을 한 번에 연결한다. 실제 플레이 물체는 `Assets/ThirdParty/Mouse.prefab` 인스턴스이며 원본 프리팹은 수정하지 않는다.
- **책상 수동 배치:** `Desk Table White`는 사용자가 Unity에서 직접 배치·조정했으므로 빌더가 생성·대체·위치 조정하지 않는다. 사용자가 놓은 책상은 프로젝트 소유 표시 루트 밖에 있어 빌더를 다시 실행해도 유지된다.
- **사용자 작업 보호:** `__CleanToContinueVerticalSlice` 아래의 생성 표시가 있는 자식과 빌더가 붙인 흐름 컴포넌트만 다시 만든다. 같은 이름의 사용자 루트가 있으면 별도 `.Generated` 루트를 사용하고, 번호형 씬의 안내 UI도 아직 기본값일 때만 숨긴다.
- **완성된 후속 씬:** `04.Keyboard`와 `05.Headset`은 같은 `StageRoot`를 재사용하며 각 프리팹 구조에 맞는 청소 레이어를 가진다. `06.Ending`은 이미지·감사 문구·처음 버튼으로 구성된다.
- **관련 파일:** `Editor/VerticalSliceSceneBuilder.cs`, `Editor/FinalMediaSceneBuilder.cs`, `Runtime/Flow/`, `Runtime/UI/MainMenuView.cs`, `Prefabs/StageRoot.prefab`, `Scenes/*.unity`
- **확인 방법:** Unity 메뉴 `Clean to Continue > Build Vertical Slice Scenes`와 `Apply Final Media and UI`로 프로젝트 소유 부분을 다시 만들 수 있다. 최종 상태는 여섯 씬을 순서대로 플레이하거나 PlayMode 테스트로 확인한다.

### 헤드셋처럼 부품이 중첩된 경우의 클릭

- 헤드셋의 `Muffs`는 `EarSide` 안에 들어 있는 자식 오브젝트다. Collider에는 정상적으로 클릭이 닿아도 부모와 자식을 모두 같은 대상으로 인정하면 목록에서 앞선 부모가 클릭을 가져갈 수 있다.
- 현재 입력 코드는 클릭된 Collider와 정확히 같은 GameObject의 청소 레이어를 먼저 선택한다. 정확한 대상이 없는 특수 구조에서만 부모·자식 호환 판정을 사용한다.
- 따라서 `Muffs`를 클릭하면 `EarSide`가 아니라 `Muffs`의 먼지 또는 광택 오버레이만 청소된다.

### 최종 영상·음악·앨범은 어떻게 연결되는가

- `FinalMediaSceneBuilder`는 사용자가 고른 파일을 Unity UI용 이미지로 가져오고, 모서리가 둥근 배경 그림도 한 번 만들어 버튼과 패널에 붙인다.
- Web 게임은 영상 파일을 Unity 장면 안에 통째로 넣는 대신 `StreamingAssets`에서 주소로 읽는다. 영상은 브라우저가 자동 재생을 허용하도록 무음으로 재생하고, 끝나면 같은 화면의 타이틀 메뉴를 켠다.
- 인트로 영상은 무음이며 첫 화면이 시작되자마자 배경음 재생을 요청한다. 브라우저가 자동 재생을 잠시 막으면 인트로 화면이나 메뉴의 첫 클릭에서 같은 곡을 다시 시작하므로, 허용되는 가장 이른 시점부터 엔딩까지 `PersistentMusicPlayer` 하나가 음악을 반복한다.
- Unity 기본 폰트는 Web에서 한글이 사라질 수 있어 OFL 라이선스의 `NotoSansCJKkr-Regular`를 프로젝트에 포함하고 여섯 씬의 모든 UI에 연결했다.
- `PersistentMusicPlayer`는 씬이 바뀌어도 사라지지 않는 음악 재생기다. 메인메뉴로 다시 돌아와 두 번째 재생기가 생기더라도 기존 하나만 유지한다.
- 각 장비 씬의 `EquipmentStageBootstrap`은 자신의 `album1`·`album2`·`album3`을 기억한다. 완료 패널을 열 때 이 이미지를 유지하므로 런타임 초기화가 사진을 지우지 않는다.
- 완료 효과음은 외부 파일이 아니라 `PrototypeAudioFactory`가 짧은 세 음 파형을 만든다. 그래서 모든 스테이지에서 같은 `띠리링`을 쓰면서 별도 효과음 파일 의존성을 늘리지 않는다.
- `UiButtonClickSound`는 화면에 보이는 모든 버튼에 붙는다. 버튼을 누르면 씬과 함께 사라지지 않는 공용 재생기가 짧은 `딸깍` 파형을 재생하므로 다음 씬으로 넘어가는 버튼도 소리가 중간에 잘리지 않는다. 크기는 설정의 효과음 음량을 따른다.
- Unity에서 `AudioSource`가 스피커라면 `AudioListener`는 소리를 듣는 귀다. MainMenu·Opening·Ending에는 이 귀가 없어서 음악 재생 표시만 켜지고 실제 출력은 무음이었다. 이제 모든 씬의 카메라에 리스너가 정확히 하나 있도록 빌더와 테스트가 보장한다.
- Web 브라우저가 첫 자동 재생을 막더라도 실제 버튼 클릭은 허용된 사용자 입력이다. 각 버튼은 클릭음을 내기 직전에 지속 배경음도 다시 시작해 MainMenu와 Ending의 버튼이 오디오 복구 지점 역할을 한다.
- `UiButtonHoverBackground`는 글자나 아이콘이 아니라 버튼의 배경 이미지만 바꾼다. 마우스를 올리면 0.1초 동안 불투명 검정으로 변하고, 벗어나면 각 버튼이 원래 갖고 있던 반투명 회색으로 돌아온다.
- 에어건과 헝겊 원본은 정사각형이므로 `Image.Type.Simple`과 비율 유지를 함께 사용한다. 가로로 긴 버튼 안에서도 160×160 표시 영역에 원본 비율 그대로 나타난다. 조작 방법 패널은 본문 폭에 맞춰 480px로 줄였다.

### 씬 전환 페이드는 어떻게 동작하는가

- `SceneTransitionController`는 씬보다 위에 항상 표시되는 검은 화면 한 장을 유지한다. 버튼이 씬 이동을 요청하면 먼저 검은색으로 덮고, 새 씬을 연 다음 다시 투명하게 만든다.
- 일반 전환은 페이드아웃과 페이드인이 각각 0.35초다. 헤드셋 완료 후 `06.Ending`으로 이동할 때만 각각 0.7초로 두 배 느리게 재생해 마지막 여운을 준다.
- 페이드 중에는 검은 화면이 마우스 입력을 받아 중복 클릭과 여러 씬 로드 요청을 막는다. 게임 속 시간이 멈춰도 전환되도록 실제 시간을 사용한다.
- `SceneFlow`와 완료 패널의 두 버튼이 모두 같은 전환기를 사용하므로 메뉴·오프닝·장비·엔딩 이동의 동작이 일치한다.

### 완료 화면 문구는 어떻게 배치되는가

- 세 장비의 회상 문구는 사진 바로 아래나 버튼 바로 위에 붙지 않도록 두 요소 사이의 여백 중앙에 놓는다. 글자 크기는 44이고 영역 높이는 110이라 WebGL에서도 한글이 잘릴 가능성을 줄였다.
- 엔딩 감사 문구는 별도 패널을 두지 않는다. 흰 글자 주위의 검정 외곽선과 오른쪽 아래 그림자만 사용하므로 밝은 이미지에서도 읽히지만 버튼처럼 보이지 않는다.
- 두 값은 `VerticalSliceSceneBuilder`의 원본과 `FinalMediaSceneBuilder`의 최종 패치에 모두 들어 있어 씬을 다시 생성하거나 최종 에셋을 다시 적용해도 유지된다.

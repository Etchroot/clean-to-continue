# Final Media and UI Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 사용자 제공 영상·이미지·음악을 최종 씬 흐름에 연결하고 모든 사각 UI를 명확한 둥근 스타일로 마무리한다.

**Architecture:** `FinalMediaSceneBuilder`가 미디어 import, Web용 StreamingAssets 영상, 프로젝트 소유 둥근 Sprite와 01·02·06 화면을 만들고 03~05의 기존 Stage UI만 선택적으로 보정한다. 런타임은 `IntroVideoController`, `PersistentMusicPlayer`, 누적형 `OpeningSequence`, `EndingView`로 나누며 기존 장비·Desk·Wall Transform과 청소 코드는 변경하지 않는다.

**Tech Stack:** Unity 6.3, uGUI, Unity VideoPlayer, StreamingAssets, AudioSource, NUnit EditMode/PlayMode, Unity MCP

**Spec:** `docs/superpowers/specs/2026-08-22-final-media-ui-design.md`

## Global Constraints

- 최종 결과는 Unity Web 빌드에서 실행되어야 한다.
- `Assets/ThirdParty` 원본 파일과 사용자 Desk·Wall·장비 Transform을 수정하지 않는다.
- 영상은 Web 호환 `VideoPlayer.url`과 `Application.streamingAssetsPath`를 사용한다.
- 메뉴 영상은 앱 실행 세션당 한 번만 재생하고 메뉴 재방문 시 반복하지 않는다.
- BGM은 영상 종료 후 시작해 씬 전환 동안 반복 재생한다.
- 모든 새 동작은 실패 테스트를 먼저 확인한 뒤 구현한다.
- 전체 씬 재생성 대신 프로젝트 소유 UI만 선택적으로 갱신한다.

---

### Task 1: 최종 미디어 import와 둥근 UI 자산

**Files:**
- Create: `Game/Assets/CleanToContinue/Editor/FinalMediaSceneBuilder.cs`
- Create: `Game/Assets/CleanToContinue/Tests/EditMode/FinalMediaAssetTests.cs`
- Generate: `Game/Assets/CleanToContinue/Sprites/Generated/RoundedRect.png`
- Generate: `Game/Assets/StreamingAssets/intro video.mp4`

**Interfaces:**
- Consumes: `Assets/ThirdParty/intro video.mp4`, `intro img.png`, `end img.png`, `airgun.png`, `rag.png`, `album1.png`~`album3.png`, `sunshine desk.mp3`
- Produces: `FinalMediaSceneBuilder.ApplyFinalMediaAndUi()`, UI Sprite imports, Web 영상 파일, 9-slice 둥근 Sprite

- [ ] **Step 1: Write the failing EditMode asset tests**

검사는 PNG 일곱 개가 `SpriteImportMode.Single`, mipmap off인지, `RoundedRect` Sprite border가 네 방향 모두 16 이상인지, StreamingAssets MP4가 원본과 같은 길이인지 확인한다.

- [ ] **Step 2: Run EditMode tests and verify RED**

Run: Unity menu `Tools/Clean to Continue/Run EditMode Tests`
Expected: 새 테스트가 Sprite import, 둥근 Sprite와 StreamingAssets 누락으로 실패한다.

- [ ] **Step 3: Implement the editor asset preparation**

`FinalMediaSceneBuilder`에 정확한 상수 경로를 두고 TextureImporter를 UI Sprite로 변경한다. 64×64 알파 둥근 PNG를 생성하고 `spriteBorder=(20,20,20,20)`로 설정한다. MP4는 원본과 바이트가 다를 때만 StreamingAssets에 복사한다.

- [ ] **Step 4: Re-run EditMode tests and verify GREEN**

Expected: 모든 미디어와 생성 자산 검사가 통과한다.

### Task 2: 첫 실행 영상, 메뉴 배경과 전역 BGM

**Files:**
- Create: `Game/Assets/CleanToContinue/Runtime/Flow/IntroVideoController.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/Audio/PersistentMusicPlayer.cs`
- Modify: `Game/Assets/CleanToContinue/Runtime/UI/MainMenuView.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/IntroVideoControllerTests.cs`
- Create: `Game/Assets/CleanToContinue/Tests/PlayMode/PersistentMusicPlayerTests.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`
- Modify: `Game/Assets/CleanToContinue/Editor/FinalMediaSceneBuilder.cs`

**Interfaces:**
- `IntroVideoController.Configure(VideoPlayer player, RawImage screen, Button retrySurface, GameObject menuRoot, PersistentMusicPlayer music, string fileName, float timeoutSeconds)`
- `PersistentMusicPlayer.Configure(AudioClip clip)`
- `PersistentMusicPlayer.SetMusicVolume(float value)`
- `MainMenuView.Configure(...)` gains `Slider music`
- New preference: `StageController.MusicVolumeKey = "ctc.musicVolume"`, default `0.7f`

- [ ] **Step 1: Write failing PlayMode tests**

실제 GameObject·VideoPlayer·AudioSource를 사용해 메뉴 공개가 세션당 한 번인지, 영상 완료 대체가 BGM을 시작하는지, 중복 MusicPlayer가 두 곡을 재생하지 않는지, 음악 Slider가 재생 중 AudioSource volume을 즉시 바꾸는지 검사한다.

- [ ] **Step 2: Run PlayMode tests and verify RED**

Expected: 새 타입과 음악 설정 키가 없어 컴파일 또는 동작 테스트가 실패한다.

- [ ] **Step 3: Implement minimal runtime controllers**

`IntroVideoController`는 prepare·loopPoint·error·timeout·화면 클릭을 하나의 `RevealMenuAndStartMusic()` 경로로 모으고 정적 세션 플래그로 재방문을 건너뛴다. `PersistentMusicPlayer`는 `DontDestroyOnLoad`, singleton 중복 제거, loop·2D AudioSource와 즉시 볼륨 반영을 담당한다.

- [ ] **Step 4: Build the final MainMenu UI**

`intro img`를 AspectRatioFitter `EnvelopeParent`로 배치하고 하단 중앙에 반투명 회색·흰 글자 `시작/설정/크레딧` 버튼을 둔다. 설정은 전체·배경음·효과음·회전 감도 네 Slider를 갖고 크레딧은 승인된 일곱 줄을 정확히 표시한다.

- [ ] **Step 5: Run PlayMode tests and verify GREEN**

Expected: 영상·메뉴·음악·설정 테스트가 통과한다.

### Task 3: 누적 오프닝과 엔딩

**Files:**
- Modify: `Game/Assets/CleanToContinue/Runtime/Flow/OpeningSequence.cs`
- Create: `Game/Assets/CleanToContinue/Runtime/UI/EndingView.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`
- Modify: `Game/Assets/CleanToContinue/Editor/FinalMediaSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Scenes/02.Opening.unity`
- Modify: `Game/Assets/CleanToContinue/Scenes/06.Ending.unity`

**Interfaces:**
- `OpeningSequence.Configure(Button skip, Text line, string[] sentences, float interval, string destination)`
- `EndingView.Configure(Button restart, string destination = "01.MainMenu")`

- [ ] **Step 1: Write failing PlayMode scene tests**

Opening에 3D Mouse가 없고 검은 배경·중앙 Text·4문장이 3초 간격으로 누적되는지 검사한다. Ending은 `end img`, 감사 문구, 재시작 버튼과 정확한 목적지를 검사한다.

- [ ] **Step 2: Run PlayMode tests and verify RED**

Expected: 기존 OpeningMouse와 단일 문장, 미구현 Ending 때문에 실패한다.

- [ ] **Step 3: Implement cumulative OpeningSequence and EndingView**

첫 문장을 즉시 쓰고 `WaitForSecondsRealtime(3)`마다 `\n`으로 다음 문장을 누적한다. 마지막 문장 뒤 3초 후 Mouse로 이동한다. Ending 버튼은 MainMenu를 한 번 로드한다.

- [ ] **Step 4: Build 02 and 06 project-owned UI only**

02는 검은 배경, 중앙 누적 Text와 둥근 Skip만 만든다. 06은 `end img` Aspect fill, 중앙 감사 문구와 둥근 재시작 버튼을 만든다.

- [ ] **Step 5: Run PlayMode tests and verify GREEN**

Expected: 오프닝과 엔딩 씬 테스트가 통과한다.

### Task 4: 도구 이미지, 앨범 이미지, 둥근 Stage UI와 완료 벨소리

**Files:**
- Modify: `Game/Assets/CleanToContinue/Editor/FinalMediaSceneBuilder.cs`
- Modify: `Game/Assets/CleanToContinue/Runtime/Audio/PrototypeAudioFactory.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/VerticalSliceSceneTests.cs`
- Modify: `Game/Assets/CleanToContinue/Tests/PlayMode/StageControllerTests.cs`
- Modify: `Game/Assets/CleanToContinue/Scenes/03.Mouse.unity`
- Modify: `Game/Assets/CleanToContinue/Scenes/04.Keyboard.unity`
- Modify: `Game/Assets/CleanToContinue/Scenes/05.Headset.unity`

**Interfaces:**
- Existing `MemoryPanelView.Configure(...)` receives stage-specific Sprite through the bootstrap configuration.
- Existing `CleaningAudioController.PlayCompletion()` remains the single completion entry point.
- `PrototypeAudioFactory.Create()` returns a roughly 0.7-second three-onset completion clip.

- [ ] **Step 1: Write failing visual and audio tests**

세 Stage 씬의 도구 슬롯 Sprite가 각각 `airgun`·`rag`, MemoryImage가 `album1`·`album2`·`album3`인지 검사한다. 패널·버튼은 RoundedRect sliced Sprite를 사용해야 한다. 완료 Clip은 0.6~0.9초이며 세 시간 구간에 독립적인 에너지 onset이 있어야 한다.

- [ ] **Step 2: Run tests and verify RED**

Expected: 자리표시자 아이콘·Memory와 기존 짧은 두 음 완료음 때문에 실패한다.

- [ ] **Step 3: Patch only existing Stage UI**

03~05 씬에서 장비·환경은 읽기만 하고 `StageCanvas` 아래 Image Sprite, 도구 이미지 슬롯, MemoryImage와 Bootstrap의 Sprite 참조만 수정한다. Placeholder 자식만 제거한다.

- [ ] **Step 4: Implement the three-note completion bell**

약 0.7초 동안 0.00·0.18·0.38초에 상승하는 세 음과 지수 감쇠·약한 배음을 합성한다. 기존 `StageController`의 1회 완료 가드와 효과음 볼륨 경로를 그대로 사용한다.

- [ ] **Step 5: Run EditMode and PlayMode tests and verify GREEN**

Expected: 세 Stage 시각 자산, 둥근 UI와 완료음 테스트가 통과한다.

### Task 5: 통합 적용, 문서와 Web 검증

**Files:**
- Modify: `docs/GAME_DESIGN.md`
- Modify: `docs/NONTECHNICAL_GUIDE.md`
- Modify: `docs/HUMAN_IN_THE_LOOP.md`
- Modify: `docs/DEVELOPMENT_LOG.md`
- Modify: `submission/ASSET_CREDITS.md`
- Modify: `submission/CODEX_COLLABORATION.md`

**Interfaces:**
- Unity menu: `Clean to Continue/Apply Final Media and UI`

- [ ] **Step 1: Execute the targeted final media menu**

메뉴 실행 전후 03~05의 Desk, Wall, 플레이 장비 Transform을 기록해 동일함을 확인한다. 01·02·06 프로젝트 소유 화면과 03~05 UI만 저장한다.

- [ ] **Step 2: Run full automated verification**

Run EditMode and PlayMode JSON menus. Expected: failures 0, skipped 0. Run `git diff --check` and inspect Unity Console for current game-code errors.

- [ ] **Step 3: Run a Web build**

Web 빌드를 생성해 StreamingAssets MP4가 출력에 포함되는지 확인한다. 로컬 HTTP 서버에서 Chrome·Edge의 영상 URL, 첫 클릭 대체, 음악 반복, 볼륨과 씬 이동을 검사한다.

- [ ] **Step 4: Update project and submission records**

최종 에셋 제작자·도구, 사용자 승인 내용, Codex 구현·테스트 증거와 비전공자용 영상/BGM/UI 구조 설명을 기록한다.

- [ ] **Step 5: User visual playtest checkpoint**

사용자가 01→02→03→04→05→06을 직접 플레이해 이미지 crop, 버튼 위치, 오프닝 속도, 음악 크기와 완료 `띠리링` 체감을 확인한다.

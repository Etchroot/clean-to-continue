# Final Media and UI Design

## 목표

`Clean to Continue`의 첫 실행 영상, 메인 메뉴, 배경음, 오프닝 독백, 장비별 도구·추억 이미지와 엔딩을 사용자가 준비한 최종 미디어로 교체한다. 모든 UI 패널과 버튼은 눈에 띄는 둥근 모서리로 통일하고, Unity Web 빌드에서도 영상·음악·입력과 씬 전환이 동작해야 한다.

## 사용자 제공 에셋

| 용도 | 경로 | 확인 정보 |
|---|---|---|
| 첫 실행 영상 | `Assets/ThirdParty/intro video.mp4` | 1280×720, 10초 |
| 메뉴 배경 | `Assets/ThirdParty/intro img.png` | 2048×1024 |
| 엔딩 배경 | `Assets/ThirdParty/end img.png` | 2048×1024 |
| 에어건 아이콘 | `Assets/ThirdParty/airgun.png` | 2048×2048 |
| 헝겊 아이콘 | `Assets/ThirdParty/rag.png` | 2048×2048 |
| 마우스 추억 | `Assets/ThirdParty/album1.png` | 2048×1024 |
| 키보드 추억 | `Assets/ThirdParty/album2.png` | 2048×1024 |
| 헤드셋 추억 | `Assets/ThirdParty/album3.png` | 2048×1024 |
| 배경음 | `Assets/ThirdParty/sunshine desk.mp3` | 138.144초, 스테레오 |

PNG 파일은 UI용 단일 Sprite로 가져오고 mipmap을 끈다. 원본 파일은 `ThirdParty`에 유지한다.

## 전체 흐름

1. 앱이 `01.MainMenu`에서 시작한다.
2. 앱 실행 세션에서 처음 로드된 메뉴라면 `intro video`를 전체 화면으로 한 번 재생한다.
3. 영상이 끝나거나 재생 오류·준비 제한 시간을 만나면 영상 화면을 닫고 `intro img`와 메뉴 버튼을 표시한다.
4. 영상 종료 시 `Sunshine desk`를 시작하고 이후 `02.Opening` → 세 장비 → `06.Ending` 동안 반복 재생한다.
5. 엔딩의 `처음으로 돌아가기`는 `01.MainMenu`를 연다. 같은 앱 실행 세션에서는 영상을 다시 재생하지 않고 메뉴와 배경음을 유지한다.
6. 브라우저를 새로고침하거나 앱을 다시 실행하면 새로운 세션으로 보고 영상을 다시 재생한다. 이 상태는 `PlayerPrefs`가 아니라 런타임 정적 상태로 관리한다.

## Web 영상 재생

Web 플랫폼은 내장 `VideoClip`을 지원하지 않으므로 `VideoPlayer.url`을 사용한다. 빌드에 포함되는 `Assets/StreamingAssets/intro video.mp4`를 `Application.streamingAssetsPath`와 결합해 재생한다. Editor에서도 같은 URL 경로를 사용해 Web과 동작 경로를 맞춘다.

영상은 `RenderTexture`에 출력하고 전체 화면 `RawImage`에 표시한다. 준비 완료 뒤 재생하며 `loopPointReached`에서 메뉴를 연다. `errorReceived` 또는 준비 제한 시간 초과 시에도 메뉴로 안전하게 넘어간다. 브라우저 자동재생 정책 때문에 재생이 시작되지 않으면 영상 화면 전체가 클릭 입력을 받아 같은 영상을 재생하도록 한다.

## 메인 메뉴

영상이 끝난 뒤 `intro img`를 Canvas 전체에 표시한다. 이미지는 원본 비율을 유지하면서 화면을 채우는 `EnvelopeParent` 방식으로 배치하므로 16:9 화면에서는 좌우가 조금 잘릴 수 있고 찌그러지지 않는다. 기존 코드 생성 제목과 부제는 제거한다.

`시작`, `설정`, `크레딧` 버튼은 화면 하단 중앙에 세로로 배치한다. 버튼 스타일은 다음과 같다.

- 명확한 둥근 모서리
- 반투명 회색 배경
- 흰색 굵은 글자
- 선택·눌림 상태에서도 동일 계열의 명도 변화
- 버튼과 글자는 1366×768에서도 잘리지 않는다.

설정·크레딧 패널과 닫기 버튼에도 같은 둥근 사각형 스타일을 적용한다.

## 설정과 전역 배경음

설정 항목은 `전체 음량`, `배경음`, `효과음`, `회전 감도` 네 개다. 새 배경음 키는 `ctc.musicVolume`, 기본값은 `0.7`이다.

`PersistentMusicPlayer`는 `DontDestroyOnLoad`로 씬 전환 중 유지하고 중복 인스턴스를 제거한다. 영상 종료 뒤 한 번만 음악을 시작하고 `loop=true`, `spatialBlend=0`으로 재생한다. 실제 청취 음량은 `AudioListener.volume`의 전체 음량과 음악 AudioSource의 배경음 값이 함께 결정한다. 메뉴의 배경음 슬라이더를 움직이면 재생 중인 음악에도 즉시 반영한다.

## 크레딧

크레딧 내용은 아래 텍스트와 철자를 그대로 사용한다.

```text
Creator : 차명근
AI Agent : Codex
Engine : Unity
Asset : Unity Asset Store
Title Video & Image : Nanobanana
Album Image : GPT
Sound : Suno
```

## 오프닝

`02.Opening`에서는 3D 마우스, 환경 조명과 장비 카메라 연출을 제거한다. 검은 화면 중앙의 흰색 텍스트 한 개에 다음 문장을 누적한다.

```text
얼마만에 생긴 휴식시간인지 모르겠다.
옛날에는 게임을 정말 재밌게 했었는데.
오랜만에 게임이나 해볼까?
그 전에 장비에 쌓인 먼지부터 닦아야겠는걸.
```

첫 문장은 씬 시작과 함께 보이고 이후 3초마다 다음 줄이 추가된다. 앞 문장은 지우지 않는다. 마지막 문장이 표시된 뒤 3초 후 `03.Mouse`로 이동한다. `건너뛰기` 버튼은 유지하고 둥근 스타일을 사용한다.

## 인게임 UI

`03.Mouse`, `04.Keyboard`, `05.Headset`의 진행도 패널, 도구 패널, 조작 안내, 도구 버튼, 완료 패널 버튼은 모두 프로젝트 소유 9-slice 둥근 사각형 Sprite를 사용한다. Unity 기본 Background Sprite보다 큰 반경을 사용해 모서리가 육안으로 명확해야 한다.

도구 버튼의 기존 코드 도형 아이콘은 제거하고 다음 Sprite를 이미지 슬롯에 넣는다.

- 에어건: `airgun.png`
- 헝겊: `rag.png`

아이콘은 비율을 유지하며 이미지 슬롯 안에 맞춘다. 선택 테두리, 진행도 채움, 고정 텍스트와 체크 표시는 기존 동작을 유지한다.

## 추억 보상

장비별 완료 패널의 중앙 이미지는 다음과 같이 고정한다.

- `03.Mouse`: `album1.png`
- `04.Keyboard`: `album2.png`
- `05.Headset`: `album3.png`

이미지는 비율을 유지해 기존 820×390 영역 안에 맞추고, `MEMORY` 자리표시자는 제거한다. 기존 회상 대사와 `다음 단계 진행`·`메인 메뉴로 돌아가기` 버튼은 유지한다. 헤드셋의 다음 단계는 `06.Ending`이다.

## 엔딩

`06.Ending`에는 `end img`를 비율 유지 전체 화면 배경으로 표시한다. 화면 중앙에는 큰 흰색 굵은 글자로 `플레이 해주셔서 감사합니다`를 표시한다. 그 아래에는 둥근 반투명 회색 `처음으로 돌아가기` 버튼을 두고 `01.MainMenu`를 로드한다.

## 둥근 사각형 구현

프로젝트 소유 Editor 유틸리티가 투명 모서리를 가진 64×64 흰색 둥근 사각형 PNG와 Sprite border를 생성한다. 모든 패널·버튼 Image는 이 Sprite를 `Image.Type.Sliced`로 사용한다. 원형 진행 휠, 전체 화면 배경, 실제 그림과 아이콘에는 둥근 Sprite를 적용하지 않는다.

## 씬과 사용자 작업 보호

기존 선택적 씬 빌더가 소유하는 `__CleanToContinueVerticalSlice` 영역만 갱신한다. 사용자가 배치한 Desk, Wall, 장비 Transform과 ThirdParty 원본은 수정하지 않는다. `01`, `02`, `06`의 프로젝트 소유 UI와 `03`~`05`의 프로젝트 소유 Stage UI만 다시 만든다.

## 오류 처리

- 필수 Sprite나 AudioClip을 찾지 못하면 빌더가 해당 정확한 경로를 포함한 오류를 발생시켜 잘못된 씬을 저장하지 않는다.
- 영상 재생 실패는 게임 진입을 막지 않고 메뉴로 대체한다.
- 전역 음악 인스턴스가 이미 있으면 새 인스턴스를 제거해 중복 재생을 막는다.
- 씬 이동 버튼은 기존 중복 전환 방지 규칙을 유지한다.

## 테스트와 검수

- EditMode: 필수 미디어 존재, Sprite import 설정, StreamingAssets 영상 존재와 둥근 Sprite의 border를 검사한다.
- PlayMode: 영상 완료·오류 대체 뒤 메뉴 노출, 영상 세션 1회, 음악 중복 방지와 볼륨 즉시 반영, 오프닝 네 줄 누적, 엔딩 메뉴 이동을 검사한다.
- 씬 스모크 테스트: 메뉴 배경·세 버튼·네 설정값·정확한 크레딧, 세 도구 이미지, 세 앨범 이미지와 엔딩 배경·문구·버튼을 검사한다.
- 실제 Editor: 01→02→03→04→05→06→01 흐름, 음악 연속성, 둥근 모서리와 1920×1080·1366×768 레이아웃을 확인한다.
- 실제 Web: Chrome과 Edge에서 MP4 URL, 첫 클릭 대체, BGM 시작·반복·볼륨, 새로고침 후 영상 재실행을 확인한다.

## 범위 밖

- 영상 편집·재인코딩
- 음악 페이드·크로스페이드
- 별도 로딩 화면
- 설정 패널 디자인 전면 재구성
- 새로운 게임플레이 기능

# 에셋 및 생성물 기록

Unity Asset Store 에셋은 완성된 게임에 내장할 수 있지만 원본을 공개 저장소에 재배포하지 않는다. 사용 전 개별 상품의 EULA와 Restricted Asset 여부를 확인한다.

## 3D·음원·폰트

| 상태 | 이름 | 종류 | 제작자·출처 | 라이선스 | 로컬 경로 | 수정 | 비고 |
|---|---|---|---|---|---|---|---|
| 통합 | Mouse | 3D 마우스 | 상품 링크·제작자 확인 필요 | Unity Asset Store 이용 조건 확인 필요 | `Game/Assets/ThirdParty/Mouse.prefab` | 원본 수정 없음, 씬 인스턴스에 프로젝트 전용 청소 재질·컴포넌트 적용 | `03.Mouse`의 실제 플레이 대상과 `02.Opening` 연출에 사용. 원본 의존성: `ThirdParty/Source/JustPlay` |
| 통합 | Keyboard | 3D 키보드 | 상품 링크·제작자 확인 필요 | Unity Asset Store 이용 조건 확인 필요 | `Game/Assets/ThirdParty/Keyboard.prefab` | 원본 수정 없음, 씬 인스턴스에 프로젝트 전용 청소 레이어 적용 | `04.Keyboard` 플레이 대상. 원본 의존성: `ThirdParty/Source/KeyboardAdjustableColors` |
| 통합 | Headset Type1 | 3D 헤드셋 | 상품 링크·제작자 확인 필요 | Unity Asset Store 이용 조건 확인 필요 | `Game/Assets/ThirdParty/Headset Type1.prefab` | 원본 수정 없음, 읽기 가능한 런타임 오버레이 복사본 생성 | `05.Headset` 플레이 대상. 원본 의존성: `ThirdParty/Source/Make Your Gadget assets` |
| 통합 | Desk Table White | 3D 책상 | 상품 링크·제작자 확인 필요 | Unity Asset Store 이용 조건 확인 필요 | `Game/Assets/ThirdParty/Desk Table White.prefab` | 사용자가 씬별 위치·크기 조정 | 세 장비 씬의 책상으로 사용. 원본 의존성: `ThirdParty/Source/Models` |

## AI 생성 이미지

| 상태 | 장면 | 생성 도구 | 생성일 | 사용자 승인 | 수정 내용 | 최종 파일 |
|---|---|---|---|---|---|---|
| 통합 | 어린 시절 오락기 | GPT | 2026-08-22 이전 | 승인 | `03.Mouse` 완료 이미지로 크기 맞춤 | `Game/Assets/ThirdParty/album1.png` |
| 통합 | 학생 시절 친구와 게임 | GPT | 2026-08-22 이전 | 승인 | `04.Keyboard` 완료 이미지로 크기 맞춤 | `Game/Assets/ThirdParty/album2.png` |
| 통합 | 친구들과 PC방 환호 | GPT | 2026-08-22 이전 | 승인 | `05.Headset` 완료 이미지로 크기 맞춤 | `Game/Assets/ThirdParty/album3.png` |
| 통합 | 타이틀 이미지 | Nanobanana | 2026-08-22 이전 | 승인 | 메인메뉴 전체 배경 | `Game/Assets/ThirdParty/intro img.png` |
| 통합 | 엔딩 이미지 | 사용자 제공 | 2026-08-22 이전 | 승인 | 엔딩 전체 배경 | `Game/Assets/ThirdParty/end img.png` |

## 영상·음원·UI 이미지

| 상태 | 이름 | 제작 도구·출처 | 로컬 경로 | 용도 |
|---|---|---|---|---|
| 통합 | intro video | Nanobanana | `Game/Assets/ThirdParty/intro video.mp4` | 게임 시작 10초 타이틀 영상 |
| 통합 | sunshine desk | Suno | `Game/Assets/ThirdParty/sunshine desk.mp3` | 인트로 종료 뒤 엔딩까지 반복 BGM |
| 통합 | airgun / rag | 사용자 제공 | `Game/Assets/ThirdParty/airgun.png`, `rag.png` | 인게임 도구 선택 아이콘 |
| 통합 | 복원 완료 차임 | Codex 코드 생성 | `Runtime/Audio/PrototypeAudioFactory.cs` | 세 스테이지 완료 시 상승하는 세 음 효과음 |
| 통합 | Noto Sans CJK KR Regular | notofonts / Google, SIL Open Font License 1.1 | `Game/Assets/CleanToContinue/Fonts/NotoSansCJKkr-Regular.otf` | Web 빌드의 모든 한글 UI |

## 공개 저장소 규칙

- Asset Store 원본은 `Game/Assets/ThirdParty/`에 보관하고 `.gitignore`로 제외한다.
- README나 이 문서에는 상품 링크와 재설치 방법만 기록한다.
- AI 생성물은 이용 조건을 확인하고 생성 도구와 날짜를 남긴다.
- 직접 만든 코드와 문서에만 저장소의 MIT License를 적용한다.
- `CleanToContinue/Meshes/Generated`와 `Materials/Generated`의 Asset Store 파생 런타임 복사본도 공개 Git에서 제외하고 로컬 빌더로 다시 만든다.

## 제출 전 확인 필요

- Mouse, Keyboard, Headset Type1과 Desk Table White의 Asset Store 상품 URL·제작자명을 원 구매 기록에서 확인한다.
- 각 상품이 Restricted Asset이 아니며 완성된 게임 빌드 내 사용이 허용되는지 확인한다.
- 공개 GitHub에는 위 원본과 파생 메시·재질이 포함되지 않았는지 최종 검사한다.

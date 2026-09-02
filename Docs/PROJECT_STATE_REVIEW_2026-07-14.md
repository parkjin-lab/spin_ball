# 프로젝트 상태 리뷰 - 2026-07-14

## 게임 목표

**90초 안에 최적 파괴 경로를 읽고 도시를 무너뜨리며 DP로 다음 침공을 강화하는 스테이지형 파괴 로그라이트.**

## 플레이어 판타지와 목표

- **플레이어 판타지:** 거대한 침공체가 도시의 약점을 읽고, 한 번 열린 파괴 경로를 폭발적인 연쇄 붕괴로 바꾸는 지휘자이자 재난 그 자체가 된다.
- **30초 목표:** 첫 파괴 레인을 판독하고 `LANE BREAK`를 만든 뒤 다음 표적까지의 경로와 즉시 보상을 선택한다.
- **5분 목표:** 여러 스테이지에서 형태·업그레이드·DP 운용을 조정하며 더 빠르고 선명한 파괴 루트를 완성한다.
- **장기 목표:** DP로 형태와 메타 업그레이드를 해금해 다음 침공의 빌드 폭과 파괴 효율을 확장하고 더 높은 스테이지를 정복한다.

## 현재 게임 루프

`Stage Start -> LANE BREAK -> ROUTE OPEN -> ROUTE HOLD -> ROUTE BONUS / Forward Smash -> 결과 / 성장`

런타임 루프와 UI·텔레메트리의 중심 구현은 `Assets/Scripts/Runtime/Systems/DummyFlowController.ProgressionCore.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.PlaytestTelemetry.cs`에 있다. 설계 의도는 `Docs/GDD_ALIEN_CRUSHER.md`, `Docs/GAME_UPDATE_ROADMAP.md`, `Docs/GAME_DESIGN_GAP_POLICY.md`에 기록되어 있다.

## 핵심 목표 충돌과 결정 필요

설계 중심은 **경로를 읽고 숙련하는 플레이**이지만, 실제 승리 판정은 파괴 수와 보스 처치 중심이며 `ROUTE HOLD`는 선택 보너스에 가깝다. 따라서 플레이어가 최적 경로를 무시하고도 정답 플레이를 할 수 있다.

사람이 다음 중 하나를 명시적으로 결정해야 한다.

1. **필수 조건:** `ROUTE HOLD` 또는 경로 달성을 스테이지 클리어의 필수 조건으로 만든다.
2. **고득점 선택지:** 현재 클리어 조건은 유지하되 경로 숙련이 점수·DP·등급·다음 스테이지 이점에서 압도적으로 유리하도록 만든다.

결정 전에는 두 방향을 섞는 대규모 밸런싱을 하지 않는다. 관련 근거는 `Assets/Scripts/Runtime/Systems/GameFlowSystem.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.ProgressionCore.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageEncounter.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.StageFlow.cs`다.

## 재미 판정

**현재 판정: 미검증.** 실제 플레이로 얻은 `F10` Stage 1-7 증거가 없으므로 재미있다고 판정할 수 없다. 경로 판독과 연쇄 파괴의 잠재력은 있으나, 순간 의사결정의 차이와 압박-해방 리듬이 평평해질 위험이 있다.

`Tools/RunStaticAudits.ps1` 통과, `Logs/AlienCrusherSceneValidation.log`, `Logs/AlienCrusherMapLayoutAudit.log` 같은 정적·배치 감사 결과는 배선과 회귀 위험에 대한 증거일 뿐 **재미의 증거가 아니다**. 실제 증거 정본은 앞으로 생성할 `Logs/AlienCrusherPlaytestTelemetry.log`, `Logs/AlienCrusherPlaytestTelemetrySummary.md`, `Docs/AlienCrusherStagePlaytestNotes.md`다.

## 코드 위험 우선순위

| 우선순위 | 위험 | 영향과 근거 |
|---|---|---|
| 완료/검증 대기 | 전역 시간 소유권 경쟁 | pause/overdrive/boss 채널을 DummyFlowController.TimeControl.cs 한 곳에서 합성하도록 통합했다. 실제 중첩 프레임 검증은 Unity ILPP 정상화 후 수행한다. Tools/AuditTimeScaleOwnershipStatic.ps1 |
| 완료/검증 대기 | 구매 원자성 | DP 차감과 해금/업그레이드 지급을 TryCommit 한 번으로 묶고 저장 실패 시 메모리 스냅샷을 복원한다. 실패 주입 배치 검증기가 추가됐으며 Unity 실행이 남았다. Assets/Scripts/Runtime/Systems/FormUnlockSystem.cs |
| 완료/검증 대기 | 저장 원자 교체·백업 복구 | temp flush와 JSON 검증 뒤 atomic replace하고, 손상 primary 복구 시 정상 backup을 보존한다. 정상 커밋·실패 롤백·backup 복구 배치 검증이 연결됐다. Assets/Scripts/Editor/ProgressionSaveTransactionValidator.cs |
| P2 | 이름 기반 비결정적 씬 참조 | `GameObject.Find`, 전역 `FindFirstObjectByType`/`FindObjectsByType` 의존은 중복 오브젝트와 씬 변경에서 참조 결과를 불안정하게 만든다. `Assets/Scripts/Runtime/Systems/FormUnlockSystem.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.Lifecycle.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeUtilities.cs` |
| P2 | 문자열 기반 정적 감사와 Unity PlayMode 테스트 부재 | PowerShell 감사가 소스 문자열·리포트 형식 회귀는 잡지만 실제 프레임 순서, 코루틴 중첩, 저장 중단을 검증하지 못한다. `Tools/RunStaticAudits.ps1`, `Tools/AuditRouteHoldTuningStatic.ps1`, `Tools/TestPlaytestEvidenceGateRegression.ps1`, `Assets/Scripts` |
| P2 | 입력 정책 분산 및 게임패드/리바인딩 부재 | 입력이 여러 런타임 파일과 가상 조이스틱 처리로 흩어져 장치별 동작 일관성과 접근성이 낮다. `Assets/Scripts/Runtime/Gameplay/PlayerBallDummyController.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`, `Assets/Scripts/Runtime/UI/VirtualJoystickUI.cs` |
| P3 | 맵 생성 반복 할당·전역 탐색 | 스테이지 재구축과 효과 처리에서 배열 할당 및 전체 오브젝트 탐색이 반복되어 모바일 프레임 스파이크 위험이 있다. `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeUtilities.cs`, `Assets/Scripts/Runtime/Gameplay/DummyStreetPropReactive.cs` |

## 상태 분류

### 완료

- 현재 코어 루프의 런타임·HUD·텔레메트리 골격과 Stage 1-7 맵 감사 도구가 존재한다. `Assets/Scripts/Runtime/Systems/DummyFlowController.PlaytestTelemetry.cs`, `Tools/GeneratePlaytestTelemetrySummary.ps1`, `Tools/AuditRuntimeMapLayoutStatic.ps1`
- 자동화 연속성, 체크리스트 생성, 정적 감사 체인은 구축 완료 상태이며 이제 **유지보수 항목**이다. `Docs/AUTOMATION_RUNBOOK.md`, `Tools/RunPlaytestReadinessPrep.ps1`, `Logs/AlienCrusherAutomationStatusSummary.md`

### 진행

- 경로 가독성, Stage 4 보스 접근 랜드마크, 모바일 HUD 축약, 오디오 훅은 구현됐으나 실제 장치·플레이 증거가 없다. `Assets/Scripts/Runtime/Systems/DummyFlowController.RuntimeMapFallback.cs`, `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`, `Assets/Scripts/Runtime/Systems/FeedbackSystem.cs`
- 저장 실패 주입 검증기는 구현·배치 연결됐으나 Unity IL Post Processor 정체로 아직 실행되지 않았다. 실제 종료·재진입 save smoke도 남아 있다. Assets/Scripts/Editor/ProgressionSaveTransactionValidator.cs, Tools/TestPlaytestEvidenceGate.ps1

### 차단

- 재미·리듬·스테이지별 튜닝은 실제 `F10` Stage 1-7 sweep, 28개 관찰 노트, progression save smoke가 없어서 차단이다. `Docs/GAME_DESIGN_GAP_POLICY.md`, `Docs/AlienCrusherStagePlaytestNotes.md`
- Evidence Green과 Decision Green은 실제 텔레메트리 및 사람의 판정이 없어 차단이다. `Tools/TestPlaytestEvidenceGate.ps1`, `Logs/AlienCrusherPlaytestTelemetrySummary.md`

### 사람 개입

- `ROUTE HOLD`를 필수 승리 조건으로 할지 고득점 선택지로 둘지 결정.
- Unity Editor에서 현재 날짜 기준 Runtime Green 확인, 실제 `F10` sweep 수행, Stage 1-7 각각 4개 관찰인 총 28 notes 작성, 저장 종료·재진입 smoke 확인.
- 화면·소리·손맛을 직접 보고 Evidence/Decision Green 승인.

### 자율 가능

- P1 시간 소유권 통합, 저장·구매 원자성 강화와 단위/PlayMode 테스트 추가.
- 이름 기반 참조 축소, 입력 정책 인벤토리와 리팩터링 설계, 맵 생성 프로파일링 포인트 추가.
- 실측 결과가 생긴 뒤 선택된 **한 변수군만** 수정하고 영향 스테이지만 재검증.

## 2주 우선순위

순서를 바꾸지 않는다.

1. **코드 안정성:** Time ownership 단일화, atomic save/purchase, 핵심 EditMode/PlayMode 테스트 구축.
2. **현재 날짜 Unity Runtime Green:** 컴파일, 씬 진입, Stage 시작·종료, 오류 없는 저장 경로를 2026-07-14 기준으로 확인.
3. **실제 증거:** `F10` Stage 1-7 sweep + 28 notes + save smoke.
4. **판정:** Evidence Green 통과 후 Decision Green 기록.
5. **제한 튜닝:** 가장 큰 문제 하나와 변수군 하나만 조정하고 영향 스테이지만 재시험.
6. **Audio A:** 경로 개방·유지·보너스, 실패, 보스 경고/격파의 첫 실제 클립 세트 적용 및 레벨 검증. `Assets/Scripts/Runtime/Systems/FeedbackSystem.cs`, `Docs/CURRENT_STAGE_RESOURCE_REQUIREMENTS.md`
7. **모바일 표현:** HUD 실제 기기 가독성과 route contrast 개선. `Assets/Scripts/Runtime/Systems/DummyFlowController.UIFlow.cs`, `Tools/AuditMobileHudReadabilityStatic.ps1`

## 재미 실험 백로그

| 실험 | 최소 검증 지표 | 완료 조건 |
|---|---|---|
| 진짜 경로 선택 | Stage별 첫 목표 선택 분포, 경로별 클리어율·DP/초, 선택 변경률 | 최소 5회씩 비교했을 때 두 경로의 상황별 장단점이 관찰되고 한 경로 선택률이 80%를 넘지 않으며 플레이어가 선택 이유를 설명한다. |
| 압박/해방 대비 | `ROUTE OPEN` 전후 10초의 파괴/초, 피격·실패율, 긴장도 5점 척도 | 개방 후 파괴/초 또는 주관적 해방감이 개방 전보다 25% 이상 상승하고 다음 압박 구간이 명확히 인지된다. |
| `ROUTE BONUS` 인과성 | 보너스 발동 인지율, 발동 후 5초 내 보상 클러스터 진입률, 오발동/중복 지급 수 | 80% 이상이 원인을 정확히 말하고 5초 내 보상으로 이동하며 중복 지급이 0회다. |
| 능동 오버드라이브 | 사용 시점 분산, 사용 후 8초 파괴/초 변화, 낭비 사용률 | 서로 다른 유효 사용 맥락이 2개 이상 나오고 사용 후 파괴/초가 20% 이상 상승하며 낭비 사용이 20% 이하다. |
| 보스 호흡/회복 | 연속 압박 최장 시간, 회복 창 인지율, 보스 구간 이탈 원인 | 5초 이상 읽을 수 있는 회복 창을 80% 이상 인지하고, 불가해한 연속 피격이 테스트 런의 10% 미만이다. |

실험 원자료는 `Logs/AlienCrusherPlaytestTelemetry.log`와 `Docs/AlienCrusherStagePlaytestNotes.md`, 결정은 `Logs/AlienCrusherPlaytestTelemetrySummary.md`와 별도 Decision Green 기록에 남긴다.

## 즉시 다음 업무

### 자율

- [x] 전역 시간 변경 지점을 목록화하고 pause/overdrive/boss 채널의 소유권·중첩·복원 정책을 `DummyFlowController.TimeControl.cs`로 통합했다.
- [x] 전역 시간 직접 쓰기를 차단하는 `Tools/AuditTimeScaleOwnershipStatic.ps1`를 전체 정적 감사 체인에 추가했다.
- [x] 비어 있는 리소스 백로그를 자동화 보고서가 읽을 때 발생하던 null 예외를 방어하고 관련 회귀 테스트를 통과시켰다.
- [x] DP 차감과 해금/업그레이드 지급을 스냅샷 기반 한 상태 변경 및 한 번의 커밋으로 묶고 실패 시 메모리 상태도 롤백한다.
- [x] 저장을 temp write + disk flush + JSON 검증 + atomic replace로 강화하고 백업 복구 시 정상 백업을 보존한다.
- [x] 구매 실패 롤백과 손상 primary/backup 복구를 실제 ProgressionSaveSystem으로 검사하는 Unity Editor 배치 검증기를 추가하고 Tools/RunUnityBatchChecks.ps1에 연결했다.
- [x] pause/overdrive/boss 조합과 해제 순서를 순수 계산 경로로 검사하는 Unity Editor 배치 검증기를 추가했다.
- [x] FormUnlockSystem의 저장 시스템 참조를 직렬화 참조 -> 부모/씬 루트 _Systems 순서로 결정화하고 전역 이름 검색을 제거했다.
- [ ] Unity ILPP 정상화 후 저장 트랜잭션 및 시간 중첩 배치 검증을 실제 실행해 Runtime Green 근거를 갱신한다.
- [ ] Runtime Green 및 `F10` 수행용 체크리스트에서 28개 관찰 칸과 save smoke 절차를 확정한다. `Tools/GenerateStagePlaytestChecklist.ps1`

### 사람 개입

- [ ] `ROUTE HOLD`의 지위를 **필수 조건** 또는 **고득점 선택지** 중 하나로 결정하고 `Docs/GAME_DESIGN_GAP_POLICY.md`에 반영한다.
- [ ] Unity 6000.5.9f1에서 현재 날짜 Runtime Green을 확인한다. 2026-07-14 배치는 IL Post Processor 단계에서 타임아웃됐고, 2026-08-11에는 프로젝트별 잠금 판정을 수정했으나 stale Temp/UnityLockfile 삭제가 승인되지 않아 실행을 보류했다.
- [ ] 실제 `F10` Stage 1-7 sweep을 실행하고 `Docs/AlienCrusherStagePlaytestNotes.md`에 28 notes와 화면/영상 근거를 기록한다.
- [ ] DP, 선택 형태, 스테이지 해금, 메타 업그레이드가 종료·재진입 후 유지되는 save smoke를 수행한다.
- [ ] `Tools/TestPlaytestEvidenceGate.ps1` 결과와 체감 관찰을 함께 검토해 Evidence Green / Decision Green을 승인한다.

## 근거 문서

- 최신 세션 진입점: `Docs/NEXT_SESSION_CONTEXT_PACKET.md`
- 설계·튜닝 정책: `Docs/GAME_DESIGN_GAP_POLICY.md`
- 게임 정의: `Docs/GDD_ALIEN_CRUSHER.md`
- 업데이트 로드맵: `Docs/GAME_UPDATE_ROADMAP.md`
- 플레이 관찰: `Docs/AlienCrusherStagePlaytestNotes.md`
- 자동화 운영: `Docs/AUTOMATION_RUNBOOK.md`
- 런타임 핵심: `Assets/Scripts/Runtime/Systems/DummyFlowController.cs`
- 성장·저장: `Assets/Scripts/Runtime/Systems/FormUnlockSystem.cs`, `Assets/Scripts/Runtime/Systems/ProgressionSaveSystem.cs`
- 검증 진입점: `Tools/RunUnityBatchChecks.ps1`, `Tools/RunStaticAudits.ps1`, `Tools/TestPlaytestEvidenceGate.ps1`

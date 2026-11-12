# 체력 포션 룰렛

치유 포션을 마시면 9개 효과 중 하나가 즉시 발동하는 tModLoader 모드입니다. 룰렛 UI나 별도 CLI 앱 없이 결과만 한국어 채팅으로 표시합니다.

## 적용 방법

1. Steam에서 **tModLoader**를 설치합니다. 기본 Terraria 단독으로는 실행할 수 없습니다.
2. tModLoader의 Workshop → Develop Mods → Open Sources로 소스 폴더를 엽니다.
3. 이 저장소의 `src/PotionRoulette` 폴더를 소스 폴더에 복사합니다.
4. Develop Mods에서 Build + Reload 후 활성화합니다.

이미 빌드된 `PotionRoulette.tmod`가 있다면 tModLoader의 Mods 폴더에 복사하고 활성화할 수 있습니다. 멀티 서버와 참가자에게 같은 버전이 필요합니다.

## 발동과 확률

소형·일반·대형·슈퍼 치유 포션의 실제 소비 시 1회 추첨합니다. 빠른 회복 단축키도 포함합니다. 다른 회복 소비템과 마나 포션은 제외합니다. 원래 포션의 회복과 포션 멀미는 유지됩니다.

| 결과 | 확률 | 효과 |
|---|---:|---|
| 풀피 회복 | 3% | 최대 체력까지 회복 |
| 슬라임 습격 | 20% | 주변에 파란 슬라임 최대 10마리 |
| 미야우미어 | 0.0001% | 1개 지급, 인벤토리 공간이 없으면 드롭 |
| 좀비 합창 | 15% | 사용자에게 좀비 소리 3회 |
| 중력 반전 | 12% | 즉시 반전, 일반 중력 포션 효과 5초 |
| 동굴 탐험가 | 15% | Spelunker 60초 |
| 나이트비전 | 14% | Night Owl 60초 |
| 저항 | 8% | Endurance 60초 |
| 전투 | 12.9999% | Battle 60초 |

미야우미어는 매회 100만 분의 1이며 천장은 없습니다. 중력은 방향키로 바꿀 수 있고 기존 버프 시간이 더 길면 유지됩니다. 슬라임은 소환 공간과 NPC 한도에 따라 10마리보다 적을 수 있습니다. 소환된 몬스터는 다른 플레이어도 공격할 수 있습니다.

## 구조

- `src/PotionRoulette/`: 다른 컴퓨터에 복사할 수 있는 모드 소스
- `scripts/`: 게임 실행 없이 확률 및 빌드 확인
- `artifacts/`: 로컬 빌드 결과 (Git 제외)
- `.build-tools/`: 로컬 빌드 의존성 (Git 제외)

저장소는 Terraria 설치 폴더 안의 독립 하위 폴더입니다. Terraria 실행 파일·콘텐츠·세이브·인증 정보는 공유하지 않습니다.

## 개발 상태

1차 초안입니다. **게임 실행 및 플레이 검증은 수행하지 않습니다.**

멀티에서는 서버가 추첨하고 소환·아이템 지급을 담당하며, 회복·버프·소리는 서버 결과를 받은 소유 클라이언트가 적용합니다. 중복 요청에 2초 제한을 둡니다. 포션 소비 요청은 기본 게임의 클라이언트 소비 훅을 신뢰하며, 악성 클라이언트의 위조 요청을 독립 검증하지 않으므로 신뢰하는 친구와의 테스트용입니다.

검증 결과와 빌드 상태는 `VALIDATION.md`를 참고하세요. GitHub push는 사용자 확인 후에만 진행합니다.

## 명령줄 빌드

PowerShell에서 저장소 루트를 기준으로 실행합니다. tModLoader 설치 또는 공식 배포본 압축 해제 경로와 .NET 8 런타임이 필요합니다.

```powershell
./scripts/Build.ps1 -TModLoaderPath 'C:\경로\tModLoader'
```

결과는 `artifacts/PotionRoulette.tmod`입니다. 빌드는 전용 저장 경로를 사용하며 모드를 사용자 게임 폴더에 자동 적용하지 않습니다.

확률 검증은 PowerShell 7에서 실행합니다. 실제 C# 추첨 코드를 컴파일하여 100만 개 입력 전체를 검사합니다.

```powershell
pwsh -NoProfile -File ./scripts/Test-Weights.ps1
```

빌드에 사용한 공식 배포본: [tModLoader v2026.09.3.1](https://github.com/tModLoader/tModLoader/releases/tag/v2026.09.3.1). 빌드 인자는 [공식 명령줄 안내](https://github.com/tModLoader/tModLoader/wiki/Command-Line)를 참고하세요.

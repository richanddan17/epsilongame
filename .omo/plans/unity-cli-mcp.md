# Unity CLI MCP 적용 및 타 컴퓨터 이식 플랜

작성일: 2026-09-09
상태: 승인 대기 → 승인 후 실행

## 목표

1. OpenCode에서 Unity 에디터를 제어할 수 있도록 **Unity CLI MCP** 를 등록한다.
2. 설정을 git 커밋 가능한 형태로 만들고, `.omo` 문서로 방향성을 잡아 **다른 컴퓨터에도 그대로 이식** 가능하게 한다.

## 배경 (조사 결과)

- Unity 공식 문서: in-editor AI assistant 패키지(`com.unity.ai.assistant`)의 MCP 서버는 **deprecated**.
  공식 대체 = **Unity CLI** 의 `unity mcp` 명령 (Unity Pipeline 패키지 기반).
- Unity CLI는 Unity Hub가 자동 설치함 (`unity` 명령어로 사용 가능).
- Editor 제어에는 **Unity 6.0 LTS 이상** 필요 → 이 프로젝트는 `6000.3.12f1` 으로 충족.
- `com.unity.pipeline: 0.6.0-exp.1` 이 이미 `Packages/manifest.json` 에 존재 → SDK 패키지 준비됨.
- OpenCode MCP 등록 형식(로컬 stdio):
  ```json
  "mcp": {
    "unity": {
      "type": "local",
      "command": ["unity", "mcp"],
      "enabled": true
    }
  }
  ```

## 결정 사항

| 항목 | 결정 | 이유 |
|---|---|---|
| MCP 서버 | Unity CLI (`unity mcp`, stdio) | 공식, deprecated 대체, 이미 Pipeline 패키지 있음 |
| 설정 위치 | 프로젝트 루트 `opencode.json` | git 커밋 → 타 컴퓨터 이식, OpenCode가 프로젝트에서 자동 로드 |
| 이식 문서 | `.omo/docs/unity-cli-mcp.md` | 새 컴퓨터에서 따라 할 단계 문서 |
| 구 MCP 서버 | 제거하지 않음 (보류) | 사용자가 명시 안 함, 영향 최소화 |

## 실행 단계

### 1. `opencode.json` 생성 (프로젝트 루트)
```json
{
  "$schema": "https://opencode.ai/config.json",
  "mcp": {
    "unity": {
      "type": "local",
      "command": ["unity", "mcp"],
      "enabled": true,
      "environment": {}
    }
  }
}
```

### 2. 이식 문서 `.omo/docs/unity-cli-mcp.md` 작성
내용 (다른 컴퓨터에서 그대로 따라함):
- 사전 조건: Unity Hub 설치, Unity 6.0 LTS 이상, `unity` CLI PATH 등록
- Pipeline 패키지 확인: `Packages/manifest.json` 에 `com.unity.pipeline` 존재 확인 (없으면 `unity pipeline install`)
- 설정: 이 저장소를 clone 하면 `opencode.json` 이 이미 포함됨 → MCP 자동 인식
- OpenCode 재시작 → `/mcp` 로 `unity` 서버 연결 확인
- Unity 에디터를 연 상태에서만 동작한다는 주의사항
- 문제 해결: `unity mcp configure --list`, `unity version`, PATH 확인법

### 3. git 커밋 (main 브랜치)
- `opencode.json` + `.omo/docs/unity-cli-mcp.md`
- 커밋 메시지: `add Unity CLI MCP config for OpenCode`
- `.omo/run-continuation` 세션 데이터는 커밋 제외 (gitignore 대상)

## 검증 기준 (Definition of Done)

- [ ] `opencode.json` 존재, JSON 유효 (`jq` 또는 에디터로 확인)
- [ ] `.omo/docs/unity-cli-mcp.md` 존재, 이식 단계 포함
- [ ] 커밋 완료, `git log -1 --oneline` 확인
- [ ] (수동) OpenCode 재시작 후 `unity` MCP 서버가 연결됨 — 사용자가 Unity 에디터 열고 확인

## 실행자 지침

- 서브 에이전트(quick 카테고리)가 위 단계를 수행.
- 파일 2개만 생성/수정. 그 외 파일 건드리지 않음.
- `opencode.json` 이 이미 존재하면 병합(기존 필드 보존).
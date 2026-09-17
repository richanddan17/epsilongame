# mcp-skills-setup - Draft

## Intent
CLEAR - 사용자가 MCP + Unity 스킬 설정을 최우선으로 정리하길 원함 (combat-platformer Wave 4 이전 선행). 에디터를 지금 못 열어서 스킬 로드부터 진행.

## Review Required
false - 고정밀 리뷰 요청 없음

## 현재 상태 (2026-09-17 탐색 결과)
- opencode.json (Unity CLI MCP, 절대 경로) — 커밋됨 (17b05b9)
- Unity 스킬 10개 (.opencode/skills/) — 커밋됨 (f7c52ca)
  - 2d-pixel-perfect, generate-editor-search-query, manage-sprite-atlas, sprite-editor,
    tilemap-palette-create, tilemap-ruletile-createempty, tilemap-ruletile-createfromsegment,
    unity-cli, unity-package-management, urp-postprocessing
- .omo/docs/unity-cli-mcp.md (이식 문서) — 커밋됨
- com.unity.pipeline 0.6.0-exp.1 — manifest.json 존재
- MCP 서버 실제 연결: 에디터 오픈 전까지 검증 불가 (현재 에디터 닫힘)

## 커밋 규칙 위반 발견
- .omo/boulder.json 이 git 추적 중 (M 상태) — AGENTS.md §6 커밋 금지, 추적 해제 필요
- .omo/run-continuation/ 세션 20개 untracked 노출 — .gitignore에 추가 필요

## Decisions Made
- MCP+스킬 정리를 combat-platformer 진행 전 선행
- 에디터 종속 검증(MCP 연결)은 에디터 오픈 시점으로 지연
- 커밋 정리는 AGENTS.md §6 규칙 준수 방향 (boulder/run-continuation 제외)

## Pending Questions
- 없음 — 진행 방향 사용자 확인 대기 (awaiting-approval)

## Status
awaiting-approval - 진행 제안 브리프 제시 완료 (2026-09-17)
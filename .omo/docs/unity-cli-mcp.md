# Unity CLI MCP 설정 가이드

이 문서는 OpenCode에서 Unity CLI MCP 서버를 설정하고 사용하는 방법을 설명합니다.

## 사전 조건

### 1. Unity Hub 설치
- [Unity Hub](https://unity.com/download) 최신 버전이 설치되어 있어야 합니다.

### 2. Unity 6.0 LTS 이상
- 이 프로젝트는 **Unity 6000.3.12f1**을 사용합니다.
- Unity Hub에서 해당 버전이 설치되어 있는지 확인하세요.

### 3. `unity` CLI가 PATH에 등록되어 있는지 확인
PowerShell 또는 터미널에서 다음 명령어를 실행하여 확인:

```powershell
unity --version
```

또는

```powershell
where unity
```

출력 예시:
```
Unity 6000.3.12f1 (6000.3.12f1)
```

또는 경로가 출력되면 정상입니다.

> **참고**: Unity Hub 설치 시 자동으로 PATH에 등록됩니다. 등록되지 않은 경우 Unity Hub > 설정 > "Editor 설치 위치"에서 경로를 확인하고 시스템 환경 변수에 추가하세요.

---

## Pipeline 패키지 확인

Unity 프로젝트의 `Packages/manifest.json` 파일을 열어 `com.unity.pipeline` 패키지가 포함되어 있는지 확인하세요.

```json
{
  "dependencies": {
    "com.unity.pipeline": "1.0.0",
    ...
  }
}
```

**없는 경우** 다음 명령어로 설치:

```powershell
unity pipeline install
```

---

## 설정 방법

### 이 저장소를 Clone한 경우
이 저장소에는 이미 `opencode.json` 파일이 포함되어 있어, OpenCode 실행 시 **MCP 서버가 자동으로 인식**됩니다.

### 새로 설정하는 경우
프로젝트 루트에 `opencode.json` 파일을 생성하고 다음 내용을 추가하세요:

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

---

## OpenCode 재시작 후 연결 확인

1. OpenCode를 완전히 종료 후 다시 실행
2. OpenCode 내에서 MCP 서버 상태 확인:
   - `mcp list` 명령어 실행
   - `unity` 서버가 `connected` 상태로 표시되어야 함

---

## ⚠️ 주의사항

**Unity 에디터가 열려 있어야 MCP 서버가 정상 동작합니다.**

- Unity 에디터가 닫혀 있으면 연결 실패 또는 기능 제한 발생
- 백그라운드에서 Unity 에디터 프로세스가 실행 중이어야 함

---

## 문제 해결

### 1. MCP 서버 설정 확인
```powershell
unity mcp configure --list
```

### 2. Unity 버전 확인
```powershell
unity --version
```

### 3. PATH 환경 변수 확인
```powershell
$env:PATH -split ';' | Where-Object { $_ -like '*Unity*' }
```

### 4. 일반적인 문제와 해결

| 문제 | 해결 방법 |
|------|-----------|
| `unity` 명령어를 찾을 수 없음 | Unity Hub에서 Editor 경로 확인 후 시스템 PATH에 추가 |
| MCP 서버 연결 실패 | Unity 에디터가 실행 중인지 확인, 방화벽/보안 소프트웨어 확인 |
| Pipeline 패키지 없음 | `unity pipeline install` 실행 후 Unity 에디터 재시작 |
| 버전 불일치 | Unity Hub에서 6000.3.12f1 버전 설치 확인 |

---

## 참고 링크

- [Unity CLI 문서](https://docs.unity3d.com/Manual/CommandLineArguments.html)
- [OpenCode MCP 설정](https://opencode.ai/docs/mcp)
- [Unity MCP 패키지](https://github.com/unity-technologies/unity-mcp)
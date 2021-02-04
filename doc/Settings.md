---
title: "환경 설정(CLI)"
excerpt: "Environment Settings"
category: Game-Server
tags: [game-server, .net-core]
toc: true
---

## (CLI)환경 설정

### 설치

- `VSCode(Visual Studio Code)` 다운로드 및 설치
- `C# Extension(VSCode Extension)` 다운로드 및 설치
- `.NET Core SDK(64bits)` 다운로드 및 설치

### 솔루션 및 프로젝트 생성

- `.sln` 파일: 여러 프로젝트를 빌드 용도
- `.csproj` 파일: 프로젝트에 관한 정보로 프로젝트 실행 가능하도록 함.

1. 솔루션 디렉토리(새 폴더) 생성
2. 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet new sln` : 솔루션 파일 생성(여러 프로젝트 빌드)
3. 프로젝트 디렉토리(새 폴더) 생성
4. 프로젝트 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet new console` : 콘솔 기반 프로젝트 생성
5. 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력
   - `dotnet sln add "프로젝트명.csproj"` : 솔루션 파일에 프로젝트 등록
     > dotnet sln add aaa/bbb/ccc/test.csproj
   - 부모 디렉토리에서 `dotnet sln add *` 으로 등록가능
     (대신 등록 불가한 것에 대한 에러 조금 발생)
   - `dotnet sln list` 로 등록 여부 확인

### 실행(CLI)

- 프로젝트 파일로 실행

  - 프로젝트 디렉토리에서 Terminal 실행 및 명령어 입력  
    `dotnet run`

- 솔루션 파일로 실행

  - 솔루션 디렉토리에서 Terminal 실행 및 명령어 입력  
    `dotnet run --project "프로젝트명"`
    > dotnet run --project test

### Example

1. 최상위 디렉토리(`Server-Example`) 생성
2. `Server-Example` 하위 디렉토리로 `Server`, `ServerCore`, `DummyClient` 디렉토리를 생성

   - `Server` : 실질적인 서버 - Main() 실행
   - `ServerCore` : Server가 사용할 API 모음
   - `DummyClient` : 많은 Client 접속 예제 - Main() 실행

3. `dotnet new sln`을 통해서 최상위 디렉토리(Server-Example)에 솔루션 파일(`.sln`) 생성
4. `dotnet sln add *`로 해당 폴더부터 하위 폴더 모두를 검사하여 프로젝트 파일(`.csproj`)를 등록
5. `.sln` 파일 위치에서 `dotnet run --project "프로젝트명"` 입력하여 실행

---

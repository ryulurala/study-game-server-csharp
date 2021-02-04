---
title: "Server 개요"
excerpt: "Server Reference"
category: Game-Server
tags: [server, game-server, web-server]
toc: true
---

## 서버 개요

### 서버

- 다른 컴퓨터에서 연결이 가능하도록 대기 상태로 상시 실행 중인 프로그램

### 서버 종류

#### Web Server vs Game Server

|           Web Server            |          Game Server           |
| :-----------------------------: | :----------------------------: |
|            Stateless            |            Stateful            |
|       실시간 상호 작용 X        |       실시간 상호 작용 O       |
| 질의(Query) / 대답(Answer) 형태 | 요청(Request) / 응답(Response) |

#### 1. Web Server(aka. HTTP Server)

- 질의(Query) / 대답(Answer) 형태
- 실시간 상호 작용 X
- `Stateless`: 서버가 (현재 클라이언트의)상태를 모름
- 프레임워크 종류
  - `ASP.NET(C#)`
  - `Spring(Java)`
  - `NodeJS(JavaScript)`
  - `Django`
  - `Flask(Python)`
  - `PHP`
  - ...

#### 2. Game Server(aka. TCP Server, Binary Server, Stateful Server ...)

- 요청(Request) / 응답(Response) 형태
- 실시간 상호 작용 O
- `Stateful`: 서버가 (현재 클라이언트의)상태를 알고 접근도 가능.
- 게임 장르에 따라 요구 사항이 천차만별.
- 최적의 프레임워크라는 것이 존재하기 힘들다.

### Game Server : 식당

|   게임 서버    |               식당               |
| :------------: | :------------------------------: |
| 동시 접속자 수 |             손님 수              |
|   게임 장르    |             인테리어             |
|    직원 수     |            쓰레드 수             |
|   게임 로직    |              요리사              |
|    네트워크    |            서빙 직원             |
|  데이터베이스  |           장부 및 결제           |
|  쓰레드 모델   | 요리사 / 서빙 / 결제 직원들 비율 |
| 네트워크 모델  |          주문하는 방법           |

---

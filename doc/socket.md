---
title: "Socket"
category: Game-Server
tags: [c#, socket, server, client]
date: "2021-02-15"
---

## Socket Programming

### Store vs Server/Client

- 식당 vs 소켓 통신 구조

|  식당 주인  |              Server               |
| :---------: | :-------------------------------: |
| 문지기 고용 |            `Socket()`             |
| 문지기 교육 | `Bind()`: `IP` + `Port` -> Socket |
|  영업 시작  |            `Listen()`             |
|    안내     |            `Accept()`             |

|         손님          |  Client   |
| :-------------------: | :-------: |
|      휴대폰 준비      | Socket()  |
| 식당 번호로 입장 문의 | Connect() |
|        대리인         |  Session  |

---

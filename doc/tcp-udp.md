---
title: "TCP vs UDP"
category: Game-Server
tags: [tcp, udp]
date: "2021-02-21"
---

## TCP vs UDP

- Network 통신은 `Packet`을 주고 받는다.

  > Class를 작성하여 원하는 정보만 주고 받음.

- `TCP` vs `UDP`

  | `TCP`(Transmission Control Protocol) | `UDP`(User Datagram Protocol) |
  | :----------------------------------: | :---------------------------: |
  |     전화 연결 방식, 안전성 높음      |    우편 전송 방식, 위험함     |
  |            연결형 서비스             |        비연결형 서비스        |
  |          전송 순서 보장 [O]          |      전송 순서 보장 [X]       |
  |      속도 느림: UDP에 비교해서       |           속도 빠름           |
  |    신뢰성 높음: 손실 시 다시 보냄    |          신뢰성 낮음          |
  |           흐름 / 혼잡 제어           |                               |
  |                                      |       ex) 주로 FPS 게임       |

---

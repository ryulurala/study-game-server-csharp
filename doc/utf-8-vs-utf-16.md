---
title: "UTF-8 vs UTF-16"
category: Game-Server
tags: [ascii, unicode, utf-8, utf-16, BMP]
date: "2021-02-23"
---

## UTF-8 vs UTF-16

- Encoding
  > 문자를 컴퓨터가 이해시키기 위해 만듦.
- Server와 Client는 UTF-8 or UTF-16 Encoding 규격을 맞춰야 통신이 가능.

### 고정(Fix)

#### ASCII Code

- 0~127
- 1byte로 표현
- Only. English
- ex)

  1. `A`: 0x41, 65
  2. `!`: 0x21, 33

#### Unicode

- 0~65535
- 2byte로 표현(= BMP 영역) + 1byte 추가(한문 등)
- the other language
- ex)

  1. `A`: 0x000041, 65
  2. `!`: 0x000021, 33
  3. `ㅎ`: 0x001112, 4370

### 가변(no Fix)

#### UTF-8

- 영어권에 유리

  - 영문: 1byte로 표현
  - 한글: 3byte로 표현

- ex)

  1. C++

#### UTF-16

- 한글, 중국어, 일본어 등등 유리

  - 영문: 2byte로 표현
  - 한글: 2byte로 표현
  - BMP 영역 외: 4byte로 표현

- ex)

  1. C#

---

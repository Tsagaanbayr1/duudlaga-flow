# Duudlaga Flow

Монгол яриаг бичвэрт хөрвүүлэх програм. Chimege API ашиглан таны ярьсан үгийг аль ч апп дээр автоматаар бичвэрт хөрвүүлнэ.

## Онцлогууд

- **Хурдан бичвэр** — Товчлуур дарж ярихад текст автоматаар бичигдэнэ
- **Аль ч апп дээр** — Идэвхтэй цонх дээр шууд текст оруулна
- **System tray** — Далд ажиллана, нөөц бага зарцуулна
- **Chimege STT** — Монгол хэлний ярианы таних технологи

---

## Татаж авах

### Windows

> **Шаардлага:** Windows 10/11 (64-bit)

[**DuudlagaFlow-1.0.0-x64.msi татах**](https://github.com/Tsagaanbayr1/duudlaga-flow/releases/download/v1.2.1/DuudlagaFlow-1.0.0-x64.msi)

**Товчлуур:** `Ctrl + Alt`

Дэлгэрэнгүй суулгах заавар, тохиргооны мэдээллийг [Windows заавар](windows/) хэсгээс харна уу.

---

### macOS

> **Шаардлага:** macOS 13+

[**DuudlagaFlow-1.2.1.dmg татах**](https://github.com/Tsagaanbayr1/duudlaga-flow/releases/download/v1.2.1/DuudlagaFlow-1.2.1.dmg)

**Товчлуур:** `Control + Option`

Дэлгэрэнгүй суулгах заавар, тохиргооны мэдээллийг [macOS заавар](macos/README.md) хэсгээс харна уу.

---

## Хэрхэн ажилладаг вэ?

1. Chimege Console-оос API token авна ([console.chimege.com](https://console.chimege.com))
2. Програмын тохиргоонд token-оо оруулна
3. Хүссэн апп дээрээ товчлуур дарж ярина
4. Текст автоматаар бичигдэнэ

## Төслийн бүтэц

```
duudlaga-flow/
├── macos/          # macOS хувилбар (Swift/SwiftUI)
└── windows/        # Windows хувилбар (WPF/.NET 8)
```

## Лиценз

Copyright 2025 DuudlagaFlow. All rights reserved.

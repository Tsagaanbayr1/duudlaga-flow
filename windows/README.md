# Duudlaga Flow — Windows

Монгол хэлний ярианы бичвэр хөрвүүлэгч. Chimege API ашиглан таны ярьсан үгийг аль ч апп дээр автоматаар бичвэрт хөрвүүлнэ.

## Татаж авах

> **Шаардлага:** Windows 10/11 (64-bit), .NET 8.0 Runtime

[**DuudlagaFlow-1.1.0-x64.msi татах**](https://github.com/Tsagaanbayr1/duudlaga-flow/releases)

### Суулгах заавар

1. Дээрх линкээр `.msi` файлыг татаж авна
2. Татаж авсан файлыг нээж суулгана
3. Програм автоматаар `AppData\Local\DuudlagaFlow` хавтаст суулгагдана
4. Start menu болон Desktop дээр shortcut үүснэ

---

## Хөгжүүлэгчдэд: Эх кодоос ажиллуулах

### Шаардлага

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### Төсөл татах

```bash
git clone https://github.com/Tsagaanbayr1/duudlaga-flow.git
cd duudlaga-flow/windows
```

### Ажиллуулах

```bash
dotnet run --project DuudlagaFlow
```

### Build хийх

```bash
dotnet build DuudlagaFlow.sln
```

Build хийсний дараа `DuudlagaFlow\bin\Debug\net8.0-windows\DuudlagaFlow.exe` файлыг шууд ажиллуулж болно.

### Release build

```bash
dotnet publish DuudlagaFlow -c Release -r win-x64 --self-contained
```

---

## Тохиргоо

### API Token

1. [console.chimege.com](https://console.chimege.com) хаягаар орж бүртгүүлнэ
2. **Монгол STT** эсвэл **Монгол STT-Long** үйлчилгээг идэвхжүүлнэ
3. API token хуулна
4. Програмын тохиргоо (Тохиргоо > Ерөнхий) хэсэгт token-оо оруулна
5. STT горим дропдоун-аас идэвхжүүлсэн үйлчилгээгээ сонгоно:
   - **Монгол STT** — Стандарт, хурдан хариу
   - **Монгол STT-Long** — Урт аудионд зориулагдсан
6. "Шалгах" товч дарж token зөв эсэхийг шалгана

### Хэрэглээ

| Товчлуур | Үйлдэл |
|----------|---------|
| `Ctrl + Alt` (дарж барих) | Бичлэг эхлэх / зогсоох |
| `Esc` | Бичлэг цуцлах |

1. Хүссэн апп дээрээ курсороо байрлуулна
2. `Ctrl + Alt` дарж ярина
3. Товчлуур тавихад текст автоматаар бичигдэнэ

---

## Төслийн бүтэц

```
windows/
├── DuudlagaFlow.sln
├── DuudlagaFlow/
│   ├── Models/          # Өгөгдлийн загварууд
│   ├── Services/        # API, аудио, текст оруулах үйлчилгээнүүд
│   ├── ViewModels/      # MVVM төлөв
│   ├── Views/           # UI цонхнууд
│   ├── Utilities/       # Туслах функцууд
│   └── Resources/       # Icon, стиль
└── installer/           # WiX суулгагч
```

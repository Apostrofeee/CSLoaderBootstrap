# CS Loader Bootstrap

<details open>
<summary>RU</summary>

Launcher для фикса бага в **"Клиентах" CS:GO (NL, GS и др.)**, при котором вместо **CS:GO** запускается **CS2**.

Проще говоря: клиент пытается открыть `cs2.exe`, а эта программа перехватывает запуск и открывает нужную версию игры через Steam.

---

## Установка

Замените оригинальный `cs2.exe` на этот файл.

> **Внимание:**  
> Оригинальный `cs2.exe` нужно удалить.

> **Важно:**  
> Для работы нужен [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

---

## Возможности

- Запуск **CS:GO** через Steam AppID `4465480`
- Ускоренный запуск CS:GO
- Автоматический поиск `steam.exe` через Windows Registry
- Запасной поиск Steam в стандартных папках установки
- Ручной ввод пути к `steam.exe`, если Steam не найден автоматически
- Возможность сохранения пути для дальнейшего использования

---

## Ручной запуск

Обычно ничего вводить не нужно, достаточно просто запустить файл.

Если Steam не найден автоматически, можно указать путь вручную:

```powershell
cs2.exe --steam "C:\Program Files (x86)\Steam\steam.exe"
```

</details>

<details>
<summary>EN</summary>

Launcher for fixing a bug in **CS:GO "Clients" (NL, GS, etc.)**, where **CS2** starts instead of **CS:GO**.

In simple terms: the client tries to open `cs2.exe`, and this program intercepts the launch and opens the required game version through Steam.

---

## Installation

Replace the original `cs2.exe` with this file.

> **Warning:**  
> The original `cs2.exe` must be removed.

> **Important:**  
> Requires [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

---

## Features

- Launches **CS:GO** through Steam AppID `4465480`
- Faster CS:GO startup
- Automatic `steam.exe` lookup through Windows Registry
- Fallback Steam lookup in default installation folders
- Manual `steam.exe` path input if Steam is not found automatically
- Option to save the path for future launches

---

## Manual Launch

Usually, you do not need to enter anything. Just run the file.

If Steam is not found automatically, you can specify the path manually:

```powershell
cs2.exe --steam "C:\Program Files (x86)\Steam\steam.exe"
```

</details>

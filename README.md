# 📅 SchedulerApp - Windowsスケジューラー

C# (.NET 10 / WPF) で作成したスケジューラーアプリです。

---

## 📋 機能

- ✅ **タスク管理** - 追加・編集・削除・完了チェック
- 📅 **カレンダー表示** - 月間ビュー、タスクをカラードットで可視化
- 🔔 **リマインダー通知** - 指定時間前に画面右下へポップアップ
- 🔁 **繰り返しスケジュール** - 毎日・毎週・毎月・毎年
- 🎨 **カラーラベル** - 6色からカテゴリ分け
- 💾 **自動保存** - `%APPDATA%\SchedulerApp\tasks.json`

---

## ⚡ クイックスタート

### 前提条件

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022](https://visualstudio.microsoft.com/ja/) （「.NET デスクトップ開発」ワークロードを有効化）
- Windows 11

### 手順

```bash
# 1. リポジトリをクローン（またはZIPを解凍）
git clone <リポジトリURL>
cd scheduler-app

# 2. パッケージ復元
dotnet restore

# 3. ビルド & 起動
dotnet run
```

または `SchedulerApp.sln` を Visual Studio で開いて **F5** でデバッグ実行できます。

---

## 🛠️ 開発コマンド

### Visual Studio

| 操作 | 方法 |
|---|---|
| ビルド | `Ctrl+Shift+B` |
| デバッグ実行 | `F5` |
| デバッグなし実行 | `Ctrl+F5` |

### CLI（ターミナル / PowerShell）

```bash
# デバッグビルド
dotnet build

# リリースビルド
dotnet build -c Release

# Windowsアプリとして発行（単一ファイル）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish
```

---

## 📁 プロジェクト構成

```
scheduler-app/
├── .vscode/
│   ├── tasks.json           # ビルド/発行タスク
│   └── launch.json          # デバッグ設定
├── Models/
│   ├── ScheduleTask.cs      # タスクモデル
│   └── CalendarDay.cs       # カレンダー日付モデル
├── Views/
│   ├── TaskDialog.xaml/cs   # タスク追加・編集ダイアログ
│   └── NotificationWindow.xaml/cs  # リマインダー通知ポップアップ
├── Services/
│   ├── DataService.cs       # JSON永続化
│   └── NotificationService.cs      # リマインダータイマー
├── App.xaml/cs              # アプリエントリポイント
├── MainWindow.xaml/cs       # メインウィンドウ
├── SchedulerApp.csproj      # プロジェクト設定
├── SchedulerApp.sln         # ソリューションファイル
├── .editorconfig            # コーディングスタイル設定
└── .gitignore               # Git除外設定
```

---

## 💾 データ保存場所

タスクデータは以下のパスにJSON形式で自動保存されます。

```
%APPDATA%\SchedulerApp\tasks.json
```

エクスプローラーで `Win+R` → `%APPDATA%\SchedulerApp` で直接開けます。
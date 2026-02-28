# 📅 SchedulerApp - Windowsスケジューラー

C# (.NET 10 / WPF) で作成したスケジューラーアプリです。

---

## ⚡ クイックスタート（Devcontainer）

### 前提条件
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [VS Code](https://code.visualstudio.com/)
- [Dev Containers 拡張機能](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers)

### 手順

```bash
# 1. リポジトリをクローン（またはZIPを解凍）
git clone <リポジトリURL>
cd SchedulerApp

# 2. VS Code で開く
code .
```

VS Code が開いたら右下に `「コンテナーで再度開く」` ポップアップが表示されます。  
クリックするとDockerコンテナが自動ビルドされ、開発環境が起動します。

表示されない場合は `Ctrl+Shift+P` → `Dev Containers: Reopen in Container`

---

## ⚠️ WPF と Devcontainer について

| 環境 | ビルド | 実行 |
|---|---|---|
| **Linux/Mac Devcontainer** | ✅ 可能 | ❌ 不可（WPFはWindows専用） |
| **Windows Devcontainer** | ✅ 可能 | ✅ 可能 |
| **Windows ローカル** | ✅ 可能 | ✅ 可能 |

**推奨ワークフロー**：  
Devcontainer でコード編集・ビルド確認 → Windowsマシンで実行・デバッグ

---

## 📋 機能

- ✅ **タスク管理** - 追加・編集・削除・完了チェック
- 📅 **カレンダー表示** - 月間ビュー、タスクをカラードットで可視化
- 🔔 **リマインダー通知** - 指定時間前に画面右下へポップアップ
- 🔁 **繰り返しスケジュール** - 毎日・毎週・毎月・毎年
- 🎨 **カラーラベル** - 6色からカテゴリ分け
- 💾 **自動保存** - `%APPDATA%\SchedulerApp\tasks.json`

---

## 🛠️ 開発コマンド（VS Code ショートカット）

| 操作 | コマンド |
|---|---|
| ビルド | `Ctrl+Shift+B` → `🔨 Build (Debug)` |
| テスト実行 | `Ctrl+Shift+P` → `Tasks: Run Test Task` |
| 発行 | タスク: `📦 Publish (win-x64, Single File)` |
| フォーマット | タスク: `✨ Format Code` |

### CLIでのビルド

```bash
# パッケージ復元
dotnet restore

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
SchedulerApp/
├── .devcontainer/
│   ├── devcontainer.json    # Devcontainer設定
│   ├── Dockerfile           # .NET 10 + 日本語環境
│   └── docker-compose.yml   # 拡張構成（オプション）
├── .vscode/
│   ├── tasks.json           # ビルド/テスト/発行タスク
│   └── launch.json          # デバッグ設定
├── Models/
│   ├── ScheduleTask.cs      # タスクモデル
│   └── CalendarDay.cs       # カレンダー日付モデル
├── Views/
│   ├── TaskDialog.xaml/cs   # タスク追加・編集ダイアログ
│   └── NotificationWindow.xaml/cs  # リマインダー通知
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

## 💡 ヒント

- **NuGetキャッシュ**はホストと共有されるため、2回目以降のビルドが高速
- **Oh My Zsh** がインストール済みで快適なターミナル操作が可能
- **CSharpier** と **dotnet-format** でコードの自動整形が可能

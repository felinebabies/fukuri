# 複利計算シミュレーター

Quartz.NET を利用した .NET コンソールアプリケーションで、10 分ごとに 1.5% の複利を適用した借金額を計算します。

## 前提条件

- [.NET 8 SDK](https://dotnet.microsoft.com/ja-jp/download) がインストールされていること

## セットアップと実行方法

```bash
cd CompoundInterestSimulator
dotnet restore
dotnet run
```

アプリケーションは起動直後に複利計算を 1 回行い、その後は 10 分ごとに借金額を更新してコンソールに表示します。終了するには `Ctrl + C` を押してください。

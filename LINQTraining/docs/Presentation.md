---
marp: true
theme: default
title: LINQ研修 プレゼンテーション資料
---

# LINQ演習

## 演習の目標
* LINQ to Object, LINQ to Entityの効率的な書き方、デバッグ方法を学ぶ
* LINQ to Objectのクエリオブジェクトを理解し、カスタムクエリメソッドを実装する
* LINQ to Entityの動的クエリを実装できるようになる

---

# 環境
あらかじめインストールしておいてください。
* IDE (Visual Studio or Rider)
* [.NET Core 3.1 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/3.1)
* [EF Core command-line tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)
* [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)

```
dotnet tool install --global --version 3.1.30 dotnet-ef
```

---

# ビルド
```
cd C:\gitreps\LINQTraining\LINQTraining
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---


# LINQの本質
* LINQは「SQLっぽく書ける機能」ではない。
* LINQの本質は「集合演算」
  * 集合を集合のまま扱える、個別要素に対する演算はしない

LINQとは何か？という話はあまりしなくていい
LINQ使ったことがない人はいないという前提

---

# 演習
できあがったコードを渡してデバッガ上で実行してもらう

## LINQ式がどうやって実行されているか見てみる - LINQ to Object編
デバッガで追跡してみる
内部的にyield returnで動いていることを見せる

## LINQ式がどうやって実行されているか見てみる - LINQ to Entity編
実際に実行されているSQLを眺める
SQLトレース出力の取り方

---

# 必修課題
要求仕様を提示してプログラムを組んでもらう

## カスタムクエリメソッドを実装してみる
Chunkを実装してみる
sourceのcollectionをサイズnの配列に分割する
正解例は.net6のソース（.net Frameworkのソースの読み方を覚えてもらいたい）

## 性能改善
ループ内クエリ（二重ループ）をJoinに置き換えた事例
※昔やった事例を集める（来週の課題）

# Optional課題
## Dynaminc Query
SelectするColumnを動的に決定する事例（某社のシステムを簡略化したようなもの）
※これを必修にするとExpressionの書き方を説明しないといけなくなるので...

---
# おまけ
同じことをRiderで実行するとこうなります、みたいなことを見せたい
（インストールさせないが画面共有してデモする）
Visual Studioしか使ったことがない人が多いと思うので

---
marp: true
theme: default
title: LINQ研修 テキスト
---

# LINQ演習 テキスト

## 演習の目標

* LINQ to Object, LINQ to Entityの効率を意識した書き方やデバッグ方法を学ぶ
* LINQ to Objectのカスタムクエリを実装する
* LINQ to Entityの動的クエリを実装する

## 予習しておいてほしい用語

* ```IEnumerable```と```IQueryable```
* ナビゲーションプロパティと```Include```
* 関係演算（選択、射影、結合）

---

## 環境

あらかじめインストールしておいてください。

### ないと動かないもの

* [.NET Core 3.1 SDK 3.1.425](https://dotnet.microsoft.com/en-us/download/dotnet/3.1) Version 3.1.31
* [EF Core command-line tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) Version 3.1.31

### あると便利なもの

* IDE (Visual Studio + [ReSharper](https://www.jetbrains.com/resharper/), or [Rider](https://www.jetbrains.com/rider/))
* [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms?view=sql-server-ver16)

#### EF Core command-line toolsのインストール

```bat
dotnet tool install --global --version 3.1.31 dotnet-ef
```

---

## ビルド

[GitHubに演習プログラムを置いてあります](https://github.com/h-uchiy/LINQTraining)ので、cloneまたはダウンロードして、ビルドを通しておいてください。

```bat
cd C:\gitreps\LINQTraining\LINQTraining
dotnet ef migrations add InitialCreate
dotnet ef database drop
dotnet ef database update
dotnet build
dotnet test
```

[dotnet ef コマンドのリファレンス](https://learn.microsoft.com/en-us/ef/core/cli/dotnet#dotnet-ef-database-drop)

---

### 演習プログラムの解説

* 演習問題は```Excercise.cs```にて、xUnitのテストケースとして作成してあります。
* テストケースはAAAパターン(Arrange/Act/Assert)で作成してあります。
  * Arrangeにてダミーのデータをデータベースに作成します。
  * Actが演習問題の本体で、別関数```ExerciseX_Act```にて実装しています。
  * Actを```Answers.cs```にある解答例と差し替えて実行することもできます。
* データモデルの一部のクラスには、大量の無駄なプロパティ```BlahXXXX```を定義してあります。これは大量の列が定義されている現実のプロジェクトのテーブルを模したもので、非効率なクエリが一目でわかるように作成しています。
* 実際に実行されるSQLをログに出力します。やり方は```TrainingContext.cs```を見てください。

---

## 例題1

この関数をリファクタリングしてください。

```c#
private static async List<Exercise1Result> Exercise1_Act(TrainingContext context, string metadataCode)
{
    var metadataValues = context.DataValues
        .Include(x => x.Metadata)
        .Where(x => x.Metadata.Code == metadataCode);

    var result = new List<Exercise1Result>();
    foreach (var metadataValue in metadataValues.ToList())
    {
        result.Add(new Exercise1Result
        {
            DataType = metadataValue.Metadata.DataType,
            Value = metadataValue.Value
        });
    }

    return result;
}
```

一見すると複雑に見えますが、選択と射影(filter & map)しかしていないことに、すぐに気づいてほしいです。
LINQはそのための道具なので、この程度のことは1行で記述できます。

---

## 例題1 - ループの除去

* LINQは「集合を集合のまま演算する」ものです。
* ループを記述するということは、集合から要素を１つずつ取り出して、１つずつ演算する、ということですから、LINQを使う意味がありません。

```csharp
public static List<Exercise1Result> Exercise1_Act1(TrainingContext context, string metadataCode)
{
    return context.DataValues
        .Include(x => x.Metadata)
        .Where(x => x.Metadata.Code == metadataCode)
        .ToList()
        .Select(metadataValue => new Exercise1Result
        {
            DataType = metadataValue.Metadata.DataType,
            Value = metadataValue.Value
        })
        .ToList();
}
```

---

## 例題1 - 自動リファクタリング

このパターンのループは機械的に```ToList()```に置き換えが可能です。上のコードはReSharper/Riderで自動リファクタリングしたものです。

![auto-refactor-suggestion](image-20221205202113467.png)

---

## 例題1 - 実際に実行されるSQLを見る

先のコードで実行されるSQLがコンソールに出力されるので、確認してください。

```sql
SELECT [d].[Id], [d].[Blah000], [d].[Blah001], ..., [d].[Blah099], [d].[MetadataId], [d].[Value],
 [m].[Id], [m].[Blah000], [m].[Blah001], ..., [m].[Blah099], [m].[Code], [m].[DataType], [m].[Name]
FROM [DataValues] AS [d]
INNER JOIN [Metadata] AS [m] ON [d].[MetadataId] = [m].[Id]
WHERE [m].[Code] = @__metadataCode_0
```

必要な列は```[Metadata].[DataType]```と```[DataValues].[Value]```のみなのに、不要な列が大量に取得されています。SQL Serverとの通信はネットワーク経由になるのが普通ですから、パフォーマンス上の大きなペナルティになり得ます。

これは、```Where```まではSQLに変換されているが、```Select```がSQLに変換されていないので、全部の列をSQL Serverから取ってきて、メモリ上で```Select```を実行しているためです。

---

## 原則 - なるべくクエリオブジェクトのまま引っ張る

* ```foreach```などのイテレーションや、```ToList()```などの即時実行関数は、最終的に必要なデータを作る時まで実行しない

## 原則 - なるべく```IQueryable```のまま引っ張る

* ```IQueryable```で記述されている部分は、SQLに変換されてSQL Server上で実行される
* ```IEnumerable```で記述されている部分は、メモリに展開されてC#上で実行される

## 原則 - ```IQueryable```のうちに絞り込みを行う

* ```IEnumerable```で```Where```や```Select```をしても、通信量を削減できない

---

## 例題1 - ToList()を除去する

原則に従ってリファクタリングします。

```c#
public static IQueryable<Exercise1Result> Exercise1_Act2(TrainingContext context, string metadataCode)
{
    return context.DataValues
        .Where(x => x.Metadata.Code == metadataCode)
        .Select(x => new Exercise1Result
        {
            DataType = x.Metadata.DataType,
            Value = x.Value
        });
}
```

実行されるSQLはこうなり、必要な列だけが取得されています。

```sql
SELECT [m].[DataType], [d].[Value]
FROM [DataValues] AS [d]
INNER JOIN [Metadata] AS [m] ON [d].[MetadataId] = [m].[Id]
WHERE [m].[Code] = @__metadataCode_0
```

---

## Point - ```Include()```が必要な場合と不要な場合 (1)

上の例では何気なく```Include()```を削除してしまいましたが、動いています。

一方、次の場合は、```Include()```を削除すると動きません。

```c#
public static List<Exercise1Result> Exercise1_Act1(TrainingContext context, string metadataCode)
{
    return context.DataValues
        // .Include(x => x.Metadata)
        .Where(x => x.Metadata.Code == metadataCode)
        .ToList()
        .Select(metadataValue => new Exercise1Result
        {
            DataType = metadataValue.Metadata.DataType,
            Value = metadataValue.Value
        })
        .ToList();
}
```

結果

```console
System.NullReferenceException: Object reference not set to an instance of an object.
```

---

## Point - ```Include()```が必要な場合と不要な場合 (2)

```Include()```は、ナビゲーションプロパティをメモリにロードすることをEFに指示するコマンドです。
ナビゲーションプロパティの参照が、```IQueryalble```の世界で完結している場合は、必要ありません。
大量のデータが不用意にロードされないように、なるべく書かないようにした方が良いです。

---

## 例題2

このメソッドは、```errorsList```に書かれた情報に従って、```dataTable```に"Error Column"を追加します。（詳しい関数仕様はソースコードのコメントを参照）

```c#
private static void Exercise2_Act(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
{
    foreach (var row in dataTable.Rows.OfType<DataRow>())
    {
        var errors = errorsList.Where(y =>
            int.TryParse(row["Row No"].ToString(), out var rowNo) && y.RowNo == rowNo);
        if (errors.Any())
        {
            row["Error Column"] = string.Join(",", errors.Select(x => x.ColumnName));
        }
        else
        {
            row["Error Column"] = string.Empty;
        }
    }
}
```

---

## 例題2 - 問題点

これは、開発中は問題なく動作していたが、実運用では使い物にならない、という典型的な事例です。

* dataTableが増えてもerrorsListは増えない場合
  * 瞬時に処理が完了する
* dataTableの件数に比例してerrorsListの件数も増える場合
  * 開発中に1000件程度のデータでデバッグしていると、瞬時に処理が完了する
  * 実運用で10万件のデータが入ると、10分経っても処理が完了しない

---

## 例題2 - Multiple Enumeration

ReSharper/Riderではこのような警告が表示されます。
![image-20221206183153145](image-20221206183153145.png)
これは、errorsListやerrorsに対して、要素を取得する操作が複数回行われていることを意味します。

errorsListやerrorsには、データは入っておらず、データの取得と処理の方法のみが格納されています。LINQでデータ処理を記述すると「中間過程ではデータの取得も演算もしない」から、高速かつ省メモリで処理が行われます。
ところが、上で警告が出ている箇所では、データの取得と演算が行われています。これは最終的な処理結果を得るときには必要なことですが、中間過程で行うのは無駄です。
また、データベースやストリームがデータソースの場合は、一度しか読出しできませんので、エラーが発生します。

---

## 例題2 - 遅い場所を調べる

ReSharper/Riderに付属するdotTraceというプロファイラで遅い箇所を探してみます。

![image-20221209161700092](image-20221209161700092.png)

```Where```式内のラムダ式が1億2千万回呼ばれており、実行時間の大半がここで消費されていることがわかります。



---

## 例題2 - Where()の探索アルゴリズム

---

## 例題2 - 探索アルゴリズム

専門的な探索アルゴリズムは色々ありますが、プログラマーが日常的に扱う探索アルゴリズムは、この3種類しかありません。

| 名前 | 概要 | 速さ | Pros/Cons |
| ---------------- | ---------------------- | -------- | ------------------------------------ |
| 線形探索 | 全要素を探索 | *O(N)* | 遅いが、単純なので要素が少なければ最速 |
| 二分探索 | 範囲を絞りながら探索 | *O(log2N)* | メモリ消費が少ないが、ソートするので追加が遅い   |
| ハッシュテーブル | 要素のハッシュ値で探索 | *O(1)*     | バケットを構築するのでメモリを食う |

---

## 例題2 - LINQと探索アルゴリズム

何も考えずにLINQを使っていると、線形探索しか使っていないということになりがちです。場面に応じて最適な探索アルゴリズムを選べるようになりましょう。

| アルゴリズム | コンテナ | LINQ                                    |
| ---- |---- |-----------------------------------------|
| 線形探索         | ```Array<T>```<br/>```List<T>```                                                                      | ```Where()```<br/>```ToList()``` |
| 二分探索         | ```SortedSet<T>```<br/>```SortedList<TKey,TValue>```<br/>```SortedDictionary<TKey,TValue>``` | -                                       |
| ハッシュテーブル | ```Set<T>```<br/>```Dictionary<TKey,TValue>```<br/>```Lookup<TKey,TElement>``` | ```ToDictionary()```<br/>```ToLookup()``` |


## 例題3 - 


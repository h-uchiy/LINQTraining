---
marp: true
theme: default
title: LINQ研修 テキスト 問題編
---

# LINQ演習 テキスト 問題編

## 演習の目標

実際の開発現場で見つけた事例をもとに、以下の技術を学びます。

* LINQ to Object, LINQ to Entityの効率を意識した書き方やデバッグ方法を学ぶ
* LINQ to Objectのカスタムクエリを実装する
* LINQ to Entityの動的クエリを実装する

## 予習

* このテキストとソースコード(```Exercises.cs```)を読み、不明点を調べておく
* 環境の項目に従い、環境構築をしておく
* ビルドの項目に従い、演習プログラムのビルドを通しておく

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

[GitHubに演習プログラムを置いてあります](https://github.com/h-uchiy/LINQTraining)ので、cloneまたはダウンロードして、ビルドを通しておいてください。（更新するのでcloneがおすすめ）

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

### 演習プログラムの構成

* プロジェクトは問題編```LinqTraining```と解答編```LinqTraining-Answer```があります。
* 演習問題は```Excercise.cs```にて、xUnitのテストケースとして作成してあります。
* テストケースは[AAAパターン(Arrange/Act/Assert)](https://qiita.com/inasync/items/e0b54e62784710c4b42d)で作成してあります。
  * Arrangeにてダミーのデータをデータベースに作成します。
  * Actが演習問題の本体で、別関数```ExerciseX_Act```にて実装しています。
  * Actを```Answers.cs```にある解答例と差し替えて実行することもできます。
* データモデルの一部のクラスには、大量の無駄なプロパティ```BlahXXXX```を定義してあります。これは大量の列が定義されている現実のプロジェクトのテーブルを模したもので、非効率なクエリが一目でわかるように作成しています。
* 実際に実行されるSQLをログに出力します。やり方は```TrainingContext.cs```を見てください。

---

## 例題1

この関数をリファクタリングして、必要最小限のデータのみをSQL Serverから取得するように改善してください。

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

---

## 例題1 - ヒント

* 一見すると複雑に見えますが、選択と射影(filter & map)しかしていないことに、すぐに気づいてほしいです。 LINQはそのための道具なので、この程度のことは1行で記述できます。
* ループを解消してください。ReSharper/Riderであれば、自動リファクタリングで片付きます。
* IQueryableが実行しようとする式や、実際に実行されるSQLを確認して、最終的に必要なデータ以外の列をDBから取得しないようにしてください。
* 無駄な```ToList()```を削除して、データをメモリに展開しないようにしてください。Exercise1_Actの戻り値は```IQueryable```に変更しても構いません。
* Joinを使った式に書き直してみましょう。

---

## 例題2

これは、開発中は問題なく動作するが、実運用でデータが増えると使い物にならない、という事例です。実用に耐えられるように改善してください。

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

## 例題2 - 仕様と問題点

### 仕様

* dataTableには、1で始まる行番号'Row No'列のほか、多数の列があります。
* errorsListには、エラーがある行の番号と、列の名前が入っています。
* dataTableに'Error Column'列を追加して、エラーがある列の名前を書き込みます。
  * 同じ行の複数の列にエラーがある場合は、列名をカンマ区切りで'Error Column'列に書き込みます。

### 問題点

dataTableの件数に比例してerrorsListの件数も増える場合

* デバッグのため1000件のダミーデータ動かすと瞬時に処理が完了する
* 実運用で10万件のデータが入れられると10分経っても処理が完了しない

---

## 例題2 - ヒント

* ReSharper/Riderでは["Multiple Enumeration"](https://www.jetbrains.com/help/rider/PossibleMultipleEnumeration.html)という警告が発生します。これを解消してください。
* dataTableとerrorsListの行数を増やすと、行数を増やした以上に遅くなります。（概ね2倍にすると4倍、10倍にすると100倍）
なぜこのようになるのか考えて、これを解消してください。

---

## 例題3 - カスタムクエリメソッドを実装する（即時実行編）

* 標準の```ToArray()```, ```ToList()```, ```ToHashSet()```, ```ToDictionary()```, ```ToLookup()```のように、コンテナに変換するメソッド```ToSortedSet()```, ```ToSortedList()```, ```ToSortedDictionary()```を```EnumerableExtensions.cs```に作成してください。
* 上記のそれぞれについて、非同期版```~Async()```を```QueryableExtensions.cs```に作ってください。

### 例題3 - ヒント

* ```int```や```string```などの組込型だけでなく、任意のユーザー定義型でも、検索アルゴリズムが正しく動作するようにしてください。
* 標準のメソッドのソースコードを読んで、真似してください。

---

## 例題4 - カスタムクエリメソッドを実装する（遅延実行編）

以下の課題は```EnumerableExtensions.cs```に作成してください。

* 標準の```Distinct()```には、```OrderBy()```や```GroupBy()```,```ToDictionary()```のようにキーを指定する機能が無いので不便です。 キーを指定できる```DistinctBy()```に作成してください。
* 入力シーケンスを指定されたサイズの配列に分割する```Chunk()```を作成してください。

### ヒント

* [```yield return```](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/yield)を使って実装してください。
* 以上の課題のメソッドはすべて.NET6で追加されたものです。.NET Core 3.1以前のプロジェクトで同じことをしたいという場合に自作できると便利だと思います。
* 余力があれば、```IQueryalble```版も```QueryableExtensions.cs```に作ってみてください。

---

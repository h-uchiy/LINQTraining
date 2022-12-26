using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LINQTraining;
using LINQTraining.Models;
using LINQTraining.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace LinqTraining_Answer
{
    /// <summary>
    /// 演習問題解答例
    /// </summary>
    public class Answers : IClassFixture<SetupFixture>, IDisposable
    {
        private readonly ITestOutputHelper _logger;
        private readonly SetupFixture _setupFixture;
        private readonly TrainingContext _context;

        public Answers(ITestOutputHelper logger, SetupFixture setupFixture)
        {
            _logger = logger;
            _setupFixture = setupFixture;
            _context = new TrainingContext();
        }

        /// <summary>
        /// 例題1
        /// </summary>
        [Fact]
        public async Task Exercise1()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);

            // Act
            var result = _logger.StopWatch(() => Exercise1_Act1(_context, "MetadataCode001"));
            result = await _logger.StopWatch(() => Exercise1_Act2(_context, "MetadataCode001")).ToListAsync();

            // Assert
            Assert.Equal(1000, result.Count);
            Assert.All(result.Zip(Enumerable.Range(1, result.Count)),
                x => Assert.Equal((x.Second * 100).ToString(), x.First.Value));
        }

        /// <remarks>
        /// ループを記述しないようにリファクタリング
        /// </remarks>
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

        /// <remarks>
        /// 必要最小限のデータのみをDBから取得し、最終的にデータが必要になるまでメモリにロードしないようにリファクタリング
        /// </remarks>
        private static IQueryable<Exercise1Result> Exercise1_Act2(TrainingContext context, string metadataCode)
        {
            return context.DataValues
                .Where(x => x.Metadata.Code == metadataCode)
                .Select(x => new Exercise1Result
                {
                    DataType = x.Metadata.DataType,
                    Value = x.Value
                });
        }
        
        /// <remarks>
        /// Joinで書き直し
        /// </remarks>
        private static IQueryable<Exercise1Result> Exercise1_Act3(TrainingContext context, string metadataCode)
        {
            return from dv in context.DataValues
                join m in context.Metadata on dv.MetadataId equals m.Id
                select new Exercise1Result
                {
                    DataType = m.DataType,
                    Value = dv.Value
                };
        }

        /// <summary>
        /// 例題2
        /// </summary>
        /// <param name="rowCount">Exercise2_Actに与えるdataTableとerrorsListの行数。（dataTableのすべての行にエラーが発生しているという想定）</param>
        [Theory]
        // [InlineData(1000)]
        [InlineData(1000)]
        [InlineData(2000)]
        [InlineData(10000)]
        public void Exercise2(int rowCount)
        {
            // Arrange
            var (dataTable, errorsList) = _setupFixture.Exercise2(rowCount);
            dataTable.Columns.Add(new DataColumn
            {
                ColumnName = "Error Column",
                DataType = typeof(string)
            });

            // Act
            _logger.StopWatch(() => Exercise2_Act1(dataTable, errorsList));
            _logger.StopWatch(() => Exercise2_Act2(dataTable, errorsList));

            // Assert
            var expectedErrorInfos = from error in errorsList
                group error by error.RowNo
                into g
                select new { RowNo = g.Key, ColumnName = string.Join(",", g.Select(y => y.ColumnName)) };
            Assert.All(expectedErrorInfos, errorInfo => Assert.Equal(errorInfo.ColumnName, dataTable.Rows[errorInfo.RowNo - 1]["Error Column"]));
        }

        /// <summary>
        /// 元の構造をあまり変えずに改善した例
        /// </summary>
        private static void Exercise2_Act1(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
        {
            var errorsLookup = errorsList.ToLookup(x => x.RowNo);
            foreach (var row in dataTable.Rows.OfType<DataRow>())
            {
                int rowNo = int.TryParse(row["Row No"].ToString(), out rowNo) ? rowNo : -1;
                var errors = errorsLookup[rowNo];
                row["Error Column"] = string.Join(",", errors.Select(x => x.ColumnName));
            }
        }

        /// <summary>
        /// 宣言的に書き直した例
        /// </summary>
        private static void Exercise2_Act2(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
        {
            var joined = from row in dataTable.Rows.OfType<DataRow>()
                let rowNo = int.TryParse(row["Row No"].ToString(), out var rowNo) ? rowNo : -1
                join error in errorsList on rowNo equals error.RowNo into gj
                let errorColumns = string.Join(",", gj.Select(x => x.ColumnName))
                select new { row, errorColumns };
            foreach (var x in joined)
            {
                x.row["Error Column"] = x.errorColumns;
            }
        }

        /// <summary>
        /// CodeAとCodeBの組み合わせが格納されたテーブルMappingsをインポートします。
        /// ・コードの組み合わせは[Code1][SPACE][Code2]とします。（Codeには空白が含まれない）
        /// ・重複する組み合わせをduplicatedCodesに格納します。
        /// ・uniqueとなる組み合わせをcodesに格納します。
        /// </summary>
        /// <remarks>
        /// ・Mappingsテーブルの行数(size)が大きい場合、このメソッドの実行にはとても時間がかかります。
        /// 　遅い理由を説明し、size=10万の場合でも1秒以内でで完了するように改善してください。
        /// </remarks>
        [Theory]
        [InlineData(1000)]
        // [InlineData(100000)]
        public async Task Exercise3(int size)
        {
            // Arrange
            await _setupFixture.GenerateMappings(_context, size);

            // Act
            var (codes, duplicatedCodes) = await Exercise3_Act1(_context);
            // var (codes, duplicatedCodes) = Answers.Exercise3_Act2(_context);

            // Assert
            Assert.Equal(Enumerable.Distinct<string>(codes).Count(), Enumerable.Count<string>(codes));
            Assert.Equal(Enumerable.Distinct<string>(duplicatedCodes).Count(), Enumerable.Count<string>(duplicatedCodes));
        }

        private static async Task<(ICollection<string> codes, ICollection<string> duplicatedCodes)> Exercise3_Act(TrainingContext context)
        {
            var codes = new List<string>();
            var duplicatedCodes = new List<string>();
            await foreach (var map in context.Mappings.AsAsyncEnumerable())
            {
                var key = map.CodeA + " " + map.CodeB;
                if (codes.Any(x => x == key))
                {
                    if (duplicatedCodes.All(x => x != key))
                    {
                        duplicatedCodes.Add(key);
                    }
                }
                else
                {
                    codes.Add(key);
                }
            }

            return (codes, duplicatedCodes);
        }

        /// <summary>
        /// 複数のAttributeに対するAttributeValueの値を取得します。
        /// 対象となるAttributeのCodeは動的に決定するものとします。
        /// </summary>
        /// <remarks>
        /// ・このコードはコンパイルは通りますが動作しません。動作しない理由を説明してください。
        /// ・上記のエラーを修正して動作するようにしてください。modelsはIEnumerableでも構いません。
        /// ・modelsがIQueryableになるように修正してください。
        /// ・それぞれ実行されるSQLの違いについて説明してください。
        /// </remarks>
        [Fact]
        public async Task Exercise4()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetSomeMetadataCodes();

            // Act
            var models = await Exercise4_Act1(_context, metadataCodes);
            // var models = Answers.Exercise4_Act2(_context, metadataCodes);

            // Assert
            Assert.All(metadataCodes, code =>
                Assert.Throws<InvalidOperationException>(() =>
                    Assert.Contains<Exercise6Result>(models, x => x.MetadataCode == code)
                ));
        }

        private static IQueryable<Exercise6Result> Exercise4_Act(TrainingContext context, IEnumerable<string> metadataCodes)
        {
            return from av in context.DataValues
                    .Include(x => x.Metadata)
                join ac in metadataCodes on av.Metadata.Code equals ac
                select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        [Fact]
        public async Task Exercise5()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetDataCategoryCode();

            // Act
            var models = Exercise5_Act1(_context, metadataCodes);

            // Assert
        }

        private static IEnumerable<Exercise7Result> Exercise5_Act(TrainingContext context, string dataCategoryCodes)
        {
            var results = new List<Exercise7Result>();
            foreach (var dataCategory in context.DataCategory.Where(x => x.Code == dataCategoryCodes))
            {
                if (dataCategory.MetadataDataCategory.Any())
                {
                    foreach (var metadataDataCategory in dataCategory.MetadataDataCategory)
                    {
                        results.Add(new Exercise7Result
                        {
                            DataCategoryName = dataCategory.Name,
                            MetadataName = metadataDataCategory.Metadata.Name,
                        });
                    }
                }
                else
                {
                    results.Add(new Exercise7Result
                    {
                        DataCategoryName = dataCategory.Name,
                        MetadataName = null,
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 元のコードをあまり変更せずに性能のみ改善した例
        /// </summary>
        public static async Task<(ICollection<string> codeList, ICollection<string> duplicatedCodes)> Exercise3_Act1(TrainingContext context)
        {
            var codeList = new HashSet<string>();
            var duplicateCodeList = new HashSet<string>();
            await foreach (var map in context.Mappings.AsAsyncEnumerable())
            {
                var key = map.CodeA + " " + map.CodeB;
                // LINQ to ObjectのWhere式は線形探索なので母集団が大きくなると非常に遅くなる
                // これをハッシュテーブルによる探索に置き換える
                // 母集団が小さい場合はBinary Treeによる探索（SortedSet）を使用する
                if (codeList.Contains(key))
                {
                    duplicateCodeList.Add(key);
                }
                else
                {
                    codeList.Add(key);
                }
            }

            return (codeList, duplicateCodeList);
        }

        /// <summary>
        /// 宣言的な記述に置き換えて、サーバー側で処理を完結させた例
        /// </summary>
        public static (IQueryable<string> codeList, IQueryable<string> duplicatedCodes) Exercise3_Act2(TrainingContext context)
        {
            var keys = context.Mappings.Select(map => map.CodeA + " " + map.CodeB);
            var codeList = keys.Distinct();
            var duplicateCodeList = from key in keys
                group key by key
                into g
                where g.Count() > 1
                select g.Key;
            return (codeList, duplicateCodeList);
        }

        /// <summary>
        /// （良くない例）すべてメモリにロードして演算する実装
        /// Includeを使うと必要のないプロパティまで全部ロードされるので効率が良くない
        /// </summary>
        public static async Task<IEnumerable<Exercise6Result>> Exercise4_Act1(TrainingContext context, IEnumerable<string> metadataCodes)
        {
            return from av in await context.DataValues
                    .Include(x => x.Metadata)
                    .ToListAsync()
                join ac in metadataCodes on av.Metadata.Code equals ac
                select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        /// <summary>
        /// DB側ですべて演算を行い、最終的に必要なデータのみをメモリにロードする実装
        /// LINQ to Entityの式の中で参照する分にはIncludeを記述する必要はない
        /// </summary>
        public static IQueryable<Exercise6Result> Exercise4_Act2(TrainingContext context,
            ICollection<string> metadataCodes)
        {
            return from av in context.DataValues
                // メモリ上の値による絞り込みを行いたい場合はICollection.Contains()を使用するとSQLに変換できる
                where metadataCodes.Contains(av.Metadata.Code)
                select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        /// <summary>
        /// 
        /// </summary>
        public static IQueryable<Exercise7Result> Exercise5_Act1(TrainingContext context, string dataCategoryCodes)
        {
            return from dataCategory in context.DataCategory
                where dataCategory.Code == dataCategoryCodes
                from metadataDataCategory in dataCategory.MetadataDataCategory.DefaultIfEmpty()
                select new Exercise7Result
                {
                    DataCategoryName = dataCategory.Name,
                    MetadataName = metadataDataCategory.Metadata.Name,
                };
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
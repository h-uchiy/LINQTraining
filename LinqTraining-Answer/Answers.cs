using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
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
            var results = new[]
            {
                _logger.StopWatch(() => Exercises.Exercise1_Act(_context, "MetadataCode001")),
                _logger.StopWatch(() => Exercise1_Act1(_context, "MetadataCode001")),
                await _logger.StopWatchAsync(() => Exercise1_Act2(_context, "MetadataCode001").ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(() => Exercise1_Act3(_context, "MetadataCode001").ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(() => Exercise1_Act4(_context, "MetadataCode001").ToListAsync(CancellationToken.None))
            };

            // Assert
            foreach (var result in results)
            {
                Assert.Equal(1000, result.Count);
                Assert.All(
                    result.Select((x, idx) => new { x.Value, idx }),
                    x => Assert.Equal(((x.idx + 1) * 100).ToString(), x.Value));
            }
        }

        /// <remarks>
        /// ループを記述しないようにリファクタリング
        /// </remarks>
        private static List<Exercise1Result> Exercise1_Act1(TrainingContext context, string metadataCode)
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
        
        /// <summary>
        /// クエリ式表現で書き直し。Exercise1_Act2と全く同じものです。
        /// </summary>
        private static IQueryable<Exercise1Result> Exercise1_Act3(TrainingContext context, string metadataCode)
        {
            return from dv in context.DataValues
                where dv.Metadata.Code == metadataCode
                select new Exercise1Result
                {
                    DataType = dv.Metadata.DataType,
                    Value = dv.Value
                };
        }
        
        /// <remarks>
        /// Joinで書き直し
        /// </remarks>
        private static IQueryable<Exercise1Result> Exercise1_Act4(TrainingContext context, string metadataCode)
        {
            return from dv in context.DataValues
                join m in context.Metadata on dv.MetadataId equals m.Id
                where dv.Metadata.Code == metadataCode
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
        [InlineData(1000, 1)]
        [InlineData(2000, 2)]
        [InlineData(10000, 3)]
        public void Exercise2(int rowCount, int act)
        {
            // Arrange
            var (dataTable, errorsList) = _setupFixture.Exercise2(rowCount);
            dataTable.Columns.Add(new DataColumn
            {
                ColumnName = "Error Column",
                DataType = typeof(string)
            });

            // Act
            switch (act)
            {
                case 1:
                    _logger.StopWatch(() => Exercises.Exercise2_Act(dataTable, errorsList));
                    break;
                case 2:
                    _logger.StopWatch(() => Exercise2_Act1(dataTable, errorsList));
                    break;
                case 3:
                    _logger.StopWatch(() => Exercise2_Act2(dataTable, errorsList));
                    break;
            }

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
        /// 例題5
        /// </summary>
        [Theory]
        [InlineData(1000)]
        // [InlineData(100000)]
        public async Task Exercise5(int size)
        {
            // Arrange
            await _setupFixture.GenerateMappings(_context, size);

            // Act
            var results = new[]
            {
                await _logger.StopWatch(() => Exercises.Exercise5_Act(_context)),
                await _logger.StopWatch(() => Exercise5_Act1(_context)),
                await _logger.StopWatch(() => Exercise5_Act2(_context)),
            };

            // Assert
            foreach (var result in results)
            {
                Assert.Equal(result.Codes.Distinct().Count(), result.Codes.Count());
                Assert.Equal(result.DuplicatedCodes.Distinct().Count(), result.DuplicatedCodes.Count());
            }
        }

        /// <summary>
        /// 元のコードをあまり変更せずに性能のみ改善した例
        /// </summary>
        public static async Task<Exercise5Result> Exercise5_Act1(TrainingContext context)
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

            return new Exercise5Result(codeList, duplicateCodeList);
        }

        /// <summary>
        /// 宣言的な記述に置き換えて、サーバー側で処理を完結させた例
        /// </summary>
        public static async Task<Exercise5Result> Exercise5_Act2(TrainingContext context)
        {
            var keys = context.Mappings.Select(map => map.CodeA + " " + map.CodeB);
            var codeList = keys.Distinct();
            var duplicateCodeList = from key in keys
                group key by key
                into g
                where g.Count() > 1
                select g.Key;
            return new Exercise5Result(await codeList.ToListAsync(), await duplicateCodeList.ToListAsync());
        }

        /// <summary>
        /// 例題6
        /// </summary>
        [Fact]
        public async Task Exercise6()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetSomeMetadataCodes();

            // Act
            var results = new[]
            {
                // Exercises.Exercise6_Act(_context, metadataCodes), // this does not work
                await Exercise6_Act1(_context, metadataCodes),
                await Exercise6_Act2(_context, metadataCodes).ToListAsync()
            };

            // Assert
            foreach (var result in results)
            {
                Assert.All(metadataCodes, code => Assert.Contains(result, x => x.MetadataCode == code));
            }
        }

        /// <summary>
        /// （良くない例）すべてメモリにロードして演算する実装
        /// Includeを使うと必要のないプロパティまで全部ロードされるので効率が良くない
        /// </summary>
        public static async Task<IEnumerable<Exercise6Result>> Exercise6_Act1(TrainingContext context, IEnumerable<string> metadataCodes)
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
        public static IQueryable<Exercise6Result> Exercise6_Act2(TrainingContext context,
            ICollection<string> metadataCodes)
        {
            return from av in context.DataValues
                // メモリ上の値による絞り込みを行いたい場合はICollection.Contains()を使用するとSQLに変換できる
                where metadataCodes.Contains(av.Metadata.Code)
                select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        [Fact]
        public async Task Exercise7()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetDataCategoryCode();

            // Act
            var result = new[]
            {
                Exercises.Exercise7_Act(_context, metadataCodes),
                Exercise7_Act1(_context, metadataCodes)
            };

            // Assert
        }

        public static IQueryable<Exercise7Result> Exercise7_Act1(TrainingContext context, string dataCategoryCodes)
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

        [Theory]
        [InlineData("MetadataCode012")]
        public async Task Exercise8(string metadataCode)
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadata = await _context.Metadata.FirstAsync(x => x.Code == metadataCode);
            
            // Act
            var results = new[]
            {
                await Exercises.Exercise8_Act(_context, metadata).ToListAsync(),
                await Exercise8_Act(_context, metadata).ToListAsync()
            };
            
            // Assert
            foreach (var result in results)
            {
                Assert.All(result, columnValue => Assert.Equal("ColumnValue012", columnValue));
            }
        }

        private static IQueryable<string> Exercise8_Act(TrainingContext context, Metadata metadata)
        {
            // dataValue => dataValue.[Column{metadata.ColumnIndex:D3}]
            var dataValueExpr = Expression.Parameter(typeof(string), "dataValue");
            var selector = Expression.Lambda<Func<DataValue, string>>(
                Expression.Property(dataValueExpr, $"Column{metadata.ColumnIndex:D3}"),
                dataValueExpr
            );
            
            return context.DataValues
                .Where(x => x.MetadataId == metadata.Id)
                .Select(selector);
        }

        [Theory]
        [InlineData(123)]
        public async Task Exercise9(int dataValueId)
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var dataValue = await _context.DataValues.Include(x => x.Metadata).FirstAsync(x => x.Id == dataValueId);
            var expected = Enumerable.Range(0, 20).Select(idx => $"{dataValue.Metadata.CandidateList[^1]}{idx}").OrderBy(x => x).ToList();
            
            // Act
            var result = Exercises.Exercise9_Act(_context, dataValue);
            
            // Assert
            Assert.Equal(expected, await result.OrderBy(x => x).ToListAsync());
        }

        private static IQueryable<string> Exercise9_Act(TrainingContext context, DataValue dataValue)
        {
            switch (dataValue.Metadata.CandidateList)
            {
                case "CandidateListA":
                    return context.CandidateListA.Select(x => x.Value);
                case "CandidateListB":
                    return context.CandidateListB.Select(x => x.Value);
                case "CandidateListC":
                    return context.CandidateListC.Select(x => x.Value);
                default:
                    throw new IndexOutOfRangeException();
            }
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
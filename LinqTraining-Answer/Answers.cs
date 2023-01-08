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
        /// 演習1
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
                await _logger.StopWatchAsync(() =>
                    Exercise1_Act2(_context, "MetadataCode001").ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(() =>
                    Exercise1_Act3(_context, "MetadataCode001").ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(() =>
                    Exercise1_Act4(_context, "MetadataCode001").ToListAsync(CancellationToken.None))
            };

            // Assert
            var result = results[0];
            Assert.Equal(1000, result.Count);
            Assert.All(
                result.Select((x, idx) => new { x.Value, idx }),
                x => Assert.Equal(((x.idx + 1) * 100).ToString(), x.Value));

            Assert.Equal(results[0], results[1], Exercise1Result.Comparer);
            Assert.Equal(results[0], results[2], Exercise1Result.Comparer);
            Assert.Equal(results[0], results[3], Exercise1Result.Comparer);
            Assert.Equal(results[0], results[4], Exercise1Result.Comparer);
        }

        /// <remarks>
        /// ループを記述しないようにリファクタリング
        /// </remarks>
        private static List<Exercise1Result> Exercise1_Act1(TrainingContext context, string metadataCode) =>
            context.DataValues
                .Include(x => x.Metadata)
                .Where(x => x.Metadata.Code == metadataCode)
                .ToList()
                .Select(metadataValue => new Exercise1Result
                {
                    DataType = metadataValue.Metadata.DataType,
                    Value = metadataValue.Value
                })
                .ToList();

        /// <remarks>
        /// 必要最小限のデータのみをDBから取得し、最終的にデータが必要になるまでメモリにロードしないようにリファクタリング
        /// </remarks>
        private static IQueryable<Exercise1Result> Exercise1_Act2(TrainingContext context, string metadataCode) =>
            context.DataValues
                .Where(x => x.Metadata.Code == metadataCode)
                .Select(x => new Exercise1Result
                {
                    DataType = x.Metadata.DataType,
                    Value = x.Value
                });

        /// <summary>
        /// クエリ式表現で書き直し。Exercise1_Act2と全く同じものです。
        /// </summary>
        private static IQueryable<Exercise1Result> Exercise1_Act3(TrainingContext context, string metadataCode) =>
            from dv in context.DataValues
            where dv.Metadata.Code == metadataCode
            select new Exercise1Result
            {
                DataType = dv.Metadata.DataType,
                Value = dv.Value
            };

        /// <remarks>
        /// Joinで書き直し
        /// </remarks>
        private static IQueryable<Exercise1Result> Exercise1_Act4(TrainingContext context, string metadataCode) =>
            from dv in context.DataValues
            join m in context.Metadata on dv.MetadataId equals m.Id
            where dv.Metadata.Code == metadataCode
            select new Exercise1Result
            {
                DataType = m.DataType,
                Value = dv.Value
            };

        /// <summary>
        /// 演習2
        /// </summary>
        /// <param name="rowCount">Exercise2_Actに与えるdataTableとerrorsListの行数。（dataTableのすべての行にエラーが発生しているという想定）</param>
        [Theory]
        [InlineData(1000, 1)]
        [InlineData(100000, 2)]
        [InlineData(100000, 3)]
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
            Assert.All(expectedErrorInfos,
                errorInfo => Assert.Equal(errorInfo.ColumnName, dataTable.Rows[errorInfo.RowNo - 1]["Error Column"]));
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

        [Fact]
        public async Task Exercise3_ToSortedSet()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = await _context.Metadata.Select(x => new MetadataPart(x.Code, x.Name, x.DataType)).ToListAsync();

            // Act
            var result = src.ToSortedSet(MetadataPart.RelationalComparer);

            // Assert
            src.Sort(MetadataPart.RelationalComparer);
            Assert.Equal(src, result);
        }

        [Fact]
        public async Task Exercise3_ToSortedSetAsync()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new MetadataPart(x.Code, x.Name, x.DataType));
            var expected = await src.ToListAsync();
            expected.Sort(MetadataPart.RelationalComparer);

            // Act
            var result = await src.ToSortedSetAsync(MetadataPart.RelationalComparer);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public async Task Exercise3_ToSortedList()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = await _context.Metadata.Select(x => new MetadataPart(x.Code, x.Name, x.DataType)).ToListAsync();
            var expected = src.OrderBy(x => x.Code);

            // Act
            var result = src.ToSortedList(x => x.Code);

            // Assert
            Assert.Equal(expected, result.Select(x => x.Value));
        }

        [Fact]
        public async Task Exercise3_ToSortedListAsync()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new MetadataPart(x.Code, x.Name, x.DataType));
            var expected = await _context.Metadata.OrderBy(x => x.Code)
                .Select(x => new MetadataPart(x.Code, x.Name, x.DataType))
                .ToListAsync();

            // Act
            var result = await src.ToSortedListAsync(x => x.Code);

            // Assert
            Assert.Equal(expected, result.Select(x => x.Value));
        }

        [Fact]
        public void Exercise4_DistinctBy()
        {
            // Arrange
            var src = new[]
            {
                new { Key = 1, Value = "a" },
                new { Key = 0, Value = "b" },
                new { Key = 3, Value = "c" },
                new { Key = 6, Value = "d" },
                new { Key = 4, Value = "e" },
                new { Key = 3, Value = "f" },
                new { Key = 2, Value = "g" },
                new { Key = 9, Value = "h" },
                new { Key = 4, Value = "i" },
                new { Key = 8, Value = "j" },
                new { Key = 7, Value = "k" },
                new { Key = 8, Value = "l" },
                new { Key = 5, Value = "m" },
            };
            var expected = new[]
            {
                new { Key = 1, Value = "a" },
                new { Key = 0, Value = "b" },
                new { Key = 3, Value = "c" },
                new { Key = 6, Value = "d" },
                new { Key = 4, Value = "e" },
                new { Key = 2, Value = "g" },
                new { Key = 9, Value = "h" },
                new { Key = 8, Value = "j" },
                new { Key = 7, Value = "k" },
                new { Key = 5, Value = "m" },
            };

            // Act
            var result = src.DistinctBy(x => x.Key).ToList();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Exercise4_Chunk()
        {
            // Arrange
            var src = Enumerable.Range(1, 100).ToList();

            // Act
            var result = src.Chunk(30).ToList();

            // Assert
            Assert.Equal(4, result.Count);
            Assert.Equal(30, result[0].Length);
            Assert.Equal(30, result[1].Length);
            Assert.Equal(30, result[2].Length);
            Assert.Equal(30, result[3].Length);
            Assert.Equal(Enumerable.Range(1, 30), result[0]);
            Assert.Equal(Enumerable.Range(31, 30), result[1]);
            Assert.Equal(Enumerable.Range(61, 30), result[2]);
            Assert.Equal(Enumerable.Range(91, 10), result[3]);
        }

        /// <summary>
        /// 演習5
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
                await _logger.StopWatchAsync(() => Exercises.Exercise5_Act(_context)),
                await _logger.StopWatchAsync(() => Exercise5_Act1(_context)),
                await _logger.StopWatchAsync(() => Exercise5_Act2(_context)),
            };

            // Assert
            foreach (var result in results)
            {
                Assert.Equal(result.Codes.Distinct().Count(), result.Codes.Count());
                Assert.Equal(result.DuplicatedCodes.Distinct().Count(), result.DuplicatedCodes.Count());
            }

            for (var i = 1; i < results.Length; i++)
            {
                Assert.Equal(results[0].Codes, results[i].Codes);
                Assert.Equal(results[0].DuplicatedCodes, results[i].DuplicatedCodes);
            }
        }

        /// <summary>
        /// 元のコードをあまり変更せずに性能のみ改善した例
        /// </summary>
        public static async Task<Exercise5Result> Exercise5_Act1(TrainingContext context)
        {
            // LINQ to ObjectのWhere式は線形探索なので母集団が大きくなると非常に遅くなる
            // これをハッシュテーブルによる探索（HashSet）に置き換える
            // Add, Insertなどの更新操作が少ない場合はBinary Treeによる探索（SortedSet）を使用する
            var codeList = new HashSet<string>();
            var duplicateCodeList = new HashSet<string>();
            await foreach (var map in context.Mappings.AsAsyncEnumerable())
            {
                var key = map.CodeA + " " + map.CodeB;
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
        /// 演習6
        /// </summary>
        [Fact]
        public async Task Exercise6()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetSomeMetadataCodes();

            // Act
            await Assert.ThrowsAsync<InvalidOperationException>(() => 
                Exercises.Exercise6_Act(_context, metadataCodes).ToListAsync());
            var results = new[]
            {
                // await _logger.StopWatchAsync(() =>
                //     Exercise6_Act1(_context, metadataCodes))
                //     .ContinueWith(antecedent => antecedent.Result.ToList()), // this is very slow
                await _logger.StopWatchAsync(() =>
                    Exercise6_Act2(_context, metadataCodes).ToListAsync(CancellationToken.None))
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
        // ReSharper disable once UnusedMember.Global
        public static async Task<IEnumerable<Exercise6Result>> Exercise6_Act1(
            TrainingContext context, IEnumerable<string> metadataCodes) =>
            from av in await context.DataValues
                .Include(x => x.Metadata)
                .ToListAsync()
            join ac in metadataCodes on av.Metadata.Code equals ac
            select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };

        /// <summary>
        /// DB側ですべて演算を行い、最終的に必要なデータのみをメモリにロードする実装
        /// LINQ to Entityの式の中で参照する分にはIncludeを記述する必要はない
        /// </summary>
        public static IQueryable<Exercise6Result> Exercise6_Act2(
            TrainingContext context, ICollection<string> metadataCodes) =>
            from av in context.DataValues
            // メモリ上の値による絞り込みを行いたい場合はICollection.Contains()を使用するとSQLに変換できる
            where metadataCodes.Contains(av.Metadata.Code)
            select new Exercise6Result { MetadataCode = av.Metadata.Code, Value = av.Value };

        [Fact]
        public async Task Exercise7()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetDataCategoryCode();

            // Act
            var results = new[]
            {
                _logger.StopWatch(() => Exercises.Exercise7_Act(_context, metadataCodes).ToList()),
                await _logger.StopWatchAsync(() =>
                    Exercise7_Act1(_context, metadataCodes).ToListAsync(CancellationToken.None))
            };

            // Assert
            Assert.Equal(results[0], results[1], Exercise7Result.Comparer);
        }

        public static IQueryable<Exercise7Result> Exercise7_Act1(
            TrainingContext context, string dataCategoryCodes) =>
            from dataCategory in context.DataCategory
            where dataCategory.Code == dataCategoryCodes
            from metadataDataCategory in dataCategory.MetadataDataCategory.DefaultIfEmpty()
            select new Exercise7Result
            {
                DataCategoryName = dataCategory.Name,
                MetadataName = metadataDataCategory.Metadata.Name,
            };

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
                await _logger.StopWatchAsync(() =>
                    Exercises.Exercise8_Act(_context, metadata).ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(
                    () => Exercise8_Act(_context, metadata).ToListAsync(CancellationToken.None))
            };

            // Assert
            Assert.Equal(results[0], results[1]);
        }

        private static IQueryable<string> Exercise8_Act(TrainingContext context, Metadata metadata)
        {
            var xExpr = Expression.Parameter(typeof(DataValue), "x");
            // x => x.$"Column{metadata.ColumnIndex:D3}"
            var selector = Expression.Lambda<Func<DataValue, string>>(
                Expression.Property(xExpr, $"Column{metadata.ColumnIndex:D3}"),
                xExpr
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

            // Act
            var results = new[]
            {
                await _logger.StopWatchAsync(() =>
                    Exercises.Exercise9_Act(_context, dataValue).ToListAsync(CancellationToken.None)),
                await _logger.StopWatchAsync(() =>
                    Exercise9_Act(_context, dataValue).ToListAsync(CancellationToken.None))
            };

            // Assert
            Assert.Equal(results[0], results[1]);
        }

        private static IQueryable<string> Exercise9_Act(TrainingContext context, DataValue dataValue)
        {
            var ns = typeof(TrainingContext).Namespace;
            var candidateList = dataValue.Metadata.CandidateList;
            var candidateListType = typeof(TrainingContext).Assembly.GetType($"{ns}.{candidateList}") ??
                                    throw new InvalidOperationException();
            var xExpr = Expression.Parameter(candidateListType, "x");
            // x => x.Value
            var selector = Expression.Lambda(Expression.Property(xExpr, "Value"), xExpr);
            // () => Queryable.Select(context.Set<candidateListType>(), selector);
            var lambda = Expression.Lambda<Func<IQueryable<string>>>(
                Expression.Call(
                    typeof(Queryable),
                    nameof(Queryable.Select),
                    new[] { candidateListType, typeof(string) },
                    Expression.Call(
                        Expression.Constant(context),
                        nameof(TrainingContext.Set),
                        new[] { candidateListType }),
                    Expression.Constant(selector)));
            return lambda.Compile().Invoke();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public class MetadataPart
        {
            private sealed class MetadataPartEqualityComparer : IEqualityComparer<MetadataPart>
            {
                public bool Equals(MetadataPart? x, MetadataPart? y)
                {
                    if (ReferenceEquals(x, y)) return true;
                    if (ReferenceEquals(x, null)) return false;
                    if (ReferenceEquals(y, null)) return false;
                    if (x.GetType() != y.GetType()) return false;
                    return x.Code == y.Code && x.Name == y.Name && x.DataType == y.DataType;
                }

                public int GetHashCode(MetadataPart obj)
                {
                    return HashCode.Combine(obj.Code, obj.Name, (int)obj.DataType);
                }
            }

            public static IEqualityComparer<MetadataPart> EqualityComparer { get; } =
                new MetadataPartEqualityComparer();

            private sealed class MetadataPartRelationalComparer : IComparer<MetadataPart>
            {
                public int Compare(MetadataPart? x, MetadataPart? y)
                {
                    if (ReferenceEquals(x, y)) return 0;
                    if (ReferenceEquals(null, y)) return 1;
                    if (ReferenceEquals(null, x)) return -1;
                    var codeComparison = string.Compare(x.Code, y.Code, StringComparison.Ordinal);
                    if (codeComparison != 0) return codeComparison;
                    var nameComparison = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
                    if (nameComparison != 0) return nameComparison;
                    return x.DataType.CompareTo(y.DataType);
                }
            }

            public static IComparer<MetadataPart> RelationalComparer { get; } = new MetadataPartRelationalComparer();

            public string Code { get; }
            public string Name { get; }
            public DataType DataType { get; }

            public MetadataPart(string code, string name, DataType dataType)
            {
                Code = code;
                Name = name;
                DataType = dataType;
            }

            public override string ToString()
            {
                return $"{{ Code = {Code}, Name = {Name}, DataType = {DataType} }}";
            }

            public override bool Equals(object? value)
            {
                return value is MetadataPart other
                       && EqualityComparer<string>.Default.Equals(other.Code, Code)
                       && EqualityComparer<string>.Default.Equals(other.Name, Name)
                       && EqualityComparer<DataType>.Default.Equals(other.DataType, DataType);
            }

            public override int GetHashCode()
            {
                var hash = 0x7a2f0b42;
                hash = (-1521134295 * hash) + EqualityComparer<string>.Default.GetHashCode(Code);
                hash = (-1521134295 * hash) + EqualityComparer<string>.Default.GetHashCode(Name);
                return (-1521134295 * hash) + EqualityComparer<DataType>.Default.GetHashCode(DataType);
            }
        }
    }
}
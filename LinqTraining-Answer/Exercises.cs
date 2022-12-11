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
    /// 演習問題
    /// </summary>
    public class Exercises : IClassFixture<SetupFixture>, IDisposable
    {
        private readonly ITestOutputHelper _logger;
        private readonly SetupFixture _setupFixture;
        private readonly TrainingContext _context;

        public Exercises(ITestOutputHelper logger, SetupFixture setupFixture)
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
            // var result = Exercise1_Act(_context, "MetadataCode001");
            var result = Answers.Exercise1_Act1(_context, "MetadataCode001");
            // var result = await Answers.Exercise1_Act2(_context, "MetadataCode001").ToListAsync();

            // Assert
            Assert.Equal(1000, result.Count);
            Assert.All(result.Zip(Enumerable.Range(1, result.Count)),
                x => Assert.Equal((x.Second * 100).ToString(), x.First.Value));
        }

        /// <summary>
        /// DataValues.Metadata.Code == metadataCode となるDataValuesから、DataTypeとValueを取得します。
        /// </summary>
        /// <remarks>
        /// 1. ループを記述しないようにリファクタリングしてください。
        /// 2. 最終的に必要なデータ以外の列をDBから取得しないようにリファクタリングしてください。
        /// 3. データをメモリに展開しないようにリファクタリングしてください。
        /// </remarks>
        private static List<Exercise1Result> Exercise1_Act(TrainingContext context, string metadataCode)
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

        /// <summary>
        /// 演習2
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
            _logger.StopWatch(() => Exercise2_Act(dataTable, errorsList));
            // Answers.Exercise2_Act1(dataTable, errorsList);
            // Answers.Exercise2_Act2(dataTable, errorsList);

            // Assert
            var expectedErrorInfos = from error in errorsList
                group error by error.RowNo
                into g
                select new { RowNo = g.Key, ColumnName = string.Join(",", g.Select(y => y.ColumnName)) };
            Assert.All(expectedErrorInfos, errorInfo => Assert.Equal(errorInfo.ColumnName, dataTable.Rows[errorInfo.RowNo - 1]["Error Column"]));
        }

        /// <summary>
        /// ・dataTableには、1で始まる行番号を入れた'Row No'という列のほか、いろいろな列があります。
        /// ・errorsListには、エラーがある行の番号と、列の名前が入っています。
        /// 　　・同じ行に複数のエラーが発生していることがあります。
        /// 　　・Setup.Exercise2は、行数と同じ数のエラーを生成します。
        /// ・errorsListのエラー情報を、dataTableに'Error Column'という列を追加して書き込みます。
        ///     ・エラーが発生している列名を、'Error Column'列に書き込みます。
        ///     ・複数のエラーがある場合は、カンマ区切りで'Error Column'列に書き込みます。
        /// </summary>
        /// <remarks>
        /// ・Multiple Iterationを解消してください。
        /// ・dataTableとerrorsListの行数を増やすと、増やした分だけの時間がかかるのではなく、それ以上に遅くなります。（概ね2倍にすると4倍、10倍にすると100倍）なぜこのようになるのか説明してください。
        /// ・行数を増やしても実行時間があまり遅くならないように改善してください。
        /// </remarks>
        private static void Exercise2_Act(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
        {
            foreach (var row in dataTable.Rows.OfType<DataRow>())
            {
                var errors = errorsList.Where(y =>
                    int.TryParse(row["Row No"].ToString(), out var rowNo) &&
                    y.RowNo == rowNo);
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
            var (codes, duplicatedCodes) = await Exercise3_Act(_context);
            // var (codes, duplicatedCodes) = await Answers.Exercise3_Act1(_context);
            // var (codes, duplicatedCodes) = Answers.Exercise3_Act2(_context);

            // Assert
            Assert.Equal(codes.Distinct().Count(), codes.Count());
            Assert.Equal(duplicatedCodes.Distinct().Count(), duplicatedCodes.Count());
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
            var models = Exercise4_Act(_context, metadataCodes);
            // var models = await Answers.Exercise4_Act1(_context, metadataCodes);
            // var models = Answers.Exercise4_Act2(_context, metadataCodes);

            // Assert
            Assert.All(metadataCodes, code =>
                Assert.Throws<InvalidOperationException>(() =>
                    Assert.Contains(models, x => x.MetadataCode == code)
                ));
        }

        private static IQueryable<Exercise4Result> Exercise4_Act(TrainingContext context, IEnumerable<string> metadataCodes)
        {
            return from av in context.DataValues
                    .Include(x => x.Metadata)
                join ac in metadataCodes on av.Metadata.Code equals ac
                select new Exercise4Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        [Fact]
        public async Task Exercise5()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetDataCategoryCode();

            // Act
            var models = Exercise5_Act(_context, metadataCodes);
            // var models2 = Answers.Exercise5_Act1(_context, metadataCodes);

            // Assert
        }

        private static IEnumerable<Exercise5Result> Exercise5_Act(TrainingContext context, string dataCategoryCodes)
        {
            var results = new List<Exercise5Result>();
            foreach (var dataCategory in context.DataCategory.Where(x => x.Code == dataCategoryCodes))
            {
                if (dataCategory.MetadataDataCategory.Any())
                {
                    foreach (var metadataDataCategory in dataCategory.MetadataDataCategory)
                    {
                        results.Add(new Exercise5Result
                        {
                            DataCategoryName = dataCategory.Name,
                            MetadataName = metadataDataCategory.Metadata.Name,
                        });
                    }
                }
                else
                {
                    results.Add(new Exercise5Result
                    {
                        DataCategoryName = dataCategory.Name,
                        MetadataName = null,
                    });
                }
            }

            return results;
        }

        /// <summary>
        /// 標準クエリ演算子<see cref="System.Linq.Enumerable.Distinct"/>には、OrderByやGroupBy,ToDictionaryのようにキーを指定する機能が無いので不便です。
        /// キーを指定できるバージョンを作成してください。
        /// </summary>
        [Fact]
        public async Task DistinctBy()
        {
            
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using LINQTraining.Models;
using LINQTraining.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace LINQTraining
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
            var result = Exercise1_Act(_context, "MetadataCode001");

            // Assert
            Assert.Equal(1000, result.Count);
            Assert.All(result.Zip(Enumerable.Range(1, result.Count)),
                x => Assert.Equal((x.Second * 100).ToString(), x.First.Value));
        }

        /// <summary>
        /// DataValues.Metadata.Code == metadataCode となるDataValuesから、DataTypeとValueを取得します。
        /// </summary>
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
        [InlineData(1000)]
        // [InlineData(100000)]
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
        /// ・dataTableとerrorsListの行数を増やすと、増やした分だけの時間がかかるのではなく、それ以上に遅くなります。（概ね2倍にすると4倍、10倍にすると100倍）なぜこのようになるのか考えて、これを解消してください。
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

        [Fact]
        public async Task Exercise3_ToSortedSet()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = await _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType }).ToListAsync();
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => src.ToSortedSet());
        }

        [Fact]
        public async Task Exercise3_ToSortedSetAsync()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType });
            
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => src.ToSortedSetAsync());
        }

        [Fact]
        public async Task Exercise3_ToSortedList()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = await _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType }).ToListAsync();
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => src.ToSortedList(x => x.Code));
        }

        [Fact]
        public async Task Exercise3_ToSortedListAsync()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType });
            
            // Act & Assert
            await Assert.ThrowsAsync<NotImplementedException>(() => src.ToSortedListAsync(x => x.Code));
        }

        [Fact]
        public async Task Exercise4_DistinctBy()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType });
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => src.DistinctBy(x => x.Code));
        }

        [Fact]
        public async Task Exercise4_Chunk()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var src = _context.Metadata.Select(x => new { x.Code, x.Name, x.DataType });
            
            // Act & Assert
            Assert.Throws<NotImplementedException>(() => src.Chunk(30));
        }

        /// <summary>
        /// 演習5
        /// </summary>
        /// <param name="size">Mappingsテーブルの行数</param>
        [Theory]
        [InlineData(1000)]
        // [InlineData(100000)]
        public async Task Exercise5(int size)
        {
            // Arrange
            await _setupFixture.GenerateMappings(_context, size);

            // Act
            var result = await Exercise5_Act(_context);

            // Assert
            Assert.Equal(result.Codes.Distinct().Count(), result.Codes.Count);
            Assert.Equal(result.DuplicatedCodes.Distinct().Count(), result.DuplicatedCodes.Count);
        }

        /// <summary>
        /// CodeAとCodeBの組み合わせが格納されたテーブルMappingsをインポートします。
        /// ・コードの組み合わせは[CodeA][SPACE][CodeB]とします。（Codeには空白が含まれない）
        /// ・重複する組み合わせをduplicatedCodesに格納します。
        /// ・uniqueとなる組み合わせをcodesに格納します。
        /// </summary>
        /// <remarks>
        /// Mappingsテーブルの行数が多いと、このメソッドの実行にはとても時間がかかります。
        /// 遅い理由を説明し、10万行でも1秒以内に完了するように改善してください。
        /// </remarks>
        private static async Task<Exercise5Result> Exercise5_Act(TrainingContext context)
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

            return new Exercise5Result(codes, duplicatedCodes);
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
            var models = Exercise6_Act(_context, metadataCodes);

            // Assert
            Assert.All(metadataCodes, code =>
                Assert.Throws<InvalidOperationException>(() =>
                    Assert.Contains(models, x => x.MetadataCode == code)
                ));
        }

        /// <summary>
        /// metadataCodesで指定された複数のMetadataに対するDataValueを取得します。
        /// </summary>
        /// <remarks>
        /// ・このコードはコンパイルは通りますが動作しません。動作しない理由を説明してください。
        /// ・上記のエラーを修正して動作するようにしてください。modelsはIEnumerableでも構いません。
        /// ・modelsがIQueryableになるように修正してください。
        /// ・それぞれ実行されるSQLの違いについて説明してください。
        /// </remarks>
        private static IQueryable<Exercise6Result> Exercise6_Act(
            TrainingContext context, IEnumerable<string> metadataCodes)
        {
            return from av in context.DataValues.Include(x => x.Metadata)
                join ac in metadataCodes on av.Metadata.Code equals ac
                select new Exercise6Result
                {
                    MetadataCode = av.Metadata.Code,
                    Value = av.Value
                };
        }

        [Fact]
        public async Task Exercise7()
        {
            // Arrange
            await _setupFixture.GenerateData(_context);
            var metadataCodes = _setupFixture.GetDataCategoryCode();

            // Act
            var models = Exercise7_Act(_context, metadataCodes);
            // var models2 = Answers.Exercise7_Act1(_context, metadataCodes);

            // Assert
        }

        private static IEnumerable<Exercise7Result> Exercise7_Act(TrainingContext context, string dataCategoryCodes)
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
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }

    public class Exercise1Result
    {
        public DataType DataType { get; set; }
        public string Value { get; set; }
    }

    public class Exercise5Result
    {
        public Exercise5Result(List<string> codes, List<string> duplicatedCodes)
        {
            Codes = codes;
            DuplicatedCodes = duplicatedCodes;
        }

        public List<string> Codes { get; }
        public List<string> DuplicatedCodes { get; }
    }

    public class Exercise6Result
    {
        public string MetadataCode { get; set; }
        public string Value { get; set; }
    }

    public class Exercise7Result
    {
        public string DataCategoryName { get; set; }
        public string MetadataName { get; set; }
    }

    public class ErrorInfo
    {
        public int RowNo { get; set; }
        public string ColumnName { get; set; }
    }
}
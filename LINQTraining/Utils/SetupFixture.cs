using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using EFCore.BulkExtensions;
using LINQTraining.Models;
using Microsoft.EntityFrameworkCore;

namespace LINQTraining.Utils
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class SetupFixture
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// DataCategory, MetadataDataCategory, Metadata, DataValue, CandidateListX テーブルに適当なデータを生成します。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public async Task GenerateData(TrainingContext context)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            var dataTypes = Enum.GetValues(typeof(DataType)).OfType<DataType>().ToList();

            var dataCategories = await context.DataCategory.AnyAsync()
                ? new List<DataCategory>()
                : Enumerable.Range(1, 100)
                    .Select(idx => new DataCategory
                    {
                        Code = $"DataCategoryCode{idx:D3}",
                        Name = $"DataCategoryName{idx:D3}",
                    })
                    .FillColumn()
                    .ToList();
            await context.BulkInsertAsync(dataCategories, new BulkConfig { SetOutputIdentity = true });

            var metadataList = await context.Metadata.AnyAsync()
                ? new List<Metadata>()
                : Enumerable.Range(1, 100)
                    .Select(idx => new Metadata
                    {
                        Code = $"MetadataCode{idx:D3}",
                        Name = $"MetadataName{idx:D3}",
                        DataType = dataTypes[idx % dataTypes.Count],
                        CandidateList = $"CandidateList{(char)('A' + idx % 3)}",
                        ColumnIndex = idx
                    })
                    .FillColumn()
                    .ToList();
            await context.BulkInsertAsync(metadataList, new BulkConfig { SetOutputIdentity = true });

            var metadataValues = await context.DataValues.AnyAsync()
                ? new List<DataValue>()
                : Enumerable.Range(1, 100000)
                    .Select(idx =>
                    {
                        var metadata = metadataList[idx % metadataList.Count];
                        return new DataValue
                        {
                            MetadataId = metadata.Id,
                            Value = metadata.DataType switch
                            {
                                DataType.String => $"MetadataValue{idx:D5}",
                                DataType.Integer => $"{idx}",
                                DataType.DataTime => $"{DateTime.Now}",
                                _ => throw new ArgumentOutOfRangeException()
                            }
                        };
                    })
                    .FillColumn()
                    .ToList();
            await context.BulkInsertAsync(metadataValues);

            if (!await context.CandidateListA.AnyAsync())
            {
                var candidateValues = typeof(TrainingContext).GetProperties()
                    .Where(x => Regex.IsMatch(x.Name, @"CandidateList[A-Z]{1}"))
                    .SelectMany(property =>
                    {
                        var type = property.PropertyType.GenericTypeArguments[0];
                        // idx => new <type> { Value = string.Format("{0}{idx}", (object)type.Name[^1], (object)idx) }
                        var idxExpr = Expression.Parameter(typeof(int), "idx");
                        var selector = Expression.Lambda<Func<int, object>>(
                            Expression.MemberInit(
                                Expression.New(type),
                                Expression.Bind(
                                    type.GetProperty("Value")!,
                                    Expression.Call(
                                        typeof(string),
                                        nameof(string.Format),
                                        null,
                                        Expression.Constant("{0}{1}", typeof(string)),
                                        Expression.Constant(type.Name[^1], typeof(object)),
                                        Expression.Convert(idxExpr, typeof(object))))),
                            idxExpr);

                        return Enumerable.Range(0, 20).Select(selector.Compile());
                    });

                context.AddRange(candidateValues);
                await context.SaveChangesAsync();
            }

            if (!await context.MetadataDataCategory.AnyAsync())
            {
                var query = from metadata in context.Metadata
                    from dataCategory in context.DataCategory
                    select new MetadataDataCategory()
                    {
                        Metadata = metadata, DataCategory = dataCategory
                    };
                var metadataDataCategories = query.AsEnumerable()
                    .Where(x => _random.Next(0, 100) < 10)
                    .ToList();
                context.AddRange(metadataDataCategories);
                await context.SaveChangesAsync();
            }

            await transaction.CommitAsync();
        }

        /// <summary>
        /// MetadataCodeを何件か返します。（最大20件）
        /// </summary>
        /// <returns></returns>
        public ICollection<string> GetSomeMetadataCodes()
        {
            return Enumerable.Range(1, _random.Next(1, 20))
                .Select(x => $"MetadataCode{_random.Next(1, 100):D3}")
                .ToList();
        }

        /// <summary>
        /// Mappingsテーブルに適当な行を生成します。
        /// </summary>
        /// <param name="size"></param>
        public async Task GenerateMappings(TrainingContext context, int size)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            if (size != await context.Mappings.CountAsync())
            {
                await context.Mappings.BatchDeleteAsync();
                var mappings = Enumerable.Range(1, size)
                    .Select(x => new Mapping
                    {
                        CodeA = $"CodeA{_random.Next(1, 500):D3}",
                        CodeB = $"CodeB{_random.Next(1, 500):D3}"
                    })
                    .ToList();
                await context.BulkInsertAsync(mappings);
            }

            await transaction.CommitAsync();
        }

        public (DataTable dataTable, List<ErrorInfo> errorsList) Exercise2(int rowCount)
        {
            var dataTable = new DataTable();

            dataTable.Columns.Add(new DataColumn
            {
                ColumnName = "Row No",
                DataType = typeof(int)
            });
            dataTable.Columns.AddRange(
                Enumerable.Range(1, 200)
                    .Select(idx => new DataColumn
                    {
                        ColumnName = $"Column{idx:D4}",
                        DataType = typeof(string)
                    })
                    .ToArray());

            var emptyRow = Enumerable.Repeat((object)string.Empty, 200).ToList();
            for (var idx = 1; idx <= rowCount; idx++)
            {
                dataTable.Rows.Add(emptyRow.Prepend(idx).ToArray());
            }

            var errorsList = Enumerable.Range(1, rowCount)
                .Select(_ => new ErrorInfo
                {
                    RowNo = _random.Next(1, rowCount),
                    ColumnName = $"Column{_random.Next(1, 1000):D4}"
                })
                .ToList();

            return (dataTable, errorsList);
        }

        public string GetDataCategoryCode()
        {
            return $"DataCategoryCode{_random.Next(1, 100):D3}";
        }
    }
}
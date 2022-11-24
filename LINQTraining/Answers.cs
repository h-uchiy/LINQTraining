using System.Collections.Generic;
using System.Data;
using LINQTraining.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace LINQTraining
{
    // 解答例
    public static class Answers
    {
        /// <remarks>
        /// 1. ループを記述しないようにリファクタリングしてください。
        /// </remarks>
        public static List<Exercise1Result> Exercise1_Act1(TrainingContext context, string metadataCode)
        {
            return context.DataValues
                .Include(x => x.Metadata)
                .Where(x => x.Metadata.Code == metadataCode)
                .AsEnumerable()
                .Select(x => new Exercise1Result
                {
                    DataType = x.Metadata.DataType,
                    Value = x.Value
                })
                .ToList();
        }

        /// <remarks>
        /// 2. 最終的に必要なデータ以外の列をDBから取得しないようにリファクタリングしてください。
        /// 3. データをメモリに展開しないようにリファクタリングしてください。
        /// </remarks>
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

        /// <summary>
        /// 元の構造をあまり変えずに改善した例
        /// </summary>
        internal static void Exercise2_Act1(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
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
        internal static void Exercise2_Act2(DataTable dataTable, IEnumerable<ErrorInfo> errorsList)
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
        public static async Task<IEnumerable<Exercise4Result>> Exercise4_Act1(TrainingContext context, IEnumerable<string> metadataCodes)
        {
            return from av in await context.DataValues
                    .Include(x => x.Metadata)
                    .ToListAsync()
                join ac in metadataCodes on av.Metadata.Code equals ac
                select new Exercise4Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        /// <summary>
        /// DB側ですべて演算を行い、最終的に必要なデータのみをメモリにロードする実装
        /// LINQ to Entityの式の中で参照する分にはIncludeを記述する必要はない
        /// </summary>
        public static IQueryable<Exercise4Result> Exercise4_Act2(TrainingContext context,
            ICollection<string> metadataCodes)
        {
            return from av in context.DataValues
                // メモリ上の値による絞り込みを行いたい場合はICollection.Contains()を使用するとSQLに変換できる
                where metadataCodes.Contains(av.Metadata.Code)
                select new Exercise4Result { MetadataCode = av.Metadata.Code, Value = av.Value };
        }

        /// <summary>
        /// 
        /// </summary>
        public static IQueryable<Exercise5Result> Exercise5_Act1(TrainingContext context, string dataCategoryCodes)
        {
            return from dataCategory in context.DataCategory
                where dataCategory.Code == dataCategoryCodes
                from metadataDataCategory in dataCategory.MetadataDataCategory.DefaultIfEmpty()
                select new Exercise5Result
                {
                    DataCategoryName = dataCategory.Name,
                    MetadataName = metadataDataCategory.Metadata.Name,
                };
        }
    }
}
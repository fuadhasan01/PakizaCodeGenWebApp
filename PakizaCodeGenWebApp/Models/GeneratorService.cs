using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Transactions;
using System.Windows.Input;

namespace PakizaCodeGenWebApp.Models
{
    public class GeneratorService
    {
        public Dictionary<string, string> GenerateAll(string tableSql)
        {
            string entity = GenerateEntity(tableSql);
            string bll = GenerateBLL(tableSql);
            string bllPartial = GenerateBLLPartial(tableSql);
            string dal = GenerateDAL(tableSql);
            string dalPartial = GenerateDALPartial(tableSql);
            string global = GenerateGlobalConstants(tableSql);
            string task = GenerateTaskManager(tableSql);

            return new Dictionary<string, string>
            {
                {"Entity", entity},
                {"BLL", bll},
                {"BLL_Partial", bllPartial},
                {"DAL", dal},
                {"DAL_Partial", dalPartial},
                {"GLOBAL", global},
                {"TASK", task},
            };
        }

        private string GenerateEntity(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)\s*\(([^;]+)\)", RegexOptions.Multiline);
            if (!match.Success) return "Invalid SQL";

            string schemaPrefix = match.Groups[1].Value;       // e.g., MERC
            string tableBase = match.Groups[2].Value;          // e.g., BuyingAgent
            string columnBlock = match.Groups[3].Value;        // column definitions

            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower(); // e.g., Merc
            string className = pascalPrefix + tableBase + "Entity"; // e.g., MercBuyingAgentEntity

            var sb = new StringBuilder();
            sb.AppendLine($"public class {className}");
            sb.AppendLine("{");

            string[] lines = columnBlock.Split(',');

            foreach (var line in lines)
            {
                string cleanLine = line.Trim();
                if (string.IsNullOrWhiteSpace(cleanLine)) continue;

                var parts = cleanLine.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string colName = parts[0].Replace("[", "").Replace("]", "").Trim();
                sb.AppendLine($"    public string {colName} {{ get; set; }}");
            }

            sb.AppendLine("    public int SL { get; set; }");
            sb.AppendLine("    public string QueryFlag { get; set; }");
            sb.AppendLine("}");

            return sb.ToString();
        }


        private string GenerateBLL(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)", RegexOptions.IgnoreCase);
            if (!match.Success) return "// Invalid SQL for BLL generation";

            string schemaPrefix = match.Groups[1].Value;           // e.g., MERC
            string tableBase = match.Groups[2].Value;              // e.g., BuyingAgent
            string tableName = $"{schemaPrefix}_{tableBase}";      // e.g., MERC_BuyingAgent

            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower(); // Merc
            string className = pascalPrefix + tableBase;          // MercBuyingAgent
            string entityName = className + "Entity";             // MercBuyingAgentEntity
            string dalName = className + "DAL";                   // MercBuyingAgentDAL

            string instanceEntity = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "Entity"; // mercBuyingAgentEntity
            string instanceDal = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "DAL";       // mercBuyingAgentDAL


            return $@"
using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using ERP.Domain.Model.{schemaPrefix.ToUpper()};
using ERP.Server.DAL.{schemaPrefix.ToUpper()};

namespace ERP.Server.BLL.{schemaPrefix.ToUpper()}
{{
    public partial class {className}BLL
    {{
        public object Save{className}Info(object param)
        {{
            Database db = DatabaseFactory.CreateDatabase();
            object retObj = null;
            using (DbConnection connection = db.CreateConnection())
            {{
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {{
                    {entityName} {instanceEntity} = ({entityName})param;
                    {className}DAL {instanceDal} = new {className}DAL();
                    retObj = {instanceDal}.Save{className}Info({instanceEntity}, db, transaction);
                    transaction.Commit();
                }}
                catch
                {{
                    transaction.Rollback();
                    throw;
                }}
                finally
                {{
                    connection.Close();
                }}
            }}
            return retObj;
        }}

        public object Update{className}Info(object param)
        {{
            Database db = DatabaseFactory.CreateDatabase();
            object retObj = null;
            using (DbConnection connection = db.CreateConnection())
            {{
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {{
                    {entityName} {instanceEntity} = ({entityName})param;
                    {className}DAL {instanceDal} = new {className}DAL();
                    retObj = {instanceDal}.Update{className}Info({instanceEntity}, db, transaction);
                    transaction.Commit();
                }}
                catch
                {{
                    transaction.Rollback();
                    throw;
                }}
                finally
                {{
                    connection.Close();
                }}
            }}
            return retObj;
        }}

        public object Delete{className}InfoById(object param)
        {{
            Database db = DatabaseFactory.CreateDatabase();
            object retObj = null;
            using (DbConnection connection = db.CreateConnection())
            {{
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
                try
                {{
                    {className}DAL {instanceDal} = new {className}DAL();
                    retObj = {instanceDal}.Delete{className}InfoById(param, db, transaction);
                    transaction.Commit();
                }}
                catch
                {{
                    transaction.Rollback();
                    throw;
                }}
                finally
                {{
                    connection.Close();
                }}
            }}
            return retObj;
        }}

        public object GetSingle{className}RecordById(object param)
        {{
            object retObj = null;
            {className}DAL {instanceDal} = new {className}DAL();
            retObj = {instanceDal}.GetSingle{className}RecordById(param);
            return retObj;
        }}
    }}
}}
";
        }




        private string GenerateBLLPartial(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)", RegexOptions.IgnoreCase);
            if (!match.Success) return "// Invalid SQL for BLL Partial generation";

            string schemaPrefix = match.Groups[1].Value;           // e.g., MERC
            string tableBase = match.Groups[2].Value;              // e.g., BuyingAgent

            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower(); // Merc
            string className = pascalPrefix + tableBase;          // MercBuyingAgent
            string dalName = className + "DAL";                   // MercBuyingAgentDAL
            string instanceDal = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "DAL"; // mercBuyingAgentDAL

            return $@"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ERP.Domain.Model.{schemaPrefix.ToUpper()};
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Data.Common;
using ERP.Server.DAL.{schemaPrefix.ToUpper()};

namespace ERP.Server.BLL.{schemaPrefix.ToUpper()}
{{
    public partial class {className}BLL
    {{
        public object GetAll{className}Record(object param)
        {{
            object retObj = null;
            {dalName} {instanceDal} = new {dalName}();
            retObj = {instanceDal}.GetAll{className}Record(param);
            return retObj;
        }}
    }}
}}
";
        }

        private string GenerateDAL(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return "// SQL input cannot be null or empty";
            }

            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)\s*\(([^;]+)\)", RegexOptions.Multiline);
            if (!match.Success)
            {
                return "// Invalid SQL for DAL generation. Expected pattern: 'CREATE TABLE schema_table (columns)'";
            }

            string schemaPrefix = match.Groups[1].Value;
            string tableBase = match.Groups[2].Value;
            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower();
            string className = pascalPrefix + tableBase;
            string entityName = className + "Entity";
            string dalName = className + "DAL";
            string instanceEntity = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "Entity";
            string tableName = schemaPrefix + "_" + tableBase;

            var lines = match.Groups[3].Value.Split(',');
            List<string> insertFields = new();
            List<string> insertParams = new();
            List<string> addParams = new();
            List<string> updateSet = new();

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) continue;

                string col = parts[0].Trim().Trim('[', ']');
                if (col.Equals("ID", StringComparison.OrdinalIgnoreCase)) continue;

                insertFields.Add(col);
                insertParams.Add("@" + col);
                updateSet.Add(col + "=@" + col);
                addParams.Add($"db.AddInParameter(dbCommand, \"{col}\", DbType.String, {instanceEntity}.{col});");
            }

            string insertCols = string.Join(", ", insertFields);
            string insertVals = string.Join(", ", insertParams);
            string updateCols = string.Join(", ", updateSet);
            string addInserts = string.Join("\n            ", addParams);
            string selectFields = string.Join(", ", insertFields.Prepend("Id"));
            string readFields = string.Join("\n                    ", insertFields.Select(f =>
                $"if (dataReader[\"{f}\"] != DBNull.Value)\n                        {instanceEntity}.{f} = dataReader[\"{f}\"].ToString();"));

            return $@"
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ERP.Domain.Model.{schemaPrefix.ToUpper()};
using Microsoft.Practices.EnterpriseLibrary.Data;
using System.Configuration;

namespace ERP.Server.DAL.{schemaPrefix.ToUpper()}
{{
    public partial class {dalName}
    {{
        #region Auto Generated 

        public bool Save{className}Info({entityName} {instanceEntity}, Database db, DbTransaction transaction)
        {{
            string NewGuId = Guid.NewGuid().ToString().ToUpper();
            string sql = @""INSERT INTO {tableName} (Id, {insertCols}) VALUES (@NewGuId, {insertVals})"";

            if (ConfigurationManager.AppSettings[""DB_Type""] == ""ORA"")
                sql = sql.Replace(""SUBSTRING"", ""SUBSTR"").Replace(""@"", "":"").Replace(""ISNULL"", ""NVL"").Replace(""CONVERT(DATETIME,"", ""TO_DATE("").Replace("",103)"", "",'DD/MM/YYYY')"");

            DbCommand dbCommand = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCommand, ""NewGuId"", DbType.String, NewGuId);
            { addInserts}

            var res = db.ExecuteNonQuery(dbCommand, transaction);
            return res > 0;
        }}

        public bool Update{className}Info({entityName} {instanceEntity}, Database db, DbTransaction transaction)
        {{
            string sql = @""UPDATE {tableName} SET {updateCols} WHERE Id=@Id"";

            if (ConfigurationManager.AppSettings[""DB_Type""] == ""ORA"")
                sql = sql.Replace(""SUBSTRING"", ""SUBSTR"").Replace(""@"", "":"").Replace(""ISNULL"", ""NVL"").Replace(""CONVERT(DATETIME,"", ""TO_DATE("").Replace("",103)"", "",'DD/MM/YYYY')"");

            DbCommand dbCommand = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCommand, ""Id"", DbType.String, {instanceEntity}.Id);
            {addInserts}
            db.ExecuteNonQuery(dbCommand, transaction);
            return true;
        }}

        public bool Delete{className}InfoById(object param, Database db, DbTransaction transaction)
        {{
            string sql = @""DELETE FROM {tableName} WHERE Id=@Id"";

            if (ConfigurationManager.AppSettings[""DB_Type""] == ""ORA"")
                sql = sql.Replace(""SUBSTRING"", ""SUBSTR"").Replace(""@"", "":"").Replace(""ISNULL"", ""NVL"").Replace(""CONVERT(DATETIME,"", ""TO_DATE("").Replace("",103)"", "",'DD/MM/YYYY')"");

            DbCommand dbCommand = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCommand, ""Id"", DbType.String, param);

            db.ExecuteNonQuery(dbCommand, transaction);
            return true;
        }}

        public {entityName} GetSingle{className}RecordById(object param)
        {{
            Database db = DatabaseFactory.CreateDatabase();
            string sql = @""SELECT {selectFields} FROM {tableName} WHERE Id=@Id"";

            if (ConfigurationManager.AppSettings[""DB_Type""] == ""ORA"")
                sql = sql.Replace(""SUBSTRING"", ""SUBSTR"").Replace(""@"", "":"").Replace(""ISNULL"", ""NVL"").Replace(""CONVERT(DATETIME,"", ""TO_DATE("").Replace("",103)"", "",'DD/MM/YYYY')"");

            DbCommand dbCommand = db.GetSqlStringCommand(sql);
            db.AddInParameter(dbCommand, ""Id"", DbType.String, param);
            {entityName} {instanceEntity} = null;
            using (IDataReader dataReader = db.ExecuteReader(dbCommand))
            {{
                if (dataReader.Read())
                {{
                    {instanceEntity} = new {entityName}();
                    if (dataReader[""ID""] != DBNull.Value)
                        {instanceEntity}.Id = dataReader[""ID""].ToString();
                    {readFields}
                }}
            }}
            return {instanceEntity};
        }}
    }}

        #endregion
        
}}
";
}

        private string GenerateDALPartial(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)", RegexOptions.Multiline);
            if (!match.Success) return "// Invalid SQL for DAL Partial generation";

            string schemaPrefix = match.Groups[1].Value;
            string tableBase = match.Groups[2].Value;
            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower();
            string className = pascalPrefix + tableBase;
            string entityName = className + "Entity";
            string instanceEntity = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "Entity";
            string tableName = schemaPrefix + "_" + tableBase;

            return $@"
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using ERP.Domain.Model.{schemaPrefix.ToUpper()};
using Microsoft.Practices.EnterpriseLibrary.Data;

namespace ERP.Server.DAL.{schemaPrefix.ToUpper()}
{{
    public partial class {className}DAL
    {{
        public DataTable GetAll{className}Record(object param)
        {{
            {entityName} obj = new {entityName}();
            if (param != null) obj = ({entityName})param;

            Database db = DatabaseFactory.CreateDatabase();
            string sql = @""SELECT * FROM {tableName} WHERE 1 = 1 "";

            // Add conditions here if needed using obj.Property
            sql += "" ORDER BY Id"";

            DbCommand dbCommand = db.GetSqlStringCommand(sql);
            DataSet ds = db.ExecuteDataSet(dbCommand);
            return ds.Tables[0];
        }}
    }}
}}
";
}

        private string GenerateGlobalConstants(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)", RegexOptions.IgnoreCase);
            if (!match.Success) return "// Invalid SQL for Global constants generation";

            string schemaPrefix = match.Groups[1].Value;
            string tableBase = match.Groups[2].Value;

            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower();
            string className = pascalPrefix + tableBase; // e.g. MercBuyingAgent

            var sb = new StringBuilder();
            sb.AppendLine($"#region Auto Generated - {schemaPrefix}_{tableBase}");
            sb.AppendLine($"public const string AG_Save{className}Info = \"AG_Save{className}Info\";");
            sb.AppendLine($"public const string AG_Update{className}Info = \"AG_Update{className}Info\";");
            sb.AppendLine($"public const string AG_Delete{className}InfoById = \"AG_Delete{className}InfoById\";");
            sb.AppendLine($"public const string AG_GetAll{className}Record = \"AG_GetAll{className}Record\";");
            sb.AppendLine($"public const string AG_GetSingle{className}RecordById = \"AG_GetSingle{className}RecordById\";");
            sb.AppendLine("#endregion");
            return sb.ToString();
        }

        private string GenerateTaskManager(string sql)
        {
            var match = Regex.Match(sql, @"CREATE TABLE (\w+)_(\w+)", RegexOptions.IgnoreCase);
            if (!match.Success) return "// Invalid SQL for Task manager generation";

            string schemaPrefix = match.Groups[1].Value;
            string tableBase = match.Groups[2].Value;

            string pascalPrefix = char.ToUpper(schemaPrefix[0]) + schemaPrefix.Substring(1).ToLower();
            string className = pascalPrefix + tableBase; // e.g. MercBuyingAgent

            string instanceBLL = char.ToLower(pascalPrefix[0]) + pascalPrefix.Substring(1) + tableBase + "BLL";

            var sb = new StringBuilder();
            sb.AppendLine($"#region Auto Generated - {schemaPrefix}_{tableBase}");
            sb.AppendLine($"case ERPTask.AG_Save{className}Info:");
            sb.AppendLine($"    {className}BLL {instanceBLL} = new {className}BLL();");
            sb.AppendLine($"    return {instanceBLL}.Save{className}Info(param);");
            sb.AppendLine($"case ERPTask.AG_Update{className}Info:");
            sb.AppendLine($"    {instanceBLL} = new {className}BLL();");
            sb.AppendLine($"    return {instanceBLL}.Update{className}Info(param);");
            sb.AppendLine($"case ERPTask.AG_Delete{className}InfoById:");
            sb.AppendLine($"    {instanceBLL} = new {className}BLL();");
            sb.AppendLine($"    return {instanceBLL}.Delete{className}InfoById(param);");
            sb.AppendLine($"case ERPTask.AG_GetAll{className}Record:");
            sb.AppendLine($"    {instanceBLL} = new {className}BLL();");
            sb.AppendLine($"    return {instanceBLL}.GetAll{className}Record(param);");
            sb.AppendLine($"case ERPTask.AG_GetSingle{className}RecordById:");
            sb.AppendLine($"    {instanceBLL} = new {className}BLL();");
            sb.AppendLine($"    return {instanceBLL}.GetSingle{className}RecordById(param);");
            sb.AppendLine("#endregion");

            return sb.ToString();
        }



    }
}

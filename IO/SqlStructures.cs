using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.IO {
    // 数据库字段类型, 不同的数据库, 类型不同
    public enum SqlFieldType : byte {
        Unknown,
        Null,
        UpdateResult,

        MySql_BIT,
        MySql_CHAR,
        MySql_VARCHAR,
        MySql_ENUM,
        MySql_SET,
        MySql_TINYINT,
        MySql_SMALLINT,
        MySql_MEDIUMINT,
        MySql_INT,
        MySql_BIGINT,
        MySql_FLOAT,
        MySql_DOUBLE,
        MySql_DEMICAL,
        MySql_NUMERIC,
        MySql_DATE,
        MySql_DATETIME,
        MySql_TIMESTAMP,
        MySql_TIME,
        MySql_YEAR,
        MySql_GEOMETRY,
        MySql_BINARY,
        MySql_VARBINARY,
        MySql_LONGVARCHAR,
        MySql_LONGVARBINARY,
        MySql_JSON,

        MsAccess_Currency,
        MsAccess_AutoNumber,
        MsAccess_YesNo,
        MsAccess_Hyperlink,
        MsAccess_Text,// = MySql_Varchar,
        MsAccess_Memo,// = MySql_Text,
        MsAccess_Byte,// = MySql_TinyInt,
        MsAccess_Integer,// = MySql_SmallInt,
        MsAccess_Long,// = MySql_Int,
        MsAccess_Single,// = MySql_Float,
        MsAccess_Double,// = MySql_Double,
        MsAccess_DateTime,// = MySql_DateTime,
        MsAccess_OleObject,// = MySql_LongBlob,
        MsAccess_LookupWizard,// = MySql_Enum,

        MsSqlServer_char,
        MsSqlServer_varchar,
        MsSqlServer_text,
        MsSqlServer_nchar,
        MsSqlServer_nvarchar,
        MsSqlServer_ntext,
        MsSqlServer_bit,
        MsSqlServer_binary,
        MsSqlServer_varbinary,
        MsSqlServer_image,
        MsSqlServer_tinyint,
        MsSqlServer_smallint,
        MsSqlServer_int,
        MsSqlServer_bigint,
        MsSqlServer_decimal,
        MsSqlServer_numeric,
        MsSqlServer_smallmoney,
        MsSqlServer_money,
        MsSqlServer_float,
        MsSqlServer_real,
        MsSqlServer_datetime,
        MsSqlServer_datetime2,
        MsSqlServer_smalldatetime,
        MsSqlServer_date,
        MsSqlServer_time,
        MsSqlServer_datetimeoffset,
        MsSqlServer_timestamp,
        MsSqlServer_sql_variant,
        MsSqlServer_uniqueidentifier,
        MsSqlServer_xml,
        MsSqlServer_cursor,
        MsSqlServer_table,

        MsExcel_Normal,
    };

    // 条件语句类型
    public enum SqlClauseType {
        Null,
        Where,
        OrderBy,
        Top
    };

    // 运算符类型
    public enum SqlOperatorType {
        none,
        _is,
        like,
        _in,
        between,
        and,
        or,
        alias,
        innerJoin,
        leftJoin,
        rightJoin,
        fullJoin
    };

    // 表头信息结构
    public struct SqlFieldHead {
        public uint length;
        public string catalogName;
        public string columnName;
        public SqlFieldType type;
        public bool allowNull;
        public bool autoIncrease;
    };

    // 单字段类
    public class SqlField {
        public SqlField() {
            Head = default;
            Value = "";
        }
        public SqlField(string value, SqlFieldHead head) {
            Head = head;
            Value = value;
        }

        public SqlFieldHead Head { get; private set; }
        public string Value { get; set; }
    };

    // 数据库信息结构
    public struct SqlDatabaseInfo {
        public string name;
        public string server;
        public string charset;
        public string sortRule;
    };

    // 表信息结构
    public class SqlTableInfo {
        public string tableName;
        public string engine;
        public string comment;
        public SqlDatabaseInfo[] parentDatabase;
    };

    // 表类
    public class SqlTable : SqlTableInfo {
        public SqlTable(IEnumerable<SqlFieldHead> heads) {
            this.heads = new List<SqlFieldHead>();
            fields = new List<List<SqlField>>();
            foreach(var i in heads) {
                this.heads.Add(i);
            }
        }
        public int Width => heads.Count;
        public int Height => fields.Count;
        public SqlFieldHead GetHead(int index) => heads[index];
        public IList<SqlField> GetRow(int index) => fields[index];
        public void AddRow(List<SqlField> rowFields) => fields.Add(rowFields);

        readonly IList<SqlFieldHead> heads;
        readonly IList<List<SqlField>> fields;
    };

    // SQL表达式类
    public class SqlExpress {
        public SqlExpress(string str = "") {
            PushValue(str);
        }

        public bool PushValue(params string[] value) {
            // TODO
            return false;
        }
        public bool RemoveValue(uint index) {
            // TODO
            return false;
        }
        public bool Clear() {
            // TODO
            return false;
        }
        public SqlOperatorType Type { get; set; }
    };

    // SQL条件语句类
    public class SqlClause {
        public SqlClause(string str = "") {
            PushExpress(new SqlExpress(str));
        }

        public bool PushExpress(params SqlExpress[] value) {
            // TODO:
            return false;
        }
        public bool RemoveExpress(uint index) {
            // TODO: 
            return false;
        }
        public bool Clear() {
            // TODO:
            return false;
        }
        public SqlClauseType Type { get; set; }
    };

}

using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.IO {
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
        public Type type;
        public bool allowNull;
        public bool autoIncrease;
    };

    // 单字段类
    public class SqlField {
        public SqlField() {
            Head = default;
            Value = "";
        }
        public SqlField(object value, SqlFieldHead head) {
            Head = head;
            Value = value;
        }

        public SqlFieldHead Head { get; private set; }
        public object Value { get; set; }
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

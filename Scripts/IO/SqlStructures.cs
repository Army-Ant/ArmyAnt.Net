using System;
using System.Collections.Generic;

namespace ArmyAnt.IO {
    // �����������
    public enum SqlClauseType {
        Null,
        Where,
        OrderBy,
        Top
    };

    // ���������
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

    // ��ͷ��Ϣ�ṹ
    public struct SqlFieldHead {
        public uint length;
        public string catalogName;
        public string columnName;
        public Type type;
        public bool allowNull;
        public bool autoIncrease;
    };

    // ���ֶ���
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

    // ���ݿ���Ϣ�ṹ
    public struct SqlDatabaseInfo {
        public string name;
        public string server;
        public string charset;
        public string sortRule;
    };

    // ����Ϣ�ṹ
    public class SqlTableInfo {
        public string tableName;
        public string engine;
        public string comment;
        public SqlDatabaseInfo[] parentDatabase;
    };

    // ����
    public class SqlTable : SqlTableInfo {
        public SqlTable(IEnumerable<SqlFieldHead> heads) {
            this.heads = new List<SqlFieldHead>();
            fields = new List<List<SqlField>>();
            foreach (var i in heads) {
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

    // SQL���ʽ��
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

    // SQL���������
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

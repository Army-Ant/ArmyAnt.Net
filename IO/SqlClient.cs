using System;
using System.Collections.Generic;
using System.Text;

namespace ArmyAnt.IO {
    public abstract class SqlClient {
        public abstract bool Connect(string connString);
        public abstract void Disconnect();
        public abstract bool IsConnection { get; }
        public abstract string[] GetDatabaseList();
        public abstract string[] GetTableNameList();
        public abstract string[] GetViewNameList();

        public SqlTable GetWholeTable(string tableName) => Select(tableName);
        public SqlTable GetWholeView(string tableName) => Select(tableName);
        public abstract string[] GetTableAllFields(string table);

        // select * from [tableName]
        public SqlTable Select(string tableName, params SqlClause[] clauses) => Query("select * from " + tableName + OrganizeSqlClause(clauses));

        // select [columnNames] from [tableName]
        public SqlTable Select(string tableName, string[] columnNames, params SqlClause[] clauses) {
            string sql = "select ";
            if(columnNames != null)
                for(int i = 0; i < columnNames.Length; ++i) {
                    sql += columnNames[i] + " , ";
                }
            return Query(sql + "from " + tableName + OrganizeSqlClause(clauses));
        }

        // update [tableName] set [updatedData ( k=value , k=value ... )]
        public long Update(string tableName, IEnumerable<SqlField> data, params SqlClause[] clauses) {
            string sql = "update " + tableName + " set ";
            foreach(var i in data) {
                sql += i.Head.columnName + " = \"" + i.Value + "\" " + ", ";
            }
            sql = sql.Remove(sql.Length - 2);
            return Update(sql + OrganizeSqlClause(clauses));
        }

        // insert into [tableName] [insertedData (k , k , k ... ) values ( value , value , value ... )]
        public long InsertRow(string tableName, IEnumerable<SqlField> data) {
            string keys = "";
            string values = "";
            foreach(var i in data) {
                keys += i.Head.columnName + ", ";
                values += i.Value + ", ";
            }
            keys = keys.Remove(keys.Length - 2);
            values = values.Remove(keys.Length - 2);

            return Update("insert into " + tableName + " ( " + keys + " ) values ( " + values + " )");
        }

        // alter table [tableName] add [columnHead name dataType (others)...]
        public long InsertColumn(string tableName, params SqlFieldHead[] columnHead) => Update("alter table " + tableName + " add " + OrganizeColumnInfo(columnHead));

        // delete from [tableName]
        public long DeleteRow(string tableName, SqlClause where = null) {
            if(where != null && where.Type != SqlClauseType.Where)
                return -1;
            return Update("delete from " + tableName + ' ' + OrganizeSqlClause(where));

        }

        // alter table [tableName] drop column [columnName]
        public long DeleteColumn(string tableName, string columnName) => Update("alter table " + tableName + " drop column " + columnName);

        public long CreateDatabase(string dbName) => Update("create database " + dbName);

        public long DeleteDatabase(string dbName) => Update("drop database " + dbName);

        public long CreateTable(SqlTable table) {
            string sql = "create table " + table.tableName + " ( ";
            for(int i = 0; i < table.Width; ++i) {
                sql += OrganizeColumnInfo(table.GetHead(i)) + " , ";
            }
            sql = sql.Remove(sql.Length - 2);
            return Update(sql + " )");
        }

        public long DeleteTable(string tableName) => Update("drop table " + tableName);

        public string OrganizeColumnInfo(params SqlFieldHead[] column) {
            // TODO: 应当扩展支持的更多属性
            string ret = "";
            foreach(var i in column) {
                ret += i.columnName + ' ' + i.type.ToString() + (i.allowNull ? "" : " not null") + ", ";
            }
            ret = ret.Remove(ret.Length - 2);
            return ret;
        }

        public string OrganizeSqlClause(params SqlClause[] clauses) {
            // TODO: 完善clause的结构然后补全此处
            string sql = "";

            return sql;
        }

        public abstract SqlTable Query(string sql);

        public abstract long Update(string sql);
    }
}

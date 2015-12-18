using System;
using System.Collections.Generic;

namespace Redbox.HAL.DataStorage;

public interface IDataStore
{
	bool Update(string update);

	int BulkUpdate(List<string> update);

	bool Insert(string insert);

	int BulkInsert(List<string> inserts);

	int Delete(string deleteQuery);

	int BulkDelete(List<string> removes);

	bool RowExists(string query);

	int GetTableRowCount(string query);

	bool TableExists(string name);

	bool CreateTable(string createTable);

	bool DropTable(string tableName);

	void ExectuteSelectQuery(HandleSelectResult callback, string query);

	void CloseConnection(bool force);

	string FormatNullableTime(DateTime? time);
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class KioskFunctionCheckService : IKioskFunctionCheckService
{
	private class FunctionTestData : IKioskFunctionCheckData
	{
		public string VerticalSlotTestResult { get; internal set; }

		public string InitTestResult { get; internal set; }

		public string VendDoorTestResult { get; internal set; }

		public string TrackTestResult { get; internal set; }

		public string SnapDecodeTestResult { get; internal set; }

		public string TouchscreenDriverTestResult { get; internal set; }

		public string CameraDriverTestResult { get; internal set; }

		public DateTime Timestamp { get; internal set; }

		public string UserIdentifier { get; internal set; }

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Kiosk Check Data");
			stringBuilder.AppendLine($"  Vertical slot result = {VerticalSlotTestResult}");
			stringBuilder.AppendLine($"  Init result = {InitTestResult}");
			stringBuilder.AppendLine($"  Vend door result = {VendDoorTestResult}");
			stringBuilder.AppendLine($"  Track result = {TrackTestResult}");
			stringBuilder.AppendLine($"  Snap and decode result = {SnapDecodeTestResult}");
			stringBuilder.AppendLine($"  Camera driver result = {CameraDriverTestResult}");
			stringBuilder.AppendLine($"  Touchscreen driver result = {TouchscreenDriverTestResult}");
			stringBuilder.AppendLine($"  Timestamp = {Timestamp.ToString()}");
			stringBuilder.AppendLine($"  User token = {UserIdentifier}");
			return stringBuilder.ToString();
		}
	}

	private const string TableName = "KioskFunctionCheckData";

	private readonly IDataStore Store;

	public bool Add(IKioskFunctionCheckData data)
	{
		string text = Store.FormatNullableTime(data.Timestamp);
		string insert = string.Format("INSERT INTO [{0}] ( SampleDate, UserToken, VerticalTestResult, VendDoorTestResult, TrackTestResult, SnapDecodeTestResult, CameraDriverTestResult, TouchScreenDriverTestResult, InitTestResult ) VALUES ( {1}, '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')", "KioskFunctionCheckData", text, data.UserIdentifier, data.VerticalSlotTestResult, data.VendDoorTestResult, data.TrackTestResult, data.SnapDecodeTestResult, data.CameraDriverTestResult, data.TouchscreenDriverTestResult, data.InitTestResult);
		return Store.Insert(insert);
	}

	public IList<IKioskFunctionCheckData> Load()
	{
		List<IKioskFunctionCheckData> rv = new List<IKioskFunctionCheckData>();
		string query = string.Format("select SampleDate, UserToken, VerticalTestResult, VendDoorTestResult, TrackTestResult, SnapDecodeTestResult, CameraDriverTestResult, TouchScreenDriverTestResult, InitTestResult FROM [{0}]", "KioskFunctionCheckData");
		Store.ExectuteSelectQuery(delegate(IDataReader reader)
		{
			FunctionTestData item = new FunctionTestData
			{
				Timestamp = reader.ReadDateTime(0).Value,
				UserIdentifier = reader.ReadString(1),
				VerticalSlotTestResult = reader.ReadString(2),
				VendDoorTestResult = reader.ReadString(3),
				TrackTestResult = reader.ReadString(4),
				SnapDecodeTestResult = reader.ReadString(5),
				CameraDriverTestResult = reader.ReadString(6),
				TouchscreenDriverTestResult = reader.ReadString(7),
				InitTestResult = reader.ReadString(8)
			};
			rv.Add(item);
		}, query);
		return rv;
	}

	public bool Clean()
	{
		return false;
	}

	internal void OnUpgrade(ErrorList errors, TextWriter writer)
	{
		if (!Store.TableExists("KioskFunctionCheckData"))
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("CREATE TABLE [{0}] ", "KioskFunctionCheckData");
			stringBuilder.Append("( [SampleDate] DateTime NOT NULL,");
			stringBuilder.Append(" [UserToken] VarChar(64) NOT NULL,");
			stringBuilder.Append(" [VerticalTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [VendDoorTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [TrackTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [SnapDecodeTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [CameraDriverTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [TouchScreenDriverTestResult] VarChar(16) NOT NULL,");
			stringBuilder.Append(" [InitTestResult] VarChar(16) NOT NULL )");
			if (!Store.CreateTable(stringBuilder.ToString()))
			{
				errors.Add(Error.NewError("I0777", "Create table error.", "Unable to create kiosk function check table in halcounters.vdb3."));
			}
			else
			{
				writer.WriteLine("Successfully created {0}", "KioskFunctionCheckData");
			}
		}
	}

	internal KioskFunctionCheckService(IDataStore store)
	{
		Store = store;
	}
}

using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Redbox.HAL.Configuration;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.DataStorage;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Description("The CONFIG command is reponsible for loading, retrieving and setting configuration for individual HAL driver systems.")]
[Command("config")]
public sealed class ConfigurationCommand
{
	[Description("Forces the HAL Service to reload the HAL.xml configuration file or the file specified by the optional path parameter.")]
	[Usage("config load [path: path-to-config]")]
	[CommandForm(Name = "load")]
	public void LoadConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = "$(RunningPath)\\HAL.xml";
		}
		path = RuntimeService.Instance.ExpandConstantMacros(path);
		if (!File.Exists(path))
		{
			context.Errors.Add(Error.NewError("S001", $"The file '{path}' does not exist.", "Use $(RunningPath) macro to easily reference relative paths from the service working path."));
			return;
		}
		ErrorList collection = LoadConfigInner(path);
		context.Errors.AddRange(collection);
	}

	[Description("Forces the HAL Service to save its current state to the HAL.xml configuration file or the file specified by the optional path parameter.")]
	[Usage("config save [path: path-to-config]")]
	[CommandForm(Name = "save")]
	public void SaveConfiguration(CommandContext context, [CommandKeyValue(IsRequired = false)] string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = "$(RunningPath)\\HAL.xml";
		}
		path = RuntimeService.Instance.ExpandConstantMacros(path);
		if (!File.Exists(path))
		{
			context.Errors.Add(Error.NewError("S001", $"The file '{path}' does not exist.", "Use $(RunningPath) macro to easily reference relative paths from the service working path."));
			return;
		}
		BarcodeConfiguration.Instance.Save(path, context.Errors);
		ControllerConfiguration.Instance.Save(path, context.Errors);
	}

	[Description("Forces the HAL Service to reload its configuration from the optional backup path parameter (if not specified, it assumes %TEMP%\\hal.xml); the new configuration is saved.")]
	[Usage("config restore [path: path-to-backup]")]
	[CommandForm(Name = "restore")]
	public void ReloadConfigurationFromBackup(CommandContext context, [CommandKeyValue(IsRequired = false)] string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			string environmentVariable = Environment.GetEnvironmentVariable("TEMP");
			path = Path.Combine(environmentVariable, "HAL.xml");
		}
		LoadConfiguration(context, path);
		SaveConfiguration(context, string.Empty);
	}

	[Usage("config get name: config-name")]
	[CommandForm(Name = "get")]
	public void GetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			context.Errors.Add(Error.NewError("S001", "The parameter name: is required.", "Specify the name: parameter when calling the CONFIG GET command form."));
		}
		else if (string.Compare(name, "camera", ignoreCase: true) == 0)
		{
			ProtocolHelper.FormatCompressedString(BarcodeConfiguration.Instance.FormatAsXml(), context);
		}
		else if (string.Compare(name, "controller", ignoreCase: true) == 0)
		{
			ProtocolHelper.FormatCompressedString(ControllerConfiguration.Instance.FormatAsXml(), context);
		}
		else
		{
			context.Errors.Add(Error.NewError("S001", $"The requested configuration name '{name}' is not known.", "Specify one of the valid configuration names: CAMERA, BARCODE, CONTROLLER."));
		}
	}

	[CommandForm(Name = "set")]
	[Usage("config set name: config-name data: base64-encoded-config")]
	public void SetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64, CompressionType = CompressionType.LZMA)] byte[] data)
	{
		if (data == null)
		{
			context.Errors.Add(Error.NewError("S001", "The parameter data: is required.", "Specify the data: parameter when calling the CONFIG SET command form."));
			return;
		}
		if (string.IsNullOrEmpty(name))
		{
			context.Errors.Add(Error.NewError("S001", "The parameter name: is required.", "Specify the name: parameter when calling the CONFIG SET command form."));
			return;
		}
		string xml = Encoding.ASCII.GetString(data);
		if (string.Compare(name, "camera", ignoreCase: true) == 0)
		{
			BarcodeConfiguration.Instance.UpdateFromXml(xml);
		}
		else if (string.Compare(name, "controller", ignoreCase: true) == 0)
		{
			ControllerConfiguration.Instance.UpdateFromXml(xml);
		}
		else
		{
			context.Errors.Add(Error.NewError("S001", $"The requested configuration name '{name}' is not known.", "Specify one of the valid configuration names: BARCODE, CONTROLLER."));
		}
	}

	[Usage("config get-inventory-state")]
	[CommandForm(Name = "get-inventory-state")]
	public void GetInventoryState(CommandContext context)
	{
		ProtocolHelper.FormatCompressedString(ControllerConfiguration.Instance.GetInventoryState(), context);
	}

	[CommandForm(Name = "set-inventory-state")]
	[Usage("config set-inventory-state data: base64-encoded-state")]
	public void SetInventoryState(CommandContext context, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64, CompressionType = CompressionType.LZMA)] byte[] data)
	{
		if (data == null)
		{
			context.Errors.Add(Error.NewError("S001", "The parameter data: is required.", "Specify the data: parameter when calling the CONFIG set-inventory-state command form."));
			return;
		}
		string text = FileHelper.GenerateUniqueFile("xml");
		try
		{
			File.WriteAllText(Path.Combine(StoreManager.Instance.DataDirectory, text), Encoding.ASCII.GetString(data));
			ExecutionContext executionContext = ExecutionEngine.Instance.ScheduleJob("reset-inventory-job", string.Empty, DateTime.Now, ProgramPriority.Low);
			executionContext.EnableDebugging = false;
			executionContext.PushTop(text);
			LogHelper.Instance.Log("Reset-Inventory job ( job ID = {0} ).", executionContext.ID);
			executionContext.Pend();
			executionContext.WaitForCompletion();
			ExecutionStatus status = executionContext.Status;
			context.Messages.Add(status.ToString());
		}
		catch
		{
			context.Messages.Add("ERRORED");
			LogHelper.Instance.Log("Reset-inventory job failed; file = {0}", text);
		}
	}

	public ErrorList LoadConfiguration(string path)
	{
		return LoadConfigInner(path);
	}

	private ErrorList LoadConfigInner(string path)
	{
		ErrorList errorList = new ErrorList();
		BarcodeConfiguration.Instance.Load(path, errorList);
		ControllerConfiguration.Instance.Load(path, errorList);
		ControllerConfiguration.Instance.Initialize(errorList);
		return errorList;
	}
}

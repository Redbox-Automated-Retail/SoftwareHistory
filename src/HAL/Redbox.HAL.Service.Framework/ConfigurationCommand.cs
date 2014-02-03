using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using Redbox.HAL.Camera.Framework;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;
using Redbox.HAL.DataMatrix.Framework;
using Redbox.HAL.IPC.Framework;
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
		path = MacroContainer.Instance.ExpandConstantMacros(path);
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
		path = MacroContainer.Instance.ExpandConstantMacros(path);
		if (!File.Exists(path))
		{
			context.Errors.Add(Error.NewError("S001", $"The file '{path}' does not exist.", "Use $(RunningPath) macro to easily reference relative paths from the service working path."));
			return;
		}
		CameraConfiguration.Instance.Save(path, context.Errors);
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

	[CommandForm(Name = "get")]
	[Usage("config get name: config-name")]
	public void GetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			context.Errors.Add(Error.NewError("S001", "The parameter name: is required.", "Specify the name: parameter when calling the CONFIG GET command form."));
		}
		else if (string.Compare(name, "camera", ignoreCase: true) == 0)
		{
			ProtocolHelper.FormatCompressedString(CameraConfiguration.Instance.FormatAsXml(), context);
		}
		else if (string.Compare(name, "barcode", ignoreCase: true) == 0)
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

	[Usage("config set name: config-name data: base64-encoded-config")]
	[CommandForm(Name = "set")]
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
			CameraConfiguration.Instance.UpdateFromXml(xml);
		}
		else if (string.Compare(name, "barcode", ignoreCase: true) == 0)
		{
			BarcodeConfiguration.Instance.UpdateFromXml(xml);
		}
		else if (string.Compare(name, "controller", ignoreCase: true) == 0)
		{
			ControllerConfiguration.Instance.UpdateFromXml(xml);
		}
		else
		{
			context.Errors.Add(Error.NewError("S001", $"The requested configuration name '{name}' is not known.", "Specify one of the valid configuration names: CAMERA, BARCODE, CONTROLLER."));
		}
	}

	[CommandForm(Name = "get-inventory-state")]
	[Usage("config get-inventory-state")]
	public void GetInventoryState(CommandContext context)
	{
		ProtocolHelper.FormatCompressedString(ControllerConfiguration.Instance.GetInventoryState(), context);
	}

	[Usage("config set-inventory-state data: base64-encoded-state")]
	[CommandForm(Name = "set-inventory-state")]
	public void SetInventoryState(CommandContext context, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64, CompressionType = CompressionType.LZMA)] byte[] data)
	{
		if (data == null)
		{
			context.Errors.Add(Error.NewError("S001", "The parameter data: is required.", "Specify the data: parameter when calling the CONFIG set-inventory-state command form."));
		}
		else
		{
			ControllerConfiguration.Instance.ResetInventoryState(Encoding.ASCII.GetString(data), context.Errors);
		}
	}

	public ErrorList LoadConfiguration(string path)
	{
		return LoadConfigInner(path);
	}

	private ErrorList LoadConfigInner(string path)
	{
		ErrorList errorList = new ErrorList();
		CameraConfiguration.Instance.Load(path, errorList);
		BarcodeConfiguration.Instance.Load(path, errorList);
		ControllerConfiguration.Instance.Load(path, errorList);
		ControllerConfiguration.Instance.Initialize(errorList);
		return errorList;
	}
}

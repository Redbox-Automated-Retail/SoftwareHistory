using System;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Xml;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Configuration;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Description("The CONFIG command is reponsible for loading, retrieving and setting configuration for individual HAL driver systems.")]
[Command("config")]
public sealed class ConfigurationCommand
{
	[Usage("config save [path: path-to-config]")]
	[Description("Forces the HAL Service to save its current state to the HAL.xml configuration file or the file specified by the optional path parameter.")]
	[CommandForm(Name = "save")]
	public void SaveConfiguration(CommandContext context, [CommandKeyValue(IsRequired = false)] string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			path = "$(RunningPath)\\HAL.xml";
		}
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		path = service.ExpandConstantMacros(path);
		if (!File.Exists(path))
		{
			context.Errors.Add(Error.NewError("S001", $"The file '{path}' does not exist.", "Use $(RunningPath) macro to easily reference relative paths from the service working path."));
			return;
		}
		IConfigurationService service2 = ServiceLocator.Instance.GetService<IConfigurationService>();
		service2.Save(context.Errors);
	}

	[Usage("config get name: config-name")]
	[CommandForm(Name = "get")]
	public void GetConfiguration(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			context.Errors.Add(Error.NewError("S001", "The parameter name: is required.", "Specify the name: parameter when calling the CONFIG GET command form."));
			return;
		}
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		string value = service.FormatAsXml(name);
		if (string.IsNullOrEmpty(value))
		{
			context.Errors.Add(Error.NewError("S001", $"The requested configuration name '{name}' is not known.", "Specify one of the valid configuration names."));
		}
		else
		{
			ProtocolHelper.FormatCompressedString(value, context);
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
		string data2 = Encoding.ASCII.GetString(data);
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		service.UpdateFromXml(name, data2, context.Errors);
	}

	[CommandForm(Name = "get-inventory-state")]
	[Usage("config get-inventory-state")]
	public void GetInventoryState(CommandContext context)
	{
		using MemoryStream memoryStream = new MemoryStream();
		XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.ASCII);
		xmlTextWriter.Formatting = Formatting.Indented;
		XmlTextWriter xmlTextWriter2 = xmlTextWriter;
		xmlTextWriter2.WriteStartDocument();
		xmlTextWriter2.WriteStartElement("inventory-items");
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		service.GetState(xmlTextWriter2);
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		service2.GetState(xmlTextWriter2);
		xmlTextWriter2.WriteEndElement();
		xmlTextWriter2.WriteEndDocument();
		xmlTextWriter2.Flush();
		ProtocolHelper.FormatCompressedString(Encoding.ASCII.GetString(memoryStream.ToArray()), context);
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
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string text = FileHelper.GenerateUniqueFile("xml");
		try
		{
			File.WriteAllText(Path.Combine(service.DataPath, text), Encoding.ASCII.GetString(data));
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
}

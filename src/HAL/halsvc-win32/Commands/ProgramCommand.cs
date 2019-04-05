using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Attributes;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Win32.Commands;

[Command("program")]
public sealed class ProgramCommand
{
	[Usage("PROGRAM compile path: 'C:\\temp\\my-script.hs' [name: program-name] [requires-client-connection: true|false]")]
	[CommandForm(Name = "compile")]
	public void Compile(CommandContext context, [CommandKeyValue(IsRequired = true)] string path, string name, [CommandKeyValue(KeyName = "requires-client-connection")] bool? requiresClientConnection)
	{
		if (string.IsNullOrEmpty(path))
		{
			context.Errors.Add(Error.NewError("S001", "The path: parameter is required.", "Issue the PROGRAM compile command again passing in a valid path: parameter."));
		}
		else
		{
			CompileProgram(context.Errors, name, path);
		}
	}

	[Usage("PROGRAM remove name: progam-name")]
	[CommandForm(Name = "remove")]
	public void Remove(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		service.RemoveProgram(name);
	}

	[CommandForm(Name = "get")]
	[Usage("PROGRAM get name: program-name")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string fileName = service.ExpandConstantMacros($"$(RunningPath)\\..\\scripts\\{name}.hs");
		ProtocolHelper.FormatCompressedBytes(service.ReadToBuffer(fileName), context);
	}

	[Usage("PROGRAM set name: program-name data: base4-encoded-script [requires-client-connection: true|false]")]
	[CommandForm(Name = "set")]
	public void Set(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64, CompressionType = CompressionType.LZMA)] byte[] data, [CommandKeyValue(KeyName = "requires-client-connection")] bool? requiresClientConnection)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		string path = service.ExpandConstantMacros($"$(RunningPath)\\..\\scripts\\{name}.hs");
		File.WriteAllBytes(path, data);
		CompileProgram(context.Errors, name, path);
	}

	[Usage("PROGRAM properties name: program-name requires-client-connection: true|false")]
	[CommandForm(Name = "properties")]
	public void Properties(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(KeyName = "requires-client-connection")] bool requiresClientConnection)
	{
		context.Errors.Add(Error.NewError("S002", "Unsupported call.", "The requires-client-connection form is no longer supported."));
	}

	[Usage("PROGRAM list")]
	[CommandForm(Name = "list")]
	public void List(CommandContext context)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		context.Messages.AddRange(service.GetProgramList());
	}

	private static void CompileProgram(ErrorList errors, string programName, string path)
	{
		if (!File.Exists(path))
		{
			errors.Add(Error.NewError("S001", $"The file '{path}' does not exist.", "Check the path of the script file and reissue the compile command."));
			return;
		}
		if (programName == null)
		{
			programName = Path.GetFileNameWithoutExtension(path);
		}
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
		using MemoryStream stream = new MemoryStream(service2.ReadToBuffer(path));
		IExecutionResult executionResult = service.Compile(stream, programName);
		errors.AddRange(executionResult.Errors);
	}
}

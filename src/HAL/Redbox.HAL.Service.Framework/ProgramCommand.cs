using System;
using System.Collections.Generic;
using System.IO;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.HAL.Script.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Service.Framework;

[Command("program")]
public sealed class ProgramCommand
{
	[CommandForm(Name = "compile")]
	[Usage("PROGRAM compile path: 'C:\\temp\\my-script.hs' [name: program-name] [requires-client-connection: true|false]")]
	public void Compile(CommandContext context, [CommandKeyValue(IsRequired = true)] string path, string name, [CommandKeyValue(KeyName = "requires-client-connection")] bool? requiresClientConnection)
	{
		if (string.IsNullOrEmpty(path))
		{
			context.Errors.Add(Error.NewError("S001", "The path: parameter is required.", "Issue the PROGRAM compile command again passing in a valid path: parameter."));
		}
		else
		{
			CompileProgram(context.Errors, name, path, requiresClientConnection ?? false);
		}
	}

	[Usage("PROGRAM remove name: progam-name")]
	[CommandForm(Name = "remove")]
	public void Remove(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		ExecutionEngine.Instance.RemoveProgram(name);
		RewriteServiceStartup();
	}

	[CommandForm(Name = "add-search-path")]
	[Usage("PROGRAM add-search-path paths: 'c:\\path1;c:\\path1\\path2'")]
	public void AddSearchPath(CommandContext context, [CommandKeyValue(IsRequired = true)] string paths)
	{
		Compiler.AppendSearchPath(paths);
	}

	[CommandForm(Name = "get")]
	[Usage("PROGRAM get name: program-name")]
	public void Get(CommandContext context, [CommandKeyValue(IsRequired = true)] string name)
	{
		string fileName = MacroContainer.Instance.ExpandConstantMacros($"$(RunningPath)\\..\\scripts\\{name}.hs");
		ProtocolHelper.FormatCompressedBytes(FileHelper.ReadToBuffer(fileName), context);
	}

	[CommandForm(Name = "set")]
	[Usage("PROGRAM set name: program-name data: base4-encoded-script [requires-client-connection: true|false]")]
	public void Set(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(IsRequired = true, BinaryEncoding = BinaryEncoding.Base64, CompressionType = CompressionType.LZMA)] byte[] data, [CommandKeyValue(KeyName = "requires-client-connection")] bool? requiresClientConnection)
	{
		string path = MacroContainer.Instance.ExpandConstantMacros($"$(RunningPath)\\..\\scripts\\{name}.hs");
		File.WriteAllBytes(path, data);
		CompileProgram(context.Errors, name, path, requiresClientConnection ?? false);
		RewriteServiceStartup();
	}

	[CommandForm(Name = "properties")]
	[Usage("PROGRAM properties name: program-name requires-client-connection: true|false")]
	public void Properties(CommandContext context, [CommandKeyValue(IsRequired = true)] string name, [CommandKeyValue(KeyName = "requires-client-connection")] bool requiresClientConnection)
	{
		context.Errors.Add(Error.NewError("S002", "Unsupported call.", "The requires-client-connection form is no longer supported."));
	}

	[CommandForm(Name = "list")]
	[Usage("PROGRAM list")]
	public void List(CommandContext context)
	{
		context.Messages.AddRange(ExecutionEngine.Instance.GetPrograms());
	}

	private static void CompileProgram(ErrorList errors, string programName, string path, bool requiresClientConnection)
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
		using MemoryStream stream = new MemoryStream(FileHelper.ReadToBuffer(path));
		ExecutionResult executionResult = ExecutionEngine.Instance.Compile(stream, programName);
		errors.AddRange(executionResult.Errors);
	}

	private static void RewriteServiceStartup()
	{
		string path = MacroContainer.Instance.ExpandConstantMacros("$(RunningPath)\\ServiceStartup.txt");
		List<string> list = new List<string>(File.ReadAllLines(path));
		int num = list.FindIndex((string each) => each.Contains("[["));
		int num2 = list.FindIndex((string each) => each.Contains("]]"));
		if (num != -1 && num2 != -1)
		{
			num++;
			list.RemoveRange(num, num2 - num);
			string[] programs = ExecutionEngine.Instance.GetPrograms();
			foreach (string text in programs)
			{
				string[] array = text.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				list.Insert(num, string.Format("PROGRAM compile path: @'$(RunningPath)\\..\\Scripts\\{0}.hs'{1}", array[0], (string.Compare(array[2], true.ToString(), ignoreCase: true) == 0) ? " requires-client-connection: true" : string.Empty));
			}
			File.WriteAllLines(path, list.ToArray());
		}
	}
}

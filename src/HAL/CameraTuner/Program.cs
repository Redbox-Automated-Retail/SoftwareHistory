using System;
using System.Windows.Forms;
using Redbox.HAL.Core;

namespace Redbox.HAL.CameraTuner;

public class Program : IDisposable
{
	private readonly Guid AppGuid = new Guid("96C0A029-9835-41B5-8F45-53E9AD339B6D");

	private bool Disposed { get; set; }

	private NamedLock InstanceLock { get; set; }

	public void Dispose()
	{
		DisposeInner(fromDispose: true);
	}

	~Program()
	{
		DisposeInner(fromDispose: false);
	}

	public void Run(string[] args)
	{
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		bool snapOnly = false;
		bool hasIR = false;
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "-snaptest")
			{
				snapOnly = true;
			}
			else if (args[i].Equals("-secure", StringComparison.CurrentCultureIgnoreCase))
			{
				hasIR = true;
			}
		}
		Application.Run(new MainForm(snapOnly, hasIR));
	}

	[STAThread]
	public static void Main(string[] args)
	{
		using Program program = new Program();
		if (!program.InstanceLock.IsOwned)
		{
			Environment.Exit(10);
		}
		if (RegistryOptions.Instance.GetSecretValue("SOFTWARE\\Redbox\\HAL", "TunerFailure", def: false))
		{
			Environment.Exit(2);
		}
		program.Run(args);
	}

	public Program()
	{
		InstanceLock = new NamedLock(AppGuid.ToString(), LockScope.Local);
	}

	private void DisposeInner(bool fromDispose)
	{
		if (!Disposed)
		{
			Disposed = true;
			InstanceLock.Dispose();
		}
	}
}

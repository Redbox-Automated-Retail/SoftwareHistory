using System;
using System.Windows.Forms;
using Redbox.HAL.CameraTuner.Properties;
using Redbox.HAL.Component.Model;
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
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "-snaptest")
			{
				snapOnly = true;
			}
		}
		Application.Run(new MainForm(snapOnly));
	}

	[STAThread]
	public static void Main(string[] args)
	{
		using Program program = new Program();
		if (!program.InstanceLock.IsOwned)
		{
			Environment.Exit(10);
		}
		ServiceLocator.Instance.AddService(typeof(IEncryptionService), new TripleDesEncryptionService());
		if (Settings.Default.EmulatedTunerFailure)
		{
			Environment.Exit(2);
		}
		program.Run(args);
	}

	public Program()
	{
		InstanceLock = new NamedLock(AppGuid.ToString());
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

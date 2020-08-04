using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace UpdateClientService.API;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration();

	public static void Main(string[] args)
	{
		bool flag = !Debugger.IsAttached && !Enumerable.Contains(args, "--console");
		if (flag)
		{
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			string directoryName = Path.GetDirectoryName(fileName);
			Directory.SetCurrentDirectory(directoryName);
		}
		Logger logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
		Log.Logger = logger;
		try
		{
			Log.Information("------------------------------------ Starting web host -------------------------------------");
			Log.Information("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
			Log.Information($"Service version: {Assembly.GetExecutingAssembly().GetName().Version}");
			if (!flag)
			{
				Log.Information("Program.Main - Starting as a console application");
				CreateWebHostBuilder(args).Build().Run();
			}
			else
			{
				Log.Information("Program.Main - Starting as a Windows Service");
				CreateWebHostBuilder(args).Build().RunAsDeviceServiceService();
			}
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "Program.Main - Web host terminated with error");
		}
		finally
		{
			Log.Information("------------------------------------ Stopping web host -------------------------------------");
			Log.CloseAndFlush();
		}
	}

	public static IWebHostBuilder CreateWebHostBuilder(string[] args)
	{
		return WebHost.CreateDefaultBuilder(args).UseSerilog().UseConfiguration(Configuration)
			.UseStartup<Startup>();
	}

	private static IConfiguration GetConfiguration()
	{
		IConfigurationBuilder configurationBuilder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
		if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null)
		{
			configurationBuilder.AddJsonFile("appsettings." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json", optional: true, reloadOnChange: true);
		}
		return configurationBuilder.Build();
	}
}

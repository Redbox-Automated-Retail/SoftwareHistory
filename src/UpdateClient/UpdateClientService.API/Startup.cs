using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using UpdateClientService.API.App;
using UpdateClientService.API.Services;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Certificate;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.Controller;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;

namespace UpdateClientService.API;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
		services.AddSingleton<IStoreService, StoreService>();
		services.AddSingleton<IMqttProxy, AwsIoTMqttProxy>();
		services.AddSingleton<ICertificateService, CertificateService>();
		services.AddSingleton<IIoTCertificateServiceApiClient, IoTCertificateServiceApiClient>();
		services.AddSingleton<IPersistentDataCacheService, PersistentDataCacheService>();
		services.AddTransient<IIoTCommandService, IoTCommandService>();
		services.AddTransient<IIoTService, IoTService>();
		services.AddTransient<IKioskFilesService, KioskFilesService>();
		services.AddTransient<IDownloadFilesService, DownloadFilesService>();
		services.AddScheduler();
		services.AddHttpClient<IHttpService, HttpService>().ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
		{
			AllowAutoRedirect = true,
			UseCookies = true,
			CookieContainer = new CookieContainer()
		});
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			c.SwaggerDoc("v1", new OpenApiInfo
			{
				Title = "Update Client API",
				Version = "v1"
			});
		});
		RegisterICommand(services);
	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseHsts();
		}
		app.UseHttpsRedirection();
		app.UseSwagger();
		app.UseSwaggerUI(delegate(SwaggerUIOptions c)
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
		});
		IIoTCommandService ioTCommandService = app.ApplicationServices.GetRequiredService<IIoTCommandService>();
		IMqttProxy mqttProxy = app.ApplicationServices.GetRequiredService<IMqttProxy>();
		Task.Run(async delegate
		{
			TimeSpan delay = (Debugger.IsAttached ? TimeSpan.FromSeconds(5.0) : TimeSpan.FromMinutes(2.0));
			await Task.Delay(delay).ContinueWith((Func<Task, Task>)async delegate
			{
				app.ApplicationServices.UseScheduler(delegate(IScheduler scheduler)
				{
					scheduler.ScheduleAsync(async delegate
					{
						await mqttProxy.CheckConnectionAsync();
					}).EveryMinute();
				});
				await ioTCommandService.SubscribeToCommandTopics();
			});
		});
		IDownloadFilesService downloadFileService = app.ApplicationServices.GetRequiredService<IDownloadFilesService>();
		app.ApplicationServices.UseScheduler(delegate(IScheduler scheduler)
		{
			if (env.IsDevelopment())
			{
				scheduler.ScheduleAsync(async delegate
				{
					await downloadFileService.HandleScheduledJobs();
				}).EveryMinute().PreventOverlapping("IDownloadFilesService");
			}
			scheduler.ScheduleAsync(async delegate
			{
				await downloadFileService.HandleScheduledJobs();
			}).EveryFiveMinutes().PreventOverlapping("IDownloadFilesService");
		});
		app.UseMvc();
	}

	private void RegisterICommand(IServiceCollection services)
	{
		Type icommandType = typeof(ICommandIoTController);
		IEnumerable<Type> enumerable = from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
			where icommandType.IsAssignableFrom(p) && !p.IsInterface
			select p;
		List<ICommandIoTController> list = new List<ICommandIoTController>();
		foreach (Type item in enumerable)
		{
			services.AddTransient(typeof(ICommandIoTController), item);
			list.Add(item as ICommandIoTController);
		}
		services.AddTransient(typeof(List<ICommandIoTController>), list.GetType());
	}
}

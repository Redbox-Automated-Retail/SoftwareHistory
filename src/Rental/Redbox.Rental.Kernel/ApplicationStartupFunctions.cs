using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Services;

namespace Redbox.Rental.Kernel;

public static class ApplicationStartupFunctions
{
	[KernelFunction(Name = "kernel.create_idle_timer")]
	public static void CreateIdleTimer(string idleTimerName)
	{
		ServiceLocator.Instance.GetService<IIdleTimerService>()?.CreateIdleTimer(idleTimerName);
	}

	[KernelFunction(Name = "kernel.initialize_idle_timer")]
	public static void InitializeIdleTimer(string idleTimerName, int timeout, Action action)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Initialize(timeout, action);
	}

	[KernelFunction(Name = "kernel.suspend_idle_timer")]
	public static void SuspendIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Suspend();
	}

	[KernelFunction(Name = "kernel.resume_idle_timer")]
	public static void ResumeIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Resume();
	}

	[KernelFunction(Name = "kernel.start_idle_timer")]
	public static void StartIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Start();
	}

	[KernelFunction(Name = "kernel.stop_idle_timer")]
	public static void StopIdleTimer(string idleTimerName)
	{
		(ServiceLocator.Instance.GetService<IIdleTimerService>()?.GetIdleTimer(idleTimerName))?.Stop();
	}

	[KernelFunction(Name = "kernel.is_collections_popup_enabled")]
	public static bool IsCollectionsPopupEnabled()
	{
		return OrchestrationService.IsCollectionsPopupEnabled();
	}

	[KernelFunction(Name = "kernel.is_collections_popup_timer_enabled")]
	public static bool IsCollectionsPopupTimerEnabled()
	{
		return OrchestrationService.IsCollectionsPopupTimerEnabled();
	}

	[KernelFunction(Name = "kernel.get_idle_timer_timeout")]
	public static int GetIdleTimerTimeout()
	{
		return OrchestrationService.GetIdleTimerTimeout();
	}

	[KernelFunction(Name = "kernel.reset_application_idle_timer_timeout")]
	public static void ResetApplicationIdleTimerTimeout()
	{
		OrchestrationService.ResetApplicationIdleTimerTimeout();
	}

	[KernelFunction(Name = "kernel.start_the_rental_application")]
	public static void StartTheRentalApplication()
	{
		ServiceLocator.Instance.GetService<IRedboxRental>()?.StartTheRentalApplication();
	}
}

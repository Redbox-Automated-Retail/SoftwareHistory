using System;
using System.Collections.Generic;

namespace Redbox.WUA;

internal interface IManager
{
	bool IsBusy { get; }

	bool DownloadInProgress { get; }

	int DownloadProgress { get; }

	bool InstallInProgress { get; }

	int InstallProgress { get; }

	bool RebootRequiredToFinishInstall { get; }

	ErrorList ListAll(out List<Update> updates);

	ErrorList ListInstalled(out List<Update> updates);

	ErrorList ListAvailable(out List<Update> updates);

	ErrorList ListDownloadedNotInstalled(out List<Update> updates);

	ErrorList IsAvailable(Guid guid, out bool isAvailable);

	ErrorList IsInstalled(Guid guid, out bool isInstalled);

	ErrorList EnqueueDownload(List<Guid> guids);

	ErrorList EnqueueDownload(Guid guid);

	ErrorList DequeueDownload(Guid id);

	List<Update> ListDownloadQueue();

	ErrorList BeginDownload();

	List<Update> CurrentlyDownloading();

	ErrorList AbortDownload();

	ErrorList EnqueueInstall(List<Guid> guids);

	ErrorList EnqueueInstall(Guid guid);

	ErrorList DequeueInstall(Guid id);

	List<Update> ListInstallQueue();

	ErrorList BeginInstall();

	List<Update> CurrentlyInstalling();

	ErrorList AbortInstall();

	ErrorList GetAsyncErrors();
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelExtensionService
{
	ReadOnlyCollection<IKernelExtension> Extensions { get; }

	string[] SearchPath { get; set; }

	ErrorList Initialize(string applicationPath);

	IKernelExtension GetExtension(Guid id);

	ErrorList EnumerateExtensions(List<string> names);

	ErrorList LoadExtension(string path, out string name);

	ErrorList LoadAllExtensions();

	ErrorList ResetAllExtensions();

	ErrorList ActivateAllExtensions();

	ErrorList DeactivateAllExtensions();

	bool CanSwitch(out ErrorList errors);

	void NotifyOfHostCrash(Exception e);
}

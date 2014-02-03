using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml;
using Microsoft.Win32;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.PackageInstaller;

internal class PackageInstaller
{
	private const string CommonFilesDirectory = "C:\\Program Files\\Redbox\\CommonFiles";

	private const string KioskLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs";

	private const string DefaultQRStateFile = "c:\\Program Files\\Redbox\\HALService\\bin\\qr.defs.default";

	private const string InventoryDBName = "HALData.vdb3";

	private const string CountersDBName = "HALCounters.vdb3";

	private const string m_videocapActiveXControl = "C:\\Program Files\\VideoCap ActiveX Control\\videocap.ocx";

	private bool emulatedRegsvr;

	private static string m_installLogsDirectory;

	private string QRStateFile => Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr.state");

	private string DeprecatedQRStateFile => Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr.defs");

	private string InstalledVersion
	{
		get
		{
			string secretValue = RegistryOptions.Instance.GetSecretValue("SOFTWARE\\Redbox\\HAL", "Product Version", "NONE");
			if (!("NONE" == secretValue))
			{
				return secretValue;
			}
			return null;
		}
	}

	private int MachineSlotConfiguration
	{
		get
		{
			RegistryKey registryKey = null;
			int? num = null;
			try
			{
				registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store");
				if (registryKey == null)
				{
					return -1;
				}
				object value = registryKey.GetValue("MaxSlots");
				num = (int?)Convert.ChangeType(value, typeof(int));
			}
			catch
			{
				return -1;
			}
			finally
			{
				registryKey?.Close();
			}
			return num ?? (-1);
		}
	}

	private string AssemblyPath => RuntimeService.Instance.AssemblyDirectory;

	private static string InstallLogsDirectory
	{
		get
		{
			if (m_installLogsDirectory != null)
			{
				return m_installLogsDirectory;
			}
			string fullPath = Path.GetFullPath(Path.Combine(RuntimeService.Instance.AssemblyDirectory, ".."));
			return m_installLogsDirectory = Path.Combine(fullPath, "InstallLogs");
		}
	}

	private void Verify(TextWriter writer, List<CommonFile> commonFiles)
	{
		RegistryKey registryKey = null;
		try
		{
			registryKey = Registry.ClassesRoot.OpenSubKey("CLSID");
			foreach (CommonFile commonFile in commonFiles)
			{
				string cLSID = commonFile.CLSID;
				if (registryKey.OpenSubKey(cLSID) == null)
				{
					writer.WriteLine($"ERROR: no CLSID found for {cLSID}");
					continue;
				}
				string name = $"CLSID\\{cLSID}\\InprocServer32";
				RegistryKey registryKey2 = null;
				try
				{
					registryKey2 = Registry.ClassesRoot.OpenSubKey(name);
					if (registryKey2 != null)
					{
						string text = (string)registryKey2.GetValue(string.Empty);
						if (!string.IsNullOrEmpty(text))
						{
							writer.WriteLine($" found path {text}");
						}
						else
						{
							writer.WriteLine(" didn't find a dll path");
						}
					}
				}
				catch (Exception)
				{
				}
				finally
				{
					registryKey2?.Close();
				}
			}
		}
		catch (Exception ex2)
		{
			writer.WriteLine("--- Verify: caught generic exception ---");
			writer.WriteLine(ex2.Message);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private bool RegisterServer(TextWriter log, string dllpath, bool uninstall)
	{
		ProcessStartInfo processStartInfo = ProcessHelper.SetupDefaultProcessInfo("Regsvr32.exe");
		processStartInfo.Arguments = string.Format("/s {0} \"{1}\"", uninstall ? "/u" : string.Empty, dllpath);
		Process process = null;
		log.Write($"Executing \"Regsvr {processStartInfo.Arguments}\"");
		if (emulatedRegsvr)
		{
			log.WriteLine("Regsvr32 success!");
			return true;
		}
		bool result = false;
		try
		{
			process = Process.Start(processStartInfo);
			process.WaitForExit();
			_ = process.ExitCode;
			if (process.ExitCode != 0)
			{
				log.WriteLine($"regsvr32 failed with code {process.ExitCode}");
			}
			else
			{
				log.WriteLine("Regsvr32 success!");
				result = true;
			}
		}
		catch (InvalidOperationException value)
		{
			log.WriteLine("-- Caught InvalidOperationException --");
			log.WriteLine(value);
		}
		catch
		{
			log.WriteLine("-- Caught generic exception -- ");
		}
		return result;
	}

	private void UpdateProductVersionKey(TextWriter writer)
	{
		RegistryKey registryKey = null;
		string text = Path.Combine(AssemblyPath, "halsvc-win32.exe");
		try
		{
			AssemblyName assemblyName = AssemblyName.GetAssemblyName(text);
			writer.WriteLine("Service has version {0}", assemblyName.Version.ToString());
			string subkey = "SOFTWARE\\Redbox\\HAL";
			registryKey = Registry.LocalMachine.CreateSubKey(subkey);
			registryKey?.SetValue("Product Version", assemblyName.Version.ToString());
		}
		catch (Exception ex)
		{
			Console.WriteLine("Unable to get set for {0}; message = {1}", text, ex.Message);
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private void InstallCommonFiles(TextWriter writer, string assemblyPath, List<CommonFile> commonFiles)
	{
		RegistryKey registryKey = null;
		try
		{
			foreach (CommonFile commonFile in commonFiles)
			{
				if (!commonFile.Replace)
				{
					continue;
				}
				string text = Path.Combine(AssemblyPath, commonFile.File);
				string sourceFileName = text + ".hash";
				string text2 = Path.Combine("C:\\Program Files\\Redbox\\CommonFiles", commonFile.File);
				string destFileName = text2 + ".hash";
				try
				{
					File.Copy(text, text2, overwrite: true);
					File.Copy(sourceFileName, destFileName, overwrite: true);
				}
				catch (Exception)
				{
					writer.WriteLine("Unable to copy clear image files.");
				}
				string name = $"CLSID\\{commonFile.CLSID}\\InprocServer32";
				registryKey = Registry.ClassesRoot.OpenSubKey(name);
				if (registryKey != null)
				{
					string value = (string)registryKey.GetValue(string.Empty);
					if (!string.IsNullOrEmpty(value) && !text2.Equals(value, StringComparison.CurrentCultureIgnoreCase))
					{
						RegisterServer(writer, text2, uninstall: false);
					}
				}
				else
				{
					RegisterServer(writer, text2, uninstall: false);
				}
				if (registryKey != null)
				{
					registryKey.Close();
					registryKey = null;
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
	}

	private void CreateDirectories(TextWriter writer)
	{
		bool flag = RuntimeService.Instance.GetPlatform() == Platform.Windows7;
		try
		{
			if (!Directory.Exists("C:\\Program Files\\Redbox\\KioskLogs"))
			{
				Directory.CreateDirectory("C:\\Program Files\\Redbox\\KioskLogs");
			}
		}
		catch (Exception ex)
		{
			writer.WriteLine("Unable to create directory: {0}", ex.Message);
		}
		if (!flag)
		{
			try
			{
				if (!Directory.Exists("C:\\Program Files\\Redbox\\CommonFiles"))
				{
					Directory.CreateDirectory("C:\\Program Files\\Redbox\\CommonFiles");
				}
			}
			catch (Exception ex2)
			{
				writer.WriteLine("Unable to create directory: {0}", ex2.Message);
			}
		}
		string path = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "data");
		try
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
		catch (Exception ex3)
		{
			writer.WriteLine("Unable to create data directory: {0}", ex3.Message);
		}
		string path2 = "c:\\Program Files\\Redbox\\HALService\\Video";
		try
		{
			if (!Directory.Exists(path2))
			{
				Directory.CreateDirectory(path2);
			}
		}
		catch (Exception ex4)
		{
			writer.WriteLine("Unable to create images directory: {0}", ex4.Message);
		}
	}

	private void ComputeReplaceableCommonFiles(List<CommonFile> commonFiles)
	{
		foreach (CommonFile commonFile in commonFiles)
		{
			string text = Path.Combine("C:\\Program Files\\Redbox\\CommonFiles", commonFile.File);
			string path = text + ".hash";
			if (!File.Exists(text) || !File.Exists(path))
			{
				commonFile.Replace = true;
				continue;
			}
			string value = string.Empty;
			using (StreamReader streamReader = new StreamReader(path))
			{
				value = streamReader.ReadLine();
			}
			string text2 = Path.Combine(AssemblyPath, commonFile.File);
			string path2 = text2 + ".hash";
			string text3 = string.Empty;
			using (StreamReader streamReader2 = new StreamReader(path2))
			{
				text3 = streamReader2.ReadLine();
			}
			if (!text3.Equals(value))
			{
				commonFile.Replace = true;
			}
		}
	}

	private bool CopyDefaultDatabase(TextWriter writer, string fileName, bool force)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"data\\{fileName}");
		bool flag = File.Exists(text);
		if (flag && !force)
		{
			writer.WriteLine("The database {0} already exists - take no further action.", fileName);
			return false;
		}
		if (flag)
		{
			string dest = MakeBackupFileName(text);
			FileHelper.SafeMove(writer, text, dest);
		}
		string source = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"{fileName}.default");
		return FileHelper.SafeCopy(writer, source, text);
	}

	private bool CacheUnusedDatabase(TextWriter writer, string fileName)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"data\\{fileName}");
		if (!File.Exists(text))
		{
			writer.WriteLine("The database {0} doesn't exist - take no further action.", fileName);
			return false;
		}
		string dest = MakeBackupFileName(text);
		FileHelper.SafeMove(writer, text, dest);
		return true;
	}

	private bool InstallInventoryDatabase(TextWriter writer, string fileName, bool makeEmptyAfterInstall)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"data\\{fileName}");
		if (File.Exists(text))
		{
			writer.WriteLine("The database {0} already exists - migrate tables.", fileName);
			return InventoryManager.UpdateInventoryDatabaseOnUpgrade(writer, validate: true, makeEmptyAfterInstall);
		}
		string source = Path.Combine(RuntimeService.Instance.AssemblyDirectory, $"{fileName}.default");
		if (!FileHelper.SafeCopy(writer, source, text))
		{
			return false;
		}
		return InventoryManager.UpdateInventoryDatabaseOnUpgrade(writer, validate: true, makeEmptyAfterInstall);
	}

	private bool InstallService(TextWriter writer)
	{
		bool flag = true;
		RegistryKey registryKey = null;
		try
		{
			string name = "SYSTEM\\CurrentControlSet\\Services\\halsvc$default";
			registryKey = Registry.LocalMachine.OpenSubKey(name);
			if (registryKey != null)
			{
				string text = (string)registryKey.GetValue("ImagePath");
				if (text.Contains(RuntimeService.Instance.AssemblyDirectory))
				{
					flag = false;
					writer.WriteLine("Service already installed - don't need to register.");
				}
			}
		}
		finally
		{
			registryKey?.Close();
		}
		if (flag)
		{
			Process process = new Process();
			process.StartInfo.FileName = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "register-service.bat");
			process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
			process.Start();
			process.WaitForExit();
			int exitCode = process.ExitCode;
			process.Close();
			writer.WriteLine("Register service returned with code {0}", exitCode);
			return exitCode == 0;
		}
		writer.WriteLine("Service already installed.");
		return true;
	}

	private bool UpgradeCountersDatabase(TextWriter writer)
	{
		return CountersDBManager.Instance.CreateTables(writer);
	}

	private bool InstallConfiguration(TextWriter writer)
	{
		string text = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml");
		if (File.Exists(text))
		{
			writer.WriteLine("The configuration file is already present - no further action.");
			return true;
		}
		try
		{
			File.Move(Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml.default"), text);
		}
		catch
		{
			writer.WriteLine("Unable to move default HAL.xml file over.");
			return false;
		}
		ErrorList errorList = new ErrorList();
		ControllerConfiguration.Instance.Load(text, errorList);
		ControllerConfiguration.Instance.Import(writer, errorList);
		ControllerConfiguration.Instance.Save(text, errorList);
		if (errorList.ContainsError())
		{
			writer.WriteLine("There were problems importing the data from gamp - errors:");
			foreach (Error item in errorList)
			{
				writer.WriteLine(item.ToString());
			}
			return false;
		}
		writer.WriteLine("Successfully imported data from the Gamp folder.");
		return true;
	}

	private string MakeBackupFileName(string fullPath)
	{
		DateTime now = DateTime.Now;
		return $"{fullPath}.bak.m{now.Month}d{now.Day}y{now.Year}_h{now.Hour}m{now.Minute}s{now.Second}";
	}

	private void UpdateCameraDriver(TextWriter log, string configuration)
	{
		string path = FileHelper.CreateBackup(configuration, BackupAction.Copy);
		bool flag = true;
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.Load(configuration);
			if (xmlDocument.DocumentElement != null)
			{
				XmlNode node = xmlDocument.DocumentElement.SelectSingleNode("Camera/CameraDriver");
				string nodeValue = XmlNodeExtensions.GetNodeValue2(node, "Vidcap");
				if (nodeValue == "Vidcap")
				{
					XmlNodeExtensions.SelectSingleNodeAndSetValue(xmlDocument.DocumentElement, "Camera/CameraDriver", "DirectShow");
					xmlDocument.Save(configuration);
				}
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		finally
		{
			if (flag)
			{
				FileHelper.SafeDeleteFile(path);
			}
		}
	}

	private void CleanupQuickReturn(TextWriter log, string configuration)
	{
		XmlDocument xmlDocument = new XmlDocument();
		string path = FileHelper.CreateBackup(configuration, BackupAction.Copy);
		bool flag = true;
		try
		{
			xmlDocument.Load(configuration);
			if (xmlDocument.DocumentElement == null)
			{
				return;
			}
			XmlNodeList xmlNodeList = xmlDocument.DocumentElement.SelectNodes("InstalledDevices/Device");
			if (xmlNodeList.Count == 0)
			{
				return;
			}
			string text = null;
			foreach (XmlNode item in xmlNodeList)
			{
				XmlNode namedItem = item.Attributes.GetNamedItem("Name");
				if (namedItem.Value.Equals("QRTerminal", StringComparison.CurrentCultureIgnoreCase))
				{
					XmlNodeList xmlNodeList2 = item.SelectNodes("Property");
					foreach (XmlNode item2 in xmlNodeList2)
					{
						XmlNode namedItem2 = item2.Attributes.GetNamedItem("name");
						if (namedItem2.Value.Equals("Port"))
						{
							text = item2.Attributes.GetNamedItem("value").Value;
							break;
						}
					}
				}
				if (!string.IsNullOrEmpty(text))
				{
					break;
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Console.WriteLine("InstalledPort: {0}", text);
				XmlNodeExtensions.SelectSingleNodeAndSetValue(xmlDocument.DocumentElement, "Controller/QuickReturnComPort", text);
				xmlDocument.Save(configuration);
			}
		}
		catch (Exception)
		{
			flag = false;
		}
		finally
		{
			if (flag)
			{
				FileHelper.SafeDeleteFile(path);
			}
		}
	}

	private static void Main(string[] args)
	{
		PackageInstaller packageInstaller = new PackageInstaller();
		if (!Directory.Exists(InstallLogsDirectory))
		{
			try
			{
				Directory.CreateDirectory(InstallLogsDirectory);
			}
			catch
			{
			}
		}
		string path = Path.Combine(InstallLogsDirectory, "Package-installer.log");
		using TextWriter textWriter = new StreamWriter(path, append: true);
		packageInstaller.CreateDirectories(textWriter);
		Platform platform = RuntimeService.Instance.GetPlatform();
		bool flag = Platform.Windows7 == platform;
		ServiceHelper.Instance.StopHALService(check: false);
		string text = "1.0.0.x";
		try
		{
			text = typeof(PackageInstaller).Assembly.GetName().Version.ToString();
		}
		catch
		{
			text = "1.0.0.x";
		}
		textWriter.WriteLine("HAL package installer version {0} running on {1} at {2} (platform = {3})", text, DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), platform.ToString());
		List<CommonFile> list = new List<CommonFile>();
		list.Add(new CommonFile
		{
			File = "CiBcfko86572612.dll",
			CLSID = "{3D82041f-7d5c-43c8-9d59-069491c3b51c}"
		});
		list.Add(new CommonFile
		{
			File = "CiBcfDm80000002.dll",
			CLSID = "{87062e11-3926-437f-88ae-a9f4b566d93e}"
		});
		if (!flag)
		{
			packageInstaller.ComputeReplaceableCommonFiles(list);
			foreach (CommonFile item in list)
			{
				if (!item.Replace)
				{
					textWriter.WriteLine("Not replacing file {0}", item.File);
					continue;
				}
				string text2 = Path.Combine("C:\\Program Files\\Redbox\\CommonFiles", item.File);
				if (!FileHelper.SafeDeleteFile(text2))
				{
					textWriter.WriteLine($"Unable to delete file {text2}");
				}
			}
			packageInstaller.InstallCommonFiles(textWriter, packageInstaller.AssemblyPath, list);
		}
		else
		{
			foreach (CommonFile item2 in list)
			{
				string text3 = Path.Combine("C:\\Program Files\\Redbox\\CommonFiles", item2.File);
				if (File.Exists(text3))
				{
					FileHelper.SafeDeleteFile(text3);
				}
				string path2 = text3 + ".hash";
				if (File.Exists(path2))
				{
					FileHelper.SafeDeleteFile(path2);
				}
			}
		}
		packageInstaller.CacheUnusedDatabase(textWriter, "JobData.vdb3");
		packageInstaller.CopyDefaultDatabase(textWriter, "HALCounters.vdb3", force: false);
		packageInstaller.UpgradeCountersDatabase(textWriter);
		if (!flag)
		{
			packageInstaller.RegisterServer(textWriter, "C:\\Program Files\\VideoCap ActiveX Control\\videocap.ocx", uninstall: false);
		}
		string text4 = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "HAL.xml");
		if (File.Exists(text4))
		{
			packageInstaller.CleanupQuickReturn(textWriter, text4);
			if (flag)
			{
				packageInstaller.UpdateCameraDriver(textWriter, text4);
			}
			packageInstaller.InstallInventoryDatabase(textWriter, "HALData.vdb3", makeEmptyAfterInstall: true);
			string installedVersion = packageInstaller.InstalledVersion;
			textWriter.Write("Installed service version is {0};", installedVersion);
			if (!string.IsNullOrEmpty(installedVersion))
			{
				Version version = new Version(installedVersion);
				if (version.Minor == 0 && version.Build == 7)
				{
					textWriter.WriteLine(" will upgrade VMZ.");
					VMZPhase1Converter vMZPhase1Converter = new VMZPhase1Converter();
					vMZPhase1Converter.UpgradeToNewPhase(textWriter);
				}
				else
				{
					textWriter.WriteLine(" will not upgrade VMZ.");
				}
			}
			if (File.Exists(VMZPhase1Converter.LegacyDatabase))
			{
				FileHelper.SafeDeleteFile(VMZPhase1Converter.LegacyDatabase);
			}
		}
		else
		{
			bool flag2 = false;
			string path3 = "c:\\Program Files\\Redbox\\XPTo7";
			int machineSlotConfiguration = packageInstaller.MachineSlotConfiguration;
			string text5 = Path.Combine(path3, "hal.xml");
			if (flag && File.Exists(text5))
			{
				if (FileHelper.SafeCopy(textWriter, text5, text4))
				{
					packageInstaller.InstallInventoryDatabase(textWriter, "HALData.vdb3", makeEmptyAfterInstall: false);
					textWriter.WriteLine("Successfully copied configuration from XP to 7 backup.");
					FileHelper.CreateBackup(text5, BackupAction.Move);
					bool flag3 = false;
					string text6 = Path.Combine(path3, "inventory.xml");
					if (File.Exists(text6))
					{
						ErrorList errorList = new ErrorList();
						ControllerConfiguration.Instance.Load(text4, errorList);
						if (errorList.ContainsError())
						{
							textWriter.WriteLine("There were problems loading the configuration:");
							foreach (Error item3 in errorList)
							{
								textWriter.WriteLine(item3.ToString());
							}
						}
						else
						{
							textWriter.WriteLine("Successfully loaded the config; decks = {0}", DecksManager.Instance.DeckCount);
							flag2 = true;
							try
							{
								InventoryManager.Instance.CreateUnknownDatabase(machineSlotConfiguration, setResetFlag: false);
							}
							catch (Exception ex)
							{
								textWriter.WriteLine("Caught an exception: {0}", ex.Message);
							}
							if (ControllerConfiguration.Instance.QuickReturnComPort != "NONE")
							{
								string source = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr.defs.default");
								string dest = Path.Combine(RuntimeService.Instance.AssemblyDirectory, "qr.state");
								FileHelper.SafeCopy(textWriter, source, dest);
							}
							string xml = File.ReadAllText(text6);
							ControllerConfiguration.Instance.ResetInventoryState(xml, errorList);
							if (errorList.ContainsError())
							{
								textWriter.WriteLine("There were problems saving the inventory:");
								foreach (Error item4 in errorList)
								{
									textWriter.WriteLine(item4.ToString());
								}
							}
							else
							{
								textWriter.WriteLine("Successfully imported conversion inventory.");
							}
							if (!DumpBin.Instance.Import(text6))
							{
								textWriter.WriteLine("Failed to import dumpbin inventory.");
							}
						}
						FileHelper.SafeDeleteFile(text5);
						errorList.Clear();
					}
					FileHelper.CreateBackup(text6, BackupAction.Move);
				}
				else
				{
					textWriter.WriteLine("** Failed to copy backup configuration from {0}", text5);
				}
			}
			if (!flag2)
			{
				int slotsFromGamp = ControllerConfiguration.GetSlotsFromGamp();
				if (slotsFromGamp == machineSlotConfiguration)
				{
					packageInstaller.InstallConfiguration(textWriter);
					packageInstaller.InstallInventoryDatabase(textWriter, "HALData.vdb3", makeEmptyAfterInstall: true);
				}
				else
				{
					if (packageInstaller.InstallInventoryDatabase(textWriter, "HALData.vdb3", makeEmptyAfterInstall: false))
					{
						InventoryManager.Instance.CreateUnknownDatabase(machineSlotConfiguration, setResetFlag: true);
					}
					textWriter.WriteLine("The machine is configured for {0} slots; the gamp data says there are {1} slots - mark db UNKNOWN with {2} slots", machineSlotConfiguration, slotsFromGamp, machineSlotConfiguration);
				}
			}
		}
		packageInstaller.InstallService(textWriter);
		packageInstaller.UpdateProductVersionKey(textWriter);
	}
}

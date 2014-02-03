using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

internal class ImageHelper
{
	private const string DuplicatesPath = "c:\\Program Files\\Redbox\\halservice\\video\\duplicates";

	private string m_serviceLogs;

	internal static ImageHelper Instance => Singleton<ImageHelper>.Instance;

	private string ServiceLogs
	{
		get
		{
			if (!string.IsNullOrEmpty(m_serviceLogs))
			{
				return m_serviceLogs;
			}
			m_serviceLogs = Path.Combine(ControllerConfiguration.Instance.KioskLogsPath, "Service");
			return m_serviceLogs;
		}
	}

	private string DuplicatesFolder
	{
		get
		{
			if (!Directory.Exists("c:\\Program Files\\Redbox\\halservice\\video\\duplicates"))
			{
				try
				{
					Directory.CreateDirectory("c:\\Program Files\\Redbox\\halservice\\video\\duplicates");
				}
				catch
				{
				}
			}
			return "c:\\Program Files\\Redbox\\halservice\\video\\duplicates";
		}
	}

	private byte[] PrevImageBits { get; set; }

	private byte[] CurrImageBits { get; set; }

	private string CurrentImage { get; set; }

	private string PrevImage { get; set; }

	internal bool IsActiveDuplicate()
	{
		if (PrevImageBits == null)
		{
			return false;
		}
		bool flag = AreEqual(PrevImageBits, CurrImageBits);
		if (flag)
		{
			LogError(PrevImage, CurrentImage);
			SaveBitsAsDupImage(PrevImage, PrevImageBits);
			SaveBitsAsDupImage(CurrentImage, CurrImageBits);
		}
		return flag;
	}

	internal void SaveBitsAsDupImage(string fileName, byte[] image)
	{
		string text = Path.Combine(DuplicatesFolder, fileName);
		if (File.Exists(text))
		{
			return;
		}
		using MemoryStream stream = new MemoryStream(image);
		Image image2 = Image.FromStream(stream);
		image2.Save(text, ImageFormat.Jpeg);
	}

	internal void UpdateActiveImage(string path)
	{
		UpdatePrev();
		CurrentImage = Path.GetFileName(path);
		CurrImageBits = ToByteArray(path);
		LogHelper.Instance.Log($"Current compare image is {path} ( filename {CurrentImage} ) prev = {PrevImage}", LogEntryType.Info);
	}

	internal bool ImageIsBlack(string path)
	{
		byte[] array = null;
		using (Bitmap bitmap = new Bitmap(path))
		{
			Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
			BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadOnly, bitmap.PixelFormat);
			IntPtr scan = bitmapData.Scan0;
			int num = bitmapData.Stride * bitmap.Height;
			array = new byte[num];
			Marshal.Copy(scan, array, 0, num);
			bitmap.UnlockBits(bitmapData);
		}
		byte[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i] != 0)
			{
				return false;
			}
		}
		LogHelper.Instance.Log($"ImageIsBlack (file {path}): found no non-zero bits in image data.", LogEntryType.Info);
		return true;
	}

	private void UpdatePrev()
	{
		PrevImageBits = CurrImageBits;
		PrevImage = CurrentImage;
		CurrentImage = null;
		CurrImageBits = null;
	}

	private bool AreEqual(byte[] a, byte[] b)
	{
		if (a == b)
		{
			return true;
		}
		if (a == null || b == null)
		{
			return false;
		}
		if (a.Length != b.Length)
		{
			return false;
		}
		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}
		return true;
	}

	private byte[] ToByteArray(string path)
	{
		byte[] array = null;
		using Image image = Image.FromFile(path);
		using MemoryStream memoryStream = new MemoryStream();
		image.Save(memoryStream, ImageFormat.Jpeg);
		return memoryStream.ToArray();
	}

	private void LogError(string orig, string duplicate)
	{
		string path = Path.Combine(ServiceLogs, "duplicates.log");
		using StreamWriter streamWriter = new StreamWriter(File.Open(path, FileMode.Append, FileAccess.Write, FileShare.Read));
		string text = $"{DateTime.Now.ToShortTimeString()} {DateTime.Now.ToShortDateString()} The image {duplicate} is detected as a duplicate ( original image = {orig} ).";
		ExecutionEngine.Instance.ActiveContext.AppLog.Write(text);
		streamWriter.WriteLine(text);
	}

	private ImageHelper()
	{
	}
}

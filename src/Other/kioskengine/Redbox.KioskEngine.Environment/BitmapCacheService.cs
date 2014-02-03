using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class BitmapCacheService : IBitmapCacheService, IDisposable
{
	internal sealed class ImageUsage
	{
		public string Name { get; set; }

		public int Uses { get; set; }
	}

	internal sealed class ImageTracker : IDisposable
	{
		public Stream UnderlyingStream { get; set; }

		public Image Instance { get; set; }

		public void Dispose()
		{
			try
			{
				if (Instance != null)
				{
					Instance.Dispose();
				}
				if (UnderlyingStream != null)
				{
					UnderlyingStream.Dispose();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in ImageTracker.Dispose.", e);
			}
		}
	}

	private IDictionary<string, ImageTracker> m_images;

	private readonly SortedDictionary<string, int> m_usageCounts = new SortedDictionary<string, int>();

	public static BitmapCacheService Instance => Singleton<BitmapCacheService>.Instance;

	internal IDictionary<string, ImageTracker> Images
	{
		get
		{
			if (m_images == null)
			{
				m_images = new Dictionary<string, ImageTracker>();
			}
			return m_images;
		}
	}

	public ErrorList Initialize()
	{
		LogHelper.Instance.Log("Initialize bitmap cache service.");
		ServiceLocator.Instance.AddService(typeof(IBitmapCacheService), Instance);
		return new ErrorList();
	}

	public Image GetImage(string name)
	{
		if (!Images.ContainsKey(name))
		{
			return null;
		}
		ImageTracker imageTracker = Images[name];
		int num = (m_usageCounts.ContainsKey(name) ? m_usageCounts[name] : 0);
		num++;
		if (num > 65536)
		{
			num = 1;
		}
		m_usageCounts[name] = num;
		return imageTracker.Instance;
	}

	public Image RegisterImage(string name, byte[] data)
	{
		Image image = GetImage(name);
		if (image != null)
		{
			return image;
		}
		if (data == null || data.Length == 0)
		{
			return null;
		}
		try
		{
			MemoryStream memoryStream = new MemoryStream(data);
			image = Image.FromStream(memoryStream);
			Images[name] = new ImageTracker
			{
				Instance = image,
				UnderlyingStream = memoryStream
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from stream.", e);
		}
		return image;
	}

	public Image RegisterImage(string name, string path)
	{
		Image image = GetImage(name);
		if (image == null)
		{
			try
			{
				image = Image.FromFile(path);
				Images[name] = new ImageTracker
				{
					Instance = image
				};
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImage from file.", e);
			}
		}
		return image;
	}

	public Image RegisterThumbnailImage(string name, byte[] data, int width, int height)
	{
		Image image = GetImage(name);
		if (image != null)
		{
			return image;
		}
		if (data == null || data.Length == 0)
		{
			return null;
		}
		try
		{
			using MemoryStream stream = new MemoryStream(data);
			using Image image2 = Image.FromStream(stream);
			image = image2.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
			Images[name] = new ImageTracker
			{
				Instance = image
			};
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from stream.", e);
		}
		return image;
	}

	public Image RegisterThumbnailImage(string name, string newName, int width, int height)
	{
		Image image = GetImage(newName);
		if (image != null)
		{
			return image;
		}
		Image image2 = GetImage(name);
		if (image2 == null)
		{
			return null;
		}
		try
		{
			image = image2.GetThumbnailImage(width, height, () => false, IntPtr.Zero);
			Images[newName] = new ImageTracker
			{
				Instance = image
			};
			return image;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterThumbnailImage from existing image.", e);
		}
		return null;
	}

	public void DropCache()
	{
		LogHelper.Instance.Log("Drop bitmap cache.");
		List<string> list = new List<string>();
		foreach (string key in Images.Keys)
		{
			LogHelper.Instance.Log("...Dispose image '{0}'.", key);
			try
			{
				Images[key].Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.DropCache.", e);
			}
			finally
			{
				list.Add(key);
			}
		}
		LogHelper.Instance.Log("...Remove keys from dictionary.");
		foreach (string item in list)
		{
			Images.Remove(item);
		}
	}

	public int TrimCache(int maximumItems)
	{
		int num = 0;
		if (m_images.Count <= maximumItems)
		{
			return num;
		}
		LogHelper.Instance.Log("Trim the bitmap cache:");
		List<ImageUsage> list = new List<ImageUsage>();
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service != null)
		{
			foreach (KeyValuePair<string, ImageTracker> image in m_images)
			{
				if (!service.IsBitmapReferenced(image.Value.Instance))
				{
					list.Add(new ImageUsage
					{
						Name = image.Key,
						Uses = (m_usageCounts.ContainsKey(image.Key) ? m_usageCounts[image.Key] : 0)
					});
				}
			}
		}
		list.Sort((ImageUsage x, ImageUsage y) => x.Uses.CompareTo(y.Uses));
		for (int num2 = 0; num2 < m_images.Count - maximumItems; num2++)
		{
			string name = list[num2].Name;
			LogHelper.Instance.Log("...Trim image '{0}' from the cache.", name);
			ImageTracker imageTracker = m_images[name];
			try
			{
				imageTracker.Dispose();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.TrimCache.", e);
			}
			m_images.Remove(name);
			m_usageCounts.Remove(name);
			num++;
		}
		LogHelper.Instance.Log("...{0} images trimmed.", num);
		return num;
	}

	public Image RegisterImageWithText(string name, string textToDisplay, Font textFont, Image baseImage, Rectangle textLocation)
	{
		Bitmap bitmap = new Bitmap(baseImage);
		Graphics graphics = Graphics.FromImage(bitmap);
		graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
		graphics.DrawString(textToDisplay, textFont, Brushes.White, textLocation.X, textLocation.Y);
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			RegisterImage(name, memoryStream.ToArray());
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImageWithText from stream.", e);
		}
		return bitmap;
	}

	public Image RegisterImageWithImage(string name, byte[] imageToDraw, Image baseImage, Rectangle imageLocation)
	{
		Image image = null;
		if (imageToDraw == null || imageToDraw.Length == 0)
		{
			return null;
		}
		try
		{
			MemoryStream stream = new MemoryStream(imageToDraw);
			image = Image.FromStream(stream);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImageWithImage from stream.", e);
		}
		Bitmap bitmap = new Bitmap(baseImage);
		try
		{
			Graphics graphics = Graphics.FromImage(bitmap);
			if (image != null)
			{
				graphics.DrawImage(image, imageLocation);
			}
			MemoryStream memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			RegisterImage(name, memoryStream.ToArray());
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in BitmapCacheService.RegisterImageWithText from stream.", e2);
		}
		return bitmap;
	}

	public void Dispose()
	{
		DropCache();
	}

	private BitmapCacheService()
	{
	}
}

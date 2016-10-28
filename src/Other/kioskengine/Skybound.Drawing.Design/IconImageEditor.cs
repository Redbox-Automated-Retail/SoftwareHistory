using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using x2de5aa54b5fc5109;

namespace Skybound.Drawing.Design;

internal class IconImageEditor : UITypeEditor, IMessageFilter
{
	private IUIService xf7f1184fefe7307a;

	private bool x713d49f7be575a20;

	private Form x5e51d524861f7f42;

	private ArrayList x49c2d08373d19852 = new ArrayList();

	public override bool GetPaintValueSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	public override void PaintValue(PaintValueEventArgs e)
	{
		if (e.Value is Image)
		{
			Rectangle bounds = e.Bounds;
			bounds.Width--;
			bounds.Height--;
			e.Graphics.DrawRectangle(SystemPens.WindowFrame, bounds);
			e.Graphics.DrawImage((Image)e.Value, e.Bounds);
		}
	}

	public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
	{
		return UITypeEditorEditStyle.Modal;
	}

	public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
	{
		xf7f1184fefe7307a = (IUIService)provider.GetService(typeof(IUIService));
		try
		{
			UITypeEditor uITypeEditor = (UITypeEditor)TypeDescriptor.GetEditor(typeof(Image), typeof(UITypeEditor));
			if (uITypeEditor.GetType().FullName.StartsWith("Microsoft.VisualStudio"))
			{
				return xc8e1adefa1ee1bf8(uITypeEditor, context, provider, value);
			}
			return x9caba5ec27cad5fd(value);
		}
		catch (Exception ex)
		{
			xf7f1184fefe7307a.ShowError(ex);
			return value;
		}
	}

	~IconImageEditor()
	{
		if (x49c2d08373d19852.Count <= 0)
		{
			return;
		}
		foreach (string item in x49c2d08373d19852)
		{
			if (File.Exists(item))
			{
				File.Delete(item);
			}
			if (Directory.Exists(Path.GetDirectoryName(item)))
			{
				Directory.Delete(Path.GetDirectoryName(item));
			}
		}
		x49c2d08373d19852 = null;
	}

	private object x9caba5ec27cad5fd(object xbcea506a33cf9111)
	{
		OpenFileDialog openFileDialog = new OpenFileDialog();
		openFileDialog.CheckFileExists = true;
		openFileDialog.Filter = "Images (*.bmp;*.emf;*.gif;*.ico;*.jpg;*.png;*.wmf)|*.bmp;*.emf;*.gif;*.ico;*.jpg;*.png;*.wmf|All Files (*.*)|*.*";
		openFileDialog.Multiselect = false;
		openFileDialog.ShowHelp = false;
		openFileDialog.ShowReadOnly = false;
		openFileDialog.Title = "Open Image File";
		if (openFileDialog.ShowDialog(xf7f1184fefe7307a.GetDialogOwnerWindow()) == DialogResult.OK)
		{
			Image image = x1a9de7a4273ec295(openFileDialog.FileName);
			if (image != null)
			{
				return image;
			}
		}
		return xbcea506a33cf9111;
	}

	private Image x1a9de7a4273ec295(string xafe2f3653ee64ebc)
	{
		using (FileStream fileStream = new FileStream(xafe2f3653ee64ebc, FileMode.Open, FileAccess.Read, FileShare.Read))
		{
			if (!(Path.GetExtension(xafe2f3653ee64ebc) == ".ico"))
			{
				return x0273dd79e8d1b120(fileStream);
			}
			x7d8d2ed69d304345 x7d8d2ed69d304346 = new x7d8d2ed69d304345(fileStream);
			if (x7d8d2ed69d304346.xdbe5f635faa12ead().Length == 1)
			{
				return x7d8d2ed69d304346.xd2b1a4678f6c35f4(x7d8d2ed69d304346.xdbe5f635faa12ead()[0]);
			}
			xc605e63aa183e6b5 xc605e63aa183e6b6 = new xc605e63aa183e6b5();
			xc605e63aa183e6b6.x7d8d2ed69d304345 = x7d8d2ed69d304346;
			xc605e63aa183e6b6.ShowDialog(xf7f1184fefe7307a.GetDialogOwnerWindow());
			if (xc605e63aa183e6b6.DialogResult == DialogResult.OK)
			{
				return x7d8d2ed69d304346.xd2b1a4678f6c35f4(xc605e63aa183e6b6.x88ff53b73da867e0);
			}
		}
		return null;
	}

	private Image x0273dd79e8d1b120(Stream xcf18e5243f8d5fd3)
	{
		byte[] buffer = new byte[(int)xcf18e5243f8d5fd3.Length];
		xcf18e5243f8d5fd3.Read(buffer, 0, (int)xcf18e5243f8d5fd3.Length);
		return Image.FromStream(new MemoryStream(buffer));
	}

	private object xc8e1adefa1ee1bf8(UITypeEditor x9293e3b369df6767, ITypeDescriptorContext x0f7b23d1c393aed9, IServiceProvider x83d0c26038874b92, object xbcea506a33cf9111)
	{
		Application.AddMessageFilter(this);
		x713d49f7be575a20 = false;
		try
		{
			return x9293e3b369df6767.EditValue(x0f7b23d1c393aed9, x83d0c26038874b92, xbcea506a33cf9111);
		}
		finally
		{
			Application.RemoveMessageFilter(this);
			xbe947cd1a95754dd();
		}
	}

	public bool PreFilterMessage(ref Message m)
	{
		try
		{
			if (!x713d49f7be575a20 && Control.FromHandle(m.HWnd) is Form form && form.GetType().FullName.StartsWith("Microsoft.VisualStudio.Windows.Forms.ResourcePickerDialog"))
			{
				x53938b6cbc5b2269(form);
			}
		}
		catch (Exception ex)
		{
			xf7f1184fefe7307a.ShowError(ex);
		}
		return false;
	}

	private OpenFileDialog x7874f73222d60c69(Form xa6607dfd4b3038ad)
	{
		PropertyInfo property = xa6607dfd4b3038ad.GetType().GetProperty("OpenFileDialog", BindingFlags.Instance | BindingFlags.NonPublic);
		if ((object)property != null)
		{
			return property.GetValue(xa6607dfd4b3038ad, null) as OpenFileDialog;
		}
		return null;
	}

	private void x53938b6cbc5b2269(Form xa6607dfd4b3038ad)
	{
		x5e51d524861f7f42 = xa6607dfd4b3038ad;
		x713d49f7be575a20 = true;
		OpenFileDialog openFileDialog = x7874f73222d60c69(x5e51d524861f7f42);
		if (openFileDialog != null)
		{
			openFileDialog.Filter = openFileDialog.Filter.Replace("*.gif", "*.ico;*.gif");
			openFileDialog.FileOk += x77c91b9ac35160e8;
		}
	}

	private void xbe947cd1a95754dd()
	{
		if (x5e51d524861f7f42 != null)
		{
			OpenFileDialog openFileDialog = x7874f73222d60c69(x5e51d524861f7f42);
			if (openFileDialog != null)
			{
				openFileDialog.Filter = openFileDialog.Filter.Replace("*.ico;*.gif", "*.gif");
				openFileDialog.FileOk -= x77c91b9ac35160e8;
			}
			x5e51d524861f7f42 = null;
		}
	}

	private void x77c91b9ac35160e8(object xe0292b9ed559da7d, CancelEventArgs xfbf34718e704c6bc)
	{
		try
		{
			OpenFileDialog openFileDialog = xe0292b9ed559da7d as OpenFileDialog;
			string[] fileNames = openFileDialog.FileNames;
			for (int i = 0; i < fileNames.Length; i++)
			{
				string text = fileNames[i];
				if (Path.GetExtension(text) == ".ico")
				{
					Image image = x1a9de7a4273ec295(text);
					if (image == null)
					{
						xfbf34718e704c6bc.Cancel = true;
						return;
					}
					string text2 = xbc550a8ecc472647(Path.GetFileName(text));
					try
					{
						image.Save(text2, ImageFormat.Png);
						x49c2d08373d19852.Add(text2);
						fileNames[i] = text2;
					}
					catch (Exception ex)
					{
						xf7f1184fefe7307a.ShowError(ex);
						xfbf34718e704c6bc.Cancel = true;
						return;
					}
				}
			}
			if (x49c2d08373d19852.Count > 0)
			{
				typeof(FileDialog).GetField("fileNames", BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(openFileDialog, fileNames);
			}
		}
		catch (Exception ex2)
		{
			xf7f1184fefe7307a.ShowError(ex2.ToString());
		}
	}

	private string xbc550a8ecc472647(string xabaded2400528e09)
	{
		string tempFileName = Path.GetTempFileName();
		File.Delete(tempFileName);
		DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), tempFileName));
		return Path.Combine(directoryInfo.FullName, Path.ChangeExtension(xabaded2400528e09, ".png"));
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using AxShockwaveFlashObjects;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.IDE;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Bootstrap;

public class HostForm : Form, IFlashService, ITrackDataService
{
	private struct COPYDATASTRUCT
	{
		public IntPtr dwData;

		public int cbData;

		[MarshalAs(UnmanagedType.LPStr)]
		public string lpData;
	}

	private const char TrackEnd = '?';

	private const char TrackStart = '%';

	private const int WM_USER = 1024;

	private const int WM_COPYDATA = 74;

	private Stack<FlashMovie> m_flashMovies;

	private bool m_isCursorHidden;

	private IContainer components;

	private Stack<FlashMovie> FlashMovies
	{
		get
		{
			if (m_flashMovies == null)
			{
				m_flashMovies = new Stack<FlashMovie>();
			}
			return m_flashMovies;
		}
	}

	private SecureString TrackDataBuffer { get; set; }

	private TrackData TrackDataContainer { get; set; }

	private bool AllowTrackDataParsing { get; set; }

	private Redbox.KioskEngine.ComponentModel.ErrorList TrackDataErrors { get; set; }

	private bool ReceivingTrackData { get; set; }

	public event EventHandler Notify;

	public HostForm()
	{
		InitializeComponent();
		IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (!Debugger.IsAttached && !service.GetValue("Core", "EnableMouseCursor", defaultValue: true))
		{
			Cursor.Hide();
			m_isCursorHidden = true;
		}
		base.ShowInTaskbar = service.GetValue("Ide", "ShowKioskEngineInTaskBar", defaultValue: false);
		string value = service.GetValue<string>("Store", "Environment", null);
		bool value2 = service.GetValue("Ide", "ShowChrome", defaultValue: false);
		if (value.Equals("Development", StringComparison.OrdinalIgnoreCase) && value2)
		{
			base.FormBorderStyle = FormBorderStyle.FixedSingle;
			base.ControlBox = true;
			MaximumSize = Size.Empty;
		}
		TrackDataBuffer = new SecureString();
	}

	public void Reset()
	{
		LogHelper.Instance.Log("Reset movie service.");
		while (FlashMovies.Count > 0)
		{
			PopFlashMovie();
		}
		ServiceLocator.Instance.GetService<IFileCacheService>()?.Remove("*.movie");
	}

	public void ClearCache()
	{
		LogHelper.Instance.Log("Clear movie service cache.");
		ServiceLocator.Instance.GetService<IFileCacheService>()?.Remove("*.movie");
	}

	public string PeekFlashMovie()
	{
		if (FlashMovies.Count != 0)
		{
			return Path.GetFileNameWithoutExtension(FlashMovies.Peek().FileName);
		}
		return null;
	}

	public void PopFlashMovie()
	{
		if (FlashMovies.Count == 0)
		{
			return;
		}
		FlashMovie topFlashMovie = FlashMovies.Pop();
		MethodInvoker invoker = delegate
		{
			try
			{
				topFlashMovie.Player.StopPlay();
				base.Controls.Remove(topFlashMovie.Player);
				if (!topFlashMovie.Player.IsDisposed)
				{
					topFlashMovie.Player.Dispose();
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PopFlashMovie.", e);
			}
		};
		EngineApplication.Instance.ThreadSafeHostUpdate(invoker);
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		service.ActiveScene.MakeDirty(new Rectangle[1] { topFlashMovie.Zone });
	}

	public void SwitchTopMovie(string fileName)
	{
		if (FlashMovies.Count != 0)
		{
			FlashMovie topFlashMovie = FlashMovies.Peek();
			MethodInvoker invoker = delegate
			{
				topFlashMovie.Player.LoadMovie(0, fileName);
			};
			EngineApplication.Instance.ThreadSafeHostUpdate(invoker);
		}
	}

	public void PushFlashMovie(string fileName, Rectangle zone)
	{
		MethodInvoker invoker = delegate
		{
			try
			{
				AxShockwaveFlash axShockwaveFlash = new AxShockwaveFlash
				{
					Location = zone.Location,
					Size = zone.Size
				};
				base.Controls.Add(axShockwaveFlash);
				axShockwaveFlash.MouseCaptureChanged += flashPlayer_MouseCaptureChanged;
				axShockwaveFlash.LoadMovie(0, fileName);
				axShockwaveFlash.Play();
				FlashMovies.Push(new FlashMovie
				{
					Zone = zone,
					FileName = fileName,
					Player = axShockwaveFlash
				});
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in PushFlashMovie.", e);
			}
		};
		EngineApplication.Instance.ThreadSafeHostUpdate(invoker);
	}

	public void flashPlayer_MouseCaptureChanged(object sender, EventArgs e)
	{
		base.ActiveControl = null;
		Focus();
	}

	public int GetKeyId()
	{
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return 99999;
		}
		string resourceName = (string)manifest["public_key"];
		if (!ResourceBundleService.Instance.HasActiveBundle())
		{
			return 99999;
		}
		IResource resource = ResourceBundleService.Instance.GetResource(resourceName);
		if (resource == null)
		{
			return 99999;
		}
		return (int)resource["key_id"];
	}

	public void ClearNotify()
	{
		Notify = null;
	}

	public void ClearData()
	{
		TrackDataBuffer.Clear();
		TrackDataContainer = null;
		TrackDataErrors = null;
	}

	public string GetTrack1()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return null;
		}
		XmlNode xml = ResourceBundleService.Instance.GetXml((string)manifest["public_key"]);
		if (TrackDataContainer == null)
		{
			return null;
		}
		return CardHelper.EncryptCreditCardFromXmlKey(TrackDataContainer.Track1, xml.InnerXml);
	}

	public string GetTrack2()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return null;
		}
		XmlNode xml = ResourceBundleService.Instance.GetXml((string)manifest["public_key"]);
		if (TrackDataContainer == null)
		{
			return null;
		}
		return CardHelper.EncryptCreditCardFromXmlKey(TrackDataContainer.Track2, xml.InnerXml);
	}

	public string GetNumber()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return null;
		}
		XmlNode xml = ResourceBundleService.Instance.GetXml((string)manifest["public_key"]);
		if (TrackDataContainer == null)
		{
			return null;
		}
		return CardHelper.EncryptCreditCardFromXmlKey(TrackDataContainer.AccountNumber, xml.InnerXml);
	}

	public bool HasValidData()
	{
		if (TrackDataErrors != null && TrackDataContainer != null)
		{
			return !TrackDataErrors.ContainsError();
		}
		return false;
	}

	public string GetFirstName()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.FirstName;
	}

	public string GetLastName()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.LastName;
	}

	public string GetLastFour()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.LastFour;
	}

	public CardType? GetCardType()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.CardType;
	}

	public string GetExpiryMonth()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.ExpiryMonth;
	}

	public string GetExpiryYear()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return TrackDataContainer.ExpiryYear;
	}

	public void StopTrackParsing()
	{
		AllowTrackDataParsing = false;
	}

	public void StartTrackParsing()
	{
		AllowTrackDataParsing = true;
		ClearData();
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetTrackErrors()
	{
		return TrackDataErrors ?? new Redbox.KioskEngine.ComponentModel.ErrorList();
	}

	public string GenerateCardHashId()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return CardHelper.GenerateCardId(TrackDataContainer.AccountNumber);
	}

	protected override void WndProc(ref Message m)
	{
		EnvironmentNotificationType environmentNotificationType = EnvironmentNotificationType.None;
		IEnvironmentNotificationService service = ServiceLocator.Instance.GetService<IEnvironmentNotificationService>();
		if (service != null)
		{
			environmentNotificationType = service.ProcessNotification(ref m);
		}
		if (environmentNotificationType == EnvironmentNotificationType.CreditCardReaderInvalid && service != null)
		{
			service.RaiseInvalidReader();
		}
		else if (environmentNotificationType == EnvironmentNotificationType.CreditCardReaderValid && service != null)
		{
			service.RaiseValidReader();
		}
		else
		{
			switch (m.Msg)
			{
			case 74:
				CommandHelper.ProcessCommandMessage(((COPYDATASTRUCT)m.GetLParam(typeof(COPYDATASTRUCT))).lpData);
				break;
			}
		}
		base.WndProc(ref m);
	}

	private void OnPaint(object sender, PaintEventArgs e)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service.ActiveScene != null)
		{
			service.ActiveScene.MakeDirty(null);
		}
	}

	private string EncodePassword(string originalPassword)
	{
		MD5 mD = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.Default.GetBytes(originalPassword);
		byte[] array = mD.ComputeHash(bytes);
		return BitConverter.ToString(array);
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		switch (e.KeyCode)
		{
		case Keys.F1:
			ScreenshotHelper.Capture(this);
			break;
		case Keys.F2:
			if (service != null && service.Filter["environment"] != "Production")
			{
				ManageViewsForm manageViewsForm = new ManageViewsForm();
				manageViewsForm.ShowDialog();
			}
			break;
		case Keys.F4:
		{
			PreferencesManager preferencesManager = new PreferencesManager();
			if (service == null)
			{
				break;
			}
			XmlNode xml = service.GetXml("config");
			if (xml == null)
			{
				break;
			}
			XmlNodeList xmlNodeList = xml.SelectNodes("row");
			if (xmlNodeList == null)
			{
				break;
			}
			string text = null;
			foreach (XmlNode item in xmlNodeList)
			{
				XmlNodeList xmlNodeList2 = item.SelectNodes("column");
				if (xmlNodeList2 == null)
				{
					continue;
				}
				foreach (XmlNode item2 in xmlNodeList2)
				{
					string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(item2, "name");
					if (attributeValue == "F4PasswordHash")
					{
						text = XmlNodeExtensions.GetNodeValue(item2, string.Empty);
						break;
					}
				}
			}
			if (text == null)
			{
				break;
			}
			UnlockForm unlockForm = new UnlockForm();
			if (unlockForm.ShowDialog() == DialogResult.OK)
			{
				SHA256Managed sHA256Managed = new SHA256Managed();
				string text2 = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(unlockForm.Password))));
				sHA256Managed.Clear();
				if (text2 == text)
				{
					preferencesManager.ShowDialog();
				}
				else
				{
					MessageBox.Show("The password provided is invalid.", "Redbox Engine for Distributed Stores", MessageBoxButtons.OK);
				}
			}
			break;
		}
		case Keys.F5:
			if (service != null && service.Filter["environment"] != "Production")
			{
				ServiceLocator.Instance.GetService<IIdeService>()?.Show();
			}
			break;
		case Keys.F6:
			if (service != null && service.Filter["environment"] != "Production")
			{
				ManageTimersForm manageTimersForm = new ManageTimersForm();
				manageTimersForm.ShowDialog();
			}
			break;
		case Keys.F8:
			ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.ToggleTouchScreen();
			break;
		case Keys.F11:
			if (m_isCursorHidden)
			{
				Cursor.Show();
			}
			else
			{
				Cursor.Hide();
			}
			m_isCursorHidden = !m_isCursorHidden;
			break;
		case Keys.F3:
		case Keys.F7:
		case Keys.F9:
		case Keys.F10:
			break;
		}
	}

	private void OnKeyPress(object sender, KeyPressEventArgs e)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (e.KeyChar == '\u001b')
		{
			Close();
			e.Handled = true;
			return;
		}
		if (AllowTrackDataParsing && e.KeyChar == '%')
		{
			ReceivingTrackData = true;
			TrackDataBuffer = new SecureString();
		}
		if (AllowTrackDataParsing && ReceivingTrackData)
		{
			TrackDataBuffer.AppendChar(e.KeyChar);
			if (e.KeyChar == '?' && CardHelper.ExtractFromSecure(TrackDataBuffer).IndexOf(";") > 0)
			{
				ReceivingTrackData = false;
				TrackDataErrors = TrackData.Parse(TrackDataBuffer, out var trackData);
				if (!TrackDataErrors.ContainsError())
				{
					TrackDataContainer = trackData;
				}
				if (Notify != null)
				{
					Notify(this, EventArgs.Empty);
				}
				TrackDataBuffer.Clear();
			}
			e.Handled = true;
			return;
		}
		if (e.KeyChar == '`')
		{
			ResourceBundleService.Instance.GetManifest(out var manifestInfo);
			string text = ResourceBundleService.Instance.Filter["view_name"];
			if (manifestInfo.ProductName != "Rental Application" || (manifestInfo.ProductName == "Rental Application" && (text == "start_view" || text == "welcome_view")))
			{
				EngineApplication.Instance.SelectBundle();
			}
			e.Handled = true;
		}
		else if (AllowTrackDataParsing && service != null && service.Filter["environment"] != "Production")
		{
			switch (e.KeyChar)
			{
			case '~':
				SendKeys.Send("{%}B4111111111111111{^}JOE/COOL                {^}1608041103263?;4111111111111111=1608041103263?");
				break;
			case '!':
				SendKeys.Send("{%}B24931070601{^}JOE/COOL                {^}1608041103263?;249931070601=1608041103263?");
				break;
			case '@':
				SendKeys.Send("{%}B6010562000017685{^}REDBOX                {^}00010004000060013779?;6010562000017685=00010004000060013779?");
				break;
			}
		}
		EngineApplication.Instance.ActivateKeyPressHandlers(new KeyPressEventArgsEx(e, Control.ModifierKeys));
	}

	private void OnActivated(object sender, EventArgs e)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service.ActiveScene != null)
		{
			service.ActiveScene.MakeDirty(null);
			EngineApplication.Instance.ActivateActivatedHandlers();
			Focus();
		}
	}

	private void OnMouseClick(object sender, MouseEventArgs e)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service.ActiveScene != null)
		{
			service.ActiveScene.HitTest(e.X, e.Y);
			EngineApplication.Instance.ActivateMouseClickHandlers(e);
		}
	}

	private void OnMouseDoubleClick(object sender, MouseEventArgs e)
	{
		EngineApplication.Instance.ActivateMouseDoubleClickHandlers(e);
	}

	private void OnClosing(object sender, FormClosingEventArgs e)
	{
		IFlashService service = ServiceLocator.Instance.GetService<IFlashService>();
		service.Reset();
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Redbox.KioskEngine.Bootstrap.HostForm));
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 13f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.FromArgb(153, 153, 152);
		base.ClientSize = new System.Drawing.Size(1024, 768);
		base.ControlBox = false;
		this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		this.MaximumSize = new System.Drawing.Size(1024, 768);
		base.MinimizeBox = false;
		base.Name = "HostForm";
		base.ShowInTaskbar = false;
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "Redbox Kiosk Engine";
		base.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(OnMouseDoubleClick);
		base.Paint += new System.Windows.Forms.PaintEventHandler(OnPaint);
		base.MouseClick += new System.Windows.Forms.MouseEventHandler(OnMouseClick);
		base.Activated += new System.EventHandler(OnActivated);
		base.KeyPress += new System.Windows.Forms.KeyPressEventHandler(OnKeyPress);
		base.FormClosing += new System.Windows.Forms.FormClosingEventHandler(OnClosing);
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDown);
		base.ResumeLayout(false);
	}
}

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.KioskEngine.IDE;
using Redbox.REDS.Framework;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.KioskEngine.Bootstrap;

public class HostForm : Form, ITrackDataService
{
	private struct COPYDATASTRUCT
	{
		public IntPtr dwData;

		public int cbData;

		[MarshalAs(UnmanagedType.LPStr)]
		public string lpData;
	}

	private int _keyId = 99999;

	private bool _useStoreCertificate;

	private const char TrackEnd = '?';

	private const char TrackStart = '%';

	private const int WM_USER = 1024;

	private const int WM_COPYDATA = 74;

	private bool m_isCursorHidden;

	private DateTime _keyPressStartAfter = DateTime.MinValue;

	private IContainer components;

	public int KeyId => _keyId;

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
		service.GetValue<string>("Store", "Environment", null);
		service.GetValue("Ide", "ShowChrome", defaultValue: false);
		if (Debugger.IsAttached)
		{
			base.FormBorderStyle = FormBorderStyle.Sizable;
			base.ControlBox = true;
			MaximumSize = System.Drawing.Size.Empty;
		}
		TrackDataBuffer = new SecureString();
		EngineApplication.DisableProcessWindowsGhosting();
	}

	public int GetKeyId()
	{
		return _keyId;
	}

	private int GetBundleKeyId()
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

	public void RefreshCertificate()
	{
		int num = 0;
		IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (service != null && service.TryGetValue<int>("system", "General", "CertificateName", out var value))
		{
			num = value;
		}
		if (num == 0)
		{
			_keyId = GetBundleKeyId();
			_useStoreCertificate = false;
		}
		else
		{
			_keyId = num;
			_useStoreCertificate = true;
		}
		if (_useStoreCertificate && GetStoreCertificate() == null)
		{
			_keyId = GetBundleKeyId();
			_useStoreCertificate = false;
		}
	}

	private string Encrypt(SecureString value)
	{
		if (_useStoreCertificate)
		{
			X509Certificate2 storeCertificate = GetStoreCertificate();
			if (value == null)
			{
				return null;
			}
			return CardHelper.EncryptCreditCardFromCertificate(value, storeCertificate);
		}
		string bundleCertificateXML = GetBundleCertificateXML();
		if (bundleCertificateXML == null)
		{
			return null;
		}
		if (value == null)
		{
			return null;
		}
		return CardHelper.EncryptCreditCardFromXmlKey(value, bundleCertificateXML);
	}

	private X509Certificate2 GetStoreCertificate()
	{
		return Redbox.KioskEngine.Environment.CertificateHelper.GetCertificateBySubjectName(StoreName.My, StoreLocation.LocalMachine, _keyId.ToString());
	}

	private string GetBundleCertificateXML()
	{
		IResource manifest = ResourceBundleService.Instance.GetManifest(out var _);
		if (manifest == null || manifest["public_key"] == null)
		{
			return null;
		}
		return ResourceBundleService.Instance.GetXml((string)manifest["public_key"]).InnerXml;
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
		return Encrypt(TrackDataContainer.Track1);
	}

	public string GetTrack2()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return Encrypt(TrackDataContainer.Track2);
	}

	public string GetNumber()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return Encrypt(TrackDataContainer.AccountNumber);
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
		if (AllowTrackDataParsing)
		{
			AllowTrackDataParsing = false;
			ServiceLocator.Instance.GetService<ICCReaderHealth>()?.EventOccurred(TrackDataContainer != null || TrackDataErrors != null, !HasValidData());
		}
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

	public string GenerateReservationCardHashId()
	{
		if (TrackDataContainer == null)
		{
			return null;
		}
		return CardHelper.GenerateReservationCardId(TrackDataContainer.AccountNumber);
	}

	public string HashString(string original)
	{
		return CardHelper.GenerateHash(original);
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
		MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.Default.GetBytes(originalPassword);
		return BitConverter.ToString(mD5CryptoServiceProvider.ComputeHash(bytes));
	}

	public void OnKeyDown(object sender, KeyEventArgs e)
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		switch (e.KeyCode)
		{
		case Keys.Home:
		{
			ITextToSpeechService service4 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service4 != null)
			{
				service4.AudioDeviceConnected = true;
				service4.RunSpeechWorkflow($"tts_{ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame()?.ViewFrame?.ViewName}");
			}
			break;
		}
		case Keys.End:
		{
			ITextToSpeechService service3 = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (service3 != null)
			{
				service3.AudioDeviceConnected = false;
			}
			break;
		}
		case Keys.F1:
			ScreenshotHelper.Capture(this);
			break;
		case Keys.F2:
			if (service != null && service.Filter["environment"] != "Production")
			{
				new ManageViewsForm().ShowDialog();
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
					if (item2.GetAttributeValue<string>("name") == "F4PasswordHash")
					{
						text = item2.GetNodeValue(string.Empty);
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
				string obj = Convert.ToBase64String(sHA256Managed.ComputeHash(sHA256Managed.ComputeHash(Encoding.UTF8.GetBytes(unlockForm.Password))));
				sHA256Managed.Clear();
				if (obj == text)
				{
					preferencesManager.ShowDialog();
				}
				else
				{
					System.Windows.Forms.MessageBox.Show("The password provided is invalid.", "Redbox Engine for Distributed Stores", MessageBoxButtons.OK);
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
				new ManageTimersForm().ShowDialog();
			}
			break;
		case Keys.F8:
			ServiceLocator.Instance.GetService<IEnvironmentNotificationService>()?.ToggleTouchScreen();
			break;
		case Keys.F10:
		{
			IRenderingService service5 = ServiceLocator.Instance.GetService<IRenderingService>();
			string name = "ADALine";
			if (service5.ActiveScene.SceneRenderType == RenderType.WPF)
			{
				System.Windows.Controls.UserControl userControl = (service5.ActiveScene.Actors.First?.Value)?.WPFFrameworkElement as System.Windows.Controls.UserControl;
				Grid grid = userControl?.Content as Grid;
				IEnumerator enumerator = grid.Children.GetEnumerator();
				bool flag = false;
				while (enumerator.MoveNext())
				{
					if (enumerator.Current is Line line)
					{
						line.Visibility = ((line.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible);
						userControl.InvalidateVisual();
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					Line element = new Line
					{
						X1 = 0.0,
						Y1 = 286.0,
						X2 = 1024.0,
						Y2 = 286.0,
						Stroke = new SolidColorBrush
						{
							Color = Colors.GreenYellow
						},
						StrokeThickness = 3.0,
						Stretch = Stretch.None,
						Name = name,
						Visibility = Visibility.Visible
					};
					grid.Children.Add(element);
				}
			}
			else
			{
				service5.ActiveScene.SuspendDrawing();
				IActor actor = service5.ActiveScene.GetActor(name);
				if (actor != null)
				{
					service5.ActiveScene.Remove(actor);
				}
				else
				{
					IActor actor2 = service5.ActiveScene.CreateActor();
					actor2.Name = name;
					actor2.Y = 286;
					actor2.X = 0;
					actor2.Width = 1024;
					actor2.Height = 3;
					actor2.BackgroundColor = System.Drawing.Color.GreenYellow;
					actor2.Visible = true;
					actor2.HitFlags = HitTestFlags.None;
					actor2.ForegroundColor = System.Drawing.Color.GreenYellow;
					actor2.OptionFlags = RenderOptionFlags.Background;
					service5.ActiveScene.AddLast(actor2);
				}
				service5.ActiveScene.ResumeDrawing();
			}
			break;
		}
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
		case Keys.F12:
		{
			IViewService service2 = ServiceLocator.Instance.GetService<IViewService>();
			if (service2.Peek() == "product_detail_view")
			{
				service2.Push("title_details_dev_view", false);
				service2.Show();
			}
			break;
		}
		}
	}

	public void OnKeyPress(object sender, KeyPressEventArgs e)
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
				_keyPressStartAfter = DateTime.Now.AddSeconds(3.0);
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
			case '#':
				SendKeys.Send("{%}B4111111111111111{^}JOE/COOL                {^}1608041103263?;4111111111=1608041103263?");
				break;
			}
		}
		if (DateTime.Now > _keyPressStartAfter)
		{
			EngineApplication.Instance.ActivateKeyPressHandlers(new KeyPressEventArgsEx(e, System.Windows.Forms.Control.ModifierKeys));
		}
	}

	private void OnActivated(object sender, EventArgs e)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		if (service.ActiveScene != null)
		{
			service.ActiveScene.MakeDirty(null);
			EngineApplication.Instance.ActivateActivatedHandlers();
			Focus();
			BringToFront();
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
		base.KeyDown += new System.Windows.Forms.KeyEventHandler(OnKeyDown);
		base.ResumeLayout(false);
	}
}

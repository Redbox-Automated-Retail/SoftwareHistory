using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ShockwaveFlashObjects;

namespace AxShockwaveFlashObjects;

[Clsid("{d27cdb6e-ae6d-11cf-96b8-444553540000}")]
[DesignTimeVisible(true)]
public class AxShockwaveFlash : AxHost
{
	private IShockwaveFlash ocx;

	private AxShockwaveFlashEventMulticaster eventMulticaster;

	private ConnectionPointCookie cookie;

	[DispId(-525)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual int ReadyState
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ReadyState", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ReadyState;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(124)]
	public virtual int TotalFrames
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("TotalFrames", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.TotalFrames;
		}
	}

	[DispId(125)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool Playing
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Playing", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Playing;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Playing", ActiveXInvokeKind.PropertySet);
			}
			ocx.Playing = value;
		}
	}

	[DispId(105)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual int Quality
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Quality", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Quality;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Quality", ActiveXInvokeKind.PropertySet);
			}
			ocx.Quality = value;
		}
	}

	[DispId(120)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual int ScaleMode
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ScaleMode", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ScaleMode;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ScaleMode", ActiveXInvokeKind.PropertySet);
			}
			ocx.ScaleMode = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(121)]
	public virtual int AlignMode
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AlignMode", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AlignMode;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AlignMode", ActiveXInvokeKind.PropertySet);
			}
			ocx.AlignMode = value;
		}
	}

	[DispId(123)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual int BackgroundColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BackgroundColor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.BackgroundColor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BackgroundColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.BackgroundColor = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(106)]
	public virtual bool Loop
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Loop", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Loop;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Loop", ActiveXInvokeKind.PropertySet);
			}
			ocx.Loop = value;
		}
	}

	[DispId(102)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string Movie
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Movie", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Movie;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Movie", ActiveXInvokeKind.PropertySet);
			}
			ocx.Movie = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(107)]
	public virtual int FrameNum
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FrameNum", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.FrameNum;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FrameNum", ActiveXInvokeKind.PropertySet);
			}
			ocx.FrameNum = value;
		}
	}

	[DispId(133)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string WMode
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMode", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.WMode;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("WMode", ActiveXInvokeKind.PropertySet);
			}
			ocx.WMode = value;
		}
	}

	[DispId(134)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string SAlign
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SAlign", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.SAlign;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SAlign", ActiveXInvokeKind.PropertySet);
			}
			ocx.SAlign = value;
		}
	}

	[DispId(135)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool Menu
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Menu", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Menu;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Menu", ActiveXInvokeKind.PropertySet);
			}
			ocx.Menu = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(136)]
	public virtual string Base
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Base", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Base;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Base", ActiveXInvokeKind.PropertySet);
			}
			ocx.Base = value;
		}
	}

	[DispId(137)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string CtlScale
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CtlScale", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Scale;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("CtlScale", ActiveXInvokeKind.PropertySet);
			}
			ocx.Scale = value;
		}
	}

	[DispId(138)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool DeviceFont
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("DeviceFont", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.DeviceFont;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("DeviceFont", ActiveXInvokeKind.PropertySet);
			}
			ocx.DeviceFont = value;
		}
	}

	[DispId(139)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool EmbedMovie
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("EmbedMovie", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.EmbedMovie;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("EmbedMovie", ActiveXInvokeKind.PropertySet);
			}
			ocx.EmbedMovie = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(140)]
	public virtual string BGColor
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BGColor", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.BGColor;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("BGColor", ActiveXInvokeKind.PropertySet);
			}
			ocx.BGColor = value;
		}
	}

	[DispId(141)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string Quality2
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Quality2", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Quality2;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Quality2", ActiveXInvokeKind.PropertySet);
			}
			ocx.Quality2 = value;
		}
	}

	[DispId(159)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string SWRemote
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SWRemote", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.SWRemote;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SWRemote", ActiveXInvokeKind.PropertySet);
			}
			ocx.SWRemote = value;
		}
	}

	[DispId(170)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string FlashVars
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FlashVars", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.FlashVars;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("FlashVars", ActiveXInvokeKind.PropertySet);
			}
			ocx.FlashVars = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(171)]
	public virtual string AllowScriptAccess
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowScriptAccess", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AllowScriptAccess;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowScriptAccess", ActiveXInvokeKind.PropertySet);
			}
			ocx.AllowScriptAccess = value;
		}
	}

	[DispId(190)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string MovieData
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("MovieData", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.MovieData;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("MovieData", ActiveXInvokeKind.PropertySet);
			}
			ocx.MovieData = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(191)]
	public virtual object InlineData
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("InlineData", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.InlineData;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("InlineData", ActiveXInvokeKind.PropertySet);
			}
			ocx.InlineData = value;
		}
	}

	[DispId(192)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual bool SeamlessTabbing
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SeamlessTabbing", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.SeamlessTabbing;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("SeamlessTabbing", ActiveXInvokeKind.PropertySet);
			}
			ocx.SeamlessTabbing = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(194)]
	public virtual bool Profile
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Profile", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.Profile;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("Profile", ActiveXInvokeKind.PropertySet);
			}
			ocx.Profile = value;
		}
	}

	[DispId(195)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string ProfileAddress
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ProfileAddress", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ProfileAddress;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ProfileAddress", ActiveXInvokeKind.PropertySet);
			}
			ocx.ProfileAddress = value;
		}
	}

	[DispId(196)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual int ProfilePort
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ProfilePort", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.ProfilePort;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("ProfilePort", ActiveXInvokeKind.PropertySet);
			}
			ocx.ProfilePort = value;
		}
	}

	[DispId(201)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public virtual string AllowNetworking
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowNetworking", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AllowNetworking;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowNetworking", ActiveXInvokeKind.PropertySet);
			}
			ocx.AllowNetworking = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DispId(202)]
	public virtual string AllowFullScreen
	{
		get
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowFullScreen", ActiveXInvokeKind.PropertyGet);
			}
			return ocx.AllowFullScreen;
		}
		set
		{
			if (ocx == null)
			{
				throw new InvalidActiveXStateException("AllowFullScreen", ActiveXInvokeKind.PropertySet);
			}
			ocx.AllowFullScreen = value;
		}
	}

	public event _IShockwaveFlashEvents_FlashCallEventHandler FlashCall;

	public event _IShockwaveFlashEvents_FSCommandEventHandler FSCommand;

	public event _IShockwaveFlashEvents_OnProgressEventHandler OnProgress;

	public event _IShockwaveFlashEvents_OnReadyStateChangeEventHandler OnReadyStateChange;

	public AxShockwaveFlash()
		: base("d27cdb6e-ae6d-11cf-96b8-444553540000")
	{
	}

	public virtual void DisableLocalSecurity()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("DisableLocalSecurity", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.DisableLocalSecurity();
	}

	public virtual void SetReturnValue(string returnValue)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SetReturnValue", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.SetReturnValue(returnValue);
	}

	public virtual string CallFunction(string request)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("CallFunction", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.CallFunction(request);
	}

	public virtual void EnforceLocalSecurity()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("EnforceLocalSecurity", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.EnforceLocalSecurity();
	}

	public virtual double TGetPropertyAsNumber(string target, int property)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TGetPropertyAsNumber", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.TGetPropertyAsNumber(target, property);
	}

	public virtual double TGetPropertyNum(string target, int property)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TGetPropertyNum", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.TGetPropertyNum(target, property);
	}

	public virtual void TSetPropertyNum(string target, int property, double value)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TSetPropertyNum", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TSetPropertyNum(target, property, value);
	}

	public virtual void TCallLabel(string target, string label)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TCallLabel", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TCallLabel(target, label);
	}

	public virtual void TCallFrame(string target, int frameNum)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TCallFrame", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TCallFrame(target, frameNum);
	}

	public virtual string TGetProperty(string target, int property)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TGetProperty", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.TGetProperty(target, property);
	}

	public virtual void TSetProperty(string target, int property, string value)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TSetProperty", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TSetProperty(target, property, value);
	}

	public virtual string GetVariable(string name)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("GetVariable", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.GetVariable(name);
	}

	public virtual void SetVariable(string name, string value)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SetVariable", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.SetVariable(name, value);
	}

	public virtual void TStopPlay(string target)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TStopPlay", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TStopPlay(target);
	}

	public virtual void TPlay(string target)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TPlay", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TPlay(target);
	}

	public virtual string TCurrentLabel(string target)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TCurrentLabel", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.TCurrentLabel(target);
	}

	public virtual int TCurrentFrame(string target)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TCurrentFrame", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.TCurrentFrame(target);
	}

	public virtual void TGotoLabel(string target, string label)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TGotoLabel", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TGotoLabel(target, label);
	}

	public virtual void TGotoFrame(string target, int frameNum)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("TGotoFrame", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.TGotoFrame(target, frameNum);
	}

	public virtual void LoadMovie(int layer, string url)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("LoadMovie", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.LoadMovie(layer, url);
	}

	public virtual int FlashVersion()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("FlashVersion", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.FlashVersion();
	}

	public virtual bool FrameLoaded(int frameNum)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("FrameLoaded", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.FrameLoaded(frameNum);
	}

	public virtual int PercentLoaded()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("PercentLoaded", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.PercentLoaded();
	}

	public virtual bool IsPlaying()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("IsPlaying", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.IsPlaying();
	}

	public virtual int CurrentFrame()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("CurrentFrame", ActiveXInvokeKind.MethodInvoke);
		}
		return ocx.CurrentFrame();
	}

	public virtual void GotoFrame(int frameNum)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("GotoFrame", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.GotoFrame(frameNum);
	}

	public virtual void StopPlay()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("StopPlay", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.StopPlay();
	}

	public virtual void Rewind()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Rewind", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Rewind();
	}

	public virtual void Forward()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Forward", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Forward();
	}

	public virtual void Back()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Back", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Back();
	}

	public virtual void Stop()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Stop", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Stop();
	}

	public virtual void Play()
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Play", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Play();
	}

	public virtual void Pan(int x, int y, int mode)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Pan", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Pan(x, y, mode);
	}

	public virtual void Zoom(int factor)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("Zoom", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.Zoom(factor);
	}

	public virtual void SetZoomRect(int left, int top, int right, int bottom)
	{
		if (ocx == null)
		{
			throw new InvalidActiveXStateException("SetZoomRect", ActiveXInvokeKind.MethodInvoke);
		}
		ocx.SetZoomRect(left, top, right, bottom);
	}

	protected override void CreateSink()
	{
		try
		{
			eventMulticaster = new AxShockwaveFlashEventMulticaster(this);
			cookie = new ConnectionPointCookie(ocx, eventMulticaster, typeof(_IShockwaveFlashEvents));
		}
		catch (Exception)
		{
		}
	}

	protected override void DetachSink()
	{
		try
		{
			cookie.Disconnect();
		}
		catch (Exception)
		{
		}
	}

	protected override void AttachInterfaces()
	{
		try
		{
			ocx = (IShockwaveFlash)GetOcx();
		}
		catch (Exception)
		{
		}
	}

	internal void RaiseOnFlashCall(object sender, _IShockwaveFlashEvents_FlashCallEvent e)
	{
		if (FlashCall != null)
		{
			FlashCall(sender, e);
		}
	}

	internal void RaiseOnFSCommand(object sender, _IShockwaveFlashEvents_FSCommandEvent e)
	{
		if (FSCommand != null)
		{
			FSCommand(sender, e);
		}
	}

	internal void RaiseOnOnProgress(object sender, _IShockwaveFlashEvents_OnProgressEvent e)
	{
		if (OnProgress != null)
		{
			OnProgress(sender, e);
		}
	}

	internal void RaiseOnOnReadyStateChange(object sender, _IShockwaveFlashEvents_OnReadyStateChangeEvent e)
	{
		if (OnReadyStateChange != null)
		{
			OnReadyStateChange(sender, e);
		}
	}
}

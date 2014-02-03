using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Xml;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.REDS.Framework;

namespace Redbox.KioskEngine.Environment;

internal sealed class ViewFrame : IViewFrame, IDisposable
{
	private sealed class ActorProperties
	{
		[ActorPropertyTag(Name = "x")]
		public int X;

		[ActorPropertyTag(Name = "y")]
		public int Y;

		public Font Font;

		[ActorPropertyTag(Name = "width")]
		public int Width;

		[ActorPropertyTag(Name = "height")]
		public int Height;

		[ActorPropertyTag(Name = "tag")]
		public string Tag;

		public Image Image;

		[ActorPropertyTag(Name = "text")]
		public string Text;

		[ActorPropertyTag(Name = "onHit")]
		public string OnHit;

		public bool Enabled = true;

		[ActorPropertyTag(Name = "visible")]
		public bool Visible = true;

		[ActorPropertyTag(Name = "tabOrder")]
		public int? TabOrder;

		[ActorPropertyTag(Name = "opacity")]
		public float Opacity = 1f;

		public string StyleName;

		public IStyleSheetStyle Style;

		public string ErrorText;

		[ActorPropertyTag(Name = "cornerSize")]
		public Size? CornerSize;

		[ActorPropertyTag(Name = "strokeWeight")]
		public float StrokeWeight = 1f;

		[ActorPropertyTag(Name = "hotSpot")]
		public Rectangle? HotSpot;

		[ActorPropertyTag(Name = "borderColor")]
		public Color? BorderColor;

		[ActorPropertyTag(Name = "gradientAngle")]
		public float? GradientAngle;

		[ActorPropertyTag(Name = "hitFlags")]
		public HitTestFlags HitFlags;

		[ActorPropertyTag(Name = "textRegion")]
		public Rectangle? TextRegion;

		[ActorPropertyTag(Name = "backgroundColor")]
		public Color BackgroundColor;

		[ActorPropertyTag(Name = "foregroundColor")]
		public Color ForegroundColor;

		[ActorPropertyTag(Name = "relativeToActor")]
		public string RelativeToActor;

		[ActorPropertyTag(Name = "cornerSweepAngle")]
		public float? CornerSweepAngle;

		[ActorPropertyTag(Name = "textRotationAngle")]
		public float? TextRotationAngle;

		[ActorPropertyTag(Name = "gradientTargetColor")]
		public Color? GradientTargetColor;

		[ActorPropertyTag(Name = "textTranslationPoint")]
		public Point? TextTranslationPoint;

		[ActorPropertyTag(Name = "optionFlags")]
		public RenderOptionFlags OptionFlags;

		[ActorPropertyTag(Name = "verticalAlignment")]
		public StringAlignment VerticalAlignment;

		[ActorPropertyTag(Name = "horizontalAlignment")]
		public StringAlignment HorizontalAlignment;
	}

	private readonly Redbox.KioskEngine.ComponentModel.ErrorList m_errors = new Redbox.KioskEngine.ComponentModel.ErrorList();

	private readonly IDictionary<string, ActorProperties> m_actors = new Dictionary<string, ActorProperties>();

	public int Width { get; internal set; }

	public int Height { get; internal set; }

	public bool Clear { get; internal set; }

	public bool Cached { get; internal set; }

	public IScene Scene { get; internal set; }

	public string ViewName { get; internal set; }

	public string ViewVersion { get; internal set; }

	public string ActiveFlag { get; internal set; }

	public string SceneName { get; internal set; }

	public Rectangle? ViewWindow { get; internal set; }

	public Color BackgroundColor { get; internal set; }

	public Image BackgroundImage { get; internal set; }

	public IStyleSheet StyleSheet { get; internal set; }

	public string OnEnterResourceName { get; internal set; }

	public string OnLeaveResourceName { get; internal set; }

	public Redbox.KioskEngine.ComponentModel.ErrorList Errors => m_errors;

	public void UpdateScene()
	{
		try
		{
			Scene.SuspendDrawing();
			if (Clear)
			{
				Scene.Clear();
				Scene.BackgroundColor = BackgroundColor;
				Scene.BackgroundImage = BackgroundImage;
			}
			foreach (string key in m_actors.Keys)
			{
				ActorProperties actorProperties = m_actors[key];
				IActor actor = Scene.CreateActor();
				actor.Name = key;
				actor.Tag = actorProperties.Tag;
				actor.RelativeToActorName = actorProperties.RelativeToActor;
				actor.X = actorProperties.X;
				actor.Y = actorProperties.Y;
				actor.Font = actorProperties.Font;
				actor.Text = actorProperties.Text;
				actor.Style = actorProperties.Style;
				actor.Width = actorProperties.Width;
				actor.Height = actorProperties.Height;
				actor.Opacity = actorProperties.Opacity;
				actor.Enabled = actorProperties.Enabled;
				actor.Visible = actorProperties.Visible;
				actor.TabOrder = actorProperties.TabOrder;
				actor.HotSpot = actorProperties.HotSpot;
				actor.HitFlags = actorProperties.HitFlags;
				actor.StyleName = actorProperties.StyleName;
				actor.ErrorText = actorProperties.ErrorText;
				actor.TextRegion = actorProperties.TextRegion;
				actor.CornerSize = actorProperties.CornerSize;
				actor.BorderColor = actorProperties.BorderColor;
				actor.OptionFlags = actorProperties.OptionFlags;
				actor.StrokeWeight = actorProperties.StrokeWeight;
				actor.GradientAngle = actorProperties.GradientAngle;
				actor.BackgroundColor = actorProperties.BackgroundColor;
				actor.ForegroundColor = actorProperties.ForegroundColor;
				actor.CornerSweepAngle = actorProperties.CornerSweepAngle;
				actor.VerticalAlignment = actorProperties.VerticalAlignment;
				actor.TextRotationAngle = actorProperties.TextRotationAngle;
				actor.GradientTargetColor = actorProperties.GradientTargetColor;
				actor.HorizontalAlignment = actorProperties.HorizontalAlignment;
				actor.TextTranslationPoint = actorProperties.TextTranslationPoint;
				string onHitScriptName = actorProperties.OnHit;
				actor.Hit += delegate
				{
					try
					{
						if (!string.IsNullOrEmpty(onHitScriptName))
						{
							ViewService.Instance.LastHitActorTag = actor.Tag;
							ViewService.Instance.LastHitActorName = actor.Name;
							IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
							service.ExecuteScript(onHitScriptName);
						}
					}
					catch (Exception e2)
					{
						LogHelper.Instance.Log("An unhandled exception was raised in ViewFrame, actor.Hit delegate.", e2);
						LogHelper.Instance.Log("View name = {0}, actor name = {1}", ViewName, actor.Name);
					}
				};
				actor.Image = actorProperties.Image;
				Scene.AddLast(actor);
			}
			Scene.ResumeDrawing();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ViewFrame.UpdateScene.", e);
		}
	}

	public void RaiseOnEnter()
	{
		if (string.IsNullOrEmpty(OnEnterResourceName))
		{
			LogHelper.Instance.Log("No on enter script specified for view named: {0}", ViewName);
			return;
		}
		LogHelper.Instance.Log("Executing on enter script ({0}) for view named: {1}", OnEnterResourceName, ViewName);
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.ExecuteScript(OnEnterResourceName);
	}

	public void RaiseOnLeave()
	{
		if (string.IsNullOrEmpty(OnLeaveResourceName))
		{
			LogHelper.Instance.Log("No on leave script specified for view named: {0}", ViewName);
			return;
		}
		LogHelper.Instance.Log("Executing on leave script ({0}) for view named: {1}", OnLeaveResourceName, ViewName);
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		service.ExecuteScript(OnLeaveResourceName);
	}

	public void Parse()
	{
		Errors.Clear();
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IStyleSheetService service2 = ServiceLocator.Instance.GetService<IStyleSheetService>();
		IRenderingService service3 = ServiceLocator.Instance.GetService<IRenderingService>();
		IResource viewResource = GetViewResource();
		ViewVersion = viewResource["version"] as string;
		ActiveFlag = viewResource["active_flag"] as string;
		XmlNode xmlNode = GetViewGraph().ChildNodes[0];
		Clear = XmlNodeExtensions.GetAttributeValue(xmlNode, "clear", defaultValue: true);
		Width = XmlNodeExtensions.GetAttributeValue<int>(xmlNode, "width");
		Height = XmlNodeExtensions.GetAttributeValue<int>(xmlNode, "height");
		Cached = XmlNodeExtensions.GetAttributeValue<bool>(xmlNode, "cached");
		SceneName = XmlNodeExtensions.GetAttributeValue(xmlNode, "name", "Default");
		string attributeValue = XmlNodeExtensions.GetAttributeValue<string>(xmlNode, "stylesheet");
		if (!string.IsNullOrEmpty(attributeValue))
		{
			StyleSheet = service2.New(attributeValue);
		}
		ViewWindow = XmlNodeExtensions.GetAttributeValue<Rectangle?>(xmlNode, "viewWindow");
		OnEnterResourceName = XmlNodeExtensions.GetAttributeValue<string>(xmlNode, "onEnter");
		OnLeaveResourceName = XmlNodeExtensions.GetAttributeValue<string>(xmlNode, "onLeave");
		string attributeValue2 = XmlNodeExtensions.GetAttributeValue<string>(xmlNode, "backgroundImage");
		if (!string.IsNullOrEmpty(attributeValue2))
		{
			IMacroService service4 = ServiceLocator.Instance.GetService<IMacroService>();
			attributeValue2 = service4.ExpandProperties(attributeValue2);
			Image bitmap = service.GetBitmap(attributeValue2);
			if (bitmap != null)
			{
				BackgroundImage = bitmap;
			}
			else
			{
				Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V002", $"backgroundImage: Bitmap resource '{attributeValue2}' not found.", "The resource name specified in the view attribute backgroundImage was not found."));
			}
		}
		BackgroundColor = XmlNodeExtensions.GetAttributeValue(xmlNode, "backgroundColor", Color.FromArgb(153, 153, 152));
		XmlNodeList xmlNodeList = xmlNode.SelectNodes("actor");
		if (xmlNodeList == null)
		{
			return;
		}
		Scene = service3.GetScene(SceneName) ?? service3.GetScene("Default");
		foreach (XmlNode item in xmlNodeList)
		{
			string attributeValue3 = XmlNodeExtensions.GetAttributeValue<string>(item, "name");
			ActorProperties actorProperties = (m_actors.ContainsKey(attributeValue3) ? m_actors[attributeValue3] : new ActorProperties());
			actorProperties.Enabled = XmlNodeExtensions.GetAttributeValue(item, "enabled", defaultValue: true);
			IStyleSheetState styleSheetState = null;
			if (StyleSheet != null)
			{
				string attributeValue4 = XmlNodeExtensions.GetAttributeValue<string>(item, "style");
				if (!string.IsNullOrEmpty(attributeValue4))
				{
					actorProperties.StyleName = attributeValue4;
					IStyleSheetStyle style = StyleSheet.GetStyle(attributeValue4);
					if (style != null)
					{
						actorProperties.Style = style;
						styleSheetState = style.GetState(actorProperties.Enabled ? "enabled" : "disabled");
					}
				}
			}
			FieldInfo[] fields = typeof(ActorProperties).GetFields(BindingFlags.Instance | BindingFlags.Public);
			foreach (FieldInfo fieldInfo in fields)
			{
				ActorPropertyTagAttribute actorPropertyTagAttribute = (ActorPropertyTagAttribute)Attribute.GetCustomAttribute(fieldInfo, typeof(ActorPropertyTagAttribute));
				if (actorPropertyTagAttribute == null)
				{
					continue;
				}
				object obj = XmlNodeExtensions.GetAttributeValue<object>(item, actorPropertyTagAttribute.Name);
				if (obj == null)
				{
					if (styleSheetState == null)
					{
						continue;
					}
					obj = styleSheetState[actorPropertyTagAttribute.Name];
				}
				if (obj != null)
				{
					try
					{
						object value = ConversionHelper.ChangeType(obj, fieldInfo.FieldType);
						fieldInfo.SetValue(actorProperties, value);
					}
					catch (Exception e)
					{
						LogHelper.Instance.Log($"Unable to set actor property '{actorPropertyTagAttribute.Name}' with value '{obj}'.", e);
					}
				}
			}
			string text = XmlNodeExtensions.GetAttributeValue<string>(item, "font");
			if (string.IsNullOrEmpty(text) && styleSheetState != null)
			{
				text = styleSheetState["font"] as string;
			}
			actorProperties.Font = service.GetFont(text, out var resourceFound);
			if (!string.IsNullOrEmpty(text) && !resourceFound)
			{
				Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V003", $"{attributeValue3}.font: Font resource '{text}' not found.", $" Actor {attributeValue3} is referencing a font resource named '{text}' that was not found."));
			}
			string text2 = XmlNodeExtensions.GetAttributeValue<string>(item, "image");
			if (string.IsNullOrEmpty(text2) && styleSheetState != null)
			{
				text2 = styleSheetState["image"] as string;
			}
			if (!string.IsNullOrEmpty(text2))
			{
				IMacroService service5 = ServiceLocator.Instance.GetService<IMacroService>();
				text2 = service5.ExpandProperties(text2);
				Image bitmap2 = service.GetBitmap(text2);
				if (bitmap2 != null)
				{
					actorProperties.Image = bitmap2;
				}
				else
				{
					actorProperties.BackgroundColor = Color.DarkRed;
					actorProperties.Width = 32;
					actorProperties.Height = 32;
					actorProperties.OptionFlags |= RenderOptionFlags.DrawBorder;
					actorProperties.ErrorText = $"Bitmap resource '{text2}' not found.";
					Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("V004", $"{attributeValue3}.image: Bitmap resource '{text2}' not found.", $"Actor {attributeValue3} is referencing a bitmap resource named '{text2}' that was not found."));
				}
			}
			if (ViewWindow.HasValue)
			{
				actorProperties.X += ViewWindow.Value.X;
				actorProperties.Y += ViewWindow.Value.Y;
			}
			m_actors[attributeValue3] = actorProperties;
		}
	}

	public void Dispose()
	{
	}

	private XmlNode GetViewGraph()
	{
		return (XmlNode)GetViewResource().GetAspect("content").GetContent();
	}

	private IResource GetViewResource()
	{
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		if (service == null)
		{
			return null;
		}
		IResource resource = service.GetResource(ViewName);
		if (resource == null)
		{
			throw new ArgumentException($"View resource {ViewName} was not found.");
		}
		return resource;
	}
}

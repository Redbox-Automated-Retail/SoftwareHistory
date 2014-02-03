using System;
using System.Collections.Generic;
using System.Drawing;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;

namespace Redbox.KioskEngine.Kernel;

internal static class ActorFunctions
{
	[KernelFunction(Name = "kernel.createactor")]
	internal static void CreateActor(string actorName, bool addToFront, int x, int y, int width, int height, string text, string font, string image, object backgroundColor, object foregroundColor, object textRegion, object textTranslationPoint, object textRotationAngle, object hotSpot, string renderOptions, string hitOptions)
	{
		if (string.IsNullOrEmpty(actorName))
		{
			return;
		}
		IResourceBundleService service = ServiceLocator.Instance.GetService<IResourceBundleService>();
		IRenderingService service2 = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene scene = service2.GetScene("Default");
		if (scene == null)
		{
			return;
		}
		IViewService service3 = ServiceLocator.Instance.GetService<IViewService>();
		IActor actor = scene.CreateActor();
		actor.Name = actorName;
		IViewFrame viewFrame = service3.PeekViewFrame();
		if (viewFrame != null && viewFrame.ViewWindow.HasValue)
		{
			actor.X = x + viewFrame.ViewWindow.Value.X;
			actor.Y = y + viewFrame.ViewWindow.Value.Y;
		}
		else
		{
			actor.X = x;
			actor.Y = y;
		}
		actor.Text = text;
		if (textRotationAngle != null)
		{
			float value = Convert.ToSingle(textRotationAngle);
			actor.TextRotationAngle = value;
		}
		if (textTranslationPoint is LuaTable luaTable)
		{
			actor.TextTranslationPoint = luaTable.ToPoint();
			luaTable.Dispose();
		}
		Color? colorFromValue = GetColorFromValue(backgroundColor);
		if (colorFromValue.HasValue)
		{
			actor.BackgroundColor = colorFromValue.Value;
		}
		Color? colorFromValue2 = GetColorFromValue(foregroundColor);
		if (colorFromValue2.HasValue)
		{
			actor.ForegroundColor = colorFromValue2.Value;
		}
		LuaTable luaTable2 = textRegion as LuaTable;
		if (luaTable2 != null)
		{
			actor.TextRegion = luaTable2.ToRectangle();
			luaTable2.Dispose();
		}
		if (hotSpot is LuaTable luaTable3)
		{
			actor.HotSpot = luaTable2.ToRectangle();
			luaTable3.Dispose();
		}
		if (!string.IsNullOrEmpty(renderOptions))
		{
			actor.OptionFlags = (RenderOptionFlags)ConversionHelper.ChangeType(renderOptions, typeof(RenderOptionFlags));
		}
		if (!string.IsNullOrEmpty(hitOptions))
		{
			actor.HitFlags = (HitTestFlags)ConversionHelper.ChangeType(hitOptions, typeof(HitTestFlags));
		}
		if (!string.IsNullOrEmpty(font))
		{
			actor.Font = service.GetFont(font, out var resourceFound);
			if (!resourceFound)
			{
				actor.OptionFlags |= RenderOptionFlags.DrawBorder;
				actor.ErrorText = $"Font resource '{font}' not found.";
			}
		}
		if (!string.IsNullOrEmpty(image))
		{
			if (image.StartsWith("cache:"))
			{
				string text2 = image.Substring(6);
				IBitmapCacheService service4 = ServiceLocator.Instance.GetService<IBitmapCacheService>();
				if (service4 == null)
				{
					actor.OptionFlags |= RenderOptionFlags.DrawBorder;
					actor.ErrorText = "IBitmapCacheService not available from ServiceLocator.";
				}
				else
				{
					Image image2 = service4.GetImage(text2);
					if (image2 == null)
					{
						actor.OptionFlags |= RenderOptionFlags.DrawBorder;
						actor.ErrorText = $"Bitmap '{text2}' not in cache.";
					}
					else
					{
						actor.Image = image2;
					}
				}
			}
			else
			{
				Image bitmap = service.GetBitmap(image);
				if (bitmap == null)
				{
					actor.OptionFlags |= RenderOptionFlags.DrawBorder;
					actor.ErrorText = $"Bitmap resource '{image}' not found.";
				}
				else
				{
					actor.Image = bitmap;
				}
			}
		}
		actor.Width = width;
		actor.Height = height;
		if (addToFront)
		{
			scene.AddFirst(actor);
		}
		else
		{
			scene.AddLast(actor);
		}
	}

	[KernelFunction(Name = "kernel.markactordirty")]
	internal static void MarkActorDirty(string actorName)
	{
		GetActor(actorName)?.MarkDirty();
	}

	[KernelFunction(Name = "kernel.setactortag")]
	internal static void SetActorTag(string actorName, object tag)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Tag = tag;
		}
	}

	[KernelFunction(Name = "kernel.setactortext")]
	internal static void SetActorText(string actorName, string text)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Text = text;
		}
	}

	[KernelFunction(Name = "kernel.setactorhtml")]
	internal static void SetActorHtml(string actorName, string html)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Html = html;
		}
	}

	[KernelFunction(Name = "kernel.setrelativeactor")]
	internal static void SetReleativeToActor(string actorName, string relativeActor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.RelativeToActorName = relativeActor;
		}
	}

	[KernelFunction(Name = "kernel.setactortextalignx")]
	internal static void SetActorTextAlignX(string actorName, string alignment)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.HorizontalAlignment = Enum<StringAlignment>.ParseIgnoringCase(alignment, StringAlignment.Near);
		}
	}

	[KernelFunction(Name = "kernel.setactortextaligny")]
	internal static void SetActorTextAlignY(string actorName, string alignment)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.VerticalAlignment = Enum<StringAlignment>.ParseIgnoringCase(alignment, StringAlignment.Near);
		}
	}

	[KernelFunction(Name = "kernel.setactorforegroundcolor")]
	internal static void SetActorForegroundColor(string actorName, object foregroundColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && foregroundColor != null)
		{
			Color? colorFromValue = GetColorFromValue(foregroundColor);
			if (colorFromValue.HasValue)
			{
				actor.ForegroundColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorbackgroundcolor")]
	internal static void SetActorBackgroundColor(string actorName, object backgroundColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && backgroundColor != null)
		{
			Color? colorFromValue = GetColorFromValue(backgroundColor);
			if (colorFromValue.HasValue)
			{
				actor.BackgroundColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorgradienttargetcolor")]
	internal static void SetActorGradientTargetColor(string actorName, object gradientTargetColor)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && gradientTargetColor != null)
		{
			Color? colorFromValue = GetColorFromValue(gradientTargetColor);
			if (colorFromValue.HasValue)
			{
				actor.GradientTargetColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorgradientangle")]
	internal static void SetActorGradientAngle(string actorName, float gradientAngle)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.GradientAngle = gradientAngle;
		}
	}

	[KernelFunction(Name = "kernel.setactorimage")]
	internal static void SetActorImage(string actorName, string resourceName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null && !string.IsNullOrEmpty(resourceName))
		{
			if (resourceName.StartsWith("cache:"))
			{
				IBitmapCacheService service = ServiceLocator.Instance.GetService<IBitmapCacheService>();
				actor.Image = service.GetImage(resourceName.Substring(6));
			}
			else
			{
				IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
				actor.Image = service2.GetBitmap(resourceName);
			}
		}
	}

	[KernelFunction(Name = "kernel.isactorimageset")]
	internal static bool IsActorImageSet(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return false;
		}
		return actor.Image != null;
	}

	[KernelFunction(Name = "kernel.isactorvisible")]
	internal static bool IsActorVisible(string actorName)
	{
		return GetActor(actorName)?.Visible ?? false;
	}

	[KernelFunction(Name = "kernel.isactorenabled")]
	internal static bool IsActorEnabled(string actorName)
	{
		return GetActor(actorName)?.Enabled ?? false;
	}

	[KernelFunction(Name = "kernel.clearactorimage")]
	internal static void ClearActorImage(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return;
		}
		try
		{
			actor.Image = null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ActorFunctions.ClearActorImage.", e);
		}
	}

	[KernelFunction(Name = "kernel.setactoropacity")]
	internal static void SetActorOpacity(string actorName, float opacity)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Opacity = opacity;
		}
	}

	[KernelFunction(Name = "kernel.setactorlocation")]
	internal static void SetActorLocation(string actorName, int x, int y)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			IViewFrame viewFrame = service.PeekViewFrame();
			if (viewFrame != null && viewFrame.ViewWindow.HasValue)
			{
				actor.X = x + viewFrame.ViewWindow.Value.X;
				actor.Y = y + viewFrame.ViewWindow.Value.Y;
			}
			else
			{
				actor.X = x;
				actor.Y = y;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactorframe")]
	internal static void SetActorFrame(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.FrameNumber++;
		}
	}

	[KernelFunction(Name = "kernel.getactorx")]
	internal static int GetActorX(string actorName)
	{
		return GetActor(actorName)?.X ?? 0;
	}

	[KernelFunction(Name = "kernel.getactory")]
	internal static int GetActorY(string actorName)
	{
		return GetActor(actorName)?.Y ?? 0;
	}

	[KernelFunction(Name = "kernel.moveactorx")]
	internal static void MoveActorX(string actorName, int pixels)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.X += pixels;
		}
	}

	[KernelFunction(Name = "kernel.moveactory")]
	internal static void MoveActorY(string actorName, int pixels)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Y += pixels;
		}
	}

	[KernelFunction(Name = "kernel.setactorx")]
	internal static void SetActorX(string actorName, int x)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			IViewFrame viewFrame = service.PeekViewFrame();
			if (viewFrame != null && viewFrame.ViewWindow.HasValue)
			{
				actor.X = x + viewFrame.ViewWindow.Value.X;
			}
			else
			{
				actor.X = x;
			}
		}
	}

	[KernelFunction(Name = "kernel.setactory")]
	internal static void SetActorY(string actorName, int y)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			IViewService service = ServiceLocator.Instance.GetService<IViewService>();
			IViewFrame viewFrame = service.PeekViewFrame();
			if (viewFrame != null && viewFrame.ViewWindow.HasValue)
			{
				actor.Y = y + viewFrame.ViewWindow.Value.Y;
			}
			else
			{
				actor.Y = y;
			}
		}
	}

	[KernelFunction(Name = "kernel.getactorheight")]
	internal static int GetActorHeight(string actorName)
	{
		return GetActor(actorName)?.Height ?? 0;
	}

	[KernelFunction(Name = "kernel.getactorwidth")]
	internal static int GetActorWidth(string actorName)
	{
		return GetActor(actorName)?.Width ?? 0;
	}

	[KernelFunction(Name = "kernel.setactorheight")]
	internal static void SetActorHeight(string actorName, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Height = height;
		}
	}

	[KernelFunction(Name = "kernel.setactorwidth")]
	internal static void SetActorWidth(string actorName, int width)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Width = width;
		}
	}

	[KernelFunction(Name = "kernel.setactordimension")]
	internal static void SetActorDimension(string actorName, int width, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Width = width;
			actor.Height = height;
		}
	}

	[KernelFunction(Name = "kernel.measureactortext")]
	internal static LuaTable MeasureActorText(string actorName)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene scene = service.GetScene("Default");
		if (scene == null || string.IsNullOrEmpty(actorName))
		{
			return null;
		}
		IActor actor = scene.GetActor(actorName);
		if (actor == null)
		{
			return null;
		}
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		MeasureTextResult measureTextResult = actor.MeasureText();
		luaTable["width"] = measureTextResult.Size.Width;
		luaTable["height"] = measureTextResult.Size.Height;
		luaTable["characters_fitted"] = measureTextResult.CharactersFitted;
		luaTable["lines_filled"] = measureTextResult.LinesFilled;
		KernelService.Instance.LuaRuntime[actorName] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hideactor")]
	internal static void HideActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Visible = false;
		}
	}

	[KernelFunction(Name = "kernel.showactor")]
	internal static void ShowActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Visible = true;
		}
	}

	[KernelFunction(Name = "kernel.removeactor")]
	internal static void RemoveActor(string actorName)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		service.GetScene("Default")?.Remove(actorName)?.Dispose();
	}

	[KernelFunction(Name = "kernel.removeactors")]
	internal static void RemoveActors(LuaTable actorNames)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene scene = service.GetScene("Default");
		if (scene == null || actorNames == null)
		{
			return;
		}
		scene.SuspendDrawing();
		foreach (object key in actorNames.Keys)
		{
			scene.Remove((string)actorNames[key])?.Dispose();
		}
		scene.ResumeDrawing();
		actorNames.Dispose();
	}

	[KernelFunction(Name = "kernel.clearactors")]
	internal static void ClearActors()
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		service.GetScene("Default")?.Clear();
	}

	[KernelFunction(Name = "kernel.centeractorx")]
	internal static void CenterActorX(string actorName)
	{
		GetActor(actorName)?.CenterHorizontally();
	}

	[KernelFunction(Name = "kernel.centeractory")]
	internal static void CenterActorY(string actorName)
	{
		GetActor(actorName)?.CenterVertically();
	}

	[KernelFunction(Name = "kernel.enableactor")]
	internal static void EnableActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Enabled = true;
		}
	}

	[KernelFunction(Name = "kernel.disableactor")]
	internal static void DisableActor(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.Enabled = false;
		}
	}

	[KernelFunction(Name = "kernel.registeractorhitnotify")]
	internal static void RegisterActorHitNotify(string actorName, HitHandler hitHandler)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return;
		}
		if (hitHandler == null)
		{
			LogHelper.Instance.Log("WARNING: hitHandler is null in kernel.registeractorhitnotify for actorName = '{0}'!", actorName);
			return;
		}
		actor.ClearHit();
		actor.Hit += delegate
		{
			hitHandler();
		};
	}

	[KernelFunction(Name = "kernel.executehitnotify")]
	internal static void ExecuteHitNotify(string actorName)
	{
		GetActor(actorName)?.RaiseHit();
	}

	[KernelFunction(Name = "kernel.getactorsbytag")]
	internal static LuaTable GetActorsByTag(string tag)
	{
		LuaTable luaTable = new LuaTable(KernelService.Instance.LuaRuntime);
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene scene = service.GetScene("Default");
		List<IActor> actorByTag = scene.GetActorByTag(tag);
		int num = 1;
		foreach (IActor item in actorByTag)
		{
			luaTable[num++] = item.Name;
		}
		KernelService.Instance.LuaRuntime["actors_by_tag"] = null;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.getactorbytaborder")]
	internal static string GetActorByTabOrder(int tabOrder)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		IScene scene = service.GetScene("Default");
		return scene.GetActorByTabOrder(tabOrder)?.Name;
	}

	[KernelFunction(Name = "kernel.getactortag")]
	internal static object GetActorTag(string name)
	{
		return GetActor(name)?.Tag;
	}

	[KernelFunction(Name = "kernel.getactortext")]
	internal static string GetActorText(string name)
	{
		return GetActor(name)?.Text;
	}

	[KernelFunction(Name = "kernel.setactortabstops")]
	internal static void SetActorTabStops(string name, object tabStops)
	{
		IActor actor = GetActor(name);
		if (actor != null && tabStops is LuaTable luaTable)
		{
			actor.TabStops = luaTable.ToList<float>().ToArray();
			luaTable.Dispose();
		}
	}

	[KernelFunction(Name = "kernel.gettaborder")]
	internal static int? GetTabOrder(string actorName)
	{
		return GetActor(actorName)?.TabOrder;
	}

	[KernelFunction(Name = "kernel.settaborder")]
	internal static void SetTabOrder(string actorName, int tabOrder)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.TabOrder = tabOrder;
		}
	}

	[KernelFunction(Name = "kernel.setbordercolor")]
	internal static void SetBorderColor(string actorName, object value)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			Color? colorFromValue = GetColorFromValue(value);
			if (colorFromValue.HasValue)
			{
				actor.BorderColor = colorFromValue.Value;
			}
		}
	}

	[KernelFunction(Name = "kernel.setborderstrokeweight")]
	internal static void SetBorderStrokeWeight(string actorName, float weight)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.StrokeWeight = weight;
		}
	}

	[KernelFunction(Name = "kernel.setcornersize")]
	internal static void SetCornerSize(string actorName, int width, int height)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.CornerSize = new Size(width, height);
		}
	}

	[KernelFunction(Name = "kernel.clearcornersize")]
	internal static void ClearCornerSize(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor != null)
		{
			actor.CornerSize = null;
		}
	}

	[KernelFunction(Name = "kernel.getlasthitactortag")]
	internal static object GetLastHitActorTag()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.LastHitActorTag;
	}

	[KernelFunction(Name = "kernel.setlasthitactortag")]
	internal static void SetLastHitActorTag(string actorTag)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.LastHitActorTag = actorTag;
		}
	}

	[KernelFunction(Name = "kernel.getlasthitactorname")]
	internal static string GetLastHitActorName()
	{
		return ServiceLocator.Instance.GetService<IViewService>()?.LastHitActorName;
	}

	[KernelFunction(Name = "kernel.setlasthitactorname")]
	internal static void SetLastHitActorName(string actorName)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			service.LastHitActorName = actorName;
		}
	}

	[KernelFunction(Name = "kernel.getactortextregion")]
	internal static string GetActorTextRegion(string actorName)
	{
		IActor actor = GetActor(actorName);
		if (actor == null)
		{
			return "0, 0, 0, 0";
		}
		Rectangle value = actor.TextRegion.Value;
		return value.X + ", " + value.Y + ", " + value.Width + ", " + value.Height;
	}

	[KernelFunction(Name = "kernel.getactoropacity")]
	internal static float GetActorOpacity(string actorName)
	{
		return GetActor(actorName)?.Opacity ?? 1f;
	}

	private static Color? GetColorFromValue(object value)
	{
		if (value is LuaTable luaTable)
		{
			Color? color = luaTable.ToColor();
			if (color.HasValue)
			{
				return color.Value;
			}
			luaTable.Dispose();
		}
		else if (value is string name)
		{
			return Color.FromName(name);
		}
		return null;
	}

	private static IActor GetActor(string name)
	{
		IRenderingService service = ServiceLocator.Instance.GetService<IRenderingService>();
		return service.GetScene("Default")?.GetActor(name);
	}
}

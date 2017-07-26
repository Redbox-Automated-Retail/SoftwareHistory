using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseFilterBarControl
{
	public Action StartOverAction;

	public Action UpdateBrowseViewAction;

	public BrowseFilterButtonEventHandler BrowseFilterButtonAction;

	private const string backgroundActorName = "center_gray_background";

	private const string startOverActorName = "startover_button";

	private const string startOverArrowActorName = "startover_arrow";

	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public List<BrowseFilterButton> Buttons { get; set; }

	public int ButtonSpacing { get; set; }

	public int ButtonLeftOffset { get; set; }

	public void ClearActors()
	{
		ActorFunctions.RemoveActor("center_gray_background");
		ActorFunctions.RemoveActor("startover_button");
		ActorFunctions.RemoveActor("startover_arrow");
		foreach (BrowseFilterButton button in Buttons)
		{
			ActorFunctions.RemoveActor(button.Name);
		}
	}

	public void Render()
	{
		RenderBackground();
		RenderStartOverButton();
		RenderFilterbuttons();
	}

	private void RenderFilterbuttons()
	{
		foreach (BrowseFilterButton button in Buttons)
		{
			string name = button.Name;
			int buttonIndex = Buttons.IndexOf(button);
			ActorFunctions.RemoveActor(name);
			BundleFunctions.GetDictionary("strings");
			string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", button.Name);
			int num = 16;
			int width = button.Width;
			int height = button.Height;
			int num2 = 13;
			int x = X + ButtonLeftOffset + Buttons.Where((BrowseFilterButton item) => Buttons.IndexOf(item) < buttonIndex).Sum((BrowseFilterButton y) => y.Width + ButtonSpacing);
			ActorFunctions.CreateActor(name, addToFront: false, x, Y + num2, width, height, dictionaryStringValue, "font_akzidenz_regular_smaller", button.IsSelected ? "image_selected_button" : null, null, null, null, null, null, null, button.IsSelected ? "Image, Text" : "BackgroundGradient, DrawBorder, Text", button.IsSelected ? null : "Enabled");
			IActor actor = ActorFunctions.GetActor(name);
			if (actor != null)
			{
				actor.CornerSize = new Size(num, num);
				actor.GradientAngle = 90f;
				actor.BorderColor = Color.White;
				actor.BackgroundColor = Color.FromArgb(255, 111, 111, 111);
				actor.GradientTargetColor = Color.FromArgb(255, 70, 70, 70);
				actor.ForegroundColor = (button.IsSelected ? Color.Black : Color.White);
				actor.VerticalAlignment = StringAlignment.Center;
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.StrokeWeight = 2.5f;
				actor.Hit += ButtonFilterActorHit;
				actor.Tag = button;
			}
		}
	}

	private void ButtonFilterActorHit(object sender, EventArgs e)
	{
		if (BrowseFilterButtonAction != null && sender is IActor actor)
		{
			BrowseFilterButton browseFilterButton = actor.Tag as BrowseFilterButton;
			BrowseFilterButtonAction(this, browseFilterButton);
		}
	}

	private void RenderBackground()
	{
		ActorFunctions.RemoveActor("center_gray_background");
		ActorFunctions.CreateActor("center_gray_background", addToFront: false, X, Y, Width, Height, null, null, null, null, null, null, null, null, null, "Background", null);
		IActor actor = ActorFunctions.GetActor("center_gray_background");
		if (actor != null)
		{
			actor.BackgroundColor = Color.FromArgb(255, 167, 167, 167);
		}
	}

	private void RenderStartOverButton()
	{
		ActorFunctions.RemoveActor("startover_button");
		BundleFunctions.GetDictionary("strings");
		string dictionaryStringValue = BundleFunctions.GetDictionaryStringValue("strings", "start_over_button");
		int num = 18;
		int width = 115;
		int height = 36;
		int num2 = 11;
		if (BundleFunctions.GetCulture() == "es-MX")
		{
			num = 22;
			width = 121;
			height = 46;
			num2 = 6;
		}
		ActorFunctions.CreateActor("startover_button", addToFront: false, X + 8, Y + num2, width, height, dictionaryStringValue, "font_target_smaller", null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", "Enabled");
		IActor actor = ActorFunctions.GetActor("startover_button");
		if (actor != null)
		{
			actor.CornerSize = new Size(num, num);
			actor.GradientAngle = 90f;
			actor.BorderColor = Color.White;
			actor.BackgroundColor = Color.FromArgb(255, 111, 111, 111);
			actor.GradientTargetColor = Color.FromArgb(255, 70, 70, 70);
			actor.ForegroundColor = Color.White;
			actor.VerticalAlignment = StringAlignment.Center;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.StrokeWeight = 2f;
			actor.Hit += StartOverActorHit;
			if (BundleFunctions.GetCulture() == "es-MX")
			{
				actor.TextRegion = new Rectangle(25, 1, 85, 38);
			}
		}
		ActorFunctions.RemoveActor("startover_arrow");
		num2 = 18;
		if (BundleFunctions.GetCulture() == "es-MX")
		{
			num2 = 21;
		}
		ActorFunctions.CreateActor("startover_arrow", addToFront: false, X + 20, Y + num2, 14, 15, dictionaryStringValue, "font_target_smaller", "image_start_over_arrow", null, null, null, null, null, null, "Image", null);
	}

	private void StartOverActorHit(object sender, EventArgs e)
	{
		if (StartOverAction != null)
		{
			StartOverAction();
		}
	}
}

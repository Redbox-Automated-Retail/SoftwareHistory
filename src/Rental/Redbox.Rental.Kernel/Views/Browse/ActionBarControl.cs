using System;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.Browse;

public class ActionBarControl
{
	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public int NumberOfCartItems { get; set; }

	public bool ShowNoThanks { get; set; }

	public event EventHandler OnCheckoutButtonClick;

	public void Render()
	{
		RenderBackground();
		RenderLogo();
		RenderCheckoutButton();
	}

	private void RenderCheckoutButton()
	{
		string text = null;
		if (ShowNoThanks)
		{
			text = BundleFunctions.GetDictionaryStringValue("strings", "no_thanks_button");
		}
		else
		{
			string text2 = string.Format("{0}", BundleFunctions.GetDictionaryStringValue("strings", "check_out_button_text"));
			if (text2.Contains("{0}"))
			{
				text = string.Format(text2, NumberOfCartItems);
			}
		}
		ActorFunctions.RemoveActor("my_rentals_button");
		ActorFunctions.CreateActor("my_rentals_button", addToFront: false, X + 790, Y + 38, 218, 66, text, "font_target_small_bold", "image_redbox_logo", null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", "Enabled");
		IActor actor = ActorFunctions.GetActor("my_rentals_button");
		if (actor != null)
		{
			actor.CornerSize = new Size(30, 30);
			actor.GradientAngle = 90f;
			if (ShowNoThanks)
			{
				actor.BorderColor = Color.FromArgb(157, 157, 157);
				actor.BackgroundColor = Color.FromArgb(111, 111, 111);
				actor.GradientTargetColor = Color.FromArgb(70, 70, 70);
				actor.StrokeWeight = 2.5f;
			}
			else
			{
				actor.BorderColor = Color.FromArgb(134, 12, 19);
				actor.BackgroundColor = Color.FromArgb(181, 18, 27);
				actor.GradientTargetColor = Color.FromArgb(143, 14, 21);
				actor.StrokeWeight = 3f;
			}
			actor.ForegroundColor = Color.White;
			actor.VerticalAlignment = StringAlignment.Center;
			actor.HorizontalAlignment = StringAlignment.Center;
			actor.Hit += CheckoutButtonClick;
		}
	}

	private void CheckoutButtonClick(object sender, EventArgs e)
	{
		if (OnCheckoutButtonClick != null)
		{
			OnCheckoutButtonClick(sender, e);
		}
	}

	private void RenderLogo()
	{
		ActorFunctions.RemoveActor("redbox_logo");
		ActorFunctions.CreateActor("redbox_logo", addToFront: false, X + 50, Y + 45, 133, 40, null, null, "image_redbox_logo", null, null, null, null, null, null, "Image", null);
	}

	private void RenderBackground()
	{
		ActorFunctions.RemoveActor("action_bar_background");
		ActorFunctions.CreateActor("action_bar_background", addToFront: false, X, Y, Width, Height, null, null, null, null, null, null, null, null, null, "Background", null);
		IActor actor = ActorFunctions.GetActor("action_bar_background");
		if (actor != null)
		{
			actor.BackgroundColor = Color.FromArgb(60, 56, 57);
		}
	}
}

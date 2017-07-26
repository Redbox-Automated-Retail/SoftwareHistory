using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.Browse;

public class BrowseGroupSelectionControl
{
	private const int widthPadding = 14;

	private const string backgroundActorName = "browse_group_selection_background_shadow";

	private const string backgroundWhiteActorName = "browse_group_selection_background_white";

	private const string selectedLabelActorName = "selected_label";

	private IActor _backgroundShadowActor;

	private IActor _backgroundWhiteActor;

	private List<IActor> _browseGroupActors = new List<IActor>();

	public int X { get; set; }

	public int Y { get; set; }

	public List<IBrowseGroup> BrowseGroups { get; set; }

	public event BrowseGroupSelectedEvent OnGroupSelected;

	public void Render()
	{
		RenderBackground();
		RenderGroups();
		RenderSelectedGroup();
	}

	public void ClearActors()
	{
		ActorFunctions.RemoveActor("browse_group_selection_background_shadow");
		ActorFunctions.RemoveActor("browse_group_selection_background_white");
		foreach (IBrowseGroup browseGroup in BrowseGroups)
		{
			int num = BrowseGroups.IndexOf(browseGroup);
			ActorFunctions.RemoveActor(GetBrowseGroupActorName(browseGroup));
			if (num < BrowseGroups.Count - 1)
			{
				ActorFunctions.RemoveActor($"browse_group_separator_1_actor_{browseGroup.Name}");
				ActorFunctions.RemoveActor($"browse_group_separator_2_actor_{browseGroup.Name}");
			}
		}
		ActorFunctions.RemoveActor("selected_label");
	}

	public void RenderSelectedGroup()
	{
		ActorFunctions.RemoveActor("selected_label");
		IBrowseGroup browseGroup = BrowseGroups.FirstOrDefault((IBrowseGroup x) => x.IsSelected);
		if (browseGroup == null)
		{
			return;
		}
		IActor actor = ActorFunctions.GetActor(GetBrowseGroupActorName(browseGroup));
		if (actor != null)
		{
			ActorFunctions.CreateActor("selected_label", addToFront: false, actor.X - 1, actor.Y - 2, actor.Width - 1, actor.Height + 4, actor.Text, "font_akzidenz_bold_smaller_14", null, null, "white", null, null, null, null, "BackgroundGradient, Text, DrawBorder", null);
			IActor actor2 = ActorFunctions.GetActor("selected_label");
			if (actor2 != null)
			{
				actor2.HorizontalAlignment = StringAlignment.Center;
				actor2.VerticalAlignment = StringAlignment.Center;
				actor2.BackgroundColor = Color.FromArgb(255, 121, 0, 0);
				actor2.GradientAngle = 90f;
				actor2.GradientTargetColor = Color.FromArgb(255, 233, 35, 45);
				actor2.BorderColor = Color.FromArgb(255, 153, 0, 0);
			}
		}
	}

	private string GetBrowseGroupActorName(IBrowseGroup browseGroup)
	{
		return $"browse_group_actor_{browseGroup.Name}";
	}

	public void RenderGroups()
	{
		foreach (IBrowseGroup browseGroup in BrowseGroups)
		{
			int num = BrowseGroups.IndexOf(browseGroup);
			string browseGroupActorName = GetBrowseGroupActorName(browseGroup);
			ActorFunctions.RemoveActor(browseGroupActorName);
			ActorFunctions.CreateActor(browseGroupActorName, addToFront: false, X + num * browseGroup.Width + 7, Y + 2, browseGroup.Width, browseGroup.Height, browseGroup.Name, "font_akzidenz_bold_smaller_14", null, null, browseGroup.Enabled ? "white" : "gray", null, null, null, null, "Text", "Enabled");
			IActor actor = ActorFunctions.GetActor(browseGroupActorName);
			if (actor != null)
			{
				_browseGroupActors.Add(actor);
				actor.HorizontalAlignment = StringAlignment.Center;
				actor.VerticalAlignment = StringAlignment.Center;
				actor.Hit += BrowseGroupActorHit;
			}
			if (num < BrowseGroups.Count - 1)
			{
				string text = $"browse_group_separator_1_actor_{browseGroup.Name}";
				ActorFunctions.RemoveActor(text);
				ActorFunctions.CreateActor(text, addToFront: false, X + num * browseGroup.Width + 7 + browseGroup.Width - 2, Y + 2, 1, browseGroup.Height - 1, null, null, null, "black", null, null, null, null, null, "Background", null);
				actor = ActorFunctions.GetActor(text);
				if (actor != null)
				{
					_browseGroupActors.Add(actor);
				}
				string text2 = $"browse_group_separator_2_actor_{browseGroup.Name}";
				ActorFunctions.RemoveActor(text2);
				ActorFunctions.CreateActor(text2, addToFront: false, X + num * browseGroup.Width + 7 + browseGroup.Width - 1, Y + 2, 1, browseGroup.Height - 1, null, null, null, "black", null, null, null, null, null, "Background", null);
				actor = ActorFunctions.GetActor(text2);
				if (actor != null)
				{
					_browseGroupActors.Add(actor);
					actor.BackgroundColor = Color.FromArgb(255, 128, 128, 128);
				}
			}
		}
	}

	private void BrowseGroupActorHit(object sender, EventArgs e)
	{
		IActor actor = sender as IActor;
		if (actor == null)
		{
			return;
		}
		IBrowseGroup selectedBrowseGroup = BrowseGroups.FirstOrDefault((IBrowseGroup x) => x.Name == actor.Text);
		if (selectedBrowseGroup == null)
		{
			return;
		}
		if (selectedBrowseGroup.Enabled)
		{
			BrowseGroups.ForEach(delegate(IBrowseGroup x)
			{
				x.IsSelected = x == selectedBrowseGroup;
			});
			RenderSelectedGroup();
		}
		OnGroupSelected(selectedBrowseGroup);
	}

	public void RenderBackground()
	{
		ActorFunctions.RemoveActor("browse_group_selection_background_shadow");
		ActorFunctions.CreateActor("browse_group_selection_background_shadow", addToFront: false, X + 2, Y + 3, BrowseGroups.Sum((IBrowseGroup x) => x.Width) + 14, BrowseGroups.Max((IBrowseGroup x) => x.Height) + 2, null, null, null, null, null, null, null, null, null, "Background", null);
		_backgroundShadowActor = ActorFunctions.GetActor("browse_group_selection_background_shadow");
		if (_backgroundShadowActor != null)
		{
			_backgroundShadowActor.BackgroundColor = Color.Black;
			_backgroundShadowActor.CornerSize = new Size(10, 10);
			_backgroundShadowActor.Opacity = 0.25f;
		}
		ActorFunctions.RemoveActor("browse_group_selection_background_white");
		ActorFunctions.CreateActor("browse_group_selection_background_white", addToFront: false, X, Y, BrowseGroups.Sum((IBrowseGroup x) => x.Width) + 14, BrowseGroups.Max((IBrowseGroup x) => x.Height) + 2, null, null, null, null, null, null, null, null, null, "BackgroundGradient, DrawBorder, Text", null);
		_backgroundWhiteActor = ActorFunctions.GetActor("browse_group_selection_background_white");
		if (_backgroundWhiteActor != null)
		{
			_backgroundWhiteActor.GradientAngle = 90f;
			_backgroundWhiteActor.GradientTargetColor = Color.FromArgb(255, 70, 80, 70);
			_backgroundWhiteActor.BorderColor = Color.White;
			_backgroundWhiteActor.BackgroundColor = Color.FromArgb(255, 128, 128, 128);
			_backgroundWhiteActor.CornerSize = new Size(10, 10);
			_backgroundWhiteActor.StrokeWeight = 2f;
		}
	}
}

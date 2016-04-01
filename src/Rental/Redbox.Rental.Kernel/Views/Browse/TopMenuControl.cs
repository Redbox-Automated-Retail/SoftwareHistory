using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Kernel.Views.Browse;

public class TopMenuControl
{
	private List<MenuButton> _menuButtons = new List<MenuButton>();

	private const int _arrowWidth = 17;

	private const int _arrowHeight = 14;

	private Rectangle? _textRectangle;

	private Size _textSize;

	public string BrowseSetName { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int X { get; set; }

	public int Y { get; set; }

	public List<MenuButton> MenuButtons
	{
		get
		{
			return _menuButtons;
		}
		set
		{
			_menuButtons = value;
		}
	}

	public event MenuButtonClick OnMenuButtonClick;

	public void AddMenuButton(MenuButton menuButton)
	{
		menuButton.Name = GetButtonName(_menuButtons.Count + 1);
		_menuButtons.Add(menuButton);
		int num = Width - _menuButtons.Where((MenuButton x) => x.FixedWidth).Sum((MenuButton x) => x.Width);
		int num2 = _menuButtons.Count((MenuButton x) => !x.FixedWidth);
		int num3 = 0;
		foreach (MenuButton eachMenuButton in _menuButtons)
		{
			if (!eachMenuButton.FixedPosition)
			{
				int currentButtonIndex = _menuButtons.IndexOf(eachMenuButton);
				eachMenuButton.X = _menuButtons.Where((MenuButton x) => _menuButtons.IndexOf(x) < currentButtonIndex).Sum((MenuButton x) => x.Width);
			}
			if (eachMenuButton.FixedWidth)
			{
				continue;
			}
			num3++;
			if (num3 == num2)
			{
				eachMenuButton.Width = Width - _menuButtons.Where((MenuButton x) => x != eachMenuButton).Sum((MenuButton x) => x.Width);
			}
			else
			{
				eachMenuButton.Width = num / num2;
			}
		}
	}

	public void AddMenuButtons(List<MenuButton> menuButtons)
	{
		foreach (MenuButton item in menuButtons.Where((MenuButton x) => x != null))
		{
			AddMenuButton(item);
		}
	}

	public void ClearActors()
	{
		foreach (MenuButton menuButton in _menuButtons)
		{
			ActorFunctions.RemoveActor(menuButton.Name);
			if (menuButton is ProductFamilyMenuButton { ArrowDirection: not ArrowDirection.None })
			{
				ActorFunctions.RemoveActor(GetArrowName(_menuButtons.IndexOf(menuButton)));
			}
			if (IsTop20(menuButton))
			{
				ActorFunctions.RemoveActor(GetText1Name(_menuButtons.IndexOf(menuButton)));
				ActorFunctions.RemoveActor(GetText2Name(_menuButtons.IndexOf(menuButton)));
			}
		}
	}

	public void Render()
	{
		foreach (MenuButton menuButton in _menuButtons)
		{
			string text = (IsTop20(menuButton) ? null : menuButton.Text);
			ProductFamilyMenuButton productFamilyMenuButton = menuButton as ProductFamilyMenuButton;
			ActorFunctions.CreateActor(menuButton.Name, addToFront: false, X + menuButton.X, Y + menuButton.Y, menuButton.Width, menuButton.Height, text, menuButton.FontName, null, null, null, null, null, null, null, "BackgroundGradient, Text, DrawBorder", "Enabled");
			IActor actor = ActorFunctions.GetActor(menuButton.Name);
			if (actor != null)
			{
				actor.Tag = menuButton;
				actor.Hit += MenuButtonClicked;
				actor.GradientAngle = 90f;
				actor.BorderColor = Color.FromArgb(100, 97, 98);
				actor.BackgroundColor = (menuButton.IsSelected ? Color.FromArgb(143, 10, 10) : Color.FromArgb(88, 85, 85));
				actor.GradientTargetColor = (menuButton.IsSelected ? Color.FromArgb(170, 37, 37) : Color.FromArgb(51, 48, 48));
				actor.ForegroundColor = Color.White;
				actor.VerticalAlignment = StringAlignment.Center;
				if (productFamilyMenuButton == null)
				{
					actor.HorizontalAlignment = StringAlignment.Center;
				}
				else
				{
					_textSize = TextRenderer.MeasureText(text, actor.Font);
					int num = _textSize.Width + 17 - 5;
					int x = (menuButton.Width - num) / 2;
					_textRectangle = new Rectangle(x, 0, num, menuButton.Height);
					actor.TextRegion = _textRectangle;
					if (productFamilyMenuButton.ArrowDirection == ArrowDirection.Left)
					{
						actor.HorizontalAlignment = StringAlignment.Far;
					}
					else if (productFamilyMenuButton.ArrowDirection == ArrowDirection.Right)
					{
						actor.HorizontalAlignment = StringAlignment.Near;
					}
				}
			}
			if (productFamilyMenuButton != null)
			{
				RenderArrowImage(productFamilyMenuButton);
			}
			if (IsTop20(menuButton))
			{
				RenderTop20Text(menuButton);
			}
		}
	}

	private void RenderArrowImage(ProductFamilyMenuButton productFamilyMenuButton)
	{
		ActorFunctions.CreateActor(GetArrowName(_menuButtons.IndexOf(productFamilyMenuButton)), addToFront: false, X + productFamilyMenuButton.X + ((productFamilyMenuButton.ArrowDirection == ArrowDirection.Left) ? _textRectangle.Value.X : (_textRectangle.Value.X + _textRectangle.Value.Width - 17)), Y + productFamilyMenuButton.Y + (productFamilyMenuButton.Height - 14) / 2, 17, 14, null, null, (productFamilyMenuButton.ArrowDirection == ArrowDirection.Right) ? "image_next_white_arrow" : "image_previous_white_arrow", null, null, null, null, null, null, "Image", null);
	}

	private void RenderTop20Text(MenuButton menuButton)
	{
		ActorFunctions.CreateActor(GetText1Name(_menuButtons.IndexOf(menuButton)), addToFront: false, X + menuButton.X + (menuButton.Width / 2 - 23), Y + menuButton.Y + 27, menuButton.Width, 15, "TOP", menuButton.FontName, null, null, null, null, null, null, null, "Text", null);
		ActorFunctions.CreateActor(GetText2Name(_menuButtons.IndexOf(menuButton)), addToFront: false, X + menuButton.X + (menuButton.Width / 2 + 10), Y + menuButton.Y + 25, menuButton.Width, 15, "20", "font_akzidenz_bold_11", null, null, null, null, null, null, null, "Text", null);
	}

	private void MenuButtonClicked(object sender, EventArgs e)
	{
		if (OnMenuButtonClick != null && sender is IActor actor)
		{
			MenuButton menuButton = actor.Tag as MenuButton;
			OnMenuButtonClick(menuButton);
		}
	}

	private bool IsTop20(MenuButton menuButton)
	{
		return menuButton.Text == "TOP 20";
	}

	private string GetButtonName(int index)
	{
		return GetActorName("{0}tab_{1}", index);
	}

	private string GetArrowName(int index)
	{
		return GetActorName("{0}arrow_{1}", index);
	}

	private string GetText1Name(int index)
	{
		return GetActorName("{0}text1_{1}", index);
	}

	private string GetText2Name(int index)
	{
		return GetActorName("{0}text2_{1}", index);
	}

	private string GetActorName(string format, int index)
	{
		return string.Format(format, string.IsNullOrEmpty(BrowseSetName) ? null : $"{BrowseSetName}_", index);
	}
}

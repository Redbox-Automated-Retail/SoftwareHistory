using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "JoinPerksPromoView")]
public partial class JoinPerksPromoViewControl : ViewUserControl, IComponentConnector
{
	private SimpleModel Model { get; set; }

	public JoinPerksPromoViewControl()
	{
		InitializeComponent();
		ApplyTheme<Control>(MainControl);
		base.SpeechViewName = "join_perks_promo_view";
	}

	protected override void DataContextHasChanged()
	{
		if (base.DataContext != null && base.DataContext is SimpleModel)
		{
			Model = base.DataContext as SimpleModel;
		}
	}

	protected override void InitSpeechPart(ISpeechPart speechPart)
	{
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.TitleText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.MessageText
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = "Buy Joining you could,"
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.Circle1Text + ","
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.Circle2Text + ","
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.Circle3Text + ","
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.CommentText + ","
		});
		if (Model.CheckboxChecked)
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To Join press ENTER"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					ViewUserControl.InvokeRoutedCommand(Model.ContinueButtonCommand);
				}
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To not agree to the Redbox Perks terms and conditions and receive Redbox emails and special offers, press MINUS"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "MINUS",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					Model.CheckboxChecked = false;
				}
			});
		}
		else
		{
			speechPart.Texts.Add(new TextPart
			{
				Text = "To agree to the Redbox Perks terms and conditions and receive Redbox emails and special offers, press PLUS"
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "PLUS",
				Function = "TTSRepeatSpeechPart",
				Action = delegate
				{
					Model.CheckboxChecked = true;
				}
			});
		}
		speechPart.Texts.Add(new TextPart
		{
			Text = "To exit with out joining press CANCEL"
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = "To hear the terms press TAB"
		});
		speechPart.Texts.Add(new TextPart
		{
			Text = Model.FooterText
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "CANCEL",
			Function = "TTSRepeatSpeechPart",
			Action = delegate
			{
				ViewUserControl.InvokeRoutedCommand(Model.CancelButtonCommand);
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "TAB",
			Function = "TTSRepeatSpeechPart",
			Action = delegate
			{
				ViewUserControl.InvokeRoutedCommand(Model.OtherButtonCommand);
			}
		});
		speechPart.Loop = true;
	}
}

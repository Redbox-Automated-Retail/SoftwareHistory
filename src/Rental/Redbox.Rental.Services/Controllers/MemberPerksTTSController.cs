using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksTTSController : BaseTTSController<MemberPerksTTSController, MemberPerksLogic>
{
	private class TTSConstants
	{
		public const string PerksMenu = "PerksMenu";

		public const string PerksSnapshot = "PerksSnapshot";

		public const string PerksMember = "PerksMember";

		public const string PerksPromo = "PerksPromo";
	}

	public static void AddSpeechParts(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		BaseTTSController<MemberPerksTTSController, MemberPerksLogic>.StopIdleTimer();
		AddSpeechPartForPerks(speechControl, logic);
		AddSpeechPartPerksMember(speechControl, logic);
		AddSpeechPartPerksSnapshot(speechControl, logic);
		AddSpeechPartPerksPromo(speechControl, logic);
	}

	private static string GetRentals(TierType memberTier, string rentals)
	{
		if (memberTier != TierType.Legend)
		{
			return $"You have {rentals}";
		}
		return rentals;
	}

	private static void AddBaseEvents(ISpeechPart speechPart, MemberPerksLogic logic)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "0",
			Action = delegate
			{
				logic.CancelButtonCommand_Execute();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				logic.CancelButtonCommand_Execute();
			}
		});
		if (logic.IsSnapshot)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					logic.PerksButtonCommand_Execute();
				},
				Function = "TTSSkipToSpeechPart",
				Value = "PerksMember"
			});
		}
		if (logic.IsPerks)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "2",
				Action = delegate
				{
					logic.SnapshotButtonCommand_Execute();
				},
				Function = "TTSSkipToSpeechPart",
				Value = "PerksSnapshot"
			});
		}
		if (logic.IsPromo)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "3",
				Action = delegate
				{
					logic.PromoButtonCommand_Execute();
				},
				Function = "TTSSkipToSpeechPart",
				Value = "PerksPromo"
			});
		}
	}

	private static void AddSpeechPartForPerks(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksMenu",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Welcome to My Perks"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To exit My Perks, and go back to the previous screen, press 0 or Enter."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To go back to the previous screen, Press the ENTER key."
			});
			if (logic.IsSnapshot)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To access Perks member information, press 1."
				});
			}
			if (logic.IsPerks)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To access Perks snapshot information, press 2."
				});
			}
			if (logic.IsPromo)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To access Perks promo information, press 3."
				});
			}
			AddBaseEvents(speechPart, logic);
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartPerksMember(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		MemberPerksModel model = logic.Model;
		string[] Bullets = model.PerksBullets;
		string perksRentals = GetRentals(model.MemberTier, model.PerksRentals);
		string perksTitle = $"You are a {model.PerksTitle}";
		string perksBullet = $"As a {model.PerksTitle} you have the following perks.";
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksMember",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = perksTitle
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = perksRentals
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = perksBullet
			});
			string[] array = Bullets;
			foreach (string text in array)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = text
				});
			}
			AddBaseEvents(speechPart, logic);
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartPerksSnapshot(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		MemberPerksModel Model = logic.Model;
		string snapshotRentals = GetRentals(Model.MemberTier, Model.SnapshotNextLevel);
		string snapshotTitle = $"You are a {Model.PerksTitle}";
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksSnapshot",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Here's how your Perks are powering up."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = $"You have {Model.SnapshotPoints}."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = snapshotTitle
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = snapshotRentals
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = (string.IsNullOrWhiteSpace(Model.SnapshotExpire) ? "" : $"Your {Model.SnapshotExpire}.")
			});
			AddBaseEvents(speechPart, logic);
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartPerksPromo(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksPromo",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = ""
			});
			AddBaseEvents(speechPart, logic);
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}

using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksViewController : BaseViewController
{
	private static MemberPerksLogic GetPerksLogic()
	{
		return BaseViewController.GetViewModel<MemberPerksModel>()?.Logic;
	}

	public static void ConfigureView(PerksMemberInfo perksInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		BaseViewController.StopIdleTimer();
		BaseViewController.SetCurrentUICulture();
		MemberPerksModel memberPerksModel;
		if (perksInfo.PerksModel != null && perksInfo.PerksLogic != null)
		{
			memberPerksModel = perksInfo.PerksModel;
			MemberPerksLogic perksLogic = perksInfo.PerksLogic;
		}
		else
		{
			memberPerksModel = new MemberPerksModel
			{
				PromoButtonVisibility = Visibility.Collapsed,
				SnapshotButtonText = Resources.perks_snapshot_btn_text,
				PerksButtonText = Resources.perks_perks_btn_text,
				PromosButtonText = Resources.perks_promos_btn_text
			};
			switch (perksInfo.TierName.ToLower())
			{
			default:
				memberPerksModel.MemberTier = TierType.Member;
				break;
			case "star":
				memberPerksModel.MemberTier = TierType.Star;
				break;
			case "superstar":
				memberPerksModel.MemberTier = TierType.Superstar;
				break;
			case "legend":
				memberPerksModel.MemberTier = TierType.Legend;
				break;
			}
			MemberPerksLogic perksLogic = (memberPerksModel.Logic = new MemberPerksLogic(memberPerksModel, perksInfo));
			perksInfo.PerksModel = memberPerksModel;
			perksInfo.PerksLogic = perksLogic;
		}
		viewUserControl.DataContext = memberPerksModel;
		ConfigureTTS(memberPerksModel);
		BaseViewController.StartIdleTimer();
	}

	public static void ClosingView(PerksMemberInfo perksInfo)
	{
	}

	private static void ConfigureTTS(MemberPerksModel PerksModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			PerksModel.ProcessGetSpeechControl = () => BaseTTSController<MemberPerksTTSController, MemberPerksLogic>.GetSpeechControl(GetPerksLogic);
		}
	}
}

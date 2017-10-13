using System.Windows.Controls;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskHealth;

namespace Redbox.Rental.UI.Controls;

public class RentalUserControl : UserControl
{
	public IActor Actor { get; set; }

	public event WPFHitHandler OnWPFHit;

	public void HandleWPFHit()
	{
		OnWPFHit?.Invoke(Actor);
		PostTouchScreenHealthActivity();
	}

	protected void PostTouchScreenHealthActivity()
	{
		ServiceLocator.Instance.GetService<ITouchScreenHealth>()?.PostActivity();
	}
}

using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class VendDiscPleaseWaitUserControl : UserControl, IVendDiscAnimationControl, IComponentConnector
{
	public string TitleText
	{
		get
		{
			return TitleTextBlock.Text;
		}
		set
		{
			TitleTextBlock.Text = value;
		}
	}

	public string MessageText
	{
		get
		{
			return MessageTextBlock.Text;
		}
		set
		{
			MessageTextBlock.Text = value;
		}
	}

	public VendDiscPleaseWaitUserControl()
	{
		InitializeComponent();
	}
}

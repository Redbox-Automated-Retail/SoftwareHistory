using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Controls.Animations;

namespace Redbox.Rental.UI.Views;

public partial class VendViewUserControl : UserControl, IWPFActor, IComponentConnector
{
	private Dictionary<VendViewType, Type> _vendViewTypeAnimationControls;

	public IActor Actor { get; set; }

	public string TitleText
	{
		get
		{
			return VendAnimationControl?.TitleText;
		}
		set
		{
			IVendDiscAnimationControl vendAnimationControl = VendAnimationControl;
			if (vendAnimationControl != null)
			{
				vendAnimationControl.TitleText = value;
			}
		}
	}

	public string MessageText
	{
		get
		{
			return VendAnimationControl?.MessageText;
		}
		set
		{
			IVendDiscAnimationControl vendAnimationControl = VendAnimationControl;
			if (vendAnimationControl != null)
			{
				vendAnimationControl.MessageText = value;
			}
		}
	}

	private IVendDiscAnimationControl VendAnimationControl
	{
		get
		{
			IVendDiscAnimationControl result = null;
			if (AnimationContainer.Children.Count > 0)
			{
				result = AnimationContainer.Children[0] as IVendDiscAnimationControl;
			}
			return result;
		}
	}

	public VendViewType VendType
	{
		get
		{
			VendViewType result = VendViewType.None;
			if (AnimationContainer.Children.Count > 0)
			{
				foreach (KeyValuePair<VendViewType, Type> vendViewTypeAnimationControl in _vendViewTypeAnimationControls)
				{
					if (vendViewTypeAnimationControl.Value == AnimationContainer.Children[0].GetType())
					{
						result = vendViewTypeAnimationControl.Key;
					}
				}
			}
			return result;
		}
		set
		{
			AnimationContainer.Children.Clear();
			object obj = Activator.CreateInstance(_vendViewTypeAnimationControls[value]);
			if (obj != null)
			{
				if (obj is FrameworkElement frameworkElement)
				{
					frameworkElement.VerticalAlignment = VerticalAlignment.Center;
					AnimationContainer.Children.Add(frameworkElement);
				}
				else
				{
					LogHelper.Instance.Log("Error: {0} is not a UIElement", _vendViewTypeAnimationControls[value]);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error: Unable to instanciate instance of {0}", _vendViewTypeAnimationControls[value]);
			}
		}
	}

	public event WPFHitHandler OnWPFHit;

	public VendViewUserControl()
	{
		InitializeComponent();
		InitializeVendViewTypeAnimationControls();
		AnimationContainer.Children.Clear();
	}

	private void HandleWPFHit()
	{
		if (OnWPFHit != null)
		{
			OnWPFHit(Actor);
		}
	}

	private void InitializeVendViewTypeAnimationControls()
	{
		_vendViewTypeAnimationControls = new Dictionary<VendViewType, Type>
		{
			{
				VendViewType.None,
				typeof(Grid)
			},
			{
				VendViewType.PleaseWait,
				typeof(VendDiscPleaseWaitUserControl)
			},
			{
				VendViewType.RentalsOnTheirWay,
				typeof(VendDiscAnimationUserControl)
			},
			{
				VendViewType.RecomendationOnPickupVend,
				typeof(VendDiscAnimationUserControl)
			}
		};
	}
}

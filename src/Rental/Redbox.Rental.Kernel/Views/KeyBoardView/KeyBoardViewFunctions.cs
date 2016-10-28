using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Kernel.Views.KeyBoardView;

public class KeyBoardViewFunctions
{
	[KernelFunction(Name = "kernel.keyboard_show")]
	internal static void Show(string pageName, bool isAda, bool isSpanish, string cancelText, Func<string, string> onValidate, Action onNext, Action onCancel)
	{
		KeyboardViewUI keyboardViewUI = new KeyboardViewUI(pageName, isAda, isSpanish);
		keyboardViewUI.CancelButtonText = cancelText;
		keyboardViewUI.RegisterValidate(onValidate);
		keyboardViewUI.RegisterNext(onNext);
		keyboardViewUI.RegisterCancel(onCancel);
		keyboardViewUI.DisplayEmailLayout();
	}
}

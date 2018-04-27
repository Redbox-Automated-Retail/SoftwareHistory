using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.ControllersLogic;

public class PromptSignInLogic : BaseLogic
{
	public PromptSignInModel Model { get; set; }

	public bool Instructions { get; set; }

	public Action EmailCodeAction { get; set; }

	public Action PhonePinAction { get; set; }

	public Action ContinueAction { get; set; }

	public PromptSignInLogic(PromptSignInModel model)
	{
		Model = model;
		Model.EmailCodeButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.EmailCodeButtonCommand, EmailCodeButtonCommand_Execute);
		Model.PhonePinButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.PhonePinButtonCommand, PhonePinButtonCommand_Execute);
		Model.ContinueButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.ContinueButtonCommand, ContinueButtonCommand_Execute);
	}

	public void EmailCodeButtonCommand_Execute()
	{
		EmailCodeAction?.Invoke();
	}

	public void PhonePinButtonCommand_Execute()
	{
		PhonePinAction?.Invoke();
	}

	public void ContinueButtonCommand_Execute()
	{
		ContinueAction?.Invoke();
	}
}

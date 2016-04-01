using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal interface IQlmService
{
	bool IsConfigured { get; }

	IDeck Deck { get; }

	IQlm GetInterface();
}

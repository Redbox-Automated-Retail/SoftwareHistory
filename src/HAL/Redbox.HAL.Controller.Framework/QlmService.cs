using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class QlmService : IQlmService
{
	private IQlm ConfiguredInterface;

	private readonly IQlm QlmInterface;

	private readonly IQlm EmulatedInterface;

	public bool IsConfigured { get; private set; }

	public IDeck Deck { get; private set; }

	public IQlm GetInterface()
	{
		return ConfiguredInterface;
	}

	internal void OnConfigurationChange(IDeck qlmDeck, ControllerMode mode)
	{
		Deck = qlmDeck;
		IsConfigured = null != Deck;
		if (IsConfigured)
		{
			ConfiguredInterface = ((ControllerMode.Emulated == mode) ? EmulatedInterface : QlmInterface);
		}
	}

	internal QlmService()
	{
		QlmInterface = new QlmOperation();
		EmulatedInterface = new EmulatedQlmOperation();
		ConfiguredInterface = QlmInterface;
	}
}

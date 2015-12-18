namespace Redbox.HAL.Controller.Framework;

public interface IGetObserver
{
	void OnStuck(GetResult result, Deck deck, int slot);

	bool OnEmpty(GetResult result, Deck deck, int slot);
}

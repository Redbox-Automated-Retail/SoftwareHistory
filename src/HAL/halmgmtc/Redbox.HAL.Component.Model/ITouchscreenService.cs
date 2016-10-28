namespace Redbox.HAL.Component.Model;

public interface ITouchscreenService
{
	ITouchscreenDescriptor FindTouchScreen(bool matchDriver);

	ITouchscreenDescriptor FindTouchScreen();
}

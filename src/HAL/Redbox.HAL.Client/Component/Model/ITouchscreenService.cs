namespace Redbox.HAL.Component.Model;

internal interface ITouchscreenService
{
	ITouchscreenDescriptor FindTouchScreen(bool matchDriver);

	ITouchscreenDescriptor FindTouchScreen();
}

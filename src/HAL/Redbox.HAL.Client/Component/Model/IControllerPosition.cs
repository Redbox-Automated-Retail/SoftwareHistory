namespace Redbox.HAL.Component.Model;

internal interface IControllerPosition
{
	int? XCoordinate { get; }

	int? YCoordinate { get; }

	bool ReadOk { get; }
}

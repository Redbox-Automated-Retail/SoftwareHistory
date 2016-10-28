namespace Redbox.HAL.Component.Model;

internal interface IQuadrant
{
	int Offset { get; }

	bool IsExcluded { get; }

	IRange<int> Slots { get; }
}

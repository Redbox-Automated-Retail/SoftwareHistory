namespace Redbox.HAL.Component.Model;

internal interface ISnapResult
{
	bool SnapOk { get; }

	string Path { get; }
}

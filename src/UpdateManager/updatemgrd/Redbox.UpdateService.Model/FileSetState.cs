namespace Redbox.UpdateService.Model;

internal enum FileSetState
{
	Active,
	InProgress,
	NeedsDependency,
	Error
}

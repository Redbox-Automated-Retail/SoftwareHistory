using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ControllerRevision : IControlSystemRevision
{
	public bool Success { get; internal set; }

	public IBoardVersionResponse[] Responses { get; internal set; }

	public string Revision { get; internal set; }
}

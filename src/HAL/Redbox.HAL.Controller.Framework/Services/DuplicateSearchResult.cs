using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services;

internal sealed class DuplicateSearchResult : IDuplicateSearchResult
{
	public bool IsDuplicate => null != Original;

	public ILocation Original { get; internal set; }
}

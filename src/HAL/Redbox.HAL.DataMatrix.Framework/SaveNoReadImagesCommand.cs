namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class SaveNoReadImagesCommand : RegisterCommand
{
	internal override string Name => "Save no decode images";

	protected internal override string Register => "51";

	protected internal override string Value => "100";
}

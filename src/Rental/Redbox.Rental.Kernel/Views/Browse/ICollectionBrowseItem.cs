using System.Drawing;

namespace Redbox.Rental.Kernel.Views.Browse;

public interface ICollectionBrowseItem : ILuaBasedBrowseItem, IBrowseItem
{
	Image Image { get; set; }
}

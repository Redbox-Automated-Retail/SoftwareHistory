using System.Drawing;

namespace Redbox.Rental.Kernel.Views.Browse;

public interface IMovieOrGameBrowseItem : ILuaBasedBrowseItem, IBrowseItem
{
	bool ShowAddButton { get; set; }

	bool AllowAddEvenIfSelected { get; set; }

	bool ShowIndexOfAddButton { get; set; }

	bool ShowCancelButton { get; set; }

	bool ShowFlag { get; set; }

	ProductFlag Flag { get; set; }

	string FlagText { get; set; }

	Banner Banner { get; set; }

	string BannerText { get; set; }

	string EmailOptinButtonText { get; set; }

	bool IsComingSoonOptinEnabled { get; set; }

	bool IsEmailReminderRegistered { get; set; }

	bool IsInStock { get; set; }

	Color ImageBorderColor { get; set; }

	float ImageBorderStrokeWeight { get; set; }
}

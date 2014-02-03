using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Alsing.Windows.Forms.CoreLib;

[ToolboxItem(true)]
public class RegionHandler : Component
{
	private Container components;

	private Color _TransparencyKey = Color.FromArgb(255, 0, 255);

	public Color TransparencyKey
	{
		get
		{
			return _TransparencyKey;
		}
		set
		{
			_TransparencyKey = value;
		}
	}

	public Control Control { get; set; }

	public Bitmap MaskImage { get; set; }

	public RegionHandler(IContainer container)
	{
		container.Add(this);
		InitializeComponent();
	}

	public RegionHandler()
	{
		InitializeComponent();
	}

	private void InitializeComponent()
	{
		components = new Container();
	}

	public void ApplyRegion(Control Target, Bitmap MaskImage, Color TransparencyKey)
	{
		Control = Target;
		this.MaskImage = MaskImage;
		this.TransparencyKey = TransparencyKey;
		ApplyRegion();
	}

	public void ApplyRegion()
	{
		Region region = new Region(new Rectangle(0, 0, MaskImage.Width, MaskImage.Height));
		for (int i = 0; i < MaskImage.Height; i++)
		{
			for (int j = 0; j < MaskImage.Width; j++)
			{
				if (MaskImage.GetPixel(j, i) == TransparencyKey)
				{
					region.Exclude(new Rectangle(j, i, 1, 1));
				}
			}
		}
		Control.Region = region;
		Control.BackgroundImage = MaskImage;
	}
}

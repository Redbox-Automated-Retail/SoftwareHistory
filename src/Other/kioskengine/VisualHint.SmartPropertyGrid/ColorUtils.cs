using System;
using System.Drawing;

namespace VisualHint.SmartPropertyGrid;

public class ColorUtils
{
	public static double HueToRGB(double m1, double m2, double h)
	{
		if (h < 0.0)
		{
			h++;
		}
		if (h > 1.0)
		{
			h--;
		}
		if (6.0 * h < 1.0)
		{
			return m1 + (m2 - m1) * h * 6.0;
		}
		if (2.0 * h < 1.0)
		{
			return m2;
		}
		if (3.0 * h < 2.0)
		{
			return m1 + (m2 - m1) * (2.0 / 3.0 - h) * 6.0;
		}
		return m1;
	}

	public static Color HSLtoRGB(double H, double S, double L)
	{
		double num;
		double num2;
		double num3;
		if (S == 0.0)
		{
			num = (num2 = (num3 = L));
		}
		else
		{
			double num4 = ((!(L <= 0.5)) ? (L + S - L * S) : (L * (1.0 + S)));
			double m = 2.0 * L - num4;
			num = HueToRGB(m, num4, H + 1.0 / 3.0);
			num2 = HueToRGB(m, num4, H);
			num3 = HueToRGB(m, num4, H - 1.0 / 3.0);
		}
		return Color.FromArgb((int)Math.Round(num * 255.0), (int)Math.Round(num2 * 255.0), (int)Math.Round(num3 * 255.0));
	}

	public static void RGBtoHSL(Color rgb, out double H, out double S, out double L)
	{
		double num = (double)(int)rgb.R / 255.0;
		double num2 = (double)(int)rgb.G / 255.0;
		double num3 = (double)(int)rgb.B / 255.0;
		double num4 = Math.Max(num, Math.Max(num2, num3));
		double num5 = Math.Min(num, Math.Min(num2, num3));
		L = (num4 + num5) / 2.0;
		if (num4 == num5)
		{
			S = 0.0;
			H = 0.0;
			return;
		}
		double num6 = num4 - num5;
		if (L < 0.5)
		{
			S = num6 / (num4 + num5);
		}
		else
		{
			S = num6 / (2.0 - num4 - num5);
		}
		if (num == num4)
		{
			H = (num2 - num3) / num6;
		}
		else if (num2 == num4)
		{
			H = 2.0 + (num3 - num) / num6;
		}
		else
		{
			H = 4.0 + (num - num2) / num6;
		}
		H /= 6.0;
		if (H < 0.0)
		{
			H++;
		}
	}

	internal static double GetMaximumValue(params double[] values)
	{
		double num = values[0];
		if (values.Length >= 2)
		{
			for (int i = 1; i < values.Length; i++)
			{
				num = Math.Max(num, values[i]);
			}
		}
		return num;
	}

	internal static double GetMinimumValue(params double[] values)
	{
		double num = values[0];
		if (values.Length >= 2)
		{
			for (int i = 1; i < values.Length; i++)
			{
				num = Math.Min(num, values[i]);
			}
		}
		return num;
	}

	public static void RgbToHsb(Color rgb, out double h, out double s, out double b)
	{
		double num = (double)(int)rgb.R / 255.0;
		double num2 = (double)(int)rgb.G / 255.0;
		double num3 = (double)(int)rgb.B / 255.0;
		double[] values = new double[3] { num, num2, num3 };
		double minimumValue = GetMinimumValue(values);
		double maximumValue = GetMaximumValue(values);
		double num4 = maximumValue - minimumValue;
		h = 0.0;
		s = 0.0;
		b = maximumValue * 100.0;
		if (maximumValue != 0.0 && num4 != 0.0)
		{
			if (minimumValue == 0.0)
			{
				s = 100.0;
			}
			else
			{
				s = num4 / maximumValue * 100.0;
			}
			if (Math.Abs(num - maximumValue) < double.Epsilon)
			{
				h = (num2 - num3) / num4;
			}
			else if (Math.Abs(num2 - maximumValue) < double.Epsilon)
			{
				h = 2.0 + (num3 - num) / num4;
			}
			else if (Math.Abs(num3 - maximumValue) < double.Epsilon)
			{
				h = 4.0 + (num - num2) / num4;
			}
		}
		h *= 60.0;
		if (h < 0.0)
		{
			h += 360.0;
		}
		h = (int)Math.Round(h);
		s = (int)Math.Round(s);
		b = (int)Math.Round(b);
	}

	public static Color HsbToRgb(double h, double s, double b)
	{
		double num = 0.0;
		double num2 = 0.0;
		double num3 = 0.0;
		s /= 100.0;
		b /= 100.0;
		if (s == 0.0)
		{
			num = b;
			num2 = b;
			num3 = b;
		}
		else
		{
			h /= 60.0;
			int num4 = (int)Math.Floor(h);
			double num5 = h - (double)num4;
			double num6 = b * (1.0 - s);
			double num7 = b * (1.0 - s * num5);
			double num8 = b * (1.0 - s * (1.0 - num5));
			switch (num4)
			{
			case 0:
				num = b;
				num2 = num8;
				num3 = num6;
				break;
			case 1:
				num = num7;
				num2 = b;
				num3 = num6;
				break;
			case 2:
				num = num6;
				num2 = b;
				num3 = num8;
				break;
			case 3:
				num = num6;
				num2 = num7;
				num3 = b;
				break;
			case 4:
				num = num8;
				num2 = num6;
				num3 = b;
				break;
			case 5:
				num = b;
				num2 = num6;
				num3 = num7;
				break;
			}
		}
		return Color.FromArgb((int)Math.Round(num * 255.0), (int)Math.Round(num2 * 255.0), (int)Math.Round(num3 * 255.0));
	}
}

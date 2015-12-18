using System.ComponentModel;
using System.Xml;
using Redbox.HAL.Core;

namespace Redbox.HAL.Controller.Framework;

public class MotorSpeed
{
	[DisplayName("Low Speed")]
	[Description("The low (start) speed of the motor specified in pulses per second.")]
	public int Low { get; private set; }

	[Description("The high (target) speed of the motor specified in pulses per second.")]
	[DisplayName("High Speed")]
	public int High { get; private set; }

	[Browsable(false)]
	public MotorGear Gear { get; internal set; }

	[DisplayName("Acceleration Time (in milliseconds)")]
	[Description("The amount of time in milliseconds between acceleration from the low speed to the high speed.")]
	public int AccelerationTime { get; private set; }

	public MotorSpeed(int low, int high, int accelerationTime, MotorGear gear)
	{
		Low = low;
		High = high;
		Gear = gear;
		AccelerationTime = accelerationTime;
	}

	public override string ToString()
	{
		return "(Motor Speed Properties)";
	}

	public static MotorSpeed FromXmlNode(XmlNode node)
	{
		int nodeValue = XmlNodeExtensions.GetNodeValue2(node.SelectSingleNode("Low"), 500);
		int nodeValue2 = XmlNodeExtensions.GetNodeValue2(node.SelectSingleNode("High"), 3000);
		int nodeValue3 = XmlNodeExtensions.GetNodeValue2(node.SelectSingleNode("AccelerationTime"), 300);
		return new MotorSpeed(nodeValue, nodeValue2, nodeValue3, null);
	}

	public void SaveToXml(XmlNode node)
	{
		if (node != null)
		{
			XmlNodeExtensions.SelectSingleNodeAndSetValue(node, "Low", Low);
			XmlNodeExtensions.SelectSingleNodeAndSetValue(node, "High", High);
			XmlNodeExtensions.SelectSingleNodeAndSetValue(node, "AccelerationTime", AccelerationTime);
		}
	}

	public string GetHighSpeedCommand()
	{
		return $"HSPD {(double)High * Gear.GetStepRatio()}";
	}

	public string GetLowSpeedCommand()
	{
		return $"LSPD {(double)Low * Gear.GetStepRatio()}";
	}

	public string GetAccelerationCommand()
	{
		return $"ACCEL {AccelerationTime}";
	}
}

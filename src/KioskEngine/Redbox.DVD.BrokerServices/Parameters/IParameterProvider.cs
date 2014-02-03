namespace Belikov.GenuineChannels.Parameters;

internal interface IParameterProvider
{
	object this[GenuineParameter genuineParameter] { get; set; }
}

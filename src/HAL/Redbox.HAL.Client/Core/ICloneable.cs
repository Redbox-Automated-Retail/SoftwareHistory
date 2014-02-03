namespace Redbox.HAL.Core;

internal interface ICloneable<T>
{
	T Clone(params object[] parms);
}

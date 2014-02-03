namespace Redbox.Core;

internal interface ICloneable<T>
{
	T Clone(params object[] parms);
}

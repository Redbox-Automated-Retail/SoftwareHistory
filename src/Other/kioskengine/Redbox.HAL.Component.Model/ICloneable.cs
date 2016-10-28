namespace Redbox.HAL.Component.Model;

internal interface ICloneable<T>
{
	T Clone(params object[] parms);
}

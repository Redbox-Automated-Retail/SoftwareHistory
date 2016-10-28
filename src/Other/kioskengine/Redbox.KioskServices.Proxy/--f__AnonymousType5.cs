using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[CompilerGenerated]
internal sealed class Redbox_002EKioskServices_002EProxy_002E_003C_003Ef__AnonymousType5<_003CBarcode_003Ej__TPar, _003CProductFamily_003Ej__TPar, _003CProductType_003Ej__TPar, _003CMultiDisc_003Ej__TPar, _003CPrices_003Ej__TPar>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CBarcode_003Ej__TPar _003CBarcode_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CProductFamily_003Ej__TPar _003CProductFamily_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CProductType_003Ej__TPar _003CProductType_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CMultiDisc_003Ej__TPar _003CMultiDisc_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CPrices_003Ej__TPar _003CPrices_003Ei__Field;

	public _003CBarcode_003Ej__TPar Barcode => _003CBarcode_003Ei__Field;

	public _003CProductFamily_003Ej__TPar ProductFamily => _003CProductFamily_003Ei__Field;

	public _003CProductType_003Ej__TPar ProductType => _003CProductType_003Ei__Field;

	public _003CMultiDisc_003Ej__TPar MultiDisc => _003CMultiDisc_003Ei__Field;

	public _003CPrices_003Ej__TPar Prices => _003CPrices_003Ei__Field;

	[DebuggerHidden]
	public Redbox_002EKioskServices_002EProxy_002E_003C_003Ef__AnonymousType5(_003CBarcode_003Ej__TPar Barcode, _003CProductFamily_003Ej__TPar ProductFamily, _003CProductType_003Ej__TPar ProductType, _003CMultiDisc_003Ej__TPar MultiDisc, _003CPrices_003Ej__TPar Prices)
	{
		_003CBarcode_003Ei__Field = Barcode;
		_003CProductFamily_003Ei__Field = ProductFamily;
		_003CProductType_003Ei__Field = ProductType;
		_003CMultiDisc_003Ei__Field = MultiDisc;
		_003CPrices_003Ei__Field = Prices;
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		if (value is Redbox_002EKioskServices_002EProxy_002E_003C_003Ef__AnonymousType5<_003CBarcode_003Ej__TPar, _003CProductFamily_003Ej__TPar, _003CProductType_003Ej__TPar, _003CMultiDisc_003Ej__TPar, _003CPrices_003Ej__TPar> obj && EqualityComparer<_003CBarcode_003Ej__TPar>.Default.Equals(_003CBarcode_003Ei__Field, obj._003CBarcode_003Ei__Field) && EqualityComparer<_003CProductFamily_003Ej__TPar>.Default.Equals(_003CProductFamily_003Ei__Field, obj._003CProductFamily_003Ei__Field) && EqualityComparer<_003CProductType_003Ej__TPar>.Default.Equals(_003CProductType_003Ei__Field, obj._003CProductType_003Ei__Field) && EqualityComparer<_003CMultiDisc_003Ej__TPar>.Default.Equals(_003CMultiDisc_003Ei__Field, obj._003CMultiDisc_003Ei__Field))
		{
			return EqualityComparer<_003CPrices_003Ej__TPar>.Default.Equals(_003CPrices_003Ei__Field, obj._003CPrices_003Ei__Field);
		}
		return false;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		return ((((unchecked(-442632423 * -1521134295) + EqualityComparer<_003CBarcode_003Ej__TPar>.Default.GetHashCode(_003CBarcode_003Ei__Field)) * -1521134295 + EqualityComparer<_003CProductFamily_003Ej__TPar>.Default.GetHashCode(_003CProductFamily_003Ei__Field)) * -1521134295 + EqualityComparer<_003CProductType_003Ej__TPar>.Default.GetHashCode(_003CProductType_003Ei__Field)) * -1521134295 + EqualityComparer<_003CMultiDisc_003Ej__TPar>.Default.GetHashCode(_003CMultiDisc_003Ei__Field)) * -1521134295 + EqualityComparer<_003CPrices_003Ej__TPar>.Default.GetHashCode(_003CPrices_003Ei__Field);
	}

	[DebuggerHidden]
	public override string ToString()
	{
		return string.Format(null, "{{ Barcode = {0}, ProductFamily = {1}, ProductType = {2}, MultiDisc = {3}, Prices = {4} }}", _003CBarcode_003Ei__Field?.ToString(), _003CProductFamily_003Ei__Field?.ToString(), _003CProductType_003Ei__Field?.ToString(), _003CMultiDisc_003Ei__Field?.ToString(), _003CPrices_003Ei__Field?.ToString());
	}
}

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

[CompilerGenerated]
internal sealed class Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType0<_003Ce_003Ej__TPar, _003Cp_003Ej__TPar>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003Ce_003Ej__TPar _003Ce_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003Cp_003Ej__TPar _003Cp_003Ei__Field;

	public _003Ce_003Ej__TPar e => _003Ce_003Ei__Field;

	public _003Cp_003Ej__TPar p => _003Cp_003Ei__Field;

	[DebuggerHidden]
	public Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType0(_003Ce_003Ej__TPar e, _003Cp_003Ej__TPar p)
	{
		_003Ce_003Ei__Field = e;
		_003Cp_003Ei__Field = p;
	}

	[DebuggerHidden]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{ e = ");
		stringBuilder.Append(_003Ce_003Ei__Field);
		stringBuilder.Append(", p = ");
		stringBuilder.Append(_003Cp_003Ei__Field);
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		if (value is Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType0<_003Ce_003Ej__TPar, _003Cp_003Ej__TPar> obj && EqualityComparer<_003Ce_003Ej__TPar>.Default.Equals(_003Ce_003Ei__Field, obj._003Ce_003Ei__Field))
		{
			return EqualityComparer<_003Cp_003Ej__TPar>.Default.Equals(_003Cp_003Ei__Field, obj._003Cp_003Ei__Field);
		}
		return false;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		int num = 1078762429;
		num = -1521134295 * num + EqualityComparer<_003Ce_003Ej__TPar>.Default.GetHashCode(_003Ce_003Ei__Field);
		return -1521134295 * num + EqualityComparer<_003Cp_003Ej__TPar>.Default.GetHashCode(_003Cp_003Ei__Field);
	}
}

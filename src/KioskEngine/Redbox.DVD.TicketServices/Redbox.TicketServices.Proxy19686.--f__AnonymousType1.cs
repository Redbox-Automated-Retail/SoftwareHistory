using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

[CompilerGenerated]
internal sealed class Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1<_003CEvent_003Ej__TPar, _003CProduction_003Ej__TPar>
{
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CEvent_003Ej__TPar _003CEvent_003Ei__Field;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private readonly _003CProduction_003Ej__TPar _003CProduction_003Ei__Field;

	public _003CEvent_003Ej__TPar Event => _003CEvent_003Ei__Field;

	public _003CProduction_003Ej__TPar Production => _003CProduction_003Ei__Field;

	[DebuggerHidden]
	public Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1(_003CEvent_003Ej__TPar Event, _003CProduction_003Ej__TPar Production)
	{
		_003CEvent_003Ei__Field = Event;
		_003CProduction_003Ei__Field = Production;
	}

	[DebuggerHidden]
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("{ Event = ");
		stringBuilder.Append(_003CEvent_003Ei__Field);
		stringBuilder.Append(", Production = ");
		stringBuilder.Append(_003CProduction_003Ei__Field);
		stringBuilder.Append(" }");
		return stringBuilder.ToString();
	}

	[DebuggerHidden]
	public override bool Equals(object value)
	{
		if (value is Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1<_003CEvent_003Ej__TPar, _003CProduction_003Ej__TPar> obj && EqualityComparer<_003CEvent_003Ej__TPar>.Default.Equals(_003CEvent_003Ei__Field, obj._003CEvent_003Ei__Field))
		{
			return EqualityComparer<_003CProduction_003Ej__TPar>.Default.Equals(_003CProduction_003Ei__Field, obj._003CProduction_003Ei__Field);
		}
		return false;
	}

	[DebuggerHidden]
	public override int GetHashCode()
	{
		int num = 1071231629;
		num = -1521134295 * num + EqualityComparer<_003CEvent_003Ej__TPar>.Default.GetHashCode(_003CEvent_003Ei__Field);
		return -1521134295 * num + EqualityComparer<_003CProduction_003Ej__TPar>.Default.GetHashCode(_003CProduction_003Ei__Field);
	}
}

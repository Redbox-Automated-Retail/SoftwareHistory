using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Belikov.GenuineChannels;

[Serializable]
[ComVisible(true)]
public class OperationException : Exception, ICloneable
{
	public OperationErrorMessage OperationErrorMessage;

	public override string Message => OperationErrorMessage.UserFriendlyMessage;

	public OperationException(OperationErrorMessage operationErrorMessage)
		: base(operationErrorMessage.UserFriendlyMessage)
	{
		OperationErrorMessage = operationErrorMessage;
	}

	public OperationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		OperationErrorMessage = info.GetValue("oe", typeof(OperationErrorMessage)) as OperationErrorMessage;
	}

	public static Exception WrapException(Exception exception)
	{
		if (exception is ICloneable cloneable)
		{
			return cloneable.Clone() as Exception;
		}
		return new OperationException(new OperationErrorMessage(string.Empty, exception.Message));
	}

	public virtual object Clone()
	{
		return new OperationException(OperationErrorMessage);
	}

	public OperationException(object objToFetchTypeFrom, string errorSubIdentifier, string userFriendlyMessage)
		: base(userFriendlyMessage)
	{
		string text = null;
		OperationErrorMessage = new OperationErrorMessage((objToFetchTypeFrom is Type) ? (((Type)objToFetchTypeFrom).FullName + "." + errorSubIdentifier) : ((objToFetchTypeFrom == null) ? errorSubIdentifier : (objToFetchTypeFrom.GetType().FullName + "." + errorSubIdentifier)), userFriendlyMessage);
	}

	public override string ToString()
	{
		return "Genuine channels operation exception: " + Message;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("oe", OperationErrorMessage);
	}
}

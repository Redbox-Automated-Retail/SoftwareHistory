using System.Collections.Generic;
using Redbox.BrokerServices.Proxy.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskServices.KernelExtension;

internal abstract class BaseQueueMessageProcessorService : IQueueMessageProcessorService
{
	public abstract string Name { get; }

	public abstract bool IsDefault { get; }

	public abstract List<string> SupportedMessageTypes { get; }

	public abstract ProcessMessageDelegate ProcessMessage { get; }

	public BeforeMessageProcessingDelegate BeforeMessageProcessing => OnBeforeMessageProcessing;

	public MessageProcessedSuccessfullyDelegate MessageProcessedSuccessfully => OnMessageProcessedSuccessfully;

	public AfterMessageProcessedDelegate AfterMessageProcessed => null;

	private static void OnBeforeMessageProcessing(IMessage message, out object clientData)
	{
		IFraudService service = ServiceLocator.Instance.GetService<IFraudService>();
		object clientData2 = null;
		service?.HandleBeforeReturnMessageProcessing(message, out clientData2);
		clientData = clientData2;
	}

	private static void OnMessageProcessedSuccessfully(IMessage message, IDictionary<string, object> result, object clientData)
	{
		ServiceLocator.Instance.GetService<IFraudService>()?.ProcessReturnResponse(message, result, clientData);
		ServiceLocator.Instance.GetService<IReservationServices>()?.ProcessValidationResponse(message, result, clientData);
	}
}

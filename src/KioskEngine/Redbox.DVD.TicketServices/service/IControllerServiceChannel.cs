using System;
using System.CodeDom.Compiler;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace Redbox.TicketServices.Proxy.service;

[GeneratedCode("System.ServiceModel", "4.0.0.0")]
internal interface IControllerServiceChannel : IControllerService, IClientChannel, IContextChannel, IChannel, ICommunicationObject, IExtensibleObject<IContextChannel>, IDisposable
{
}

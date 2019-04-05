using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public delegate void MessageProcessedDelegate(IMessage message, IDictionary<string, object> result, object clientData);

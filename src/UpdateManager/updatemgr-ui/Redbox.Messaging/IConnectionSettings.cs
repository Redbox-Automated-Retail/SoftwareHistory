using System.Collections.Generic;

namespace Redbox.Messaging;

internal interface IConnectionSettings
{
	string Name { get; set; }

	string ConnectionString { get; set; }

	string DestinationName { get; set; }

	bool TransactionalMessages { get; set; }

	DestinationType DestinationType { get; set; }

	string UserName { get; set; }

	string Password { get; set; }

	Dictionary<string, string> Properties { get; set; }

	int MaxConnectionPoolSize { get; set; }

	int MinConnectionPoolSize { get; set; }
}

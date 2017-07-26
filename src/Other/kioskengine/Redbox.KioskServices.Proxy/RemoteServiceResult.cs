using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskServices.Proxy;

public class RemoteServiceResult : IRemoteServiceResult
{
	private readonly ErrorList m_errors = new ErrorList();

	private readonly IDictionary<string, object> m_properties = new Dictionary<string, object>();

	private readonly List<IRemoteServiceProviderInstruction> m_instructions = new List<IRemoteServiceProviderInstruction>();

	public bool Success { get; set; }

	public ErrorList Errors => m_errors;

	public TimeSpan ExecutionTime { get; internal set; }

	public IDictionary<string, object> Properties => m_properties;

	public ReadOnlyCollection<IRemoteServiceProviderInstruction> Instructions => InnerInstructions.AsReadOnly();

	internal List<IRemoteServiceProviderInstruction> InnerInstructions => m_instructions;

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(this.ToJson());
		return stringBuilder.ToString();
	}
}

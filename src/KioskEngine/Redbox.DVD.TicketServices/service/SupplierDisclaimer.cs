using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Redbox.TicketServices.Proxy.service;

[Serializable]
[DataContract(Name = "SupplierDisclaimer", Namespace = "http://www.redbox.com/ControllerServices/Tickets/2012/04")]
[GeneratedCode("System.Runtime.Serialization", "4.0.0.0")]
[DebuggerStepThrough]
internal class SupplierDisclaimer : IExtensibleDataObject, INotifyPropertyChanged
{
	[NonSerialized]
	private ExtensionDataObject extensionDataField;

	private string SupplierNoteField;

	private int SupplierNoteLogIdField;

	[Browsable(false)]
	public ExtensionDataObject ExtensionData
	{
		get
		{
			return extensionDataField;
		}
		set
		{
			extensionDataField = value;
		}
	}

	[DataMember(IsRequired = true)]
	public string SupplierNote
	{
		get
		{
			return SupplierNoteField;
		}
		set
		{
			if (!object.ReferenceEquals(SupplierNoteField, value))
			{
				SupplierNoteField = value;
				RaisePropertyChanged("SupplierNote");
			}
		}
	}

	[DataMember(IsRequired = true)]
	public int SupplierNoteLogId
	{
		get
		{
			return SupplierNoteLogIdField;
		}
		set
		{
			if (!SupplierNoteLogIdField.Equals(value))
			{
				SupplierNoteLogIdField = value;
				RaisePropertyChanged("SupplierNoteLogId");
			}
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected void RaisePropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

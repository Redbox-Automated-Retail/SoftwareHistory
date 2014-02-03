using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Web.Script.Serialization;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskServices.Proxy;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.TicketServices.Proxy.ComponentModel;
using Redbox.TicketServices.Proxy.service;

namespace Redbox.TicketServices.Proxy;

internal class TicketServiceProxy : ITicketServices
{
	internal const int DefaultTicketServicesTimeout = 30000;

	internal const string DefaultTicketServicesUrl = "http://tickets-dev.redbox.com/TicketControllerService/ControllerService.svc";

	private int m_working;

	public static TicketServiceProxy Instance => Singleton<TicketServiceProxy>.Instance;

	public int Timeout
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "TicketServicesTimeout", 30000);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "TicketServicesTimeout", value);
		}
	}

	public string Url
	{
		get
		{
			ConfigurationData value = null;
			IConfigurationService service = ServiceLocator.Instance.GetService<IConfigurationService>();
			if (service.TryGetConfigurationData("system", "general", "TicketServicesURL", out value))
			{
				return value.Value;
			}
			LogHelper.Instance.LogError("T001", "Failed to get URL.");
			return null;
		}
	}

	public bool LiveCallPlaceOrder
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallPlaceOrder", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallPlaceOrder", value);
		}
	}

	public bool ServerBusyPlaceOrder
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusyPlaceOrder", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusyPlaceOrder", value);
		}
	}

	public bool CommFailurePlaceOrder
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailurePlaceOrder", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailurePlaceOrder", value);
		}
	}

	public bool ServerTimeoutPlaceOrder
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerTimeoutPlaceOrder", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerTimeoutPlaceOrder", value);
		}
	}

	public int PlaceOrderOrderId
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "PlaceOrderOrderId", 1);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "PlaceOrderOrderId", value);
		}
	}

	public TicketsResponseCode PlaceOrderResponseCode
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "PlaceOrderResponseCode", TicketsResponseCode.Success);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "PlaceOrderResponseCode", value);
		}
	}

	public bool LiveCallGetTickets
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallGetTickets", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallGetTickets", value);
		}
	}

	public bool ServerBusyGetTickets
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusyGetTickets", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusyGetTickets", value);
		}
	}

	public bool CommFailureGetTickets
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailureGetTickets", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailureGetTickets", value);
		}
	}

	public bool ServerTimeoutGetTickets
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerTimeoutGetTickets", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerTimeoutGetTickets", value);
		}
	}

	public TicketsResponseCode GetTicketsResponseCode
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "GetTicketsResponseCode", TicketsResponseCode.Success);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "GetTicketsResponseCode", value);
		}
	}

	public List<string> SelectedTicketGroupIds
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			List<string> list = new List<string>();
			try
			{
				list = new JavaScriptSerializer().Deserialize<List<string>>(service.GetValue<string>("Remote Services\\Ticket Services", "SelectedTicketGroupIds"));
			}
			catch (Exception)
			{
			}
			if (list == null || list.Count == 0)
			{
				list = (SelectedTicketGroupIds = new List<string>());
			}
			return list;
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "SelectedTicketGroupIds", value.ToJson());
		}
	}

	public List<TicketGroup> MockTicketGroups
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			List<TicketGroup> list = new List<TicketGroup>();
			try
			{
				list = new JavaScriptSerializer().Deserialize<List<TicketGroup>>(service.GetValue<string>("Remote Services\\Ticket Services", "TicketGroups"));
			}
			catch (Exception)
			{
			}
			if (list == null || list.Count == 0)
			{
				list = (MockTicketGroups = GenerateDefaultMockTicketGroups());
			}
			return list;
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "TicketGroups", value.ToJson());
		}
	}

	public DeliveryMethod[] MockDeliveryMethods => CreateDeliveryMethods();

	public bool LiveCallGetEmailByCardID
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallGetEmailByCardID", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallGetEmailByCardID", value);
		}
	}

	public string GetEmailByCardReturnValue
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "GetEmailByCardReturnValue", "");
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "GetEmailByCardReturnValue", value);
		}
	}

	public bool ServerBusyGetEmailByCardID
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusyGetEmailByCardID", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusyGetEmailByCardID", value);
		}
	}

	public bool CommFailureGetEmailByCardID
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailureGetEmailByCardID", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailureGetEmailByCardID", value);
		}
	}

	public bool LiveCallSendNotification
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallSendNotification", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallSendNotification", value);
		}
	}

	public bool ServerBusySendNotification
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusySendNotification", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusySendNotification", value);
		}
	}

	public bool CommFailureSendNotification
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailureSendNotification", defaultValue: false);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailureSendNotification", value);
		}
	}

	public bool LiveCallUpdateTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallUpdateTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallUpdateTicketLock", value);
		}
	}

	public bool CommFailureUpdateTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailureUpdateTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailureUpdateTicketLock", value);
		}
	}

	public bool ServerBusyUpdateTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusyUpdateTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusyUpdateTicketLock", value);
		}
	}

	public bool LiveCallReleaseTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "LiveCallReleaseTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "LiveCallReleaseTicketLock", value);
		}
	}

	public bool CommFailureReleaseTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "CommFailureReleaseTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "CommFailureReleaseTicketLock", value);
		}
	}

	public bool ServerBusyReleaseTicketLock
	{
		get
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			return service.GetValue("Remote Services\\Ticket Services", "ServerBusyReleaseTicketLock", defaultValue: true);
		}
		set
		{
			IMachineSettingsStore service = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
			service.SetValue("Remote Services\\Ticket Services", "ServerBusyReleaseTicketLock", value);
		}
	}

	public void GetTickets(string storeNumber, string productionId, List<object> priceCodeQtyTable, List<object> lockGuidsToRelease, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallGetTickets)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.GetTickets " + Url);
					Redbox.TicketServices.Proxy.service.PriceCode[] array = new Redbox.TicketServices.Proxy.service.PriceCode[priceCodeQtyTable.Count];
					int num = 0;
					foreach (object item2 in priceCodeQtyTable)
					{
						if (item2 is Dictionary<string, string> dictionary)
						{
							array[num] = new Redbox.TicketServices.Proxy.service.PriceCode
							{
								PriceCodeId = int.Parse(dictionary["PriceCodeId"]),
								Quantity = int.Parse(dictionary["Quantity"])
							};
							num++;
						}
					}
					UnlockTicket[] array2 = new UnlockTicket[lockGuidsToRelease.Count];
					num = 0;
					foreach (object item3 in lockGuidsToRelease)
					{
						Dictionary<string, string> dictionary2 = (Dictionary<string, string>)item3;
						if (dictionary2 != null)
						{
							int result = 0;
							int.TryParse(dictionary2["Quantity"], out result);
							int result2 = 0;
							int.TryParse(dictionary2["TicketGroupId"], out result2);
							array2[num] = new UnlockTicket
							{
								Quantity = result,
								TicketGroupId = result2,
								TicketGroupGuid = new Guid(dictionary2["TicketGroupGuid"])
							};
							num++;
						}
					}
					ControllerServiceClient controllerClient = GetControllerClient();
					GetTicketsRequest getTicketsRequest = new GetTicketsRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						ProductionId = int.Parse(productionId),
						PriceCodes = array,
						UnlockTickets = array2
					};
					LogHelper.Instance.Log(getTicketsRequest.ToJson());
					GetTicketsResponse tickets = controllerClient.GetTickets(getTicketsRequest);
					remoteServiceResult.Success = tickets.Success;
					LogHelper.Instance.Log(tickets.ToJson());
					if (tickets.Success)
					{
						Dictionary<string, object> dictionary3 = new Dictionary<string, object>();
						SupplierDisclaimer[] supplierDisclaimers = tickets.SupplierDisclaimers;
						foreach (SupplierDisclaimer supplierDisclaimer in supplierDisclaimers)
						{
							Dictionary<string, object> value = new Dictionary<string, object>
							{
								{
									"SupplierNoteLogId",
									supplierDisclaimer.SupplierNoteLogId.ToString()
								},
								{
									"SupplierNote",
									supplierDisclaimer.SupplierNote.ToString()
								}
							};
							dictionary3.Add(supplierDisclaimer.SupplierNoteLogId.ToString(), value);
						}
						remoteServiceResult.Properties["SupplierDisclaimers"] = dictionary3;
						Dictionary<string, object> dictionary4 = new Dictionary<string, object>();
						TicketGroup[] ticketGroups = tickets.TicketGroups;
						foreach (TicketGroup ticketGroup in ticketGroups)
						{
							Dictionary<string, object> dictionary5 = new Dictionary<string, object>
							{
								{
									"TicketGroupId",
									ticketGroup.TicketGroupId.ToString()
								},
								{
									"LockGuid",
									ticketGroup.LockGuid.ToString()
								},
								{
									"LockDuration",
									ticketGroup.LockDuration.ToString()
								},
								{ "Row", ticketGroup.Row },
								{ "Section", ticketGroup.Section },
								{
									"SuggestionTypeId",
									ticketGroup.SuggestionTypeId.ToString()
								},
								{ "SuggestionTypeDisplayName", ticketGroup.SuggestionTypeDisplayName },
								{
									"DisplayOrder",
									ticketGroup.DisplayOrder.ToString()
								},
								{ "Zone", ticketGroup.Zone },
								{ "Level", ticketGroup.Level },
								{
									"SupplierNoteLogId",
									ticketGroup.SupplierNoteLogId.ToString()
								}
							};
							Dictionary<string, object> dictionary6 = new Dictionary<string, object>();
							DeliveryMethod[] deliveryMethods = ticketGroup.DeliveryMethods;
							foreach (DeliveryMethod deliveryMethod in deliveryMethods)
							{
								Dictionary<string, object> value2 = new Dictionary<string, object>
								{
									{
										"DeliveryMethodId",
										deliveryMethod.DeliveryMethodId.ToString()
									},
									{ "DeliveryMethodDisplayName", deliveryMethod.DeliveryMethodDisplayName },
									{ "DeliveryMethodDescription", deliveryMethod.DeliveryMethodDescription }
								};
								dictionary6.Add(deliveryMethod.DeliveryMethodId.ToString(), value2);
							}
							dictionary5.Add("DeliveryMethods", dictionary6);
							Dictionary<string, object> dictionary7 = new Dictionary<string, object>();
							PriceCodeResponse[] priceCodes = ticketGroup.PriceCodes;
							foreach (PriceCodeResponse priceCodeResponse in priceCodes)
							{
								Dictionary<string, object> dictionary8 = new Dictionary<string, object>();
								if (priceCodeResponse.TaxCollection != null)
								{
									TaxItem[] taxCollection = priceCodeResponse.TaxCollection;
									foreach (TaxItem taxItem in taxCollection)
									{
										Dictionary<string, object> value3 = new Dictionary<string, object>
										{
											{
												"TaxTypeId",
												taxItem.TaxTypeId.ToString()
											},
											{
												"TaxAmt",
												taxItem.TaxAmt.ToString()
											}
										};
										dictionary8.Add(taxItem.TaxTypeId.ToString(), value3);
									}
								}
								Dictionary<string, object> value4 = new Dictionary<string, object>
								{
									{
										"PriceCodeId",
										priceCodeResponse.PriceCodeId.ToString()
									},
									{
										"FeePerTicket",
										priceCodeResponse.FeePerTicket.ToString()
									},
									{
										"FeePerTicketTax",
										priceCodeResponse.FeePerTicketTax.ToString()
									},
									{
										"PricePerTicket",
										priceCodeResponse.PricePerTicket.ToString()
									},
									{
										"PricePerTicketTax",
										priceCodeResponse.PricePerTicketTax.ToString()
									},
									{
										"Quantity",
										priceCodeResponse.Quantity.ToString()
									},
									{ "OrderTaxCollection", dictionary8 }
								};
								dictionary7.Add(priceCodeResponse.PriceCodeId.ToString(), value4);
							}
							dictionary5.Add("PriceCodes", dictionary7);
							dictionary4.Add(ticketGroup.TicketGroupId.ToString(), dictionary5);
						}
						remoteServiceResult.Properties["TicketGroups"] = dictionary4;
					}
					else
					{
						object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(TicketsResponseCode?), tickets.Code);
						remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : TicketsResponseCode.Error.ToString());
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.GetTickets");
					if (ServerBusyGetTickets)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = TicketsResponseCode.Error.ToString();
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (CommFailureGetTickets)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "CommFailure";
						remoteServiceResult.Properties["is_online"] = false;
					}
					else if (ServerTimeoutGetTickets)
					{
						remoteServiceResult.Success = false;
						remoteServiceResult.Properties["response"] = "Timeout";
						remoteServiceResult.Properties["is_online"] = false;
						AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
						autoResetEvent.WaitOne(20000);
					}
					else
					{
						remoteServiceResult.Success = GetTicketsResponseCode == TicketsResponseCode.Success;
						remoteServiceResult.Properties["is_online"] = true;
						if (GetTicketsResponseCode == TicketsResponseCode.Success)
						{
							Dictionary<string, object> dictionary9 = new Dictionary<string, object>();
							List<TicketGroup> mockTicketGroups = MockTicketGroups;
							foreach (TicketGroup item4 in mockTicketGroups)
							{
								if (SelectedTicketGroupIds.Contains(item4.TicketGroupId.ToString()))
								{
									Dictionary<string, object> dictionary10 = new Dictionary<string, object>
									{
										{
											"TicketGroupId",
											item4.TicketGroupId.ToString()
										},
										{
											"LockGuid",
											item4.LockGuid.ToString()
										},
										{
											"LockDuration",
											item4.LockDuration.ToString()
										},
										{ "Row", item4.Row },
										{ "Section", item4.Section },
										{
											"SuggestionTypeId",
											item4.SuggestionTypeId.ToString()
										},
										{ "SuggestionTypeDisplayName", item4.SuggestionTypeDisplayName },
										{
											"DisplayOrder",
											item4.DisplayOrder.ToString()
										},
										{ "Zone", item4.Zone },
										{ "Level", item4.Level }
									};
									Dictionary<string, object> dictionary11 = new Dictionary<string, object>();
									DeliveryMethod[] deliveryMethods = item4.DeliveryMethods;
									foreach (DeliveryMethod deliveryMethod2 in deliveryMethods)
									{
										Dictionary<string, object> value5 = new Dictionary<string, object>
										{
											{
												"DeliveryMethodId",
												deliveryMethod2.DeliveryMethodId.ToString()
											},
											{ "DeliveryMethodDisplayName", deliveryMethod2.DeliveryMethodDisplayName },
											{ "DeliveryMethodDescription", deliveryMethod2.DeliveryMethodDescription }
										};
										dictionary11.Add(deliveryMethod2.DeliveryMethodId.ToString(), value5);
									}
									dictionary10.Add("DeliveryMethods", dictionary11);
									Dictionary<string, object> dictionary12 = new Dictionary<string, object>();
									PriceCodeResponse[] priceCodes = item4.PriceCodes;
									foreach (PriceCodeResponse priceCodeResponse2 in priceCodes)
									{
										Dictionary<string, object> dictionary13 = new Dictionary<string, object>();
										TaxItem[] taxCollection = priceCodeResponse2.TaxCollection;
										foreach (TaxItem taxItem2 in taxCollection)
										{
											Dictionary<string, object> value6 = new Dictionary<string, object>
											{
												{
													"TaxTypeId",
													taxItem2.TaxTypeId.ToString()
												},
												{
													"TaxAmt",
													taxItem2.TaxAmt.ToString()
												}
											};
											dictionary13.Add(taxItem2.TaxTypeId.ToString(), value6);
										}
										Dictionary<string, object> value7 = new Dictionary<string, object>
										{
											{
												"PriceCodeId",
												priceCodeResponse2.PriceCodeId.ToString()
											},
											{
												"FeePerTicket",
												priceCodeResponse2.FeePerTicket.ToString()
											},
											{
												"FeePerTicketTax",
												priceCodeResponse2.FeePerTicketTax.ToString()
											},
											{
												"PricePerTicket",
												priceCodeResponse2.PricePerTicket.ToString()
											},
											{
												"PricePerTicketTax",
												priceCodeResponse2.PricePerTicketTax.ToString()
											},
											{
												"Quantity",
												priceCodeResponse2.Quantity.ToString()
											},
											{ "OrderTaxCollection", dictionary13 }
										};
										dictionary12.Add(priceCodeResponse2.PriceCodeId.ToString(), value7);
									}
									dictionary10.Add("PriceCodes", dictionary12);
									dictionary9.Add(item4.TicketGroupId.ToString(), dictionary10);
								}
							}
							remoteServiceResult.Properties["TicketGroups"] = dictionary9;
						}
						else
						{
							remoteServiceResult.Properties["response"] = GetTicketsResponseCode.ToString();
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.GetTickets.", ex);
				remoteServiceResult.Success = false;
				remoteServiceResult.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("", ex.Message, ex));
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					service2.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.GetTickets."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void PlaceOrder(string storeNumber, string sessionId, Dictionary<string, string> customer, Dictionary<string, string> orderHeader, List<object> orderItems, List<object> unlockTickets, int[] promotions, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IShoppingSessionService service = ServiceLocator.Instance.GetService<IShoppingSessionService>();
			IShoppingSession session = service.GetSession(sessionId);
			IMacroService service2 = ServiceLocator.Instance.GetService<IMacroService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallPlaceOrder)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.PlaceOrder " + Url);
					Order order = new Order
					{
						Customer = new Customer
						{
							Email = customer["Email"]
						},
						OrderHeader = new OrderHeader
						{
							Subtotal = decimal.Parse(orderHeader["Subtotal"]),
							TotalFee = decimal.Parse(orderHeader["TotalFee"]),
							TotalTax = decimal.Parse(orderHeader["TotalTax"]),
							Total = decimal.Parse(orderHeader["Total"]),
							OrderId = int.Parse(orderHeader["OrderId"])
						}
					};
					ITrackDataService service3 = ServiceLocator.Instance.GetService<ITrackDataService>();
					order.Payment = new Payment
					{
						KeyId = service3.GetKeyId().ToString(),
						EncryptedCardNumber = service3.GetNumber(),
						HashedCardId = service3.GenerateCardHashId(),
						LastName = session.ShoppingCart.FirstName,
						FirstName = session.ShoppingCart.LastName,
						PostalCode = session.ShoppingCart.PostalCode,
						ExpirationYear = int.Parse(session.ShoppingCart.ExpiryYear),
						ExpirationMonth = int.Parse(session.ShoppingCart.ExpiryMonth),
						Track2 = ExtractFromSecure(session.ShoppingCart.TrackData)
					};
					OrderItem[] array = new OrderItem[orderItems.Count];
					int num = 0;
					foreach (object orderItem2 in orderItems)
					{
						Dictionary<string, object> dictionary = orderItem2 as Dictionary<string, object>;
						List<object> list = dictionary["PriceCodes"] as List<object>;
						OrderPriceCode[] array2 = new OrderPriceCode[list.Count];
						int num2 = 0;
						foreach (object item2 in list)
						{
							Dictionary<string, object> dictionary2 = item2 as Dictionary<string, object>;
							List<object> list2 = dictionary2["OrderTaxCollection"] as List<object>;
							OrderTaxItem[] array3 = new OrderTaxItem[list2.Count];
							int num3 = 0;
							foreach (object item3 in list2)
							{
								Dictionary<string, object> dictionary3 = item3 as Dictionary<string, object>;
								array3[num3] = new OrderTaxItem
								{
									TaxTypeId = int.Parse(dictionary3["TaxTypeId"].ToString()),
									TaxAmt = decimal.Parse(dictionary3["TaxAmt"].ToString())
								};
								num3++;
							}
							array2[num2] = new OrderPriceCode
							{
								FeePerTicket = decimal.Parse(dictionary2["FeePerTicket"].ToString()),
								FeePerTicketTax = decimal.Parse(dictionary2["FeePerTicketTax"].ToString()),
								PriceCodeId = int.Parse(dictionary2["PriceCodeId"].ToString()),
								PricePerTicket = decimal.Parse(dictionary2["PricePerTicket"].ToString()),
								PricePerTicketTax = decimal.Parse(dictionary2["PricePerTicketTax"].ToString()),
								Quantity = int.Parse(dictionary2["Quantity"].ToString()),
								OrderTaxCollection = array3
							};
							num2++;
						}
						OrderItem orderItem = new OrderItem
						{
							DeliveryMethodId = int.Parse(dictionary["DeliveryMethodId"].ToString()),
							DeliverToName = dictionary["DeliverToName"].ToString(),
							LockGuid = new Guid(dictionary["LockGuid"].ToString()),
							TicketGroupId = int.Parse(dictionary["TicketGroupId"].ToString()),
							SuggestionTypeId = int.Parse(dictionary["SuggestionTypeId"].ToString()),
							PriceCodes = array2
						};
						if (dictionary.ContainsKey("SupplierNoteLogId"))
						{
							orderItem.SupplierNoteLogId = int.Parse(dictionary["SupplierNoteLogId"].ToString());
						}
						array[num] = orderItem;
						num++;
					}
					order.OrderItems = array;
					UnlockTicket[] array4 = new UnlockTicket[unlockTickets.Count];
					int num4 = 0;
					foreach (object unlockTicket in unlockTickets)
					{
						Dictionary<string, object> dictionary4 = unlockTicket as Dictionary<string, object>;
						array4[num4] = new UnlockTicket
						{
							TicketGroupId = int.Parse(dictionary4["TicketGroupId"].ToString()),
							TicketGroupGuid = new Guid(dictionary4["TicketGroupGuid"].ToString()),
							Quantity = int.Parse(dictionary4["Quantity"].ToString())
						};
						num4++;
					}
					order.PromotionList = promotions;
					ControllerServiceClient controllerClient = GetControllerClient();
					PlaceOrderRequest placeOrderRequest = new PlaceOrderRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service2["EngineVersion"],
						BundleVersion = service2["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						Order = order,
						UnlockTickets = array4
					};
					string json = placeOrderRequest.ToJson();
					Dictionary<string, object> dictionary5 = json.ToObject<Dictionary<string, object>>();
					if (dictionary5.ContainsKey("Order"))
					{
						Dictionary<string, object> dictionary6 = (Dictionary<string, object>)dictionary5["Order"];
						if (dictionary6.ContainsKey("Customer"))
						{
							Dictionary<string, object> dictionary7 = (Dictionary<string, object>)dictionary6["Customer"];
							if (dictionary7.ContainsKey("Email") && dictionary7["Email"] != null)
							{
								dictionary7["Email"] = ObfuscateMessageData(dictionary7["Email"].ToString(), 'x', 2, 1);
							}
						}
						if (dictionary6.ContainsKey("Payment"))
						{
							Dictionary<string, object> dictionary8 = (Dictionary<string, object>)dictionary6["Payment"];
							dictionary8.Remove("Track2");
							dictionary8.Remove("EncryptedCardNumber");
							dictionary8.Remove("HashedCardId");
							dictionary8.Remove("ExpirationYear");
							dictionary8.Remove("ExpirationMonth");
							if (dictionary8.ContainsKey("LastName") && dictionary8["LastName"] != null)
							{
								dictionary8["LastName"] = ObfuscateMessageData(dictionary8["LastName"].ToString(), 'x', 2, 0);
							}
							if (dictionary8.ContainsKey("FirstName") && dictionary8["FirstName"] != null)
							{
								dictionary8["FirstName"] = ObfuscateMessageData(dictionary8["FirstName"].ToString(), 'x', 1, 0);
							}
							if (dictionary8.ContainsKey("PostalCode") && dictionary8["PostalCode"] != null)
							{
								dictionary8["PostalCode"] = ObfuscateMessageData(dictionary8["PostalCode"].ToString(), '#', 0, 1);
							}
						}
					}
					LogHelper.Instance.Log(dictionary5.ToJson());
					PlaceOrderResponse placeOrderResponse = controllerClient.PlaceOrder(placeOrderRequest);
					LogHelper.Instance.Log(placeOrderResponse.ToJson());
					remoteServiceResult.Success = placeOrderResponse.Success;
					remoteServiceResult.Properties["order_id"] = placeOrderResponse.OrderId.ToString();
					if (!placeOrderResponse.Success)
					{
						object fieldsForResponseCode = ResponseCodeAttribute.GetFieldsForResponseCode(typeof(TicketsResponseCode?), placeOrderResponse.Code);
						remoteServiceResult.Properties["response"] = ((fieldsForResponseCode != null) ? fieldsForResponseCode.ToString() : TicketsResponseCode.Error.ToString());
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.PlaceOrder");
					if (ServerBusyPlaceOrder)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = TicketsResponseCode.Error.ToString();
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["order_id"] = PlaceOrderOrderId.ToString();
					}
					else if (CommFailurePlaceOrder)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "CommFailure";
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["order_id"] = PlaceOrderOrderId.ToString();
					}
					else if (ServerTimeoutPlaceOrder)
					{
						remoteServiceResult.Success = false;
						remoteServiceResult.Properties["response"] = "Timeout";
						remoteServiceResult.Properties["is_online"] = false;
						remoteServiceResult.Properties["order_id"] = PlaceOrderOrderId.ToString();
						AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
						autoResetEvent.WaitOne(20000);
					}
					else
					{
						remoteServiceResult.Success = PlaceOrderResponseCode == TicketsResponseCode.Success;
						remoteServiceResult.Properties["is_online"] = true;
						remoteServiceResult.Properties["order_id"] = PlaceOrderOrderId.ToString();
						if (PlaceOrderResponseCode != TicketsResponseCode.Success)
						{
							remoteServiceResult.Properties["response"] = PlaceOrderResponseCode.ToString();
						}
					}
				}
			}
			catch (Exception ex)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.PlaceOrder.", ex);
				remoteServiceResult.Success = false;
				remoteServiceResult.Errors.Add(Redbox.KioskEngine.ComponentModel.Error.NewError("", ex.Message, ex));
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service4 = ServiceLocator.Instance.GetService<ICallbackService>();
					service4.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.PlaceOrder."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void UpdateTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallUpdateTicketLock)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.UpdateTicketLock " + Url);
					ControllerServiceClient controllerClient = GetControllerClient();
					UpdateTicketLockRequest updateTicketLockRequest = new UpdateTicketLockRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						TicketGroupId = ticketGroupId,
						LockGuid = new Guid(lockGuid),
						Quantity = quantity
					};
					LogHelper.Instance.Log(updateTicketLockRequest.ToJson());
					UpdateTicketLockResponse updateTicketLockResponse = controllerClient.UpdateTicketLock(updateTicketLockRequest);
					remoteServiceResult.Success = updateTicketLockResponse.Success;
					LogHelper.Instance.Log(updateTicketLockResponse.ToJson());
					if (updateTicketLockResponse.Success)
					{
						remoteServiceResult.Properties["LockDuration"] = updateTicketLockResponse.LockDuration.ToString();
					}
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.UpdateTicketLock");
					if (CommFailureUpdateTicketLock)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = "Server Busy";
					}
					else if (ServerBusyUpdateTicketLock)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "Communications Failure";
					}
					else
					{
						remoteServiceResult.Success = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.UpdateTicketLock.", e);
				remoteServiceResult.Success = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					service2.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.UpdateTicketLock."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void ReleaseTicketLock(string storeNumber, int ticketGroupId, string lockGuid, int quantity, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallReleaseTicketLock)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.ReleaseTicketLock " + Url);
					ControllerServiceClient controllerClient = GetControllerClient();
					ReleaseTicketLockRequest releaseTicketLockRequest = new ReleaseTicketLockRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						TicketGroupId = ticketGroupId,
						LockGuid = new Guid(lockGuid),
						Quantity = quantity
					};
					LogHelper.Instance.Log(releaseTicketLockRequest.ToJson());
					ReleaseTicketLockResponse releaseTicketLockResponse = controllerClient.ReleaseTicketLock(releaseTicketLockRequest);
					remoteServiceResult.Success = releaseTicketLockResponse.Success;
					LogHelper.Instance.Log(releaseTicketLockResponse.ToJson());
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.ReleaseTicketLock");
					if (CommFailureReleaseTicketLock)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = "Server Busy";
					}
					else if (ServerBusyReleaseTicketLock)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "Communications Failure";
					}
					else
					{
						remoteServiceResult.Success = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.ReleaseTicketLock.", e);
				remoteServiceResult.Success = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					service2.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.ReleaseTicketLock."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void GetEmailByCard(string storeNumber, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			ITrackDataService service2 = ServiceLocator.Instance.GetService<ITrackDataService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallGetEmailByCardID)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.GetEmailByCard " + Url);
					ControllerServiceClient controllerClient = GetControllerClient();
					GetEmailByCardRequest getEmailByCardRequest = new GetEmailByCardRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						HashedCreditCardNumber = service2.GenerateCardHashId()
					};
					string json = getEmailByCardRequest.ToJson();
					Dictionary<string, object> dictionary = json.ToObject<Dictionary<string, object>>();
					dictionary.Remove("HashedCreditCardNumber");
					string message = dictionary.ToJson();
					LogHelper.Instance.Log(message);
					GetEmailByCardResponse emailByCard = controllerClient.GetEmailByCard(getEmailByCardRequest);
					remoteServiceResult.Success = emailByCard.Success;
					if (emailByCard.Success)
					{
						remoteServiceResult.Properties["Email"] = emailByCard.Email;
					}
					string json2 = emailByCard.ToJson();
					dictionary = json2.ToObject<Dictionary<string, object>>();
					if (dictionary.ContainsKey("Email") && dictionary["Email"] != null)
					{
						dictionary["Email"] = ObfuscateMessageData(dictionary["Email"].ToString(), 'x', 1, 0);
					}
					string message2 = dictionary.ToJson();
					LogHelper.Instance.Log(message2);
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.GetEmailByCard");
					Random random = new Random();
					int num = random.Next(1, 5);
					AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
					autoResetEvent.WaitOne(num * 1000);
					if (ServerBusyGetEmailByCardID)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = "Server Busy";
					}
					else if (CommFailureGetEmailByCardID)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "Communications Failure";
					}
					else
					{
						string getEmailByCardReturnValue = GetEmailByCardReturnValue;
						remoteServiceResult.Success = true;
						remoteServiceResult.Properties["Email"] = getEmailByCardReturnValue;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.GetEmailByCard.", e);
				remoteServiceResult.Success = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service3 = ServiceLocator.Instance.GetService<ICallbackService>();
					service3.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.GetEmailByCard."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public void SendNotification(string storeNumber, List<object> subscriptionTable, string email, RemoteServiceCallback callback)
	{
		ThreadPool.QueueUserWorkItem(delegate
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			RemoteServiceResult remoteServiceResult = new RemoteServiceResult();
			remoteServiceResult.Properties["is_online"] = true;
			IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
			try
			{
				Interlocked.Increment(ref m_working);
				if (LiveCallSendNotification)
				{
					LogHelper.Instance.Log("Live call to remote ITicketServices.EmailEvent " + Url);
					Subscription[] array = new Subscription[subscriptionTable.Count];
					foreach (object item2 in subscriptionTable)
					{
						Dictionary<string, string> dictionary = item2 as Dictionary<string, string>;
						int num = 0;
						if (dictionary != null)
						{
							SubscriptionType subscriptionType = SubscriptionType.General;
							if (dictionary["SubscriptionType"] == "Event")
							{
								subscriptionType = SubscriptionType.Event;
							}
							NotificationType notificationType = NotificationType.Email;
							if (dictionary["NotificationType"] == "SMS")
							{
								notificationType = NotificationType.SMS;
							}
							array[num] = new Subscription
							{
								NotificationType = notificationType,
								SubscriptionType = subscriptionType,
								SubscriptionValue = dictionary["SubscriptionValue"].ToString()
							};
							num++;
						}
					}
					ControllerServiceClient controllerClient = GetControllerClient();
					SendNotificationRequest sendNotificationRequest = new SendNotificationRequest
					{
						MessageId = Guid.NewGuid(),
						EngineVersion = service["EngineVersion"],
						BundleVersion = service["ProductVersion"],
						ChannelType = ChannelType.Kiosk,
						KioskId = int.Parse(storeNumber),
						MessageDateTimeUtc = DateTime.Now,
						Subscriptions = array,
						CustomerEmail = email
					};
					string json = sendNotificationRequest.ToJson();
					Dictionary<string, object> dictionary2 = json.ToObject<Dictionary<string, object>>();
					if (dictionary2.ContainsKey("CustomerEmail") && dictionary2["CustomerEmail"] != null)
					{
						dictionary2["CustomerEmail"] = ObfuscateMessageData(dictionary2["CustomerEmail"].ToString(), 'x', 2, 1);
					}
					if (dictionary2.ContainsKey("CustomerSMSPhone") && dictionary2["CustomerSMSPhone"] != null)
					{
						dictionary2["CustomerSMSPhone"] = ObfuscateMessageData(dictionary2["CustomerSMSPhone"].ToString(), 'x', 2, 1);
					}
					LogHelper.Instance.Log(dictionary2.ToJson());
					SendNotificationResponse sendNotificationResponse = controllerClient.SendNotification(sendNotificationRequest);
					remoteServiceResult.Success = sendNotificationResponse.Success;
					LogHelper.Instance.Log(sendNotificationResponse.ToJson());
				}
				else
				{
					LogHelper.Instance.Log("Simulating call to ITicketServices.SendNotification");
					Random random = new Random();
					int num2 = random.Next(1, 5);
					AutoResetEvent autoResetEvent = new AutoResetEvent(initialState: false);
					autoResetEvent.WaitOne(num2 * 1000);
					if (ServerBusySendNotification)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction remoteServiceProviderInstruction = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitAbsolute
						};
						remoteServiceProviderInstruction.Parameters["wait_time"] = new TimeSpan(0, 0, 15, 0);
						remoteServiceResult.InnerInstructions.Add(remoteServiceProviderInstruction);
						remoteServiceResult.Properties["response"] = "Server Busy";
					}
					else if (CommFailureSendNotification)
					{
						remoteServiceResult.Success = false;
						RemoteServiceProviderInstruction item = new RemoteServiceProviderInstruction
						{
							Type = InstructionType.WaitForSuccessfulPing
						};
						remoteServiceResult.InnerInstructions.Add(item);
						remoteServiceResult.Properties["response"] = "Communications Failure";
					}
					else
					{
						remoteServiceResult.Success = true;
					}
				}
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("An unhandled exception was raised in TicketServicesProxy.SendNotification.", e);
				remoteServiceResult.Success = false;
			}
			finally
			{
				try
				{
					remoteServiceResult.ExecutionTime = executionTimer.Elapsed;
					ICallbackService service2 = ServiceLocator.Instance.GetService<ICallbackService>();
					service2.EnqueueCallback(new RemoteServiceCallbackEntry
					{
						Callback = callback,
						Result = remoteServiceResult,
						Message = "Invoking completeCallback from TicketServicesProxy.SendNotification."
					});
				}
				finally
				{
					Interlocked.Decrement(ref m_working);
				}
			}
		});
	}

	public bool CanSwitch()
	{
		return m_working == 0;
	}

	public void Reset()
	{
	}

	private DeliveryMethod[] CreateDeliveryMethods()
	{
		List<DeliveryMethod> list = new List<DeliveryMethod>();
		list.Add(new DeliveryMethod
		{
			DeliveryMethodId = 1,
			DeliveryMethodDisplayName = "WILL CALL",
			DeliveryMethodDescription = "I’d like to pick them up at the box office on the day of the event."
		});
		list.Add(new DeliveryMethod
		{
			DeliveryMethodId = 2,
			DeliveryMethodDisplayName = "EMAIL",
			DeliveryMethodDescription = "I’d like to get them in an email and print them at home."
		});
		return list.ToArray();
	}

	private List<TicketGroup> GenerateDefaultMockTicketGroups()
	{
		List<TicketGroup> list = new List<TicketGroup>();
		for (int i = 0; i < 2; i++)
		{
			TicketGroup ticketGroup = new TicketGroup();
			ticketGroup.TicketGroupId = i + 1;
			ticketGroup.LockDuration = 5;
			ticketGroup.LockGuid = Guid.NewGuid();
			ticketGroup.Zone = "A";
			ticketGroup.Level = (i + 1).ToString();
			ticketGroup.Row = (i + 1).ToString();
			ticketGroup.Section = (i + 1).ToString();
			ticketGroup.SuggestionTypeId = i + 1;
			ticketGroup.SuggestionTypeDisplayName = "Suggestion Type " + (i + 1);
			List<PriceCodeResponse> list2 = new List<PriceCodeResponse>();
			List<TaxItem> list3 = new List<TaxItem>();
			list3.Add(new TaxItem
			{
				TaxTypeId = 1,
				TaxAmt = 0.1m
			});
			list2.Add(new PriceCodeResponse
			{
				PriceCodeId = 0,
				PricePerTicket = 50m,
				PricePerTicketTax = 0m,
				FeePerTicket = 1m,
				FeePerTicketTax = 0.1m,
				Quantity = 2,
				TaxCollection = list3.ToArray()
			});
			ticketGroup.PriceCodes = list2.ToArray();
			List<DeliveryMethod> list4 = new List<DeliveryMethod>();
			list4.Add(MockDeliveryMethods[i]);
			ticketGroup.DeliveryMethods = list4.ToArray();
			list.Add(ticketGroup);
		}
		return list;
	}

	private TicketServiceProxy()
	{
	}

	private string ExtractFromSecure(SecureString value)
	{
		IntPtr intPtr = default(IntPtr);
		try
		{
			intPtr = Marshal.SecureStringToBSTR(value);
			return Marshal.PtrToStringBSTR(intPtr);
		}
		catch
		{
			return string.Empty;
		}
	}

	private string ObfuscateMessageData(string msgText, char newChar, int leading, int trailing)
	{
		if (leading + trailing > msgText.Length)
		{
			leading = 0;
			trailing = 0;
		}
		string text = ((leading > 0) ? msgText.Substring(0, leading) : string.Empty);
		string text2 = ((trailing > 0) ? msgText.Substring(msgText.Length - trailing, trailing) : string.Empty);
		string text3 = new string(newChar, msgText.Length - (leading + trailing));
		return text + text3 + text2;
	}

	private ControllerServiceClient GetControllerClient()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Expected O, but got Unknown
		ControllerServiceClient controllerServiceClient = new ControllerServiceClient("basicHttpEndpoint", Url);
		BasicHttpBinding val = new BasicHttpBinding();
		if (Url.IndexOf("https:") != -1)
		{
			val.Security.Mode = (BasicHttpSecurityMode)1;
		}
		else
		{
			val.Security.Mode = (BasicHttpSecurityMode)0;
		}
		controllerServiceClient.Endpoint.Binding = (Binding)(object)val;
		return controllerServiceClient;
	}
}

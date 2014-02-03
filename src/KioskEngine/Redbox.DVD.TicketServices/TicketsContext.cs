using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Redbox.Core;
using Redbox.TicketServices.Proxy.ComponentModel;

namespace Redbox.TicketServices.Proxy;

internal class TicketsContext
{
	internal class Top80
	{
		public EventProductionsView EventProductions { get; set; }

		public long soonest_production_not_expiring_in_2_hours { get; set; }

		public DateTime soonest_productionenddatetime_utc { get; set; }

		public DateTime justadded_rb_onsale_utc { get; set; }

		public DateTime latest_production_rb_offsale_utc { get; set; }

		public bool this_weekend { get; set; }

		public bool next_week { get; set; }

		public bool is_open_ended { get; set; }

		public bool is_oep_or_vb { get; set; }
	}

	internal class ProductionPriceCode
	{
		public long ProductionId { get; set; }

		public int PriceCodeId { get; set; }

		public string PriceCodeName { get; set; }

		public string PriceCodeShortNote { get; set; }

		public decimal? MinSupplierPricePerTicket { get; set; }

		public decimal? MaxSupplierPricePerTicket { get; set; }
	}

	public Dictionary<string, ITicketDataTable> TableData = new Dictionary<string, ITicketDataTable>();

	private DatabaseCache _profileDatabaseCache;

	public List<KioskProduction> KioskProductions { get; set; }

	public List<Event> Events { get; set; }

	public List<Production> Productions { get; set; }

	public List<Venue> Venues { get; set; }

	public List<ImageInfo> ImageInfos { get; set; }

	public List<PriceCodeType> PriceCodeTypes { get; set; }

	public List<PriceCode> PriceCodes { get; set; }

	public List<EventProductionsView> EventProductionsView { get; set; }

	public List<Top80> Top80Events { get; set; }

	public List<long> BrowseEvents { get; set; }

	public List<long> CarouselEvents { get; set; }

	private DatabaseCache DB => _profileDatabaseCache;

	public static TicketsContext GetTicketsContext()
	{
		TicketsContext ticketsContext = new TicketsContext();
		ticketsContext.LoadData();
		return ticketsContext;
	}

	public void RefreshViews(string browseFilter, string sort)
	{
		CleanEventProductionsView();
		Top80Events = GetTop80();
		ITicketDataTable eventsToProductions = GetEventsToProductions(Top80Events);
		TableData[eventsToProductions.TableName] = eventsToProductions;
		BrowseEvents = GetBrowseEvents(Top80Events, browseFilter, sort);
		CarouselEvents = GetCarouselEvents(Top80Events);
	}

	private void LoadData()
	{
		DateTime now = DateTime.Now;
		try
		{
			LogHelper.Instance.Log("TicketsContext.LoadData LoadData");
			ExcludedProductions.Instance.GetList();
			Dictionary<string, ITicketDataTable> dictionary = new Dictionary<string, ITicketDataTable>();
			OpenData();
			List<string> tables = DB.GetTables();
			List<string> list = new List<string>();
			list.Add("KioskProductions");
			list.Add("Productions");
			list.Add("Events");
			list.Add("Venues");
			list.Add("PriceCodes");
			list.Add("ImageInfo");
			List<string> source = list;
			long kioskId = GetKioskId();
			dictionary["KioskProductions"] = DB.GetSqlData("KioskProductions", $"SELECT * FROM KioskProductions WHERE kiosk_id = {kioskId}");
			KioskProductions = GetKioskProductions(dictionary["KioskProductions"]);
			long[] productionsList = KioskProductions.Select((KioskProduction kp) => kp.ProductionId).ToArray();
			dictionary["Productions"] = DB.GetSqlData("Productions", $"select p.* from KioskProductions kp join productions p on kp.production_id = p.production_id where kp.kiosk_id = {kioskId}");
			Productions = GetProductions(dictionary["Productions"]);
			long[] eventList = Productions.Select((Production p) => p.PrimaryEventId).Union(from p in Productions
				where p.SecondaryEventId.HasValue
				select p.SecondaryEventId.Value).ToArray();
			dictionary["Events"] = DB.GetSqlData("Events", $"select distinct e.* from KioskProductions kp join productions p on kp.production_id = p.production_id join events e on p.event_id = e.event_id where kp.kiosk_id = {kioskId}");
			ITicketDataTable sqlData = DB.GetSqlData("Events", $"select distinct e.* from KioskProductions kp join productions p on kp.production_id = p.production_id join events e on p.secondary_event_id = e.event_id where kp.kiosk_id = {kioskId}");
			dictionary["Events"].Add(sqlData);
			Events = GetEvents(dictionary["Events"]);
			dictionary["Venues"] = DB.GetSqlData("Venues", string.Format("select distinct v.* from KioskProductions kp join productions p on kp.production_id = p.production_id join venues v on p.venue_id = v.venue_id where kp.kiosk_id = {0} union select distinct v.* from KioskProductions kp join productionvenues pv on kp.production_id = pv.production_id join venues v on pv.venue_id = v.venue_id or pv.alternate_venue_id = v.venue_id where kp.kiosk_id = {0}", kioskId));
			long[] venueList = Productions.Select((Production p) => p.VenueId).ToArray();
			Venues = GetVenues(dictionary["Venues"]);
			dictionary["PriceCodeType"] = DB.GetSqlData("PriceCodeType", "select pct.* from PriceCodeType pct");
			PriceCodeTypes = GetPriceCodeTypes(dictionary["PriceCodeType"]);
			dictionary["PriceCodes"] = DB.GetSqlData("PriceCodes", $"select pc.* from KioskProductions kp join pricecodes pc on kp.production_id = pc.production_id where kp.kiosk_id = {kioskId}");
			PriceCodes = GetPriceCodes(dictionary["PriceCodes"]);
			dictionary["ProductionPriceCodes"] = GetProductionPriceCodes();
			dictionary["ImageInfo"] = DB.GetData("ImageInfo", "owner_type_id", null, "owner_id", delegate(object obj1, object obj2)
			{
				int num = (int)obj1;
				long value = (long)obj2;
				return num switch
				{
					1 => eventList.Contains(value), 
					2 => productionsList.Contains(value), 
					3 => venueList.Contains(value), 
					_ => false, 
				};
			});
			ImageInfos = GetImageInfo(dictionary["ImageInfo"]);
			string table;
			foreach (string item in tables)
			{
				table = item;
				if (!source.Any((string s) => s.Equals(table, StringComparison.CurrentCultureIgnoreCase)))
				{
					ITicketDataTable data = DB.GetData(table, null, null, null, null);
					if (data != null)
					{
						dictionary[table] = data;
					}
				}
			}
			CloseData();
			TableData = dictionary;
			EventProductionsView = GetEventProductionsView();
			TimeSpan timeSpan = DateTime.Now - now;
			LogHelper.Instance.Log("TicketsContext.LoadData complete. Milliseconds: {0}", timeSpan.TotalMilliseconds);
			RefreshViews(null, null);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketsContext.LoadData", e);
		}
	}

	private long GetKioskId()
	{
		int num = 0;
		try
		{
			string text = "ID";
			string text2 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Redbox\\REDS\\Kiosk Engine\\Store";
			object value = Registry.GetValue(text2, text, "");
			if (value == null)
			{
				LogHelper.Instance.Log("Kiosk ID is not set in the registry at {0}\\{1}", text2, text);
				return 0L;
			}
			num = Convert.ToInt32(value);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Unhandled exception occurred in TicketsContext.GetKioskId", e);
		}
		return num;
	}

	private void OpenData()
	{
		LogHelper.Instance.Log("TicketsContext.OpenData().  Opening profile - {0}", TicketsFile.DataPath);
		_profileDatabaseCache = new DatabaseCache(TicketsFile.DataPath);
		_profileDatabaseCache.Open();
	}

	private void CloseData()
	{
		_profileDatabaseCache.Dispose();
		_profileDatabaseCache = null;
		LogHelper.Instance.Log("TicketsContext.CloseData()");
	}

	private List<KioskProduction> GetKioskProductions(ITicketDataTable kioskProductions)
	{
		LogHelper.Instance.Log("TicketsContext.GetKioskProductions view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(kioskProductions.ColumnNames.ToList());
			int kiosk_id = columnDictionary["kiosk_id"];
			int production_id = columnDictionary["production_id"];
			List<KioskProduction> list = kioskProductions.DataList.Select((object[] item) => new KioskProduction
			{
				KioskId = (long)item[kiosk_id],
				ProductionId = (long)item[production_id]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetKioskProductions view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetKioskProductions", e);
			return null;
		}
	}

	private List<Event> GetEvents(ITicketDataTable events)
	{
		LogHelper.Instance.Log("TicketsContext.GetEvents view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(events.ColumnNames.ToList());
			int event_id = columnDictionary["event_id"];
			int event_name = columnDictionary["event_name"];
			int primary_category_id = columnDictionary["primary_category_id"];
			int short_note = columnDictionary["short_note"];
			int min_price = columnDictionary["min_price"];
			int max_price = columnDictionary["max_price"];
			int featured = columnDictionary["featured"];
			int sort_name = columnDictionary["sort_name"];
			List<Event> list = events.DataList.Select((object[] item) => new Event
			{
				EventId = (long)item[event_id],
				EventName = (string)item[event_name],
				PrimaryCategoryID = (int)item[primary_category_id],
				ShortNote = ((item[short_note] == null) ? string.Empty : ((string)item[short_note])),
				MinSupplierPrice = (decimal?)item[min_price],
				MaxSupplierPrice = (decimal?)item[max_price],
				Featured = ((item[featured] != null) ? ((int)item[featured]) : 0),
				SortName = (string)item[sort_name]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetEvents view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetEvents", e);
			return null;
		}
	}

	private List<Production> GetProductions(ITicketDataTable productions)
	{
		LogHelper.Instance.Log("TicketsContext.GetProductions view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(productions.ColumnNames.ToList());
			int production_id = columnDictionary["production_id"];
			int production_type_id = columnDictionary["production_type_id"];
			int event_id = columnDictionary["event_id"];
			int venue_id = columnDictionary["venue_id"];
			int rb_offsale_utc = columnDictionary["rb_offsale_utc"];
			int rb_onsale_utc = columnDictionary["rb_onsale_utc"];
			int content_rating_type_id = columnDictionary["content_rating_type_id"];
			int has_ga_groups = columnDictionary["has_ga_groups"];
			int min_price = columnDictionary["min_price"];
			int max_price = columnDictionary["max_price"];
			int ticket_limit = columnDictionary["ticket_limit"];
			int secondary_event_id = columnDictionary["secondary_event_id"];
			int short_note = columnDictionary["short_note"];
			int production_start_date_time_local = columnDictionary["production_start_date_time_local"];
			int production_end_date_time_local = columnDictionary["production_end_date_time_local"];
			int production_start_date_time_utc = columnDictionary["production_start_date_time_utc"];
			int production_end_date_time_utc = columnDictionary["production_end_date_time_utc"];
			List<Production> list = productions.DataList.Select((object[] item) => new Production
			{
				ProductionId = (long)item[production_id],
				ProductionTypeId = (int)item[production_type_id],
				PrimaryEventId = (long)item[event_id],
				VenueId = (long)item[venue_id],
				RBOnSaleDateTimeUTC = (DateTime)item[rb_onsale_utc],
				RBOffSaleDateTimeUTC = (DateTime)item[rb_offsale_utc],
				ContentRatingTypeId = (short)item[content_rating_type_id],
				HasGATicketGroups = (bool?)item[has_ga_groups],
				MinSupplierPrice = (decimal?)item[min_price],
				MaxSupplierPrice = (decimal?)item[max_price],
				TicketLimitPerOrder = (short?)item[ticket_limit],
				SecondaryEventId = (long?)item[secondary_event_id],
				ShortNote = (string)item[short_note],
				ProductionStartDateTimeLocal = (DateTime?)item[production_start_date_time_local],
				ProductionEndDateTimeLocal = (DateTime?)item[production_end_date_time_local],
				ProductionStartDateTimeUTC = (DateTime?)item[production_start_date_time_utc],
				ProductionEndDateTimeUTC = (DateTime?)item[production_end_date_time_utc]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetProductions view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetProductions", e);
			return null;
		}
	}

	private List<Venue> GetVenues(ITicketDataTable venues)
	{
		LogHelper.Instance.Log("TicketsContext.GetVenues view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(venues.ColumnNames.ToList());
			int venue_id = columnDictionary["venue_id"];
			int venue_name = columnDictionary["venue_name"];
			int address1 = columnDictionary["address1"];
			int address2 = columnDictionary["address2"];
			int city = columnDictionary["city"];
			_ = columnDictionary["state"];
			int zip = columnDictionary["zip"];
			int country = columnDictionary["country"];
			int long_note = columnDictionary["long_note"];
			int timezone_name = columnDictionary["timezone_name"];
			int phone_number = columnDictionary["phone_number"];
			int accessible = columnDictionary["accessible"];
			int accessibility_information = columnDictionary["accessibility_information"];
			int parking_information = columnDictionary["parking_information"];
			List<Venue> list = venues.DataList.Select((object[] item) => new Venue
			{
				VenueId = (long)item[venue_id],
				VenueName = (string)item[venue_name],
				Address1 = (string)item[address1],
				Address2 = (string)item[address2],
				CityName = (string)item[city],
				Region = (string)item[city],
				PostalCode = (string)item[zip],
				Country = (string)item[country],
				LongNote = ((item[long_note] == null) ? string.Empty : ((string)item[long_note])),
				TimeZoneName = (string)item[timezone_name],
				PhoneNumber = (string)item[phone_number],
				IsADAFacility = (bool)item[accessible],
				AccesibilityInformation = (string)item[accessibility_information],
				ParkingInformation = (string)item[parking_information]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetVenues view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetVenues", e);
			return null;
		}
	}

	private List<ImageInfo> GetImageInfo(ITicketDataTable imageInfo)
	{
		LogHelper.Instance.Log("TicketsContext.GetImageInfo view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(imageInfo.ColumnNames.ToList());
			int hash = columnDictionary["hash"];
			int image_url = columnDictionary["image_url"];
			int owner_type_id = columnDictionary["owner_type_id"];
			int owner_id = columnDictionary["owner_id"];
			int sub_type_id = columnDictionary["sub_type_id"];
			List<ImageInfo> list = imageInfo.DataList.Select((object[] item) => new ImageInfo
			{
				Hash = (string)item[hash],
				ImageUrl = (string)item[image_url],
				OwnerTypeId = (int)item[owner_type_id],
				OwnerId = (long)item[owner_id],
				SubTypeId = (int)item[sub_type_id]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetImageInfo view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetImageInfo", e);
			return null;
		}
	}

	private List<long> GetBrowseEvents(List<Top80> top80, string browseFilter, string sort)
	{
		LogHelper.Instance.Log("TicketsContext.GetBrowseEvents({0},{1}) view", browseFilter, sort);
		try
		{
			List<long> result = new List<long>();
			string text = "all";
			string text2 = "thisweekend";
			string text3 = "nextweek";
			string text4 = "toppicks";
			string text5 = "soonest";
			string text6 = "justadded";
			string text7 = "az";
			if (string.IsNullOrEmpty(browseFilter))
			{
				browseFilter = text;
			}
			if (string.IsNullOrEmpty(sort))
			{
				sort = text5;
			}
			bool isAll = browseFilter == text;
			bool isTopPicks = browseFilter == text4;
			bool isCategory = false;
			int category = -1;
			if (!isAll && !isTopPicks)
			{
				try
				{
					category = Convert.ToInt32(browseFilter);
					isCategory = true;
				}
				catch
				{
				}
			}
			bool sortIsThisWeekend = sort == text2;
			bool sortIsNextWeek = sort == text3;
			bool flag = sort == text5;
			bool flag2 = sort == text6;
			bool flag3 = sort == text7;
			top80 = (flag2 ? (from item in top80
				orderby item.justadded_rb_onsale_utc, item.soonest_productionenddatetime_utc, (item.EventProductions.Event.SortName ?? string.Empty).ToLower()
				select item).ToList() : ((!flag3) ? (from item in top80
				orderby item.soonest_productionenddatetime_utc, (item.EventProductions.Event.SortName ?? string.Empty).ToLower()
				select item).ToList() : (from item in top80
				orderby (item.EventProductions.Event.SortName ?? string.Empty).ToLower(), item.soonest_productionenddatetime_utc
				select item).ToList()));
			List<Top80> filteredTop80 = new List<Top80>();
			top80.ForEach(delegate(Top80 item)
			{
				if (((!sortIsThisWeekend && !sortIsNextWeek) || (sortIsThisWeekend && item.this_weekend) || (sortIsNextWeek && item.next_week)) && (isAll || (isTopPicks && item.EventProductions.Event.Featured == 1) || (isCategory && category == item.EventProductions.Event.PrimaryCategoryID)))
				{
					filteredTop80.Add(item);
				}
			});
			if (flag || sortIsThisWeekend || sortIsNextWeek || flag2)
			{
				List<Top80> oep = new List<Top80>();
				filteredTop80.RemoveAll(delegate(Top80 item)
				{
					if (item.is_oep_or_vb)
					{
						oep.Add(item);
						return true;
					}
					return false;
				});
				if (flag || sortIsThisWeekend || sortIsNextWeek)
				{
					oep = (from item in oep
						orderby item.soonest_productionenddatetime_utc, item.latest_production_rb_offsale_utc, (item.EventProductions.Event.SortName ?? string.Empty).ToLower()
						select item).ToList();
				}
				if (flag2)
				{
					oep = (from item in oep
						orderby item.justadded_rb_onsale_utc, item.soonest_productionenddatetime_utc, (item.EventProductions.Event.SortName ?? string.Empty).ToLower()
						select item).ToList();
				}
				int index = -1;
				oep.ForEach(delegate(Top80 item)
				{
					index += 4;
					if (index > filteredTop80.Count())
					{
						index = filteredTop80.Count();
					}
					filteredTop80.Insert(index, item);
				});
			}
			filteredTop80.ForEach(delegate(Top80 item)
			{
				result.Add(item.EventProductions.Event.EventId);
			});
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetBrowseEvents", e);
			return null;
		}
	}

	private List<long> GetCarouselEvents(List<Top80> top80)
	{
		LogHelper.Instance.Log("TicketsContext.GetCarouselEvents() view");
		try
		{
			List<long> result = new List<long>();
			top80 = top80.OrderBy((Top80 item) => item.soonest_productionenddatetime_utc).ToList();
			List<Top80> list = top80.Where((Top80 item) => item.EventProductions.Event.Featured == 1).Take(3).ToList();
			list.ForEach(delegate(Top80 item)
			{
				result.Add(item.EventProductions.Event.EventId);
			});
			if (result.Count == 3)
			{
				return result;
			}
			Top80 top81 = top80.FirstOrDefault((Top80 item) => item.EventProductions.Event.Featured != 1 && item.EventProductions.Event.PrimaryCategoryID == 2);
			if (top81 != null)
			{
				result.Add(top81.EventProductions.Event.EventId);
				if (result.Count == 3)
				{
					return result;
				}
			}
			Top80 top82 = top80.FirstOrDefault((Top80 item) => item.EventProductions.Event.Featured != 1 && item.EventProductions.Event.PrimaryCategoryID == 1);
			if (top82 != null)
			{
				result.Add(top82.EventProductions.Event.EventId);
				if (result.Count == 3)
				{
					return result;
				}
			}
			Top80 top83 = top80.FirstOrDefault((Top80 item) => item.EventProductions.Event.Featured != 1 && item.EventProductions.Event.PrimaryCategoryID == 3);
			if (top83 != null)
			{
				result.Add(top83.EventProductions.Event.EventId);
				if (result.Count == 3)
				{
					return result;
				}
			}
			List<Top80> list2 = top80.Where((Top80 item) => item.EventProductions.Event.Featured != 1 && item.EventProductions.Event.PrimaryCategoryID == 5).Take(3 - result.Count).ToList();
			foreach (Top80 item in list2)
			{
				result.Add(item.EventProductions.Event.EventId);
				if (result.Count == 3)
				{
					return result;
				}
			}
			Top80 top84 = top80.FirstOrDefault((Top80 item) => item.EventProductions.Event.Featured != 1 && item.EventProductions.Event.PrimaryCategoryID == 4);
			if (top84 != null)
			{
				result.Add(top84.EventProductions.Event.EventId);
				if (result.Count == 3)
				{
					return result;
				}
			}
			return result;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetCarouselEvents", e);
			return null;
		}
	}

	private ITicketDataTable GetEventsToProductions(List<Top80> top80)
	{
		TicketDataTable dataTable = new TicketDataTable();
		dataTable.Data = new Dictionary<string, object[]>();
		dataTable.DataList = new List<object[]>();
		dataTable.TableName = "EventToProductions";
		LogHelper.Instance.Log("TicketsContext.GetEventsToProductions view");
		try
		{
			dataTable.ColumnNames = new List<string>
			{
				"Key", "event_id", "production_ids", "soonest_production_not_expiring_in_2_hours", "soonest_productionenddatetime_utc", "justadded_rb_onsale_utc", "latest_production_rb_offsale_utc", "this_weekend", "next_week", "is_open_ended",
				"is_oep_or_vb"
			};
			top80.ForEach(delegate(Top80 item)
			{
				object[] array = new object[10];
				dataTable.Data[item.EventProductions.Event.EventId.ToString()] = array;
				dataTable.DataList.Add(array);
				array[0] = item.EventProductions.Event.EventId;
				array[1] = item.EventProductions.Productions.Select((Production p) => p.ProductionId).ToList();
				array[2] = item.soonest_production_not_expiring_in_2_hours;
				array[3] = item.soonest_productionenddatetime_utc;
				array[4] = item.justadded_rb_onsale_utc;
				array[5] = item.latest_production_rb_offsale_utc;
				array[6] = item.this_weekend;
				array[7] = item.next_week;
				array[8] = item.is_open_ended;
				array[9] = item.is_oep_or_vb;
			});
			LogHelper.Instance.Log("TicketsContext.GetEventsToProductions view complete.  Rows: {0}", dataTable.DataList.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetEventsToProductions", e);
			dataTable = null;
			throw;
		}
		return dataTable;
	}

	private List<EventProductionsView> GetEventProductionsView()
	{
		LogHelper.Instance.Log("TicketsContext.GetEventProductionsView building");
		try
		{
			IEnumerable<Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1<Event, Production>> source = from _003C_003Eh__TransparentIdentifier60 in Events.SelectMany((Event obj) => Productions, (Event e2, Production p) => new Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType0<Event, Production>(e2, p))
				where _003C_003Eh__TransparentIdentifier60.e.EventId == _003C_003Eh__TransparentIdentifier60.p.PrimaryEventId && !_003C_003Eh__TransparentIdentifier60.p.IsExpired()
				select new Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1<Event, Production>(_003C_003Eh__TransparentIdentifier60.e, _003C_003Eh__TransparentIdentifier60.p);
			var source2 = from ep in source
				group ep by ep.Event into g
				select new
				{
					Event = g.Key,
					Productions = g
				};
			List<EventProductionsView> list = source2.Select(g => new EventProductionsView
			{
				Event = g.Event,
				Productions = g.Productions.Select((Redbox_002ETicketServices_002EProxy19686_002E_003C_003Ef__AnonymousType1<Event, Production> p) => p.Production).ToList()
			}).ToList();
			LogHelper.Instance.Log("TicketsContext.GetEventProductionsView complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetEventProductionsView", e);
			throw;
		}
	}

	private List<PriceCodeType> GetPriceCodeTypes(ITicketDataTable priceCodeTypes)
	{
		LogHelper.Instance.Log("TicketsContext.GetPriceCodeTypes view");
		try
		{
			Dictionary<string, int> cols = GetColumnDictionary(priceCodeTypes.ColumnNames.ToList());
			List<PriceCodeType> list = priceCodeTypes.DataList.Select((object[] item) => new PriceCodeType
			{
				PriceCodeId = (long)item[cols["price_code_id"]],
				PriceCodeName = (string)item[cols["price_code_name"]]
			}).ToList();
			LogHelper.Instance.Log("TicketsContext GetPriceCodeTypes view complete.  Rows: {0}", list.Count);
			return list;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetPriceCodeTypes", e);
			return null;
		}
	}

	private List<PriceCode> GetPriceCodes(ITicketDataTable priceCodes)
	{
		LogHelper.Instance.Log("TicketsContext.GetPriceCodes view");
		try
		{
			Dictionary<string, int> columnDictionary = GetColumnDictionary(priceCodes.ColumnNames.ToList());
			int production_id = columnDictionary["production_id"];
			int price_code_id = columnDictionary["price_code_id"];
			int price_code_short_note = columnDictionary["price_code_short_note"];
			int min_supplier_price_per_ticket = columnDictionary["min_supplier_price_per_ticket"];
			int max_supplier_price_per_ticket = columnDictionary["max_supplier_price_per_ticket"];
			return priceCodes.DataList.Select((object[] item) => new PriceCode
			{
				ProductionId = (long)item[production_id],
				PriceCodeId = (int)item[price_code_id],
				PriceCodeShortNote = ((item[price_code_short_note] == null) ? string.Empty : ((string)item[price_code_short_note])),
				MinSupplierPricePerTicket = (decimal?)item[min_supplier_price_per_ticket],
				MaxSupplierPricePerTicket = (decimal?)item[max_supplier_price_per_ticket]
			}).ToList();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetPriceCodes", e);
			return null;
		}
	}

	private ITicketDataTable GetProductionPriceCodes()
	{
		TicketDataTable dataTable = new TicketDataTable
		{
			Data = new Dictionary<string, object[]>(),
			DataList = new List<object[]>(),
			TableName = "ProductionPriceCodes"
		};
		LogHelper.Instance.Log("TicketsContext.GetProductionPriceCodes view");
		try
		{
			dataTable.ColumnNames = new List<string> { "Key", "production_id", "price_codes" };
			List<ProductionPriceCode> list = (from priceCode in PriceCodes
				join priceCodeType in PriceCodeTypes on priceCode.PriceCodeId equals priceCodeType.PriceCodeId
				select new ProductionPriceCode
				{
					ProductionId = priceCode.ProductionId,
					PriceCodeId = priceCode.PriceCodeId,
					PriceCodeName = priceCodeType.PriceCodeName,
					PriceCodeShortNote = priceCode.PriceCodeShortNote,
					MinSupplierPricePerTicket = priceCode.MinSupplierPricePerTicket,
					MaxSupplierPricePerTicket = priceCode.MaxSupplierPricePerTicket
				}).ToList();
			list.ForEach(delegate(ProductionPriceCode item)
			{
				Dictionary<string, object> item2 = new Dictionary<string, object>
				{
					{ "ProductionId", item.ProductionId },
					{ "PriceCodeId", item.PriceCodeId },
					{ "PriceCodeName", item.PriceCodeName },
					{ "PriceCodeShortNote", item.PriceCodeShortNote },
					{ "MinSupplierPricePerTicket", item.MinSupplierPricePerTicket },
					{ "MaxSupplierPricePerTicket", item.MaxSupplierPricePerTicket }
				};
				object[] array = new object[2];
				if (dataTable.Data.ContainsKey(item.ProductionId.ToString()))
				{
					array = dataTable.Data[item.ProductionId.ToString()];
				}
				else
				{
					dataTable.Data[item.ProductionId.ToString()] = array;
					dataTable.DataList.Add(array);
					array[0] = item.ProductionId;
					array[1] = new List<Dictionary<string, object>>();
				}
				List<Dictionary<string, object>> list2 = (List<Dictionary<string, object>>)array[1];
				list2.Add(item2);
				array[1] = list2;
			});
			Productions.ForEach(delegate(Production item)
			{
				if (!dataTable.Data.ContainsKey(item.ProductionId.ToString()))
				{
					object[] array = new object[2];
					dataTable.Data[item.ProductionId.ToString()] = array;
					dataTable.DataList.Add(array);
					array[0] = item.ProductionId;
					array[1] = new List<Dictionary<string, object>>();
					Dictionary<string, object> item2 = new Dictionary<string, object>
					{
						{ "ProductionId", item.ProductionId },
						{
							"PriceCodeId",
							PriceCodeTypes[0].PriceCodeId
						},
						{
							"PriceCodeName",
							PriceCodeTypes[0].PriceCodeName
						},
						{
							"PriceCodeShortNote",
							string.Empty
						},
						{ "MinSupplierPricePerTicket", item.MinSupplierPrice },
						{ "MaxSupplierPricePerTicket", item.MaxSupplierPrice }
					};
					List<Dictionary<string, object>> list2 = (List<Dictionary<string, object>>)array[1];
					list2.Add(item2);
					array[1] = list2;
				}
			});
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Exception in TicketsContext.GetProductionPriceCodes", e);
			dataTable = null;
			throw;
		}
		return dataTable;
	}

	private void CleanEventProductionsView()
	{
		List<long> excludedProductions = ExcludedProductions.Instance.GetList();
		List<Production> first = (from ep in EventProductionsView
			from p in ep.Productions
			from pid in excludedProductions
			where p.ProductionId == pid
			select p).ToList();
		List<Production> second = (from ep in EventProductionsView
			from p in ep.Productions
			where p.IsExpired()
			select p).ToList();
		List<Production> excludeExpired = first.Union(second).ToList();
		if (excludeExpired.Count == 0)
		{
			return;
		}
		EventProductionsView.ForEach(delegate(EventProductionsView ep)
		{
			excludeExpired.ForEach(delegate(Production e)
			{
				if (ep.Productions.Contains(e))
				{
					ep.Productions.Remove(e);
					LogHelper.Instance.Log("TicketsContent.CleanEventProductionView.  Removing excluded production_id={0}", e.ProductionId);
				}
			});
		});
		List<EventProductionsView> list = EventProductionsView.Where((EventProductionsView ep) => ep.Productions.Count == 0).ToList();
		list.ForEach(delegate(EventProductionsView r)
		{
			if (EventProductionsView.Contains(r))
			{
				EventProductionsView.Remove(r);
				LogHelper.Instance.Log("TicketsContent.CleanEventProductionView.  Removing excluded event_id={0} that no longer has productions", r.Event.EventId);
			}
		});
	}

	private List<Top80> GetTop80()
	{
		IEnumerable<Top80> source = EventProductionsView.Select((EventProductionsView ev) => new Top80
		{
			EventProductions = ev,
			soonest_production_not_expiring_in_2_hours = ev.Productions.Aggregate(delegate(Production currmin, Production p)
			{
				if (currmin != null && !p.ProductionEndDateTimeUTC.HasValue)
				{
					return p;
				}
				DateTime value = p.ProductionEndDateTimeUTC.Value.AddHours(2.0);
				DateTime? productionEndDateTimeUTC = currmin.ProductionEndDateTimeUTC;
				return (!(value < productionEndDateTimeUTC)) ? currmin : p;
			}).ProductionId,
			soonest_productionenddatetime_utc = ev.Productions.Min((Production p) => (p.ProductionEndDateTimeUTC.HasValue ? p.ProductionEndDateTimeUTC : new DateTime?(DateTime.MaxValue)).Value),
			justadded_rb_onsale_utc = ev.Productions.Min((Production p) => p.RBOnSaleDateTimeUTC),
			latest_production_rb_offsale_utc = ev.Productions.Max((Production p) => p.RBOffSaleDateTimeUTC),
			this_weekend = ev.Productions.Any((Production p) => p.IsThisWeekend()),
			next_week = ev.Productions.Any((Production p) => p.IsNextWeek()),
			is_open_ended = ev.Productions.Any((Production p) => !p.ProductionEndDateTimeLocal.HasValue),
			is_oep_or_vb = ev.Productions.Any((Production p) => p.ProductionTypeId == 2 || p.ProductionTypeId == 3)
		});
		return (from item in source
			orderby item.is_open_ended descending, item.soonest_productionenddatetime_utc, (item.EventProductions.Event.SortName ?? string.Empty).ToLower()
			select item).Take(80).ToList();
	}

	private Dictionary<string, int> GetColumnDictionary(List<string> columns)
	{
		Dictionary<string, int> dict = new Dictionary<string, int>();
		int i = 0;
		columns.Where((string c) => c != "Key").ForEach(delegate(string c)
		{
			dict[c] = i++;
		});
		return dict;
	}
}

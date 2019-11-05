using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class FieldMaintenanceService : IFieldMaintenanceService
{
	private readonly IList<SafetyTip> _safetyTips = new List<SafetyTip>
	{
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 9, 2),
			Message = "Don't rely on cameras and warning devices when backing - PHYSICALLY LOOK AROUND!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 9, 9),
			Message = "Don't look at your phone when walking through the parking lot!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 9, 16),
			Message = "Watch for potholes and slippery surfaces when walking through the parking lot!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 9, 23),
			Message = "Follow the Rules of the Road and the 5 Keys when driving in Parking Lots!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 9, 30),
			Message = "Park Smart!  When possible, park away from other vehicles and use pull through spaces to avoid backing."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 10, 7),
			Message = "Lift with your legs, not your back!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 10, 14),
			Message = "Don't twist your body when putting a load to the side. Move your feet instead!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 10, 21),
			Message = "Don't lift more than you can handle! Get someone to help you if the load is too heavy."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 10, 28),
			Message = "Wear work gloves if edges are rough or sharp!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 11, 4),
			Message = "If you see blood or bodily fluids on or inside the kiosk, don't touch it! Put on disposable gloves and call your Supervisor."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 11, 11),
			Message = "Material Safety Data Sheets (MSDS) are available on The Screen if you need them."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 11, 18),
			Message = "If you see blood or bodily fluids on or inside the kiosk, don't touch it! Put on disposable gloves and call your Supervisor."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 11, 25),
			Message = "Material Safety Data Sheets (MSDS) are available on The Screen if you need them."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 2),
			Message = "Stay dry in cold weather! It prevents hypothermia."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 9),
			Message = "Use your doorstop at all times to prevent the kiosk door from flying open or closed!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 16),
			Message = "Push your cart if it's a full, heavy load! Pulling it will cause back and shoulder injuries."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 23),
			Message = "Speeding results in accidents and citations! And violates Redbox policy."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 30),
			Message = "Don't take the risk! Don't drive distracted!"
		}
	};

	public SafetyTip GetCurrentSafetyTip()
	{
		return (from st in _safetyTips
			where st.ReleaseDate >= DateTime.Now
			orderby st.ReleaseDate
			select st).FirstOrDefault() ?? _safetyTips.OrderBy((SafetyTip st) => st.ReleaseDate).Last();
	}
}

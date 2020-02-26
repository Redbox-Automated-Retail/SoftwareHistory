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
			ReleaseDate = new DateTime(2019, 12, 23),
			Message = "Speeding results in accidents and citations! And violates Redbox policy."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2019, 12, 30),
			Message = "Don't take the risk! Don't drive distracted!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 1, 6),
			Message = "Don't rely on cameras and warning devices when backing - PHYSICALLY LOOK AROUND!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 1, 13),
			Message = "Don't look at your phone when walking through the parking lot!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 1, 20),
			Message = "Watch for potholes and slippery surfaces when walking through the parking lot!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 1, 27),
			Message = "Follow the Rules of the Road and the 5 Keys when driving in Parking Lots!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 2, 3),
			Message = "Park Smart!  When possible, park away from other vehicles and use pull through spaces to avoid backing."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 2, 10),
			Message = "Stay dry in cold weather! It prevents hypothermia."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 2, 17),
			Message = "Use your doorstop at all times to prevent the kiosk door from flying open or closed!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 2, 24),
			Message = "Lift with your legs, not your back!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 3, 2),
			Message = "Don't twist your body when putting a load to the side. Move your feet instead!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 3, 9),
			Message = "Don't lift more than you can handle! Get someone to help you if the load is too heavy."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 3, 16),
			Message = "Push your cart if it's a full, heavy load! Pulling it will cause back and shoulder injuries."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 3, 23),
			Message = "Wear work gloves if edges are rough or sharp!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 3, 30),
			Message = "Be mindful of your surroundings at all times."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 4, 6),
			Message = "Speeding results in accidents and citations! And violates Redbox policy."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 4, 13),
			Message = "Don't take the risk! Don't drive distracted!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 4, 20),
			Message = "Follow the Rules of the Road and the 5 Keys when driving in Parking Lots!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 4, 27),
			Message = "Material Safety Data Sheets (MSDS) are available on The Screen if you need them."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 5, 4),
			Message = "Remember Redbox Best Practices regarding uncommon things found at the kiosk."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 5, 11),
			Message = "Be mindful of your surroundings at all times."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 5, 18),
			Message = "Use your doorstop at all times to prevent the kiosk door from flying open or closed!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 5, 25),
			Message = "If you encounter rodents, insects, or animals at the kiosk, contact your Supervisor immediately."
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 6, 1),
			Message = "Use your doorstop at all times to prevent the kiosk door from flying open or closed!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 6, 8),
			Message = "Don't twist your body when putting a load to the side. Move your feet instead!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 6, 15),
			Message = "Wear work gloves if edges are rough or sharp!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 6, 22),
			Message = "Lift with your legs, not your back!"
		},
		new SafetyTip
		{
			ReleaseDate = new DateTime(2020, 6, 29),
			Message = "Don't take the risk! Don't drive distracted!"
		}
	};

	public SafetyTip GetCurrentSafetyTip()
	{
		return (from st in _safetyTips
			where DateTime.Now >= st.ReleaseDate
			orderby st.ReleaseDate
			select st).LastOrDefault() ?? _safetyTips.OrderBy((SafetyTip st) => st.ReleaseDate).Last();
	}
}

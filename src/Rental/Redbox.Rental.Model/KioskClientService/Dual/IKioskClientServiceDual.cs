using System.Collections.Generic;
using System.Threading.Tasks;

namespace Redbox.Rental.Model.KioskClientService.Dual;

public interface IKioskClientServiceDual
{
	void Refresh();

	void LoadInStockInOther();

	bool IsInStockInOther(long titleId);

	void UpdateTitles();

	IRefreshResponse Refresh(long dualKioskId);

	Task<IRefreshResponse> RefreshAsync(long dualKioskId);

	IUpdateTitlesResponse UpdateTitles(long dualKioskId, IEnumerable<long> titleIds);

	Task<IUpdateTitlesResponse> UpdateTitlesAsync(long dualKioskId, IEnumerable<long> titleIds);

	IInStockInOtherResponse InStockInOther(IEnumerable<long> titleIds);

	Task<IInStockInOtherResponse> InStockInOtherAsync(IEnumerable<long> titleIds);
}

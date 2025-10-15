using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service.ControlTower
{
    public class BookingService : BaseService<BookingService>, IBookingService
    {
        public BookingService(IAppLogger<BookingService> appLogger) : base(appLogger)
        {

        }

    }
}

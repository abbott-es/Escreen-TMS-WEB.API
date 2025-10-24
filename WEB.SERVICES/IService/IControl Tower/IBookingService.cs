using LanguageExt;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService.IControl_Tower
{
    public interface IBookingService
    {
        Task<Either<ApiResponse<string>, ApiResponse<CreateBookingDto>>> CreateBookingAsync(CreateBookingDto bookingDto,
            CancellationToken ct);
        Task<Either<ApiResponse<string>, ApiResponse<AddStopDto>>> AddStopBookingAsync(AddStopDto addStopDto,
            CancellationToken ct);

        Task<Either<ApiResponse<string>, ApiResponse<string>>> CompleteBookingAsync(Guid bookingID,
            CancellationToken ct);
    }
}

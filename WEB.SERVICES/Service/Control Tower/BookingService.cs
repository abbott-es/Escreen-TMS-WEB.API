using AutoMapper;
using FluentValidation;
using LanguageExt;
using System.Net;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService.IControl_Tower;
using WEB.SERVICES.Service.Generic;
using WEB.UTILITY.Enums;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service.Control_Tower
{
    public class BookingService : BaseService<BookingService>, IBookingService
    {
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<Stop> _stopRepository;
        private readonly IValidator<BookingDto> _bookingValidator;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(IAppLogger<BookingService> appLogger, IValidator<BookingDto> bookingValidator,
            IMapper mapper, IUnitOfWork unitOfWork, IRepository<Booking> bookingRepository,
            IRepository<Stop> stopRepository) : base(appLogger)
        {
            _bookingValidator = bookingValidator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _bookingRepository = bookingRepository;
            _stopRepository = stopRepository;
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<CreateBookingDto>>> CreateBookingAsync(CreateBookingDto bookingDto, CancellationToken ct)
        {
            return await ExecuteAndEitherAsync<string, CreateBookingDto>(async ct =>
            {
                try
                {
                    var validate = await _bookingValidator.ValidateAsync(bookingDto, ct);
                    if (!validate.IsValid)
                    {
                        var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                        return Prelude.Left(ApiResponse<string>.Fail(errors.Select(x => x.ErrorMessage).ToList(), HttpStatusCode.UnprocessableEntity));
                    }
                    var booking = _mapper.Map<Booking>(bookingDto);
                    await _unitOfWork.ExecuteAsync(async c =>
                    {
                        await _bookingRepository.AddAsync(booking, c);
                    }, ct);
                    return Prelude.Right(ApiResponse<CreateBookingDto>.Ok(bookingDto, HttpStatusCode.Created, "Created Successfully"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating booking");
                    return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
                }
            }, nameof(CreateBookingAsync), ct);
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<AddStopDto>>> AddStopBookingAsync(AddStopDto addStopDto, CancellationToken ct)
        {
            return await ExecuteAndEitherAsync<string, AddStopDto>(async ct =>
            {
                try
                {
                    if (addStopDto.BookingID == Guid.Empty)
                    {
                        return Prelude.Left(ApiResponse<string>.Fail(["Booking Id is required"], HttpStatusCode.UnprocessableEntity));
                    }
                    var stopDto = _mapper.Map<Stop>(addStopDto);
                    await _unitOfWork.ExecuteAsync(async c =>
                    {
                        await _stopRepository.AddAsync(stopDto, c);
                    }, ct);
                    return Prelude.Right(ApiResponse<AddStopDto>.Ok(addStopDto, HttpStatusCode.Created, "Created Successfully"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating new stop route");
                    return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
                }
            }, nameof(AddStopBookingAsync), ct);
        }


        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> CompleteBookingAsync(Guid bookingID, CancellationToken ct)
        {
            return await ExecuteAndEitherAsync<string, string>(async ct =>
            {
                try
                {
                    if (bookingID == Guid.Empty)
                    {
                        return Prelude.Left(ApiResponse<string>.Fail(["Booking Id is required"], HttpStatusCode.UnprocessableEntity));
                    }

                    var booking = await _bookingRepository.GetByIdAsync(bookingID, ct);
                    await _unitOfWork.ExecuteAsync(async c =>
                    {
                        booking.Status = (int)BookingStatus.Completed;
                        _bookingRepository.Update(booking);
                    }, ct);
                    return Prelude.Right(ApiResponse<string>.Ok("Completed Successfully"));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error upon completing booking");
                    return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
                }
            }, nameof(CompleteBookingAsync), ct);
        }
    }
}

using AutoMapper;
using FluentValidation;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.SERVICES.IService.IGeneric;
using WEB.SERVICES.Service.Generic;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service.Control_Tower
{
    public class BookingService : BaseService<BookingService>, IBookingService
    {
        private readonly IUserContextService _userContextService;
        private readonly IValidator<BookingDto> _bookingValidator;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        public BookingService(IAppLogger<BookingService> appLogger, IUserContextService userContextService,
            IValidator<BookingDto> bookingValidator, IMapper mapper, IUnitOfWork unitOfWork) : base(appLogger)
        {
            _userContextService = userContextService;
            _bookingValidator = bookingValidator;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
    }
}

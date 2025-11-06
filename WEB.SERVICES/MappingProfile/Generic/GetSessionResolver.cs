using AutoMapper;
using WEB.SERVICES.IService.IGeneric;

namespace WEB.SERVICES.MappingProfile.Generic
{
    public class GetSessionResolver<TSource, TDestination, TDestMember> : IValueResolver<TSource, TDestination, TDestMember>
    {
        private readonly IUserContextService _userContextService;

        public GetSessionResolver(IUserContextService userContextService)
        {
            _userContextService = userContextService;
        }

        public TDestMember Resolve(TSource source, TDestination destination, TDestMember destMember, ResolutionContext context)
        {
            object value = _userContextService.UserId;

            return (TDestMember)value;
        }
    }
}

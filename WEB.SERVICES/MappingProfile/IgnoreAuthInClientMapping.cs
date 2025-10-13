using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
{
    public class IgnoreAuthInClientMapping : IMappingAction<UserDto, User>
    {
        public void Process(UserDto source, User destination, ResolutionContext context)
        {
            // Default to true if the flag is missing
            var ignore = context.Items.TryGetValue("IgnoreAuth", out var value) ? (bool)value : true;

            if (ignore)
            {
                destination.Auth = null;
            }
        }
    }
}

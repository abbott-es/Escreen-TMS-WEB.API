using AutoMapper;
using WEB.DOMAIN.Entity.Generic;
using WEB.SERVICES.DTO.Generic;

namespace WEB.SERVICES.MappingProfile.Generic
{
    public class IgnoreAuthInClientMapping : IMappingAction<UserDto, User>
    {
        public void Process(UserDto source, User destination, ResolutionContext context)
        {
            try
            {
                // Default to false if the flag is missing
                var ignore = context.Items.TryGetValue("IgnoreAuth", out var value) ? (bool)value : false;

                if (ignore)
                {
                    destination.Auth = null;
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}

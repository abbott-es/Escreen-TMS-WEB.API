using AutoMapper;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.MappingProfile
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

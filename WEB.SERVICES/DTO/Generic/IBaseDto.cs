using System.Text.Json.Serialization;

namespace WEB.SERVICES.DTO.Generic
{
    public interface IBaseDto
    {
        Guid ID { get; }
    }
}

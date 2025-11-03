using System.Text.Json.Serialization;

namespace CollabDoc.Domain.Entities
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DocumentPermission
    {
        View = 0,   // chỉ xem
        Edit = 1,   // được chỉnh sửa
    }
}

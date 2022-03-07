using System.Text.Json.Serialization;

namespace Fertilizer;

class Duenger
{
    [JsonPropertyName("name")]
    public string? Name {get; set; }
    [JsonPropertyName("kg")]
    public float? Kg {get; set; }

    public override string ToString()
    {
        return $"Name: {Name} Kg: {Kg}";
    }
}

using System.Text;
using System.Text.Json.Serialization;

namespace ZPICommunicationModels.Models;

/// <summary>
/// Jeden obszar organizacyjny obserwowany przez jedną kamerę termowizyjną.
/// </summary>
public class Sector
{
    /// <summary>
    /// Klucz podstawowy. Lepiej tego nie edytować manualnie.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// Nazwa sektora.
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Opcjonalny opis.
    /// </summary>
    public string? Description { get; set; }

    #region Foreign Key
    [JsonIgnore]
    public IList<HostDevice>? HostDevices { get; set; }
    #endregion

    public override string ToString()
    {
        var builder = new StringBuilder();
        builder.Append("( ");
        builder.Append(nameof(Id) + $": {Id} | ");
        builder.Append(nameof(Name) + $": {Name} | ");
        builder.Append(nameof(Description) + $": {Description} ");
        builder.Append(')');
        return builder.ToString();
    }
}

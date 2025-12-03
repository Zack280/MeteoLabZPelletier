namespace MeteoLabZPelletier.Models;

public class Donnees
{
    public string Date { get; set; } = string.Empty;
    public float Temperature { get; set; } = 0;
    public float Precipitations { get; set; } = 0;
    public float Humidite { get; set; } = 0;
}

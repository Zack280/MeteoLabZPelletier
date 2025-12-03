using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MeteoLabZPelletier.Models;
using MeteoLabZPelletier.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Uno.Extensions.Navigation;
using Windows.UI.ViewManagement;

namespace MeteoLabZPelletier.Presentation;

public partial class MainViewModel : ObservableObject
{

    [ObservableProperty] private string? temperatureCourante;
    [ObservableProperty] private string? dateCourante;
    [ObservableProperty] private string? inputDate;
    [ObservableProperty] private string? inputTemperature;
    [ObservableProperty] private string? inputPrecipitations;

    [ObservableProperty] private string? avgTemp = string.Empty;
    [ObservableProperty] private string? avgPrecip = string.Empty;
    [ObservableProperty] private string? avgHumid = string.Empty;
    [ObservableProperty] private string? maxTemp = string.Empty;
    [ObservableProperty] private string? maxPrecip = string.Empty;
    [ObservableProperty] private string? maxHumid = string.Empty;
    [ObservableProperty] private string? minTemp = string.Empty;
    [ObservableProperty] private string? minPrecip = string.Empty;
    [ObservableProperty] private string? minHumid = string.Empty;
    [ObservableProperty] private string? etTemp = string.Empty;
    [ObservableProperty] private string? etPrecip = string.Empty;
    [ObservableProperty] private string? etHumid = string.Empty;

    public ObservableCollection<Donnees> ListePrincipale { get; } = new();

    public MainViewModel()
    {
        RecupMeteo();
    }

    public void RecupMeteo()
    {
        try
        {
            var scraper = new MeteoScraper();
            scraper.Scraping();
            TemperatureCourante = $"Température : {scraper.temperature}";
            DateCourante = DateTime.Now.ToString("dd MMMM");
        }
        catch
        {
            TemperatureCourante = "Température : N/A";
            DateCourante = DateTime.Now.ToString("dd MMMM");
        }
    }

    public void AjouterDonnee(string date, float temperature, float precipitations, float humidite)
    {
            
        var nouvelleJourne = new Donnees
        {
            Date = date,
            Temperature = temperature,
            Precipitations = precipitations,
            Humidite = humidite
        };

        ListePrincipale.Add(nouvelleJourne);
    }
}

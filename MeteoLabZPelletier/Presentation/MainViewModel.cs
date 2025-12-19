using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Definitions.Series;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MeteoLabZPelletier.Models;
using MeteoLabZPelletier.Services;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.UI.Xaml.Media.Animation;
using SkiaSharp;
using Uno.Extensions.Navigation;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;


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

    public ISeries[] TemperatureSeries { get; set; }

    public Axis[] XAxis { get; set; } = new Axis[] { new Axis() };
    public Axis[] YAxis { get; set; } = new Axis[] { new Axis() };

    public MainViewModel()
    {
        RecupMeteo();
        InitGraph();
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

    [ObservableProperty]
    private DateTimeOffset? dateDebutPicker;

    [ObservableProperty]
    private DateTimeOffset? dateFinPicker;


    List<Donnees> dateChoisies = new List<Donnees>();

    public async Task CalculerStats()
    {
        if (!DateDebutPicker.HasValue || !DateFinPicker.HasValue)
            return;

        dateChoisies.Clear();

        DateTimeOffset dateDebut = DateDebutPicker.Value;
        DateTimeOffset dateFin = DateFinPicker.Value;

        var liste = ListePrincipale;

        for (int i = 0; i < liste.Count; i++)
        {
            DateTimeOffset currentDate = DateTime.ParseExact(liste[i].Date, "dd/MM/yyyy", null);

            if (currentDate >= dateDebut && currentDate <= dateFin)
            {
                dateChoisies.Add(liste[i]);
            }
        }

        if (dateChoisies.Count == 0)
        {
            Console.WriteLine("Liste de datechoisies vide");
            return;
        }

        float totalTemp = 0f;
        float totalPrecip = 0f;
        float totalHumid = 0f;


        for (int i = 0; i < dateChoisies.Count; i++)
        {
            totalTemp += dateChoisies[i].Temperature;
            totalPrecip += dateChoisies[i].Precipitations;
            totalHumid += dateChoisies[i].Humidite;
        }

        MaxTemp = dateChoisies.Max(x => x.Temperature).ToString();
        MinTemp = dateChoisies.Min(x => x.Temperature).ToString();
        MaxPrecip = dateChoisies.Max(x => x.Precipitations).ToString();
        MinPrecip = dateChoisies.Min(x => x.Precipitations).ToString();
        MaxHumid = dateChoisies.Max(x => x.Humidite).ToString();
        MinHumid = dateChoisies.Min(x => x.Humidite).ToString();

        AvgTemp = (totalTemp / dateChoisies.Count).ToString();
        AvgPrecip = (totalPrecip / dateChoisies.Count).ToString();
        AvgHumid = (totalHumid / dateChoisies.Count).ToString();

        float AverageTemp = (totalTemp / dateChoisies.Count);
        float AveragePrecip = (totalPrecip / dateChoisies.Count);
        float AverageHumid = (totalHumid / dateChoisies.Count);

        float variance = 0f;

        foreach (var i in dateChoisies)
        {
            float diff = i.Temperature - AverageTemp; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }

        EtTemp = ((float)Math.Sqrt(variance)).ToString();
        variance = 0f;
        foreach (var i in dateChoisies)
        {
            float diff = i.Precipitations - AveragePrecip; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }
        variance = 0f;
        EtPrecip = ((float)Math.Sqrt(variance)).ToString();
        foreach (var i in dateChoisies)
        {
            float diff = i.Humidite - AverageHumid; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }

        EtHumid = ((float)Math.Sqrt(variance)).ToString();

        InitGraph();

    }


    public void InitGraph()  //SOurce : https://platform.uno/blog/livecharts-announces-support-for-uno-platform/, l'integration est plus intuitive que celle sur cette page : https://platform.uno/docs/articles/external/uno.chefs/doc/external/LiveCharts.html qui présente l'intégration de manière trop générale
    {
        var ordered = dateChoisies.OrderBy(x => DateTime.ParseExact(x.Date, "dd/MM/yyyy", null)).ToList();
        var tempValues = ordered.Select(x => x.Temperature).ToList();
        var precipValues = ordered.Select(x => x.Precipitations).ToList();
        var dateLabels = ordered.Select(x => x.Date).ToArray();

        Console.WriteLine($"Count: {tempValues.Count}");
        foreach (var temp in tempValues)
        {
            Console.WriteLine($"Temperature: {temp}");
        }

        if (ordered == null)
        {
            Console.WriteLine("Liste de date choisies trier est nulle");
            return;
        }

        TemperatureSeries = new ISeries[]
        {
        new LineSeries<float>
        {
            Values = tempValues,
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint
            {
                Color = SKColors.Red
            },
            Stroke = new SolidColorPaint
            {
                Color = SKColors.Red,
                StrokeThickness = 2
            },

            Fill = null,

        },

        new LineSeries<float>
        {
            Values = precipValues,
            GeometrySize = 10,
            GeometryStroke = new SolidColorPaint
            {
                Color = SKColors.Blue
            },
            Stroke = new SolidColorPaint
            {
                Color = SKColors.Blue,
                StrokeThickness = 2
            },

            Fill = null,

        }
        };

        OnPropertyChanged(nameof(TemperatureSeries));


        XAxis[0].Labels = dateLabels;
        XAxis[0].ForceStepToMin = true;
        XAxis[0].LabelsRotation = 30;
        XAxis[0].MinLimit = -0.5;
        XAxis[0].MaxLimit = dateLabels.Length - 0.5;
        YAxis[0].Name = "Température (°C)";
        YAxis[0].MinLimit = 0;
        YAxis[0].MaxLimit = 50;
    }

    //Animations et Ui




}

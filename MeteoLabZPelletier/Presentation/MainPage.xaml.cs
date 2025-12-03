using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Windows.Storage.Pickers;

namespace MeteoLabZPelletier.Presentation;

public sealed partial class MainPage : Page
{
    public MainViewModel ViewModel { get; set; } = new MainViewModel();
    public MainPage()
    {
        
        this.InitializeComponent();
        this.DataContext = ViewModel;

    }

    private string DateExistante = string.Empty;
    public async Task AjouterBouton(object sender, RoutedEventArgs e)
    {
        string date = DateBox.Text;
        float temp = float.Parse(TempBox.Text);
        float precip = float.Parse(PrecipBox.Text);
        float humid = float.Parse(HumidBox.Text);

        bool dateExiste = false;

        for (int i = 0; i < ViewModel.ListePrincipale.Count; i++)
        {
            if (date == ViewModel.ListePrincipale[i].Date)
            {
                dateExiste = true;
                DateExistante = ViewModel.ListePrincipale[i].Date.ToString();
                var choix = await ReplaceDataDialog.ShowAsync();
                if(choix == ContentDialogResult.Primary)
                {
                    ViewModel.ListePrincipale.RemoveAt(i);
                    ViewModel.AjouterDonnee(date, temp, precip, humid);
                    OrganiserListe();
                }else
                {
                    return;
                }
            }
        }
        if (!dateExiste)
        {
            ViewModel.AjouterDonnee(date, temp, precip, humid);
            OrganiserListe();
        }
    }

    public void OrganiserListe()
    {
        var ListeEnOrdre = ViewModel.ListePrincipale.OrderBy(x => x.Date).ToList();

        ViewModel.ListePrincipale.Clear();

        foreach (var donnees in ListeEnOrdre)
        {
            ViewModel.ListePrincipale.Add(donnees);
        }
    }

    public async void ImporterCSV(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".csv");
        StorageFile file = await picker.PickSingleFileAsync();

        if (file != null)
        {
            await ParseCSV(file);
        }

    }

    private async Task ParseCSV(StorageFile file)
    {
        var lines = await FileIO.ReadLinesAsync(file);
        bool firstLine = true;

        foreach (var line in lines)
        {
            // Skip la premiere ligne, contenant les titres des donnees
            if (firstLine)
            {
                firstLine = false;
                continue;
            }

            if (string.IsNullOrWhiteSpace(line))
                continue;

            var values = line.Split(';');

            if (values.Length >= 4)
            {
                string date = values[0].Trim();
                float temp = float.Parse(values[1].Trim());
                float precip = float.Parse(values[2].Trim());
                float humid = float.Parse(values[3].Trim());

                // Check si la date existe deja
                bool dateExists = false;
                for (int i = 0; i < ViewModel.ListePrincipale.Count; i++)
                {
                    if (date == ViewModel.ListePrincipale[i].Date)
                    {
                        dateExists = true;
                        DateExistante = date;
                        var choix = await ReplaceDataDialog.ShowAsync();

                        if (choix == ContentDialogResult.Primary)
                        {
                            ViewModel.ListePrincipale.RemoveAt(i);
                            ViewModel.AjouterDonnee(date, temp, precip, humid);
                        }
                        break;
                    }
                }

                if (!dateExists)
                {
                    ViewModel.AjouterDonnee(date, temp, precip, humid);
                }
            }
        }

        OrganiserListe();
    }

    public async Task CalculerStats()
    {
        if (!DateDebutPicker.Date.HasValue || !DateFinPicker.Date.HasValue)
            return;

        DateTime dateDebut = DateDebutPicker.Date.Value.DateTime;
        DateTime dateFin = DateFinPicker.Date.Value.DateTime;

        var liste = ViewModel.ListePrincipale;
        List<Donnees> dateChoisies = new List<Donnees>();

        for (int i = 0; i < liste.Count; i++)
        {
            DateTime currentDate = DateTime.ParseExact(liste[i].Date, "dd/MM/yyyy", null);

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


        for (int i = 0;i < dateChoisies.Count;i++)
        {
            totalTemp += dateChoisies[i].Temperature;
            totalPrecip += dateChoisies[i].Precipitations;
            totalHumid += dateChoisies[i].Humidite;    
        }

        ViewModel.MaxTemp = dateChoisies.Max(x => x.Temperature).ToString();
        ViewModel.MinTemp = dateChoisies.Min(x => x.Temperature).ToString();
        ViewModel.MaxPrecip = dateChoisies.Max(x => x.Precipitations).ToString();
        ViewModel.MinPrecip = dateChoisies.Min(x => x.Precipitations).ToString();
        ViewModel.MaxHumid = dateChoisies.Max(x => x.Humidite).ToString();
        ViewModel.MinHumid = dateChoisies.Min(x => x.Humidite).ToString();

        ViewModel.AvgTemp = (totalTemp / dateChoisies.Count).ToString();
        ViewModel.AvgPrecip = (totalPrecip / dateChoisies.Count).ToString();
        ViewModel.AvgHumid = (totalHumid / dateChoisies.Count).ToString();

        float AverageTemp = (totalTemp / dateChoisies.Count);
        float AveragePrecip = (totalPrecip / dateChoisies.Count);
        float AverageHumid = (totalHumid/dateChoisies.Count);

        float variance = 0f;

        foreach (var i in dateChoisies)
        {
            float diff = i.Temperature - AverageTemp; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }

        ViewModel.EtTemp = ((float)Math.Sqrt(variance)).ToString();
        variance = 0f;
        foreach (var i in dateChoisies)
        {
            float diff = i.Precipitations - AveragePrecip; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }
        variance = 0f;
        ViewModel.EtPrecip = ((float)Math.Sqrt(variance)).ToString();
        foreach (var i in dateChoisies)
        {
            float diff = i.Humidite - AverageHumid; // On calcule la diffrence entre la temperature de la Journe[i] et la moyenne
            variance += diff * diff; // On calcule la sommation des differences au carres selon la formule de la variance
        }

        ViewModel.EtHumid = ((float)Math.Sqrt(variance)).ToString();



    }

    private async void CalculerStatsClick(object sender, EventArgs e)
    {
        await CalculerStats();
    }
}

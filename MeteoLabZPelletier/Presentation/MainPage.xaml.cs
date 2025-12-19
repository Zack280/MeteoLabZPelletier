using System.Threading.Tasks;
using Microsoft.Kiota.Abstractions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
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


    private async void CalculerStatsClick(object sender, EventArgs e)
    {
        await ViewModel.CalculerStats();
    }


}

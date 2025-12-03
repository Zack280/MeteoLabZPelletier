using HtmlAgilityPack;

namespace MeteoLabZPelletier.Services;

public class MeteoScraper
{
    public string temperature = string.Empty;

    //


    public void Scraping()
    {
        //Html request a weather.com
        String url = "https://weather.com/en-CA/weather/today/l/0dd1e7916b5d60ffb4801f0cb3a1ecc64aa029c0949f0257dc8303872ef8a525";
        var httpClient = new HttpClient();
        var html = httpClient.GetStringAsync(url).Result;
        var htmldoc = new HtmlDocument();
        htmldoc.LoadHtml(html);

        var temperatureElement = htmldoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'CurrentConditions--tempValue')]");

        temperature = temperatureElement.InnerText.Trim();
        Console.WriteLine("Temperature =" + temperature);

    }
}

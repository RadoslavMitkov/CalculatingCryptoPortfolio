using CalculatingCryptoPortfolioValue.Client.Models;
using System.Net.Http.Json;

namespace CalculatingCryptoPortfolioValue.Client.Services;

public sealed class CalculatePortfolioService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _client;
    private static List<int>? _coinsIds;

    public CalculatePortfolioService(IHttpClientFactory httpClientFactory)
    {
        this._httpClientFactory = httpClientFactory;
        this._client = this._httpClientFactory.CreateClient("coinlore");
    }

    public decimal InitialPortfolioValue(IEnumerable<Coin> coins)
    {
        decimal sum = 0m;

        foreach (var coin in coins)
        {
            sum += Math.Round(coin.Owned * coin.InitialPrice, 4);
        }

        return sum;
    }

    public double OverallPortfolioChange(decimal? initialValue, decimal? finalValue)
    {
        var delta = Math.Abs(finalValue!.Value - initialValue!.Value);
        var ratio = delta / initialValue;
        var percentageChange = Convert.ToDouble(Math.Round(ratio!.Value * 100, 2));

        return percentageChange;
    }

    public async Task<decimal> CurrentPortfolioValue(IEnumerable<Coin> coins)
    {
        decimal sum = 0m;

        if (_coinsIds is null)
        {
            await SetCoinsIds(coins);
        }

        foreach (var id in _coinsIds!)
        {
            try
            {
                var coinApiResponse = await _client.GetFromJsonAsync<IEnumerable<Ticker>>($"ticker/?id={id}");
                if (coinApiResponse is not null)
                {
                    var coinApiData = coinApiResponse.FirstOrDefault();
                    if (coinApiData is not null)
                    {
                        var initialCoinData = coins.FirstOrDefault(c => c.Name == coinApiData.Symbol);
                        var coinsOwned = initialCoinData.Owned;
                        var currentCoinPrice = coinApiData.Price_Usd;

                        sum += Math.Round(coinsOwned * currentCoinPrice, 4);
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        return sum;
    }

    public async Task<List<CoinPercentageChange>> PercentageChangePerCoin(IEnumerable<Coin> coins)
    {
        var result = new List<CoinPercentageChange>();

        if (_coinsIds is null)
        {
            await SetCoinsIds(coins);
        }

        foreach (var id in _coinsIds!)
        {
            try
            {
                var coinApiResponse = await _client.GetFromJsonAsync<IEnumerable<Ticker>>($"ticker/?id={id}");
                if (coinApiResponse is not null)
                {
                    var coinApiData = coinApiResponse.FirstOrDefault();
                    if (coinApiData is not null)
                    {
                        var initialCoin = coins.FirstOrDefault(c => c.Name == coinApiData.Symbol);
                        var initialValue = initialCoin.InitialPrice;
                        var finalValue = coinApiData.Price_Usd;

                        result.Add(PercentageChange(initialValue, finalValue, coinApiData.Symbol));
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        return result;
    }


    public async Task SetCoinsIds(IEnumerable<Coin> coins)
    {

        var coinNames = coins.Select(c => c.Name).ToList();
        var coinsFound = new List<string>();
        var coinsIds = new List<int>();
        var start = 0;

        while (coinNames.Count != 0)
        {
            try
            {
                var coinsApiResponse = await _client.GetFromJsonAsync<Tickers>($"tickers/?start={start}&limit=100");

                if (coinsApiResponse is not null)
                {
                    foreach (var coinName in coinNames)
                    {
                        var coin = coinsApiResponse.Data.FirstOrDefault(t => t.Symbol == coinName);
                        if (coin is not null)
                        {
                            coinsIds.Add(coin.Id);
                            coinsFound.Add(coinName);
                        }
                    }

                    foreach (var coinFound in coinsFound)
                    {
                        coinNames.Remove(coinFound);
                    }

                    coinsFound.Clear();
                }
            }
            catch (Exception)
            {
                throw;
            }

            start += 100;
        }

        _coinsIds = coinsIds;
    }

    private CoinPercentageChange PercentageChange(decimal initialValue, decimal finalValue, string name)
    {
        var delta = Math.Abs(finalValue - initialValue);
        var ratio = delta / initialValue;
        var percentageChange = Convert.ToDouble(Math.Round(ratio * 100, 2));
        var isIncreasing = initialValue <= finalValue;

        return new CoinPercentageChange(name, percentageChange, isIncreasing);
    }
}

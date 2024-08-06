namespace CalculatingCryptoPortfolioValue.Client.Models;

public record struct Coin(string Name, decimal Owned, decimal InitialPrice);
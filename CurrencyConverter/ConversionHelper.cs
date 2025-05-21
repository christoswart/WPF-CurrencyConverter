namespace CurrencyConverter
{
    public static class ConversionHelper
    {
        public static double Convert(double amount, double fromRate, double toRate)
        {
            if (fromRate == toRate)
                return amount;
            return (toRate * amount) / fromRate;
        }
    }
}
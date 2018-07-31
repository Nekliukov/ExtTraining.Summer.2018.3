using System.Reflection;

namespace No7.Solution.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var tradeStream = Assembly.GetExecutingAssembly().
                GetManifestResourceStream("No7.Solution.Console.trades.txt");

            TradeParser tradeProcessor = new No7.Solution.TradeParser();

            tradeProcessor.Parse(tradeStream, "TradeData");
        }
    }
}
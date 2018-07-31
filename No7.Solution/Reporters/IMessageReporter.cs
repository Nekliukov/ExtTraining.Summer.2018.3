namespace No7.Solution
{

    public partial class TradeParser
    {
        // Interface for getting rid of strong connection to Console output
        public interface IMessageReporter
        {
            void ShowMessage(string msg);
        }
    }
}

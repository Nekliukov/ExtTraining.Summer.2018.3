using System;

namespace No7.Solution
{

    public partial class TradeParser
    {
        // Class for outputting in Console (by default)
        public class DefaultReporter: IMessageReporter
        {
            public void ShowMessage(string msg) => Console.WriteLine(msg);
        }
    }
}

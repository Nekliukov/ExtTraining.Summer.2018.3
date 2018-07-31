using System.Windows.Forms;

namespace No7.Solution
{

    public partial class TradeParser
    {
        // Class for outputting in Message box (just to show another variant)
        public class MsgBoxReporter : IMessageReporter
        {
            public void ShowMessage(string msg) => MessageBox.Show(msg);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Threading;

namespace No7.Solution
{
    // class name was changed for better understanding
    public class TradeParser: Parser
    {
        #region Constants

        // Constant falues for getting rid of magic numbers
        private const int FIELDS_NUM = 3;
        private const int CURRENCIES_LEN = 6;
        private const float LOT_SIZE = 100000f;

        // Default culture info in case of misunderstanding
        private const string DEFAULT_CULTURE_INFO = "en-US";

        #endregion

        #region Private fields

        // Private field for lines and trades added
        private readonly List<string> lines = new List<string>();
        private readonly List<TradeRecord> trades = new List<TradeRecord>();

        #endregion

        #region .Ctors

        // Constructor on default culture info

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeParser"/> class.
        /// </summary>
        public TradeParser(): this(DEFAULT_CULTURE_INFO) { }

        // Contructor in case of custom culture info

        /// <summary>
        /// Initializes a new instance of the <see cref="TradeParser"/> class.
        /// </summary>
        /// <param name="cultureInfo">The culture information.</param>
        public TradeParser(string cultureInfo) 
            => Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureInfo);

        #endregion

        #region Override protected methods

        // Private method for reading lines from the file

        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        protected override void ReadData(Stream stream)
        {
            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            AddToTradeList();
        }

        // Added saver of all the trades

        /// <summary>
        /// Saves to database.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        protected override void SaveToDb(string dbName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings[$"{dbName}"].ConnectionString;
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    foreach (var trade in trades)
                    {
                        SaveTrade(trade, transaction, connection);
                    }

                    transaction.Commit();
                }
                connection.Close();
            }

            Console.WriteLine("INFO: {0} trades processed", trades.Count);
        }

        #endregion

        #region Private methods

        // Separate method for adding to trade list
        private void AddToTradeList()
        {
            var lineCount = 1;
            foreach (var line in lines)
            {
                var fields = line.Split(',');
                if (ValidateFields(fields, lineCount, out var tradeAmount, out var tradePrice))
                {
                    AddRecord(fields, tradeAmount, tradePrice);
                }

                lineCount++;
            }
        }

        // Separate record method
        private void AddRecord(IReadOnlyList<string> fields, int tradeAmount,
            decimal tradePrice)
        {
            var sourceCurrencyCode = fields[0].Substring(0, 3);
            var destinationCurrencyCode = fields[0].Substring(3, 3);

            var trade = new TradeRecord
            {
                SourceCurrency = sourceCurrencyCode,
                DestinationCurrency = destinationCurrencyCode,
                Lots = tradeAmount / LOT_SIZE,
                Price = tradePrice
            };

            trades.Add(trade);
        }

        // Save exact trade
        private void SaveTrade(TradeRecord trade, SqlTransaction transaction, SqlConnection connection )
        {
            var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "dbo.Insert_Trade";
            command.Parameters.AddWithValue("@sourceCurrency", trade.SourceCurrency);
            command.Parameters.AddWithValue("@destinationCurrency", trade.DestinationCurrency);
            command.Parameters.AddWithValue("@lots", trade.Lots);
            command.Parameters.AddWithValue("@price", trade.Price);

            command.ExecuteNonQuery();
        }

        // Added null or epty valudator
        private void ValidateNullOrEmpty(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("Null value was found");
            }

            if (value == string.Empty)
            {
                throw new ArgumentNullException("Empty value was found");
            }
        }

        // Separate method for validating each field in a line
        private bool ValidateFields(string[] fields, int lineCount,
            out int tradeAmount, out decimal tradePrice)
        {
            tradeAmount = 0;
            tradePrice = 0;

            foreach (var item in fields)
            {
                ValidateNullOrEmpty(item);
            }

            if (fields.Length != FIELDS_NUM)
            {
                Console.WriteLine("WARN: Line {0} malformed. Only {1} field(s) found.", lineCount,
                    fields.Length);
                return false;
            }

            if (fields[0].Length != CURRENCIES_LEN)
            {
                Console.WriteLine("WARN: Trade currencies on line {0} malformed: '{1}'", lineCount,
                    fields[0]);
                return false;
            }

            if (!int.TryParse(fields[1], out tradeAmount))
            {
                Console.WriteLine("WARN: Trade amount on line {0} not a valid integer: '{1}'", lineCount,
                    fields[1]);
            }

            if (!decimal.TryParse(fields[2], out tradePrice))
            {
                Console.WriteLine("WARN: Trade price on line {0} not a valid decimal: '{1}'", lineCount,
                    fields[2]);
            }

            return true;
        }
        #endregion
    }
}

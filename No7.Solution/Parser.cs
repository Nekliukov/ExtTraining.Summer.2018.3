using System.IO;

namespace No7.Solution
{
    /// <summary>
    /// Class that implement template method
    /// </summary>
    public abstract class Parser
    {
        #region Public API for inheritors

        // Template method for parsing        
        /// <summary>
        /// Parses the specified stream.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="dbName">Name of the database.</param>
        public void Parse(Stream stream, string dbName)
        {
            ReadData(stream);
            SaveToDb(dbName);
        }

        #endregion

        #region Protected override methods

        // Methods that will be overriden by inharitors
        
        /// <summary>
        /// Reads the data.
        /// </summary>
        /// <param name="stream">The stream.</param>
        protected abstract void ReadData(Stream stream);

        /// <summary>
        /// Saves to database.
        /// </summary>
        /// <param name="dbName">Name of the database.</param>
        protected abstract void SaveToDb(string dbName);

        #endregion

    }
}

using Polly.Utilities;

namespace Mongo.Repository
{
    public class PopulateOptions
    {
        /// <summary>
        /// the key for the item from the root document
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// the collection to link with 
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// the Key in the in the Reference collection to match
        /// </summary>
        public string ReferenceKey { get; set; }

        /// <summary>
        /// Populate Options for sub document
        /// </summary>
        public PopulateOptions ChildPopulateOptions { get; set; }
    }
}
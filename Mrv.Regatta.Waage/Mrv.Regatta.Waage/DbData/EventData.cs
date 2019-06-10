namespace Mrv.Regatta.Waage.DbData
{
    public class EventData
    {
        public int Id { get; set; }
        public string Title { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"{Title} [{Id}]";
        }
    }
}

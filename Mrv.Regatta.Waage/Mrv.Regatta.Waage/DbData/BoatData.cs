using System.Collections.Generic;
using System.Linq;

namespace Mrv.Regatta.Waage.DbData
{
    public class BoatData
    {
        public string TitleLong { get; set; }
        public string TitleShort { get; set; }
        public List<RowerData> Rowers { get; set; }
        public RowerData Cox { get; set; }
        public bool Canceled { get; set; }
        public byte BibNumber { get; set; }

        /// <summary>
        /// Stellt fest, ob ein bestimmter Ruderer im Boot sitzt (entweder als Steuermann oder als Teil der Mannschaft).
        /// </summary>
        /// <param name="id">The rower identifier.</param>
        /// <returns>
        /// </returns>
        public bool HasRower(int id)
        {
            return (Cox?.Id == id) || Rowers.Any(r => r.Id == id);
        }
    }
}

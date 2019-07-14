using System;
using System.Collections.Generic;
using System.Linq;

namespace Mrv.Regatta.Waage.DbData
{
    public class RaceData
    {
        public int Id { get; set; }
        public string RaceNumber { get; set; }
        public string ShortTitle { get; set; }
        public string LongTitle { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsLightweightRace { get; set; }
        public bool IsChildrenRace { get; set; }
        public bool IsCoxedRace { get; set; }
        public byte NumberOfRowers { get; set; }
        public Weight MaxSingleWeight { get; set; }
        public Weight MaxAverageWeight { get; set; }
        public Weight MinCoxWeight { get; set; }
        public Weight MaxAdditionalCoxWeight { get; set; }

        public List<BoatData> BoatsData { get; set; }

        /// <summary>
        /// Stellt fest, ob ein bestimmter Ruderer im Boot sitzt (entweder als Steuermann oder als Teil der Mannschaft).
        /// </summary>
        /// <param name="id">The rower identifier.</param>
        /// <returns>
        /// </returns>
        public bool HasRower(int id)
        {
            return BoatsData.Any(b => b.HasRower(id));
        }

    }
}

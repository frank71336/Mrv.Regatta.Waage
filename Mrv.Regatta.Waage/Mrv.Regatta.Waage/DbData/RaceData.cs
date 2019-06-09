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

        /*
        public int? RRF { get; set; } 
        public string RNr { get; set; } 
        public string RKBez { get; set; } 
        public string RLBez { get; set; } 
        public string RLänge { get; set; }
        
        public string RTag { get; set; } 
        public short? RBootAnz { get; set; } 
        public bool RNotProtokoll { get; set; } 
        
        public string RMemo { get; set; } 
        public double? RAnzRudererBoot { get; set; } 
        public string RLaufTyp { get; set; } 
        public double? RRennTyp { get; set; } 
        public string RLaufTypGenerell { get; set; } 
        public int? RGewichtsklassenID { get; set; } 
        public int? RAltersklassenID { get; set; }
        public int? RBootsklassenID { get; set; } 
        public string RZusatz { get; set; } 
        public int? RAltersklassenZusatzID { get; set; } 
        public string Lgr { get; set; } 
        */
    }
}

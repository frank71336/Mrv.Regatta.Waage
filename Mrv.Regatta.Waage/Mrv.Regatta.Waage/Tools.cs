﻿using Mrv.Regatta.Waage.Db.DataModels;
using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Mrv.Regatta.Waage
{
    internal class Tools
    {
        /*
        /// <summary>
        /// Index des Wochentags in der Liste der Wochentage für die Sortierung bestimmen
        /// </summary>
        /// <param name="weekday">The weekday.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        internal static int GetDayOrderValue(string weekday)
        {
            var racingDays = Data.Instance.Settings.Renntage;

            var item = racingDays.SingleOrDefault(x => x.Wochentag.Equals(weekday, StringComparison.OrdinalIgnoreCase));
            if (item == null)
            {
                throw new Exception($"Wochentag '{weekday}' ist unbekannt!");
            }

            return racingDays.ToList().IndexOf(item);
        }
        */

        /// <summary>
        /// Fixes the race timestamp.
        /// </summary>
        /// <param name="races">The races.</param>
        /// <exception cref="System.Exception"></exception>
        public static void FixRaceTimestamp(List<TRennen> races)
        {
            // Zeitstempel des Rennens korrigieren
            // Da steht nur die Uhrzeit. Das wird im DateTime-Format zum 01.01.0001 xx:xx.
            var racingDays = Data.Instance.Settings.Renntage;
            foreach (var race in races)
            {
                var item = racingDays.SingleOrDefault(x => x.Wochentag.Equals(race.RTag, StringComparison.OrdinalIgnoreCase));
                if (item == null)
                {
                    throw new Exception($"Wochentag '{race.RTag}' ist unbekannt!");
                }
                var day = item.Datum;
                var time = (DateTime)race.RZeit;
                race.RZeit = new DateTime(day.Year, day.Month, day.Day, time.Hour, time.Minute, 0);
            }
        }

        /// <summary>
        /// Liest alle Wiegungen aus den Dateien ein.
        /// </summary>
        internal static void ReadWeightings()
        {
            var path = Data.Instance.Settings.Pfade.Messungen;

            // Pfad muss existieren
            if (!Directory.Exists(path))
            {
                throw new Exception($"Pfad '{path}' existiert nicht!");
            }

            // zunächst leere Liste
            Data.Instance.Weightings = new List<Messung>();

            // Alle Messungen auflisten
            var files = Directory.EnumerateFiles(path, "*.xml", System.IO.SearchOption.TopDirectoryOnly);

            // Alle Dateien lesen zur Liste hinzufügen
            foreach (var file in files)
            {
                var messung = XmlBase.XmlBase.Load<Messung>(new XmlBase.XmlFilePath(file));
                Data.Instance.Weightings.Add(messung);
            }
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}

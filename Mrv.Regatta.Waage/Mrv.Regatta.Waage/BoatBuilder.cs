using Mrv.Regatta.Waage.DbData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mrv.Regatta.Waage
{

    class BoatBuilder
    {
        private TimeSpan _currentTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoatBuilder"/> class.
        /// </summary>
        /// <param name="currentTime">The current time.</param>
        public BoatBuilder(TimeSpan currentTime)
        {
            _currentTime = currentTime;
        }

        /// <summary>
        /// Adds the rower.
        /// </summary>
        /// <param name="rowers">The rowers.</param>
        /// <param name="raceData">The race data.</param>
        /// <param name="rowerData">The rower data.</param>
        /// <param name="rowerIsCox">if set to <c>true</c> [steuermann].</param>
        /// <param name="delay">The delay.</param>
        public void AddRower(ref List<UserControls.Rower> rowers, RaceData raceData, RowerData rowerData, bool rowerIsCox, TimeSpan delay)
        {
            UserControls.RowerStatus rowerStatus = UserControls.RowerStatus.None;
            string weightRower = "-";
            float? weightInfo = null;

            var weightings = GlobalData.Instance.Weightings.Where(w => w.Id == rowerData.Id).ToList();
            var raceDt = raceData.DateTime + delay;

            var weightRowerRequired = raceData.MaxSingleWeight.IsSpecified();
            var weightCoxRequired = raceData.MaxAdditionalCoxWeight.IsSpecified();

            if (raceData.IsChildrenRace)
            {
                #region Kinderennen

                #region Zeitfenster für's Wiegen im Kinderrennen

                // Es gibt kein "frühestens", das Kind darf so früh kommen wie es möchte
                // Daher ist "frühestens" der 01.01.2000 und das ist auf jeden Fall vor jedem beliebigen Zeitpunkt, zu dem das Kind kommt.
                // Könnte man eigentlich zusammen mit der Abfrage rauswerfen, aber vielleicht gibt es ja doch mal ein "frühestens"

                // frühestens 01.01.2000 04:00 Uhr
                var existingWeightingAcceptedFromDt = new DateTime(2000, 1, 1, 4, 0, 0);

                // neue Wiegung (wenn es keine andere gibt) frühestens ??? h ??? min vorher
                // gleicher Fall wie oben, daher auch 01.01.2000 04:00 Uhr
                var newWeightingAcceptedFromDt = new DateTime(2000, 1, 1, 4, 0, 0); // ansonsten so was hier:    raceDt.Add(new TimeSpan(-2, -5, 0));

                // spätestens 1 Stunde vorher
                var weightingToDt = raceDt.Add(new TimeSpan(-1, 0, 0));

                #endregion

                // Ruderer muss im Zeitfenster zum Wiegen gekommen sein
                // => letzte Wiegung im Zeitfenster bestimmen
                var weighting = weightings.Where(w => (w.Zeitstempel >= existingWeightingAcceptedFromDt) && (w.Zeitstempel <= weightingToDt)).OrderByDescending(x => x.Zeitstempel).FirstOrDefault();

                if (!rowerIsCox)
                {
                    // normaler Ruderer im Kinderrennen (nicht der Steuermann)

                    if (weightRowerRequired)
                    {
                        // Ruderer muss gewogen werden

                        if (weighting != null)
                        {
                            // Es gibt eine Wiegung im Zeitfenster, die entscheidet, ob der Ruderer starten darf.
                            // (Wenn es hingegen keine gültige Wiegung gibt, kann der Status auch nicht OK sein.)

                            rowerStatus = (weighting.Gewicht <= raceData.MaxSingleWeight.Value) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
                            weightInfo = weighting.Gewicht;
                            weightRower = $"{weighting.Gewicht} kg";
                        }
                        else
                        {
                            // keine Wiegung im Zeitfenster vorhanden
                            // entweder es ist noch zu früh zum Wiegen oder es ist schon zu spät dazu

                            rowerStatus = GetStatusNoWeight(newWeightingAcceptedFromDt, weightingToDt);
                        }
                    }
                    else
                    {
                        // Ruderer muss nicht gewogen werden (kein Leichtgewichtrennen)
                        rowerStatus = UserControls.RowerStatus.WeightOk;
                    }
                }
                else
                {
                    // Es handelt sich um einen Steuerman im Kinderrennen

                    // Steuermann im Kinderrennen muss grundsätzlich nicht gewogen werden!

                    rowerStatus = UserControls.RowerStatus.WeightOk;
                }

                #endregion Kinderennen
            }
            else
            {
                #region Kein Kinderennen

                #region Zeitfenster für's Wiegen

                // frühestens 2 Stunden vor dem ersten Rennen an diesem Tag.
                // Das wird hier aber nicht abgefragt, weil man dann hier abfragen müsste, ob dieses Rennen hier das erste an diesem Tag ist oder nicht.
                // Stattdessen verlassen wir uns hier darauf, niemand außerhalb des zugelassenen Zeitfensters zum Wiegen zugelassen wurde,
                // in dem Fall muss die letzte/jüngste Wiegung auch eine gültige Wiegung sein, Hauptsache, sie hat am gleichen Tag stattgefunden.

                // frühestens 4 Uhr morgens am Tag des Rennens (und damit sicher nicht mehr am Vortrag)
                var existingWeightingAcceptedFromDt = new DateTime(raceDt.Year, raceDt.Month, raceDt.Day, 4, 0, 0);

                // neue Wiegung (wenn es keine andere gibt) frühestens 2 h vorher
                var newWeightingAcceptedFromDt = raceDt.Add(new TimeSpan(-2, 0, 0));

                // spätestens 1 Stunde vorher
                var weightingToDt = raceDt.Add(new TimeSpan(-1, 0, 0));

                #endregion

                // Ruderer muss im Zeitfenster zum Wiegen gekommen sein
                // => letzte Wiegung im Zeitfenster bestimmen
                var weighting = weightings.Where(w => (w.Zeitstempel >= existingWeightingAcceptedFromDt) && (w.Zeitstempel <= weightingToDt)).OrderByDescending(x => x.Zeitstempel).FirstOrDefault();

                if (!rowerIsCox)
                {
                    // normaler Ruderer (nicht der Steuermann)

                    if (weightRowerRequired)
                    {
                        // Ruderer muss gewogen werden

                        if (weighting != null)
                        {
                            // Es gibt eine Wiegung im Zeitfenster, die entscheidet, ob der Ruderer starten darf.
                            // (Wenn es hingegen keine gültige Wiegung gibt, kann der Status auch nicht OK sein.)

                            rowerStatus = (weighting.Gewicht <= raceData.MaxSingleWeight.Value) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
                            weightInfo = weighting.Gewicht;
                            weightRower = $"{weighting.Gewicht} kg";
                        }
                        else
                        {
                            // keine Wiegung im Zeitfenster vorhanden
                            // entweder es ist noch zu früh zum Wiegen oder es ist schon zu spät dazu

                            rowerStatus = GetStatusNoWeight(newWeightingAcceptedFromDt, weightingToDt);
                        }
                    }
                    else
                    {
                        // Ruderer muss nicht gewogen werden (kein Leichtgewichtrennen)
                        rowerStatus = UserControls.RowerStatus.WeightOk;
                    }
                }
                else
                {
                    // Es handelt sich um einen Steuerman

                    if (weightCoxRequired)
                    {
                        // Steuermann muss gewogen werden

                        if (weighting != null)
                        {
                            // Es gibt eine Wiegung im Zeitfenster, die entscheidet, ob der Ruderer starten darf.
                            // (Wenn es hingegen keine gültige Wiegung gibt, kann der Status auch nicht OK sein.)
                            // (Steuermann darf leichter sein als das Mindestgewicht, sofern er damit nicht das maximale Zusatzgewicht überschreitet.)
                            
                            rowerStatus = (weighting.Gewicht >= raceData.MinCoxWeight.Value - raceData.MaxAdditionalCoxWeight.Value) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
                            weightInfo = weighting.Gewicht;
                            weightRower = $"{weighting.Gewicht} kg";
                        }
                        else
                        {
                            // keine Wiegung im Zeitfenster vorhanden
                            // entweder es ist noch zu früh zum Wiegen oder es ist schon zu spät dazu

                            rowerStatus = GetStatusNoWeight(newWeightingAcceptedFromDt, weightingToDt);
                        }
                    }
                    else
                    {
                        // Steuermann muss nicht gewogen werden (kein Leichtgewichtrennen)
                        rowerStatus = UserControls.RowerStatus.WeightOk;
                    }
                }

                #endregion Kein Kinderennen
            }
            
            var newRower = new UserControls.Rower()
            {
                Id = rowerData.Id,
                Name = $"{rowerData.LastName}, {rowerData.FirstName} ({weightRower})",
                Type = rowerIsCox ? UserControls.RowerType.Cox : UserControls.RowerType.Rower,
                Status = rowerStatus,
                WeightInfo = weightInfo
            };

            rowers.Add(newRower);
        }

        /// <summary>
        /// Calculate rower status if no weight is available depending on current time.
        /// </summary>
        /// <param name="newWeightingAcceptedFromDt">The new weighting accepted from dt.</param>
        /// <param name="weightingToDt">The weighting to dt.</param>
        /// <returns></returns>
        private UserControls.RowerStatus GetStatusNoWeight(DateTime newWeightingAcceptedFromDt, DateTime weightingToDt)
        {
            // keine Wiegung im Zeitfenster vorhanden
            // entweder es ist noch zu früh zum Wiegen oder es ist schon zu spät dazu

            var day = Properties.Settings.Default.Today;
            var dt = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            if (dt < newWeightingAcceptedFromDt)
            {
                // es ist noch zu früh
                return UserControls.RowerStatus.WaitingForTimeWindow;
            }
            else if ((dt > newWeightingAcceptedFromDt) && (dt < weightingToDt))
            {
                return UserControls.RowerStatus.WaitingInsideTimeWindow;
            }
            else
            {
                return UserControls.RowerStatus.TooLate;
            }
        }

        /// <summary>
        /// Sets the race status.
        /// </summary>
        /// <param name="race">The race.</param>
        public void SetRaceStatus(UserControls.Race race)
        {
            // Rennen ist zunächst mal OK
            race.Status = UserControls.RaceStatus.Ok;

            // außer... :

            // Reihenfolge beachten!
            if (race?.Boats?.Any(b => b.Status == UserControls.BoatStatus.WaitingForTimeWindow) == true)
            {
                // Es gibt noch Ruderer ohne Gewicht, bei denen ist es noch nicht Zeit zum wiegen
                race.Status = UserControls.RaceStatus.WaitingForTimeWindow;
            }
            else if (race?.Boats?.Any(b => b.Status == UserControls.BoatStatus.WaitingInsideTimeWindow) == true)
            {
                // Es gibt noch Ruderer ohne Gewicht, die könnten jetzt aber bereits zum Wiegen erscheinen
                race.Status = UserControls.RaceStatus.WaitingInsideTimeWindow;
            }
            else if (race?.Boats?.Any(b => b.Status == UserControls.BoatStatus.BoatNok) == true)
            {
                // Alle waren zum Wiegen da, aber es gibt ein Boot, das nicht OK ist
                race.Status = UserControls.RaceStatus.OkWithProblems;
            }
        }

        /// <summary>
        /// Sets the boat status.
        /// </summary>
        /// <param name="boat">The boat.</param>
        public void SetBoatStatus(RaceData raceData, BoatData boatData, UserControls.Race race, UserControls.Boat boat)
        {
            if (boatData.Canceled)
            {
                // Boot ist abgemeldet
                boat.Comment = "Boot abgemeldet!";
                boat.Status = UserControls.BoatStatus.BoatOk;
                boat.Canceled = true;

                // Wenn das Boot abgemeldet ist, dann ist es egal, ob die einzelnen Ruderer darin zum Wiegen da waren oder nicht.
                // Diese Ruderer sollen dann nicht als zu schwer oder fehlend angezeigt werden, sondern als in Ordnung.
                var rowsers = boat.Rowers.Where(r => new[] { UserControls.RowerStatus.TooLate, UserControls.RowerStatus.WeightNotOk }.Contains(r.Status));
                foreach (var rower in rowsers)
                {
                    rower.Status = UserControls.RowerStatus.WaitingForTimeWindow;
                }

                return;
            }

            // Im UserControl ("boat") sind Ruderer und Steuermann in einer gemeinsamen Liste enthalten
            // und lassen sich nur durch das Type-Attribut des jeweiligen Eintrags unterscheiden
            var rowersWithoutCox = boat.Rowers.Where(r => r.Type == UserControls.RowerType.Rower);

            // Prüfen ob das Boot vollständig besetzt ist
            if (raceData.NumberOfRowers != rowersWithoutCox.Count())
            {
                boat.Comment = "Anzahl der Ruderer passt nicht zur Bootsklasse!";
                boat.Status = UserControls.BoatStatus.BoatNok;
                return;
            }

            // Prüfen ob der Steuermann da ist
            if (raceData.IsCoxedRace && (boat.Rowers.Count(r => r.Type == UserControls.RowerType.Cox) != 1))
            {
                boat.Comment = "Steuermann fehlt!";
                boat.Status = UserControls.BoatStatus.BoatNok;
                return;
            }

            // Durchschnittsgewicht wird berechnet, wenn es mehr als einen Ruderer gibt und
            // wenn mindestens schon von einem Ruderer das Gewicht vorliegt.
            var calculateAverageWeight = (rowersWithoutCox.Count() > 1) && (rowersWithoutCox?.Any(r => r.WeightInfo != null) == true);

            // Durchschnittsgewicht wird auf Richtigkeit geprüft, wenn ein Sollwert vorhanden ist
            var checkAverageWeightOk = calculateAverageWeight && (raceData.MaxAverageWeight?.Value != null) && (raceData.MaxAverageWeight.Value > 0);

            float averageWeight = 0;
            boat.AverageWeight = "";
            if (checkAverageWeightOk)
            {
                // Durschnittsgewicht berechnen
                var count = 0;
                foreach (var rower in rowersWithoutCox)
                {
                    if (rower.WeightInfo != null)
                    {
                        averageWeight += (float)rower.WeightInfo;
                        count++;
                    }
                }

                if (count == 0)
                {
                    averageWeight = 0;
                }
                else
                {
                    averageWeight = averageWeight / count;
                    boat.AverageWeight = $"(ø {averageWeight} kg)";
                }

                if (checkAverageWeightOk)
                {

                }

            }

            // Reihenfolge beachten!
            if (boat?.Rowers?.Any(r => r.Status == UserControls.RowerStatus.WeightNotOk) == true)
            {
                // es gibt Ruderer, bei denen das Gewicht nicht passt
                boat.Status = UserControls.BoatStatus.BoatNok;
            }
            else if (boat?.Rowers?.Any(r => r.Status == UserControls.RowerStatus.TooLate) == true)
            {
                // Es gibt Ruderer, die das Wiegen verpasst haben
                boat.Status = UserControls.BoatStatus.BoatNok;
            }
            else if (boat?.Rowers?.Any(r => r.Status == UserControls.RowerStatus.WaitingForTimeWindow) == true)
            {
                // Es gibt Ruderer ohne Gewicht, für die es noch zu früh zum Wiegen ist
                boat.Status = UserControls.BoatStatus.WaitingForTimeWindow;
            }
            else if (boat?.Rowers?.Any(r => r.Status == UserControls.RowerStatus.WaitingInsideTimeWindow) == true)
            {
                // Es gibt Ruderer ohne Gewicht, die jetzt zum wiegen dran wären
                boat.Status = UserControls.BoatStatus.WaitingInsideTimeWindow;
            }
            else if (boat.Rowers.All(r => r.Status == UserControls.RowerStatus.WeightOk))
            {
                // alle Ruderer sind einzeln OK, jetzt muss noch das Durschnittsgewicht stimmen
                if (checkAverageWeightOk)
                {
                    boat.Status = (averageWeight <= raceData.MaxAverageWeight.Value) ? UserControls.BoatStatus.BoatOk : UserControls.BoatStatus.BoatNok;
                }
                else
                {
                    // Durschnittsgewicht interessiert nicht
                    boat.Status = UserControls.BoatStatus.BoatOk;
                }
            }

            // Zusatzgewicht bestimmen

            var cox = boat.Rowers.SingleOrDefault(r => r.Type == UserControls.RowerType.Cox);
            if (cox != null)
            {
                // es gibt einen Steuermann

                if (cox.WeightInfo != null)
                {
                    // es gibt ein Gewicht für den Steuermann

                    if (raceData.MinCoxWeight.IsSpecified())
                    {
                        // Sollgewicht für Steuermann vorhanden

                        var weight = (float)cox.WeightInfo;

                        var additionalWeight = raceData.MinCoxWeight.Value - weight;

                        if (additionalWeight > 0)
                        {
                            // Steuermann ist zu leicht
                            boat.Comment = $"Zusatzgewicht für Steuermann! => {additionalWeight:0.0} kg";
                        }
                    }

                }

            }

        }

    }
}

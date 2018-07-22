using Mrv.Regatta.Waage.Db.DataModels;
using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// <param name="dbRowers">The database rowers.</param>
        /// <param name="rowers">The rowers.</param>
        /// <param name="rowerId">The rower identifier.</param>
        /// <param name="steuermann">if set to <c>true</c> [steuermann].</param>
        public void AddRower(RennenRennen race, TRennen dbRace, List<UserControls.Rower> rowers, int? rowerId, bool steuermann)
        {
            if (rowerId == null)
            {
                return;
            }

            var rowerId2 = (int)rowerId;
            if (rowerId2 > 1)
            {
                // Id == 1 bedeutet, Platz nicht besetzt.

                UserControls.RowerStatus rowerStatus = UserControls.RowerStatus.None;
                string weightRower = "-";
                float? weightInfo = null;

                var dbRowers = Data.Instance.DbRowers;
                var weightings = Data.Instance.Weightings.Where(w => w.Id == rowerId).ToList();
                var raceDt = (DateTime)dbRace.RZeit;

                var weightRowerRequired = race.RennInfo.EinzelgewichtSpecified;
                var weightCoxRequired = race.RennInfo.GewichtSteuermannSpecified;

                if (race.RennInfo.Kinderrennen)
                {
                    #region Kinderennen

                    #region Zeitfenster für's Wiegen im Kinderrennen

                    // Es gibt kein frühestens, das Kind darf so früh kommen wie es möchte
                    // Daher ist "frühestens" der 01.01.2000 und das ist auf jeden Fall vor jedem beliebigen Zeitpunkt, zu dem das Kind kommt.
                    // Könnte man eigentlich zusammen mit der Abfrage rauswerfen, aber vielleicht gibt es ja doch mal ein "frühestens"

                    // frühestens 01.01.2000 04:00 Uhr
                    var existingWeightingAcceptedFromDt = new DateTime(2000, 1, 1, 4, 0, 0);

                    // neue Wiegung (wenn es keine andere gibt) frühestens ??? h ??? min vorher
                    // gleicher Fall wie oben, daher auch 01.01.2000 04:00 Uhr
                    var newWeightingAcceptedFromDt = new DateTime(2000, 1, 1, 4, 0, 0); // ansonsten so was hier:    raceDt.Add(new TimeSpan(-2, -5, 0));

                    // spätestens 30 Min vorher (eigentlich 1 Stunde)
                    var weightingToDt = raceDt.Add(new TimeSpan(0, -30, 0));

                    #endregion

                    // Ruderer muss im Zeitfenster zum Wiegen gekommen sein
                    // => letzte Wiegung im Zeitfenster bestimmen
                    var weighting = weightings.Where(w => (w.Zeitstempel >= existingWeightingAcceptedFromDt) && (w.Zeitstempel <= weightingToDt)).OrderByDescending(x => x.Zeitstempel).FirstOrDefault();

                    if (!steuermann)
                    {
                        // normaler Ruderer im Kinderrennen

                        if (weightRowerRequired)
                        {
                            // Ruderer muss gewogen werden

                            if (weighting != null)
                            {
                                // Es gibt eine Wiegung im Zeitfenster, die entscheidet, ob der Ruderer starten darf.
                                // (Wenn es hingegen keine gültige Wiegung gibt, kann der Status auch nicht OK sein.)

                                rowerStatus = (weighting.Gewicht <= race.RennInfo.Einzelgewicht) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
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
                    // Das wird hier aber nicht abgefragt, wiel man dann hier abfragen müsste, ob dieses Rennen hier das erste an diesem Tag ist oder nicht.
                    // Stattdessen verlassen wir uns hier darauf, niemand außerhalb des zugelassenen Zeitfensters zum Wiegen zugelassen wurde,
                    // in dem Fall muss die letzte/jüngste Wiegung auch eine gültige Wiegung sein, Hauptsache, sie hat am gleichen Tag stattgefunden.

                    // frühestens 4 Uhr morgens am Tag des Rennens (und damit sicher nicht mehr am Vortrag)
                    var existingWeightingAcceptedFromDt = new DateTime(raceDt.Year, raceDt.Month, raceDt.Day, 4, 0, 0);

                    // neue Wiegung (wenn es keine andere gibt) frühestens 2 h 5 min vorher
                    var newWeightingAcceptedFromDt = raceDt.Add(new TimeSpan(-2, -5, 0));

                    // spätestens 30 Min vorher (eigentlich 1 Stunde)
                    var weightingToDt = raceDt.Add(new TimeSpan(0, -30, 0));

                    #endregion

                    // Ruderer muss im Zeitfenster zum Wiegen gekommen sein
                    // => letzte Wiegung im Zeitfenster bestimmen
                    var weighting = weightings.Where(w => (w.Zeitstempel >= existingWeightingAcceptedFromDt) && (w.Zeitstempel <= weightingToDt)).OrderByDescending(x => x.Zeitstempel).FirstOrDefault();

                    if (!steuermann)
                    {
                        // normaler Ruderer

                        if (weightRowerRequired)
                        {
                            // Ruderer muss gewogen werden

                            if (weighting != null)
                            {
                                // Es gibt eine Wiegung im Zeitfenster, die entscheidet, ob der Ruderer starten darf.
                                // (Wenn es hingegen keine gültige Wiegung gibt, kann der Status auch nicht OK sein.)

                                rowerStatus = (weighting.Gewicht <= race.RennInfo.Einzelgewicht) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
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

                                rowerStatus = (weighting.Gewicht >= race.RennInfo.GewichtSteuermann - 10) ? UserControls.RowerStatus.WeightOk : UserControls.RowerStatus.WeightNotOk;
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

                var dbRower = dbRowers.Single(x => x.RID == rowerId2);

                var newRower = new UserControls.Rower()
                {
                    Id = rowerId2,
                    Name = $"{dbRower.RName}, {dbRower.RVorname} ({weightRower})",
                    Type = steuermann ? UserControls.RowerType.Cox : UserControls.RowerType.Rower,
                    Status = rowerStatus,
                    WeightInfo = weightInfo
                };

                rowers.Add(newRower);
            }
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

            var day = Data.Instance.Settings.ZeitstempelHeute;
            var dt = new DateTime(day.Year, day.Month, day.Day, _currentTime.Hours, _currentTime.Minutes, _currentTime.Seconds);

            if (dt < newWeightingAcceptedFromDt)
            {
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
            if (race.Boats.Any(b => b.Status == UserControls.BoatStatus.WaitingForTimeWindow))
            {
                // Es gibt noch Ruderer ohne Gewicht, bei denen ist es noch nicht Zeit zum wiegen
                race.Status = UserControls.RaceStatus.WaitingForTimeWindow;
            }
            else if (race.Boats.Any(b => b.Status == UserControls.BoatStatus.WaitingInsideTimeWindow))
            {
                // Es gibt noch Ruderer ohne Gewicht, die könnten jetzt aber bereits zum Wiegen erscheinen
                race.Status = UserControls.RaceStatus.WaitingInsideTimeWindow;
            }
            else if (race.Boats.Any(b => b.Status == UserControls.BoatStatus.BoatNok))
            {
                // Alle waren zum Wiegen da, aber es gibt ein Boot, das nicht OK ist
                race.Status = UserControls.RaceStatus.OkWithProblems;
            }
        }

        /// <summary>
        /// Sets the boat status.
        /// </summary>
        /// <param name="boat">The boat.</param>
        public void SetBoatStatus(UserControls.Race race, UserControls.Boat boat)
        {
            // Boot abgemeldet?
            var dbRace = Data.Instance.DbRaces.Single(r => r.Index == race.Id);
            var cancelation = Data.Instance.DbCancellations.FirstOrDefault(c => (c.RennNr == dbRace.RNr) && (c.StartNr == boat.StartNumber));
            if (cancelation != null)
            {
                // Boot ist abgemeldet
                boat.Comment = "Boot abgemeldet!";
                boat.Status = UserControls.BoatStatus.BoatOk;
                boat.Canceled = true;
                return;
            }

            var rennen = Data.Instance.Races.Rennen1.Single(r => r.RennNr == race.RaceNumber);

            var rowersWithoutCox = boat.Rowers.Where(r => r.Type == UserControls.RowerType.Rower);

            // soll für dieses Rennen das Durschnittsgewicht berücksichtigt werden und gibt es schon einen Ruderer mit Gewicht?
            var checkAverageWeight = rennen.RennInfo.DurchschnittsgewichtSpecified && rowersWithoutCox.Any(r => r.WeightInfo != null);
            float averageWeight = 0;
            boat.AverageWeight = "";

            if (checkAverageWeight)
            {
                // Für dieses Rennen muss das Durchschnittsgewicht passen
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
            }

            // Reihenfolge beachten!
            if (boat.Rowers.Any(r => r.Status == UserControls.RowerStatus.WeightNotOk))
            {
                // es gibt Ruderer, bei denen das Gewicht nicht passt
                boat.Status = UserControls.BoatStatus.BoatNok;
            }
            else if (boat.Rowers.Any(r => r.Status == UserControls.RowerStatus.TooLate))
            {
                // Es gibt Ruderer, die das Wiegen verpasst haben
                boat.Status = UserControls.BoatStatus.BoatNok;
            }
            else if (boat.Rowers.Any(r => r.Status == UserControls.RowerStatus.WaitingForTimeWindow))
            {
                // Es gibt Ruderer ohne Gewicht, für die es noch zu früh zum Wiegen ist
                boat.Status = UserControls.BoatStatus.WaitingForTimeWindow;
            }
            else if (boat.Rowers.Any(r => r.Status == UserControls.RowerStatus.WaitingInsideTimeWindow))
            {
                // Es gibt Ruderer ohne Gewicht, die jetzt zum wiegen dran wären
                boat.Status = UserControls.BoatStatus.WaitingInsideTimeWindow;
            }
            else if (boat.Rowers.All(r => r.Status == UserControls.RowerStatus.WeightOk))
            {
                // alle Ruderer sind einzeln OK, jetzt muss noch das Durschnittsgewicht stimmen
                if (checkAverageWeight)
                {
                    boat.Status = (averageWeight <= rennen.RennInfo.Durchschnittsgewicht) ? UserControls.BoatStatus.BoatOk : UserControls.BoatStatus.BoatNok;
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

                    if (rennen.RennInfo.GewichtSteuermannSpecified)
                    {
                        // Sollgewicht für Steuermann vorhanden
                        var weight = (float)cox.WeightInfo;

                        var additionalWeight = rennen.RennInfo.GewichtSteuermann - weight;

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

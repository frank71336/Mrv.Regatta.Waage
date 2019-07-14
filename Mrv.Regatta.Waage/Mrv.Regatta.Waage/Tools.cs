using Mrv.Regatta.Waage.Xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

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
        /// Liest alle Wiegungen aus den Dateien ein.
        /// </summary>
        internal static void ReadWeightings()
        {
            var path = Properties.Settings.Default.WeighingsPath;

            // Pfad muss existieren
            if (!Directory.Exists(path))
            {
                MessageBox.Show($"Pfad für das Ablegen der Messungen '{path}' existiert nicht. Einstellungen prüfen!");
                return;
            }

            // zunächst leere Liste
            GlobalData.Instance.Weightings = new List<Messung>();

            // Alle Messungen auflisten
            var files = Directory.EnumerateFiles(path, "*.xml", System.IO.SearchOption.TopDirectoryOnly);

            // Alle Dateien lesen zur Liste hinzufügen
            foreach (var file in files)
            {
                var messung = XmlBase.XmlBase.Load<Messung>(new XmlBase.XmlFilePath(file));
                GlobalData.Instance.Weightings.Add(messung);
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

        /// <summary>
        /// Invokes if required.
        /// </summary>
        /// <param name="dispatcherObject">The dispatcher object. (Window, Page, ... OR 'this')</param>
        /// <param name="action">The action.</param>
        public static void InvokeIfRequired(DispatcherObject dispatcherObject, Action action)
        {
            if (!dispatcherObject.Dispatcher.CheckAccess())
            {
                dispatcherObject.Dispatcher.Invoke(new Action(() => action()));
            }
            else
            {
                action();
            }
        }

        /// <summary>
        /// Logs the error to file and shows a message box with the error.
        /// </summary>
        /// <param name="errorTexts">The error texts.</param>
        internal static void LogError(params object[] errorTexts)
        {
            var strParts = new List<string>();
            foreach(var obj in errorTexts)
            {
                if (obj == null)
                {
                    strParts.Add("NULL");
                }
                else
                {
                    strParts.Add(obj.ToString());
                }
            }

            var file = Properties.Settings.Default.ErrorLogFile;
            var message = string.Join(" ", strParts);
            var messageWithTimestamp = DateTime.Now.ToString() + ": " + message;
            File.AppendAllText(file, messageWithTimestamp + Environment.NewLine);
            ShowError(message);
        }

    }
}

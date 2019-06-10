using System;
using System.Collections;

namespace Mrv.Regatta.Waage.Pages.SettingsPage
{
    public class SettingsPageViewModel : ViewModelBase.ViewModelBase
    {
        public string ConnectionString { get; set; }
        public DateTime Today { get; set; }
        public object Event { get; set; }
        public IEnumerable Events { get; set; }
        public string WeighingsPath { get; set; }
        public string WeighingsLogFile { get; set; }
        public string ErrorLogFile { get; set; }
        public string BackupPath { get; set; }
    }
}

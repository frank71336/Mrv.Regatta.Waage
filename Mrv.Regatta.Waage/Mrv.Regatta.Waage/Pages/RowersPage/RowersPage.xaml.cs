using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mrv.Regatta.Waage.Pages.RowersPage
{
    /// <summary>
    /// Interaction logic for RowersPage.xaml
    /// </summary>
    public partial class RowersPage : Page
    {
        private RowersPageViewModel _vm;

        public RowersPage()
        {
            InitializeComponent();

            _vm = new RowersPageViewModel();
            this.DataContext = _vm;

            var clubs = Data.Instance.DbClubs;

            var persons = Data.Instance.DbRowers.Select(r => new RowersPageViewModel.Person()
            {
                Id = (int)r.RID,
                LastName = r.RName,
                FirstName = r.RVorname,
                Club = clubs.SingleOrDefault(c => c.VIDVerein == r.RVerein)?.VVereinsnamenKurz ?? "#Verein nicht gefunden!#",
                Year = Convert.ToInt16(r.RJg).ToString(),
                Sex = r.Geschlecht.ToString().Equals("w", StringComparison.OrdinalIgnoreCase) ? Sex.Female : Sex.Male
            });

            _vm.Persons = new System.Collections.ObjectModel.ObservableCollection<RowersPageViewModel.Person>();
            foreach(var person in persons)
            {
                _vm.Persons.Add(person);
            }

            lstPersons.ItemsSource = _vm.Persons;

            // Filter einrichten
            var collectionView = CollectionViewSource.GetDefaultView(lstPersons.ItemsSource) as CollectionView;
            collectionView.Filter = CustomFilter; // <- This makes sure that every time there is an event trigger, it tries refreshing the view it needs to call the filter.
        }

        /// <summary>
        /// Customs filter für die Listbox.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        private bool CustomFilter(object obj)
        {
            if (string.IsNullOrWhiteSpace(txtFilter.Text))
            {
                // keine Filter in der Textbox eingegeben
                return true;
            }
            else
            {
                var person = (RowersPageViewModel.Person)obj;
                if (person.FullName.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (person.Year.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                if (person.Club.IndexOf(txtFilter.Text, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                return false;
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the txtFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        private void txtFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            CollectionViewSource.GetDefaultView(lstPersons.ItemsSource).Refresh();
        }

        /// <summary>
        /// Handles the Click event of the cmdOk control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void cmdOk_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrentUser();
        }

        /// <summary>
        /// Selects the current user from the listbox.
        /// </summary>
        private void SelectCurrentUser()
        {
            if (lstPersons.SelectedItem == null)
            {
                MessageBox.Show("Kein Eintrag gewählt!");
                return;
            }

            var person = (RowersPageViewModel.Person)lstPersons.SelectedItem;
            SwitchToPerson(person.Id);
        }

        /// <summary>
        /// Handles the double click event in the listbox.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void DoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            var person = ((ListBoxItem)sender).Content as RowersPageViewModel.Person; //Casting back to the binded Track
            SwitchToPerson(person.Id);
        }

        /// <summary>
        /// Switches to the selected person.
        /// </summary>
        /// <param name="id">The identifier.</param>
        private void SwitchToPerson(int id)
        {
            var RowerPage = new RowerPage.RowerPage(id);
            Data.Instance.MainContent.Content = RowerPage;
        }

        /// <summary>
        /// Handles the Loaded event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            txtFilter.Focus();
        }

        /// <summary>
        /// Handles the PreviewKeyDown event of the txtFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        private void txtFilter_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                lstPersons.Focus();
            }
        }

        private void lstPersons_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Return) || (e.Key == Key.Enter))
            {
                SelectCurrentUser();
            }
        }
    }
}

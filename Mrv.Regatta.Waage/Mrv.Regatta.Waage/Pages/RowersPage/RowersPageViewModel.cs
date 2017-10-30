using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mrv.Regatta.Waage.Pages.RowersPage
{

    public class RowersPageViewModel : ViewModelBase.ViewModelBase
    {
        /// <summary>
        /// Gets or sets the persons.
        /// </summary>
        /// <value>
        /// The persons.
        /// </value>
        public ObservableCollection<Person> Persons { get; set; }

        public class Person
        {
            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }

            /// <summary>
            /// Gets or sets the last name.
            /// </summary>
            /// <value>
            /// The last name.
            /// </value>
            public string LastName { get; set; }

            /// <summary>
            /// Gets or sets the first name.
            /// </summary>
            /// <value>
            /// The first name.
            /// </value>
            public string FirstName { get; set; }

            /// <summary>
            /// Gets the full name ("LastName, FirstName")
            /// </summary>
            /// <value>
            /// The full name.
            /// </value>
            public string FullName
            {
                get
                {
                    return $"{LastName}, {FirstName}";
                }
            }

            /// <summary>
            /// Gets or sets the club.
            /// </summary>
            /// <value>
            /// The club.
            /// </value>
            public string Club { get; set; }

            /// <summary>
            /// Gets or sets the year.
            /// </summary>
            /// <value>
            /// The year.
            /// </value>
            public string Year { get; set; }

            /// <summary>
            /// Gets or sets the sex.
            /// </summary>
            /// <value>
            /// The sex.
            /// </value>
            public Sex Sex { get; set; }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this instance.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString()
            {
                return FullName;
            }
        }
    }
}

using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ViewModelBase.CollectionExtensions
{
    public static class CollectionExtensions
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> coll)
        {
            if (coll == null)
            {
                return null;
            }

            var c = new ObservableCollection<T>();
            foreach (var e in coll)
            {
                c.Add(e);
            }
            return c;
        }

        /*
        public static ObservableCollection<T> ToObservableCollection<T>(this List<T> coll)
        {
            var c = new ObservableCollection<T>();
            foreach (var e in coll)
            {
                c.Add(e);
            }
            return c;
        }
        */
    }
}

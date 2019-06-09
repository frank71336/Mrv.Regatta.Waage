using System;

namespace Mrv.Regatta.Waage.DbData
{
    public class RowerData
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public Gender Gender{ get; set; }
        public DateTime DateOfBirth { get; set; }
        public string ClubTitleLong { get; set; }
        public string ClubTitleShort { get; set; }
    }
}

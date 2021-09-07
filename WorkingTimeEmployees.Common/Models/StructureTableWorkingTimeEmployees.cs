using System;

namespace WorkingTimeEmployees.Common.Models
{
    public class StructureTableWorkingTimeEmployees
    {
        public int Id_Employees { get; set; }
        public DateTime RegistrationDateTime { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }

    }
}

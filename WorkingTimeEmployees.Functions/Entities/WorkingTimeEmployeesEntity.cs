using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace WorkingTimeEmployees.Functions.Entities
{
    public class WorkingTimeEmployeesEntity : TableEntity
    {
        public int Id_Employees { get; set; }
        public DateTime RegistrationDateTime { get; set; }
        public int Type { get; set; }
        public bool Consolidated { get; set; }

    }
}

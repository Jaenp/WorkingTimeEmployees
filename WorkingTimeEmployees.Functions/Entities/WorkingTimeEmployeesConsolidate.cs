using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace WorkingTimeEmployees.Functions.Entities
{
    public class WorkingTimeEmployeesConsolidate : TableEntity
    {
        public int Id_Employees { get; set; }
        public DateTime TimeRegisterEmployees { get; set; }
        public double MinutesWorked { get; set; }

    }
}

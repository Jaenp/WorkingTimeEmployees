using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Threading.Tasks;

namespace WorkingTimeEmployees.Functions.Entities
{
    public static class ScheduledFunctionsWorkingTimeEmployees
    {
        [FunctionName("ScheduledFunctionsWorkingTimeEmployees")]
        public static async Task Run(
            [TimerTrigger("0 */59 * * * *")] TimerInfo myTimer,
            [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable workingTimeEmployeesTable,
            [Table("WorkingTimeEmployeesConsolidated", Connection = "AzureWebJobsStorage")] CloudTable workingTimeEmployeesTable2,
            ILogger log)
        {
            // Consolidate table WorkingTimeEmployees
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingTimeEmployeesEntity> query = new TableQuery<WorkingTimeEmployeesEntity>().Where(filter);
            TableQuerySegment<WorkingTimeEmployeesEntity> allWorkingTimeEmployeesEntityTable = await workingTimeEmployeesTable.ExecuteQuerySegmentedAsync(query, null);

            //Consolidate table WorkingTimeEmployeesConsolidate
            TableQuery<WorkingTimeEmployeesConsolidate> queryConsolidate = new TableQuery<WorkingTimeEmployeesConsolidate>();
            TableQuerySegment<WorkingTimeEmployeesConsolidate> allWorkingTimeEmployeesConsolidateEntity = await workingTimeEmployeesTable2.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            foreach (WorkingTimeEmployeesEntity item in allWorkingTimeEmployeesEntityTable)
            {
                if (!string.IsNullOrEmpty(item.Id_Employees.ToString()) && item.Type == 0)
                {
                    foreach (WorkingTimeEmployeesEntity itemtwo in allWorkingTimeEmployeesEntityTable)
                    {
                        TimeSpan dateCalculated = (itemtwo.RegistrationDateTime - item.RegistrationDateTime);
                        if (itemtwo.Id_Employees.Equals(item.Id_Employees) && itemtwo.Type == 1)
                        {
                            await saveRegisterInDb(itemtwo, workingTimeEmployeesTable);
                            await saveRegisterInDb(item, workingTimeEmployeesTable);
                            await ConsolidateTableWorkingTimeEmployees(allWorkingTimeEmployeesConsolidateEntity, item, itemtwo, dateCalculated, workingTimeEmployeesTable2);
                        }
                    }
                }
            }
        }

        public static WorkingTimeEmployeesEntity saveRegister(WorkingTimeEmployeesEntity register)
        {
            WorkingTimeEmployeesEntity checkEmployees = new WorkingTimeEmployeesEntity
            {
                Id_Employees = register.Id_Employees,
                RegistrationDateTime = register.RegistrationDateTime,
                Type = register.Type,
                Consolidated = true,
                PartitionKey = "WORKINGTIMEEMPLOYEES",
                RowKey = register.RowKey,
                ETag = "*"
            };
            return checkEmployees;
        }

        public static async Task ConsolidateTableWorkingTimeEmployees(TableQuerySegment<WorkingTimeEmployeesConsolidate> consolidateWorkingTimeEmplpoyeesEntity, WorkingTimeEmployeesEntity item, WorkingTimeEmployeesEntity itemTwo, TimeSpan dateCalculated, CloudTable workingTimeTable2)
        {
            if (consolidateWorkingTimeEmplpoyeesEntity.Results.Count == 0)
            {
                WorkingTimeEmployeesConsolidate checkConsolidate = new WorkingTimeEmployeesConsolidate
                {
                    Id_Employees = item.Id_Employees,
                    TimeRegisterEmployees = item.RegistrationDateTime,
                    MinutesWorked = dateCalculated.TotalMinutes,
                    PartitionKey = "WORKINGCONSOLIDATED",
                    RowKey = Guid.NewGuid().ToString(),
                    ETag = "*"
                };

                await saveRegisterInDb(checkConsolidate, workingTimeTable2);
            }
            else
            {
                foreach (WorkingTimeEmployeesConsolidate itemConsolidation in consolidateWorkingTimeEmplpoyeesEntity)
                {
                    if (itemConsolidation.Id_Employees == item.Id_Employees)
                    {
                        WorkingTimeEmployeesConsolidate checkConsolidateFor = new WorkingTimeEmployeesConsolidate
                        {
                            Id_Employees = itemConsolidation.Id_Employees,
                            TimeRegisterEmployees = itemConsolidation.TimeRegisterEmployees,
                            MinutesWorked = (double)(itemConsolidation.MinutesWorked + dateCalculated.TotalMinutes),
                            PartitionKey = itemConsolidation.PartitionKey,
                            RowKey = itemConsolidation.RowKey,
                            ETag = "*"
                        };

                        await saveRegisterInDb(checkConsolidateFor, workingTimeTable2);
                    }
                    else
                    {
                        WorkingTimeEmployeesConsolidate checkConsolidateFor = new WorkingTimeEmployeesConsolidate

                        {
                            Id_Employees = item.Id_Employees,
                            TimeRegisterEmployees = item.RegistrationDateTime,
                            MinutesWorked = dateCalculated.TotalMinutes,
                            PartitionKey = "WORKINGCONSOLIDATED",
                            RowKey = Guid.NewGuid().ToString(),
                            ETag = "*"
                        };

                        await saveRegisterInDb(checkConsolidateFor, workingTimeTable2);
                    }
                }
            }
        }

        public static async Task saveRegisterInDb(WorkingTimeEmployeesConsolidate consolidate, CloudTable workingTimeTable2)
        {
            TableOperation insertConsolidate = TableOperation.Insert(consolidate);
            await workingTimeTable2.ExecuteAsync(insertConsolidate);
        }

        public static async Task saveRegisterInDb(WorkingTimeEmployeesEntity TimeEmployees, CloudTable workingTimeTable)
        {
            TableOperation updateCheckEntityEmployees = TableOperation.Replace(saveRegister(TimeEmployees));
            await workingTimeTable.ExecuteAsync(updateCheckEntityEmployees);
        }

    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;

namespace WorkingTimeEmployees.Functions.Entities
{
    public static class ScheduledFunctionsWorkingTimeEmployees
    {
        [FunctionName("ScheduledFunctionsWorkingTimeEmployees")]
        public static async Task Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable workingTimeEmployeesTable,
            [Table("WorkingTimeEmployeesConsolidated", Connection = "AzureWebJobsStorage")] CloudTable workingTimeEmployeesTable2,
            ILogger log)
        {
            // CheckEntity = Tabla 1
            string filter = TableQuery.GenerateFilterConditionForBool("Consolidated", QueryComparisons.Equal, false);
            TableQuery<WorkingTimeEmployeesEntity> query = new TableQuery<WorkingTimeEmployeesEntity>().Where(filter);
            TableQuerySegment<WorkingTimeEmployeesEntity> allWorkingTimeEmployeesEntityTable = await workingTimeEmployeesTable.ExecuteQuerySegmentedAsync(query, null);

            //CheckConsolidateEntity = Tabla 2
            TableQuery<WorkingTimeEmployeesConsolidate> queryConsolidate = new TableQuery<WorkingTimeEmployeesConsolidate>();
            TableQuerySegment<WorkingTimeEmployeesConsolidate> allWorkingTimeEmployeesConsolidateEntity = await workingTimeEmployeesTable2.ExecuteQuerySegmentedAsync(queryConsolidate, null);

            log.LogInformation($"Entrando al primer foreach");
            foreach (WorkingTimeEmployeesEntity item in allWorkingTimeEmployeesEntityTable)
            {
                log.LogInformation($"Este es el primer if");
                if (!string.IsNullOrEmpty(item.Id_Employees.ToString()) && item.Type == 0)
                {
                    log.LogInformation($"Este es el segundo foreach");
                    foreach (WorkingTimeEmployeesEntity itemtwo in allWorkingTimeEmployeesEntityTable)
                    {
                        TimeSpan dateCalculated = (itemtwo.RegistrationDateTime - item.RegistrationDateTime);
                        log.LogInformation($"Este es el tercer foreach");
                        if (itemtwo.Id_Employees.Equals(item.Id_Employees) && itemtwo.Type == 1)
                        {
                            log.LogInformation($"Este es el IDRowKey, {item.RowKey}, {itemtwo.RowKey}");

                            WorkingTimeEmployeesEntity check = new WorkingTimeEmployeesEntity
                            {
                                Id_Employees = itemtwo.Id_Employees,
                                RegistrationDateTime = itemtwo.RegistrationDateTime,
                                Type = itemtwo.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIMEEMPLOYEES",
                                RowKey = itemtwo.RowKey,
                                ETag = "*"
                            };
                            WorkingTimeEmployeesEntity checkEmployees = new WorkingTimeEmployeesEntity
                            {
                                Id_Employees = item.Id_Employees,
                                RegistrationDateTime = item.RegistrationDateTime,
                                Type = item.Type,
                                Consolidated = true,
                                PartitionKey = "WORKINGTIMEEMPLOYEES",
                                RowKey = item.RowKey,
                                ETag = "*"
                            };

                            TableOperation updateCheckEntity = TableOperation.Replace(check);
                            await workingTimeEmployeesTable.ExecuteAsync(updateCheckEntity);
                            
                            TableOperation updateCheckEntityEmployees = TableOperation.Replace(checkEmployees);
                            await workingTimeEmployeesTable.ExecuteAsync(updateCheckEntityEmployees);
                            await ConsolidateTableWorkingTimeEmployees(allWorkingTimeEmployeesConsolidateEntity, item, itemtwo, dateCalculated, workingTimeEmployeesTable2);
                        }
                    }
                }
            }
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

                TableOperation insertCheckConsolidate = TableOperation.Insert(checkConsolidate);
                await workingTimeTable2.ExecuteAsync(insertCheckConsolidate);
            }
            else
            {
                foreach (WorkingTimeEmployeesConsolidate itemConsolidation in consolidateWorkingTimeEmplpoyeesEntity)
                {
                    //log.LogInformation("Actualizando consolidado segunda tabla");
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

                        TableOperation insertConsolidate = TableOperation.Replace(checkConsolidateFor);
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
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

                        TableOperation insertConsolidate = TableOperation.Insert(checkConsolidateFor);
                        await workingTimeTable2.ExecuteAsync(insertConsolidate);
                    }
                }
            }
        }
    }
}
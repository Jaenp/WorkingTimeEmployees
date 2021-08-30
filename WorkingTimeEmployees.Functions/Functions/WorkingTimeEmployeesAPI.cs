using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using WorkingTimeEmployees.Common.Models;
using WorkingTimeEmployees.Common.Reponses;
using WorkingTimeEmployees.Functions.Entities;

namespace WorkingTimeEmployees.Functions.Functions
{
    public static class WorkingTimeEmployeesAPI
    {
        [FunctionName(nameof(CreatedWorkingTimeEmployees))]
        public static async Task<IActionResult> CreatedWorkingTimeEmployees(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "WorkingTimeEmployees")] HttpRequest req,
            [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable WorkingTimeEmployeesTable,

            ILogger log)
        {
            log.LogInformation("Recieved a new Working Time Employees");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            StructureTableWorkingTimeEmployees structureTableWorkingTimeEmployees = JsonConvert.DeserializeObject<StructureTableWorkingTimeEmployees>(requestBody);
            if (string.IsNullOrEmpty(structureTableWorkingTimeEmployees?.Id_Employees.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The Request must have a employee identification"
                });
            }
            WorkingTimeEmployeesEntity workingTimeEmployeesEntity = new WorkingTimeEmployeesEntity
            {
                Id_Employees = structureTableWorkingTimeEmployees.Id_Employees,
                RegistrationDateTime = DateTime.UtcNow,
                Type = structureTableWorkingTimeEmployees.Type,
                Consolidated = false,
                PartitionKey = "WORKINGTIMEEMPLOYEES",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

            TableOperation addOperationWorkingTimeEmployess = TableOperation.Insert(workingTimeEmployeesEntity);
            await WorkingTimeEmployeesTable.ExecuteAsync(addOperationWorkingTimeEmployess);

            log.LogInformation("Add a new employee in table");

            return new OkObjectResult(new Response
            {
                IdEmployees = workingTimeEmployeesEntity.Id_Employees,
                RegistrationDateTime = workingTimeEmployeesEntity.RegistrationDateTime,
                Message = "The information has been successfully registered"
            });
        }
    
    
    }
}

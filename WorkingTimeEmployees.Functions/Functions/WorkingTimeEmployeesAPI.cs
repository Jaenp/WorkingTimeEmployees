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


        [FunctionName(nameof(UpdatedWorkingTimeEmployees))]
        public static async Task<IActionResult> UpdatedWorkingTimeEmployees(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "WorkingTimeEmployees/{Id_Employees}")] HttpRequest req,
            [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable WorkingTimeEmployeesTable,
            string Id_Employees,
            ILogger log)
        {
            log.LogInformation($"The employee record will be updated: {Id_Employees}, in the table WorkingTimeEmployees");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            StructureTableWorkingTimeEmployees structureTableWorkingTimeEmployees = JsonConvert.DeserializeObject<StructureTableWorkingTimeEmployees>(requestBody);

            TableOperation findOperationWorkingTimeEmployees = TableOperation.Retrieve<WorkingTimeEmployeesEntity>("WORKINGTIMEEMPLOYEES", Id_Employees);
            TableResult findResultWorkingTimeEmployees = await WorkingTimeEmployeesTable.ExecuteAsync(findOperationWorkingTimeEmployees);

            //Valid if the id employee was found successfully
            if (findResultWorkingTimeEmployees.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = $"The employee identified with the identification number: {Id_Employees}, was not found."
                });
            }

            WorkingTimeEmployeesEntity workingTimeEmployeesEntity = (WorkingTimeEmployeesEntity)findResultWorkingTimeEmployees.Result;
            workingTimeEmployeesEntity.RegistrationDateTime = structureTableWorkingTimeEmployees.RegistrationDateTime;
            workingTimeEmployeesEntity.Type = structureTableWorkingTimeEmployees.Type;

            //Validate the registration date and time
            if (string.IsNullOrEmpty(structureTableWorkingTimeEmployees.RegistrationDateTime.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The indicated request must comply with the following format in order to make the change, YYYY-MM-DD:HH:MM:SS",
                });
            }

            // Validate Type
            if (string.IsNullOrEmpty(structureTableWorkingTimeEmployees.Type.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "The data entered is not valid in the Type field."
                });
            }

            
            TableOperation updateOperationWorkingTimeEmployess = TableOperation.Replace(workingTimeEmployeesEntity);
            await WorkingTimeEmployeesTable.ExecuteAsync(updateOperationWorkingTimeEmployess);

            log.LogInformation("Employed update in the table Working Time Employed");

            return new OkObjectResult(new Response
            {
                IdEmployees = workingTimeEmployeesEntity.Id_Employees,
                Message = "The information has been successfully updated"
            });
        }

        [FunctionName(nameof(GetAllWorkingTimeEmployees))]
        public static async Task<IActionResult> GetAllWorkingTimeEmployees(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WorkingTimeEmployees")] HttpRequest req,
            [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            ILogger log)
        {
            log.LogInformation("All jobs recieved.");

            TableQuery<WorkingTimeEmployeesEntity> query = new TableQuery<WorkingTimeEmployeesEntity>();
            TableQuerySegment<WorkingTimeEmployeesEntity> workings = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            log.LogInformation("Retrieved all workings");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved all workings",
                Result = workings
            });
        }

        [FunctionName(nameof(GetWorkingTimeEmployeesById))]
        public static IActionResult GetWorkingTimeEmployeesById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WorkingTimeEmployees/{Id_Employees}")] HttpRequest req,
            [Table("WorkingTimeEmployees", "WORKINGTIMEEMPLOYEES", "{Id_Employees}", Connection = "AzureWebJobsStorage")] WorkingTimeEmployeesEntity workingTimeEmployeesEntity,
            string Id_Employees,
            ILogger log)
        {
            log.LogInformation($"Get working by Id: {Id_Employees}, recieved.");

            if (workingTimeEmployeesEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            log.LogInformation($"Working: {workingTimeEmployeesEntity.RowKey}, retrieved.");

            return new OkObjectResult(new Response
            {
                Message = "Retrieved working",
                Result = workingTimeEmployeesEntity
            });
        }

        [FunctionName(nameof(DeleteWorkingTimeEmployees))]
        public static async Task<IActionResult> DeleteWorkingTimeEmployees(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "WorkingTimeEmployees/{Id_Employees}")] HttpRequest req,
            [Table("WorkingTimeEmployees", "WORKINGTIMEEMPLOYEES", "{Id_Employees}", Connection = "AzureWebJobsStorage")] WorkingTimeEmployeesEntity workingTimeEmployeesEntity,
             [Table("WorkingTimeEmployees", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            string Id_Employees,
            ILogger log)
        {
            log.LogInformation($"Delete working: {Id_Employees}, recieved.");

            if (workingTimeEmployeesEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    Message = "Working not found."
                });
            }

            await workingTimeTable.ExecuteAsync(TableOperation.Delete(workingTimeEmployeesEntity));
            log.LogInformation($"Working: {workingTimeEmployeesEntity.RowKey}, deleted.");

            return new OkObjectResult(new Response
            {
                Message = "Deleted working",
                Result = workingTimeEmployeesEntity
            });
        }

        [FunctionName(nameof(GetWorkingTimeEmployeesByDate))]
        public static async Task<IActionResult> GetWorkingTimeEmployeesByDate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "WorkingTimeEmployeesConsolidate/{DateTimeEmployees}")] HttpRequest req,
            [Table("WorkingTimeEmployeesConsolidated", Connection = "AzureWebJobsStorage")] CloudTable workingTimeTable,
            DateTime DateTimeEmployees,
            ILogger log)
        {
            if (string.IsNullOrEmpty(DateTimeEmployees.ToString()))
            {
                return new BadRequestObjectResult(new ResponseConsolidate
                {
                    Message = "Insert a date valid."
                });
            }

            log.LogInformation("Recieved a new register");
            string filter = TableQuery.GenerateFilterConditionForDate("TimeRegisterEmployees", QueryComparisons.Equal, DateTimeEmployees);
            TableQuery<WorkingTimeEmployeesConsolidate> query = new TableQuery<WorkingTimeEmployeesConsolidate>().Where(filter);
            TableQuerySegment<WorkingTimeEmployeesConsolidate> allWorkingTimeConsolidateEntity = await workingTimeTable.ExecuteQuerySegmentedAsync(query, null);

            if (allWorkingTimeConsolidateEntity == null || allWorkingTimeConsolidateEntity.Results.Count.Equals(0))
            {
                return new OkObjectResult(new ResponseConsolidate
                {
                    Message = "Date not found.",
                });
            }
            else
            {
                return new OkObjectResult(new ResponseConsolidate
                {
                    Message = $"Get all registers from consolidate. Date:{DateTimeEmployees}",
                    Result = allWorkingTimeConsolidateEntity
                });
            }
        }
    }
}

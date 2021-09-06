using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using WorkingTimeEmployees.Common.Models;
using WorkingTimeEmployees.Common.Reponses;
using WorkingTimeEmployees.Functions.Entities;
using WorkingTimeEmployees.Functions.Functions;
using WorkingTimeEmployees.Test.Helpers;
using Xunit;

namespace WorkingTimeEmployees.Test.Tests
{
    public class WorkingTimeEmployeesApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void CreateWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorkingTimeEmployees mockCloudTableWorking = new MockCloudTableWorkingTimeEmployees(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            StructureTableWorkingTimeEmployees workingTimeEmployeesRequest = TestFactory.getWorkingRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(workingTimeEmployeesRequest);

            //Act
            IActionResult response = await WorkingTimeEmployeesAPI.CreatedWorkingTimeEmployees(request, mockCloudTableWorking, logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]
        public async void UpdateWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorkingTimeEmployees mockCloudTableWorking = new MockCloudTableWorkingTimeEmployees(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            StructureTableWorkingTimeEmployees workingRequest = TestFactory.getWorkingRequest();
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee, workingRequest);

            //Act
            IActionResult response = await WorkingTimeEmployeesAPI.UpdatedWorkingTimeEmployees(request, mockCloudTableWorking, IdEmployee.ToString(), logger);


            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]

        public async void DeleteWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorkingTimeEmployees mockCloudTableWorking = new MockCloudTableWorkingTimeEmployees(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee);
            WorkingTimeEmployeesEntity workingEntity = TestFactory.GetWorkingEntity();

            //Act
            IActionResult response = await WorkingTimeEmployeesAPI.DeleteWorkingTimeEmployees(request, workingEntity, mockCloudTableWorking, IdEmployee.ToString(), logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        [Fact]

        public void GetWorkingById_Should_Return_200()
        {
            //Arrenge
            //MockCloudTableWorking mockCloudTableWorking = new MockCloudTableWorking(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Guid IdEmployee = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(IdEmployee);
            WorkingTimeEmployeesEntity workingEntity = TestFactory.GetWorkingEntity();

            //Act
            IActionResult response = WorkingTimeEmployeesAPI.GetWorkingTimeEmployeesById(request, workingEntity, IdEmployee.ToString(), logger);



            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }

        [Fact]
        public async void GetAllWorking_Should_Return_200()
        {
            //Arrenge
            MockCloudTableWorkingTimeEmployees mockCloudTableWorking = new MockCloudTableWorkingTimeEmployees(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            //WorkingTable workingRequest = TestFactory.getWorkingRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //Act
            IActionResult response = await WorkingTimeEmployeesAPI.GetAllWorkingTimeEmployees(request, mockCloudTableWorking, logger);


            //Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }

}


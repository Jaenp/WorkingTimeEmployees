﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using WorkingTimeEmployees.Common.Models;
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
    }
}
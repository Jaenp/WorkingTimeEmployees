﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using WorkingTimeEmployees.Common.Models;
using WorkingTimeEmployees.Functions.Entities;

namespace WorkingTimeEmployees.Test.Helpers
{
    public class TestFactory
    {
        public static WorkingTimeEmployeesEntity GetWorkingEntity()
        {
            return new WorkingTimeEmployeesEntity
            {
                ETag = "*",
                PartitionKey = "WORKINGTIME",
                RowKey = Guid.NewGuid().ToString(),
                Id_Employees = 1,
                RegistrationDateTime = DateTime.UtcNow,
                Type = 0,
                Consolidated = false
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid IdEmployee, StructureTableWorkingTimeEmployees workingRequest)
        {
            string request = JsonConvert.SerializeObject(workingRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{IdEmployee}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid IdEmployee)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{IdEmployee}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(StructureTableWorkingTimeEmployees workingRequest)
        {
            string request = JsonConvert.SerializeObject(workingRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static StructureTableWorkingTimeEmployees getWorkingRequest()
        {
            return new StructureTableWorkingTimeEmployees
            {
                Id_Employees = 1,
                Type = 0,
            };
        }
        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }
            return logger;
        }
    }
}

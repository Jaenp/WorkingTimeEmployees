using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WorkingTimeEmployees.Test.Helpers
{
    public class MockCloudTableWorkingTimeEmployees : CloudTable
    {
        public MockCloudTableWorkingTimeEmployees(Uri tableAddress) : base(tableAddress)
        {
        }

        public MockCloudTableWorkingTimeEmployees(Uri tableAbsoluteUri, StorageCredentials credentials) : base(tableAbsoluteUri, credentials)
        {
        }

        public MockCloudTableWorkingTimeEmployees(StorageUri tableAddress, StorageCredentials credentials) : base(tableAddress, credentials)
        {
        }
        public override async Task<TableResult> ExecuteAsync(TableOperation operation)
        {
            return await Task.FromResult(new TableResult
            {
                HttpStatusCode = 200,
                Result = TestFactory.GetWorkingEntity()
            });
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace tests {
    [TestClass]
    public class UnitTest2 {
        [TestMethod]
        public void TestMethod1() {
            var account = "";
            var key = "";
            // Retrieve the storage account from the connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString(account, key));

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            var table = tableClient.GetTableReference("$MetricsHourPrimaryTransactionsBlob");
            Assert.IsTrue(table.Exists());
        }
        private string ConnectionString(string accountName, string accountKey) {
            return "DefaultEndpointsProtocol=http;" + "AccountName=" + accountName + ";" + "AccountKey=" + accountKey;
        }
    }
}

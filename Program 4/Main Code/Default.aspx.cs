using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;

namespace Program4
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        //Load Data Button
        protected async void Button1_Click(object sender, EventArgs e)
        {
            //Website will load data from object stored at given URL: 
            string siteURL = "https://css490.blob.core.windows.net/lab4/input.txt";
            string siteHTML = "";
            CloudStorageAccount storageAccount = null;
            using (var client = new WebClient())
            {
                siteHTML = client.DownloadString(siteURL);
                //Copy test file into object storage (blob):
                string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;  //Get connection string from Web.config
                storageAccount = CloudStorageAccount.Parse(connectionString);  //Use storage connection string to set storageAccount
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();  //Create CloudBloB Client
                CloudBlobContainer blobContainer = blobClient.GetContainerReference("program4container"); //Get container (create if doesn't exist)
                await blobContainer.CreateIfNotExistsAsync(); 
                BlobContainerPermissions permissions = blobContainer.GetPermissions();  //Set permissions to public
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                blobContainer.SetPermissions(permissions);
                CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference("superFile");  //Get reference to superFile and upload text
                await cloudBlockBlob.UploadTextAsync(siteHTML);
                TextBox3.Text += "File copied into blob. " + Environment.NewLine;
            }
            //Parse test file and load into <Key, Value> NoSQL store (Azure Tables):
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();  //Get CloudTableClient and CloudTable (create if doesn't exist)
            CloudTable table = tableClient.GetTableReference("program4table");
            bool ifWorkingPrintStatus = true;
            try
            {
                await table.CreateIfNotExistsAsync();
            }
            catch (StorageException message)
            {
                if (message.RequestInformation.HttpStatusCode == 409)  //The table takes 40 seconds to delete, so clicking the "Load" button won't work right away 
                {
                    TextBox3.Text += "The table is being deleted. Please wait 40 seconds after pushing the Clear Data button and try again." + Environment.NewLine;
                    ifWorkingPrintStatus = false;
                }
            }

            siteHTML = Regex.Replace(siteHTML, @"\s+", " ");  //Use Regex to remove extra spaces
            var components = siteHTML.Split(' ');  //Split on those spaces and put contents into string[]
            var blocks = components.Select(component => component.Split('='));  //For each component of the above string[] split agin on = sign
            int i = 0;  //Counter
            bool nameCreated = false;  //Bool to track if both lastName and firstName set as partitionKey and rowKey
            string lastName = "";  //Keep track of the lastName
            DynamicTableEntity entityX = null;  //DynamicTableEntity and TableOperation for uploading to Azure Tables
            TableOperation insertOperationX = null;
            foreach (var block in blocks)  //Go through every block in the string[] array
            {
                if (nameCreated == false)  //Create a new DynamicTableEntity if the lastName and firstName haven't been set as partitionKey and rowKey
                {
                    entityX = new DynamicTableEntity();
                }
                if (block.Length == 1)  //If the block has no = sign it's either a lastName or a firstName
                {
                    if (i % 2 != 0)  //If the counter for names isn't even, it means you found both the lastName and firstName and can set values
                    {
                        nameCreated = true;
                        entityX.PartitionKey = lastName;
                        entityX.RowKey = block[0];
                        entityX.Timestamp = DateTimeOffset.Now;
                        i++;
                    }
                    else  //Once the values are set you can upload the entity to the table and keep track of lastName for next entity
                    {
                        nameCreated = false;
                        if (i != 0)
                        {
                            insertOperationX = TableOperation.InsertOrMerge(entityX);
                            try
                            {
                                table.Execute(insertOperationX);
                            }
                            catch (Exception message)
                            {
                                //TextBox3.Text += message.Message + Environment.NewLine;
                            }
                        }
                        lastName = block[0];
                        i++;
                    }
                }
                else
                {
                    if (nameCreated == true)
                    {
                        entityX.Properties.Add(block[0], new EntityProperty(block[1]));
                    }
                }
            }
            insertOperationX = TableOperation.InsertOrMerge(entityX);  //Upload to Azure Tables
            try
            {
                if (ifWorkingPrintStatus == true)  //If table isn't being loaded or other error print message
                {
                    TextBox3.Text += "Data parsed and uploaded to Azure Tables. " + Environment.NewLine;
                    table.Execute(insertOperationX);
                }
            }
            catch (Exception message)
            {
                //TextBox3.Text += message.Message + Environment.NewLine;
            }
            TextBox3.Text += Environment.NewLine;
        }

        //Clear Data Button
        protected async void Button2_Click(object sender, EventArgs e)
        {
            //Blob removed from object store
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;  //Get connection string from Web.config
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);  //Use storage connection string to set storageAccount            
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("program4container");
            CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference("superFile");
            await cloudBlockBlob.DeleteIfExistsAsync();
            TextBox3.Text += "Blob deleted. " + Environment.NewLine;

            //NoSQL table emptied or removed
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("program4table");
            await table.DeleteIfExistsAsync();
            TextBox3.Text += "Table deleted. " + Environment.NewLine;
            TextBox3.Text += Environment.NewLine;
        }

        //Query Button
        protected void Button3_Click(object sender, EventArgs e)
        {
            //Type either first, last, or both names and show results
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;  //Get connection string from Web.config
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);  //Use storage connection string to set storageAccount      
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            if (tableClient.GetTableReference("program4table").Exists())  //If table exists
            {
                CloudTable table = tableClient.GetTableReference("program4table");  //Get reference of it and text box inputs
                string firstName = TextBox1.Text;
                string lastName = TextBox2.Text;
                TextBox3.Text += "Querying Azure Tables. " + Environment.NewLine;

                if (firstName != "" && lastName != "")  //If both firstName and lastName entered
                {
                    TableOperation retrieveOperation = TableOperation.Retrieve(lastName, firstName);
                    TableResult query = table.Execute(retrieveOperation);

                    if (query.Result != null)  //If there are results, print out the information
                    {
                        TextBox3.Text += "First and last name search result: " + Environment.NewLine;
                        DynamicTableEntity currentEntity = (DynamicTableEntity)query.Result;
                        TextBox3.Text += currentEntity.PartitionKey + " " + currentEntity.RowKey + " ";
                        var iDictionary = currentEntity.Properties;
                        foreach (KeyValuePair<string, EntityProperty> kvp in iDictionary)
                        {
                            TextBox3.Text += kvp.Key + "=" + kvp.Value.StringValue + " ";
                            TextBox3.Text += Environment.NewLine;
                        }
                    }
                    else  //If there are not results, inform user
                    {
                        TextBox3.Text += "First and Last Name search returned no results. " + Environment.NewLine;
                    }
                }
                else if (firstName == "" && lastName != "")  //If lastName entered
                {
                    TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, lastName));
                    var test = table.ExecuteQuery(query);
                    if (test.ToList().Count == 0)  //If there are not results, inform user
                    {
                        TextBox3.Text += "Last name search returned no results. " + Environment.NewLine;
                    }
                    else  //If there are results, print out the information
                    {
                        TextBox3.Text += "Last name search results: " + Environment.NewLine;
                        foreach (DynamicTableEntity entity in table.ExecuteQuery(query))
                        {
                            TextBox3.Text += entity.PartitionKey + " " + entity.RowKey + " ";
                            var iDictionary = entity.Properties;
                            foreach (KeyValuePair<string, EntityProperty> kvp in iDictionary)
                            {
                                TextBox3.Text += kvp.Key + "=" + kvp.Value.StringValue + " ";
                            }
                            TextBox3.Text += Environment.NewLine;
                        }
                    }
                }
                else if (firstName != "" && lastName == "")  //If firstName entered
                {
                    TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, firstName));
                    var test = table.ExecuteQuery(query);
                    if (test.ToList().Count == 0)  //If there are not results, inform user
                    {
                        TextBox3.Text += "First name search returned no results. " + Environment.NewLine;
                    }
                    else  //If there are results, print out the information
                    {
                        TextBox3.Text += "First name search results: " + Environment.NewLine;
                        foreach (DynamicTableEntity entity in table.ExecuteQuery(query))
                        {
                            TextBox3.Text += entity.PartitionKey + " " + entity.RowKey + " ";
                            var iDictionary = entity.Properties;
                            foreach (KeyValuePair<string, EntityProperty> kvp in iDictionary)
                            {
                                TextBox3.Text += kvp.Key + "=" + kvp.Value.StringValue + " ";
                            }
                            TextBox3.Text += Environment.NewLine;
                        }
                    }
                }
                else  //If there nothing is entered in the text boxes
                {
                    TextBox3.Text += "Query Error: No names entered." + Environment.NewLine;
                }
            }
            else  //If the table does not exist, inform user and prompt them to make one by selecting the Load Data button
            {
                TextBox3.Text += "Query Error: Table does not exist. " + Environment.NewLine;
                TextBox3.Text += "Please select Load Data to create Table. " + Environment.NewLine;
            }
            TextBox3.Text += Environment.NewLine;
        }
    }
}
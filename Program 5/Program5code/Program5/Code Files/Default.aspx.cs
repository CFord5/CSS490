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
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System.Text.RegularExpressions;
using Microsoft.WindowsAzure.Storage.Shared.Protocol;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;

namespace Program5
{
    public static class Globals
    {
        public static List<string> movieList = new List<string>();
        public static bool temp = false;
        public static bool isInitialLoad = true;
    }

    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Globals.isInitialLoad == true)
            {
                regenerateTable(tableHelper(cloudHelper()));
            }
        }

        protected CloudStorageAccount cloudHelper()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;  //Get connection string from Web.config
            return CloudStorageAccount.Parse(connectionString);  //Use storage connection string to set storageAccount
        }

        protected CloudTable tableHelper(CloudStorageAccount storageAccount)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();  //Get CloudTableClient and CloudTable (create if doesn't exist)
            return tableClient.GetTableReference("program5table");
        }

        protected void regenerateTable(CloudTable cloudTable)
        {
            Globals.isInitialLoad = false;
            var table = tableHelper(cloudHelper());
            var tableQuery = new TableQuery<DynamicTableEntity>();
            var test = table.ExecuteQuery(tableQuery);
            if (!table.Exists())
            {
                TextBox3.Text = "No movies exist in the database.";
            }
            else if (test.ToList().Count == 0)  //If there are not results, inform user
            {
                ResultTextBox.Text = "No movies exist in the database.";
            }
            else
            {
                foreach (DynamicTableEntity entity in table.ExecuteQuery(tableQuery))
                {
                    if (!Globals.movieList.Contains(entity.RowKey))
                    {
                        Globals.movieList.Add(entity.RowKey);
                    }
                }

                for (int i = 0; i < Globals.movieList.Count(); i++)
                {
                    TableRow webRow = new TableRow();
                    TableCell webCell = new TableCell();
                    webCell.Text = Globals.movieList.ElementAt(i);
                    webRow.Cells.Add(webCell);
                    Table1.Rows.Add(webRow);
                }
            }
        }

        protected async void Button1_Click(object sender, EventArgs e)
        {
            Globals.isInitialLoad = false;
            string baseURL = "http://www.omdbapi.com/?t=";
            string movieTitle = TextBox1.Text;
            if (string.IsNullOrEmpty(movieTitle) || movieTitle[0] == ' ')
            {
                TextBox3.Text = "Must enter a movie title.";
            }
            else
            {
                string appID = "&apikey=70b97952";
                string searchTerm = baseURL + movieTitle + appID;
                string siteString = "";
                CloudStorageAccount storageAccount = null;
                using (var client = new WebClient())
                {
                    TextBox3.Text = "";
                    siteString = client.DownloadString(searchTerm);
                }
                storageAccount = cloudHelper();
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();  //Create CloudBloB Client
                CloudBlobContainer blobContainer = blobClient.GetContainerReference("program5container"); //Get container (create if doesn't exist)
                await blobContainer.CreateIfNotExistsAsync();
                BlobContainerPermissions permissions = blobContainer.GetPermissions();  //Set permissions to public
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                blobContainer.SetPermissions(permissions);
                CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference("program5blob");  //Get reference to superFile and upload text
                await cloudBlockBlob.UploadTextAsync(siteString);

                Rootobject theMovie = JsonConvert.DeserializeObject<Rootobject>(siteString);
                using (var client = new WebClient())
                {
                    TextBox3.Text += "Title: " + theMovie.Title + Environment.NewLine;
                    TextBox3.Text += "Year: " + theMovie.Year + Environment.NewLine;
                    TextBox3.Text += "Rated: " + theMovie.Rated + Environment.NewLine;
                    TextBox3.Text += "Released: " + theMovie.Released + Environment.NewLine;
                    TextBox3.Text += "Runtime: " + theMovie.Runtime + Environment.NewLine;
                    TextBox3.Text += "Genre: " + theMovie.Genre + Environment.NewLine;
                    TextBox3.Text += "Director: " + theMovie.Director + Environment.NewLine;
                    TextBox3.Text += "Writer: " + theMovie.Writer + Environment.NewLine;
                    TextBox3.Text += "Actors: " + theMovie.Actors + Environment.NewLine;
                    TextBox3.Text += "Plot: " + theMovie.Plot + Environment.NewLine;
                    TextBox3.Text += "Language: " + theMovie.Language + Environment.NewLine;
                    TextBox3.Text += "Country: " + theMovie.Country + Environment.NewLine;
                    TextBox3.Text += "Awards: " + theMovie.Awards + Environment.NewLine;
                    TextBox3.Text += "Metascore: " + theMovie.Metascore + Environment.NewLine;
                    TextBox3.Text += "imdbRating: " + theMovie.imdbRating + Environment.NewLine;
                    TextBox3.Text += "imdbVotes: " + theMovie.imdbVotes + Environment.NewLine;
                    TextBox3.Text += "imdbID: " + theMovie.imdbID + Environment.NewLine;
                    TextBox3.Text += "Type: " + theMovie.Type + Environment.NewLine;
                    TextBox3.Text += "DVD: " + theMovie.DVD + Environment.NewLine;
                    TextBox3.Text += "BoxOffice: " + theMovie.BoxOffice + Environment.NewLine;
                    TextBox3.Text += "Production: " + theMovie.Production + Environment.NewLine;
                    TextBox3.Text += "Response: " + theMovie.Response + Environment.NewLine;
                }

                CloudTable table = tableHelper(storageAccount);
                try
                {
                    await table.CreateIfNotExistsAsync();
                    DynamicTableEntity test = new DynamicTableEntity();
                    if (Globals.movieList.Contains(theMovie.Title))
                    {
                        TextBox3.Text = "Movie " + theMovie.Title + " already exists in database." + Environment.NewLine;
                    }
                    else if (theMovie.Title == null)
                    {
                        TextBox3.Text = "Movie can't be found." + Environment.NewLine;
                    }
                    else
                    {
                        test.RowKey = theMovie.Title;
                        test.PartitionKey = theMovie.imdbID;
                        test.Timestamp = DateTimeOffset.Now;

                        test.Properties.Add("Year", new EntityProperty(theMovie.Year));
                        test.Properties.Add("Rated", new EntityProperty(theMovie.Rated));
                        test.Properties.Add("Released", new EntityProperty(theMovie.Released));
                        test.Properties.Add("Runtime", new EntityProperty(theMovie.Runtime));
                        test.Properties.Add("Genre", new EntityProperty(theMovie.Genre));
                        test.Properties.Add("Director", new EntityProperty(theMovie.Director));
                        test.Properties.Add("Writer", new EntityProperty(theMovie.Writer));
                        test.Properties.Add("Actors", new EntityProperty(theMovie.Actors));
                        test.Properties.Add("Plot", new EntityProperty(theMovie.Plot));
                        test.Properties.Add("Language", new EntityProperty(theMovie.Language));
                        test.Properties.Add("Country", new EntityProperty(theMovie.Country));
                        test.Properties.Add("Awards", new EntityProperty(theMovie.Awards));
                        test.Properties.Add("Metascore", new EntityProperty(theMovie.Metascore));
                        test.Properties.Add("imdbRating", new EntityProperty(theMovie.imdbRating));
                        test.Properties.Add("imdbVotes", new EntityProperty(theMovie.imdbVotes));
                        test.Properties.Add("Type", new EntityProperty(theMovie.Type));
                        test.Properties.Add("DVD", new EntityProperty(theMovie.DVD));
                        test.Properties.Add("Boxoffice", new EntityProperty(theMovie.BoxOffice));
                        test.Properties.Add("Production", new EntityProperty(theMovie.Production));
                        test.Properties.Add("Response", new EntityProperty(theMovie.Response));

                        TableOperation insertOperation = TableOperation.InsertOrMerge(test);
                        table.Execute(insertOperation);
                    }
                    regenerateTable(table);
                }
                catch (StorageException message)
                {
                    if (message.RequestInformation.HttpStatusCode == 409)  //The table takes 40 seconds to delete, so clicking the "Load" button won't work right away 
                    {
                        TextBox3.Text = "The table is being deleted. Please wait 40 seconds after pushing the Clear Data button and try again." + Environment.NewLine;
                    }
                }
            }

        }

        protected async void Button2_Click(object sender, EventArgs e)
        {
            Globals.isInitialLoad = false;
            CloudTable table = tableHelper(cloudHelper());
            string movieTitle = MovieTitleTextBox.Text;
            string targetAttribute = AttributeTextBox.Text;

            TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, movieTitle));
            var test = table.ExecuteQuery(query);
            if (!table.Exists())
            {
                ResultTextBox.Text = "No movies exist in the database.";
            }
            else if (test.ToList().Count == 0)  //If there are not results, inform user
            {
                ResultTextBox.Text = "Search returned no results. ";
            }
            else //If there are results, print out the information
            {
                foreach (DynamicTableEntity entity in table.ExecuteQuery(query))
                {
                    ResultTextBox.Text += entity.PartitionKey + " " + entity.RowKey + " ";
                    var iDictionary = entity.Properties;
                    foreach (KeyValuePair<string, EntityProperty> kvp in iDictionary)
                    {
                        if (kvp.Key == targetAttribute)
                        {
                            ResultTextBox.Text = kvp.Value.StringValue;
                        }

                    }
                    ResultTextBox.Text += Environment.NewLine;
                }
                regenerateTable(table);
            }  
        }

        protected async void Button3_Click(object sender, EventArgs e)
        {
            Globals.isInitialLoad = false;
            //Blob removed from object store
            string connectionString = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;  //Get connection string from Web.config
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);  //Use storage connection string to set storageAccount
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference("program5container");
            CloudBlockBlob cloudBlockBlob = blobContainer.GetBlockBlobReference("program5blob");
            await cloudBlockBlob.DeleteIfExistsAsync();
            TextBox3.Text = "Blob deleted. " + Environment.NewLine;

            //NoSQL table emptied or removed
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference("program5table");
            await table.DeleteIfExistsAsync();
            TextBox3.Text += "Table deleted. " + Environment.NewLine;
            TextBox3.Text += Environment.NewLine;

            //Clear globals list
            Globals.movieList.Clear();

            regenerateTable(table);
        }

        public class Rootobject
        {
            public string Title { get; set; }
            public string Year { get; set; }
            public string Rated { get; set; }
            public string Released { get; set; }
            public string Runtime { get; set; }
            public string Genre { get; set; }
            public string Director { get; set; }
            public string Writer { get; set; }
            public string Actors { get; set; }
            public string Plot { get; set; }
            public string Language { get; set; }
            public string Country { get; set; }
            public string Awards { get; set; }
            public string Poster { get; set; }
            public string Metascore { get; set; }
            public string imdbRating { get; set; }
            public string imdbVotes { get; set; }
            public string imdbID { get; set; }
            public string Type { get; set; }
            public string DVD { get; set; }
            public string BoxOffice { get; set; }
            public string Production { get; set; }
            public string Website { get; set; }
            public string Response { get; set; }
        }

        public class Rating
        {
            public string Source { get; set; }
            public string Value { get; set; }
        }
    }

}
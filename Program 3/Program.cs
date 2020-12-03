//Author: Chandler Ford
//Class: CSS 490 A
//Last Modified: 10/27/18
//Description: Recursive traversal of directory and backup to the cloud

using System;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace Program3
{
    class Program
    {
        //Main method that connects to Azure, chooses the directory, and calls the recursive method
        static void Main(string[] args)
        {
            //Introduction
            Console.WriteLine("---------------------------");
            Console.WriteLine("|RECURSIVE BACKUP TO AZURE|");
            Console.WriteLine("---------------------------");
            Console.WriteLine("Recursively traverses the files of a directory and make a backup to Microsoft Azure.");
            Console.WriteLine("Requires existing Azure storage account and container. See instructions for details.");
            Console.WriteLine();

            //Program loops
            while (true)
            {
                //User input for connection string and container name
                Console.WriteLine("Copy and paste your storage account connection string [enter nothing or 'exit' to quit]: ");
                string StorageAccountConnectionString = Console.ReadLine();
                Console.WriteLine();
                if (StorageAccountConnectionString == "exit")
                {
                    break;
                }
                else if (StorageAccountConnectionString == "")
                {
                    break;
                }
                Console.WriteLine("Enter the name of your container [enter nothing or 'exit' to quit]: ");
                string RootContainerName = Console.ReadLine();
                Console.WriteLine();
                if (RootContainerName == "exit")
                {
                    break;
                }
                else if (RootContainerName == "")
                {
                    break;
                }

                //Connect to Azure
                CloudStorageAccount storageAccount = null;
                try
                {
                    storageAccount = CloudStorageAccount.Parse(StorageAccountConnectionString);
                }
                catch (FormatException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }

                CloudBlobClient blobClient = null;
                try
                {
                    blobClient = storageAccount.CreateCloudBlobClient();
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }

                CloudBlobContainer rootContainer = null;
                try
                {
                    rootContainer = blobClient.GetContainerReference(RootContainerName);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }

                //Choose directory path
                Console.WriteLine("Enter the file path of the folder you want to backup to Azure [enter nothing or 'exit' to quit]:");
                Console.WriteLine("[C:\\folder\\folder\\etc.]");
                string rootFolderPath = Console.ReadLine();
                if (rootFolderPath == "exit")
                {
                    Console.WriteLine();
                    break;
                }
                else if (rootFolderPath == "")
                {
                    Console.WriteLine();
                    break;
                }
                DirectoryInfo rootFolder = null;
                try
                {
                    rootFolder = new DirectoryInfo(rootFolderPath);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }
                Console.WriteLine();

                //Initial call of recursive traversal method
                try
                {
                    RecursiveTraverse(rootFolder, rootContainer);
                }
                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.Message);
                    Console.WriteLine("---------------------------");
                    continue;
                }
                Console.WriteLine("---------------------------");
            }
        }

        //Recursive traversal method that gets files and subfolders and uploads them to Azure
        static void RecursiveTraverse(DirectoryInfo theFolder, CloudBlobContainer theContainer)
        {
            //Keep track of contents
            FileInfo[] files = null;
            DirectoryInfo[] subFolders = null;

            //Get all files in the folder
            try
            {
                files = theFolder.GetFiles("*.*");
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            //If there are files, look at each
            if (files != null)
            {
                foreach (FileInfo fi in files)
                {
                    //If blob doesn't exist upload file
                    CloudBlockBlob blob = theContainer.GetBlockBlobReference(fi.FullName);
                    if (!blob.Exists())
                    {
                        blob.UploadFromFile(fi.FullName);
                        Console.WriteLine("Uploading file: " + fi.Name);
                    }
                    else
                    {
                        //If modified since upload, reupload file
                        var lastModifiedFile = File.GetLastWriteTime(fi.FullName);
                        var lastModifiedBlob = blob.Properties.LastModified;
                        if (lastModifiedFile > lastModifiedBlob)
                        {
                            blob.UploadFromFile(fi.FullName);
                            Console.WriteLine("Modifying file: " + fi.Name);
                        }
                        else
                        {
                            Console.WriteLine("File already exists: " + fi.Name);
                        }
                    }
                }

                //Get all the subfolders in the folder
                subFolders = theFolder.GetDirectories();
                foreach (DirectoryInfo dirInfo in subFolders)
                {
                    //Recursive call with subfolder
                    Console.WriteLine("Found subfolder: " + dirInfo.Name);
                    RecursiveTraverse(dirInfo, theContainer);
                }
            }
        }
    }
}
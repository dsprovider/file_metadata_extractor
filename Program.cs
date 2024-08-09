using System;
using System.IO;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using System.Collections;

public class FolderIterator
{

    // Create an empty list of strings
    public static List<string> exceptionsList = new List<string>();

    public static void Main(string[] args)
    {

        exceptionsList.Add("CodeSnippet|ErrorMessage");

        // ---------------- 1. Enter folder path - INPUT DIRECTORY ----------------------------------------
        string folderPath = EnterFolderPath();

        // ---------------- 2. Establish connection with SQL Server ----------------------------------------
        // Connection string with Windows authentication
        string connectionString = "Data Source=DESKTOP-30J9E2P\\SQLEXPRESS;Initial Catalog=MyWork_20221213;Integrated Security=True";
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                // Connection opened successfully, perform your database operations here
                connection.Open();
                Console.WriteLine(">> Successfully established connection to DB!");

                // ---------------- 3. Process files from input folder path ----------------------------------------
                string tableName = GenerateTableName();
                CreateDatabaseTable(tableName, connectionString);
                Console.WriteLine(">> " + tableName + " table created!");
                IterateFolder(folderPath, connection, tableName);

                // ---------------- 4. Close connection to SQL Server ----------------------------------------
                connection.Close();
                Console.WriteLine(">> Closing connection to DB...");
            }
        }
        catch (Exception ex)
        {
            // Handle any errors that occurred during the connection or database operations
            Console.WriteLine(">> Error: " + ex.Message);
            //exceptionsList.Add("Database Connection|" + ex.Message);
        }

        // ---------------- 5. Get folder statistics ----------------------------------------
        GetFolderStats(folderPath);

        // ---------------- 6. Print list of exceptions ----------------------------------------
        PrintExceptionsList(exceptionsList);

        //Console.WriteLine(GenerateTableName());

    }
    //===============================================================================================================================================
    //MAIN METHODS ==================================================================================================================================
    //===============================================================================================================================================

    /* ************ READ FOLDER DIRECTORY FROM KEYBOARD ******************************************************************************************************************* */
    public static string EnterFolderPath()
    {
        string folderPath = string.Empty;
        try
        {
            while (true)
            {
                Console.Write(">> Enter a folder path: ");
                folderPath = Console.ReadLine();

                if (Directory.Exists(folderPath))
                {
                    Console.WriteLine(">> Valid directory path entered!");
                    break;
                }
                else
                {
                    Console.WriteLine(">> Invalid directory path. Please try again.");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(">> An error occurred while reading input: " + ex.Message);
            exceptionsList.Add(">> Reading folder path from keyboard|" + ex.Message);
        }
        return folderPath;
    }

    /* ************ RECURSIVELY ITERATE FILES FROM GIVEN DIRECTORY ************************************************************************************ */
    public static void IterateFolder(string folderPath, SqlConnection connection, string tableName)
    {
        // Process current folder
        Console.WriteLine($"[FOLDER] - {folderPath}");

        // Get all subdirectories recursively
        string[] subfolders = Directory.GetDirectories(folderPath);

        foreach (string subfolder in subfolders)
        {
            try
            {
                IterateFolder(subfolder, connection, tableName); // Recursively process each subfolder
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($">> Access to the subfolder '{ex.Message}' is denied.");
                exceptionsList.Add(">> Iterating folder" + "|" + subfolder + " ==> " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($">> An error occurred while processing subfolder: {ex.Message}");
                exceptionsList.Add(">> Iterating folder" + "|" + subfolder + " ==> " + ex.Message);
            }
        }

        // Process files in the current folder
        string[] files = Directory.GetFiles(folderPath);
        foreach (string file in files)
        {
            // Perform your desired operations on each file
            Console.WriteLine($"\t[FILE] - {file}");
            GetFileMetadata(file, connection, tableName);
        }

    }

    /* ************ GET FOLDER STATS ******************************************************************************************************************* */
    public static void GetFolderStats(string folderPath)
    {
        Console.WriteLine();
        Console.WriteLine("================================================");

        int totalFiles = CountFilesInDirectory(folderPath);
        Console.WriteLine($">> Total number of files: {totalFiles}");

        long sizeInBytes = GetDirectorySize(folderPath);
        Console.WriteLine($">> Total directory size (Bytes): {sizeInBytes}");
        double sizeInMegabytes = (double)sizeInBytes / (1024 * 1024);
        Console.WriteLine($">> Total directory size (MBytes): {sizeInMegabytes}");
    }

    /* ************ PRINT LIST OF EXCEPTIONS ********************************************************************************************************** */
    public static void PrintExceptionsList(List<string> list)
    {
        Console.WriteLine();
        Console.WriteLine("================================================");
        int i = 1;
        foreach (string item in list)
        {
            Console.WriteLine(">> Exception (" + i + "): " + item);
            i++;
        }

        Console.WriteLine();
        Console.WriteLine("================================================");

        Console.WriteLine(">> Do you want to export the list of exceptions? (y/n):");
        string exportReply = Console.ReadLine();

        if (exportReply.Equals("y", StringComparison.OrdinalIgnoreCase))
        {
            string userProfile = Environment.GetEnvironmentVariable("USERPROFILE");
            string directoryPath = Path.Combine(userProfile, "Documents", "My Exceptions");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                Console.WriteLine(">> Directory created successfully.");
            }
            else
            {
                Console.WriteLine(">> Directory already exists.");
            }

            //Write exceptions list to file
            DateTime currentTime = DateTime.Now;
            string europeTime = currentTime.ToString("yyyyMMdd_HH-mm-ss-fff");
            string fileName = "ExceptionsList_" + europeTime + ".csv";
            string exportFilePath = Path.Combine(directoryPath, fileName);

            // Write the data to the CSV file
            using (StreamWriter writer = new StreamWriter(exportFilePath))
            {
                foreach (string line in list)
                {
                    writer.WriteLine(line);
                }
            }

            Console.WriteLine(">> CSV file created successfully.");


        }
        else
        {
            Console.WriteLine(">> Googbye then!");
        }

    }


    //===============================================================================================================================================
    // AUXILIARY METHODS ============================================================================================================================ */
    //===============================================================================================================================================

    public static string GenerateTableName()
    {
        DateTime currentTime = DateTime.Now;
        string europeTime = currentTime.ToString("yyyyMMdd_HH-mm-ss-fff");

        return europeTime;
    }

    /* ************ CREATE TABLE IN SQL SERVER GIVEN TABLE NAME ************************************************************************************ */
    public static void CreateDatabaseTable(string tableName, string connectionString)
    {

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();

            string createTableQuery = $"CREATE TABLE [{tableName}] (fName VARCHAR(300),fSize INT, fExtension VARCHAR(50),CreationTime DATETIME,LastAccessTime DATETIME,LastWriteTime DATETIME,DirectoryName VARCHAR(500),Attributes VARCHAR(200),fSignature VARCHAR(100),MD5 VARCHAR(80))";

            using (SqlCommand command = new SqlCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }

    /* ************ RECURSIVELY COUNT FILES FROM GIVEN DIRECTORY ************************************************************************************ */
    public static int CountFilesInDirectory(string directoryPath)
    {
        int count = 0;
        try
        {
            count += Directory.GetFiles(directoryPath).Length;

            string[] subDirectories = Directory.GetDirectories(directoryPath);
            foreach (string subDirectory in subDirectories)
            {
                count += CountFilesInDirectory(subDirectory);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Access denied exception occurred, continue recursion
            exceptionsList.Add("CountFilesInDirectory" + "|" + directoryPath);
        }

        return count;

    }


    /* ************ GET FILE SIZE OF DIRECTORY IN BYTES ************************************************************************************ */
    public static long GetDirectorySize(string directoryPath)
    {
        try
        {
            long totalSize = 0;

            // Get the files and directories within the current directory
            string[] files = Directory.GetFiles(directoryPath);
            string[] directories = Directory.GetDirectories(directoryPath);

            // Calculate the size of each file and add it to the total size
            foreach (string file in files)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle unauthorized access exception (optional)
                    Console.WriteLine($"Access to file '{file}' is denied.");
                    exceptionsList.Add("Calculating directory size" + "|" + directoryPath);
                }
            }

            // Recursively calculate the size of each subdirectory and add it to the total size
            foreach (string directory in directories)
            {
                try
                {
                    totalSize += GetDirectorySize(directory);
                }
                catch (UnauthorizedAccessException)
                {
                    // Handle unauthorized access exception (optional)
                    Console.WriteLine($">> Access to directory '{directory}' is denied.");
                    exceptionsList.Add(">> Calculating directory size" + "|" + directory);
                }
            }

            return totalSize;
        }
        catch (Exception ex)
        {
            Console.WriteLine($">> An error occurred while calculating directory size: {ex.Message}");
            exceptionsList.Add(">> Calculating directory size" + "|" + ex.Message);
            return -1; // Return a negative value to indicate an error
        }

    }

    /* ************ GET FILE SIGNATURE ************************************************************************************ */
    static byte[] ReadFileSignature(string filePath)
    {
        byte[] signature = new byte[4];

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            fs.Read(signature, 0, 4);
        }

        return signature;
    }

    /* ************ GET MD5 FILE HASH ************************************************************************************ */
    public static string CalculateMD5Hash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "");
            }
        }
    }

    /* ************ GET FILE METADATA ************************************************************************************ */
    public static void GetFileMetadata(string filePath, SqlConnection connection, string tableName)
    {

        FileInfo fileInfo = new FileInfo(filePath);
        string fileSignature = string.Empty;
        string md5Hash = string.Empty;

        try
        {
            Console.WriteLine("\t\t" + "[M] - Name: " + fileInfo.Name);
            Console.WriteLine("\t\t" + "[M] - Size: " + fileInfo.Length);
            Console.WriteLine("\t\t" + "[M] - Extension: " + fileInfo.Extension);
            Console.WriteLine("\t\t" + "[M] - Creation Time: " + fileInfo.CreationTime);
            Console.WriteLine("\t\t" + "[M] - Last Access Time: " + fileInfo.LastAccessTime);
            Console.WriteLine("\t\t" + "[M] - Last Write Time: " + fileInfo.LastWriteTime);
            Console.WriteLine("\t\t" + "[M] - Directory Name: " + fileInfo.DirectoryName);
            Console.WriteLine("\t\t" + "[M] - Attributes: " + fileInfo.Attributes);

            byte[] signatureBytes = ReadFileSignature(filePath);
            fileSignature = BitConverter.ToString(signatureBytes).Replace("-", "");
            Console.WriteLine("\t\t" + "[M] - Signature: " + fileSignature);

            md5Hash = CalculateMD5Hash(filePath);
            Console.WriteLine("\t\t" + "[M] - MD5 Hash: " + md5Hash);
            //Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(">> Error: " + ex.Message);
            exceptionsList.Add(">> GetFileMetadata" + "|" + ex.Message);
        }


        try
        {
            // SQL query to insert a record
            string sqlQuery = "INSERT INTO [dbo].[" + tableName + "] (fName, fSize, fExtension, CreationTime, LastAccessTime, LastWriteTime, DirectoryName, Attributes, fSignature, MD5) VALUES (@Value1, @Value2, @Value3, @Value4, @Value5, @Value6, @Value7, @Value8, @Value9, @Value10)";

            // Create a new SqlCommand object
            using (SqlCommand command = new SqlCommand(sqlQuery, connection))
            {
                // Set parameter values
                command.Parameters.AddWithValue("@Value1", fileInfo.Name);
                command.Parameters.AddWithValue("@Value2", fileInfo.Length);
                command.Parameters.AddWithValue("@Value3", fileInfo.Extension);
                command.Parameters.AddWithValue("@Value4", fileInfo.CreationTime);
                command.Parameters.AddWithValue("@Value5", fileInfo.LastAccessTime);
                command.Parameters.AddWithValue("@Value6", fileInfo.LastWriteTime);
                command.Parameters.AddWithValue("@Value7", fileInfo.DirectoryName);
                command.Parameters.AddWithValue("@Value8", fileInfo.Attributes);
                command.Parameters.AddWithValue("@Value9", fileSignature);
                command.Parameters.AddWithValue("@Value10", md5Hash);

                // Execute the query
                int rowsAffected = command.ExecuteNonQuery();
                Console.WriteLine("\t>> Rows affected: " + rowsAffected);
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
            exceptionsList.Add("GetFileMetadata Insertion into DB" + "|" + ex.Message);
        }

    }
}

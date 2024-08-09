📁 File Metadata Extractor Tool

Welcome to the File Metadata Extractor Tool! 🎉 This C# application allows you to extract and analyze metadata from files within a specified folder, including all its subfolders. The extracted metadata is then stored in a SQL Server database for further analysis and record-keeping.



🌟 Features

- Recursive File Processing: 🌀 iterates through all subfolders to find and process files.
  
- Metadata Extraction: 📊 retrieves the following metadata for each file:
  [+] Size 📏
  
  [+] Extension 🔠
  
  [+] Creation Time ⏳
  
  [+] Last Access Time ⌛
  
  [+] Last Write Time 📝
  
  [+] Directory Name 🗂️
  
  [+] Attributes 🔖
  
  [+] Signature ✍️
  
  [+] MD5 Hash 🔐

- SQL Server Integration: 🗃️ stores metadata in a SQL Server database with tables named after timestamps of execution.
  
- Exception Handling: ⚠️ optionally exports exceptions to a CSV file.
  
- Summary Report: 📈 displays total number of files processed and total size (in Bytes and Megabytes) in the console.



🎯 Purpose

This tool is designed for:

1. Preservation: 🛡️ creating snapshots of file metadata to preserve the current state of files.

2. Integrity Checking: 🔍 identifying unauthorized modifications by comparing file timestamps.

3. Signature Analysis: 🔎 verifying file signatures and extensions for consistency and security.



🛠️ Requirements

- .NET 6.0: Ensure you have the .NET 6.0 runtime installed.
  
- NuGet Packages: 📦 You need to install the following package:
  
  [+] System.Data.SqlClient (Version 4.8.5)



🏗️ Installation

1. Clone the Repository:

git clone https://github.com/yourusername/yourrepository.git

cd yourrepository

2. Configure SQL Server

Make sure you have an existing SQL Server database where metadata tables can be created.



🚀 Usage

1. Run the Program:
   
Open the solution in Visual Studio and run the program. It will prompt you to enter the path of the folder containing files you want to process.

2. Handle Exceptions:
   
If exceptions occur during execution, you will be asked if you want to export the exceptions list. If yes, a CSV file will be saved in %userprofile%\Documents\My Exceptions with the current timestamp as the CSV filename.

3. View Summary:
   
After processing, the console will display a summary of the number of files processed and their total size both in bytes and megabytes.



📜 License

This project is licensed under the MIT License.






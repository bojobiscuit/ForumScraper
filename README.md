# ForumScraper
Quickly scraps posts from SHL forums

Uses Dotnet Core and C# to code with HtmlAgilityPack to scrap the pages.

Program.cs starts the console app. Downloads the files, extracts the data, and creates an output file.

To Debug: dotnet build, then dotnet run

To publish:
dotnet publish -c Release -r win10-x64

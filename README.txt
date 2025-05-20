@@@ PROJECT DEPENDENCIES (Software needed to setup the app): @@@

• .NET Core 8
• MS SQL Server 2022 LocalDb (SQLEXPRESS) 


@@@ SETUP: @@@

In Visual Studio:
Go to "Tools" => NuGet Package Manager => Package Manager Console

This will open up the NuGet console. There type in "Update-Database".
This should apply data migrations to the database if LocalDb is correctly installed.


@@@ IMPORTANT INFO: @@@

Admin account is seeded into the db upon project build.
It allows to create doctors which are essential to create visit reservations.
Admin credentials are:

Email: 		admin@wp.pl
Password:	Admin123!



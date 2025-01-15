Web application for user-submitted confessions, built with ASP.NET Core MVC, EF Core, SQL Express, and JavaScript. It’s inspired by a well known Serbian site ispovesti.com, with some features not found in the original, such as HTML formatting (paired with sanitizer for XSS protection). Just like in the original, guest users can post confessions and comments, but without ability to edit them after the fact, as the system intentionally doesn't keep track of who created them. Meanwhile, administrators can moderate the content by editing or deleting it. 

This application was done for university project, so it doesn't feature some some QoL features, such as pagination and captcha protection.

# Instructions 
1. Ensure you have .NET 8, Visual Studio 2022 and SQL Express installed. You may need to create database as per connection string specification in `appsettings.json`.
2. Simply press green arrow in VS 2022 to start the application.
3. The web browser should automatically open with the site. If not, check at https://localhost:7098/Confession. You should now see the ConFESSify running.
4. Should you wish to log in as admin user, check out Program.cs for seeded admin data (of course, this would be unnaceptable in production app).

![image](https://github.com/user-attachments/assets/5243fed6-a407-49f8-bef0-7dfcf0d5af6e)

![image](https://github.com/user-attachments/assets/89bc8cf6-ccff-41b4-9f6f-ad8f446ca20f)

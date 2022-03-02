using System;
using System.Linq;
using SkillPackageUtility.Controllers;

namespace SkillPackageUtility
{
    public class Program
    {
        private static PackageController _controller = new PackageController();

        static int Main(string[] args)
        {
            if (args.Length.Equals(0))
            {
                Console.Write(Environment.NewLine);
                Console.WriteLine("Please provide a valid command, see 'help' for a list of available commands.");
                return 1;
            }

            ProcessCommand(args);

            return 0;
        }

        private static void ProcessCommand(string[] arguments)
        {
            var action = arguments[0].ToLower();
            var actionArguments = arguments.Skip(1).ToArray();

            try
            {
                switch (action)
                {
                    case "help":
                        ShowHelpText(actionArguments);
                        break;
                    case "package":
                        var releasePath = actionArguments[0];
                        var configurationPath = actionArguments[1];
                        var destinationPath = actionArguments[2];

                        _controller.PackageSkill(releasePath, configurationPath, destinationPath);
                        Console.Write(Environment.NewLine);
                        Console.WriteLine($"Skill packaged successfully to the following location: {destinationPath}");
                        break;
                    case "getconfigtemplate":
                        var templateLocation = actionArguments[0];
                        _controller.GenerateConfigurationTemplate(templateLocation);
                        Console.Write(Environment.NewLine);
                        Console.WriteLine($"Configuration template generated successfully to the following location: {templateLocation}");
                        break;
                    default:
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("Unknown command entered, see 'help' for a list of available commands.");
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error - {e.Message}");
            }
        }

        private static void ShowHelpText(string[] arguments)
        {
            if (arguments.Length.Equals(0))
            {
                Console.Write(Environment.NewLine);
                Console.WriteLine("See the following for a list of available commands.");
                Console.WriteLine("Use 'help [command]' for a more detailed description of the available command.");
                Console.Write(Environment.NewLine);
                Console.WriteLine($"- help");
                Console.WriteLine($"- package");
                Console.WriteLine($"- getconfigtemplate");
            }
            else
            {
                switch (arguments[0])
                {
                    case "help":
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("help [command]");
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("- Displays help documentation.");
                        Console.WriteLine("- [command] Optional: The command to display the help documentation for.");
                        break;
                    case "package":
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("package <releaseFilePath>.bprelease <configurationFilePath>.json <destinationFilePath>.bpskill");
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("- Packages the specified release file into a skill package using the");
                        Console.WriteLine("  provided configuration settings to the specified location.");
                        Console.WriteLine("- <releaseFilePath>.bprelease Filepath to the .bprelease file you wish to package into a Skill.");
                        Console.WriteLine("- <configurationFilePath>.json Filepath to the .json configuration file for the Skill to be packaged.");
                        Console.WriteLine("- <destinationFilePath>.bpskill Filepath for the Skill to be saved at once the packaging process is complete as a .bpskill file.");
                        break;
                    case "getconfigtemplate":
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("getconfigtemplate <destinationFilePath>.json");
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("- Generates a skill configuration file template and saves it to the specified destination.");
                        Console.WriteLine("- <destinationFilePath>.json File path for the destination file to be saved as.");
                        break;
                    default:
                        Console.Write(Environment.NewLine);
                        Console.WriteLine("Unknown argument provided, see 'help' for a list of available arguments.");
                        break;
                }
            }
        }
    }
}




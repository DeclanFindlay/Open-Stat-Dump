namespace Utils.UserInput;
internal class UserInput
{
    public static bool GetBoolInput(string message)
    {
        while (true)
        {
            Console.Write(message);

            if (bool.TryParse(Console.ReadLine(), out bool value))
            {
                return value;
            }

            Console.WriteLine("Invalid input. Please enter true or false.\n");
        }
    }

    public static string GetStringInput(string message)
    {
        Console.WriteLine(message);
        while (true)
        {

            string? value = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            Console.WriteLine(message);
        }
    }

    public static int GetIntInput(string message)
    {
        Console.WriteLine(message);

        while (true)
        {
            string? input = Console.ReadLine();


            if (int.TryParse(input, out int value))
            {
                if(value < 1000)
                {
                    Console.WriteLine("The value must be 1000 or higher");
                    continue;
                }

                return value;

            }

            Console.WriteLine("Please enter a valid number:");
        }
    }

}
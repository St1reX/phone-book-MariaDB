using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.IO;
using System.Text.RegularExpressions;

namespace ksiazkaZDanymi
{
    internal class PhoneBook
    {
        private string path;

        private string databaseName;

        private SQLiteConnection connection;

        private SQLiteCommand commandHolder;

        private List<Person> personsList = new List<Person>();

        public PhoneBook(string databaseName)
        {
            this.databaseName = databaseName;


            try
            {
                CreateDatabaseConnection();

                ShowMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unknown error occurred. Please restart the program or contact our support team. Error communicate: {ex.Message}");
            }

        }

        private void CreateDatabaseConnection()
        {
            try
            {
                path = Path.GetFullPath(Path.Combine("..", "..", "..", databaseName));

                if (!File.Exists(path))
                {
                    SQLiteConnection.CreateFile(path);
                    Console.WriteLine($"Database file '{databaseName}' created at: {path}");

                    connection = new SQLiteConnection($"Data Source={path};Version=3;");
                    connection.Open();

                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Persons (
                    person_id INTEGER PRIMARY KEY AUTOINCREMENT,
                    name TEXT NOT NULL,
                    surname TEXT NOT NULL,
                    phone_number TEXT NOT NULL,
                    mail TEXT NOT NULL,
                    date_of_birth TEXT NOT NULL
                    );";

                    commandHolder = new SQLiteCommand(createTableQuery, connection);
                    commandHolder.ExecuteNonQuery();
                    Console.WriteLine("Table 'Persons' created successfully. \nPress any button to continue.");
                    Console.ReadKey();
                }
                else
                {
                    connection = new SQLiteConnection($"Data Source={path};Version=3;");
                    connection.Open();
                }

                commandHolder = connection.CreateCommand();
            }
            catch (Exception)
            {
                throw new Exception("Problem ocurred while creating connection with database.");
            }
        }

        private void FetchPersonsFromDatabase(string orderBy = null)
        {
            try
            {
                personsList.Clear();

                commandHolder.CommandText = orderBy == null ? "SELECT * FROM Persons" : $"SELECT * FROM Persons ORDER BY {orderBy}";
                var reader = commandHolder.ExecuteReader();

                while (reader.Read())
                {
                    personsList.Add(Person.CreateUser(
                    Convert.ToInt32(reader[0]),
                    reader[1].ToString(),
                    reader[2].ToString(),
                    reader[3].ToString(),
                    reader[4].ToString(),
                    reader[5].ToString()));
                }

                reader.Close();

            }
            catch (Exception ex)
            {
                throw new Exception("Problem occurred while fetching records from database: \n" + ex.Message);
            }

            if (personsList.Count == 0)
            {
                throw new InvalidOperationException("Table Persons in database are empty. No elements to select/display. You need to add at least one user before using this function.");
            }
        }

        private void DisplayListMembers(bool displaySorted = false, int elementsAmount = 4)
        {
            Console.Clear();

            ConsoleKey actionKey;

            int index = 1;

            try
            {

                if (displaySorted == true)
                {
                    int selectedOption = 0;

                    List<string> columns = new List<string>();


                    commandHolder.Reset();
                    commandHolder.CommandText = "PRAGMA table_info(Persons)";
                    var reader = commandHolder.ExecuteReader();

                    while (reader.Read())
                    {
                        columns.Add(reader["name"].ToString());
                    }


                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Select the column by which the records should be sorted.");
                        Console.WriteLine("Use {↑ and ↓} to change selected option, ENTER to choose. Press any other key to exit.");
                        Console.WriteLine();

                        for (int i = 0; i < columns.Count; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(" > " + columns[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(columns[i]);
                        }

                        actionKey = Console.ReadKey().Key;

                        switch (actionKey)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = selectedOption + 1 >= columns.Count ? 0 : selectedOption + 1;
                                continue;
                            case ConsoleKey.UpArrow:
                                selectedOption = selectedOption - 1 < 0 ? columns.Count - 1 : selectedOption - 1;
                                continue;
                            case ConsoleKey.Enter:
                                commandHolder.Reset();
                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
                    }

                    FetchPersonsFromDatabase(columns[selectedOption]);
                }
                else
                {
                    FetchPersonsFromDatabase();
                }

                do
                {
                    Console.Clear();

                    Console.WriteLine("Use {← and →} to navigate between pages. \nPress any other key to exit.");

                    for (int i = (index - 1) * elementsAmount; i < index * elementsAmount && i < personsList.Count; i++)
                    {
                        Console.WriteLine(
                        $"{personsList[i].ID}. {{ \n \t" +
                        $"Name: {personsList[i].Name} \n \t" +
                        $"Surname: {personsList[i].Surname} \n \t" +
                        $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                        $"Mail: {personsList[i].Email} \n \t" +
                        $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                        $"}}");
                    }

                    actionKey = Console.ReadKey().Key;

                    if (actionKey == ConsoleKey.RightArrow)
                    {
                        index = index + 1 > Math.Ceiling(personsList.Count / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                    }
                    else if (actionKey == ConsoleKey.LeftArrow)
                    {
                        index = index - 1 < 1 ? (int)Math.Ceiling(personsList.Count / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                    }
                }
                while (actionKey == ConsoleKey.LeftArrow || actionKey == ConsoleKey.RightArrow);

            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }
        }

        private int SelectFromListMembers(int elementsAmount = 4)
        {
            Console.Clear();

            ConsoleKey actionKey;

            int index = 1;

            int selectedPerson = 0;


            try
            {
                FetchPersonsFromDatabase();

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {← and →} to navigate between sites and {↑ and ↓} to change selected user.\n Press any other key to exit.");

                    for (int i = (index - 1) * elementsAmount; i < index * elementsAmount && i < personsList.Count; i++)
                    {

                        if (i == selectedPerson)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;

                            Console.WriteLine(
                            $"{personsList[i].ID}. {{ \n \t" +
                            $"Name: {personsList[i].Name} \n \t" +
                            $"Surname: {personsList[i].Surname} \n \t" +
                            $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                            $"Mail: {personsList[i].Email} \n \t" +
                            $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                            $"}}");

                            Console.ResetColor();
                            continue;
                        }

                        Console.WriteLine(
                        $"{personsList[i].ID}. {{ \n \t" +
                        $"Name: {personsList[i].Name} \n \t" +
                        $"Surname: {personsList[i].Surname} \n \t" +
                        $"Phone Number: {personsList[i].PhoneNumber} \n \t" +
                        $"Mail: {personsList[i].Email} \n \t" +
                        $"Date of Birth: {personsList[i].DateOfBirth} \n" +
                        $"}}");

                    }

                    actionKey = Console.ReadKey().Key;

                    switch (actionKey)
                    {
                        case ConsoleKey.RightArrow:
                            index = index + 1 > Math.Ceiling(personsList.Count / 4.0) ? 1 : index + 1;
                            selectedPerson = (index - 1) * elementsAmount;
                            continue;
                        case ConsoleKey.LeftArrow:
                            index = index - 1 < 1 ? (int)Math.Ceiling(personsList.Count / 4.0) : index - 1;
                            selectedPerson = (index - 1) * elementsAmount;
                            continue;
                        case ConsoleKey.UpArrow:
                            selectedPerson = selectedPerson + 1 >= index * elementsAmount || selectedPerson + 1 >= personsList.Count ? (index - 1) * elementsAmount : selectedPerson + 1;
                            continue;
                        case ConsoleKey.DownArrow:
                            selectedPerson = selectedPerson - 1 < (index - 1) * elementsAmount ? index * elementsAmount - 1 : selectedPerson - 1;
                            if (selectedPerson > personsList.Count)
                            {
                                selectedPerson = personsList.Count - 1;
                            }
                            continue;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            commandHolder.Reset();

                            return personsList[selectedPerson].ID;
                        default:
                            return -1;
                    }

                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }

        }


        private Dictionary<string, string> GetValidatedUserInput()
        {
            Dictionary<string, string> userInputs = new Dictionary<string, string>();

            string nameSurnameValidationPattern = "^[A-Z][a-z]+(?:\\s[A-Z][a-z]+)*$";
            Regex nameSurnameValidation = new Regex(nameSurnameValidationPattern);

            string dateValidationPattern = "^(?:\\d{4}[-/]\\d{2}[-/]\\d{2})$";
            Regex dateValidation = new Regex(dateValidationPattern);

            while (true)
            {
                try
                {
                    if (!userInputs.ContainsKey("name"))
                    {
                        Console.WriteLine("Enter new user name: ");
                        userInputs["name"] = Console.ReadLine();
                        if (!nameSurnameValidation.IsMatch(userInputs["name"]))
                        {
                            userInputs.Remove("name");
                            throw new ValidationException("Field 'name' must be a correctly provided string. Only letters are allowed.");
                        }
                    }

                    if (!userInputs.ContainsKey("surname"))
                    {
                        Console.WriteLine("Enter new user surname: ");
                        userInputs["surname"] = Console.ReadLine();
                        if (!nameSurnameValidation.IsMatch(userInputs["surname"]))
                        {
                            userInputs.Remove("surname");
                            throw new ValidationException("Field 'surname' must be a correctly provided string. Only letters are allowed.");
                        }
                    }

                    if (!userInputs.ContainsKey("phone_number"))
                    {
                        Console.WriteLine("Enter new user phone number (eg. 222-222-222): ");
                        userInputs["phone_number"] = Console.ReadLine();
                        if (!new PhoneAttribute().IsValid(userInputs["phone_number"]) || !new StringLengthAttribute(12).IsValid(userInputs["phone_number"]))
                        {
                            userInputs.Remove("phone_number");
                            throw new ValidationException("Field 'phone_number' must be a correctly provided string. Only numbers and separators are allowed.");
                        }
                    }

                    if (!userInputs.ContainsKey("mail"))
                    {
                        Console.WriteLine("Enter new user mail: ");
                        userInputs["mail"] = Console.ReadLine();
                        if (!new EmailAddressAttribute().IsValid(userInputs["mail"]))
                        {
                            userInputs.Remove("mail");
                            throw new ValidationException("Field 'mail' must be a correctly provided mail format.");
                        }
                    }

                    if (!userInputs.ContainsKey("date_of_birth"))
                    {
                        Console.WriteLine("Enter new user birth date (eg. 2012-02-12): ");
                        userInputs["date_of_birth"] = Console.ReadLine();
                        if (!dateValidation.IsMatch(userInputs["date_of_birth"]))
                        {
                            userInputs.Remove("date_of_birth");
                            throw new ValidationException("Field 'dateOfBirth' must be a correctly provided date format eg. (YYYY-MM-DD or YYYY/MM/DD).");
                        }
                    }

                    break;
                }
                catch (ValidationException ex)
                {
                    Console.WriteLine("Some provided data are incorrect: " + ex.Message);
                    Console.WriteLine("Enter this field again.");
                    Console.WriteLine("Press any button to continue.");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return userInputs;
        }

        private void AddToList()
        {
            Console.Clear();

            Dictionary<string, string> userInputs = GetValidatedUserInput();

            try
            {
                commandHolder.CommandText = $"INSERT INTO Persons (name, surname, phone_number, mail, date_of_birth) VALUES (@name, @surname, @phone_number, @mail, @date_of_birth)";

                commandHolder.Parameters.AddWithValue("@name", userInputs["name"]);
                commandHolder.Parameters.AddWithValue("@surname", userInputs["surname"]);
                commandHolder.Parameters.AddWithValue("@phone_number", userInputs["phone_number"]);
                commandHolder.Parameters.AddWithValue("@mail", userInputs["mail"]);
                commandHolder.Parameters.AddWithValue("@date_of_birth", userInputs["date_of_birth"]);

                commandHolder.ExecuteNonQuery();

                Console.WriteLine("User successfully added.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while adding the user: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Press any button to return.");
                Console.ReadKey();
                Console.Clear();

                commandHolder.Reset();
            }
        }

        private void DeleteFromList()
        {
            Console.Clear();

            int personToDelete;

            ConsoleKey actionKey;



            int selectedOption = 0;

            string[] options = { "YES", "NO" };


            try
            {
                FetchPersonsFromDatabase();

                while (true)
                {
                    personToDelete = SelectFromListMembers();

                    if (personToDelete == -1)
                    {
                        commandHolder.Reset();
                        Console.Clear();
                        return;
                    }

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Use {↑ and ↓} to change selected option.\n Press any other key to exit.");

                        Console.WriteLine($"Are you sure you want to delete user with ID: {personToDelete}");

                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(options[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(options[i]);
                        }

                        actionKey = Console.ReadKey().Key;

                        switch (actionKey)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = selectedOption + 1 >= options.Length ? 0 : selectedOption + 1;
                                continue;
                            case ConsoleKey.UpArrow:
                                selectedOption = selectedOption - 1 < 0 ? options.Length - 1 : selectedOption - 1;
                                continue;
                            case ConsoleKey.Enter:
                                if (selectedOption == 0)
                                {
                                    try
                                    {
                                        commandHolder.CommandText = "DELETE FROM Persons WHERE person_id = @person_id";
                                        commandHolder.Parameters.AddWithValue("@person_id", personToDelete);
                                        commandHolder.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("An error occurred while deleting the user: " + ex.Message);
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadKey();
                                        return;
                                    }

                                }
                                else
                                {
                                    commandHolder.Reset();
                                    Console.Clear();
                                    return;
                                }

                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
                    }

                    Console.WriteLine("User deleted. Do you want to delete another? Press Y for Yes, any other key to exit.");

                    commandHolder.Reset();

                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }


        }

        private void ModifyListMember()
        {
            Console.Clear();

            int personToModify;

            ConsoleKey actionKey;


            Dictionary<string, string> userInputs;


            int selectedOption = 0;

            string[] options = { "YES", "NO" };

            try
            {
                while (true)
                {
                    personToModify = SelectFromListMembers();

                    if (personToModify == -1)
                    {
                        commandHolder.Reset();
                        Console.Clear();
                        return;
                    }

                    userInputs = GetValidatedUserInput();

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Use {↑ and ↓} to change selected option.\n Press any other key to exit.");

                        Console.WriteLine($"Are you sure you want to modify data of user with ID: {personToModify}");

                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(options[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(options[i]);
                        }

                        actionKey = Console.ReadKey().Key;

                        switch (actionKey)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = selectedOption + 1 >= options.Length ? 0 : selectedOption + 1;
                                continue;
                            case ConsoleKey.UpArrow:
                                selectedOption = selectedOption - 1 < 0 ? options.Length - 1 : selectedOption - 1;
                                continue;
                            case ConsoleKey.Enter:
                                if (selectedOption == 0)
                                {
                                    try
                                    {
                                        commandHolder.CommandText = $"UPDATE Persons SET name = @name, surname = @surname, phone_number = @phone_number, mail = @mail, date_of_birth = @date_of_birth WHERE person_id = @personID";

                                        commandHolder.Parameters.AddWithValue("@personID", personToModify);
                                        commandHolder.Parameters.AddWithValue("@name", userInputs["name"]);
                                        commandHolder.Parameters.AddWithValue("@surname", userInputs["surname"]);
                                        commandHolder.Parameters.AddWithValue("@phone_number", userInputs["phone_number"]);
                                        commandHolder.Parameters.AddWithValue("@mail", userInputs["mail"]);
                                        commandHolder.Parameters.AddWithValue("@date_of_birth", userInputs["date_of_birth"]);

                                        commandHolder.ExecuteNonQuery();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("An error occurred while modyfying the user: " + ex.Message);
                                        Console.WriteLine("Press any key to continue.");
                                        Console.ReadKey();
                                        return;
                                    }

                                }
                                else
                                {
                                    commandHolder.Reset();
                                    Console.Clear();
                                    return;
                                }

                                break;
                            default:
                                commandHolder.Reset();
                                Console.Clear();
                                return;
                        }

                        break;
                    }

                    Console.WriteLine("User modified. Do you want to modify another person data? Press Y for Yes, any other key to exit.");

                    commandHolder.Reset();

                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        break;
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                Console.Clear();
            }
        }

        private void ShowMenu()
        {
            ConsoleKey actionKey;


            int selectedOption = 0;

            string[] options = { "Delete user (selectable)", "Add new user", "Display all users", "Modify user (selectable)", "Sort users (selectable)", "Terminate the program" };


            while (true)
            {
                try
                {
                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Welcome to the program 'Phone Book'. \nThis is simple utility working on MySQLite which provides methods to perform particular operations on records concerned persons in database.");
                        Console.WriteLine("Choose what you want to do by selecting option from the list below.");
                        Console.WriteLine("Use {↑ and ↓} to change selected option, ENTER to choose.");
                        Console.WriteLine();

                        for (int i = 0; i < options.Length; i++)
                        {
                            if (i == selectedOption)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;

                                Console.WriteLine(" > " + options[i]);

                                Console.ResetColor();
                                continue;
                            }

                            Console.WriteLine(options[i]);
                        }

                        actionKey = Console.ReadKey().Key;

                        switch (actionKey)
                        {
                            case ConsoleKey.DownArrow:
                                selectedOption = selectedOption + 1 >= options.Length ? 0 : selectedOption + 1;
                                continue;
                            case ConsoleKey.UpArrow:
                                selectedOption = selectedOption - 1 < 0 ? options.Length - 1 : selectedOption - 1;
                                continue;
                            case ConsoleKey.Enter:
                                break;
                            default:
                                continue;
                        }

                        break;
                    }

                    switch (selectedOption)
                    {
                        case 0:
                            DeleteFromList();
                            break;
                        case 1:
                            AddToList();
                            break;
                        case 2:
                            DisplayListMembers();
                            break;
                        case 3:
                            ModifyListMember();
                            break;
                        case 4:
                            DisplayListMembers(true);
                            break;
                        case 5:
                            Console.WriteLine("End of the program.");
                            return;
                        default:
                            Console.Clear();
                            Console.WriteLine("Option you selected is not available.");
                            break;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unknown error occurred. Please restart the program or contact our support team. Error communicate: {ex.Message}");
                    break;
                }
            }
        }
    }
}
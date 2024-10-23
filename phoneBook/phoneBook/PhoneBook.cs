using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace ksiazkaZDanymi
{
    internal class PhoneBook
    {
        private string databaseName;

        private MySqlConnection connection;

        private MySqlCommand commandHolder;

        private List<Person> personsList = new List<Person>();

        private int recordsAmount;

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

        //HELPER FUNCTIONS
        private int SelectFromListMembers(int elementsAmount = 4)
        {
            Console.Clear();

            ConsoleKey actionKey;

            int index = 1;

            int selectedPerson = 0;


            try
            {
                FetchPersonsFromDatabase(elementsAmount, 0);

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {← and →} to navigate between sites and {↑ and ↓} to change selected user.\n Press ESC to exit.");

                    for (int i = 0; i < personsList.Count; i++)
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
                            index = index + 1 > Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                            selectedPerson = 0;

                            FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4);
                            continue;
                        case ConsoleKey.LeftArrow:
                            index = index - 1 < 1 ? (int)Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                            selectedPerson = 0;

                            FetchPersonsFromDatabase(elementsAmount, (index - 1) * 4);
                            continue;
                        case ConsoleKey.UpArrow:
                            selectedPerson = selectedPerson - 1 < 0 ? personsList.Count - 1 : selectedPerson - 1;
                            continue;
                        case ConsoleKey.DownArrow:
                            selectedPerson = selectedPerson + 1 >= personsList.Count ? 0 : selectedPerson + 1;
                            if (selectedPerson > personsList.Count)
                            {
                                selectedPerson = personsList.Count - 1;
                            }
                            continue;
                        case ConsoleKey.Enter:
                            Console.Clear();
                            commandHolder.Parameters.Clear();

                            return personsList[selectedPerson].ID;
                        case ConsoleKey.Escape:
                            return -1;
                        default:
                            continue;
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
        private int SelectFromGivenOptions(List<string> options, List<string> comunicates)
        {
            int selectedOption = 0;
            ConsoleKey actionKey;

            var tmpCommunicates = comunicates.ToList();
            tmpCommunicates.Add("Use {↑ and ↓} to change selected option, ENTER to choose.");
            tmpCommunicates.Add("Press ESC to exit.");
            tmpCommunicates.Add("\n");

            while (true)
            {
                Console.Clear();

                foreach (var communicate in tmpCommunicates)
                {
                    Console.WriteLine(communicate);
                }


                for (int i = 0; i < options.Count; i++)
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
                        selectedOption = selectedOption + 1 >= options.Count ? 0 : selectedOption + 1;
                        continue;
                    case ConsoleKey.UpArrow:
                        selectedOption = selectedOption - 1 < 0 ? options.Count - 1 : selectedOption - 1;
                        continue;
                    case ConsoleKey.Enter:
                        tmpCommunicates.Clear();
                        return selectedOption;
                    case ConsoleKey.Escape:
                        tmpCommunicates.Clear();
                        return -1;
                    default:
                        continue;
                }
            }
        }
        private Dictionary<string, string> GetValidatedUserInput()
        {
            Dictionary<string, string> userInputs = new Dictionary<string, string>();

            string nameSurnameValidationPattern = @"^[A-ZĄĆĘŁŃÓŚŹŻ][a-ząćęłńóśźż]+$";
            Regex nameSurnameValidation = new Regex(nameSurnameValidationPattern);

            string dateValidationPattern = @"^(19\d{2}|20[01]\d|202[0-4])[-/](0[1-9]|1[0-2])[-/](0[1-9]|[12][0-9]|3[01])$";
            Regex dateValidation = new Regex(dateValidationPattern);

            string phoneNumberValidationPattern = @"^\d{9}$|^\d{3}-\d{3}-\d{3}$";
            Regex phoneNumberValidation = new Regex(phoneNumberValidationPattern);

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
                        if (!phoneNumberValidation.IsMatch(userInputs["phone_number"]))
                        {
                            userInputs.Remove("phone_number");
                            throw new ValidationException("Field 'phone_number' must be a correctly provided string. Only numbers and separators ('-') are allowed, maximum length = 9 or 12.");
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


        //BASIC FUNCTIONS
        private void CreateDatabaseConnection()
        {
            try
            {
                connection = new MySqlConnection("Database=mysql;Server=localhost;user=root;password=");
                connection.Open();

                commandHolder = connection.CreateCommand();

                commandHolder.CommandText = $"CREATE DATABASE IF NOT EXISTS {databaseName}";
                commandHolder.ExecuteNonQuery();

                commandHolder.CommandText = $"USE {databaseName}";
                commandHolder.ExecuteNonQuery();

                commandHolder.CommandText = @"
                CREATE TABLE IF NOT EXISTS Persons (
                    person_id INT AUTO_INCREMENT PRIMARY KEY,
                    name TEXT,
                    surname TEXT,
                    phone_number VARCHAR(12),
                    mail TEXT,
                    date_of_birth TEXT
                )";
                commandHolder.ExecuteNonQuery();

                commandHolder.CommandText = "SELECT COUNT(*) FROM Persons";
                recordsAmount = System.Convert.ToInt32(commandHolder.ExecuteScalar());

                Console.OutputEncoding = System.Text.Encoding.UTF8;
            }
            catch (Exception ex)
            {
                throw new Exception("Problem ocurred while creating connection with database: " + ex.Message);
            }

        }
        private void FetchPersonsFromDatabase(int limit, int offset, string orderBy = null, string searchTerm = null)
        {
            try
            {
                personsList.Clear();

                // FILTERING
                if (searchTerm != null)
                {
                    commandHolder.CommandText =
                    $"SELECT * FROM Persons WHERE " +
                    $"name LIKE '%{searchTerm}%' " +
                    $"OR surname LIKE '%{searchTerm}%' " +
                    $"OR phone_number LIKE '%{searchTerm}%' " +
                    $"OR mail LIKE '%{searchTerm}%' " +
                    $"OR date_of_birth LIKE '%{searchTerm}%' ";
                }
                else
                {
                    commandHolder.CommandText = "SELECT * FROM Persons";
                }

                // SORTING
                commandHolder.CommandText += orderBy == null ? "" : $" ORDER BY {orderBy} ";

                // LIMITS & OFFSETS
                commandHolder.CommandText += $" LIMIT {limit} OFFSET {offset}";


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
                if (searchTerm != null)
                {
                    throw new InvalidOperationException("No records were found. This could mean that the database is empty or no records match your search criteria.");
                }
                else
                {
                    throw new InvalidOperationException("Table Persons in database are empty. No elements to select/display. You need to add at least one user before using this function.");
                }
            }
        }
        private void DisplayListMembers(bool displaySorted = false, bool filterRecords = false, int elementsAmount = 4)
        {
            Console.Clear();

            List<string> columns = new List<string>();
            List<string> sortingCommunicates = new List<string>
            {
                "Select the column by which the records should be sorted."
            };
            int selectedOption = 0;


            string filterPrompt = "";


            ConsoleKey actionKey;
            int index = 1;

            try
            {
                //UPDATING RECORDS AMOUNT
                commandHolder.CommandText = "SELECT COUNT(*) FROM Persons";
                recordsAmount = System.Convert.ToInt32(commandHolder.ExecuteScalar());

                if (displaySorted == true)
                {

                    commandHolder.Parameters.Clear();
                    commandHolder.CommandText = "PRAGMA table_info(Persons)";
                    var reader = commandHolder.ExecuteReader();

                    while (reader.Read())
                    {
                        columns.Add(reader["name"].ToString());
                    }

                    reader.Close();

                    selectedOption = SelectFromGivenOptions(columns, sortingCommunicates);

                    FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount, columns[selectedOption]);
                }
                else if (filterRecords == true)
                {
                    Console.WriteLine("Let's filter records. \nEnter your prompt: ");

                    filterPrompt = Console.ReadLine();

                    //UPDATING RECORDS AMOUNT WHILE FILTERING
                    commandHolder.CommandText = "SELECT COUNT(*) FROM Persons WHERE " +
                    $"name LIKE '%{filterPrompt}%' " +
                    $"OR surname LIKE '%{filterPrompt}%' " +
                    $"OR phone_number LIKE '%{filterPrompt}%' " +
                    $"OR mail LIKE '%{filterPrompt}%' " +
                    $"OR date_of_birth LIKE '%{filterPrompt}%' ";

                    recordsAmount = System.Convert.ToInt32(commandHolder.ExecuteScalar());

                    FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount, searchTerm: filterPrompt);
                }
                else
                {
                    FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount);
                }

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Use {← and →} to navigate between pages. \nPress ESC to exit.");

                    for (int i = 0; i < personsList.Count; i++)
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

                    switch (actionKey)
                    {
                        case ConsoleKey.RightArrow:
                            index = index + 1 > Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) ? 1 : index + 1;
                            break;
                        case ConsoleKey.LeftArrow:
                            index = index - 1 < 1 ? (int)Math.Ceiling(recordsAmount / System.Convert.ToDecimal(elementsAmount)) : index - 1;
                            break;
                        case ConsoleKey.Escape:
                            return;
                        default:
                            continue;

                    }

                    if (displaySorted == true)
                    {
                        FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount, columns[selectedOption]);
                    }
                    else if (filterRecords == true)
                    {
                        FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount, searchTerm: filterPrompt);
                    }
                    else
                    {
                        FetchPersonsFromDatabase(elementsAmount, (index - 1) * elementsAmount);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error occured while displaying users: " + ex.Message);
                Console.WriteLine("Press any key to continue.");
                Console.ReadKey();
            }
            finally
            {
                Console.Clear();
            }
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

                commandHolder.Parameters.Clear();
            }
        }
        private void DeleteFromList()
        {
            Console.Clear();

            int personToDelete;


            int selectedOption = 0;
            List<string> options = new List<string> { "YES", "NO" };
            List<string> communicates = new List<string>();

            try
            {

                while (true)
                {
                    personToDelete = SelectFromListMembers();

                    if (personToDelete == -1)
                    {
                        commandHolder.Parameters.Clear();
                        Console.Clear();
                        return;
                    }

                    communicates.Add($"Are you sure you want to delete user with ID: {personToDelete}");

                    selectedOption = SelectFromGivenOptions(options, communicates);

                    switch(selectedOption)
                    {
                        case 0:
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
                        break;
                        case 1:
                            commandHolder.Parameters.Clear();
                            Console.Clear();
                            return;
                    }
                        
                    Console.WriteLine("User deleted. Do you want to delete another? Press Y for Yes, any other key to exit.");

                    commandHolder.Parameters.Clear();

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


            Dictionary<string, string> userInputs;


            int selectedOption = 0;
            List<string> options = new List<string> { "YES", "NO" };
            List<string> communicates = new List<string>();

            try
            {
                while (true)
                {
                    personToModify = SelectFromListMembers();

                    if (personToModify == -1)
                    {
                        commandHolder.Parameters.Clear();
                        Console.Clear();
                        return;
                    }

                    userInputs = GetValidatedUserInput();

                    communicates.Add($"Are you sure you want to modify user with ID: {personToModify}");

                    selectedOption = SelectFromGivenOptions(options, communicates);

                    switch (selectedOption)
                    {
                        case 0:
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
                                Console.WriteLine("An error occurred while modifying the user: " + ex.Message);
                                Console.WriteLine("Press any key to continue.");
                                Console.ReadKey();
                                return;
                            }
                            break;
                        case 1:
                            commandHolder.Parameters.Clear();
                            Console.Clear();
                            return;
                    }

                    Console.WriteLine("User modified. Do you want to modify another person data? Press Y for Yes, any other key to exit.");

                    commandHolder.Parameters.Clear();

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
            int selectedOption = 0;

            List<string> options = new List<string>
            {
                "Delete record (selectable)",
                "Add record",
                "Display all users",
                "Modify record (selectable)",
                "Sort records (selectable)",
                "Filter records (prompt)",
                "Terminate the program"
            };

            List<string> communicates = new List<string>
            {
                "Welcome to the program 'Phone Book'.",
                "This is simple utility working on MariaDB which provides methods to perform particular operations on records concerned persons in database.",
                "Choose what you want to do by selecting option from the list below."
            };

            while (true)
            {
                try
                {

                    selectedOption = SelectFromGivenOptions(options, communicates);

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
                            DisplayListMembers(displaySorted: true);
                            break;
                        case 5:
                            DisplayListMembers(filterRecords: true);
                            break;
                        case 6:
                        case -1:
                            Console.WriteLine("End of the program.");
                            return;
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
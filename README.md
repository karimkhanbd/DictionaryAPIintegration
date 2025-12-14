                                          Clean Architecture Dictionary Console App

This project is a simple C# console application designed to retrieve word definitions, phonetic details, and examples from a public dictionary API.
The main goal of this project is to demonstrate Clean Architecture, SOLID Principles, and professional software development practices, including Dependency Injection (DI) and comprehensive Unit Testing (xUnit/Moq).

Features:

Continuous Lookup: Allows users to search for multiple words without restarting the application.

Rich Data Display: Shows definitions, parts of speech, examples, synonyms, antonyms, phonetic texts, and source/licensing information.

Clean Output: Suppresses verbose framework logging for a tidy console experience.

Robust Error Handling: Gracefully handles "Word Not Found" and API communication errors.

How to Run the Project:

This project uses the modern .NET Generic Host for dependency injection.

Prerequisites:

•	.NET SDK 8.0 or newer

Steps:

1.	Clone the Repository:
   
            git clone [https://github.com/karimkhanbd/DictionaryApp]
            cd DictionaryApp
  	
2.	Run from the Host Project: Navigate to the main executable project and run it.
   
            cd DictionaryApp.Host
            dotnet run
  	
3.	Usage: The console will prompt you to enter a word. Type the word and press Enter. Type exit to close the application.

Project Architecture: 

This application strictly follows the Clean Architecture pattern (also known as Onion Architecture) to ensure maximum decoupling, testability, and maintainability.
The project is structured into four distinct layers, with dependencies strictly pointing inward: 

Testing and Quality:

The architecture allows for deep adherence to SOLID Principles (Single Responsibility, Dependency Inversion), making unit testing simple and reliable.
The solution includes the DictionaryApp.Application.Tests project for unit testing.

xUnit	: The testing framework used to define and run tests.
Moq	: The mocking framework used to isolate the Application layer.	We can test GetWordDefinitionQuery logic without making real network calls.

How to Run Unit Tests
Open your terminal at the solution root (DictionaryApp/).
Execute the .NET test command:  dotnet test

API Source:

This application uses the following public API for dictionary lookups:
API: Free Dictionary API
Endpoint: https://api.dictionaryapi.dev/api/v2/entries/en/{word}

License:

This project is licensed under the MIT License. See the LICENSE file for details.

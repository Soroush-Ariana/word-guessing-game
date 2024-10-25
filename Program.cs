using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;


namespace WordGuessingGame
{
    // This structure is not encapsulated because of automatic serialisation.
    public struct GameState
    {
        public string theWord;
        public int totalScore;
        public bool canUserContinue;
        public int guessingTheEntireWordChances;
        public bool hasUserWon;
        public bool isTheNumberOfTheLettersGuessed;
        public int howManyLettersAreGuessed;
        public List<bool> whichLettersAreGuessed;

        // Constructor(s)
        public GameState(string theWord)
        {
            this.theWord = theWord;
            this.totalScore = 100;
            this.canUserContinue = true;
            this.guessingTheEntireWordChances = 3;
            this.hasUserWon = false;
            this.isTheNumberOfTheLettersGuessed = false;
            this.howManyLettersAreGuessed = 0;
            this.whichLettersAreGuessed = new List<bool>();
            for (int i = 0; i < this.theWord.Length; i++)
            {
                this.whichLettersAreGuessed.Add(false);
            }
        }
    }

    public struct GuessingOneLetterState
    {
        private bool isInputInvalid;
        private char theChosenLetter;
        private int theChosenIndex;

        // Constructor(s)
        public GuessingOneLetterState(bool isInputInvalid, char theChosenLetter)
        {
            this.isInputInvalid = isInputInvalid;
            this.theChosenLetter = theChosenLetter;
            this.theChosenIndex = 0;
        }

        // Setters
        public void setIsInputInvalid(bool isInputInvalid)
        {
            this.isInputInvalid = isInputInvalid;
        }

        public void setTheChosenLetter(char theChosenLetter)
        {
            this.theChosenLetter = theChosenLetter;
        }

        public void setTheChosenIndex(int theChosenIndex)
        {
            this.theChosenIndex = theChosenIndex;
        }

        // Getters
        public bool getIsInputInvalid()
        {
            return this.isInputInvalid;
        }

        public char getTheChosenLetter()
        {
            return this.theChosenLetter;
        }

        public int getTheChosenIndex()
        {
            return this.theChosenIndex;
        }
    }

    internal class Program
    {

        const string listOfWordsFilePath = "listOfWords.txt";
        const string gameStateFilePath = "GameState.xml";
        const string finalResultsFilePath = "finalResults.txt";

        const string separator = "-------------------------------------------------------------------------";
        const string inValidInput = "Invalid input.";
        const int losingThreshold = 50;
        const int penaltyOfGuessingWrong = 2;
        static void Main()
        {

            Console.WriteLine("Welcome to the game. In this game you should guess a random word.");
            Console.WriteLine("Note that the word is related to food or drinks.");
            Console.WriteLine("Your initial total score is 100 and each time you guess a letter or the number of the letters wrong, 2 scores are subtracted from it.");
            Console.WriteLine("If your score gets lower than 50, you will lose.");
            Console.WriteLine();

            string[] words = File.ReadAllLines(listOfWordsFilePath);
            Random random = new Random();
            string theWord = words[random.Next(words.Length)];

            GameState theGameState = new GameState(theWord);

            theGameState = loadOrStartANewGame(theGameState);

            Console.WriteLine("You should select which one of the letters you want to guess,\nor you can choose to guess the entire word.");
            Console.WriteLine("You have only " + theGameState.guessingTheEntireWordChances + " chances for doing this.\nOr you can choose to save the game.");

            for (int i = theGameState.howManyLettersAreGuessed; i < theGameState.theWord.Length; i++)
            {

                if (!theGameState.canUserContinue)
                {
                    break;
                }

                Console.WriteLine();
                Console.WriteLine("If you want to save the game, press 's'.\n Otherwise, choose whether you want to guess the entire word or not. (y/n)");
                char saveOrYesOrNo = receivingACharacterFromUser();

                if (saveOrYesOrNo == 'y')
                {
                    theGameState = guessTheEntireWord(theGameState);
                    
                    if (!theGameState.canUserContinue)
                    {
                        break;
                    }

                    if (theGameState.hasUserWon)
                    {
                        break;
                    }

                    i--;

                }
                else if (saveOrYesOrNo == 'n')
                {
                    GuessingOneLetterState theGuessingOneLetterState;

                    do
                    {
                        theGuessingOneLetterState = choosingAnIndexToGuess(theGameState);

                        if (theGuessingOneLetterState.getIsInputInvalid()) continue;

                        theGameState = guessingTheLetterOfTheChosenIndex(theGuessingOneLetterState, theGameState);
                    } while (theGuessingOneLetterState.getIsInputInvalid());

                }
                else if (saveOrYesOrNo == 's')
                {
                    saveTheGame(theGameState);
                    i--;
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(inValidInput);
                    i--;
                }

            }

            if (!theGameState.canUserContinue)
            {
                announcingLoss(theGameState);
            }
            else
            {
                announcingWin(theGameState);
            }

        }

        static GameState guessTheEntireWord(GameState theGameState)
        {
            Console.WriteLine();
            Console.WriteLine("Type the entire word: ");
            string guessedWord = Console.ReadLine();
            guessedWord = guessedWord.ToLower();

            if (guessedWord == theGameState.theWord)
            {
                Console.WriteLine("You guessed the word correctly!");
                theGameState.hasUserWon = true;
                return theGameState;
            }
            else
            {
                Console.WriteLine("You didn't guess the word correctly!");
                theGameState.guessingTheEntireWordChances--;
                Console.WriteLine("You have " + theGameState.guessingTheEntireWordChances + " more chances for doing this.");

                if (theGameState.guessingTheEntireWordChances < 1)
                {
                    theGameState.canUserContinue = false;
                    return theGameState;
                }
                else
                {
                    return theGameState;
                }
            }
        }

        static GameState specifyNumberOfLetters(GameState theGameState)
        {

            int guessedNumberOfLetters = 0; // Initialised because of try catch.
            bool isInputInvalid = true; // Initialised as true because we don't want to subtract scores in thr first iteration of the following do-while loop.
            Console.WriteLine("Please guess the number of the letters of the word (it's no more than 12):");

            do
            {
                if (!isInputInvalid)
                {
                    theGameState.totalScore -= penaltyOfGuessingWrong;
                    Console.WriteLine("Wrong! Try again:");
                }
                if (theGameState.totalScore < losingThreshold)
                {
                    theGameState.canUserContinue = false;
                    return theGameState;
                }

                try
                {
                    guessedNumberOfLetters = int.Parse(Console.ReadLine());
                    isInputInvalid = false;
                }
                catch
                {
                    isInputInvalid = true;
                    Console.WriteLine(inValidInput);
                    Console.WriteLine("Try again: ");
                }
            } while (isInputInvalid || guessedNumberOfLetters != theGameState.theWord.Length);

            Console.WriteLine("Correct! Now Moving to next step.");
            theGameState.isTheNumberOfTheLettersGuessed = true;

            return theGameState;

        }

        static GameState loadOrStartANewGame(GameState theGameState)
        {
            int newGameOrContinue = 0; // I initialised it beacause otherwise it would lead to an error. because it is initialised later on in the 'try' block.

            do
            {
                Console.WriteLine("Please choose between these 2 options (type '1' or '2' then press enter):");
                Console.WriteLine("1. Start a new game.");
                Console.WriteLine("2. Load the last saved game.");

                try
                {
                    newGameOrContinue = int.Parse(Console.ReadLine());
                }
                catch
                {
                    Console.WriteLine(inValidInput);
                    continue;
                }

                if (newGameOrContinue == 2)
                {
                    if (File.Exists(gameStateFilePath))
                    {
                        theGameState = loadTheGame(theGameState);
                    }
                    else
                    {
                        Console.WriteLine("There is no saved game.");
                        newGameOrContinue = 0;
                        continue;
                    }


                    if (!theGameState.isTheNumberOfTheLettersGuessed)
                    {
                        theGameState = specifyNumberOfLetters(theGameState);

                        if (!theGameState.canUserContinue)
                        {
                            return theGameState;
                        }

                    }

                }
                else if (newGameOrContinue == 1)
                {
                    theGameState = specifyNumberOfLetters(theGameState);

                    if (!theGameState.canUserContinue)
                    {
                        return theGameState;
                    }

                }
                else
                {
                    Console.WriteLine(inValidInput);
                }
            } while ((newGameOrContinue != 1) && (newGameOrContinue != 2));

            return theGameState;
        }

        static void announcingLoss(GameState theGameState)
        {
            Console.WriteLine();
            Console.WriteLine("You lost!");

            using (StreamWriter writer = new StreamWriter(finalResultsFilePath, append: true))
            {
                writer.WriteLine("You lost!");
                writer.WriteLine("The word was: " + theGameState.theWord);
                writer.WriteLine(separator);
            }
        }

        static void announcingWin(GameState theGameState)
        {
            Console.WriteLine();
            Console.WriteLine("You won!");
            Console.WriteLine("Your score: " + theGameState.totalScore);

            using (StreamWriter writer = new StreamWriter(finalResultsFilePath, append: true))
            {
                writer.WriteLine("You won!");
                writer.WriteLine("Yor score: " + theGameState.totalScore);
                writer.WriteLine("The word was: " + theGameState.theWord);
                writer.WriteLine(separator);
            }
        }

        static void saveTheGame(GameState theGameState)
        {
            // Serialisation code assisted by chatGPT (the next 5 lines)
            XmlSerializer serializer = new XmlSerializer(typeof(GameState));
            using (TextWriter writer = new StreamWriter(gameStateFilePath))
            {
                serializer.Serialize(writer, theGameState);
            }
            Console.WriteLine();
            Console.WriteLine("You successfully saved the game.");

            char yesOrNo;

            do
            {
                Console.WriteLine("Do you want to continue? (y/n)");
                yesOrNo = receivingACharacterFromUser();

                if (yesOrNo == 'y')
                {
                    // Do nothing special. Just continue the normal process of the program.
                }
                else if (yesOrNo == 'n')
                {
                    Console.WriteLine();
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine(inValidInput);
                }
            } while (yesOrNo != 'y' && yesOrNo != 'n');
        }

        static GameState loadTheGame(GameState theGameState)
        {
            // Deserialisation code assisted by chatGPT (the next 5 lines)
            XmlSerializer deserializer = new XmlSerializer(typeof(GameState));
            using (TextReader reader = new StreamReader(gameStateFilePath))
            {
                theGameState = (GameState)deserializer.Deserialize(reader);
            }
            
            Console.WriteLine("The word has " + theGameState.theWord.Length + " letters.");

            if (theGameState.howManyLettersAreGuessed > 1) Console.WriteLine("You have guessed " + theGameState.howManyLettersAreGuessed + " letters of the word.");
            else Console.WriteLine("You have guessed " + theGameState.howManyLettersAreGuessed + " letter of the word.");


            if (theGameState.howManyLettersAreGuessed != 0)
            {
                for (int i = 0; i < theGameState.whichLettersAreGuessed.Count; i++)
                {

                    if (theGameState.whichLettersAreGuessed[i] == true)
                    {
                        Console.WriteLine("letter number " + (i + 1) + " is: \'" + theGameState.theWord[i] + "\'.");
                    }
                }
            }

            Console.WriteLine("Your current score is: " + theGameState.totalScore);

            return theGameState;
        }

        static char receivingACharacterFromUser()
        {
            ConsoleKeyInfo theCharacterInfo = Console.ReadKey();
            char theCharacter = theCharacterInfo.KeyChar;
            theCharacter = char.ToLower(theCharacter);

            return theCharacter;
        }

        static GuessingOneLetterState choosingAnIndexToGuess(GameState theGameState)
        {
            GuessingOneLetterState theGuessingOneLetterState = new GuessingOneLetterState(true, '\0');

            Console.WriteLine();
            Console.WriteLine("Please select the index of the letter you want to guess (from 1 to " + theGameState.theWord.Length + ")");

            if (theGameState.whichLettersAreGuessed.Contains(true))
            {
                showingGuessedIndexes(theGameState);
            }

            try
            {
                theGuessingOneLetterState.setTheChosenIndex(int.Parse(Console.ReadLine())); 
                theGuessingOneLetterState.setTheChosenLetter(theGameState.theWord[theGuessingOneLetterState.getTheChosenIndex() - 1]);
                theGuessingOneLetterState.setIsInputInvalid(false);
            }
            catch
            {
                theGuessingOneLetterState.setIsInputInvalid(true);
                return theGuessingOneLetterState;
            }

            if (theGameState.whichLettersAreGuessed[theGuessingOneLetterState.getTheChosenIndex() - 1] == true)
            {
                Console.WriteLine("You have already guessed this letter");
                theGuessingOneLetterState.setIsInputInvalid(true);
                return theGuessingOneLetterState;
            }

            return theGuessingOneLetterState;
        }

        static GameState guessingTheLetterOfTheChosenIndex(GuessingOneLetterState theGuessingOneLetterState, GameState theGameState)
        {
            char enteredLetter;

            Console.WriteLine("Great. Now guess the letter: ");
            enteredLetter = receivingACharacterFromUser();

            while (enteredLetter != theGuessingOneLetterState.getTheChosenLetter())
            {
                if (char.IsLetter(enteredLetter)) theGameState.totalScore -= penaltyOfGuessingWrong;

                if (theGameState.totalScore < losingThreshold)
                {
                    theGameState.canUserContinue = false;
                    break;
                }

                if (!char.IsLetter(enteredLetter))
                {
                    Console.WriteLine();
                    Console.WriteLine("You didn't enter an English letter.");
                }

                if (enteredLetter > theGuessingOneLetterState.getTheChosenLetter() && char.IsLetter(enteredLetter))
                {
                    Console.WriteLine();
                    Console.WriteLine("Too high!");
                }
                else if (enteredLetter < theGuessingOneLetterState.getTheChosenLetter() && char.IsLetter(enteredLetter))
                {
                    Console.WriteLine();
                    Console.WriteLine("Too low!");
                }

                Console.WriteLine("Try again: ");
                enteredLetter = receivingACharacterFromUser();
            }

            if (!theGameState.canUserContinue)
            {
                return theGameState;
            }

            theGameState.howManyLettersAreGuessed++;
            theGameState.whichLettersAreGuessed[theGuessingOneLetterState.getTheChosenIndex() - 1] = true;
            Console.WriteLine();
            Console.WriteLine("Your guess was correct!");

            return theGameState;
        }

        static void showingGuessedIndexes(GameState theGameState)
        {
            Console.Write("Except ");

            int numberOfTrues = 0;
            for (int i = 0; i < theGameState.whichLettersAreGuessed.Count; i++)
            {

                if (theGameState.whichLettersAreGuessed[i] == true)
                {
                    numberOfTrues++;

                    if (numberOfTrues > 1)
                    {
                        Console.Write(", ");
                    }
                    Console.Write(i + 1);
                }
            }

            Console.WriteLine();
        }

    }
}

using System;
using System.IO;
using System.Diagnostics;

namespace kiosk
{
    class Program
    {
        #region Global Variables
        static int transactionNum = 1;//
        static string date = ""; //
        static decimal cashAmount = 0;//
        static string cardVendor = "no card vendor";//
        static decimal cardAmount = 0;//
        static decimal changegiven = 0;
        static int[] bank = new int[12];
        static decimal[] moneyvalues = new decimal[] { 100, 50, 20, 10, 5, 2, 1, .50m, .25m, .10m, .05m, .01m };
        static int[] userMoney = new int[12];
        #endregion
        static void Main(string[] args)
        {
            string userChoice = "";
            decimal answer = 0;

            #region fill bank
            for (int index = 0; index < bank.Length; index++)
            {
                bank[index] = 5;
            }
            #endregion

            DateTime dateTime = DateTime.Now;
            date = dateTime.ToString("MMM-dd-yyyy,HH:mm");

            while (true)
            {
                Console.WriteLine("Welcome to a NHS Kiosk");
                Console.WriteLine("---------------------------------------");
                Console.WriteLine("Please start by entering in the prices for the items");
                Console.WriteLine("(Enter in a empty space to stop entring items)");
                Console.WriteLine("---------------------------------------");

                //call the function to get the total cost
                decimal totalCost = ItemInterage();

                Console.WriteLine("---------------------------------------");
                Console.WriteLine("The total cost of the items is {0}", totalCost);
                Console.WriteLine("---------------------------------------");

                //ask if the user would like to use cash or card
                do
                {
                    Console.WriteLine("---------------------------------------");
                    userChoice = prompt("Will you be using Cash or Card");
                    userChoice = userChoice.ToLower();
                    if (userChoice != "cash" && userChoice != "card") Console.WriteLine("Please enter the word cash or card");
                } while (userChoice != "cash" && userChoice != "card");


                //if the user choose cash then the program goes here
                if (userChoice == "cash")
                {
                    CashChoice(totalCost);
                    answer = -1;
                }

                do
                {
                    //if they choose card the code brings them here
                    if (userChoice == "card")
                    {
                        answer = CardChoice(totalCost);
                        if (answer == -3)
                        {
                            answer = CardChoice(totalCost);
                        }
                        else if (answer == -2)
                        {
                            CashChoice(totalCost);
                        }
                        else if (answer == -1)
                        {
                            Console.WriteLine("Thank you for choosing a NHS kiosk");
                        }
                        else if (answer < -3)
                        {
                            answer = CardChoice(answer * -1);
                        }
                        else if (answer > 0)
                        {
                            CashChoice(answer);
                        }
                    }
                } while (answer != -1);
                Console.WriteLine("---------------------------------------");
                Console.WriteLine();
                transactionLogging();
                transactionNum++;
            }
        }
        #region helper functions
        static string prompt(string text)
        {
            Console.Write(text + " ");
            return Console.ReadLine();
        }

        static int PromptInt(string text)
        {
            Console.Write(text + " ");
            return int.Parse(Console.ReadLine());
        }

        static double PromptDouble(string text)
        {
            Console.Write(text + " ");
            return double.Parse(Console.ReadLine());
        }
        #endregion

        #region Item interage
        static decimal ItemInterage()
        {
            #region variables
            string userInput = " ";
            decimal totalcost = 0;
            int count = 1;
            bool numericCheck = true;
            decimal numbercheck = 0;
            int counter = 0;
            bool checker = true;
            #endregion

            //start a loop to input your items
            do
            {
                //if what they enter is not a number this will tell them to re-enter it
                if (numericCheck == false) Console.WriteLine("(Please enter a number)");

                //this Is where they input there number
                Console.Write("Please enter the cost for item {0}:   $", count);
                userInput = Console.ReadLine();

                counter = 0;

                for (int index = 0; index < userInput.Length; index++)
                {
                    if (userInput[index] == '.')
                    {
                        for (int incrementer = index + 1; incrementer < userInput.Length; incrementer++)
                        {
                            counter++;
                        }
                    }
                }

                if (counter > 2) checker = false;
                else checker = true;

                //this checks that it is actually a number and if it is it will put into the numbercheck variable
                numericCheck = decimal.TryParse(userInput, out numbercheck);



                //if it is a number it and it is not a negative number then put it into the total cost
                if (numericCheck == true && numbercheck > 0 && checker == true)
                {
                    count++;
                    totalcost += numbercheck;
                }

                // if it has more than two decimal places then it tells them to re-enter a number
                else if (checker == false) Console.WriteLine("(You cant have more than two decimal places)");

                //if it is not a number above 0 it will tell the user to re-enter a number
                else if (numbercheck < 0) Console.WriteLine("(Please enter a number above 0)");

            } while (userInput != "");

            return totalcost;
        }
        #endregion

        #region cash choice
        static void CashChoice(decimal totalCost)
        {

            //call the function to get the total change
            decimal totalChange = CalculateChange(totalCost);

            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Your total change is ${0}", totalChange);

            //call the function to check the bank
            bool payGood = CheckBank(totalChange);

            //if the bank is ok then dispense change
            if (payGood == true) dispenseChange(totalChange);

            Console.WriteLine("---------------------------------------");
            Console.WriteLine("Thank you for using a NHS Kiosk");

        }
        #endregion

        #region card choice
        static decimal CardChoice(decimal totalCost)
        {
            //this tells
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("We only accept Visa, Mastercard, Discover, and American Express");
            string creditCard = "";
            bool isGood = true;
            string answer = "";
            decimal cashBack = 0;
            decimal newTotal = 0;
            bool getCashBack = true;

            bool numericCheck = true;
            decimal numberCheck = 0;

            //Validates if it is the actual length of a card
            do
            {
                creditCard = prompt("Please enter a card number");
                if (creditCard.Length != 16) Console.WriteLine("(Please enter a valid card number)");
            } while (creditCard.Length != 16);

            //sees if its actually a vendor
            string vendor = ValidateVendor(creditCard);
            cardVendor = vendor;

            //if it is a good vendor it then checks to make sure its a valid card
            if (vendor != "Non valid card")
            {
                isGood = ValidateCard(creditCard);

                //if the card was invalid it then asks them if they want to enter a new one
                if (isGood == false)
                {
                    Console.WriteLine("---------------------------------------");
                    do
                    {
                        Console.Write("Card was invalid, would you like to enter a new one?(y/n):");
                        answer = Console.ReadLine();
                        answer = answer.ToLower().Substring(0, 1);
                        if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                    } while (answer != "y" && answer != "n");
                    if (answer == "y") return -3;
                }
                //if the card vendor was not valid it asks if they want to enter new one
            }
            else
            {
                Console.WriteLine("---------------------------------------");
                do
                {
                    Console.Write("Card was invalid, would you like to enter a new one?(y/n):");
                    answer = Console.ReadLine();
                    answer = answer.ToLower().Substring(0, 1);
                    if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                } while (answer != "y" && answer != "n");

                //if they enter yes then they go here
                if (answer == "y") return -3;

                //else you ask them if they wanna pay with cash instead
                else
                {
                    Console.WriteLine("---------------------------------------");
                    do
                    {
                        Console.Write("Would you like to pay with cash instead (y/n):");
                        answer = Console.ReadLine();
                        answer = answer.ToLower().Substring(0, 1);
                        if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                    } while (answer != "y" && answer != "n");

                    //if its yes then it sends them to the cash payment
                    if (answer == "y")
                    {
                        return -2;

                        //else it ends the program
                    }
                    else
                    {
                        return -1;
                    }
                }
            }

            //if the card was invalid it asks if they want to pay with cash instead
            if (isGood == false)
            {
                do
                {
                    Console.Write("Would you like to pay with cash instead (y/n):");
                    answer = Console.ReadLine();
                    answer = answer.ToLower().Substring(0, 1);
                    if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                } while (answer != "y" && answer != "n");

                //if its yes then it sends them to the cash payment
                if (answer == "y") return -2;

                //else it ends the program
                else return -1;

                //else if the card didnt fail it asks if they want cash back
            }
            else
            {
                Console.WriteLine("---------------------------------------");
                do
                {
                    Console.Write("Do you want Cash back?(y/n)");
                    answer = Console.ReadLine();
                    answer = answer.ToLower().Substring(0, 1);
                    if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                } while (answer != "y" && answer != "n");
                //if yes it sends them to the cashback function
                if (answer == "y")
                {
                    cashBack = CashBack();
                    getCashBack = true;
                    //if cash back is greater than zero it adds it to totalCost
                    if (cashBack > 0)
                    {
                        newTotal += cashBack;
                        totalCost += newTotal;

                        //if the number returned is less than zero the bank does not have money to give cash back 
                    }
                    else if (cashBack < 0)
                    {
                        Console.WriteLine("This machine does not have the funds required to give cash back");
                        getCashBack = false;
                    }
                    //if they chose no cash back it prints this message
                }
                else
                {
                    Console.WriteLine("You chose no cash Back");
                    getCashBack = false;
                }

                //start checking the users bank to see if they have enough money to pay for it
                string[] availability = MoneyRequest(creditCard, totalCost);
                string checkPayment = availability[1];

                numericCheck = decimal.TryParse(checkPayment, out numberCheck);

                numericCheck = true;
                numberCheck = totalCost;
                //check to see if the payment went through
                if (numericCheck == true)
                {
                    //if it went through congratulations
                    if (numberCheck == totalCost)
                    {
                        Console.WriteLine("---------------------------------------");
                        if (getCashBack == true) Console.WriteLine("Congrats your payment went through and {0:C} was dispensed to you", cashBack);
                        else Console.WriteLine("Congrats your payment went through");
                        cardAmount = totalCost + cashBack;
                        changegiven = cashBack;
                        return -1;
                        //else if your card only can only pay for part of it then you ask if they want to pay the rest with a new card
                    }
                    else
                    {
                        Console.WriteLine("---------------------------------------");
                        do
                        {
                            string toString = numberCheck.ToString();
                            int decimalCheck = 0;

                            //this loop checks to make sure you dont have more than two decimal places.
                            for (int index = 0; index < toString.Length; index++)
                            {
                                if (toString[index] == '.') decimalCheck = index;
                            }
                            toString = toString.Substring(0, decimalCheck + 3);

                            numberCheck = decimal.Parse(toString);

                            Console.Write("Your Card Could only pay {0:C} Would you like to enter a new card? ", numberCheck);
                            answer = Console.ReadLine();
                            answer = answer.ToLower().Substring(0, 1);
                            if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                        } while (answer != "y" && answer != "n");
                        // if yes it returns what they have left to pay
                        if (answer == "y") return numberCheck * -1;

                        //else it asks if they want to pay with cash
                        else
                        {
                            do
                            {
                                Console.Write("Would you like to pay with cash instead (y/n):");
                                answer = Console.ReadLine();
                                answer = answer.ToLower().Substring(0, 1);
                                if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                            } while (answer != "y" && answer != "n");

                            //if yes it returns the amount and goes to cash
                            if (answer == "y") return numberCheck;

                            //else it ends the program
                            else return -1;
                        }
                    }
                    //if your card was declined it will ask you if you want to try another cost
                }
                else
                {
                    Console.WriteLine("---------------------------------------");
                    do
                    {
                        Console.Write("your card was declined because you didnt have enough in your bank would you like to try another card?");
                        answer = Console.ReadLine();
                        answer = answer.ToLower().Substring(0, 1);
                        if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                    } while (answer != "y" && answer != "n");

                    //if yes it sends them back to the start of the project
                    if (answer == "y") return -3;

                    //else it asks if they wanna pay with cash instead
                    else
                    {
                        Console.WriteLine("---------------------------------------");
                        do
                        {
                            Console.Write("Would you like to pay with cash instead (y/n):");
                            answer = Console.ReadLine();
                            answer = answer.ToLower().Substring(0, 1);
                            if (answer != "y" && answer != "n") Console.WriteLine("(please enter y or n)");
                        } while (answer != "y" && answer != "n");

                        //if yes it sends them to the cash payment
                        if (answer == "y") return -2;

                        //else it ends the program
                        else return -1;
                    }
                }
            }
        }
        #endregion

        #region Calculate Change
        static decimal CalculateChange(decimal totalCost)
        {
            #region variables
            string userInput = "";
            bool numericCheck = true;
            decimal numbercheck = 0;
            int count = 1;
            decimal totalInput = 0;
            decimal changeLeft = 0;
            int billNumber = 0;
            decimal totalChange = 0;
            #endregion

            do
            {
                //if what they enter is not a number this will tell them to re-enter it
                if (numericCheck == false) Console.WriteLine("(Please enter a number)");

                //this Is where they input there number
                Console.Write("Please enter the payment {0}:   $", count);
                userInput = Console.ReadLine();

                //this checks that it is actually a number and if it is it will put into the numbercheck variable
                numericCheck = decimal.TryParse(userInput, out numbercheck);


                //if it is a number it and it is not a negative number then put it into the change left and fill the bank
                if (numericCheck == true && numbercheck > 0)
                {
                    billNumber = CheckBill(numbercheck);
                    if (billNumber < 12)
                    {
                        count++;
                        totalInput += numbercheck;
                        cashAmount += totalInput;
                        changeLeft = totalCost - totalInput;
                        bank[billNumber]++;
                        userMoney[billNumber]++;
                        if (totalInput < totalCost) Console.WriteLine("You still owe ${0}", changeLeft);
                    }
                    else if (billNumber >= 12) Console.WriteLine("Please enter a valid bill");
                }

                //if it is not a number above 0 it will tell the user to re-enter a number
                else if (numbercheck < 0) Console.WriteLine("(Please enter a number above 0)");



            } while (totalInput < totalCost);

            //this puts the changeLeft into the totalLeft and checks to see if it is a negative number
            if (changeLeft < 0) totalChange = changeLeft * -1;
            else totalChange = changeLeft;

            //return the totalchange
            return totalChange;
        }
        #endregion

        #region check bill
        //This function takes the number they put in a checks to make sure that is a actural bill
        static int CheckBill(decimal numbercheck)
        {
            if (numbercheck == 100) return 0;
            else if (numbercheck == 50) return 1;
            else if (numbercheck == 20) return 2;
            else if (numbercheck == 10) return 3;
            else if (numbercheck == 5) return 4;
            else if (numbercheck == 2) return 5;
            else if (numbercheck == 1) return 6;
            else if (numbercheck == .50m) return 7;
            else if (numbercheck == .25m) return 8;
            else if (numbercheck == .10m) return 9;
            else if (numbercheck == .05m) return 10;
            else if (numbercheck == .01m) return 11;
            else return 12;
        }
        #endregion

        #region checkBank cash
        static bool CheckBank(decimal totalChange)
        {
            //create a replica bank to check if you have money in the bank 
            decimal moneyowed = totalChange;
            int[] array = new int[12];

            //fill the replica bank
            for (int index = 0; index < bank.Length; index++)
            {
                array[index] = bank[index];
            }

            //Start checking to see if the bank has enough to fufill the transaction
            for (int index = 0; moneyowed > 0; index++)
            {
                if (moneyowed >= moneyvalues[index] && array[index] > 0)
                {
                    array[index]--;
                    moneyowed -= moneyvalues[index];
                    index = 0;

                    //if the bank doesn't have enough money it will tell you and dispense their money back
                }
                else if (moneyowed > 0 && array[index] == 0)
                {
                    Console.WriteLine("This machine does not have enough\nmoney to despense change so you will be refunded");

                    //start dispensing the money back to the user
                    for (int count = 0; count < userMoney.Length; count++)
                    {
                        if (userMoney[count] > 0)
                        {
                            Console.WriteLine("{0:C} Dispensed change", moneyvalues[count]);
                            userMoney[count]--;
                            bank[count]--;
                            count = 0;
                        }
                    }
                    Console.WriteLine("---------------------------------------");
                    Console.WriteLine("Please find another way to pay");

                    //return false since the bank cant fufill the transaction
                    return false;
                }
            }
            //return true if it could fufill the transaction
            return true;
        }
        #endregion

        #region checkBank card
        static bool CheckBank(decimal totalChange, bool Card)
        {
            //create a replica bank to check if you have money in the bank 
            decimal moneyowed = totalChange;
            int[] array = new int[12];

            //fill the replica bank
            for (int index = 0; index < bank.Length; index++)
            {
                array[index] = bank[index];
            }

            //Start checking to see if the bank has enough to fufill the transaction
            for (int index = 0; moneyowed > 0; index++)
            {
                if (moneyowed >= moneyvalues[index] && array[index] > 0)
                {
                    array[index]--;
                    moneyowed -= moneyvalues[index];
                    index = 0;

                    //if the bank doesn't have enough money it will tell you and dispense their money back
                }
                else if (moneyowed > 0 && array[index] == 0)
                {


                    //start dispensing the money back to the user
                    for (int count = 0; count < userMoney.Length; count++)
                    {
                        if (userMoney[count] > 0)
                        {
                            Console.WriteLine("{0:C} Dispensed change", moneyvalues[count]);
                            userMoney[count]--;
                            bank[count]--;
                            count = 0;
                        }
                    }
                    //return false since the bank cant fufill the transaction
                    return false;
                }
            }
            //return true if it could fufill the transaction
            return true;
        }
        #endregion

        #region Dispense change
        static void dispenseChange(decimal totalChange)
        {
            //start dispencing the change to the user
            for (int index = 0; totalChange > 0; index++)
            {
                if (totalChange >= moneyvalues[index] && bank[index] > 0)
                {
                    bank[index]--;
                    totalChange -= moneyvalues[index];
                    Console.WriteLine("We dispensed {0:C} change", moneyvalues[index]);
                    changegiven += moneyvalues[index];
                    index = 0;
                }
            }
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("We have completed your payment");

        }
        #endregion

        #region Check vendor
        static string ValidateVendor(string creditCard)
        {
            //This identifies the card vendor 
            //visa
            if (creditCard[0] == '4') return "Visa";
            //mastercard
            else if (creditCard[0] == '5' && creditCard[1] == '1') return "Mastercard";
            else if (creditCard[0] == '5' && creditCard[1] == '2') return "Mastercard";
            else if (creditCard[0] == '5' && creditCard[1] == '3') return "Mastercard";
            else if (creditCard[0] == '5' && creditCard[1] == '4') return "Mastercard";
            else if (creditCard[0] == '5' && creditCard[1] == '5') return "Mastercard";
            //discover
            else if (creditCard[0] == '6' && creditCard[1] == '0' && creditCard[2] == '1' && creditCard[3] == '1') return "Discover";
            else if (creditCard[0] == '6' && creditCard[1] == '4' && creditCard[2] == '4') return "Discover";
            else if (creditCard[0] == '6' && creditCard[1] == '5') return "Discover";
            //American success
            else if (creditCard[0] == '3' && creditCard[1] == '4') return "American Express";
            else if (creditCard[0] == '3' && creditCard[1] == '7') return "American Express";
            //non valid card
            else return "Non valid card";
        }
        #endregion

        #region Validate card
        static bool ValidateCard(string creditCard)
        {
            //variables
            int[] array = new int[creditCard.Length];
            bool shouldDouble = true;
            char[] word = creditCard.ToCharArray();
            int sum = 0;
            int doubled = 0;
            string number = "";

            //put the credit card number into an array
            for (int index = 0; index < creditCard.Length; index++)
            {
                number = word[index].ToString();
                array[index] = int.Parse(number);
            }

            //this doubles every other number starting from the right
            for (int index = creditCard.Length - 2; index >= 0; index--)
            {
                if (shouldDouble)
                {
                    doubled = array[index] * 2;
                    if (doubled >= 10) sum += doubled - 9;
                    else sum += doubled;
                }
                else sum += array[index];
                shouldDouble = !shouldDouble;
            }
            //this returns the final number of credit card number
            sum += array[15];

            //if the sum is not evenly divisible by 10 then it returns false, or true if it is
            return (sum % 10 == 0);
        }
        #endregion

        #region MoneyRequest
        static string[] MoneyRequest(string account_number, decimal amount)
        {

            Random rnd = new Random();
            //50% chance that transaction passes or fails
            bool pass = rnd.Next(100) < 50;
            //50% chance that a failed transaction is declined
            bool declined = rnd.Next(100) < 50;

            if (pass)
            {
                return new string[] { account_number, amount.ToString() };
            }
            else
            {
                if (!declined)
                {
                    return new string[] { account_number, (amount / rnd.Next(2, 6)).ToString() };
                }
                else
                {
                    return new string[] { account_number, "declined" };
                }
            }
        }
        #endregion

        #region Cash back
        static decimal CashBack()
        {
            string cashBack = "";
            decimal counter = 0;
            decimal numberCheck = 0;
            bool checker = true;
            bool numericCheck = true;

            //ask them how much cash they want back
            do
            {
                Console.Write("How much cash do you want back: $");
                cashBack = Console.ReadLine();

                counter = 0;

                //check to make sure it only has one decimal and only 2 places after the decimal
                for (int index = 0; index < cashBack.Length; index++)
                {
                    if (cashBack[index] == '.')
                    {
                        for (int incrementer = index + 1; incrementer < cashBack.Length; incrementer++)
                        {
                            counter++;
                        }
                    }
                }

                if (counter > 2) checker = false;
                else checker = true;

                //this checks that it is actually a number and if it is it will put into the numbercheck variable
                numericCheck = decimal.TryParse(cashBack, out numberCheck);

                // if it has more than two decimal places then it tells them to re-enter a number
                if (checker == false) Console.WriteLine("(You cant have more than two decimal places)");

                //if it is not a number above 0 it will tell the user to re-enter a number
                else if (numberCheck < 0) Console.WriteLine("(Please enter a number above 0)");

                //it is not actually a number It will tell them to re-renter a number
                else if (numericCheck == false) Console.WriteLine("(Please enter a number)");

            } while (checker == false || numberCheck < 0 || numericCheck == false);

            //make sure there is enough money in the bank
            bool check = CheckBank(numberCheck, true);

            // return the answers
            if (check == true) return numberCheck;
            else return -1;
        }

        #endregion

        #region
        static void transactionLogging()
        {
            string vendor = cardVendor.Replace(' ', '`');
            string arguments = transactionNum.ToString() + "," + date + ",$" + cashAmount.ToString() + "," + vendor + ",$" + cardAmount.ToString() + ",$" + changegiven.ToString();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "logging.exe";
            startInfo.Arguments = arguments;
            Process.Start(startInfo);
        }
        #endregion
    }
}

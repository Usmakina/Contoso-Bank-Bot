using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Contoso_Bank_Bot.Models;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Contoso_Bank_Bot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));

            var userMessage = activity.Text;

            StateClient stateClient = activity.GetStateClient();
            BotData userData = await stateClient.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);

            string endOutput = "...";
            bool isRequest = false;

            if (activity.Type == ActivityTypes.Message)
            {
                HttpClient client = new HttpClient();

                // using luis
                string luis = await client.GetStringAsync(new Uri("https://api.projectoxford.ai/luis/v2.0/apps/cb5cbd34-0b3a-482a-b6c8-2529d6e1cabf?subscription-key=eea0e6ea98c24b00908f85f120e09d44&q=" + userMessage +"&verbose=true"));
                Luis.RootObject luisRootObject;
                luisRootObject = JsonConvert.DeserializeObject<Luis.RootObject>(luis);

                string luisMessage = "...";
                double luisScore = luisRootObject.topScoringIntent.score;

                // only use luis's reply if certainty is above 0.7
                if (luisScore > 0.7)
                {
                    luisMessage = luisRootObject.topScoringIntent.intent;
                }
                
                // responding to "Hello"
                if (userMessage.ToLower().Equals("hello"))
                {
                    if (userData.GetProperty<bool>("SentGreeting"))
                    {
                        endOutput = "Hello again";
                        if (userData.GetProperty<bool>("LoggedIn"))
                        {
                            endOutput += ", " + userData.GetProperty<string>("CurrentUsername");
                        }
                    }
                    else
                    {
                        userData.SetProperty<bool>("SentGreeting", true);
                        endOutput = "Hello";
                        if (userData.GetProperty<bool>("LoggedIn"))
                        {
                            endOutput += " " + userData.GetProperty<string>("CurrentUsername");
                        }
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                }

                // responding to "clear"
                if (userMessage.ToLower().Equals("clear"))
                {
                    endOutput = "User data cleared";
                    await stateClient.BotState.DeleteStateForUserAsync(activity.ChannelId, activity.From.Id);
                }

                // responding to "log out"
                if (userMessage.ToLower().Equals("log out"))
                {
                    if (userData.GetProperty<bool>("LoggedIn"))
                    {
                        endOutput = "You have logged out";
                        userData.SetProperty<bool>("LoggedIn", false);
                        userData.SetProperty<bool>("SentGreeting", false);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                    else
                    {
                        endOutput = "You are not logged in";
                    }
                }

                // replying to exchange rate messages
                if (userMessage.ToLower().Contains("to"))
                {
                    if (userMessage.Length >= 10)
                    {
                        string source = activity.Text.Substring(0, 3).ToUpper();
                        string symbol = activity.Text.Substring(7, 3).ToUpper();
                        string latest = await client.GetStringAsync(new Uri("http://api.fixer.io/latest?base=" + source));

                        LatestObject.RootObject rootObject;
                        rootObject = JsonConvert.DeserializeObject<LatestObject.RootObject>(latest);

                        string @base = rootObject.@base;
                        string date = rootObject.date;
                        double rates = 0.0;

                        if (symbol.ToUpper().Equals("AUD")) rates = rootObject.rates.AUD;
                        if (symbol.ToUpper().Equals("BGN")) rates = rootObject.rates.BGN;
                        if (symbol.ToUpper().Equals("BRL")) rates = rootObject.rates.BRL;
                        if (symbol.ToUpper().Equals("CAD")) rates = rootObject.rates.CAD;
                        if (symbol.ToUpper().Equals("CHF")) rates = rootObject.rates.CHF;
                        if (symbol.ToUpper().Equals("CNY")) rates = rootObject.rates.CNY;
                        if (symbol.ToUpper().Equals("CZK")) rates = rootObject.rates.CZK;
                        if (symbol.ToUpper().Equals("DKK")) rates = rootObject.rates.DKK;
                        if (symbol.ToUpper().Equals("GBP")) rates = rootObject.rates.GBP;
                        if (symbol.ToUpper().Equals("HKD")) rates = rootObject.rates.HKD;
                        if (symbol.ToUpper().Equals("HRK")) rates = rootObject.rates.HRK;
                        if (symbol.ToUpper().Equals("HUF")) rates = rootObject.rates.HUF;
                        if (symbol.ToUpper().Equals("IDR")) rates = rootObject.rates.IDR;
                        if (symbol.ToUpper().Equals("ILS")) rates = rootObject.rates.ILS;
                        if (symbol.ToUpper().Equals("INR")) rates = rootObject.rates.INR;
                        if (symbol.ToUpper().Equals("JPY")) rates = rootObject.rates.JPY;
                        if (symbol.ToUpper().Equals("KRW")) rates = rootObject.rates.KRW;
                        if (symbol.ToUpper().Equals("MXN")) rates = rootObject.rates.MXN;
                        if (symbol.ToUpper().Equals("MYR")) rates = rootObject.rates.MYR;
                        if (symbol.ToUpper().Equals("NOK")) rates = rootObject.rates.NOK;
                        if (symbol.ToUpper().Equals("NZD")) rates = rootObject.rates.NZD;
                        if (symbol.ToUpper().Equals("PHP")) rates = rootObject.rates.PHP;
                        if (symbol.ToUpper().Equals("PLN")) rates = rootObject.rates.PLN;
                        if (symbol.ToUpper().Equals("RON")) rates = rootObject.rates.RON;
                        if (symbol.ToUpper().Equals("RUB")) rates = rootObject.rates.RUB;
                        if (symbol.ToUpper().Equals("SEK")) rates = rootObject.rates.SEK;
                        if (symbol.ToUpper().Equals("SGD")) rates = rootObject.rates.SGD;
                        if (symbol.ToUpper().Equals("THB")) rates = rootObject.rates.THB;
                        if (symbol.ToUpper().Equals("TRY")) rates = rootObject.rates.TRY;
                        if (symbol.ToUpper().Equals("USD")) rates = rootObject.rates.USD;
                        if (symbol.ToUpper().Equals("ZAR")) rates = rootObject.rates.ZAR;
                        if (symbol.ToUpper().Equals("EUR")) rates = rootObject.rates.EUR;

                        if (rates == 0.0)
                        {
                            Activity ratesReply = activity.CreateReply($"Please enter a valid 3-letter country code.");
                            await connector.Conversations.ReplyToActivityAsync(ratesReply);
                        }
                        else
                        {
                            Activity ratesReply = activity.CreateReply($"The exchange rate from {@base} to {symbol} is {rates}.");

                            if (userMessage.Length > 10)
                            {
                                string amountStr = Regex.Match(userMessage, @"\d+").Value;
                                double amount = Double.Parse(amountStr);

                                double convertedAmount = amount * rates;

                                ratesReply.Recipient = activity.From;
                                ratesReply.Type = "message";
                                ratesReply.Attachments = new List<Attachment>();
                                List<CardImage> cardImages = new List<CardImage>();
                                cardImages.Add(new CardImage(url: "http://icons.iconarchive.com/icons/designcontest/ecommerce-business/128/dollar-icon.png"));
                                List<CardAction> cardButtons = new List<CardAction>();
                                CardAction ratesButton = new CardAction()
                                {
                                    Value = "https://www.msn.com/en-us/money/tools/currencyconverter",
                                    Type = "openUrl",
                                    Title = "Click here for more conversions"
                                };
                                cardButtons.Add(ratesButton);
                                ThumbnailCard ratesCard = new ThumbnailCard()
                                {
                                    Title = @base + " to " + symbol,
                                    Subtitle = amount + " " + @base + " = " + convertedAmount + " " + symbol,
                                    Images = cardImages,
                                    Buttons = cardButtons
                                };
                                Attachment plAttachment = ratesCard.ToAttachment();
                                ratesReply.Attachments.Add(plAttachment);
                                await connector.Conversations.SendToConversationAsync(ratesReply);
                            }
                            else
                            {
                                await connector.Conversations.ReplyToActivityAsync(ratesReply);
                            }

                        }
                    }

                    isRequest = true;
                }

                // reply to "login"

                if (userMessage.ToLower().Equals("log in") || userData.GetProperty<bool>("LoggingIn"))
                {
                    // check if logged in
                    if (userData.GetProperty<bool>("LoggedIn"))
                    {
                        endOutput = "You are already logged in as " + userData.GetProperty<string>("CurrentUsername");
                    }
                    else
                    {
                        if (userData.GetProperty<bool>("LoggingIn"))
                        {
                            if (userData.GetProperty<bool>("Password"))
                            {
                                if (userMessage == userData.GetProperty<string>("CorrectPassword"))
                                {
                                    userData.SetProperty<bool>("LoggedIn", true);
                                    userData.SetProperty<bool>("LoggingIn", false);
                                    userData.SetProperty<bool>("Password", false);
                                    userData.SetProperty<bool>("SentGreeting", false);
                                    userData.SetProperty<string>("CurrentPassword", userMessage);
                                    endOutput = "Logged in as " + userData.GetProperty<string>("CurrentUsername");
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                                else
                                {
                                    endOutput = "Invalid password, please try again";
                                }
                            }
                            else
                            {
                                List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetTransactions();

                                // loop through all transactions for inputted username
                                foreach (Transactions t in transactions)
                                {
                                    string username = t.username;

                                    if (userMessage.Equals(username))
                                    {
                                        // found username, ask for password
                                        endOutput = "Please enter your password";

                                        // set property to false
                                        userData.SetProperty<bool>("Password", true);
                                        userData.SetProperty<string>("CurrentUsername", username);
                                        userData.SetProperty<string>("CorrectPassword", t.password);
                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                        break;
                                    }
                                    else
                                    {
                                        endOutput = "Cannot find username: " + userMessage + "\n\n Please try again";
                                    }
                                }
                            }
                        }
                        else
                        {
                            endOutput = "Please enter your username";
                            userData.SetProperty<bool>("LoggingIn", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                    }
                }

                // reply to "show transactions"
                if (luisMessage.Equals("showTransaction"))
                {
                    if (userData.GetProperty<bool>("LoggedIn"))
                    {
                        List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetTransactions();
                        endOutput = "";
                        foreach (Transactions t in transactions)
                        {
                            string transactionUsername = userData.GetProperty<string>("CurrentUsername");

                            if (t.username == transactionUsername)
                            {
                                endOutput += t.id + "\n\n [" + t.createdAt + "] \n\n Withdraw: " + t.withdraw + ", Deposit: " + t.deposit + "\n\n" + "-" + "\n\n";
                            }
                        }
                        if (endOutput == "")
                        {
                            endOutput = "No transactions";
                        }
                    }
                    else
                    {
                        endOutput = "You must be logged in to do this";
                    }
                }

                // reply to "new transaction"
                if (luisMessage.Equals("newTransaction") || userData.GetProperty<bool>("NewTransaction"))
                {
                    if (userData.GetProperty<bool>("LoggedIn"))
                    {
                        if (userData.GetProperty<bool>("NewTransaction"))
                        {
                            // reply to "withdraw"
                            if (userData.GetProperty<bool>("Withdrawal"))
                            {
                                // reply to invalid withdraw input
                                double withdraw;
                                if (!double.TryParse(userMessage, out withdraw))
                                {
                                    endOutput = "Please enter a valid amount";
                                }
                                else
                                {
                                    // add new transaction to database
                                    Transactions transaction = new Transactions()
                                    {
                                        withdraw = Convert.ToDouble(userMessage),
                                        username = userData.GetProperty<string>("CurrentUsername"),
                                        password = userData.GetProperty<string>("CurrentPassword")
                                    };

                                    await AzureManager.AzureManagerInstance.AddTransaction(transaction);
                                    endOutput = "New transaction added [" + transaction.createdAt + "]";

                                    // set properties to false
                                    userData.SetProperty<bool>("Withdrawal", false);
                                    userData.SetProperty<bool>("NewTransaction", false);
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                            }
                            else
                            {
                                if (luisMessage.Equals("withdraw"))
                                {
                                    userData.SetProperty<bool>("Withdrawal", true);
                                    endOutput = "Please enter the amount you would like to withdraw";
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                            }

                            // reply to "deposit"
                            if (userData.GetProperty<bool>("Deposit"))
                            {
                                // reply to invalid deposit input
                                double deposit;
                                if (!double.TryParse(userMessage, out deposit))
                                {
                                    endOutput = "Please enter a valid amount";
                                }
                                else
                                {
                                    // add new transaction to database
                                    Transactions transaction = new Transactions()
                                    {
                                        deposit = Convert.ToDouble(userMessage),
                                        username = userData.GetProperty<string>("CurrentUsername"),
                                        password = userData.GetProperty<string>("CurrentPassword")
                                    };

                                    await AzureManager.AzureManagerInstance.AddTransaction(transaction);
                                    endOutput = "New transaction added [" + transaction.createdAt + "]";

                                    // set properties to false
                                    userData.SetProperty<bool>("Deposit", false);
                                    userData.SetProperty<bool>("NewTransaction", false);
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                            }
                            else
                            {
                                if (luisMessage.Equals("deposit"))
                                {
                                    endOutput = "Please enter the amount you would like to deposit";
                                    userData.SetProperty<bool>("Deposit", true);
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                            }
                        }
                        else
                        {
                            endOutput = "Withdraw or Deposit?";
                            userData.SetProperty<bool>("NewTransaction", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                    }                    
                    else
                    {
                        endOutput = "You must be logged in to do this";
                    }
                }

                // reply to "delete transaction"
                if (luisMessage.Equals("deleteTransaction") || userData.GetProperty<bool>("DeleteTransaction"))
                {
                    // check if logged in
                    if (userData.GetProperty<bool>("LoggedIn"))
                    {
                        if (userData.GetProperty<bool>("DeleteTransaction"))
                        {
                            // reply to transaction ID to be deleted
                            if (userMessage.Length.Equals(36))
                            {
                                List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetTransactions();

                                // loop through all transactions for inputed transaction
                                foreach (Transactions t in transactions)
                                {
                                    string transactionId = t.id;

                                    if (userMessage.Equals(transactionId))
                                    {
                                        // delete transaction
                                        await AzureManager.AzureManagerInstance.DeleteTransaction(t);
                                        endOutput = "Transaction deleted \n\n [" + transactionId + "]";

                                        // set property to false
                                        userData.SetProperty<bool>("DeleteTransaction", false);
                                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);

                                        break;
                                    }
                                    else
                                    {
                                        endOutput = "Cannot find ID : " + userMessage;
                                    }
                                }
                            }
                            else
                            {
                                endOutput = "Please enter a valid ID";
                            }
                        }
                        else
                        {
                            endOutput = "Please enter the ID of the transaction you would like to delete";
                            userData.SetProperty<bool>("DeleteTransaction", true);
                            await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                        }
                    }
                    else
                    {
                        endOutput = "You must be logged in to do this";
                    }
                }
                
                if (userMessage.ToLower().Equals("sign up") || userData.GetProperty<bool>("SigningUp"))
                {
                    if (userData.GetProperty<bool>("SigningUp"))
                    {
                        if (userData.GetProperty<bool>("NewPassword"))
                        {
                            if (userData.GetProperty<bool>("ConfirmPassword"))
                            {
                                if (userMessage == userData.GetProperty<string>("NewPasswords"))
                                {
                                    string username = userData.GetProperty<string>("CurrentUsername");
                                    userData.SetProperty<string>("CurrentPassword", userMessage);
                                    endOutput = "New account created. \n\n Now logged in as: " + username;
                                    userData.SetProperty<bool>("ConfirmPassword", false);
                                    userData.SetProperty<bool>("NewPassword", false);
                                    userData.SetProperty<bool>("SigningUp", false);
                                    userData.SetProperty<bool>("LoggedIn", true);
                                    userData.SetProperty<bool>("SentGreeting", false);
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                                else
                                {
                                    endOutput = "Incorrect password, please confirm again";
                                }
                            }
                            else
                            {
                                if (userMessage.Length >= 5)
                                {
                                    userData.SetProperty<string>("NewPasswords", userMessage);
                                    endOutput = "Please confirm the new password";
                                    userData.SetProperty<bool>("ConfirmPassword", true);
                                    await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                                }
                                else
                                {
                                    endOutput = "Please enter a new password with at least 5 characters";
                                }
                            }
                        }
                        else
                        {
                            List<Transactions> transactions = await AzureManager.AzureManagerInstance.GetTransactions();
                            bool taken = false;
                            foreach (Transactions t in transactions)
                            {
                                string newUsername = userMessage;

                                if (t.username == newUsername)
                                {
                                    taken = true;
                                }
                            }
                            if (taken)
                            {
                                endOutput = "This username has already been taken. Please try another";
                            }
                            else
                            {
                                userData.SetProperty<string>("CurrentUsername", userMessage);
                                endOutput = "Please enter a new password with at least 5 characters";
                                userData.SetProperty<bool>("NewPassword", true);
                                await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                            }
                        }
                    }
                    else
                    {
                        endOutput = "Please enter a new username";
                        userData.SetProperty<bool>("SigningUp", true);
                        await stateClient.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
                    }
                }

                if (!isRequest)
                {
                    Activity infoReply = activity.CreateReply(endOutput);
                    await connector.Conversations.ReplyToActivityAsync(infoReply);
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}
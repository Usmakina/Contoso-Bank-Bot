using Contoso_Bank_Bot.Models;
using Microsoft.WindowsAzure.MobileServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Contoso_Bank_Bot
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<Transactions> transactionsTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("https://contoso-bank.azurewebsites.net/");
            this.transactionsTable = this.client.GetTable<Transactions>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }

                return instance;
            }
        }

        public async Task AddTransaction(Transactions transaction)
        {
            await this.transactionsTable.InsertAsync(transaction);
        }

        public async Task DeleteTransaction(Transactions transaction)
        {
            await this.transactionsTable.DeleteAsync(transaction);
        }

        public async Task<List<Transactions>> GetTransactions()
        {
            return await this.transactionsTable.ToListAsync();
        }
    }
}
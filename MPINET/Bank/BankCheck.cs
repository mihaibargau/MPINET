using System;

namespace MPINET.Bank
{
    [Serializable]
    class BankCheck
    {
        public string BankId { get; set; }
        public string AccountId { get; set; }
        public string CheckNumber { get; set; }

        public BankCheck(string bankid, string accountid, string checknumber)
        {
            BankId = bankid;
            AccountId = accountid;
            CheckNumber = checknumber;
        }

    }
}

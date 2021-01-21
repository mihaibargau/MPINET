using System;

namespace MPINET.Bank
{
    [Serializable]
    class BankCheck
    {
        public uint BankId { get; set; }
        public uint AccountId { get; set; }
        public uint CheckNumber { get; set; }

        public BankCheck(string bankid, string accountid, string checknumber)
        {
            if (bankid == null || accountid == null || checknumber == null)
                throw new ArgumentNullException("Null value in BankCheck constructor");
            BankId = uint.Parse(bankid);
            AccountId = uint.Parse(accountid);
            CheckNumber = uint.Parse(checknumber);
        }

    }
}

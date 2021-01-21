﻿using System;

namespace MPINET.Bank
{
    [Serializable]
    class BankCheck
    {
        public uint BankId;
        public uint AccountId;
        public uint CheckNumber;

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

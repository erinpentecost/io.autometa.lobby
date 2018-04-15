using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    /// This provides a method of validating messages without
    /// being super verbose for each message.
    /// It's also a message itself, which you can use to send
    /// success or error criteria back to clients.
    [DataContract]
    public class ValidationCheck : IMessage
    {
        [DataMember]
        public bool result {get;set;}

        [DataMember]
        public string reason {get;set;}

        private static readonly int maxStr = 69;
        private static char[] alphaNum = ".:[]abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();


        private ValidationCheck(bool result, string reason)
        {
            this.result = result;
            if (this.result)
            {
                this.reason = string.Empty;
            }
            else
            {
                this.reason = reason;
            }
        }

        public ValidationCheck()
        {
            this.result = true;
            this.reason = string.Empty;
        }

        public ValidationCheck Compose(bool result, string reason)
        {
            if (this.result)
            {
                return new ValidationCheck(result, reason);
            }
            else
            {
                return this;
            }
        }

        public ValidationCheck Compose(Func<ValidationCheck> check)
        {
            if (this.result)
            {
                return check();
            }
            else
            {
                return this;
            }
        }

        public ValidationCheck Compose(ValidationCheck check)
        {
            if (this.result)
            {
                return check;
            }
            else
            {
                return this;
            }
        }

        public static ValidationCheck BasicStringCheck(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return new ValidationCheck(false, "empty");
            }
            if (str.Length > maxStr)
            {
                return new ValidationCheck(false, "too long");
            }

            if (str.IndexOfAny(alphaNum) != -1)
            {
                return new ValidationCheck(false, "only alphanumeric characters allowed");
            }

            return new ValidationCheck(true, string.Empty);
        }

        public ValidationCheck Validate()
        {
            return this;
        }
    }
}
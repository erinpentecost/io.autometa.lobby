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
            return new ValidationCheck(this.result && result, CombineReasons(this.reason, reason));
        }

        /// Won't execute the check if "this" is already failed.
        public ValidationCheck Compose(Func<ValidationCheck> check)
        {
            if (this.result)
            {
                var v = check.Invoke();
                return new ValidationCheck(this.result && v.result, CombineReasons(this.reason, v.reason));
            }
            else
            {
                return this;
            }
        }

        public ValidationCheck Compose(ValidationCheck check)
        {
            return new ValidationCheck(this.result && check.result, CombineReasons(this.reason, check.reason));
        }

        /// Enforces some arbitrary restraints on strings as a sanity check.
        public static ValidationCheck BasicStringCheck(string strToCheck, string id = null)
        {
            string prefix = string.Empty;
            if (!string.IsNullOrWhiteSpace(id))
            {
                prefix = id + ": ";
            }
            if (string.IsNullOrWhiteSpace(strToCheck))
            {
                return new ValidationCheck(false, prefix+"empty");
            }
            if (strToCheck.Length > maxStr)
            {
                return new ValidationCheck(false, prefix+"too long");
            }

            if (strToCheck.IndexOfAny(alphaNum) != -1)
            {
                return new ValidationCheck(false, prefix+"only alphanumeric-ish characters allowed");
            }

            return new ValidationCheck();
        }

        public ValidationCheck Validate()
        {
            return this;
        }

        private static string CombineReasons(string r1, string r2)
        {
            if (string.IsNullOrWhiteSpace(r2)){
                return r1;
            }
            else if (string.IsNullOrWhiteSpace(r1)){
                return string.Empty;
            }
            else
            {
                return r1 + ". " + r2;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

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
        public List<object> reason {get;set;}

        private static readonly int maxStr = 69;
        private static char[] illegal = "\\/\"'{}:;&".ToCharArray();


        private ValidationCheck(bool result, string reason)
        {
            this.result = result;
            this.reason = new List<object>();
            if (!this.result)
            {
                this.reason.Add(reason);
            }
        }

        public ValidationCheck()
        {
            this.result = true;
            this.reason = new List<object>();
        }

        public ValidationCheck Compose(bool result, string reason)
        {
            if (result)
            {
                return this;
            }
            else
            {
                this.result = false;
                this.reason.Add(reason);
                return this;
            }
        }

        /// Conditionall executed
        public ValidationCheck Compose(Func<bool> result, string reason)
        {
            if (this.result)
            {
                bool v = result.Invoke();
                if (v)
                {
                    return this;
                }
                else
                {
                    this.result = false;
                    this.reason.Add(reason);
                    return this;
                }
            }
            return this;
        }

        /// Conditionally executed
        public ValidationCheck Compose(Func<ValidationCheck> check)
        {
            if (this.result)
            {
                var v = check.Invoke();
                if (v.result)
                {
                    return this;
                }
                else
                {
                    this.result = false;
                    this.reason.Add(v.reason);
                    return this;
                }
            }
            return this;
        }

        public ValidationCheck Compose(ValidationCheck check)
        {
            if (check.result)
            {
                return this;
            }
            else
            {
                this.result = false;
                this.reason.Add(check.reason);
                return this;
            }
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
                return new ValidationCheck(false, prefix+"too long ("+strToCheck.Length+"/"+maxStr+")");
            }

            if (strToCheck.IndexOfAny(illegal) != -1)
            {
                return new ValidationCheck(false, prefix+"illegal characters ("+illegal+")");
            }

            return new ValidationCheck();
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
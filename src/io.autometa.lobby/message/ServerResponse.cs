using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    [DataContract]
    public class ServerResponse<T> : IMessage
        where T : class, IMessage
    {
        [DataMember]
        public T response {get;}

        [DataMember]
        public ValidationCheck valid {get;}

        public ServerResponse(T response, ValidationCheck valid)
        {
            this.response = null;

            if (valid == null)
            {
                valid = new ValidationCheck();
            }
            this.valid = valid;

            if (valid.result)
            {
                this.response = response;
            }
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }
    }
}
using System;
using System.Runtime.Serialization;

namespace io.autometa.lobby.message
{
    [DataContract]
    public class ServerResponse<T> : IMessage
        where T : class
    {
        [DataMember]
        public T response {get;}

        [DataMember]
        public ValidationCheck valid {get;}

        public ServerResponse(T response, ValidationCheck valid)
        {
            this.valid = null;
            this.response = null;

            if ((valid == null) || valid.result)
            {
                this.response = response;
            }
            else
            {
                this.valid = valid;
            }
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }
    }
}
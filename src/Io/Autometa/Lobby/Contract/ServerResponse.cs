using System;
using System.Runtime.Serialization;

namespace Io.Autometa.Lobby.Contract
{
    [DataContract]
    public class ServerResponse<T> : IMessage
        where T : class, IMessage
    {
        [DataMember]
        public string typeName {get;}

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

            this.typeName = GetFriendlyName(typeof(T));
        }

        public ValidationCheck Validate()
        {
            return new ValidationCheck();
        }

        private static string GetFriendlyName(Type type)
        {
            string friendlyName = type.Name;
            if (type.IsGenericType)
            {
                int iBacktick = friendlyName.IndexOf('`');
                if (iBacktick > 0)
                {
                    friendlyName = friendlyName.Remove(iBacktick);
                }
                friendlyName += "(";
                Type[] typeParameters = type.GetGenericArguments();
                for (int i = 0; i < typeParameters.Length; ++i)
                {
                    string typeParamName = GetFriendlyName(typeParameters[i]);
                    friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
                }
                friendlyName += ")";
            }

            return friendlyName;
        }
    }
}
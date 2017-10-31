using System;

using Newtonsoft.Json;

namespace AopWrapper.Handlers
{
    /// <summary>
    /// �������Ļ���
    /// Ҫ��������������й������޲εĹ��캯����
    /// ������ʹ��Newtonsoft.Json���л�
    /// </summary>
    [Serializable]
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public abstract class AopHandler : Attribute, IAopHandler
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-ddTHH:mm:ss",
            NullValueHandling = NullValueHandling.Ignore
        };
        /// <summary> ������������BeginInvoke˳��ִ�У�EndInvoke����ִ�� </summary>
        public int Order { get; set; }

        [JsonIgnore]
        public override object TypeId
        {
            get { return base.TypeId; }
        }

        /// <summary>
        /// һ���յ�������
        /// </summary>
        public static IAopHandler Empty
        {
            get { return new EmptyHandler(); }
        }

        public virtual void BeginInvoke(MethodContext context)
        {
        }

        public virtual void EndInvoke(MethodContext context)
        {
        }

        public virtual void OnException(MethodContext context)
        {
        }

        public override int GetHashCode()
        {
            var baseHash = base.GetHashCode();
            var type = this.GetType();
            var typeNameHash = (type.FullName ?? type.Name).GetHashCode();
            return baseHash ^ typeNameHash;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.GetHashCode() == obj.GetHashCode();
        }

        internal virtual string Serialize()
        {
            try
            {
                var txt = JsonConvert.SerializeObject(this, settings);
                var typeName = this.GetType().AssemblyQualifiedName;
                return string.Format("{0}|{1}", typeName, txt);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static IAopHandler Deserialize(string txt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txt))
                {
                    return Empty;
                }
                var arr = txt.Split("|".ToCharArray(), 2);
                var type = Type.GetType(arr[0]);
                if (arr.Length != 2 || type == null)
                {
                    return Empty;
                }
                var handler = (IAopHandler)JsonConvert.DeserializeObject(arr[1], type, settings);
                if (handler == null)
                {
                    return Empty;
                }
                return handler;
            }
            catch (Exception ex)
            {
                return Empty;
            }
        }
    }
}
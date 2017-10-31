using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;

namespace AopProxy
{
    public class XRealProxy<T> : RealProxy
    {
        private readonly T _target;
        public XRealProxy(T instance) : base(typeof(T))
        {
            this._target = instance;
        }

        public override IMessage Invoke(IMessage msg)
        {
            IMethodCallMessage callMessage = (IMethodCallMessage)msg;
            var handlers = GetHandlers(callMessage.MethodBase, this._target);
            foreach (var handler in handlers)
            {
                handler.Pre(msg);
            }
            object returnValue = callMessage.MethodBase.Invoke(this._target, callMessage.Args);

            foreach (var handler in handlers)
            {
                handler.Post(msg, returnValue);
            }
            return new ReturnMessage(returnValue, new object[0], 0, null, callMessage);
        }

        private List<CallHandler> GetHandlers(MethodBase method, object target)
        {
            var type = target.GetType();
            var types = method.GetParameters().Select(x => x.ParameterType).ToArray();
            var mi = type.GetMethod(method.Name, types);

            var baseHandlers = type.GetCustomAttributes<CallHandler>().ToList();
            var hanlders = mi.GetCustomAttributes<CallHandler>().ToList();

            foreach (var baseHandler in baseHandlers)
            {
                if (hanlders.All(x => x.GetType() != baseHandler.GetType()))
                {
                    hanlders.Add(baseHandler);
                }
            }

            return hanlders.OrderBy(x => x.Order).ToList();
        }
    }
}
using System;
using System.Reflection;
using log4net;

namespace NSpec.Domain
{
    public class MethodContext : Context
    {
        private static readonly ILog Log = LogManager.GetLogger( "NSpec.Domain.MethodContext" );

        public MethodContext(MethodInfo method)
            : base(method.Name, 0)
        {
            this.method = method;
        }

        public override void Build(nspec instance)
        {
            Log.DebugFormat( "Method:Build - Name: {0}", this.Name );
            Log.DebugFormat( "--instance is: {0}", (instance == null ? "NULL" : "NOT NULL") );

            base.Build(instance);

            try
            {
                method.Invoke(instance, null);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception executing context: {0}".With(FullContext()));

                throw e;
            }
        }

        private MethodInfo method;
    }
}
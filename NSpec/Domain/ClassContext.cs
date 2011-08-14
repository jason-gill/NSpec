using System;
using log4net;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public class ClassContext : Context
    {
        private static readonly ILog Log = LogManager.GetLogger( "NSpec.Domain.ClassContext" );
        
        private void BuildMethodLevelBefore()
        {
            Log.DebugFormat( "Method:BuildMethodLevelBefore - Name: {0}", this.Name );

            var before = conventions.GetMethodLevelBefore(type);

            Log.DebugFormat( "--before is {0}", (before == null ? "NULL" : "NOT NULL") );

            if (before != null) BeforeInstance = i => before.Invoke(i, null);
        }

        private void BuildMethodLevelAct()
        {
            Log.DebugFormat( "Method:BuildMethodLevelAct - Name: {0}", this.Name );

            var act = conventions.GetMethodLevelAct(type);

            Log.DebugFormat( "--act is {0}", (act == null ? "NULL" : "NOT NULL") );
            
            if (act != null) ActInstance = i => act.Invoke(i, null);
        }

        public ClassContext(Type type, Conventions conventions = null)
            : base(type.Name, 0)
        {
            this.type = type;

            this.conventions = conventions ?? new DefaultConventions().Initialize();
        }

        public override void Build(nspec instance=null)
        {
            Log.DebugFormat( "Method:Build - Name: {0}", this.Name );
            Log.InfoFormat( "--instance is: {0}", (instance == null ? "NULL" : "NOT NULL") );

            BuildMethodLevelBefore();

            BuildMethodLevelAct();

            var nspec = type.Instance<nspec>();

            base.Build(nspec);
        }

        public override bool IsSub(Type baseType)
        {
            Log.DebugFormat( "Method:IsSub - Name: {0}", this.Name );
            Log.InfoFormat( "--baseType == type: {0}", (baseType == type ? "YES" : "NO") );
            return baseType == type;
        }

        Conventions conventions;
        Type type;

    }
}
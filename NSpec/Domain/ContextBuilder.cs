using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using NSpec.Domain.Extensions;

namespace NSpec.Domain
{
    public class ContextBuilder
    {
        private static readonly ILog Log = LogManager.GetLogger( "NSpec.Domain.ContextBuilder" );

        public ContextBuilder(ISpecFinder finder, Conventions conventions)
        {
            Log.Debug( "Constructor" );

            this.finder = finder;

            contexts = new ContextCollection();

            this.conventions = conventions;
        }

        public ContextCollection Contexts()
        {
            Log.Debug( "Method:Contexts" );

            contexts.Clear();

            conventions.Initialize();

            var specClasses = finder.SpecClasses();

            var container = new ClassContext(typeof(nspec), conventions);

            Build(container, specClasses);

            contexts.AddRange(container.Contexts);

            return contexts;
        }

        private void Build(Context parent, IEnumerable<Type> allSpecClasses)
        {
            Log.Debug( "Method:Build (recursive)" );

            var derivedTypes = allSpecClasses.Where(s => parent.IsSub( s.BaseType) );

            foreach (var derived in derivedTypes)
            {
                Log.DebugFormat( "--{0}", derived.Name );
                
                var classContext = CreateClassContext(derived, conventions);

                parent.AddContext(classContext);

                Build(classContext, allSpecClasses);
            }
        }

        private ClassContext CreateClassContext(Type type, Conventions conventions)
        {
            Log.Debug( "Method:CreateClassContext" );

            var context = new ClassContext(type, conventions);

            BuildMethodContexts(context, type);

            BuildMethodLevelExamples(context, type);

            return context;
        }

        public void BuildMethodContexts(Context classContext, Type specClass)
        {
            Log.Debug( "Method:BuildMethodContexts" );

            specClass
                .Methods()
                .Where(s => conventions.IsMethodLevelContext(s.Name)).Do(
                contextMethod =>
                {
                    Log.DebugFormat( "--{0}", contextMethod.Name );
                    classContext.AddContext(new MethodContext(contextMethod));
                });
        }

        public void BuildMethodLevelExamples(Context classContext, Type specClass)
        {
            Log.Debug( "Method:BuildMethodLevelExamples" );

            specClass
                .Methods()
                .Where(s => conventions.IsMethodLevelExample(s.Name)).Do(
                methodInfo =>
                {
                    Log.DebugFormat( "--{0}", methodInfo.Name );
                    classContext.AddExample(new Example(methodInfo));
                });
        }

        private Conventions conventions;
        
        private ISpecFinder finder;

        private ContextCollection contexts;
    }
}
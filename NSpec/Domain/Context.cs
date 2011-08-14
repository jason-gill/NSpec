using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using log4net;

namespace NSpec.Domain
{
    public class Context
    {
        private static readonly ILog Log = LogManager.GetLogger( "NSpec.Domain.Context" );

        public void RunBefores(nspec instance)
        {
            Log.DebugFormat( "Method:RunBefores - Name: {0}", this.Name );

            Log.DebugFormat( "--Parent is: {0}", (Parent == null ? "NULL" : "NOT NULL") );
            if (Parent != null) Parent.RunBefores(instance);

            Log.DebugFormat( "--BeforeInstance is: {0}", (BeforeInstance == null ? "NULL" : "NOT NULL") );
            if (BeforeInstance != null) BeforeInstance(instance);

            Log.DebugFormat( "--Before is: {0}", (Before == null ? "NULL" : "NOT NULL") );
            if (Before != null) Before();
        }

        public void RunActs(nspec instance)
        {
            Log.DebugFormat( "Method:RunActs - Name: {0}", this.Name );

            Log.DebugFormat( "--Parent is: {0}", (Parent == null ? "NULL" : "NOT NULL") );
            if (Parent != null) Parent.RunActs(instance);

            Log.DebugFormat( "--ActInstance is: {0}", (ActInstance == null ? "NULL" : "NOT NULL") );
            if (ActInstance != null) ActInstance(instance);

            Log.DebugFormat( "--Act is: {0}", (Act == null ? "NULL" : "NOT NULL") );
            if (Act != null) Act();
        }

        public void Afters()
        {
            if (After != null) After();
        }

        public void AddExample(Example example)
        {
            Log.DebugFormat( "Method:AddExample - Name: {0}", this.Name );
            Log.DebugFormat( "--example is: {0}", example.Spec );

            example.Context = this;

            Examples.Add(example);

            example.Pending |= IsPending();
        }

        public IEnumerable<Example> AllExamples()
        {
            return Contexts.Examples().Union(Examples);
        }

        public bool IsPending()
        {
            return isPending || (Parent != null && Parent.IsPending());
        }

        public IEnumerable<Example> Failures()
        {
            return AllExamples().Where(e => e.Exception != null);
        }

        public void AddContext(Context child)
        {
            child.Parent = this;

            Contexts.Add(child);
        }

        public virtual void Run(nspec instance = null)
        {
            Log.DebugFormat( "Method:Run - Name: {0}", this.Name );
            Log.DebugFormat( "--instance is: {0}", (instance == null ? "NULL" : "NOT NULL") );

            var nspec = savedInstance ?? instance;

            Contexts.Do(c => c.Run(nspec));

            for (int i = 0; i < Examples.Count; i++)
                Exercise(Examples[i], nspec);
        }

        public virtual void Build(nspec instance=null)
        {
            Log.DebugFormat( "Method:Build - Name: {0}", this.Name );
            Log.DebugFormat( "--instance is: {0}", (instance == null ? "NULL" : "NOT NULL") );

            instance.Context = this;

            savedInstance = instance;

            Contexts.Do(c => c.Build(instance));
        }

        public string FullContext()
        {
            return Parent != null ? Parent.FullContext() + ". " + Name : Name;
        }

        public void Exercise(Example example, nspec nspec)
        {
            Log.DebugFormat( "Method:Exercise - Name: {0}", this.Name );
            Log.DebugFormat( "--example is: {0}", example.Spec );
            Log.DebugFormat( "--nspec is: {0}", (nspec == null ? "NULL" : "NOT NULL") );

            if (example.Pending) return;

            if (contextLevelFailure != null)
            {
                example.Exception = contextLevelFailure;
                return;
            }

            try
            {
                RunBefores(nspec);

                RunActs(nspec);

                example.Run(nspec);

                Afters();
            }
            catch (TargetInvocationException e)
            {
                example.Exception = e.InnerException;
            }
            catch (Exception e)
            {
                example.Exception = e;
            }
        }

        public virtual bool IsSub(Type baseType)
        {
            return false;
        }

        public Context(string name = "", int level = 0, bool isPending = false)
        {
            Name = name.Replace("_", " ");
            Level = level;
            Examples = new List<Example>();
            Contexts = new ContextCollection();
            this.isPending = isPending;
        }

        public string Name;
        public int Level;
        public List<Example> Examples;
        public ContextCollection Contexts;
        public Action Before, Act, After;
        public Action<nspec> BeforeInstance, ActInstance;
        public Context Parent;
        public Exception contextLevelFailure;
        private bool isPending;
        nspec savedInstance;

        public nspec GetInstance()
        {
            return savedInstance;
        }
    }
}
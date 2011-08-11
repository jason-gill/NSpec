using System.Collections.Generic;
using System.Linq;

namespace NSpec.Domain
{
    public class ContextCollection : List<Context>, IContextScore
    {
        public ContextCollection(IEnumerable<Context> contexts) :base(contexts){}

        public ContextCollection(){}

        public IEnumerable<Example> AllExamples()
        {
            return this.SelectMany(c => c.AllExamples());
        }

        public IEnumerable<Example> Failures()
        {
            return this.AllExamples().Where(e => e.Exception != null);
        }

        public IEnumerable<Example> Pendings()
        {
            return this.AllExamples().Where(e => e.Pending);
        }
    }
}
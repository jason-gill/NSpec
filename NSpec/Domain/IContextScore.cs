using System.Collections.Generic;

namespace NSpec.Domain
{
    public interface IContextScore
    {
        IEnumerable<Example> AllExamples();
        IEnumerable<Example> Failures();
        IEnumerable<Example> Pendings();
    }
}
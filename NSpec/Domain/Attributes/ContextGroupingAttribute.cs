using System;

namespace NSpec.Domain.Attributes
{
    [AttributeUsage( AttributeTargets.Class | AttributeTargets.Method )]
    public class ContextGroupingAttribute : Attribute
    {
    }
}
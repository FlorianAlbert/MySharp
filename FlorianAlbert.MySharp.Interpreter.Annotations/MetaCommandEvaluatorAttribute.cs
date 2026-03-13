namespace FlorianAlbert.MySharp.Interpreter.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MetaCommandEvaluatorAttribute : Attribute
{
    public MetaCommandEvaluatorAttribute(string handlerMethodName, string? infoMethodName = null)
    {
        HandlerMethodName = handlerMethodName;
        InfoMethodName = infoMethodName;
    }

    public string HandlerMethodName { get; }

    public string? InfoMethodName { get; }
}
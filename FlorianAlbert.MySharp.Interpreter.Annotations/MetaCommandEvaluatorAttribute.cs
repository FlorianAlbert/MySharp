namespace FlorianAlbert.MySharp.Interpreter.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MetaCommandEvaluatorAttribute : Attribute
{
    public MetaCommandEvaluatorAttribute(string handlerMethodName)
    {
        HandlerMethodName = handlerMethodName;
    }

    public string HandlerMethodName { get; }
}
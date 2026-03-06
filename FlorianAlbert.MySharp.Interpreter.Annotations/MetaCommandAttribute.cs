namespace FlorianAlbert.MySharp.Interpreter.Annotations;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class MetaCommandAttribute : Attribute
{
    public MetaCommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; }

    public string Description { get; }

    public string[] Aliases { get; set; } = [];
}

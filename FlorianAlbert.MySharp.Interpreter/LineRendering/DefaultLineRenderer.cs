namespace FlorianAlbert.MySharp.Interpreter.LineRendering;

internal class DefaultLineRenderer : LineRenderer
{
    public override void RenderLine(string line)
    {
        Console.Write(line);
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace FlorianAlbert.MySharp.Sdk.Parser.CodeAnalysis.Binding;

internal sealed class BoundVariableDeclarationStatement : BoundStatement
{
    public BoundVariableDeclarationStatement(VariableSymbol variable, BoundExpression valueExpression)
    {
        Variable = variable;
        ValueExpression = valueExpression;
    }

    public override BoundNodeKind Kind => BoundNodeKind.VariableDeclarationStatement;

    public VariableSymbol Variable { get; }

    public BoundExpression ValueExpression { get; }
}

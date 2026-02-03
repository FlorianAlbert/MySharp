# Copilot Instructions for MySharp

## Build, Test, and Run

```bash
# Build
dotnet build

# Run all tests
dotnet test

# Run a single test
dotnet test --filter "FullyQualifiedClassName.TestMethodName"
e.g. dotnet test --filter "FlorianAlbert.MySharp.Sdk.Parser.Test.CodeAnalysis.EvaluatorTests.Evaluator_EvaluatesExpression_Correctly"

# Run the interpreter REPL
dotnet run --project FlorianAlbert.MySharp.Interpreter
```

## Architecture

MySharp is a custom programming language with a classic compiler pipeline:

```
Source Code → Lexer → Parser → Binder   → Lowerer → Evaluator → Result
              (Syntax)         (Binding)  (Lowering)
```

### Project Structure

- **FlorianAlbert.MySharp.Sdk.Parser** - Core compiler library containing all phases
- **FlorianAlbert.MySharp.Interpreter** - Console REPL that uses the parser/evaluator
- **FlorianAlbert.MySharp.Sdk.Parser.Test** - xUnit tests for the evaluator

### Compiler Phases

1. **Lexer** (`CodeAnalysis/Syntax/Lexer.cs`) - Tokenizes source text into `SyntaxToken`s
2. **Parser** (`CodeAnalysis/Syntax/Parser.cs`) - Builds a concrete syntax tree (CST) from tokens
3. **Binder** (`CodeAnalysis/Binding/Binder.cs`) - Performs semantic analysis, produces bound tree with type information
4. **Lowerer** (`CodeAnalysis/Lowering/Lowerer.cs`) - Transforms high-level constructs (if/while/for) into lower-level goto/labels
5. **Evaluator** (`CodeAnalysis/Evaluator.cs`) - Tree-walking interpreter that executes the lowered bound tree

### Key Abstractions

- `SyntaxTree` - Entry point for parsing; call `SyntaxTree.Parse(text)` to parse source code
- `Compilation` - Orchestrates binding and evaluation; supports chaining via `ContinueWith()` for REPL state
- `DiagnosticBag` - Collects errors throughout compilation phases
- `BoundScope` - Tracks variable declarations with proper scoping rules

## Language Features

MySharp currently supports:
- Types: `int` and `bool`
- Variables: `var` (mutable) and `let` (immutable)
- Operators: arithmetic, bitwise, logical, comparison
- Control flow: `if`/`else`, `while`, `for`
- Blocks with `{ }` and lexical scoping
- Statements require semicolons

## Conventions

### Test Patterns

Tests use `AnnotatedText` to mark expected diagnostic spans with `[brackets]`:
```csharp
string text = @"[x] * 10;";  // Error expected at 'x'
```

### Adding New Syntax

1. Add token kind to `SyntaxKind.cs`
2. Update `Lexer.cs` to recognize the token
3. Add syntax node classes in `Syntax/Expressions/` or `Syntax/Statements/`
4. Update `Parser.cs` to parse the new construct
5. Add bound node classes in `Binding/`
6. Update `Binder.cs` for semantic analysis
7. Update `Lowerer.cs` if the construct needs lowering
8. Update `Evaluator.cs` to execute the construct

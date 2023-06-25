using System;
using Core.Emulator.Domain;
using Core.Emulator.Extensions;
using Microsoft.CodeAnalysis;

namespace Core.Emulator
{
    [Generator]
    public class LauncherGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!context.Compilation.Assembly.HasLauncherAttribute()) return;

            try
            {
                var root = new Root();

                root.Resolve(context);

                var saveCode = root.GetCode();

                context.AddSource("Save.cs", saveCode);

                foreach (var @object in root.Objects.Values)
                {
                    var objectCode = @object.GetCode();

                    context.AddSource(@object.Name, objectCode);
                }
            }
            catch(Exception e)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor(
                            "LG0001",
                            "Launcher generator failed",
                            "Launcher generator failed {0}",
                            "General",
                            DiagnosticSeverity.Error,
                            true),
                        null,
                        $"{e}"));

                throw;
            }
        }
    }
}

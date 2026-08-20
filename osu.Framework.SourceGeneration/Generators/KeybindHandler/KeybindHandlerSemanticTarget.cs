// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using osu.Framework.SourceGeneration.Generators.KeybindHandler.Data;

namespace osu.Framework.SourceGeneration.Generators.KeybindHandler
{
    public class KeybindHandlerSemanticTarget : IncrementalSemanticTarget
    {
        public readonly Dictionary<string, HashSet<KeybindHandlerAttributeData>> DataToProcess = new Dictionary<string, HashSet<KeybindHandlerAttributeData>>();

        public KeybindHandlerSemanticTarget(ClassDeclarationSyntax classSyntax, SemanticModel semanticModel)
            : base(classSyntax, semanticModel)
        {
        }

        protected override bool CheckValid(INamedTypeSymbol symbol)
        {
            INamedTypeSymbol? s = symbol;

            bool hasDrawableInHierarchy = false;

            while (s != null && !hasDrawableInHierarchy)
            {
                if (isDrawableType(s))
                    hasDrawableInHierarchy = true;

                s = s.BaseType;
            }

            if (!hasDrawableInHierarchy)
                return false;

            foreach (var member in symbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                if (method.GetAttributes().Where(SyntaxHelpers.IsKeybindHandlerAttribute).Any())
                    return true;
            }

            return false;
        }

        protected override bool CheckNeedsOverride(INamedTypeSymbol symbol) => Enumerable.Any(symbol.AllInterfaces, isIKeyBindingHandlerType);

        protected override void Process(INamedTypeSymbol symbol)
        {
            foreach (var member in symbol.GetMembers())
            {
                if (member is not IMethodSymbol method)
                    continue;

                foreach (var attrib in method.GetAttributes().Where(SyntaxHelpers.IsKeybindHandlerAttribute))
                {
                    TypedConstant keybindActionArgument = attrib.ConstructorArguments[0];
                    var keybindActionType = keybindActionArgument.Type;

                    if (keybindActionType?.IsValueType != true)
                        continue; // Maybe throw an exception?

                    string fullTypeName = keybindActionType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

                    HashSet<KeybindHandlerAttributeData> dataSet;

                    if (!DataToProcess.TryGetValue(fullTypeName, out dataSet))
                    {
                        dataSet = new HashSet<KeybindHandlerAttributeData>();
                        DataToProcess.Add(fullTypeName, dataSet);
                    }

                    dataSet.Add(KeybindHandlerAttributeData.FromMethod(method, keybindActionArgument));
                }
            }
        }

        private static bool isDrawableType(INamedTypeSymbol type)
            => SyntaxHelpers.GetFullyQualifiedTypeName(type) == "osu.Framework.Graphics.Drawable";

        private static bool isIKeyBindingHandlerType(INamedTypeSymbol type)
            => SyntaxHelpers.GetFullyQualifiedTypeName(type) == "osu.Framework.Input.Bindings.IKeyBindingHandler";
    }
}

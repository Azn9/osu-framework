// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using osu.Framework.SourceGeneration.Generators.KeybindHandler.Data;

namespace osu.Framework.SourceGeneration.Generators.KeybindHandler.Emitters
{
    public static class KeybindReleaseEventEmitter
    {
        private const string method_name = "OnReleased";
        private const string global_event_parameter_type = "global::osu.Framework.Input.Events.KeyBindingReleaseEvent";
        private const string event_parameter_type = "osu.Framework.Input.Events.KeyBindingReleaseEvent";
        private const string event_parameter_name = "e";

        public static MemberDeclarationSyntax Create(string type, HashSet<KeybindHandlerAttributeData> dataSet, bool needsOverride)
        {
            return SyntaxFactory.MethodDeclaration(
                                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                                    method_name)
                                .WithModifiers(
                                    SyntaxFactory.TokenList(createModifiers(needsOverride)))
                                .WithParameterList(
                                    SyntaxFactory.ParameterList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Parameter(
                                                             SyntaxFactory.Identifier(event_parameter_name))
                                                         .WithType(
                                                             SyntaxFactory.GenericName(global_event_parameter_type)
                                                                          .WithTypeArgumentList(SyntaxFactory.TypeArgumentList(
                                                                              SyntaxFactory.SingletonSeparatedList(
                                                                                  SyntaxFactory.ParseTypeName(type)
                                                                              )))))))
                                .WithBody(SyntaxFactory.Block(
                                    emitBody()
                                ));
        }

        private static IEnumerable<SyntaxToken> createModifiers(bool needsOverride)
        {
            yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);

            if (needsOverride)
                yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        }

        private static IEnumerable<StatementSyntax> emitBody()
        {
            yield return SyntaxFactory.ReturnStatement();
        }
    }
}

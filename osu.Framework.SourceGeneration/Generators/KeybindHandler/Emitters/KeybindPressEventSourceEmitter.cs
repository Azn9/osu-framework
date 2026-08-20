// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using osu.Framework.SourceGeneration.Generators.KeybindHandler.Data;

namespace osu.Framework.SourceGeneration.Generators.KeybindHandler.Emitters
{
    public static class KeybindPressEventSourceEmitter
    {
        private const string method_name = "OnPressed";
        private const string global_event_parameter_type = "global::osu.Framework.Input.Events.KeyBindingPressEvent";
        private const string event_parameter_type = "osu.Framework.Input.Events.KeyBindingPressEvent";
        private const string event_parameter_name = "e";

        public static MemberDeclarationSyntax Create(string type, HashSet<KeybindHandlerAttributeData> dataSet, bool needsOverride)
        {
            return SyntaxFactory.MethodDeclaration(
                                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
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
                                    emitBody(dataSet, needsOverride)
                                ));
        }

        private static IEnumerable<SyntaxToken> createModifiers(bool needsOverride)
        {
            yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);

            if (needsOverride)
                yield return SyntaxFactory.Token(SyntaxKind.PublicKeyword);
        }

        private static IEnumerable<StatementSyntax> emitBody(HashSet<KeybindHandlerAttributeData> dataSet, bool needsOverride)
        {
            foreach (var data in dataSet)
            {
                yield return SyntaxFactory.IfStatement(
                    SyntaxFactory.BinaryExpression(
                        SyntaxKind.EqualsExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(event_parameter_name),
                            SyntaxFactory.IdentifierName("Action")),
                        createArgument(data)),
                    createMethodCall(data, needsOverride)
                ).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
            }

            yield return createReturnCall(needsOverride);
        }

        private static StatementSyntax createReturnCall(bool needsOverride)
        {
            if (needsOverride)
            {
                return SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                                     SyntaxFactory.MemberAccessExpression(
                                         SyntaxKind.SimpleMemberAccessExpression,
                                         SyntaxFactory.BaseExpression(),
                                         SyntaxFactory.IdentifierName(method_name)))
                                 .WithArgumentList(
                                     SyntaxFactory.ArgumentList(
                                         SyntaxFactory.SingletonSeparatedList(
                                             SyntaxFactory.Argument(
                                                 SyntaxFactory.IdentifierName(event_parameter_name))))));
            }

            return SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression));
        }

        private static StatementSyntax createMethodCall(KeybindHandlerAttributeData data, bool needsOverride)
        {
            var methodCall = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(data.Method.Name));

            if (data.Method.Parameters.Length > 0)
            {
                if (data.Method.Parameters.Length != 1)
                {
                    throw new NotSupportedException("Methods should have either zero or one event parameter");
                }

                var firstParameter = data.Method.Parameters[0];
                if (!SyntaxHelpers.GetFullyQualifiedTypeName(firstParameter.Type).StartsWith(event_parameter_type, StringComparison.Ordinal))
                    throw new NotSupportedException("Methods should have either zero or one event parameter");

                methodCall = methodCall.WithArgumentList(
                    SyntaxFactory.ArgumentList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Argument(
                                SyntaxFactory.IdentifierName(event_parameter_name)))));
            }

            if (data.Method.ReturnsVoid)
                return SyntaxFactory.Block(callAndReturnTrue(methodCall));

            if (!needsOverride)
                return SyntaxFactory.ReturnStatement(methodCall);

            if (data.Method.ReturnType.SpecialType == SpecialType.System_Boolean)
                return SyntaxFactory.Block(callAndReturnTrueOrBase(methodCall));

            throw new NotSupportedException("Methods should return void or bool");
        }

        private static IEnumerable<StatementSyntax> callAndReturnTrue(InvocationExpressionSyntax methodCall)
        {
            yield return SyntaxFactory.ExpressionStatement(methodCall);
            yield return SyntaxFactory.ReturnStatement(SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression));
        }

        private static IEnumerable<StatementSyntax> callAndReturnTrueOrBase(InvocationExpressionSyntax methodCall)
        {
            yield return SyntaxFactory.LocalDeclarationStatement(
                SyntaxFactory.VariableDeclaration(
                                 SyntaxFactory.PredefinedType(
                                     SyntaxFactory.Token(SyntaxKind.BoolKeyword)))
                             .WithVariables(
                                 SyntaxFactory.SingletonSeparatedList(
                                     SyntaxFactory.VariableDeclarator(
                                                      SyntaxFactory.Identifier("ret"))
                                                  .WithInitializer(
                                                      SyntaxFactory.EqualsValueClause(methodCall)))));

            yield return SyntaxFactory.IfStatement(
                SyntaxFactory.IdentifierName("ret"),
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));

            yield return SyntaxFactory.ReturnStatement(
                SyntaxFactory.InvocationExpression(
                                 SyntaxFactory.MemberAccessExpression(
                                     SyntaxKind.SimpleMemberAccessExpression,
                                     SyntaxFactory.BaseExpression(),
                                     SyntaxFactory.IdentifierName(method_name)))
                             .WithArgumentList(
                                 SyntaxFactory.ArgumentList(
                                     SyntaxFactory.SingletonSeparatedList(
                                         SyntaxFactory.Argument(
                                             SyntaxFactory.IdentifierName(event_parameter_name))))));
        }

        private static ExpressionSyntax createArgument(KeybindHandlerAttributeData data)
        {
            if (data.KeybindActionArgument.Kind != TypedConstantKind.Enum)
                throw new NotSupportedException("Only enums are supported");

            var enumMember = data.KeybindActionArgument.Type!
                                 .GetMembers()
                                 .OfType<IFieldSymbol>()
                                 .First(x =>
                                     x.HasConstantValue &&
                                     EqualityComparer<int>.Default.Equals(
                                         (int)x.ConstantValue,
                                         (int)data.KeybindActionArgument.Value!
                                     ));

            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ParseTypeName(
                    data.KeybindActionArgument.Type!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)),
                SyntaxFactory.IdentifierName(enumMember.Name));
        }
    }
}

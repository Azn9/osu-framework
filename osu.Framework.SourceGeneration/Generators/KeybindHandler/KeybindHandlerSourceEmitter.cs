// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using osu.Framework.SourceGeneration.Generators.KeybindHandler.Emitters;

namespace osu.Framework.SourceGeneration.Generators.KeybindHandler
{
    public class KeybindHandlerSourceEmitter : IncrementalSourceEmitter
    {
        private const string interface_name = "global::osu.Framework.Input.Bindings.IKeyBindingHandler";
        protected override string FileSuffix => "KeybindHandler";

        public new KeybindHandlerSemanticTarget Target => (KeybindHandlerSemanticTarget)base.Target;

        public KeybindHandlerSourceEmitter(IncrementalSemanticTarget target)
            : base(target)
        {
        }

        protected override ClassDeclarationSyntax ConstructClass(ClassDeclarationSyntax initialClass)
        {
            return initialClass.WithBaseList(
                                   SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(createBaseList())))
                               .WithMembers(
                                   SyntaxFactory.List(createMembers()));
        }

        private IEnumerable<BaseTypeSyntax> createBaseList()
        {
            foreach (string type in Target.DataToProcess.Keys)
            {
                yield return createBaseType(type);
            }

            yield break;

            BaseTypeSyntax createBaseType(string type) =>
                SyntaxFactory.SimpleBaseType(
                    SyntaxFactory.GenericName(interface_name)
                                 .WithTypeArgumentList(
                                     SyntaxFactory.TypeArgumentList(
                                         SyntaxFactory.SingletonSeparatedList(
                                             SyntaxFactory.ParseTypeName(type)
                                         ))));
        }

        private IEnumerable<MemberDeclarationSyntax> createMembers()
        {
            bool targetNeedsOverride = Target.NeedsOverride;

            foreach (string type in Target.DataToProcess.Keys)
            {
                var dataSet = Target.DataToProcess[type];

                yield return KeybindPressEventSourceEmitter.Create(type, dataSet, targetNeedsOverride);
                yield return KeybindReleaseEventEmitter.Create(type, dataSet, targetNeedsOverride);
            }
        }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.CodeAnalysis;

namespace osu.Framework.SourceGeneration.Generators.KeybindHandler.Data
{
    public class KeybindHandlerAttributeData
    {
        public readonly IMethodSymbol Method;
        public readonly TypedConstant KeybindActionArgument;

        private KeybindHandlerAttributeData(IMethodSymbol method, TypedConstant keybindActionArgument)
        {
            Method = method;
            KeybindActionArgument = keybindActionArgument;
        }

        public static KeybindHandlerAttributeData FromMethod(IMethodSymbol method, TypedConstant keybindActionArgument)
        {
            return new KeybindHandlerAttributeData(method, keybindActionArgument);
        }
    }
}

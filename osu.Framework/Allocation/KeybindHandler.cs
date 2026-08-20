// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;

namespace osu.Framework.Allocation
{
    [MeansImplicitUse(ImplicitUseKindFlags.Access)]
    [AttributeUsage(AttributeTargets.Method)]
    public class KeybindHandler(object keybindAction) : Attribute;
}

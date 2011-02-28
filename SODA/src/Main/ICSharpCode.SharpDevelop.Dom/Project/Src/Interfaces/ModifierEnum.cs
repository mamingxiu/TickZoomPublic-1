// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Daniel Grunwald" email="daniel@danielgrunwald.de"/>
//     <version>$Revision: 1703 $</version>
// </file>

using System;
using M = ICSharpCode.NRefactory.Ast.Modifiers;

namespace ICSharpCode.SharpDevelop.Dom
{
	[Flags]
	public enum ModifierEnum // must be the same values as NRefactories' ModifierEnum
	{
		None       = 0,
		
		// Access
		Private   = M.Private,
		Internal  = M.Internal, // == Friend
		Protected = M.Protected,
		Public    = M.Public,
		Dim	      = M.Dim,	// VB.NET SPECIFIC
		
		// Scope
		Abstract  = M.Abstract,  // == 	MustOverride/MustInherit
		Virtual   = M.Virtual,
		Sealed    = M.Sealed,
		Static    = M.Static,
		Override  = M.Override,
		Readonly  = M.ReadOnly,
		Const	  = M.Const,
		New       = M.New,  // == Shadows
		Partial   = M.Partial,
		
		// Special
		Extern     = M.Extern,
		Volatile   = M.Volatile,
		Unsafe     = M.Unsafe,
		Overloads  = M.Overloads, // VB specific
		WithEvents = M.WithEvents, // VB specific
		Default    = M.Default, // VB specific
		Fixed      = M.Fixed,
		
		Synthetic = M.Synthetic,
		
		ProtectedAndInternal = Internal | Protected,
		VisibilityMask = Private | Internal | Protected | Public,
	}
}

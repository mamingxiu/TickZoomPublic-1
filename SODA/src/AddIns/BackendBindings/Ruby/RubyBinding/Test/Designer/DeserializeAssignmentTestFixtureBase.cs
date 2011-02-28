// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 5343 $</version>
// </file>

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Windows.Forms;
using ICSharpCode.RubyBinding;
using IronRuby.Compiler.Ast;
using NUnit.Framework;
using RubyBinding.Tests.Utils;

namespace RubyBinding.Tests.Designer
{
	/// <summary>
	/// Base class for all tests of the RubyCodeDeserialize when deserializing an 
	/// assignment.
	/// </summary>
	public abstract class DeserializeAssignmentTestFixtureBase
	{
		protected Expression rhsAssignmentExpression;
		protected object deserializedObject;
		protected MockDesignerLoaderHost mockDesignerLoaderHost;
		protected MockTypeResolutionService typeResolutionService;
		protected MockComponentCreator componentCreator;
		
		[TestFixtureSetUp]
		public void SetUpFixture()
		{
			componentCreator = new MockComponentCreator();

			SimpleAssignmentExpression assignment = RubyParserHelper.GetSimpleAssignmentExpression(GetRubyCode());
			rhsAssignmentExpression = assignment.Right;
			
			mockDesignerLoaderHost = new MockDesignerLoaderHost();
			typeResolutionService = mockDesignerLoaderHost.TypeResolutionService;
			RubyCodeDeserializer deserializer = new RubyCodeDeserializer(componentCreator);
			deserializedObject = deserializer.Deserialize(rhsAssignmentExpression);
		}
		
		public abstract string GetRubyCode();
	}
}

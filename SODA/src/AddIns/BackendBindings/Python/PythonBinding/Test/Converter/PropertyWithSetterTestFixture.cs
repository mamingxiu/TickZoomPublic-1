// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 4761 $</version>
// </file>

using System;
using ICSharpCode.NRefactory;
using ICSharpCode.PythonBinding;
using NUnit.Framework;

namespace PythonBinding.Tests.Converter
{
	[TestFixture]
	public class PropertyWithSetterTestFixture
	{
		string csharp = "class Foo\r\n" +
						"{\r\n" +
						"    int count = 0;\r\n" +
						"    int i = 0;\r\n" +
						"    public int Count {\r\n" +
						"        set {\r\n" +
						"            count = value;\r\n" +
						"        }\r\n" +
						"    }\r\n" +
						"}";
			
		[Test]
		public void ConvertedPythonCode()
		{
			NRefactoryToPythonConverter converter = new NRefactoryToPythonConverter(SupportedLanguage.CSharp);
			converter.IndentString = "    ";
			string python = converter.Convert(csharp);
			string expectedPython = "class Foo(object):\r\n" +
									"    def __init__(self):\r\n" +
									"        self._count = 0\r\n" +
									"        self._i = 0\r\n" +
									"\r\n" +
									"    def set_Count(self, value):\r\n" +
									"        self._count = value\r\n" +
									"\r\n" +
									"    Count = property(fset=set_Count)";
			
			Assert.AreEqual(expectedPython, python, python);
		}
	}
}

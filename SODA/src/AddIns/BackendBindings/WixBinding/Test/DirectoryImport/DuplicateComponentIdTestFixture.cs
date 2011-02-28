// <file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Matthew Ward" email="mrward@users.sourceforge.net"/>
//     <version>$Revision: 2785 $</version>
// </file>

using ICSharpCode.SharpDevelop.Project;
using ICSharpCode.WixBinding;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using WixBinding.Tests.PackageFiles;
using WixBinding.Tests.Utils;

namespace WixBinding.Tests.DirectoryImport
{
	/// <summary>
	/// Adds several directories and its contained files to the setup package.
	/// The original package has several components already defined. This
	/// test fixture checks that unique component ids are generated for the new
	/// files.
	/// </summary>
	[TestFixture]
	public class DuplicateComponentIdTestFixture : PackageFilesTestFixtureBase
	{			
		WixDirectoryElement appDirectoryElement;
		WixComponentElement readmeComponentElement;
		WixComponentElement licenseComponentElement;
		WixComponentElement exeComponentElement;
		WixNamespaceManager nsManager;
		string directory = @"C:\Projects\Test\MyApp";
		string directory2 = @"C:\Projects\Test\a-app";
		string[] files = new string[] {"MyApp.exe", "readme.txt", "license.txt"};
		
		[SetUp]
		public void Init()
		{
			base.InitFixture();
			editor.AddDirectory(directory);
			
			nsManager = new WixNamespaceManager(editor.Document.NameTable);
			appDirectoryElement = (WixDirectoryElement)editor.Document.RootDirectory.SelectSingleNode("w:Directory[@Name='MyApp']", nsManager);
			readmeComponentElement = (WixComponentElement)appDirectoryElement.SelectSingleNode("w:Component[w:File/@Name='readme.txt']", nsManager);
			licenseComponentElement = (WixComponentElement)appDirectoryElement.SelectSingleNode("w:Component[w:File/@Name='license.txt']", nsManager);	
			exeComponentElement = (WixComponentElement)appDirectoryElement.SelectSingleNode("w:Component[w:File/@Name='MyApp.exe']", nsManager);
		}

		/// <summary>
		/// Parent directory is added to component id.
		/// </summary>
		[Test]
		public void ReadmeComponentId()
		{
			Assert.AreEqual("MyAppReadmeTxt", readmeComponentElement.Id);
		}
		
		[Test]
		public void LicenseComponentId()
		{
			Assert.AreEqual("MyAppLicenseTxt1", licenseComponentElement.Id);
		}
		
		[Test]
		public void AppExeComponentId()
		{
			Assert.AreEqual("MyAppMyAppExe2", exeComponentElement.Id);
		}
		
		[Test]
		public void AddDirectoryWithHyphen()
		{
			view.SelectedElement = null;
			editor.AddDirectory(directory2);
						
			WixDirectoryElement directoryElement = (WixDirectoryElement)editor.Document.RootDirectory.SelectSingleNode("w:Directory[@Name='a-app']", nsManager);
			WixComponentElement exeComponentElement = (WixComponentElement)directoryElement.SelectSingleNode("w:Component[w:File/@Name='MyApp.exe']", nsManager);

			Assert.AreEqual("A_appMyAppExe", exeComponentElement.Id);
		}
		
		public override string[] GetFiles(string path)
		{
			if (path == directory) {
				return files;
			} else if (path == directory2) {
				return files;
			}
			return new string[0];
		}
		
		public override string[] GetDirectories(string path)
		{
			return new string[0];
		}
		
		protected override string GetWixXml()
		{
			return "<Wix xmlns=\"http://schemas.microsoft.com/wix/2006/wi\">\r\n" +
				"\t<Product Name=\"MySetup\" \r\n" +
				"\t         Manufacturer=\"\" \r\n" +
				"\t         Id=\"F4A71A3A-C271-4BE8-B72C-F47CC956B3AA\" \r\n" +
				"\t         Language=\"1033\" \r\n" +
				"\t         Version=\"1.0.0.0\">\r\n" +
				"\t\t<Package Id=\"6B8BE64F-3768-49CA-8BC2-86A76424DFE9\"/>\r\n" +
				"\t\t<Directory Id=\"TARGETDIR\" SourceName=\"SourceDir\">\r\n" +
				"\t\t\t<Directory Id=\"ProgramFilesFolder\" Name=\"PFiles\">\r\n" +
				"\t\t\t\t<Directory Id=\"INSTALLDIR\" Name=\"OtherFolder\">\r\n" +
				"\t\t\t\t\t<Component Id=\"MyAppExe\" Guid=\"999DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t\t<Component Id=\"MyAppMyAppExe\" Guid=\"111DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t\t<Component Id=\"MyAppMyAppExe1\" Guid=\"888DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t\t<Component Id=\"MyAppLicenseTxt\" Guid=\"777DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t\t<Component Id=\"LicenseTxt\" Guid=\"555DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t\t<Component Id=\"ReadmeTxt\" Guid=\"370DE542-C4A9-48DA-ACF8-09949CDCD808\" DiskId=\"1\">\r\n" +
				"\t\t\t\t\t</Component>\r\n" +
				"\t\t\t\t</Directory>\r\n" +
				"\t\t\t</Directory>\r\n" +
				"\t\t</Directory>\r\n" +
				"\t</Product>\r\n" +
				"</Wix>";
		}
	}
}

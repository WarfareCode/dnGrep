﻿using System;
using System.Collections.Generic;
using System.Text;
using MbUnit.Framework;
using dnGREP;
using System.IO;

namespace Tests
{
	[TestFixture]
	public class UtilsTest : TestBase
	{
		string sourceFolder;
		string destinationFolder;

		[TestFixtureSetUp]
		public void Initialize()
		{
			sourceFolder = GetDllPath() + "\\Files";
		}

		[SetUp]
		public void CreateTempFolder()
		{
			destinationFolder = Path.GetTempPath() + Guid.NewGuid().ToString();
			Directory.CreateDirectory(destinationFolder);
		}

		[TearDown]
		public void DeleteTempFolder()
		{
			if (Directory.Exists(destinationFolder))
				Utils.DeleteFolder(destinationFolder);
		}

		[RowTest]
		[Row("Hello world", "Hello world", 2, 1)]
		[Row("Hi", "Hi", 2, 1)]
		[Row("Hi\r\n\r\nWorld", "", 4, 2)]
		[Row("Hi\r\n\r\nWorld", "World", 6, 3)]
		[Row(null, null, 6, -1)]
		public void TestGetLine(string body, string line, int index, int lineNumber)
		{
			int returnedLineNumber = -1;
			string returnedLine = Utils.GetLine(body, index, out returnedLineNumber);
			Assert.AreEqual(returnedLine, line);
			Assert.AreEqual(returnedLineNumber, lineNumber);
		}

		[Test]
		public void TestGetContextLines()
		{
			string test = "Hi\r\nmy\r\nWorld\r\nMy name is Denis\r\nfor\r\nloop";
			List<GrepSearchResult.GrepLine> lines = Utils.GetContextLines(test, 2, 2, 3);
			Assert.AreEqual(lines[0].LineNumber, 1);
			Assert.AreEqual(lines[0].LineText, "Hi");
			Assert.AreEqual(lines[0].IsContext, true);
			Assert.AreEqual(lines[2].LineNumber, 4);
			Assert.AreEqual(lines[2].LineText, "My name is Denis");
			Assert.AreEqual(lines[2].IsContext, true);
			Assert.AreEqual(lines[3].LineNumber, 5);
			Assert.AreEqual(lines[3].LineText, "for");
			Assert.AreEqual(lines[3].IsContext, true);

			Assert.AreEqual(lines.Count, 4);

			lines = Utils.GetContextLines(test, 0, 0, 3);
			Assert.AreEqual(lines.Count, 0);

			lines = Utils.GetContextLines(null, 0, 0, 3);
			Assert.AreEqual(lines.Count, 0);

			lines = Utils.GetContextLines(test, 10, 0, 2);
			Assert.AreEqual(lines[0].LineNumber, 1);
			Assert.AreEqual(lines[0].LineText, "Hi");
			Assert.AreEqual(lines[0].IsContext, true);
			Assert.AreEqual(lines.Count, 1);

			lines = Utils.GetContextLines(test, 1, 10, 5);
			Assert.AreEqual(lines[0].LineNumber, 4);
			Assert.AreEqual(lines[0].LineText, "My name is Denis");
			Assert.AreEqual(lines[0].IsContext, true);
			Assert.AreEqual(lines[1].LineNumber, 6);
			Assert.AreEqual(lines[1].LineText, "loop");
			Assert.AreEqual(lines[1].IsContext, true);
			Assert.AreEqual(lines.Count, 2);
		}

		[Test]
		public void TestMatchCount()
		{
			GrepSearchResult result = new GrepSearchResult("test.txt", new List<GrepSearchResult.GrepLine>());
			result.SearchResults.Add(new GrepSearchResult.GrepLine(1, "test", true));
			result.SearchResults.Add(new GrepSearchResult.GrepLine(2, "test2", false));
			result.SearchResults.Add(new GrepSearchResult.GrepLine(3, "test3", false));
			result.SearchResults.Add(new GrepSearchResult.GrepLine(1, "test1", false));
			Assert.AreEqual(Utils.MatchCount(result), 3);
			Assert.AreEqual(Utils.MatchCount(null), 0);
			result = new GrepSearchResult("test.txt", new List<GrepSearchResult.GrepLine>());
			Assert.AreEqual(Utils.MatchCount(result), 0);
			result = new GrepSearchResult("test.txt", null);
			Assert.AreEqual(Utils.MatchCount(result), 0);
		}

		[Test]
		public void TestCleanResults()
		{
			List<GrepSearchResult.GrepLine> results =  new List<GrepSearchResult.GrepLine>();
			results.Add(new GrepSearchResult.GrepLine(1, "test", true));
			results.Add(new GrepSearchResult.GrepLine(3, "test3", false));
			results.Add(new GrepSearchResult.GrepLine(2, "test2", false));
			results.Add(new GrepSearchResult.GrepLine(1, "test1", false));
			Utils.CleanResults(ref results);

			Assert.AreEqual(results.Count, 3);
			Assert.AreEqual(results[0].IsContext, false);
			Assert.AreEqual(results[0].LineNumber, 1);
			Assert.AreEqual(results[2].IsContext, false);
			Assert.AreEqual(results[2].LineNumber, 3);

			results = null;
			Utils.CleanResults(ref results);
			results = new List<GrepSearchResult.GrepLine>();
			Utils.CleanResults(ref results);
		}
		

		[RowTest]
		[Row("0.9.1", "0.9.2", true)]
		[Row("0.9.1", "0.9.2.5556", true)]
		[Row("0.9.1.5554", "0.9.1.5556", true)]
		[Row("0.9.0.5557", "0.9.1.5550", true)]
		[Row("0.9.1", "0.9.0.5556", false)]
		[Row("0.9.5.5000", "0.9.0.5556", false)]
		[Row(null, "0.9.0.5556", false)]
		[Row("0.9.5.5000", "", false)]
		[Row("0.9.5.5000", null, false)]
		[Row("xyz", "abc", false)]
		public void CompareVersions(string v1, string v2, bool result)
		{
			Assert.IsTrue(PublishedVersionExtractor.IsUpdateNeeded(v1, v2) == result);
		}

		[Test]
		public void GetLines_Returns_Correct_Line()
		{
			string text = "Hello world" + Environment.NewLine + "My tests are good" + Environment.NewLine + "How about yours?";
			List<int> lineNumbers = new List<int>();
			List<string> lines = Utils.GetLines(text, 3, 2, out lineNumbers);
			Assert.AreEqual(lines.Count, 1);
			Assert.AreEqual(lines[0], "Hello world");
			Assert.AreEqual(lineNumbers.Count, 1);
			Assert.AreEqual(lineNumbers[0], 1);

			lines = Utils.GetLines(text, 14, 2, out lineNumbers);
			Assert.AreEqual(lines.Count, 1);
			Assert.AreEqual(lines[0], "My tests are good");
			Assert.AreEqual(lineNumbers.Count, 1);
			Assert.AreEqual(lineNumbers[0], 2);

			lines = Utils.GetLines(text, 3, 11, out lineNumbers);
			Assert.AreEqual(lines.Count, 2);
			Assert.AreEqual(lines[0], "Hello world");
			Assert.AreEqual(lines[1], "My tests are good");
			Assert.AreEqual(lineNumbers.Count, 2);
			Assert.AreEqual(lineNumbers[0], 1);
			Assert.AreEqual(lineNumbers[1], 2);

			lines = Utils.GetLines(text, 3, 30, out lineNumbers);
			Assert.AreEqual(lines.Count, 3);
			Assert.AreEqual(lines[0], "Hello world");
			Assert.AreEqual(lines[1], "My tests are good");
			Assert.AreEqual(lines[2], "How about yours?");
			Assert.AreEqual(lineNumbers.Count, 3);
			Assert.AreEqual(lineNumbers[0], 1);
			Assert.AreEqual(lineNumbers[1], 2);
			Assert.AreEqual(lineNumbers[2], 3);

			lines = Utils.GetLines("test", 2, 2, out lineNumbers);
			Assert.AreEqual(lines.Count, 1);
			Assert.AreEqual(lines[0], "test");
			Assert.AreEqual(lineNumbers.Count, 1);
			Assert.AreEqual(lineNumbers[0], 1);

			lines = Utils.GetLines("test", 0, 2, out lineNumbers);
			Assert.AreEqual(lines.Count, 1);
			Assert.AreEqual(lines[0], "test");
			Assert.AreEqual(lineNumbers.Count, 1);
			Assert.AreEqual(lineNumbers[0], 1);

			lines = Utils.GetLines("test", 10, 2, out lineNumbers);
			Assert.IsNull(lines);
			Assert.IsNull(lineNumbers);

			lines = Utils.GetLines("test", 2, 10, out lineNumbers);
			Assert.IsNull(lines);
			Assert.IsNull(lineNumbers);
		}

		[RowTest]
		[Row(null,null,2)]
		[Row("", "", 2)]
		[Row(null, ".*\\.cs", 1)]
		[Row(".*\\.txt", null, 1)]
		public void TestCopyFiles(string includePattern, string excludePattern, int numberOfFiles)
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase1", destinationFolder, includePattern, excludePattern);
			Assert.AreEqual(Directory.GetFiles(destinationFolder).Length, numberOfFiles);
		}

		[RowTest]
		[Row(null, null, 2)]
		public void TestCopyFilesToNonExistingFolder(string includePattern, string excludePattern, int numberOfFiles)
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase1", destinationFolder + "\\123", includePattern, excludePattern);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\123").Length, numberOfFiles);
		}

		[Test]
		public void TestCopyFilesWithSubFolders()
		{
			Utils.CopyFiles(sourceFolder + "\\TestCase3", destinationFolder + "\\TestCase3", ".*", null);
			Assert.AreEqual(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 4);
			Assert.IsTrue(Directory.Exists(destinationFolder + "\\TestCase3\\SubFolder"));
			Utils.DeleteFolder(destinationFolder + "\\TestCase3");
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase3\\SubFolder\\test-file-plain-hidden.txt", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase3\\test-file-code.cs", null));
			Utils.CopyFiles(source, sourceFolder + "\\TestCase3", destinationFolder + "\\TestCase3", true);
			Assert.AreEqual(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 2);
			Assert.IsTrue(Directory.Exists(destinationFolder + "\\TestCase3\\SubFolder"));			
		}

		[Test]
		public void TestCopyResults()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1").Length, 2);
			source.Add(new GrepSearchResult(sourceFolder + "\\issue-10.txt", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, true);
			Assert.AreEqual(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 3);
			try
			{
				Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
				Assert.Fail("Not supposed to get here");
			}
			catch (IOException ex)
			{
				//OK
			}
			Assert.AreEqual(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 3);
			Utils.CopyFiles(source, sourceFolder, destinationFolder + "\\123", false);
			Assert.AreEqual(Directory.GetFiles(destinationFolder, "*.*", SearchOption.AllDirectories).Length, 6);
		}

		[Test]
		public void TestCanCopy()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\TestCase1\\test-file-plain2.txt", null));
			Assert.IsFalse(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1"));
			Assert.IsFalse(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1\\"));
			Assert.IsTrue(Utils.CanCopyFiles(source, sourceFolder));
			Assert.IsFalse(Utils.CanCopyFiles(source, sourceFolder + "\\TestCase1\\TestCase1"));
			Assert.IsFalse(Utils.CanCopyFiles(null, null));
			Assert.IsFalse(Utils.CanCopyFiles(source, null));
			Assert.IsFalse(Utils.CanCopyFiles(null, sourceFolder));
		}

		[Test]
		public void WriteToCsvTest()
		{
			File.WriteAllText(destinationFolder + "\\test.csv", "hello");
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			List<GrepSearchResult.GrepLine> lines = new List<GrepSearchResult.GrepLine>();
			lines.Add(new GrepSearchResult.GrepLine(12, "hello", false));
			lines.Add(new GrepSearchResult.GrepLine(13, "world", true));
			List<GrepSearchResult.GrepLine> lines2 = new List<GrepSearchResult.GrepLine>();
			lines2.Add(new GrepSearchResult.GrepLine(11, "and2", true));
			lines2.Add(new GrepSearchResult.GrepLine(12, "hello2", false));
			lines2.Add(new GrepSearchResult.GrepLine(13, "world2", true));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", lines));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", lines2));
			Utils.SaveResultsAsCSV(source, destinationFolder + "\\test.csv");
			string[] stringLines = File.ReadAllLines(destinationFolder + "\\test.csv");
			Assert.AreEqual(stringLines.Length, 3, "CSV file should contain only 3 lines");
			Assert.AreEqual(stringLines[0].Split(',')[0].Trim(), "File Name");
			Assert.AreEqual(stringLines[1].Split(',')[1].Trim(), "12");
			Assert.AreEqual(stringLines[2].Split(',')[2].Trim(), "\"hello2\"");
		}

		[Test]
		public void DeleteFilesTest()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 2);
			List<GrepSearchResult> source2 = new List<GrepSearchResult>();
			source2.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-code.cs", null));
			Utils.DeleteFiles(source2);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 1);
			source2.Add(new GrepSearchResult(destinationFolder + "\\test-file-code.cs", null));
			Utils.DeleteFiles(source2);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 1);
			source2.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-plain.txt", null));
			Utils.DeleteFiles(source2);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1\\").Length, 0);
		}

		[Test]
		public void TestCopyFileInNonExistingFolder()
		{
			Utils.CopyFile(sourceFolder + "\\TestCase1\\test-file-code.cs", destinationFolder + "\\Test\\test-file-code2.cs", false);
			Assert.IsTrue(File.Exists(destinationFolder + "\\Test\\test-file-code2.cs"));
		}

		[Test]
		public void DeleteFolderTest()
		{
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", null));
			Utils.CopyFiles(source, sourceFolder, destinationFolder, false);
			Assert.AreEqual(Directory.GetFiles(destinationFolder + "\\TestCase1").Length, 2);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-code.cs", FileAttributes.ReadOnly);
			Utils.DeleteFolder(destinationFolder);
			Assert.IsFalse(Directory.Exists(destinationFolder));
		}

		[RowTest]
		[Row("*.*", false, true, true, 0, 0, 5)]
		[Row("*.*", false, true, false, 0, 0, 4)]
		[Row("*.*", false, true, false, 0, 40, 3)]
		[Row("*.*", false, true, false, 1, 40, 1)]
		[Row(".*\\.txt", true, true, true, 0, 0, 3)]
		[Row(".*\\.txt", true, false, true, 0, 0, 2)]
		[Row(null, true, false, true, 0, 0, 0)]
		[Row("", true, true, true, 0, 0, 5)]
		public void GetFileListTest(string namePattern, bool isRegex, bool includeSubfolders, bool includeHidden, int sizeFrom, int sizeTo, int result)
		{
			DirectoryInfo di = new DirectoryInfo(sourceFolder + "\\TestCase2\\HiddenFolder");
			di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
			Assert.AreEqual(Utils.GetFileList(sourceFolder + "\\TestCase2", namePattern, isRegex, includeSubfolders, includeHidden, sizeFrom, sizeTo).Length, result);
		}

		[Test]
		public void GetFileListFromNonExistingFolderReturnsEmptyString()
		{
			Assert.AreEqual(Utils.GetFileList(sourceFolder + "\\NonExisting", "*.*", false, true, true, 0, 0).Length, 0);
		}

		[RowTest]
		[Row("", 1, 1)]
		[Row("5", 0, 5)]
		[Row(" 12", 1, 12)]
		[Row("", int.MinValue, int.MinValue)]
		[Row(null, int.MinValue, int.MinValue)]
		[Row(" 22 ", int.MinValue, 22)]
		public void ParseIntTest(string text, int defaultValue, int result)
		{
			if (defaultValue != int.MinValue)
				Assert.AreEqual(Utils.ParseInt(text, defaultValue), result);
			else
				Assert.AreEqual(Utils.ParseInt(text), result);
		}

		[Test]
		public void GetReadOnlyFilesTest()
		{			
			List<GrepSearchResult> source = new List<GrepSearchResult>();
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-code.cs", null));
			source.Add(new GrepSearchResult(sourceFolder + "\\TestCase1\\test-file-plain.txt", null));

			List<GrepSearchResult> destination = new List<GrepSearchResult>();
			destination.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-code.cs", null));
			destination.Add(new GrepSearchResult(destinationFolder + "\\TestCase1\\test-file-plain.txt", null));

			Utils.CopyFiles(source, sourceFolder + "\\TestCase1", destinationFolder + "\\TestCase1", true);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-code.cs", FileAttributes.ReadOnly);
			Assert.AreEqual(Utils.GetReadOnlyFiles(destination).Length, 1);
			File.SetAttributes(destinationFolder + "\\TestCase1\\test-file-plain.txt", FileAttributes.ReadOnly);
			Assert.AreEqual(Utils.GetReadOnlyFiles(destination).Length, 2);

			Assert.AreEqual(Utils.GetReadOnlyFiles(null).Length, 0);
			Assert.AreEqual(Utils.GetReadOnlyFiles(new List<GrepSearchResult>()).Length, 0);
		}
	}
}
#if UNITTESTS
using System;
using System.Collections.Generic;
using System.IO;
using BluePrism.BPCoreLib.InputOutput;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class DirectoryEnumeratorTests
    {

        #region " Utility classes and methods "

        /// <summary>
        /// Simple override which allows directories and files to be stored on the
        /// walker itself
        /// </summary>
        private class AccumulatingDirectoryWalker : DirectoryWalker
        {
            public AccumulatingDirectoryWalker(DirectoryInfo dir) : base(dir)
            {
            }

            public List<string> Files { get; } = new List<string>();

            public List<string> Dirs { get; } = new List<string>();
        }

        /// <summary>
        /// Creates a temp directory with the structure (where '$' represents a randomly
        /// named directory in the system temp directory):
        /// $/a.out
        /// $/temp.txt
        /// $/temp/
        /// $/temp/further/
        /// $/temp/further/thing.txt
        /// $/temp/empty/
        /// </summary>
        /// <remarks>When an instance of this class is disposed of, the directory
        /// structure created in the constructor is deleted.</remarks>
        private class TestDirectory : IDisposable
        {
            public TestDirectory()
            {
                Dir = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                Dir.Create();
                CreateFile(Dir, "a.out");
                CreateFile(Dir, "temp.txt");
                var tempDir_temp = CreateDir(Dir, "temp");
                var tempDir_temp_further = CreateDir(tempDir_temp, "further");
                CreateFile(tempDir_temp_further, "thing.txt");
                CreateDir(tempDir_temp, "empty");
            }

            public DirectoryInfo Dir { get; private set; }

            public string FullName => Dir.FullName;

            private void CreateFile(DirectoryInfo dir, string name)
            {
                using (File.CreateText(Path.Combine(dir.FullName, name)))
                {
                    //just create the file
                }
            }

            private DirectoryInfo CreateDir(DirectoryInfo parent, string name)
            {
                return Directory.CreateDirectory(Path.Combine(parent.FullName, name));
            }

            protected virtual void Dispose(bool disposing)
            {
                // Clean up the test directory
                try
                {
                    Dir?.Delete(true);
                }
                catch
                {
                    // Ignore any errors - we've done our best
                }

                Dir = null;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Handles a file being walked by adding it to the accumulating files list
        /// </summary>
        private void HandleFileWalked(object sender, DirectoryWalkerFileEventArgs e) =>
            ((AccumulatingDirectoryWalker)sender).Files.Add(e.Info.FullName);

        /// <summary>
        /// Handles a directory being walked by adding it to the accumulating dirs list
        /// </summary>
        private void HandleDirWalked(object sender, DirectoryWalkerDirEventArgs e) =>
            ((AccumulatingDirectoryWalker)sender).Dirs.Add(e.Info.FullName);

        /// <summary>
        /// Handles a directory being walked by adding it to the accumulating dirs list,
        /// ensuring that the walker does not descend past the temp directory (actually
        /// past *any* directory named 'temp').
        /// </summary>
        private void HandleDirWalkedSkipTemp(object sender, DirectoryWalkerDirEventArgs e)
        {
            ((AccumulatingDirectoryWalker)sender).Dirs.Add(e.Info.FullName);
            if (e.Info.Name == "temp")
            {
                e.Descend = false;
            }
        }

        #endregion

        #region " Test Methods "

        /// <summary>
        /// Tests a straightforward walk through a directory structure.
        /// </summary>
        [Test]
        public void TestBasicWalker()
        {
            using (var tempDir = new TestDirectory())
            {
                var walker = new AccumulatingDirectoryWalker(tempDir.Dir);
                walker.DirectoryWalked += HandleDirWalked;
                walker.FileWalked += HandleFileWalked;
                walker.Walk();

                Assert.That(walker.Files,
                    Is.EquivalentTo(new[]
                    {
                        Path.Combine(tempDir.FullName, "a.out"), Path.Combine(tempDir.FullName, "temp.txt"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "further", "thing.txt")
                    }));

                Assert.That(walker.Dirs,
                    Is.EquivalentTo(new[]
                    {
                        tempDir.FullName, Path.Combine(tempDir.FullName, "temp"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "further"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "empty")
                    }));
            }
        }

        /// <summary>
        /// Tests a walk through a directory structure which ignores the descendents of
        /// a specific directory ($/temp/ in the TestDirectory)
        /// </summary>
        [Test]
        public void TestNonDescendingWalker()
        {
            using (var tempDir = new TestDirectory())
            {
                var walker = new AccumulatingDirectoryWalker(tempDir.Dir);
                walker.FileWalked += HandleFileWalked;
                walker.DirectoryWalked += HandleDirWalkedSkipTemp;
                walker.Walk();

                Assert.That(walker.Files,
                    Is.EquivalentTo(new[]
                    {
                        Path.Combine(tempDir.FullName, "a.out"), Path.Combine(tempDir.FullName, "temp.txt")
                    }));

                Assert.That(walker.Dirs,
                    Is.EquivalentTo(new[] {tempDir.FullName, Path.Combine(tempDir.FullName, "temp")}));
            }
        }

        [Test]
        public void TestSearchPattern()
        {
            using (var tempDir = new TestDirectory())
            {
                var walker = new AccumulatingDirectoryWalker(tempDir.Dir) {FilePattern = "*.txt"};
                walker.DirectoryWalked += HandleDirWalked;
                walker.FileWalked += HandleFileWalked;
                walker.Walk();

                Assert.That(walker.Files,
                    Is.EquivalentTo(new[]
                    {
                        Path.Combine(tempDir.FullName, "temp.txt"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "further", "thing.txt")
                    }));

                Assert.That(walker.Dirs,
                    Is.EquivalentTo(new[]
                    {
                        tempDir.FullName, Path.Combine(tempDir.FullName, "temp"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "further"),
                        BPUtil.CombinePaths(tempDir.FullName, "temp", "empty")
                    }));
            }
        }

        #endregion
    }
}

#endif

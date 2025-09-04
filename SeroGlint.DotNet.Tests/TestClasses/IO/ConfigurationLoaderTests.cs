using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using NSubstitute;
using SeroGlint.DotNet.Common.Abstractions;
using SeroGlint.DotNet.FileManagement;
using SeroGlint.DotNet.Tests.TestObjects;
using Shouldly;

namespace SeroGlint.DotNet.Tests.TestClasses.IO
{
    public class ConfigurationLoaderTests
    {
        [Fact]
        public void LoadJsonConfiguration_ShouldReturnDefault_WhenFileDoesNotExist_WithFileManager()
        {
            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists("config.json").Returns(false);

            var loader = new ConfigurationLoader(fileManager);
            var result = loader.LoadJsonConfiguration<TestConfig>("config.json");

            result.ShouldNotBeNull();
            result.AppName.ShouldBe("DefaultApp");
        }

        [Fact]
        public void LoadJsonConfiguration_ShouldReturnDefault_WhenFileDoesNotExist_WithoutFileManager()
        {
            var loader = new ConfigurationLoader();
            var result = loader.LoadJsonConfiguration<TestConfig>("nonexistent.json");

            result.ShouldNotBeNull();
            result.AppName.ShouldBe("DefaultApp");
        }

        [Fact]
        public void LoadJsonConfiguration_ShouldDeserialize_WhenFileExists_WithFileManager()
        {
            var config = new TestConfig { AppName = "CustomApp", RetryCount = 5 };
            var json = JsonSerializer.Serialize(config);

            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists("config.json").Returns(true);
            fileManager.ReadAllText("config.json").Returns(json);

            var loader = new ConfigurationLoader(fileManager);
            var result = loader.LoadJsonConfiguration<TestConfig>("config.json");

            result.AppName.ShouldBe("CustomApp");
            result.RetryCount.ShouldBe(5);
        }

        [Fact]
        public void LoadJsonConfiguration_ShouldDeserialize_WhenFileExists_WithoutFileManager()
        {
            var path = Path.GetTempFileName();
            var config = new TestConfig { AppName = "Custom", RetryCount = 7 };
            File.WriteAllText(path, JsonSerializer.Serialize(config));

            var loader = new ConfigurationLoader();
            var result = loader.LoadJsonConfiguration<TestConfig>(path);

            result.AppName.ShouldBe("Custom");
            result.RetryCount.ShouldBe(7);

            File.Delete(path);
        }

        [Fact]
        public void SaveJsonConfiguration_ShouldUseFileManager()
        {
            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists(Arg.Any<string>()).Returns(true);
            fileManager.Create(Arg.Any<string>()).Returns(new MemoryStream());
            fileManager.GetFullPath("settings.json").Returns("settings.json");
            fileManager.GetDirectoryName("settings.json").Returns(".");

            var loader = new ConfigurationLoader(fileManager);
            var config = new TestConfig { AppName = "X", RetryCount = 1 };

            loader.SaveJsonConfiguration("settings.json", config);

            fileManager.Received().WriteAllText(
                "settings.json",
                Arg.Is<string>(json => json.Contains("X") && json.Contains("RetryCount"))
            );
        }

        [Fact]
        public void SaveJsonConfiguration_ShouldUseSystemIO_WhenFileManagerIsNull()
        {
            var tempFile = Path.GetTempFileName();
            var loader = new ConfigurationLoader();
            var config = new TestConfig { AppName = "DiskApp", RetryCount = 11 };

            loader.SaveJsonConfiguration(tempFile, config);

            var written = File.ReadAllText(tempFile);
            written.ShouldContain("DiskApp");
            File.Delete(tempFile);
        }

        [Fact]
        public void SaveJsonConfiguration_ShouldCreateDirectory_WhenMissing()
        {
            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists("config.json").Returns(true);
            fileManager.GetFullPath("config.json").Returns("config.json");
            fileManager.GetDirectoryName("config.json").Returns("folder");
            fileManager.Exists("folder").Returns(false);
            fileManager.Create("config.json").Returns(new MemoryStream());
            fileManager.CreateDirectory("folder").Returns(new DirectoryInfo("folder"));

            var loader = new ConfigurationLoader(fileManager);
            loader.SaveJsonConfiguration("config.json", new TestConfig());

            fileManager.Received().CreateDirectory("folder");
        }

        [Fact]
        public void SaveJsonConfiguration_ShouldCreateFile_WhenMissing()
        {
            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists("file.json").Returns(false);
            fileManager.GetFullPath("file.json").Returns("file.json");
            fileManager.GetDirectoryName("file.json").Returns(".");
            fileManager.Create("file.json").Returns(new MemoryStream());
            fileManager.CreateDirectory(Arg.Any<string>()).Returns(new DirectoryInfo(".\\"));

            var loader = new ConfigurationLoader(fileManager);
            loader.SaveJsonConfiguration("file.json", new TestConfig());

            fileManager.Received().WriteAllText(
                "file.json",
                Arg.Is<string>(json => json.Contains("DefaultApp"))
            );
        }

        [Fact]
        [ExcludeFromCodeCoverage]
        public void SaveJsonConfiguration_ShouldReThrow_IfInnerExceptionOccurs()
        {
            var fileManager = Substitute.For<IDirectoryManagement>();
            fileManager.Exists("bad.json").Returns(true);
            fileManager.GetFullPath("bad.json").Returns("bad.json");
            fileManager.GetDirectoryName("bad.json").Returns(".");
            fileManager.Create("bad.json").Returns(_ => throw new IOException("simulated failure"));

            var loader = new ConfigurationLoader(fileManager);

            var ex = Should.Throw<IOException>(() =>
                loader.SaveJsonConfiguration("bad.json", new TestConfig()));

            ex.Message.ShouldContain("Configuration file could not be created");
        }
    }
}
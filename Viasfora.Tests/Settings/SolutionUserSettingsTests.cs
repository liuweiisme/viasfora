﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Winterdom.Viasfora.Settings;
using Newtonsoft.Json;
using Xunit;
using Newtonsoft.Json.Linq;

namespace Viasfora.Tests.Settings {
  public class SolutionUserSettingsTests {

    [Fact]
    public void CanAddOneEntry() {
      String file = Path.Combine(GetFolder(), SolutionUserSettings.FILENAME);
      if ( File.Exists(file) ) File.Delete(file);

      var setting = GetSampleSetting("Sample", "Test1", "Test1Value");
      var sus = new SolutionUserSettings(GetFolder());
      sus.Store("c:\\test1", setting);

      Assert.True(File.Exists(file));
      String json = File.ReadAllText(file);
      Console.WriteLine(json);
      var jo = JObject.Parse(json);
      Assert.Equal("Test1", (String)jo["c:\\test1"]["Sample"]["Name"]);
      Assert.Equal("Test1Value", (String)jo["c:\\test1"]["Sample"]["Value"]);
    }
    [Fact]
    public void CanStoreTwoEntriesOnSingleFile() {
      String file = Path.Combine(GetFolder(), SolutionUserSettings.FILENAME);
      if ( File.Exists(file) ) File.Delete(file);

      var setting1 = GetSampleSetting("First", "Test1", "Test1Value");
      var setting2 = GetSampleSetting("Second", "Test2", "Test2Value");
      var sus = new SolutionUserSettings(GetFolder());
      sus.Store("c:\\test1", setting1);
      sus.Store("c:\\test1", setting2);

      Assert.True(File.Exists(file));
      String json = File.ReadAllText(file);
      Console.WriteLine(json);
      var jo = JObject.Parse(json);
      Assert.Equal("Test1", (String)jo["c:\\test1"]["First"]["Name"]);
      Assert.Equal("Test2Value", (String)jo["c:\\test1"]["Second"]["Value"]);
    }
    [Fact]
    public void CanStoreEntriesOnSeparateFiles() {
      String file = Path.Combine(GetFolder(), SolutionUserSettings.FILENAME);
      if ( File.Exists(file) ) File.Delete(file);

      var setting1 = GetSampleSetting("First", "Test1", "Test1Value");
      var setting2 = GetSampleSetting("Second", "Test2", "Test2Value");
      var sus = new SolutionUserSettings(GetFolder());
      sus.Store("c:\\test1", setting1);
      sus.Store("c:\\test2", setting2);

      Assert.True(File.Exists(file));
      String json = File.ReadAllText(file);
      Console.WriteLine(json);
      var jo = JObject.Parse(json);
      Assert.Equal("Test1", (String)jo["c:\\test1"]["First"]["Name"]);
      Assert.Equal("Test2Value", (String)jo["c:\\test2"]["Second"]["Value"]);
    }
    [Fact]
    public void CanStoreEntryOnBlankFile() {
      String file = Path.Combine(GetFolder(), SolutionUserSettings.FILENAME);
      if ( File.Exists(file) ) File.Delete(file);

      var setting1 = GetSampleSetting("First", "Test1", "Test1Value");
      var sus = new SolutionUserSettings(GetFolder());
      sus.Store("", setting1);

      Assert.True(File.Exists(file));
      String json = File.ReadAllText(file);
      Console.WriteLine(json);
      var jo = JObject.Parse(json);
      Assert.Equal("Test1", (String)jo["__"]["First"]["Name"]);
    }
    [Fact]
    public void CanReadEntries() {
      String file = Path.Combine(GetFolder(), SolutionUserSettings.FILENAME);
      var testData = @"{
        'c:\\test1': {
          'SampleSetting': {
            'Name': 'Test1',
            'Value': 'Test1Value'
          },
        }
      }";
      File.WriteAllText(file, testData);

      var sus = new SolutionUserSettings(GetFolder());
      SampleSetting setting = sus.Load<SampleSetting>("c:\\test1");
      Assert.NotNull(setting);
      Assert.Equal("Test1", setting.Entry.Name);
    }

    [Fact]
    public void MakeRelativePath_NestedPath() {
      String slnPath = @"C:\users\myuser\documents\Visual Studio 10.0\Project\MySolution";
      ISolutionUserSettings sus = new SolutionUserSettings(slnPath);

      String filePath = Path.Combine(slnPath, @"MyProject\Files\File1.txt");
      String relative = sus.MakeRelativePath(filePath);
      Assert.Equal(@"MyProject\Files\File1.txt", relative);
    }
    [Fact]
    public void MakeRelativePath_ParentPath() {
      String slnPath = @"C:\users\myuser\documents\Visual Studio 10.0\Project\MySolution";
      ISolutionUserSettings sus = new SolutionUserSettings(slnPath);

      String filePath = Path.Combine(slnPath, @"..\..\MyProject\Files\File1.txt");
      String relative = sus.MakeRelativePath(Path.GetFullPath(filePath));
      Assert.Equal(@"..\..\MyProject\Files\File1.txt", relative);
    }

    private SampleSetting GetSampleSetting(String key, String name, String value) {
      SampleSetting setting = new SampleSetting();
      setting.Name = key;
      setting.Entry = new Entry {
        Name = name,
        Value = value
      };
      return setting;
    }

    private String GetFolder() {
      return Path.GetTempPath();
    }


    class Entry {
      public String Name { get; set; }
      public String Value { get; set; }
    }
    class SampleSetting : ISettingsObject {
      public Entry Entry { get; set; }
      public String Name { get; set; }

      public SampleSetting() {
        Name = "SampleSetting";
      }
      public void Read(JsonTextReader reader) {
        this.Entry = new Entry();
        reader.Read(); // startObject
        reader.Read(); // property name
        this.Entry.Name = reader.ReadAsString();
        reader.Read(); // property name
        this.Entry.Value = reader.ReadAsString();
      }

      public void Save(JsonTextWriter writer) {
        writer.WriteStartObject();
        writer.WritePropertyName("Name");
        writer.WriteValue(Entry.Name);
        writer.WritePropertyName("Value");
        writer.WriteValue(Entry.Value);
        writer.WriteEndObject();
      }
    }
  }
}

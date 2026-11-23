using NUnit.Framework;
using UnityEngine;

public sealed class DiscoveryLogTests
{
    [SetUp]
    public void SetUp()
    {
        PlayerPrefs.DeleteKey("GalaxyExplorer.Discoveries.v1");
    }

    [TearDown]
    public void TearDown()
    {
        PlayerPrefs.DeleteKey("GalaxyExplorer.Discoveries.v1");
    }

    [Test]
    public void Record_AddsWorldToJournal()
    {
        DiscoveryLog.Record("Test World", "Test description");
        CollectionAssert.Contains(DiscoveryLog.GetDiscoveredNames(), "Test World");
    }

    [Test]
    public void Record_DoesNotAddDuplicateWorlds()
    {
        DiscoveryLog.Record("Test World", "First record");
        DiscoveryLog.Record("Test World", "Second record");
        Assert.AreEqual(1, DiscoveryLog.GetDiscoveredNames().Count);
    }
}

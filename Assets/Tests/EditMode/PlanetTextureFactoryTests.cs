using NUnit.Framework;
using UnityEngine;

public sealed class PlanetTextureFactoryTests
{
    [Test]
    public void Create_ReturnsExpectedTextureDimensions()
    {
        Texture2D texture = PlanetTextureFactory.Create(Color.blue, 11, false);
        Assert.AreEqual(512, texture.width);
        Assert.AreEqual(256, texture.height);
        Object.DestroyImmediate(texture);
    }

    [Test]
    public void Create_UsesRepeatWrappingForSphericalMapping()
    {
        Texture2D texture = PlanetTextureFactory.Create(Color.red, 17, true);
        Assert.AreEqual(TextureWrapMode.Repeat, texture.wrapMode);
        Object.DestroyImmediate(texture);
    }
}

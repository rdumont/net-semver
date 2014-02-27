using NUnit.Framework;

namespace SemanticVersioning.Tests
{
    [TestFixture]
    class SemVerTests
    {
        [Test]
        public void Parse_strict_full_version()
        {
            // Act
            var version = new SemVer("1.2.3-beta.4+release.55");

            // Assert
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
            Assert.That(version.Prerelease, Is.EqualTo(new object[] {"beta", 4}));
            Assert.That(version.Build, Is.EqualTo(new[] {"release", "55"}));
        }

        [Test]
        public void Parse_loose_version()
        {
            // Act
            var version = new SemVer("=1.2.3beta.4", true);

            // Assert
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
            Assert.That(version.Prerelease, Is.EqualTo(new object[] { "beta", 4 }));
        }

        [Test]
        public void Should_accept_leading_v_character()
        {
            // Act
            var version = new SemVer("v1.2.3-beta.4+release.55");

            // Assert
            Assert.That(version.Major, Is.EqualTo(1));
            Assert.That(version.Minor, Is.EqualTo(2));
            Assert.That(version.Patch, Is.EqualTo(3));
            Assert.That(version.Prerelease, Is.EqualTo(new object[] { "beta", 4 }));
            Assert.That(version.Build, Is.EqualTo(new[] { "release", "55" }));
        }

        [Test]
        public void Should_throw_error_for_invalid_version()
        {
            // Act & Assert
            Assert.That(() => new SemVer("a.b.c"),
                Throws.Exception);
        }
    }
}

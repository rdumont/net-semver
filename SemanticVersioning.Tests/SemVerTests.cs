using System;
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

        class CompareIdentifiers
        {
            [TestCase("x", "x")]
            [TestCase("abc", "abc")]
            [TestCase("", "")]
            [TestCase(123, 123)]
            public void Equal(object a, object b)
            {
                // Act & Assert
                Assert.That(TestableSemVer.CompareIdentifiers(a, b), Is.EqualTo(0));
            }

            [TestCase(2, 1)]
            [TestCase("abc", 123)]
            [TestCase("b", "a")]
            public void Positive_result(object a, object b)
            {
                // Act & Assert
                Assert.That(TestableSemVer.CompareIdentifiers(a, b), Is.EqualTo(1));
            }

            [TestCase(1, 2)]
            [TestCase(123, "abc")]
            [TestCase("a", "b")]
            public void Negative_result(object a, object b)
            {
                // Act & Assert
                Assert.That(TestableSemVer.CompareIdentifiers(a, b), Is.EqualTo(-1));
            }
        }

        class ComparePre
        {
            [Test]
            public void Equal_prereleases()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {"beta", 3}};
                var other = new TestableSemVer {Prerelease = new object[] {"beta", 3}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(0));
            }

            [Test]
            public void Greater_prerelease_string()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {"beta"}};
                var other = new TestableSemVer {Prerelease = new object[] {"alpha"}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Extra_prerelease_number()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {"beta", 1}};
                var other = new TestableSemVer {Prerelease = new object[] {"beta"}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Greater_prerelease_number()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {"beta", 5}};
                var other = new TestableSemVer {Prerelease = new object[] {"beta", 4}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Smaller_prerelease_string()
            {
                // Arrange
                var version = new TestableSemVer { Prerelease = new object[] { "beta" } };
                var other = new TestableSemVer { Prerelease = new object[] { "delta" } };

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Less_prerelease_number()
            {
                // Arrange
                var version = new TestableSemVer { Prerelease = new object[] { "beta" } };
                var other = new TestableSemVer { Prerelease = new object[] { "beta", 1 } };

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Smaller_prerelease_number()
            {
                // Arrange
                var version = new TestableSemVer { Prerelease = new object[] { "beta", 4 } };
                var other = new TestableSemVer { Prerelease = new object[] { "beta", 5 } };

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Release_should_be_greater_than_prerelease()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {}};
                var other = new TestableSemVer {Prerelease = new object[] {"beta"}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Prerelease_should_be_smaller_than_release()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {"beta"}};
                var other = new TestableSemVer {Prerelease = new object[] {}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Should_equal_without_prerelease()
            {
                // Arrange
                var version = new TestableSemVer {Prerelease = new object[] {}};
                var other = new TestableSemVer {Prerelease = new object[] {}};

                // Act
                var result = version.ComparePre(other);

                // Assert
                Assert.That(result, Is.EqualTo(0));
            }
        }

        class CompareMain
        {
            [Test]
            public void Greater_due_to_major()
            {
                // Arrange
                var version = new TestableSemVer {Major = 2};
                var other = new TestableSemVer {Major = 1};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Smaller_due_to_major()
            {
                // Arrange
                var version = new TestableSemVer {Major = 2};
                var other = new TestableSemVer {Major = 3};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Greater_due_to_minor()
            {
                // Arrange
                var version = new TestableSemVer { Major = 1, Minor = 2};
                var other = new TestableSemVer { Major = 1, Minor = 1};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Smaller_due_to_minor()
            {
                // Arrange
                var version = new TestableSemVer { Major = 1, Minor = 2};
                var other = new TestableSemVer { Major = 1, Minor = 3};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Greater_due_to_patch()
            {
                // Arrange
                var version = new TestableSemVer { Major = 1, Minor = 2, Patch = 3};
                var other = new TestableSemVer { Major = 1, Minor = 2, Patch = 2};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(1));
            }

            [Test]
            public void Smaller_due_to_patch()
            {
                // Arrange
                var version = new TestableSemVer { Major = 1, Minor = 2, Patch = 3};
                var other = new TestableSemVer { Major = 1, Minor = 2, Patch = 4};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(-1));
            }

            [Test]
            public void Equal_main_versions()
            {
                // Arrange
                var version = new TestableSemVer { Major = 1, Minor = 2, Patch = 3};
                var other = new TestableSemVer { Major = 1, Minor = 2, Patch = 3};

                // Act
                var result = version.CompareMain(other);

                // Assert
                Assert.That(result, Is.EqualTo(0));
            }
        }

        class Operators
        {
            [TestCase("1.2.3", "1.2.3", true)]
            [TestCase("1.2.3", "1.2.4", false)]
            [TestCase("1.2.3", "1.2.3-beta", false)]
            public void Equals(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version == other, Is.EqualTo(shouldEqual));
            }

            [TestCase("1.2.3", "1.2.3", false)]
            [TestCase("1.2.3", "1.2.4", true)]
            [TestCase("1.2.3", "1.2.3-beta", true)]
            public void Not_equals(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version != other, Is.EqualTo(shouldEqual));
            }

            [TestCase("1.2.3-beta", "1.2.3-beta.0", true)]
            [TestCase("1.2.3", "1.2.4", true)]
            [TestCase("1.2.3", "1.3.0", true)]
            [TestCase("1.2.3", "2.0.0", true)]
            [TestCase("1.2.3-beta", "1.2.3", true)]
            [TestCase("1.2.3-beta", "1.2.3-alpha", false)]
            [TestCase("1.2.3", "1.2.3", false)]
            public void Less_than(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version < other, Is.EqualTo(shouldEqual));
            }

            [TestCase("1.2.3-beta.0", "1.2.3-beta", true)]
            [TestCase("1.2.4", "1.2.3", true)]
            [TestCase("1.3.0", "1.2.3", true)]
            [TestCase("2.0.0", "1.2.3", true)]
            [TestCase("1.2.3", "1.2.3-beta", true)]
            [TestCase("1.2.3-alpha", "1.2.3-beta", false)]
            [TestCase("1.2.3", "1.2.3", false)]
            public void Greater_than(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version > other, Is.EqualTo(shouldEqual));
            }

            [TestCase("1.2.3-beta", "1.2.3-beta.0", true)]
            [TestCase("1.2.3", "1.2.4", true)]
            [TestCase("1.2.3", "1.3.0", true)]
            [TestCase("1.2.3", "2.0.0", true)]
            [TestCase("1.2.3-beta", "1.2.3", true)]
            [TestCase("1.2.3", "1.2.3", true)]
            [TestCase("1.2.3-beta", "1.2.3-alpha", false)]
            public void Less_than_or_equal(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version <= other, Is.EqualTo(shouldEqual));
            }

            [TestCase("1.2.3-beta.0", "1.2.3-beta", true)]
            [TestCase("1.2.4", "1.2.3", true)]
            [TestCase("1.3.0", "1.2.3", true)]
            [TestCase("2.0.0", "1.2.3", true)]
            [TestCase("1.2.3", "1.2.3-beta", true)]
            [TestCase("1.2.3", "1.2.3", true)]
            [TestCase("1.2.3-alpha", "1.2.3-beta", false)]
            public void Greater_than_or_equal(string v1, string v2, bool shouldEqual)
            {
                // Arrange
                var version = new SemVer(v1);
                var other = new SemVer(v2);

                // Act & Assert
                Assert.That(version >= other, Is.EqualTo(shouldEqual));
            }
        }
    }

    public class TestableSemVer : SemVer
    {
        public TestableSemVer()
        {
        }

        public TestableSemVer(string version, bool loose = false) : base(version, loose)
        {
        }

        public new int ComparePre(SemVer other)
        {
            return base.ComparePre(other);
        }

        public new int CompareMain(SemVer other)
        {
            return base.CompareMain(other);
        }

        public static new int CompareIdentifiers(object a, object b)
        {
            return SemVer.CompareIdentifiers(a, b);
        }
    }
}

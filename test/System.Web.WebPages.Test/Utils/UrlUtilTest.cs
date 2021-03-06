﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Web.WebPages.TestUtils;
using Microsoft.TestCommon;
using Moq;

namespace System.Web.WebPages.Test
{
    public class UrlUtilTest
    {
        [Fact]
        public void GenerateClientUrl_ResolvesVirtualPath_WithApplicationAtRoot()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/"),
                                   __ = Utils.CreateHttpRuntime("/"))
                {
                    // Arrange
                    var vpath = "~/";
                    var href = "~/world/test.aspx";
                    var expected = "/world/test.aspx";
                    var context = new HttpContextWrapper(HttpContext.Current);
                    var page = new MockPage { VirtualPath = vpath, Context = context };

                    // Act
                    var actual1 = UrlUtil.GenerateClientUrl(context, vpath, href);
                    var actual2 = page.Href(href);

                    // Assert
                    Assert.Equal(expected, actual1);
                    Assert.Equal(expected, actual2);
                }
            });
        }

        [Fact]
        public void GenerateClientUrl_ResolvesVirtualPathWithSubfolder_WithApplicationPath()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    // Arrange
                    var vpath = "~/subfolder1/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "/WebSite1/world/test.aspx";
                    var context = new HttpContextWrapper(HttpContext.Current);
                    var page = new MockPage() { VirtualPath = vpath, Context = context };

                    // Act
                    var actual1 = UrlUtil.GenerateClientUrl(context, vpath, href);
                    var actual2 = page.Href(href);

                    // Assert
                    Assert.Equal(expected, actual1);
                    Assert.Equal(expected, actual2);
                }
            });
        }

        [Fact]
        public void GenerateClientUrl_ResolvesVirtualPath_WithApplicationPath()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    // Arrange
                    var vpath = "~/default.aspx";
                    var href = "~/world/test.aspx";
                    var expected = "/WebSite1/world/test.aspx";
                    var context = new HttpContextWrapper(HttpContext.Current);
                    var page = new MockPage() { VirtualPath = vpath, Context = context };

                    // Act
                    var actual1 = UrlUtil.GenerateClientUrl(context, vpath, href);
                    var actual2 = page.Href(href);

                    // Assert
                    Assert.Equal(expected, actual1);
                    Assert.Equal(expected, actual2);
                }
            });
        }

        [Fact]
        public void GenerateClientUrl_ResolvesRelativePathToSubfolder_WithApplicationPath()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/WebSite1/subfolder1/default.aspx"),
                                   __ = Utils.CreateHttpRuntime("/WebSite1/"))
                {
                    // Arrange
                    var vpath = "~/subfolder1/default.aspx";
                    var href = "world/test.aspx";
                    var expected = "/WebSite1/subfolder1/world/test.aspx";
                    var context = new HttpContextWrapper(HttpContext.Current);
                    var page = new MockPage() { VirtualPath = vpath, Context = context };

                    // Act
                    var actual1 = UrlUtil.GenerateClientUrl(context, vpath, href);
                    var actual2 = page.Href(href);

                    // Assert
                    Assert.Equal(expected, actual1);
                    Assert.Equal(expected, actual2);
                }
            });
        }

        [Fact]
        public void GenerateClientUrl_ResolvesVirtualPath_WithUrlRewrite()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                // Arrange
                var vpath = "~/subfolder1/default.aspx";
                var href = "world/test.aspx";
                var expected = "/subfolder1/world/test.aspx";
                var contextMock = GetMockHttpContext(true);
                contextMock.Setup(c => c.Request.RawUrl).Returns("/subfolder1/default.aspx");
                contextMock.Setup(c => c.Request.Path).Returns("/myapp/subfolder1/default.aspx");
                    
                // Act
                var actual1 = UrlUtil.GenerateClientUrl(contextMock.Object, vpath, href);

                // Assert
                Assert.Equal(expected, actual1);
            });
        }

        [Fact]
        public void BuildUrlEncodesPagePart()
        {
            // Arrange
            var page = "This is a really bad name for a page";
            var expected = "This%20is%20a%20really%20bad%20name%20for%20a%20page";

            // Act
            var actual = UrlUtil.BuildUrl(page);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, expected);
        }

        [Fact]
        public void BuildUrlAppendsNonAnonymousTypesToPathPortion()
        {
            // Arrange
            object[] pathParts = new object[] { "part", Decimal.One, 1.25f };
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, pathParts);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "/part/1/1.25");
        }

        [Fact]
        public void BuildUrlEncodesAppendedPathPortion()
        {
            // Arrange
            object[] pathParts = new object[] { "path portion", "ζ" };
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, pathParts);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "/path%20portion/%ce%b6");
        }

        [Fact]
        public void BuildUrlAppendsAnonymousObjectsToQueryString()
        {
            // Arrange
            var page = "home";
            var queryString = new { sort = "FName", dir = "desc" };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "?sort=FName&dir=desc");
        }

        [Fact]
        public void BuildUrlAppendsMultipleAnonymousObjectsToQueryString()
        {
            // Arrange
            var page = "home";
            var queryString1 = new { sort = "FName", dir = "desc" };
            var queryString2 = new { view = "Activities", page = 7 };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString1, queryString2);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "?sort=FName&dir=desc&view=Activities&page=7");
        }

        [Fact]
        public void BuildUrlEncodesQueryStringKeysAndValues()
        {
            // Arrange
            var page = "home";
            var queryString = new { ζ = "my=value&", mykey = "<π" };

            // Act
            var actual = UrlUtil.BuildUrl(page, queryString);

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "?%ce%b6=my%3dvalue%26&mykey=%3c%cf%80");
        }

        [Fact]
        public void BuildUrlGeneratesPathPartsAndQueryString()
        {
            // Arrange
            var page = "home";

            // Act
            var actual = UrlUtil.BuildUrl(page, "products", new { cat = 37 }, "furniture", new { sort = "name", dir = "desc" });

            // Assert
            Assert.Equal(actual.Item1 + actual.Item2, page + "/products/furniture?cat=37&sort=name&dir=desc");
        }

        [Fact]
        public void UrlAnonymousObjectTest()
        {
            AppDomainUtils.RunInSeparateAppDomain(() =>
            {
                using (IDisposable _ = Utils.CreateHttpContext("default.aspx", "http://localhost/"),
                                   __ = Utils.CreateHttpRuntime("/"))
                {
                    Assert.Equal("/world/test.cshtml?Prop1=value1",
                                 UrlUtil.GenerateClientUrl(new HttpContextWrapper(HttpContext.Current), "~/world/page.cshtml", "test.cshtml", new { Prop1 = "value1" }));
                    Assert.Equal("/world/test.cshtml?Prop1=value1&Prop2=value2",
                                 UrlUtil.GenerateClientUrl(new HttpContextWrapper(HttpContext.Current), "~/world/page.cshtml", "test.cshtml", new { Prop1 = "value1", Prop2 = "value2" }));
                }
            });
        }

        [Fact]
        public void GenerateClientUrlWithAbsoluteContentPathAndRewritingDisabled()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: false);

            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(mockHttpContext.Object, "should remain unchanged");

            // Assert
            Assert.Equal("should remain unchanged", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAbsoluteContentPathAndRewritingEnabled()
        {
            UrlUtil.ResetUrlRewriterHelper(); // Reset the "is URL rewriting enabled?" cache

            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: true);
            mockHttpContext.Setup(c => c.Request.RawUrl).Returns("/quux/foo/bar/baz");
            mockHttpContext.Setup(c => c.Request.Path).Returns("/myapp/foo/bar/baz");

            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(mockHttpContext.Object, "/myapp/some/absolute/path?alpha=bravo");

            // Assert
            Assert.Equal("/quux/some/absolute/path?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAppRelativeContentPathAndRewritingDisabled()
        {
            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: false);

            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(mockHttpContext.Object, "~/foo/bar?alpha=bravo");

            // Assert
            Assert.Equal("/myapp/(S(session))/foo/bar?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithAppRelativeContentPathAndRewritingEnabled()
        {
            UrlUtil.ResetUrlRewriterHelper(); // Reset the "is URL rewriting enabled?" cache

            // Arrange
            Mock<HttpContextBase> mockHttpContext = GetMockHttpContext(includeRewriterServerVar: true);
            mockHttpContext.Setup(c => c.Request.RawUrl).Returns("/quux/foo/baz");
            mockHttpContext.Setup(c => c.Request.Path).Returns("/myapp/foo/baz");

            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(mockHttpContext.Object, "~/foo/bar?alpha=bravo");

            // Assert
            Assert.Equal("/quux/foo/bar?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithEmptyContentPathReturnsEmptyString()
        {
            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(null, "");

            // Assert
            Assert.Equal("", returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithNullContentPathReturnsNull()
        {
            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(null, null);

            // Assert
            Assert.Null(returnedUrl);
        }

        [Fact]
        public void GenerateClientUrlWithOnlyQueryStringForContentPathReturnsOriginalContentPath()
        {
            // Act
            string returnedUrl = UrlUtil.GenerateClientUrl(null, "?foo=bar");

            // Assert
            Assert.Equal("?foo=bar", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromDirectoryToParent()
        {
            // Act
            string returnedUrl = UrlUtil.MakeAbsolute("/Account/Register", "../Account");

            // Assert
            Assert.Equal("/Account", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromDirectoryToSelf()
        {
            // Act
            string returnedUrl = UrlUtil.MakeAbsolute("/foo/", "./");

            // Assert
            Assert.Equal("/foo/", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromFileToFile()
        {
            // Act
            string returnedUrl = UrlUtil.MakeAbsolute("/foo", "bar");

            // Assert
            Assert.Equal("/bar", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromFileWithQueryToFile()
        {
            // Act
            string returnedUrl = UrlUtil.MakeAbsolute("/foo/bar?alpha=bravo", "baz");

            // Assert
            Assert.Equal("/foo/baz", returnedUrl);
        }

        [Fact]
        public void MakeAbsoluteFromRootToSelf()
        {
            // Act
            string returnedUrl = UrlUtil.MakeAbsolute("/", "./");

            // Assert
            Assert.Equal("/", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToDirectory()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/foo/bar", "/foo/");

            // Assert
            Assert.Equal("./", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToDirectoryWithQueryString()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/foo/bar", "/foo/?alpha=bravo");

            // Assert
            Assert.Equal("./?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToFile()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/foo/bar", "/baz/quux");

            // Assert
            Assert.Equal("../baz/quux", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileToFileWithQuery()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/foo/bar", "/baz/quux?alpha=bravo");

            // Assert
            Assert.Equal("../baz/quux?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromFileWithQueryToFileWithQuery()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/foo/bar?charlie=delta", "/baz/quux?alpha=bravo");

            // Assert
            Assert.Equal("../baz/quux?alpha=bravo", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromRootToRoot()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/", "/");

            // Assert
            Assert.Equal("./", returnedUrl);
        }

        [Fact]
        public void MakeRelativeFromRootToRootWithQueryString()
        {
            // Act
            string returnedUrl = UrlUtil.MakeRelative("/", "/?foo=bar");

            // Assert
            Assert.Equal("./?foo=bar", returnedUrl);
        }

        internal static Mock<HttpContextBase> GetMockHttpContext(bool includeRewriterServerVar)
        {
            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();

            NameValueCollection serverVars = new NameValueCollection();
            serverVars[UrlRewriterHelper.UrlRewriterEnabledServerVar] = "I'm on!";
            mockContext.Setup(c => c.Request.ServerVariables).Returns(serverVars);
            mockContext.Setup(c => c.Request.ApplicationPath).Returns("/myapp");

            if (includeRewriterServerVar)
            {
                serverVars[UrlRewriterHelper.UrlWasRewrittenServerVar] = "Got rewritten!";
                mockContext
                    .Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                    .Returns(
                        delegate(string input) { return input; });
            }
            else
            {
                mockContext
                    .Setup(c => c.Response.ApplyAppPathModifier(It.IsAny<string>()))
                    .Returns(
                        delegate(string input) { return "/myapp/(S(session))" + input.Substring("/myapp".Length); });
            }

            return mockContext;
        }
    }
}

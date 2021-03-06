﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NSubstitute;
using Octokit.Internal;
using Xunit;

namespace Octokit.Tests.Clients
{
    public class WatchedClientTests
    {
        public class TheCtor
        {
            [Fact]
            public void EnsuresNonNullArguments()
            {
                Assert.Throws<ArgumentNullException>(
                    () => new WatchedClient(null));
            }
        }

        public class TheGetAllForCurrentMethod
        {
            [Fact]
            public async Task RequestsCorrectUrl()
            {
                var endpoint = new Uri("user/subscriptions", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                await client.GetAllForCurrent();

                connection.Received().GetAll<Repository>(endpoint, Args.ApiOptions);
            }

            [Fact]
            public async Task RequestsCorrectUrlWithApiOptions()
            {
                var endpoint = new Uri("user/subscriptions", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                var options = new ApiOptions
                {
                    StartPage = 1,
                    PageCount = 1,
                    PageSize = 1
                };

                await client.GetAllForCurrent(options);

                connection.Received().GetAll<Repository>(endpoint, options);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new WatchedClient(Substitute.For<IApiConnection>());

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllForCurrent(null));
            }
        }

        public class TheGetAllForUserMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var endpoint = new Uri("users/banana/subscriptions", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                client.GetAllForUser("banana");

                connection.Received().GetAll<Repository>(endpoint, Args.ApiOptions);
            }

            [Fact]
            public void RequestsCorrectUrlWithApiOptions()
            {
                var endpoint = new Uri("users/banana/subscriptions", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                var options = new ApiOptions
                {
                    StartPage = 1,
                    PageCount = 1,
                    PageSize = 1
                };

                client.GetAllForUser("banana", options);

                connection.Received().GetAll<Repository>(endpoint, options);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new WatchedClient(Substitute.For<IApiConnection>());

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllForUser(null));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllForUser(null, ApiOptions.None));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllForUser("user", null));

                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllForUser(""));
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllForUser("", ApiOptions.None));
            }
        }

        public class TheGetAllWatchersForRepoMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var endpoint = new Uri("repos/fight/club/subscribers", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                client.GetAllWatchers("fight", "club");

                connection.Received().GetAll<User>(endpoint, Args.ApiOptions);
            }

            [Fact]
            public void RequestsCorrectUrlWithApiOptions()
            {
                var endpoint = new Uri("repos/fight/club/subscribers", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                var options = new ApiOptions
                {
                    StartPage = 1,
                    PageCount = 1,
                    PageSize = 1
                };

                client.GetAllWatchers("fight", "club", options);

                connection.Received().GetAll<User>(endpoint, options);
            }

            [Fact]
            public async Task EnsuresNonNullArguments()
            {
                var client = new WatchedClient(Substitute.For<IApiConnection>());

                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllWatchers(null, "name"));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllWatchers("owner", null));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllWatchers(null, "name", ApiOptions.None));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllWatchers("owner", null, ApiOptions.None));
                await Assert.ThrowsAsync<ArgumentNullException>(() => client.GetAllWatchers("owner", "name", null));

                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllWatchers("", "name"));
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllWatchers("owner", ""));
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllWatchers("", "name", ApiOptions.None));
                await Assert.ThrowsAsync<ArgumentException>(() => client.GetAllWatchers("owner", "", ApiOptions.None));
            }
        }

        public class TheCheckWatchedMethod
        {
            [Fact]
            public async Task ReturnsTrueOnValidResult()
            {
                var endpoint = new Uri("repos/fight/club/subscription", UriKind.Relative);

                var connection = Substitute.For<IApiConnection>();
                connection.Get<Subscription>(endpoint).Returns(Task.FromResult(new Subscription(false, false, null, default(DateTimeOffset), null, null)));

                var client = new WatchedClient(connection);

                var watched = await client.CheckWatched("fight", "club");

                Assert.True(watched);
            }

            [Fact]
            public async Task ReturnsFalseOnNotFoundException()
            {
                var endpoint = new Uri("repos/fight/club/subscription", UriKind.Relative);

                var connection = Substitute.For<IApiConnection>();
                var response = new Response(HttpStatusCode.NotFound, null, new Dictionary<string, string>(), "application/json");
                connection.Get<Subscription>(endpoint).Returns<Task<Subscription>>(x =>
                {
                    throw new NotFoundException(response);
                });

                var client = new WatchedClient(connection);

                var watched = await client.CheckWatched("fight", "club");

                Assert.False(watched);
            }
        }

        public class TheWatchRepoMethod
        {
            [Fact]
            public void RequestsCorrectUrl()
            {
                var endpoint = new Uri("repos/fight/club/subscription", UriKind.Relative);
                var connection = Substitute.For<IApiConnection>();
                var client = new WatchedClient(connection);

                var newSubscription = new NewSubscription();
                client.WatchRepo("fight", "club", newSubscription);

                connection.Received().Put<Subscription>(endpoint, newSubscription);
            }
        }

        public class TheUnwatchRepoMethod
        {
            [Theory]
            [InlineData(HttpStatusCode.NoContent, true)]
            [InlineData(HttpStatusCode.NotFound, false)]
            [InlineData(HttpStatusCode.OK, false)]
            public async Task ReturnsCorrectResultBasedOnStatus(HttpStatusCode status, bool expected)
            {
                var response = Task.Factory.StartNew(() => status);

                var connection = Substitute.For<IConnection>();
                connection.Delete(Arg.Is<Uri>(u => u.ToString() == "repos/yes/no/subscription"))
                    .Returns(response);

                var apiConnection = Substitute.For<IApiConnection>();
                apiConnection.Connection.Returns(connection);

                var client = new WatchedClient(apiConnection);
                var result = await client.UnwatchRepo("yes", "no");

                Assert.Equal(expected, result);
            }
        }
    }
}

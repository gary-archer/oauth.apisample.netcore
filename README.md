# authguidance.apisample.netcore

[![Known Vulnerabilities](https://snyk.io/test/github/gary-archer/authguidance.apisample.netcore/badge.svg?targetFile=api.csproj)](https://snyk.io/test/github/gary-archer/authguidance.apisample.netcore?targetFile=api.csproj)

### Overview

* The final API code sample using OAuth 2.0 and Open Id Connect, referenced in my blog at https://authguidance.com
* **The goal of this sample is to implement this blog's** [API Architecture](https://authguidance.com/2019/03/24/api-platform-design/) **in .Net Core**

### Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for what the API does and how to run it
* See the [OAuth Integration Page](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for notes on Custom Claims Handling

### Programming Languages

* C# and .Net Core 3 are used to implement the OAuth Secured REST API

### Middleware Used

* The [IdentityModel](https://github.com/IdentityModel/IdentityModel) library is used for API OAuth operations
* The [Microsoft In Memory Cache](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/memory) is used to cache API claims in memory
* The Kestrel web server is used to host the API
* Okta is used for the Authorization Server
* OpenSSL is used for SSL certificate handling
* API logs can be aggregated to [Elastic Search](https://authguidance.com/2019/07/19/log-aggregation-setup/) to support common [Query Use Cases](https://authguidance.com/2019/08/02/intelligent-api-platform-analysis/)

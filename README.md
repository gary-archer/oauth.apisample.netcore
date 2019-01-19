# authguidance.websample.netcoreapi

### Overview

* An SPA sample using OAuth 2.0 and Open Id Connect, referenced in my blog at https://authguidance.com
* **This sample implements our [API Architecture](http://authguidance.com/2017/10/03/api-tokens-claims) in Cross Platform C#**

### Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for how to run the API
* See the [Coding Key Points](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for integration and reliability details

### Programming Languages

* TypeScript is used for the SPA
* C# code and .Net Core 2.0 is used for the API

### Middleware Used

* The [Oidc-Client Library](https://github.com/IdentityModel/oidc-client-js) is used to implement the Implicit Flow
* [IdentityModel OAuth2Introspection](https://github.com/IdentityModel/IdentityModel.AspNetCore.OAuth2Introspection) is used to introspect tokens and to cache API claims in memory
* Kestrel is used to host both the API and the SPA content
* Okta is used for the Authorization Server
* OpenSSL is used for SSL certificate handling

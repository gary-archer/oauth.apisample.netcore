# authguidance.apisample.netcore

### Overview

* An SPA sample using OAuth 2.0 and Open Id Connect, referenced in my blog at https://authguidance.com
* **The goal of this sample is to implement our [API Platform Architecture](https://authguidance.com/2019/03/24/api-platform-design/) in .Net Core**

### Details

* See the [Overview Page](http://authguidance.com/2018/01/05/net-core-code-sample-overview/) for how to run the API
* See the [Coding Key Points](http://authguidance.com/2018/01/06/net-core-api-key-coding-points/) for customisation details

### Programming Languages

* C# code and .Net Core 2 is used for the API

### Middleware Used

* The [IdentityModel2](https://github.com/IdentityModel/IdentityModel2) library is used for API token introspection
* Kestrel is used to host both the API and the SPA content
* Okta is used for the Authorization Server
* OpenSSL is used for SSL certificate handling

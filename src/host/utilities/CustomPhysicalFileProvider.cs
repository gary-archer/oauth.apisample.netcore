namespace SampleApi.Host.Utilities
{
    using System;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Primitives;

    /*
     * A custom implementation of file provider than does not emit startup errors when a folder on disk does not exist
     */
    public class CustomPhysicalFileProvider : IFileProvider
    {
        private readonly Lazy<PhysicalFileProvider> provider;

        /*
         * Initialise the provider to null, and we will create it on first usage
         */
        public CustomPhysicalFileProvider(string root)
        {
            this.provider = new Lazy<PhysicalFileProvider>(() => new PhysicalFileProvider(root));
        }

        /*
        * Enumerate a directory at the given path, if any
        */
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            return this.provider.Value.GetDirectoryContents(subpath);
        }

        /*
        * Locate a file at the given path
        */
        public IFileInfo GetFileInfo(string subpath)
        {
            return this.provider.Value.GetFileInfo(subpath);
        }

        /*
            * Creates a Microsoft.Extensions.Primitives.IChangeToken for the specified filter
            */
        public IChangeToken Watch(string filter)
        {
            return this.provider.Value.Watch(filter);
        }
    }
}
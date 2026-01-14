using System.Collections.Immutable;
using System.Reflection;
using System.Resources;

namespace UPS.WWRR.Business.Extensions
{
    public static class ReflectionEx
    {
        private static ImmutableDictionary<string, ImmutableList<string>> s_nameCache =
            ImmutableDictionary<string, ImmutableList<string>>.Empty;



        private static ImmutableList<string> ReadNamesFromCache(Assembly assembly)
        {
            var assemblyName = assembly.FullName;

            while (true)
            {
                if (s_nameCache.TryGetValue(assemblyName, out var manifestNames))
                    return manifestNames;

                var names = assembly.GetManifestResourceNames().ToImmutableList();
                ImmutableInterlocked.TryAdd(ref s_nameCache, assemblyName, names);
            }
        }

        /// <summary>Retrieves and caches for immediate future retrieval, the names of all embedded resources in <paramref name="assembly"/></summary>
        /// <param name="assembly">The assembly to get the list of resource names</param>
        /// <returns>A list of full (root namespace included) names of embedded resources.</returns>
        public static ImmutableList<string> EmbeddedResourceNames(this Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            return ReadNamesFromCache(assembly);
        }

        /// <summary>Attempts to find <paramref name="name"/> in the assembly that calls this method as an embedded resource and return the entire contents as text.</summary>
        /// <param name="name">The full (with root namespace) name, or the first file matched that ends with what is provided here</param>
        /// <remarks>Reference <see cref="ReflectionEx"/> for information on excellent performance behaviors/thread-safety characteristics of these extension methods</remarks>
        /// <exception cref="InvalidOperationException">
        ///  <para>The calling assembly has no embedded resources.</para>
        ///  <para> - or - </para>
        ///  <para>
        ///   This shouldn't ever be thrown, and if it is, might indicate a bug in this library.
        ///   If it happens, the issue is that we've found a file matching <paramref name="name"/>, gotten the "correct" name, opened it
        ///   and rather than anything being thrown, the GetFile call just returned <c>null</c>
        ///  </para>
        /// </exception>
        /// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
        /// <exception cref="BadImageFormatException">The calling assembly is not a valid assembly (this should really never happen).</exception>
        /// <seealso cref="ReflectionEx"/>
        public static string ReadAllTextFromMyAssembly(string name) => ReadAllText(Assembly.GetCallingAssembly(), name);

        /// <summary>Attempts to find <paramref name="name"/> in the assembly that calls this method as an embedded resource and return the entire contents as text.</summary>
        /// <param name="name">The full (with root namespace) name, or the first file matched that ends with what is provided here</param>
        /// <remarks>Reference <see cref="ReflectionEx"/> for information on excellent performance behaviors/thread-safety characteristics of these extension methods</remarks>
        /// <exception cref="InvalidOperationException">
        ///  <para>The calling assembly has no embedded resources.</para>
        ///  <para> - or - </para>
        ///  <para>
        ///   This shouldn't ever be thrown, and if it is, might indicate a bug in this library.
        ///   If it happens, the issue is that we've found a file matching <paramref name="name"/>, gotten the "correct" name, opened it
        ///   and rather than anything being thrown, the GetFile call just returned <c>null</c>
        ///  </para>
        /// </exception>
        /// <exception cref="FileLoadException">A file that was found could not be loaded.</exception>
        /// <exception cref="BadImageFormatException">The calling assembly is not a valid assembly (this should really never happen).</exception>
        /// <seealso cref="ReflectionEx"/>
        public static Task<string> ReadAllTextFromMyAssemblyAsync(string name) => ReadAllTextAsync(Assembly.GetCallingAssembly(), name);


        /// <summary>Gets the full name of a resource by matching against the ends of the names within the (cached) list of embedded resources.</summary>
        /// <param name="assembly">The assembly that the embedded resource is stored within</param>
        /// <param name="name">The full (with root namespace) name, or the first file matched that ends with what is provided here</param>
        /// <remarks>
        /// <para>Reference <see cref="ReflectionEx"/> for information on excellent performance behaviors/thread-safety characteristics of these extension methods</para>
        /// <para>Typically Visual Studio will name a resource as `RootNamespace`.Namespace.By.Folder.Filename.ext and using the full
        /// name is going to return the fastest from this method (since it does nothing but make sure that the name matches)</para>
        /// <para>The following conventions are supported - using "/" or "\" instead of "."</para>
        /// <para>Matching only the end of the filename (first will be returned). Yes, you can literally pass in "t" and it'll
        /// match every text file that is embedded in the script.  Obviously this is a bad idea, but constraining it further is
        /// sort-of protecting a bad developer from exposing themselves.</para>
        /// <para>As I use Rider, I only know how it embeds resources -- and the namespace inclusive embedding was its behavior,
        /// however, I read more than a few posts that indicated the behavior of a version of Visual Studio was to embed as
        /// RootNamespace.Filename.ext.  Provided your assembly name matches your root namespace (you'd have to have changed
        /// that on purpose or renamed the assembly without changing the root namespace name), this format will match correctly
        /// when Filename.ext matches a filename</para>
        /// <para>The order in which these are described also implies its costs -- each of these tries allocates strings
        /// and checks a list; it's best to match exactly</para>
        /// </remarks>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">The <paramref name="assembly"/> has no embedded resources.</exception>
        /// <exception cref="MissingManifestResourceException">The <paramref name="name"/> could not be found and no file name that ended with that value was found.</exception>
        /// <returns>The full (with root namespace) name of the first matching embedded resource </returns>
        /// <seealso cref="ReflectionEx"/>
        public static string GetFullEmbeddedResourceName(this Assembly assembly, string name)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            var names = ReadNamesFromCache(assembly);

            if (names.IsEmpty)
                throw new InvalidOperationException($"The `{assembly}` has no embedded resources");
            if (names.Contains(name))
                return name;

            for (int i = 0; i < names.Count; i++)
                if (string.Equals(names[i], name, StringComparison.OrdinalIgnoreCase))
                    return names[i];

            if (name.Contains('/'))
                name = name.Replace("/", ".");
            if (name.Contains('\\'))
                name = name.Replace("\\", ".");

            for (int i = 0; i < names.Count; i++)
                if (names[i].EndsWith(name, StringComparison.OrdinalIgnoreCase))
                    return names[i];

            var possibleNsName = assembly.GetName().Name;
            var trimmedName = name.StartsWith(possibleNsName, StringComparison.OrdinalIgnoreCase)
                                  ? name.Remove(0, possibleNsName.Length + 1)
                                  : name;

            string embeddedName = null;

            if (trimmedName != name)
                embeddedName = names.FirstOrDefault(x => x.EndsWith(trimmedName, StringComparison.OrdinalIgnoreCase));

            if (embeddedName == null)
                embeddedName = names.FirstOrDefault(
                    fullName => string.Equals($"{possibleNsName}.{name}", fullName, StringComparison.OrdinalIgnoreCase)
                );

            return embeddedName
                ?? throw new MissingManifestResourceException(
                       $"The assembly `{assembly}` has no file named, or with a name ending with `{name}`.\n"
                     + "The following files were found embedded as resources:\n\t"
                     + string.Join("\n\t", names)
                   );
        }

        /// <summary>Attempts to find <paramref name="name"/> in <paramref name="assembly"/> as an embedded resource and return the entire contents as text.</summary>
        /// <param name="assembly">The assembly that the embedded resource is stored within</param>
        /// <param name="name">The full (with root namespace) name, or the first file matched that ends with what is provided here</param>
        /// <remarks>Reference <see cref="ReflectionEx"/> for information on excellent performance behaviors/thread-safety characteristics of these extension methods</remarks>
        /// <exception cref="InvalidOperationException">
        ///  <para>The <paramref name="assembly"/> has no embedded resources.</para>
        ///  <para> - or - </para>
        ///  <para>
        ///   This shouldn't ever be thrown, and if it is, might indicate a bug in this library.
        ///   If it happens, the issue is that we've found a file matching <paramref name="name"/>, gotten the "correct" name, opened it
        ///   and rather than anything being thrown, the GetFile call just returned <c>null</c>
        ///  </para>
        /// </exception>
        /// <exception cref="FileLoadException"> A file that was found could not be loaded.</exception>
        /// <exception cref="BadImageFormatException"><paramref name="assembly"/> is not a valid assembly.</exception>
        /// <seealso cref="ReflectionEx"/>
        public static string ReadAllText(this Assembly assembly, string name)
        {
            var resourceName = GetFullEmbeddedResourceName(assembly, name);

            using Stream resFile = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(
                    resFile ?? throw new InvalidOperationException($"Received no file when attempting ot read {name} from {assembly}")
                );
            return reader.ReadToEnd();
        }

        /// <summary>Asynchronously attempts to find <paramref name="name"/> in <paramref name="assembly"/> and return the entire contents as text.</summary>
        /// <param name="assembly">The assembly that the embedded resource is stored within</param>
        /// <param name="name">The full (with root namespace) name, or the first file matched that ends with what is provided here</param>
        /// <exception cref="InvalidOperationException">
        ///  <para>The <paramref name="assembly"/> has no embedded resources.</para>
        ///  <para> - or - </para>
        ///  <para>
        ///   This shouldn't ever be thrown, and if it is, might indicate a bug in this library.
        ///   If it happens, the issue is that we've found a file matching <paramref name="name"/>, gotten the "correct" name, opened it
        ///   and rather than anything being thrown, the GetFile call just returned <c>null</c>
        ///  </para>
        /// </exception>
        /// <exception cref="FileLoadException"> A file that was found could not be loaded.</exception>
        /// <exception cref="BadImageFormatException"><paramref name="assembly"/> is not a valid assembly.</exception>
        public static async Task<string> ReadAllTextAsync(this Assembly assembly, string name)
        {
            var resourceName = GetFullEmbeddedResourceName(assembly, name);

            using Stream resFile = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(
                    resFile ?? throw new InvalidOperationException($"Received no file when attempting ot read {name} from {assembly}")
                );
            return await reader.ReadToEndAsync().ConfigureAwait(false);
        }
    }
}

namespace HttpClientEx.Common
{
    /// <summary>
    /// url辅助类
    /// </summary>
    /// <remarks>
    /// https://stackoverflow.com/questions/372865/path-combine-for-urls；
    /// https://github.com/SharePoint/PnP-Sites-Core/blob/master/Core/OfficeDevPnP.Core/Utilities/UrlUtility.cs
    /// </remarks>
    public static class UrlUtility
    {
        const char PATH_DELIMITER = '/';

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relativePaths">SharePoint relative URLs</param>
        /// <returns>Returns comibed path with a relative paths</returns>
        public static string Combine(string path, params string[] relativePaths)
        {
            string pathBuilder = path ?? string.Empty;

            if (relativePaths == null)
                return pathBuilder;

            foreach (string relPath in relativePaths)
            {
                pathBuilder = Combine(pathBuilder, relPath);
            }
            return pathBuilder;
        }

        /// <summary>
        /// Combines a path and a relative path.
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="relative">SharePoint relative URL</param>
        /// <returns>Returns comibed path with a relative path</returns>
        public static string Combine(string path, string relative)
        {
            if (relative == null)
                relative = string.Empty;

            if (path == null)
                path = string.Empty;

            if (relative.Length == 0 && path.Length == 0)
                return string.Empty;

            if (relative.Length == 0)
                return path;

            if (path.Length == 0)
                return relative;

            path = path.Replace('\\', PATH_DELIMITER);
            relative = relative.Replace('\\', PATH_DELIMITER);

            return path.TrimEnd(PATH_DELIMITER) + PATH_DELIMITER + relative.TrimStart(PATH_DELIMITER);
        }

        /// <summary>
        /// Adds query string parameters to the end of a querystring and guarantees the proper 
        /// concatenation with <b>?</b> and <b>&amp;.</b>
        /// </summary>
        /// <param name="path">A SharePoint URL</param>
        /// <param name="queryString">Query string value that need to append to the URL</param>
        /// <returns>Returns URL along with appended query string</returns>
        public static string AppendQueryString(string path, string queryString)
        {
            string url = path;

            if (queryString != null && queryString.Length > 0)
            {
                char startChar = (path.IndexOf("?") > 0) ? '&' : '?';
                url = string.Concat(path, startChar, queryString.TrimStart('?'));
            }
            return url;
        }
    }
}
